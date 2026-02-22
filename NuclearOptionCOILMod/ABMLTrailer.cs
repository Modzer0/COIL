using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuclearOptionCOILMod
{
    /// <summary>
    /// ABM-L Ground Laser Trailer - creates a new WeaponMount that reuses
    /// QoL's LaserTrailer1x1 prefab. When the trailer deploys, we detect
    /// the spawned LaserTrailer1 vehicle and modify its Laser to use COIL stats.
    /// 
    /// Key design: we do NOT clone the prefab GameObject. Cloning causes
    /// physics issues (yaw loss) and network spawn failures. Instead we
    /// create a new WeaponMount ScriptableObject pointing to the same prefab,
    /// and modify the spawned vehicle post-deploy via Unit.OnEnable postfix.
    /// </summary>
    public static class ABMLTrailer
    {
        private static BepInEx.Logging.ManualLogSource _logger;
        private static bool _initialized;
        private static bool _qolDetected;
        private static WeaponMount _abmlMount;
        // Track which LaserTrailer1 spawns should get COIL stats
        private static readonly HashSet<int> _pendingAbmlUnits = new HashSet<int>();
        // Mission editor vehicle definition
        private static VehicleDefinition _abmlVehicleDef;

        public static void SetLogger(BepInEx.Logging.ManualLogSource logger) => _logger = logger;

        private static void Log(string message)
        {
            if (_logger != null) _logger.LogInfo($"[ABM-L] {message}");
        }

        private static void LogError(string message)
        {
            if (_logger != null) _logger.LogError($"[ABM-L] {message}");
        }

        public static bool IsQoLDetected() => _qolDetected;
        public static WeaponMount GetMount() => _abmlMount;
        public static WeaponInfo GetInfo() => _abmlInfo;

        public static bool DetectQoL()
        {
            foreach (var plugin in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                string guid = plugin.Key.ToLower();
                if (guid.Contains("qol") || guid.Contains("qualityoflife"))
                {
                    _qolDetected = true;
                    Log($"QoL mod detected: {plugin.Key}");
                    return true;
                }
            }
            Log("QoL mod not detected - ABM-L trailer disabled");
            return false;
        }

        /// <summary>
        /// Called from Encyclopedia.AfterLoad PREFIX.
        /// Adds our mount to encyclopedia.weaponMounts so AfterLoad processes it
        /// (calls Initialize(), assigns LookupIndex, adds to WeaponLookup).
        /// </summary>
        public static void TryInitialize(Encyclopedia encyclopedia)
        {
            if (!COILModPlugin.EnableABMLTrailer.Value)
            {
                Log("ABM-L trailer disabled in config");
                return;
            }

            if (_initialized && _abmlMount != null)
            {
                if (!encyclopedia.weaponMounts.Contains(_abmlMount))
                    encyclopedia.weaponMounts.Add(_abmlMount);
                return;
            }

            if (!DetectQoL())
                return;

            try
            {
                CreateABMLTrailer(encyclopedia);
                if (_abmlMount != null)
                    _initialized = true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to create ABM-L trailer: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // Store our custom WeaponInfo separately so Initialize() can't destroy it
        private static WeaponInfo _abmlInfo;

        private static void CreateABMLTrailer(Encyclopedia encyclopedia)
        {
            // Find the LaserTrailer1x1 mount created by QoL
            var laserTrailerMount = UnityEngine.Resources.FindObjectsOfTypeAll<WeaponMount>()
                .FirstOrDefault(m => m.name.Equals("LaserTrailer1x1", StringComparison.InvariantCultureIgnoreCase));

            if (laserTrailerMount == null)
            {
                Log("LaserTrailer1x1 mount not found yet - QoL may not have initialized");
                _initialized = false;
                return;
            }

            Log($"Found LaserTrailer1x1: Cargo={laserTrailerMount.Cargo}, ammo={laserTrailerMount.ammo}, mountName={laserTrailerMount.mountName}");
            if (laserTrailerMount.info != null)
                Log($"  LADS info: weaponName={laserTrailerMount.info.weaponName}");

            // Clone the WeaponMount ScriptableObject — NOT the prefab GameObject.
            // Reuse the same prefab so physics and networking work identically to LADS.
            _abmlMount = UnityEngine.Object.Instantiate(laserTrailerMount);
            _abmlMount.name = "ABMLTrailer1x1";
            _abmlMount.jsonKey = "ABMLTrailer1x1";
            _abmlMount.dontAutomaticallyAddToEncyclopedia = false;
            // prefab stays the same as LaserTrailer1x1 — same MountedCargo, same vehicle

            // Create custom WeaponInfo for display purposes
            _abmlInfo = ScriptableObject.CreateInstance<WeaponInfo>();
            _abmlInfo.name = "ABMLTrailer1_info";
            _abmlInfo.weaponName = "HLT ABM-L Trailer";
            _abmlInfo.shortName = "ABM-L";
            _abmlInfo.description = "Anti-Ballistic Missile Laser - COIL-based ground defense";
            _abmlInfo.fireInterval = 0f;
            _abmlInfo.energy = false;
            _abmlInfo.gun = false;
            _abmlInfo.bomb = false;
            _abmlInfo.nuclear = false;
            _abmlInfo.strategic = false;
            _abmlInfo.overHorizon = false;
            _abmlInfo.boresight = false;
            _abmlInfo.laserGuided = false;
            _abmlInfo.cargo = true;
            _abmlInfo.troops = false;
            _abmlInfo.sling = false;
            _abmlInfo.muzzleVelocity = 299792458f;
            _abmlInfo.pierceDamage = 0f;
            _abmlInfo.blastDamage = COILModPlugin.DamagePerShot.Value;
            _abmlInfo.armorTierEffectiveness = 10f;
            _abmlInfo.visibilityWhenFired = 50f;
            _abmlInfo.costPerRound = 0f;
            _abmlInfo.massPerRound = 0f;
            _abmlInfo.useWeaponDoors = false;
            _abmlInfo.effectiveness = new RoleIdentity
            {
                antiSurface = 0f,
                antiAir = 0f,
                antiMissile = 1f,
                antiRadar = 0f
            };
            _abmlInfo.pK = 0.9f;
            _abmlInfo.targetRequirements = new TargetRequirements
            {
                maxRange = COILModPlugin.MaxRange.Value,
                minRange = 100f,
                maxSpeed = 10000f,
                maxAltitude = 50000f,
                lineOfSight = true,
                minAlignment = 360f,
                minOwnerSpeed = 0f
            };
            _abmlInfo.weaponPrefab = null;

            // Copy icon from original
            if (laserTrailerMount.info != null && laserTrailerMount.info.weaponIcon != null)
                _abmlInfo.weaponIcon = laserTrailerMount.info.weaponIcon;

            // Add to encyclopedia BEFORE AfterLoad processes it.
            // Do NOT call Initialize() ourselves — AfterLoad calls Initialize() on
            // every mount in encyclopedia.weaponMounts. Initialize() will overwrite
            // our info and mountName because Cargo=true causes it to read from the
            // prefab's MountedCargo.info. We fix this in a postfix after AfterLoad.
            _abmlMount.info = _abmlInfo;
            _abmlMount.mountName = "HLT ABM-L Trailer";
            encyclopedia.weaponMounts.Add(_abmlMount);

            Log($"Added ABM-L to encyclopedia.weaponMounts (count: {encyclopedia.weaponMounts.Count})");
            Log($"AfterLoad will call Initialize() and assign LookupIndex automatically");

            // Add to cargo hardpoint options
            AddToCargoHardpoints();

            Log($"ABM-L trailer created (reusing LADS prefab) - range: {COILModPlugin.MaxRange.Value}m");
        }

        /// <summary>
        /// Called AFTER Encyclopedia.AfterLoad completes. Restores our custom info
        /// and mountName that Initialize() overwrote with the prefab's LADS values.
        /// Also registers in WeaponLookup since AfterLoad builds that too.
        /// </summary>
        public static void RestoreAfterInitialize()
        {
            if (_abmlMount == null || _abmlInfo == null) return;

            // Initialize() overwrites info with prefab's MountedCargo.info and
            // sets ammo = number of Weapon components in prefab. Restore ours.
            string prevInfo = _abmlMount.info != null ? _abmlMount.info.weaponName : "null";
            _abmlMount.info = _abmlInfo;
            _abmlMount.mountName = "HLT ABM-L Trailer";
            // Ammo for cargo = number of deployable units (1 trailer)
            _abmlMount.ammo = 1;

            Log($"Restored ABM-L after Initialize(): info was '{prevInfo}', now '{_abmlMount.info.weaponName}', mountName='{_abmlMount.mountName}', ammo={_abmlMount.ammo}");

            // Ensure in WeaponLookup (AfterLoad rebuilds it)
            if (!Encyclopedia.WeaponLookup.ContainsKey(_abmlMount.jsonKey))
                Encyclopedia.WeaponLookup.Add(_abmlMount.jsonKey, _abmlMount);

            // Verify LookupIndex was assigned by AfterLoad
            var netDef = (INetworkDefinition)_abmlMount;
            if (netDef.LookupIndex.HasValue)
                Log($"LookupIndex assigned by AfterLoad: {netDef.LookupIndex.Value}");
            else
                LogError("LookupIndex NOT assigned — network serialization will fail!");

            // Create mission editor vehicle definition
            TryCreateVehicleDefinition();
        }

        /// <summary>
        /// Creates a VehicleDefinition for the ABM-L so it appears in the
        /// mission editor's vehicle list. Clones the LaserTrailer1's definition.
        /// </summary>
        public static void TryCreateVehicleDefinition()
        {
            if (_abmlVehicleDef != null) return;
            if (!COILModPlugin.EnableABMLTrailer.Value) return;
            if (!_qolDetected) DetectQoL();
            if (!_qolDetected) return;

            try
            {
                var laserTrailerDef = UnityEngine.Resources.FindObjectsOfTypeAll<VehicleDefinition>()
                    .FirstOrDefault(d => d.name != null &&
                        d.name.IndexOf("LaserTrailer1", StringComparison.InvariantCultureIgnoreCase) >= 0
                        && !d.disabled);

                if (laserTrailerDef == null)
                {
                    laserTrailerDef = UnityEngine.Resources.FindObjectsOfTypeAll<VehicleDefinition>()
                        .FirstOrDefault(d => d.unitName != null &&
                            d.unitName.IndexOf("Laser", StringComparison.InvariantCultureIgnoreCase) >= 0
                            && d.unitName.IndexOf("Trailer", StringComparison.InvariantCultureIgnoreCase) >= 0
                            && !d.disabled);
                }

                if (laserTrailerDef == null)
                {
                    Log("LaserTrailer1 VehicleDefinition not found — mission editor entry skipped");
                    return;
                }

                Log($"Found LaserTrailer1 VehicleDefinition: name={laserTrailerDef.name}, unitName={laserTrailerDef.unitName}");

                _abmlVehicleDef = UnityEngine.Object.Instantiate(laserTrailerDef);
                _abmlVehicleDef.name = "ABMLTrailer1";
                _abmlVehicleDef.jsonKey = "ABMLTrailer1";
                _abmlVehicleDef.unitName = "HLT ABM-L Trailer";
                _abmlVehicleDef.bogeyName = "ABM-L";
                _abmlVehicleDef.code = "ABM-L";
                _abmlVehicleDef.description = "Anti-Ballistic Missile Laser — COIL-based ground defense. Prioritizes nuclear cruise missiles.";
                _abmlVehicleDef.disabled = false;
                _abmlVehicleDef.dontAutomaticallyAddToEncyclopedia = false;
                _abmlVehicleDef.unitPrefab = laserTrailerDef.unitPrefab;
                _abmlVehicleDef.roleIdentity = new RoleIdentity
                {
                    antiSurface = 0f, antiAir = 0f, antiMissile = 1f, antiRadar = 0f
                };

                Encyclopedia enc = Encyclopedia.i;
                if (enc == null) { Log("Encyclopedia.i null — cannot register"); return; }

                if (!enc.vehicles.Contains(_abmlVehicleDef))
                    enc.vehicles.Add(_abmlVehicleDef);

                if (!Encyclopedia.Lookup.ContainsKey(_abmlVehicleDef.jsonKey))
                    Encyclopedia.Lookup.Add(_abmlVehicleDef.jsonKey, _abmlVehicleDef);

                var vehNetDef = (INetworkDefinition)_abmlVehicleDef;
                if (!vehNetDef.LookupIndex.HasValue)
                {
                    int newIndex = enc.IndexLookup.Count;
                    enc.IndexLookup.Add(vehNetDef);
                    vehNetDef.LookupIndex = newIndex;
                }

                _abmlVehicleDef.CacheMass();
                Log($"ABM-L VehicleDefinition registered: unitName={_abmlVehicleDef.unitName}, jsonKey={_abmlVehicleDef.jsonKey}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to create ABM-L VehicleDefinition: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static VehicleDefinition GetVehicleDefinition() => _abmlVehicleDef;

        /// <summary>
        /// Called when the mission editor spawns an ABM-L. Flags the next
        /// LaserTrailer1 spawn for COIL stat application.
        /// </summary>
        public static void OnEditorSpawnDetected()
        {
            _abmlDeployPending = true;
            _lastAbmlDeployTime = Time.timeSinceLevelLoad;
            Log("Mission editor ABM-L spawn — next LaserTrailer1 will get COIL stats");
        }

        private static void AddToCargoHardpoints()
        {
            var laserTrailerMount = UnityEngine.Resources.FindObjectsOfTypeAll<WeaponMount>()
                .FirstOrDefault(m => m.name.Equals("LaserTrailer1x1", StringComparison.InvariantCultureIgnoreCase));

            if (laserTrailerMount == null)
            {
                Log("LaserTrailer1x1 mount not found - cannot add ABM-L to cargo slots");
                return;
            }

            int added = 0;
            foreach (var wm in UnityEngine.Resources.FindObjectsOfTypeAll<WeaponManager>())
            {
                if (wm.hardpointSets == null) continue;
                foreach (var hs in wm.hardpointSets)
                {
                    if (hs.weaponOptions != null && hs.weaponOptions.Contains(laserTrailerMount))
                    {
                        if (!hs.weaponOptions.Contains(_abmlMount))
                        {
                            hs.weaponOptions.Add(_abmlMount);
                            added++;
                            Log($"Added ABM-L to {wm.gameObject.name}/{hs.name} weapon options");
                        }
                    }
                }
            }
            Log($"Added ABM-L to {added} cargo hardpoint set(s)");
        }

        public static void EnsureInWeaponOptions(HardpointSet hardpointSet)
        {
            if (!COILModPlugin.EnableABMLTrailer.Value) return;
            if (hardpointSet == null || hardpointSet.weaponOptions == null) return;

            // Retry initialization if it failed earlier (QoL wasn't ready)
            if (!_initialized || _abmlMount == null)
            {
                if (!DetectQoL()) return;
                try
                {
                    var encyclopedia = UnityEngine.Resources.FindObjectsOfTypeAll<Encyclopedia>()
                        .FirstOrDefault();
                    if (encyclopedia != null)
                    {
                        // Late init: QoL is now ready. Create the mount.
                        CreateABMLTrailer(encyclopedia);
                        if (_abmlMount != null)
                        {
                            _initialized = true;
                            // Since AfterLoad already ran, we must manually register
                            LateRegisterInEncyclopedia(encyclopedia);
                            Log("Late-initialized ABM-L trailer");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Late init failed: {ex.Message}");
                }
            }

            if (_abmlMount == null) return;

            var laserTrailerMount = UnityEngine.Resources.FindObjectsOfTypeAll<WeaponMount>()
                .FirstOrDefault(m => m.name.Equals("LaserTrailer1x1", StringComparison.InvariantCultureIgnoreCase));

            if (laserTrailerMount != null && hardpointSet.weaponOptions.Contains(laserTrailerMount))
            {
                if (!hardpointSet.weaponOptions.Contains(_abmlMount))
                {
                    hardpointSet.weaponOptions.Add(_abmlMount);
                    Log($"Added ABM-L to {hardpointSet.name} weapon options");
                }
            }
        }

        /// <summary>
        /// Late registration when AfterLoad already ran. Manually handles
        /// IndexLookup, WeaponLookup, and restores info after Initialize().
        /// </summary>
        private static void LateRegisterInEncyclopedia(Encyclopedia encyclopedia)
        {
            if (_abmlMount == null) return;

            // AfterLoad already ran, so we need to manually call Initialize
            // and then fix the damage it does to our info/mountName.
            try { _abmlMount.Initialize(); }
            catch (Exception ex) { Log($"Late Initialize note: {ex.Message}"); }

            // Restore our custom values
            _abmlMount.info = _abmlInfo;
            _abmlMount.mountName = "HLT ABM-L Trailer";
            _abmlMount.ammo = 1;

            // Register in IndexLookup manually
            var netDef = (INetworkDefinition)_abmlMount;
            if (!netDef.LookupIndex.HasValue)
            {
                int newIndex = encyclopedia.IndexLookup.Count;
                encyclopedia.IndexLookup.Add(netDef);
                netDef.LookupIndex = newIndex;
                Log($"Late-registered in IndexLookup at {newIndex}");
            }

            // Register in WeaponLookup
            if (!Encyclopedia.WeaponLookup.ContainsKey(_abmlMount.jsonKey))
                Encyclopedia.WeaponLookup.Add(_abmlMount.jsonKey, _abmlMount);

            Log("Late registration complete");

            // Also create the mission editor vehicle definition
            TryCreateVehicleDefinition();
        }

        /// <summary>
        /// Called when a unit spawns. Since we reuse the LADS prefab, the spawned
        /// vehicle is a LaserTrailer1. We detect it and apply COIL stats.
        /// We identify ABM-L deploys by checking if the spawning aircraft had
        /// our ABM-L mount selected.
        /// </summary>
        public static void OnUnitSpawned(Unit unit)
        {
            if (!_qolDetected || !COILModPlugin.EnableABMLTrailer.Value) return;
            if (unit == null) return;

            string unitName = unit.name ?? "";

            // The spawned vehicle from our mount will be a LaserTrailer1 (same prefab)
            // We detect it by checking if it's a LaserTrailer1 and was recently deployed
            if (!unitName.Contains("LaserTrailer1") && !unitName.Contains("ABMLTrailer"))
                return;

            // Check if this unit's instance ID was flagged for ABM-L modification
            int instanceId = ((MonoBehaviour)unit).gameObject.GetInstanceID();
            if (_pendingAbmlUnits.Contains(instanceId))
            {
                _pendingAbmlUnits.Remove(instanceId);
                ApplyCoilStats(unit);
                return;
            }

            // Fallback: apply COIL stats to ALL LaserTrailer1 spawns that happen
            // shortly after an ABM-L deploy was triggered
            if (_abmlDeployPending && Time.timeSinceLevelLoad - _lastAbmlDeployTime < 5f)
            {
                _abmlDeployPending = false;
                ApplyCoilStats(unit);
            }
        }

        private static bool _abmlDeployPending;
        private static float _lastAbmlDeployTime;

        /// <summary>
        /// Called from MountedCargo.Fire postfix when our ABM-L mount fires.
        /// Flags that the next LaserTrailer1 spawn should get COIL stats.
        /// </summary>
        public static void OnAbmlDeployed()
        {
            _abmlDeployPending = true;
            _lastAbmlDeployTime = Time.timeSinceLevelLoad;
            Log("ABM-L deploy triggered - next LaserTrailer1 spawn will get COIL stats");
        }

        private static void ApplyCoilStats(Unit unit)
        {
            Log($"Applying COIL stats to spawned trailer: {unit.name}");

            // Modify Laser components
            Laser[] lasers = ((MonoBehaviour)unit).GetComponentsInChildren<Laser>(true);
            foreach (var laser in lasers)
            {
                var t = Traverse.Create(laser);
                float dps = COILModPlugin.DamagePerShot.Value;
                t.Field("blastDamage").SetValue(dps * 0.7f);
                t.Field("fireDamage").SetValue(dps * 0.3f);

                AnimationCurve rangeCurve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(COILModPlugin.MaxRange.Value * 0.8f, 1f),
                    new Keyframe(COILModPlugin.MaxRange.Value, 0.5f),
                    new Keyframe(COILModPlugin.MaxRange.Value * 1.2f, 0f)
                );
                t.Field("damageAtRange").SetValue(rangeCurve);
                t.Field("power").SetValue(5000f);
                t.Field("maxAngle").SetValue(360f);

                Log($"Laser modified: blastDmg={dps * 0.7f}, fireDmg={dps * 0.3f}, range={COILModPlugin.MaxRange.Value}");
            }

            // Modify Turret for instant targeting of nuclear cruise missiles
            Turret[] turrets = ((MonoBehaviour)unit).GetComponentsInChildren<Turret>(true);
            foreach (var turret in turrets)
            {
                var tt = Traverse.Create(turret);
                // Zero lock time = fire immediately when on target
                tt.Field("lockTime").SetValue(0f);
                // Very fast target reassessment
                tt.Field("targetAssessmentInterval").SetValue(0.25f);
                // Fast traverse to snap onto targets quickly
                tt.Field("traverseRate").SetValue(180f);
                Log("Turret modified: lockTime=0, assessInterval=0.25s, traverseRate=180");
            }

            // Set targeting to anti-missile only
            if (unit.definition != null)
            {
                unit.definition.roleIdentity = new RoleIdentity
                {
                    antiSurface = 0f,
                    antiAir = 0f,
                    antiMissile = 1f,
                    antiRadar = 0f
                };
            }

            foreach (var station in unit.weaponStations)
            {
                if (station.WeaponInfo != null)
                {
                    station.WeaponInfo.effectiveness = new RoleIdentity
                    {
                        antiSurface = 0f,
                        antiAir = 0f,
                        antiMissile = 1f,
                        antiRadar = 0f
                    };
                    var reqs = station.WeaponInfo.targetRequirements;
                    reqs.lineOfSight = true;
                    reqs.maxRange = COILModPlugin.MaxRange.Value;
                    reqs.minRange = 100f;
                    reqs.minAlignment = 360f;
                    reqs.maxSpeed = 10000f;
                    reqs.maxAltitude = 50000f;
                    station.WeaponInfo.targetRequirements = reqs;

                    // High armor tier effectiveness so it can engage anything
                    station.WeaponInfo.armorTierEffectiveness = 10f;
                }
            }
            Log("ABM-L targeting set to anti-missile, instant lock, nuclear priority");

            // Add nuclear priority targeting override component
            var go = ((MonoBehaviour)unit).gameObject;
            if (go.GetComponent<ABMLTargetOverride>() == null)
            {
                go.AddComponent<ABMLTargetOverride>();
                Log("Added ABMLTargetOverride component for nuclear cruise missile priority");
            }
        }
    }
}
