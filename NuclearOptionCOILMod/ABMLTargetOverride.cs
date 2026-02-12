using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace NuclearOptionCOILMod
{
    /// <summary>
    /// Attached to deployed ABM-L trailers. Overrides the turret's target
    /// selection to prioritize nuclear cruise missiles above all else.
    /// For non-nuclear missiles, defers to the game's default targeting
    /// which naturally prefers closer targets.
    /// </summary>
    public class ABMLTargetOverride : MonoBehaviour
    {
        private Unit _attachedUnit;
        private Turret[] _turrets;
        private float _lastScan;
        private const float SCAN_INTERVAL = 0.25f;
        private List<TrackingInfo> _scanResults = new List<TrackingInfo>();

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
                Log("WARNING: No attached unit found");
            else
                Log($"Target override active on {_attachedUnit.name} with {_turrets.Length} turret(s)");
        }

        private void FixedUpdate()
        {
            if (_attachedUnit == null || _attachedUnit.disabled) return;
            if (_attachedUnit.NetworkHQ == null) return;
            if (!_attachedUnit.IsServer) return;
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
                    // Nuclear missile — highest priority, prefer closest
                    if (dist < bestNuclearDist)
                    {
                        bestNuclear = target;
                        bestNuclearDist = dist;
                    }
                }
                else
                {
                    // Regular missile — prefer closest
                    if (dist < bestMissileDist)
                    {
                        bestMissile = target;
                        bestMissileDist = dist;
                    }
                }
            }

            // Nuclear always wins over non-nuclear
            Unit chosenTarget = bestNuclear ?? bestMissile;
            if (chosenTarget == null) return;

            // Force-set target on all turrets
            foreach (var turret in _turrets)
            {
                if (turret == null) continue;

                Unit currentTurretTarget = turret.GetTarget();
                if (currentTurretTarget == chosenTarget) continue;

                // Only override if we found a nuclear target or turret has no target
                if (bestNuclear != null || currentTurretTarget == null || currentTurretTarget.disabled)
                {
                    turret.SetTargetFromController(chosenTarget);
                    if (bestNuclear != null && currentTurretTarget != chosenTarget)
                        Log($"NUCLEAR PRIORITY: {chosenTarget.name} at {bestNuclearDist:F0}m");
                }
            }
        }

        private void OnDestroy()
        {
            _turrets = null;
            _attachedUnit = null;
        }
    }
}
