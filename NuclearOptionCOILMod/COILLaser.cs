using UnityEngine;
using NuclearOption.Networking;
using System.Collections.Generic;

namespace NuclearOptionCOILMod
{
    /// <summary>
    /// COIL (Chemical Oxygen Iodine Laser) weapon implementation.
    /// Continuous beam model: fires as long as trigger is held, drains 1 ammo per second.
    /// Beam is invisible but laser sound plays while firing.
    /// 
    /// Auto-fire mode: when enabled, automatically scans for and engages
    /// missiles/bombs/nuclear weapons with priority targeting.
    /// Priority: incoming AA missiles > nuclear > other missiles > AA missiles > bombs
    /// 
    /// The Darkreach Inner Bay has multiple hardpoints, so this prefab gets
    /// instantiated multiple times. Only the first instance (primary) fires
    /// and tracks ammo. Secondary instances report 0 ammo so AccountAmmo
    /// doesn't double-count.
    /// </summary>
    public class COILLaser : Weapon
    {
        private int _shotsRemaining;
        private bool _isFiring;
        private bool _initialized;
        private bool _isPrimary = true;
        private float _lastDamageTick;
        private float _ammoAccumulator;

        // Auto-fire targeting
        private bool _autoFireEnabled;
        private Unit _autoTarget;
        private float _lastTargetScan;
        private bool _manualFiring; // True when player is manually holding trigger
        private float _lastTargetSwitchTime; // When we last switched auto-targets
        private bool _ammoChargedForBurst; // True once we've charged the initial 1 ammo for this burst
        private const float TARGET_SCAN_INTERVAL = 0.5f;
        private List<TrackingInfo> _scanResults = new List<TrackingInfo>();

        // Audio
        private AudioSource _audioSource;
        private bool _soundPlaying;

        // Layer mask matching the game's Laser class: ~0x2001
        private const int LASER_LAYER_MASK = -8193;
        private const float BEAM_ORIGIN_OFFSET = 30f;
        // Damage tick interval matching game's Laser (0.2s)
        private const float DAMAGE_TICK_INTERVAL = 0.2f;

        private static BepInEx.Logging.ManualLogSource _logger;
        // Track the primary instance per aircraft to avoid doubled ammo
        private static readonly Dictionary<Unit, COILLaser> _primaryInstances
            = new Dictionary<Unit, COILLaser>();

        public static void SetLogger(BepInEx.Logging.ManualLogSource logger) => _logger = logger;

        private static void Log(string message)
        {
            if (_logger != null) _logger.LogInfo($"[COILLaser] {message}");
        }

        public bool AutoFireEnabled => _autoFireEnabled;

        public void SetAutoFire(bool enabled)
        {
            _autoFireEnabled = enabled;
            if (!enabled)
            {
                _autoTarget = null;
                if (_isFiring && currentTarget != null && currentTarget is Missile)
                {
                    // Stop auto-firing when disabled
                    _isFiring = false;
                    _ammoAccumulator = 0f;
                    _ammoChargedForBurst = false;
                }
            }
            Log($"Auto-fire {(enabled ? "ENABLED" : "DISABLED")}");
        }

        public void ToggleAutoFire() => SetAutoFire(!_autoFireEnabled);

        /// <summary>
        /// Called from SpawnMount postfix. Determines if this is the primary
        /// instance for this aircraft (first one registered gets to fire).
        /// </summary>
        public void ForceInitialize()
        {
            _shotsRemaining = COILModPlugin.MaxShots.Value;
            _ammoAccumulator = 0f;
            _lastDamageTick = 0f;
            _initialized = true;
            _autoFireEnabled = COILModPlugin.AutoFireDefault.Value;

            // Determine primary/secondary status
            if (attachedUnit != null)
            {
                if (_primaryInstances.ContainsKey(attachedUnit) && _primaryInstances[attachedUnit] != null
                    && _primaryInstances[attachedUnit] != this)
                {
                    _isPrimary = false;
                    _shotsRemaining = 0;
                    Log($"ForceInitialize SECONDARY on {attachedUnit.name} - 0 ammo");
                }
                else
                {
                    _isPrimary = true;
                    _primaryInstances[attachedUnit] = this;
                    Log($"ForceInitialize PRIMARY on {attachedUnit.name} - {_shotsRemaining} ammo");
                }
            }
        }

        private void Start()
        {
            if (!_initialized)
            {
                _shotsRemaining = _isPrimary ? COILModPlugin.MaxShots.Value : 0;
                _initialized = true;
            }

            // Set up audio for laser firing sound
            InitializeAudio();

            Log($"Start - primary={_isPrimary}, ammo={_shotsRemaining}");
        }

        private void InitializeAudio()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1f;
            _audioSource.spread = 20f;
            _audioSource.dopplerLevel = 0f;
            _audioSource.minDistance = 10f;
            _audioSource.maxDistance = 200f;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.volume = 0.8f;

            // Try to borrow the sustained fire clip from an existing game Laser
            if (SoundManager.i != null)
                _audioSource.outputAudioMixerGroup = SoundManager.i.EffectsMixer;

            Laser existingLaser = Object.FindObjectOfType<Laser>();
            if (existingLaser != null)
            {
                // The game's Laser has 3 AudioSources: fireStart, fireSustained (looped), fireEnd
                AudioSource[] laserSources = existingLaser.GetComponents<AudioSource>();
                foreach (var src in laserSources)
                {
                    if (src.loop && src.clip != null)
                    {
                        _audioSource.clip = src.clip;
                        _audioSource.pitch = 0.7f; // Lower pitch for the big COIL
                        Log("Borrowed sustained fire clip from game Laser");
                        break;
                    }
                }
            }

            if (_audioSource.clip == null)
            {
                // Generate a simple tone as fallback
                _audioSource.clip = GenerateLaserTone();
                Log("Using generated laser tone");
            }
        }

        private static AudioClip GenerateLaserTone()
        {
            int sampleRate = 44100;
            int length = sampleRate * 2; // 2 second loop
            float[] samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sampleRate;
                // Low hum with harmonic overtones
                float sample = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.4f
                             + Mathf.Sin(2f * Mathf.PI * 240f * t) * 0.2f
                             + Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.3f;
                samples[i] = sample;
            }
            AudioClip clip = AudioClip.Create("COILLaserTone", length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public override void Fire(Unit owner, Unit target, Vector3 inheritedVelocity,
            WeaponStation weaponStation, GlobalPosition aimpoint)
        {
            // Only primary instance fires
            if (!_isPrimary || _shotsRemaining <= 0)
                return;

            if (target != null && !IsTargetInFiringArc(target))
                return;

            // Manual fire takes priority over auto-fire
            _manualFiring = true;

            if (!_isFiring)
            {
                _isFiring = true;
                _ammoAccumulator = 0f;
                _ammoChargedForBurst = false; // Will be charged in next FixedUpdate
                Log($"Firing started (manual) - {_shotsRemaining} ammo remaining");
            }

            lastFired = Time.timeSinceLevelLoad;
            weaponStation.LastFiredTime = Time.timeSinceLevelLoad;
            currentTarget = target;
        }

        public override void SetTarget(Unit target) => currentTarget = target;

        private bool IsTargetInFiringArc(Unit target)
        {
            if (attachedUnit == null || target == null) return false;
            Transform unitTransform = ((MonoBehaviour)attachedUnit).transform;
            Vector3 toTarget = (((MonoBehaviour)target).transform.position - unitTransform.position).normalized;
            return Vector3.Angle(unitTransform.forward, toTarget) <= COILModPlugin.FiringArc.Value;
        }

        private void FixedUpdate()
        {
            if (!_isFiring || !_isPrimary)
                return;

            // Minimum 1 ammo per firing burst: charge immediately when firing starts
            if (!_ammoChargedForBurst && _shotsRemaining > 0)
            {
                _ammoChargedForBurst = true;
                _shotsRemaining--;
                _ammoAccumulator = 0f; // Reset — next tick at 1s from now

                if (weaponStation != null)
                {
                    weaponStation.AccountAmmo();
                    weaponStation.Updated();
                }
                Log($"Burst start - charged 1 ammo, {_shotsRemaining} remaining");
            }

            // Drain 1 ammo per second (continuous fire after initial charge)
            _ammoAccumulator += Time.fixedDeltaTime;
            while (_ammoAccumulator >= 1f && _shotsRemaining > 0)
            {
                _ammoAccumulator -= 1f;
                _shotsRemaining--;

                if (weaponStation != null)
                {
                    weaponStation.AccountAmmo();
                    weaponStation.Updated();
                }

                Log($"Ammo tick - {_shotsRemaining} remaining");
            }

            // Out of ammo, stop firing
            if (_shotsRemaining <= 0)
            {
                _isFiring = false;
                _ammoAccumulator = 0f;
                _ammoChargedForBurst = false;
                Log("Out of ammo - firing stopped");
                if (weaponStation != null)
                {
                    weaponStation.AccountAmmo();
                    weaponStation.Updated();
                }
                return;
            }

            ApplyLaserDamage();
        }

        private void Update()
        {
            if (!_isPrimary) return;

            // Key toggle for auto-fire — only when this aircraft is the player's
            HandleAutoFireToggle();
        }

        private void HandleAutoFireToggle()
        {
            if (attachedUnit == null) return;

            // Only respond to input if this is the player's aircraft
            Aircraft aircraft = attachedUnit as Aircraft;
            if (aircraft == null || aircraft.Player == null) return;

            // Parse the configured key
            KeyCode toggleKey;
            try
            {
                toggleKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), COILModPlugin.AutoFireToggleKey.Value, true);
            }
            catch
            {
                toggleKey = KeyCode.V;
            }

            if (Input.GetKeyDown(toggleKey))
            {
                ToggleAutoFire();
            }
        }

        private void LateUpdate()
        {
            if (!_isPrimary) return;

            // Check if manual fire has timed out (player released trigger)
            if (_manualFiring && Time.timeSinceLevelLoad - lastFired > 0.2f)
            {
                _manualFiring = false;
                // If auto-fire is off, stop firing entirely
                if (!_autoFireEnabled)
                {
                    _isFiring = false;
                    _ammoAccumulator = 0f;
                    _ammoChargedForBurst = false;
                }
                // If auto-fire is on, let AutoFireUpdate take over
            }

            // Auto-fire targeting scan — runs even when COIL is not selected weapon
            // but defers to manual fire when player is holding trigger
            if (_autoFireEnabled && _shotsRemaining > 0 && !_manualFiring)
                AutoFireUpdate();

            // Manage sound
            UpdateSound();
        }

        private void OnGUI()
        {
            if (!_isPrimary) return;
            if (attachedUnit == null) return;

            // Only show HUD for the player's aircraft
            Aircraft aircraft = attachedUnit as Aircraft;
            if (aircraft == null || aircraft.Player == null) return;

            // Show auto-fire status in top-left area below other HUD elements
            string status = _autoFireEnabled ? "COIL AUTO: ON" : "COIL AUTO: OFF";
            string targetText = "";
            if (_autoFireEnabled && _autoTarget != null && !_autoTarget.disabled)
                targetText = $"\nTGT: {_autoTarget.name}";

            Color statusColor = _autoFireEnabled ? Color.green : Color.red;
            if (_isFiring && _autoFireEnabled)
                statusColor = Color.cyan; // Actively engaging

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = statusColor;
            // Add shadow for readability
            GUIStyle shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = Color.black;

            float x = 10f;
            float y = Screen.height - 80f;

            GUI.Label(new Rect(x + 1, y + 1, 300, 50), status + targetText, shadowStyle);
            GUI.Label(new Rect(x, y, 300, 50), status + targetText, style);
        }

        // =================================================================
        // AUTO-FIRE TARGETING SYSTEM
        // Priority: incoming AA missiles targeting us > nuclear weapons >
        //           other missiles > air-to-air missiles > bombs
        // =================================================================

        private void AutoFireUpdate()
        {
            if (attachedUnit == null || attachedUnit.disabled) return;
            if (attachedUnit.NetworkHQ == null) return;

            // Scan for targets periodically
            if (Time.timeSinceLevelLoad - _lastTargetScan < TARGET_SCAN_INTERVAL)
            {
                // Keep firing at current auto-target between scans
                if (_autoTarget != null && !_autoTarget.disabled)
                {
                    // Re-check LOS on current target
                    if (HasLineOfSight(_autoTarget))
                    {
                        ContinueAutoFire();
                        return;
                    }
                    else
                    {
                        // Lost LOS, drop target and start acquisition delay
                        Log($"Auto-target {_autoTarget.name} lost LOS");
                        _lastTargetSwitchTime = Time.timeSinceLevelLoad;
                        _autoTarget = null;
                        if (_isFiring)
                        {
                            _isFiring = false;
                            _ammoAccumulator = 0f;
                            _ammoChargedForBurst = false;
                        }
                    }
                }
                // Target lost/destroyed, stop firing and start acquisition delay
                if (_autoTarget != null && _autoTarget.disabled)
                {
                    _lastTargetSwitchTime = Time.timeSinceLevelLoad;
                    _autoTarget = null;
                }
                if (_isFiring && _autoTarget == null)
                {
                    _isFiring = false;
                    _ammoAccumulator = 0f;
                    _ammoChargedForBurst = false;
                }
                return;
            }

            _lastTargetScan = Time.timeSinceLevelLoad;

            // Get all tracked enemy targets within range
            _scanResults = attachedUnit.NetworkHQ.GetTargetsWithinRange(
                _scanResults, transform, COILModPlugin.MaxRange.Value, false);

            Unit bestTarget = null;
            float bestScore = 0f;

            foreach (var trackingInfo in _scanResults)
            {
                Unit target;
                if (!trackingInfo.TryGetUnit(out target)) continue;
                if (target.disabled) continue;

                // Only engage missiles and bombs
                float score = ScoreTarget(target);
                if (score <= 0f) continue;

                // Must be in firing arc
                if (!IsTargetInFiringArc(target)) continue;

                // Must have line of sight
                if (!HasLineOfSight(target)) continue;

                // Distance factor — prefer closer targets
                float dist = Vector3.Distance(transform.position,
                    ((MonoBehaviour)target).transform.position);
                if (dist > COILModPlugin.MaxRange.Value) continue;

                // Closer targets get a bonus
                float distFactor = 1f + (1f - dist / COILModPlugin.MaxRange.Value);
                score *= distFactor;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = target;
                }
            }

            if (bestTarget != null)
            {
                // Acquisition delay — wait before locking any new target
                float switchDelay = COILModPlugin.AutoFireSwitchDelay.Value;
                if (_autoTarget != bestTarget && Time.timeSinceLevelLoad - _lastTargetSwitchTime < switchDelay)
                {
                    // Still in cooldown — don't acquire new target
                    return;
                }

                if (_autoTarget != bestTarget)
                {
                    Log($"Auto-target: {bestTarget.name} (score={bestScore:F1})");
                    _lastTargetSwitchTime = Time.timeSinceLevelLoad;
                }
                _autoTarget = bestTarget;
                ContinueAutoFire();
            }
            else if (_autoTarget != null)
            {
                // Target lost — start the acquisition delay timer
                _lastTargetSwitchTime = Time.timeSinceLevelLoad;
                _autoTarget = null;
                if (_isFiring)
                {
                    _isFiring = false;
                    _ammoAccumulator = 0f;
                    _ammoChargedForBurst = false;
                }
            }
        }

        /// <summary>
        /// Check line of sight to a target using a raycast.
        /// Returns true if nothing blocks the path to the target.
        /// </summary>
        private bool HasLineOfSight(Unit target)
        {
            if (target == null) return false;

            Transform targetTransform = ((MonoBehaviour)target).transform;
            Vector3 direction = (targetTransform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetTransform.position);

            Vector3 origin = transform.position + direction * BEAM_ORIGIN_OFFSET;
            float castDistance = distance - BEAM_ORIGIN_OFFSET;
            if (castDistance <= 0f) return true; // Very close, assume LOS

            RaycastHit hit;
            if (!Physics.Raycast(origin, direction, out hit, castDistance, LASER_LAYER_MASK, QueryTriggerInteraction.Ignore))
                return false; // Didn't hit anything — no collider on target path (unlikely but safe)

            // Check if what we hit belongs to the target or is close to it
            Unit hitUnit = hit.collider.GetComponentInParent<Unit>();
            if (hitUnit == target) return true;

            // If we hit our own aircraft, ignore and assume LOS
            if (hitUnit != null && hitUnit == attachedUnit) return true;

            // Hit something else — no clear LOS
            return false;
        }

        private void ContinueAutoFire()
        {
            if (_autoTarget == null || _autoTarget.disabled || _shotsRemaining <= 0)
            {
                _autoTarget = null;
                if (_isFiring) { _isFiring = false; _ammoAccumulator = 0f; _ammoChargedForBurst = false; }
                return;
            }

            if (!IsTargetInFiringArc(_autoTarget))
            {
                _autoTarget = null;
                if (_isFiring) { _isFiring = false; _ammoAccumulator = 0f; _ammoChargedForBurst = false; }
                return;
            }

            // Simulate Fire() call for auto-fire
            if (!_isFiring)
            {
                _isFiring = true;
                _ammoAccumulator = 0f;
                _ammoChargedForBurst = false; // Will be charged in next FixedUpdate
                Log($"Auto-fire started on {_autoTarget.name}");
            }

            lastFired = Time.timeSinceLevelLoad;
            if (weaponStation != null)
                weaponStation.LastFiredTime = Time.timeSinceLevelLoad;
            currentTarget = _autoTarget;
        }

        /// <summary>
        /// Score a target for auto-fire priority.
        /// Higher score = higher priority.
        /// Only scores missiles and bombs (returns 0 for everything else).
        /// </summary>
        private float ScoreTarget(Unit target)
        {
            if (target == null || target.definition == null) return 0f;

            Missile missile = target as Missile;
            if (missile == null) return 0f; // Only engage missiles/bombs

            WeaponInfo missileInfo = missile.GetWeaponInfo();
            if (missileInfo == null) return 0f;

            float score = 0f;

            // Base score by type
            if (missileInfo.nuclear)
            {
                // Nuclear weapons — very high priority
                score = 1000f;
            }
            else if (missileInfo.bomb)
            {
                // Bombs — lowest missile priority
                score = 50f;
            }
            else
            {
                // Regular missiles
                score = 200f;

                // Air-to-air missiles get moderate priority
                if (missileInfo.effectiveness.antiAir > 0.5f)
                    score = 300f;
            }

            // Massive priority boost for missiles targeting US
            if (attachedUnit != null && missile.targetID == attachedUnit.persistentID)
            {
                score *= 10f;
                // Extra boost for close incoming missiles (more urgent)
                float dist = Vector3.Distance(transform.position,
                    ((MonoBehaviour)target).transform.position);
                if (dist < 5000f)
                    score *= 2f;
            }

            // Boost for missiles targeting friendly aircraft nearby
            // (lower boost than self-defense but still important)
            else if (attachedUnit != null && attachedUnit.NetworkHQ != null)
            {
                Unit missileTarget = null;
                if (missile.targetID.IsValid)
                    UnitRegistry.TryGetUnit(new PersistentID?(missile.targetID), out missileTarget);

                if (missileTarget != null && missileTarget.NetworkHQ == attachedUnit.NetworkHQ)
                    score *= 3f;
            }

            return score;
        }

        private void UpdateSound()
        {
            if (_audioSource == null) return;

            if (_isFiring && _shotsRemaining > 0)
            {
                if (!_soundPlaying)
                {
                    _audioSource.Play();
                    _soundPlaying = true;
                }
            }
            else
            {
                if (_soundPlaying)
                {
                    _audioSource.Stop();
                    _soundPlaying = false;
                }
            }
        }

        private void ApplyLaserDamage()
        {
            if (currentTarget == null) return;
            if (!IsTargetInFiringArc(currentTarget))
            {
                _isFiring = false;
                _ammoAccumulator = 0f;
                _ammoChargedForBurst = false;
                return;
            }

            // Only apply damage on server (matches game's Laser.cs pattern)
            if (!NetworkManagerNuclearOption.i.Server.Active)
                return;

            // Rate-limit damage ticks to match game's Laser (every 0.2s)
            if (Time.timeSinceLevelLoad - _lastDamageTick < DAMAGE_TICK_INTERVAL)
                return;

            Transform targetTransform = ((MonoBehaviour)currentTarget).transform;
            Vector3 direction = (targetTransform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetTransform.position);

            if (distance > COILModPlugin.MaxRange.Value)
                return;

            Vector3 origin = transform.position + direction * BEAM_ORIGIN_OFFSET;
            float castDistance = distance - BEAM_ORIGIN_OFFSET;
            if (castDistance <= 0f) return;

            RaycastHit hit;
            if (!Physics.Raycast(origin, direction, out hit, castDistance, LASER_LAYER_MASK, QueryTriggerInteraction.Ignore))
                return;

            // Skip hits on own aircraft
            if (attachedUnit != null)
            {
                Unit hitUnit = hit.collider.GetComponentInParent<Unit>();
                if (hitUnit != null && hitUnit == attachedUnit)
                    return;
            }

            // Try collider first, then parent (game objects may have IDamageable on parent)
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable == null)
                damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return;

            _lastDamageTick = Time.timeSinceLevelLoad;

            // DamagePerShot now means damage per ammo unit (per second)
            // Each tick applies a fraction: damagePerSecond * tickInterval
            float damagePerSecond = COILModPlugin.DamagePerShot.Value;
            float damageThisTick = damagePerSecond * DAMAGE_TICK_INTERVAL;

            // Reduced damage against surface targets (ground vehicles, ships, buildings)
            // Full damage only against Aircraft and Missiles
            Unit targetUnit = hit.collider.GetComponentInParent<Unit>();
            if (targetUnit != null && !(targetUnit is Aircraft) && !(targetUnit is Missile))
                damageThisTick *= Mathf.Clamp01(COILModPlugin.GroundDamagePercent.Value / 100f);

            float fireDamage = damageThisTick * 0.3f;
            float blastDamage = damageThisTick * 0.7f;

            damageable.TakeDamage(0f, blastDamage, 1f, fireDamage, 0f,
                attachedUnit != null ? attachedUnit.persistentID : PersistentID.None);
        }

        // Ammo reporting — secondary instances report 0 so AccountAmmo doesn't double
        public override int GetAmmoTotal() => _isPrimary ? _shotsRemaining : 0;
        public override int GetAmmoLoaded() => _isPrimary ? _shotsRemaining : 0;
        public override int GetFullAmmo() => _isPrimary ? COILModPlugin.MaxShots.Value : 0;

        public override void Rearm()
        {
            if (!_isPrimary) return;
            _shotsRemaining = COILModPlugin.MaxShots.Value;
            _isFiring = false;
            _ammoAccumulator = 0f;
            Log($"Rearmed to {_shotsRemaining} ammo");
        }

        private void OnDestroy()
        {
            // Clean up primary tracking
            if (attachedUnit != null && _primaryInstances.ContainsKey(attachedUnit)
                && _primaryInstances[attachedUnit] == this)
            {
                _primaryInstances.Remove(attachedUnit);
            }

            // Clean up audio
            if (_soundPlaying && _audioSource != null)
                _audioSource.Stop();
        }
    }
}
