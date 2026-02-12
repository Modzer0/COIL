using UnityEngine;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace NuclearOptionCOILMod
{
    /// <summary>
    /// Attached to deployed ABM-L trailers. Overrides the turret's target
    /// selection to prioritize nuclear cruise missiles above all else.
    /// For non-nuclear missiles, defers to the game's default targeting
    /// which naturally prefers closer targets.
    /// 
    /// Also manages ammo (fire-seconds) when ABMLUseAmmo is enabled.
    /// Tracks laser firing time, depletes ammo, zeroes damage when empty,
    /// and hooks into the rearm system for resupply from ammo trucks/containers.
    /// </summary>
    public class ABMLTargetOverride : MonoBehaviour
    {
        private Unit _attachedUnit;
        private Turret[] _turrets;
        private float _lastScan;
        private const float SCAN_INTERVAL = 0.25f;
        private List<TrackingInfo> _scanResults = new List<TrackingInfo>();

        // Ammo tracking
        private Laser[] _lasers;
        private float _fireSecondsRemaining;
        private float _maxFireSeconds;
        private bool _ammoEnabled;
        private bool _depleted;
        private float _savedBlastDamage;
        private float _savedFireDamage;
        private bool _damageValuesSaved;
        private bool _rearmRegistered;
        private Renderer _beamRenderer;

        private static BepInEx.Logging.ManualLogSource _logger;
        public static void SetLogger(BepInEx.Logging.ManualLogSource logger) => _logger = logger;

        private static void Log(string msg)
        {
            if (_logger != null) _logger.LogInfo($"[ABM-L TGT] {msg}");
        }

        private void Start()
        {
            _attachedUnit = GetComponentInParent<Unit>();
            _turrets = GetComponentsInChildren<Turret>(true);

            if (_attachedUnit == null)
            {
                Log("WARNING: No attached unit found");
                return;
            }

            Log($"Target override active on {_attachedUnit.name} with {_turrets.Length} turret(s)");

            // Initialize ammo system
            _ammoEnabled = COILModPlugin.ABMLUseAmmo.Value;
            _maxFireSeconds = COILModPlugin.ABMLFireSeconds.Value;
            _fireSecondsRemaining = _maxFireSeconds;
            _depleted = false;

            if (_ammoEnabled)
            {
                _lasers = GetComponentsInChildren<Laser>(true);
                if (_lasers != null && _lasers.Length > 0)
                {
                    // Cache the beam renderer to detect firing state
                    var traverse = Traverse.Create(_lasers[0]);
                    var rendererField = traverse.Field("beamRenderer");
                    if (rendererField.FieldExists())
                        _beamRenderer = rendererField.GetValue<Renderer>();

                    // Save original damage values
                    SaveDamageValues(_lasers[0]);
                    Log($"Ammo system active: {_fireSecondsRemaining:F0}s fire time, {_lasers.Length} laser(s)");
                }
                else
                {
                    Log("WARNING: No Laser components found — ammo system disabled");
                    _ammoEnabled = false;
                }

                // Subscribe to rearm events
                RegisterRearm();
            }
        }

        private void SaveDamageValues(Laser laser)
        {
            var t = Traverse.Create(laser);
            _savedBlastDamage = t.Field("blastDamage").GetValue<float>();
            _savedFireDamage = t.Field("fireDamage").GetValue<float>();
            _damageValuesSaved = true;
            Log($"Saved damage values: blast={_savedBlastDamage:F1}, fire={_savedFireDamage:F1}");
        }

        private void RegisterRearm()
        {
            if (_rearmRegistered || _attachedUnit == null) return;

            GroundVehicle gv = _attachedUnit as GroundVehicle;
            if (gv != null)
            {
                gv.OnRearm += OnRearm;
                _rearmRegistered = true;
                Log("Registered for rearm events");
            }
        }

        private void OnRearm(RearmEventArgs args)
        {
            if (!_ammoEnabled) return;

            _fireSecondsRemaining = _maxFireSeconds;
            _depleted = false;

            // Restore damage values on all lasers
            if (_damageValuesSaved && _lasers != null)
            {
                foreach (var laser in _lasers)
                {
                    if (laser == null) continue;
                    var t = Traverse.Create(laser);
                    t.Field("blastDamage").SetValue(_savedBlastDamage);
                    t.Field("fireDamage").SetValue(_savedFireDamage);
                }
            }

            Log($"Rearmed — {_fireSecondsRemaining:F0}s fire time restored");
        }

        private void FixedUpdate()
        {
            if (_attachedUnit == null || _attachedUnit.disabled) return;
            if (_attachedUnit.NetworkHQ == null) return;
            if (!_attachedUnit.IsServer) return;

            // Ammo tracking — drain fire-seconds while laser is active
            if (_ammoEnabled && !_depleted)
                TrackAmmo();

            if (_turrets == null || _turrets.Length == 0) return;

            if (Time.timeSinceLevelLoad - _lastScan < SCAN_INTERVAL) return;
            _lastScan = Time.timeSinceLevelLoad;

            // Scan for threats
            _scanResults = _attachedUnit.NetworkHQ.GetTargetsWithinRange(
                _scanResults, transform, COILModPlugin.MaxRange.Value, false);

            Unit bestNuclear = null;
            float bestNuclearDist = float.MaxValue;
            Unit bestMissile = null;
            float bestMissileDist = float.MaxValue;

            foreach (var trackingInfo in _scanResults)
            {
                Unit target;
                if (!trackingInfo.TryGetUnit(out target)) continue;
                if (target.disabled) continue;

                Missile missile = target as Missile;
                if (missile == null) continue;

                WeaponInfo missileInfo = missile.GetWeaponInfo();
                if (missileInfo == null) continue;

                float dist = Vector3.Distance(transform.position,
                    ((MonoBehaviour)target).transform.position);

                if (missileInfo.nuclear)
                {
                    if (dist < bestNuclearDist)
                    {
                        bestNuclear = target;
                        bestNuclearDist = dist;
                    }
                }
                else
                {
                    if (dist < bestMissileDist)
                    {
                        bestMissile = target;
                        bestMissileDist = dist;
                    }
                }
            }

            Unit chosenTarget = bestNuclear ?? bestMissile;
            if (chosenTarget == null) return;

            foreach (var turret in _turrets)
            {
                if (turret == null) continue;

                Unit currentTurretTarget = turret.GetTarget();
                if (currentTurretTarget == chosenTarget) continue;

                if (bestNuclear != null || currentTurretTarget == null || currentTurretTarget.disabled)
                {
                    turret.SetTargetFromController(chosenTarget);
                    if (bestNuclear != null && currentTurretTarget != chosenTarget)
                        Log($"NUCLEAR PRIORITY: {chosenTarget.name} at {bestNuclearDist:F0}m");
                }
            }
        }

        private void TrackAmmo()
        {
            // Detect if the laser is actively firing by checking beam renderer
            bool isFiring = _beamRenderer != null && _beamRenderer.enabled;

            if (isFiring)
            {
                _fireSecondsRemaining -= Time.fixedDeltaTime;

                if (_fireSecondsRemaining <= 0f)
                {
                    _fireSecondsRemaining = 0f;
                    _depleted = true;

                    // Zero out damage on all lasers so they do nothing
                    if (_lasers != null)
                    {
                        foreach (var laser in _lasers)
                        {
                            if (laser == null) continue;
                            var t = Traverse.Create(laser);
                            t.Field("blastDamage").SetValue(0f);
                            t.Field("fireDamage").SetValue(0f);
                        }
                    }

                    // Request resupply
                    GroundVehicle gv = _attachedUnit as GroundVehicle;
                    if (gv != null)
                        gv.RequestRearm();

                    Log("AMMO DEPLETED — requesting resupply");
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from rearm events
            if (_rearmRegistered && _attachedUnit != null)
            {
                GroundVehicle gv = _attachedUnit as GroundVehicle;
                if (gv != null)
                    gv.OnRearm -= OnRearm;
            }

            _turrets = null;
            _attachedUnit = null;
            _lasers = null;
        }
    }
}
