using UnityEngine;
using System;

namespace NuclearOptionCOILMod
{
    /// <summary>
    /// COIL (Chemical Oxygen Iodine Laser) weapon implementation
    /// Based on YAL-1 Airborne Laser system
    /// </summary>
    public class COILLaser : Laser
    {
        private int _shotsRemaining;
        private float _shotStartTime;
        private bool _isFiring;
        private float _shotDuration;
        
        private LineRenderer _beamRenderer;
        private Light _beamLight;
        private ParticleSystem _muzzleFlash;
        private AudioSource _audioSource;

        private void Start()
        {
            _shotsRemaining = COILModPlugin.MaxShots.Value;
            _shotDuration = COILModPlugin.ShotDuration.Value;
            InitializeVisuals();
            InitializeAudio();
            
            Debug.Log($"[COIL Mod] COIL Laser initialized with {_shotsRemaining} shots of {_shotDuration}s each");
        }

        private void InitializeVisuals()
        {
            // Create beam renderer
            GameObject beamObj = new GameObject("COIL_Beam");
            beamObj.transform.SetParent(transform);
            beamObj.transform.localPosition = Vector3.zero;
            
            _beamRenderer = beamObj.AddComponent<LineRenderer>();
            _beamRenderer.startWidth = 0.5f;
            _beamRenderer.endWidth = 0.3f;
            _beamRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _beamRenderer.startColor = new Color(1f, 0.3f, 0.1f, 0.8f); // Orange-red for COIL
            _beamRenderer.endColor = new Color(1f, 0.5f, 0.2f, 0.4f);
            _beamRenderer.enabled = false;
            _beamRenderer.positionCount = 2;

            // Create beam light
            GameObject lightObj = new GameObject("COIL_Light");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            
            _beamLight = lightObj.AddComponent<Light>();
            _beamLight.type = LightType.Spot;
            _beamLight.color = new Color(1f, 0.4f, 0.1f);
            _beamLight.intensity = 8f;
            _beamLight.range = 100f;
            _beamLight.spotAngle = 30f;
            _beamLight.enabled = false;

            // Create muzzle flash particles
            GameObject particleObj = new GameObject("COIL_Particles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;
            
            _muzzleFlash = particleObj.AddComponent<ParticleSystem>();
            var main = _muzzleFlash.main;
            main.startColor = new Color(1f, 0.5f, 0.2f);
            main.startSize = 2f;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.maxParticles = 50;
            
            var emission = _muzzleFlash.emission;
            emission.rateOverTime = 100f;
            
            _muzzleFlash.Stop();
        }

        private void InitializeAudio()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1f;
            _audioSource.minDistance = 50f;
            _audioSource.maxDistance = 500f;
            _audioSource.loop = true;
            _audioSource.volume = 0.7f;
            _audioSource.pitch = 0.8f;
            
            // Note: In a full implementation, load actual audio clips
            // For now, this will be silent but functional
        }

        public override void Fire(Unit owner, Unit target, Vector3 inheritedVelocity, 
            WeaponStation weaponStation, GlobalPosition aimpoint)
        {
            if (_shotsRemaining <= 0)
            {
                Debug.Log("[COIL Mod] COIL Laser depleted");
                return;
            }

            // Check if target is within firing arc
            if (target != null && !IsTargetInFiringArc(target))
            {
                Debug.Log("[COIL Mod] Target outside firing arc");
                return;
            }

            if (!_isFiring)
            {
                _isFiring = true;
                _shotStartTime = Time.timeSinceLevelLoad;
                EnableBeam(true);
                Debug.Log($"[COIL Mod] COIL Laser firing - {_shotsRemaining} shots remaining");
            }

            base.Fire(owner, target, inheritedVelocity, weaponStation, aimpoint);
        }

        private bool IsTargetInFiringArc(Unit target)
        {
            if (attachedUnit == null || target == null)
                return false;

            // Get transforms from MonoBehaviour base
            Transform unitTransform = ((MonoBehaviour)attachedUnit).transform;
            Transform targetTransform = ((MonoBehaviour)target).transform;
            
            // Calculate direction to target
            Vector3 toTarget = (targetTransform.position - unitTransform.position).normalized;
            
            // Calculate angle from aircraft nose (forward direction)
            float angleFromNose = Vector3.Angle(unitTransform.forward, toTarget);
            
            // Check if within configured firing arc
            bool inArc = angleFromNose <= COILModPlugin.FiringArc.Value;
            
            if (!inArc)
            {
                Debug.Log($"[COIL Mod] Target at {angleFromNose:F1}° from nose (max: {COILModPlugin.FiringArc.Value}°)");
            }
            
            return inArc;
        }

        private void FixedUpdate()
        {
            if (!_isFiring)
                return;

            // Check if shot duration exceeded
            float firingDuration = Time.timeSinceLevelLoad - _shotStartTime;
            if (firingDuration >= _shotDuration)
            {
                _shotsRemaining--;
                _isFiring = false;
                EnableBeam(false);
                
                Debug.Log($"[COIL Mod] COIL shot complete - {_shotsRemaining} shots remaining");
                
                if (_shotsRemaining <= 0)
                {
                    Debug.Log("[COIL Mod] COIL Laser depleted");
                }
                return;
            }

            // Update beam visuals
            UpdateBeam();
            
            // Apply damage
            ApplyLaserDamage();
        }

        private void UpdateBeam()
        {
            if (_beamRenderer == null || currentTarget == null)
                return;

            Vector3 startPos = transform.position;
            Vector3 endPos = ((MonoBehaviour)currentTarget).transform.position;
            
            // Raycast to find actual hit point
            RaycastHit hit;
            if (Physics.Raycast(startPos, (endPos - startPos).normalized, out hit, 
                COILModPlugin.MaxRange.Value, ~0, QueryTriggerInteraction.Ignore))
            {
                endPos = hit.point;
            }

            _beamRenderer.SetPosition(0, startPos);
            _beamRenderer.SetPosition(1, endPos);
            
            if (_beamLight != null)
            {
                _beamLight.transform.position = startPos;
                _beamLight.transform.LookAt(endPos);
            }
        }

        private void ApplyLaserDamage()
        {
            if (currentTarget == null)
                return;

            // Check if target is still in firing arc
            if (!IsTargetInFiringArc(currentTarget))
            {
                _isFiring = false;
                EnableBeam(false);
                Debug.Log("[COIL Mod] Target left firing arc - stopping fire");
                return;
            }

            Transform targetTransform = ((MonoBehaviour)currentTarget).transform;
            Vector3 direction = (targetTransform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetTransform.position);
            
            if (distance > COILModPlugin.MaxRange.Value)
                return;

            // Raycast to hit target
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // Calculate damage per tick based on total damage per shot
                    float damagePerSecond = COILModPlugin.DamagePerShot.Value / _shotDuration;
                    float damageThisTick = damagePerSecond * Time.fixedDeltaTime;
                    float fireDamage = damageThisTick * 0.3f; // 30% fire damage
                    float blastDamage = damageThisTick * 0.7f; // 70% blast damage
                    
                    damageable.TakeDamage(0f, blastDamage, 1f, fireDamage, 0f, 
                        attachedUnit != null ? attachedUnit.persistentID : PersistentID.None);
                }
            }
        }

        private void EnableBeam(bool enable)
        {
            if (_beamRenderer != null)
                _beamRenderer.enabled = enable;
            
            if (_beamLight != null)
                _beamLight.enabled = enable;
            
            if (_muzzleFlash != null)
            {
                if (enable)
                    _muzzleFlash.Play();
                else
                    _muzzleFlash.Stop();
            }
            
            if (_audioSource != null)
            {
                if (enable)
                    _audioSource.Play();
                else
                    _audioSource.Stop();
            }
        }

        public override int GetAmmoTotal()
        {
            return _shotsRemaining;
        }

        public override int GetAmmoLoaded()
        {
            return _shotsRemaining;
        }

        public override int GetFullAmmo()
        {
            return COILModPlugin.MaxShots.Value;
        }

        public override void Rearm()
        {
            _shotsRemaining = COILModPlugin.MaxShots.Value;
            _isFiring = false;
            EnableBeam(false);
            Debug.Log($"[COIL Mod] COIL Laser rearmed to {_shotsRemaining} shots");
        }

        private new void OnDestroy()
        {
            EnableBeam(false);
        }
    }
}
