using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using NuclearOption.SavedMission;
using System.Collections;

namespace NuclearOptionCOILMod
{
    /// <summary>
    /// Harmony patches to add COIL laser weapon to Darkreach bomber.
    /// 
    /// KEY DESIGN (matching QoL mod pattern):
    /// We patch Encyclopedia.AfterLoad as a PREFIX to add our WeaponMount to
    /// encyclopedia.weaponMounts BEFORE AfterLoad builds IndexLookup.
    /// This ensures our mount gets a proper LookupIndex assigned by the game's
    /// own code, which means it survives network serialization (SyncVar/Mirage).
    /// </summary>
    public class COILLaserPatch
    {
        private static WeaponInfo _coilWeaponInfo;
        private static WeaponMount _coilWeaponMount;
        private static bool _initialized = false;
        private static BepInEx.Logging.ManualLogSource _logger;

        public static WeaponMount GetCOILMount() => _coilWeaponMount;

        public static void SetLogger(BepInEx.Logging.ManualLogSource logger)
        {
            _logger = logger;
        }

        private static void Log(string message)
        {
            if (_logger != null)
                _logger.LogInfo($"[COIL] {message}");
        }

        private static void LogError(string message)
        {
            if (_logger != null)
                _logger.LogError($"[COIL] {message}");
        }

        // =====================================================================
        // PATCH: Encyclopedia.AfterLoad PREFIX
        // Add our WeaponMount to encyclopedia.weaponMounts BEFORE AfterLoad
        // builds IndexLookup. AfterLoad then assigns LookupIndex automatically.
        // =====================================================================

        public static void Encyclopedia_AfterLoad_Prefix(Encyclopedia __instance)
        {
            try
            {
                if (_initialized && _coilWeaponMount != null)
                {
                    if (!__instance.weaponMounts.Contains(_coilWeaponMount))
                    {
                        __instance.weaponMounts.Add(_coilWeaponMount);
                        Log("Re-added existing COIL mount to encyclopedia.weaponMounts");
                    }
                    // Also handle ABM-L trailer
                    ABMLTrailer.TryInitialize(__instance);
                    return;
                }

                InitializeCOILLaser();
                _initialized = true;

                __instance.weaponMounts.Add(_coilWeaponMount);
                Log($"Added COIL to encyclopedia.weaponMounts (count now: {__instance.weaponMounts.Count})");
                Log("AfterLoad will assign LookupIndex automatically");

                // Initialize ABM-L trailer (will detect QoL and create if available)
                ABMLTrailer.TryInitialize(__instance);
            }
            catch (System.Exception ex)
            {
                LogError($"Encyclopedia_AfterLoad_Prefix error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // =====================================================================
        // PATCH: Encyclopedia.AfterLoad POSTFIX
        // After AfterLoad runs Initialize() on all mounts, restore our ABM-L
        // mount's info/mountName/ammo that Initialize() overwrote with the
        // prefab's LADS values (because Cargo=true reads from prefab weapons).
        // Also restores COIL mount info as a safety measure.
        // =====================================================================

        public static void Encyclopedia_AfterLoad_Postfix(Encyclopedia __instance)
        {
            try
            {
                // Restore COIL mount (shouldn't be needed since Cargo=false, but be safe)
                if (_coilWeaponMount != null && _coilWeaponInfo != null)
                {
                    _coilWeaponMount.info = _coilWeaponInfo;
                    _coilWeaponMount.mountName = "ABM COIL Laser";
                    _coilWeaponMount.ammo = COILModPlugin.MaxShots.Value;
                    Log("AfterLoad POSTFIX: restored COIL mount info");
                }

                // Restore ABM-L mount (critical — Initialize() overwrites info with LADS)
                ABMLTrailer.RestoreAfterInitialize();
            }
            catch (System.Exception ex)
            {
                LogError($"Encyclopedia_AfterLoad_Postfix error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // =====================================================================
        // PATCH: WeaponSelector.PopulateOptions PREFIX
        // Add COIL to Inner Bay weapon options in the loadout screen.
        // Also ensures COIL is persistently in the prefab's weaponOptions
        // so VetLoadout won't strip it at spawn time.
        // =====================================================================

        public static void WeaponSelector_PopulateOptions_Prefix(HardpointSet hardpointSet)
        {
            try
            {
                if (hardpointSet == null) return;

                // Add COIL to Inner Bay
                if (_coilWeaponMount != null && hardpointSet.name.ToLower().Contains("inner bay"))
                {
                    if (!hardpointSet.weaponOptions.Contains(_coilWeaponMount))
                    {
                        hardpointSet.weaponOptions.Add(_coilWeaponMount);
                        Log($"Added COIL to {hardpointSet.name} options (count: {hardpointSet.weaponOptions.Count})");
                    }
                }

                // Add ABM-L to cargo slots that have the LADS trailer
                ABMLTrailer.EnsureInWeaponOptions(hardpointSet);
            }
            catch (System.Exception ex)
            {
                LogError($"PopulateOptions error: {ex.Message}");
            }
        }

        // =====================================================================
        // PATCH: WeaponChecker.VetLoadout PREFIX
        // Ensure COIL is in the Inner Bay's weaponOptions BEFORE VetLoadout
        // checks. This covers the case where VetLoadout runs before the
        // loadout screen has been opened (e.g. mission editor spawns).
        // =====================================================================

        public static void WeaponChecker_VetLoadout_Prefix(AircraftDefinition definition, Loadout loadout)
        {
            try
            {
                if (definition == null || loadout == null) return;

                bool hasCOIL = false;
                bool hasABML = false;
                foreach (var w in loadout.weapons)
                {
                    if (w == null) continue;
                    if (w.mountName == "ABM COIL Laser") hasCOIL = true;
                    if (w.mountName == "HLT ABM-L Trailer") hasABML = true;
                }

                if (hasCOIL && _coilWeaponMount != null)
                {
                    Aircraft prefabAircraft = definition.unitPrefab.GetComponent<Aircraft>();
                    if (prefabAircraft != null && prefabAircraft.weaponManager != null)
                    {
                        EnsureCOILInWeaponOptions(prefabAircraft.weaponManager);
                        Log($"VetLoadout PREFIX: ensured COIL in weaponOptions for {definition.name}");
                    }
                }

                if (hasABML)
                {
                    // Ensure ABM-L is in cargo weapon options on the prefab
                    Aircraft prefabAircraft = definition.unitPrefab.GetComponent<Aircraft>();
                    if (prefabAircraft != null && prefabAircraft.weaponManager != null)
                    {
                        foreach (var hs in prefabAircraft.weaponManager.hardpointSets)
                            ABMLTrailer.EnsureInWeaponOptions(hs);
                        Log($"VetLoadout PREFIX: ensured ABM-L in weaponOptions for {definition.name}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogError($"VetLoadout prefix error: {ex.Message}");
            }
        }

        // =====================================================================
        // PATCH: WeaponManager.InitializeWeaponManager PREFIX
        // Ensure COIL is in weaponOptions before the loadout count check.
        // Also re-inject COIL into loadout if it was lost during serialization.
        // =====================================================================

        public static void WeaponManager_InitializeWeaponManager_Prefix(WeaponManager __instance)
        {
            if (_coilWeaponMount == null) return;

            Aircraft aircraft = null;
            try
            {
                var field = Traverse.Create(__instance).Field("aircraft");
                if (field == null || !field.FieldExists()) return;
                aircraft = field.GetValue<Aircraft>();
            }
            catch { return; }

            if (aircraft == null || aircraft.definition == null) return;

            string defName = aircraft.definition.name ?? "";
            string uName = aircraft.unitName ?? "";
            if (!defName.ToLower().Contains("darkreach") && !defName.ToLower().Contains("bomber")
                && !uName.ToLower().Contains("darkreach"))
                return;

            try
            {
                EnsureCOILInWeaponOptions(__instance);
            }
            catch { }
        }

        // =====================================================================
        // PATCH: WeaponManager.SpawnWeapons PREFIX - Diagnostic logging
        // =====================================================================

        public static void WeaponManager_SpawnWeapons_Prefix(WeaponManager __instance)
        {
            if (_coilWeaponMount == null) return;

            Aircraft aircraft = null;
            try
            {
                var field = Traverse.Create(__instance).Field("aircraft");
                if (field == null || !field.FieldExists()) return;
                aircraft = field.GetValue<Aircraft>();
            }
            catch { return; }

            if (aircraft == null || aircraft.loadout == null) return;

            bool hasCOIL = false;
            foreach (var w in aircraft.loadout.weapons)
            {
                if (w != null && w.mountName == "ABM COIL Laser")
                { hasCOIL = true; break; }
            }
            if (hasCOIL)
                Log($"SpawnWeapons PREFIX on {aircraft.name} - COIL detected");
        }

        // =====================================================================
        // PATCH: HardpointSet.BlockedByOtherHardpoint - COIL preclusion
        // When COIL is selected in Inner Bay, block Forward/Rear/Outer bays
        // =====================================================================

        public static void HardpointSet_BlockedByOtherHardpoint_Postfix(
            HardpointSet __instance, Loadout loadout, ref bool __result)
        {
            try
            {
                if (__result || loadout == null || loadout.weapons == null)
                    return;

                WeaponManager wm = FindOwnerWeaponManager(__instance);
                if (wm == null || wm.hardpointSets == null)
                    return;

                int currentIndex = -1;
                int innerBayIndex = -1;

                for (int i = 0; i < wm.hardpointSets.Length; i++)
                {
                    if (wm.hardpointSets[i] == __instance)
                        currentIndex = i;
                    if (wm.hardpointSets[i].name.ToLower().Contains("inner bay"))
                        innerBayIndex = i;
                }

                if (currentIndex < 0 || innerBayIndex < 0)
                    return;

                string currentName = __instance.name.ToLower();
                bool isBlockableBay = currentName.Contains("forward") ||
                                      currentName.Contains("rear") ||
                                      currentName.Contains("outer");

                if (isBlockableBay && innerBayIndex < loadout.weapons.Count)
                {
                    WeaponMount innerMount = loadout.weapons[innerBayIndex];
                    if (innerMount != null && innerMount.mountName == "ABM COIL Laser")
                    {
                        __result = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogError($"BlockedByOtherHardpoint error: {ex.Message}");
            }
        }

        private static WeaponManager FindOwnerWeaponManager(HardpointSet target)
        {
            foreach (var wm in Object.FindObjectsOfType<WeaponManager>())
            {
                if (wm.hardpointSets == null) continue;
                foreach (var hs in wm.hardpointSets)
                {
                    if (hs == target) return wm;
                }
            }
            return null;
        }

        // =====================================================================
        // PATCH: Hardpoint.SpawnMount POSTFIX - Initialize COIL after spawning
        // =====================================================================

        public static void Hardpoint_SpawnMount_Postfix(
            Hardpoint __instance, Aircraft aircraft,
            WeaponMount weaponMount, GameObject __result)
        {
            try
            {
                if (__result == null || weaponMount == null) return;

                // Handle COIL laser
                if (weaponMount.mountName == "ABM COIL Laser")
                {
                    bool isPlayer = aircraft.Player != null;
                    Log($"SpawnMount POSTFIX - COIL on {aircraft.name} (isPlayer={isPlayer})");

                    COILLaser coilLaser = __result.GetComponentInChildren<COILLaser>(true);
                    if (coilLaser != null)
                    {
                        coilLaser.ForceInitialize();
                        Log($"ForceInitialize done - Ammo: {coilLaser.GetAmmoTotal()}/{coilLaser.GetFullAmmo()}");

                        foreach (WeaponStation station in aircraft.weaponStations)
                        {
                            if (station.Weapons.Contains(coilLaser))
                            {
                                station.AccountAmmo();
                                Log($"Station #{station.Number} ammo: {station.Ammo}/{station.FullAmmo}");
                                break;
                            }
                        }
                    }
                    else
                    {
                        LogError("COILLaser component not found on spawned prefab");
                    }
                }

                // Handle ABM-L trailer: tag the MountedCargo so we can detect
                // its deployment in the Fire postfix. Use GetInfo() which returns
                // our preserved WeaponInfo, not the one Initialize() overwrote.
                if (weaponMount.mountName == "HLT ABM-L Trailer")
                {
                    Log($"SpawnMount POSTFIX - ABM-L on {aircraft.name}");
                    var cargos = __result.GetComponentsInChildren<MountedCargo>(true);
                    foreach (var cargo in cargos)
                    {
                        var abmlInfo = ABMLTrailer.GetInfo();
                        if (abmlInfo != null)
                        {
                            cargo.info = abmlInfo;
                            Log($"Tagged MountedCargo with ABM-L info: {abmlInfo.weaponName}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogError($"SpawnMount error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // =====================================================================
        // PATCH: WeaponManager.OrganizeWeaponStations POSTFIX - Diagnostic
        // =====================================================================

        public static void WeaponManager_OrganizeWeaponStations_Postfix(WeaponManager __instance)
        {
            Aircraft aircraft = null;
            try
            {
                var field = Traverse.Create(__instance).Field("aircraft");
                if (field == null || !field.FieldExists()) return;
                aircraft = field.GetValue<Aircraft>();
            }
            catch { return; }

            if (aircraft == null) return;

            bool hasCOIL = false;
            foreach (var station in aircraft.weaponStations)
            {
                if (station.WeaponInfo != null && station.WeaponInfo.weaponName == "YAL-1 COIL Laser")
                    hasCOIL = true;
            }
            if (!hasCOIL) return;

            Log($"=== OrganizeWeaponStations on {aircraft.name} ===");
            Log($"  Total weapon stations: {aircraft.weaponStations.Count}");
        }

        // =====================================================================
        // PATCH: WeaponManager.NextWeaponStation/PreviousWeaponStation
        // =====================================================================

        public static void WeaponManager_NextWeaponStation_Postfix(WeaponManager __instance)
        {
            try
            {
                var field = Traverse.Create(__instance).Field("aircraft");
                if (field == null || !field.FieldExists()) return;
                var aircraft = field.GetValue<Aircraft>();
                if (aircraft == null) return;
                string csName = __instance.currentWeaponStation?.WeaponInfo?.weaponName ?? "NULL";
                Log($"NextWeaponStation: now=#{__instance.currentWeaponStation?.Number} '{csName}'");
            }
            catch { }
        }

        public static void WeaponManager_PreviousWeaponStation_Postfix(WeaponManager __instance)
        {
            try
            {
                var field = Traverse.Create(__instance).Field("aircraft");
                if (field == null || !field.FieldExists()) return;
                var aircraft = field.GetValue<Aircraft>();
                if (aircraft == null) return;
                string csName = __instance.currentWeaponStation?.WeaponInfo?.weaponName ?? "NULL";
                Log($"PreviousWeaponStation: now=#{__instance.currentWeaponStation?.Number} '{csName}'");
            }
            catch { }
        }

        // =====================================================================
        // PATCH: Unit.OnEnable POSTFIX - Detect ABM-L trailer spawns
        // =====================================================================

        public static void Unit_OnEnable_Postfix(Unit __instance)
        {
            try
            {
                ABMLTrailer.OnUnitSpawned(__instance);
            }
            catch (System.Exception ex)
            {
                LogError($"Unit_OnEnable_Postfix error: {ex.Message}");
            }
        }

        // =====================================================================
        // PATCH: MountedCargo.Fire POSTFIX - Detect ABM-L trailer deployment
        // When the cargo fires (deploys), flag the next LaserTrailer1 spawn
        // for COIL stat modification.
        // =====================================================================

        public static void MountedCargo_Fire_Postfix(MountedCargo __instance)
        {
            try
            {
                if (__instance == null || __instance.info == null) return;
                // Check if this MountedCargo is on a hardpoint that was spawned
                // from our ABM-L mount. We tag the mount name during SpawnMount.
                var mount = ABMLTrailer.GetMount();
                if (mount == null) return;

                // Check if this cargo's parent weapon station has ABM-L info
                // or if the cargo was flagged as ABM-L
                if (__instance.gameObject.name.Contains("ABML_Tagged") ||
                    (__instance.info != null && __instance.info.weaponName == "HLT ABM-L Trailer"))
                {
                    ABMLTrailer.OnAbmlDeployed();
                }
            }
            catch (System.Exception ex)
            {
                LogError($"MountedCargo_Fire_Postfix error: {ex.Message}");
            }
        }

        // =====================================================================
        // HELPER: Ensure COIL is in Inner Bay weaponOptions
        // =====================================================================

        private static void EnsureCOILInWeaponOptions(WeaponManager wm)
        {
            if (wm == null || wm.hardpointSets == null || _coilWeaponMount == null) return;

            foreach (var hs in wm.hardpointSets)
            {
                if (hs.name.ToLower().Contains("inner bay"))
                {
                    if (!hs.weaponOptions.Contains(_coilWeaponMount))
                    {
                        hs.weaponOptions.Add(_coilWeaponMount);
                        Log($"Injected COIL into {hs.name} weaponOptions (count: {hs.weaponOptions.Count})");
                    }
                    break;
                }
            }
        }

        // =====================================================================
        // INITIALIZATION
        // =====================================================================

        private static void InitializeCOILLaser()
        {
            // Create WeaponInfo
            _coilWeaponInfo = ScriptableObject.CreateInstance<WeaponInfo>();
            _coilWeaponInfo.name = "ABM_COIL_Laser_info";
            _coilWeaponInfo.weaponName = "YAL-1 COIL Laser";
            _coilWeaponInfo.shortName = "COIL";
            _coilWeaponInfo.description = "Anti-Ballistic Missile Chemical Oxygen Iodine Laser - High-energy directed energy weapon using chemical charges to generate megawatt-class laser beam";
            _coilWeaponInfo.fireInterval = 0f;
            _coilWeaponInfo.energy = false;
            _coilWeaponInfo.gun = false;
            _coilWeaponInfo.bomb = false;
            _coilWeaponInfo.nuclear = false;
            _coilWeaponInfo.strategic = false;
            _coilWeaponInfo.overHorizon = false;
            _coilWeaponInfo.boresight = false;
            _coilWeaponInfo.laserGuided = false;
            _coilWeaponInfo.cargo = false;
            _coilWeaponInfo.troops = false;
            _coilWeaponInfo.sling = false;
            _coilWeaponInfo.muzzleVelocity = 299792458f;
            _coilWeaponInfo.pierceDamage = 0f;
            _coilWeaponInfo.blastDamage = COILModPlugin.DamagePerShot.Value;
            _coilWeaponInfo.armorTierEffectiveness = 10f;
            _coilWeaponInfo.visibilityWhenFired = 100f;
            _coilWeaponInfo.costPerRound = 0f;
            _coilWeaponInfo.massPerRound = 0f;
            _coilWeaponInfo.useWeaponDoors = true;
            _coilWeaponInfo.effectiveness = new RoleIdentity
            {
                antiSurface = 1f,
                antiAir = 1f,
                antiMissile = 1f,
                antiRadar = 1f
            };
            _coilWeaponInfo.pK = 0.95f;
            _coilWeaponInfo.targetRequirements = new TargetRequirements
            {
                maxRange = COILModPlugin.MaxRange.Value,
                minRange = 500f,
                maxSpeed = 10000f,
                maxAltitude = 50000f,
                lineOfSight = true,
                minAlignment = 180f,
                minOwnerSpeed = 0f
            };
            _coilWeaponInfo.weaponIcon = CreateCOILIcon();

            // Create weapon prefab with COILLaser component
            GameObject weaponPrefab = new GameObject("COIL_Laser_Prefab");
            Object.DontDestroyOnLoad(weaponPrefab);

            COILLaser coilLaser = weaponPrefab.AddComponent<COILLaser>();
            coilLaser.info = _coilWeaponInfo;

            // weaponPrefab must be null on WeaponInfo — Initialize() assumes non-null means Missile
            // and calls GetComponent<Missile>() which NREs. The mount.prefab holds the actual GO.
            _coilWeaponInfo.weaponPrefab = null;
            weaponPrefab.SetActive(true);

            // Create WeaponMount (ScriptableObject - survives like game's own mounts)
            _coilWeaponMount = ScriptableObject.CreateInstance<WeaponMount>();
            _coilWeaponMount.name = "ABM_COIL_Laser";
            _coilWeaponMount.mountName = "ABM COIL Laser";
            _coilWeaponMount.jsonKey = "ABM_COIL_Laser";
            _coilWeaponMount.info = _coilWeaponInfo;
            _coilWeaponMount.prefab = weaponPrefab;
            _coilWeaponMount.ammo = COILModPlugin.MaxShots.Value;
            _coilWeaponMount.turret = false;
            _coilWeaponMount.missileBay = false;
            _coilWeaponMount.radar = false;
            _coilWeaponMount.Cargo = false;
            _coilWeaponMount.Troops = false;
            _coilWeaponMount.GearSafety = true;
            _coilWeaponMount.GroundSafety = true;
            _coilWeaponMount.emptyCost = 100f;
            _coilWeaponMount.emptyMass = 5000f;
            _coilWeaponMount.emptyDrag = 0f;
            _coilWeaponMount.emptyRCS = 0f;
            _coilWeaponMount.drag = 0f;
            _coilWeaponMount.RCS = 0f;
            _coilWeaponMount.mass = _coilWeaponMount.emptyMass;

            Log($"Initialized COIL - cost: ${_coilWeaponMount.emptyCost}M, mass: {_coilWeaponMount.emptyMass}kg, shots: {_coilWeaponMount.ammo}");
        }

        // =====================================================================
        // ICON GENERATION
        // =====================================================================

        private static Sprite CreateCOILIcon()
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            Color green = new Color(0f, 1f, 0f, 1f);
            int border = 8, margin = 20;

            for (int x = margin; x < size - margin; x++)
                for (int y = size - margin; y < size - margin + border; y++)
                    pixels[y * size + x] = green;
            for (int x = margin; x < size - margin; x++)
                for (int y = margin; y < margin + border; y++)
                    pixels[y * size + x] = green;
            for (int x = margin; x < margin + border; x++)
                for (int y = margin; y < size - margin; y++)
                    pixels[y * size + x] = green;
            for (int x = size - margin - border; x < size - margin; x++)
                for (int y = margin; y < size - margin; y++)
                    pixels[y * size + x] = green;

            DrawCOILText(pixels, size, green);
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void DrawCOILText(Color[] pixels, int size, Color color)
        {
            int centerX = size / 2, centerY = size / 2;
            int lw = 20, lh = 40, ls = 8, sw = 6;
            int totalW = (lw * 4) + (ls * 3);
            int sx = centerX - totalW / 2, sy = centerY - lh / 2;

            DrawC(pixels, size, sx, sy, lw, lh, sw, color);
            DrawO(pixels, size, sx + lw + ls, sy, lw, lh, sw, color);
            DrawI(pixels, size, sx + (lw + ls) * 2, sy, lw, lh, sw, color);
            DrawL(pixels, size, sx + (lw + ls) * 3, sy, lw, lh, sw, color);
        }

        private static void SetPixel(Color[] p, int s, int x, int y, Color c)
        {
            if (x >= 0 && x < s && y >= 0 && y < s) p[y * s + x] = c;
        }

        private static void FillRect(Color[] p, int s, int x1, int y1, int x2, int y2, Color c)
        {
            for (int x = x1; x < x2; x++)
                for (int y = y1; y < y2; y++)
                    SetPixel(p, s, x, y, c);
        }

        private static void DrawC(Color[] p, int s, int x, int y, int w, int h, int sw, Color c)
        {
            FillRect(p, s, x, y + h - sw, x + w, y + h, c);
            FillRect(p, s, x, y, x + w, y + sw, c);
            FillRect(p, s, x, y, x + sw, y + h, c);
        }

        private static void DrawO(Color[] p, int s, int x, int y, int w, int h, int sw, Color c)
        {
            FillRect(p, s, x, y + h - sw, x + w, y + h, c);
            FillRect(p, s, x, y, x + w, y + sw, c);
            FillRect(p, s, x, y, x + sw, y + h, c);
            FillRect(p, s, x + w - sw, y, x + w, y + h, c);
        }

        private static void DrawI(Color[] p, int s, int x, int y, int w, int h, int sw, Color c)
        {
            int cx = x + w / 2 - sw / 2;
            FillRect(p, s, cx, y, cx + sw, y + h, c);
            FillRect(p, s, x, y + h - sw, x + w, y + h, c);
            FillRect(p, s, x, y, x + w, y + sw, c);
        }

        private static void DrawL(Color[] p, int s, int x, int y, int w, int h, int sw, Color c)
        {
            FillRect(p, s, x, y, x + sw, y + h, c);
            FillRect(p, s, x, y, x + w, y + sw, c);
        }

        // =====================================================================
        // PATCH: Laser.FixedUpdate PREFIX/POSTFIX
        // Reduce Medusa laser damage against surface targets (ground, ships,
        // buildings). Temporarily scales blastDamage/fireDamage fields before
        // FixedUpdate runs, restores them after.
        // =====================================================================

        private static float _savedBlastDamage;
        private static float _savedFireDamage;
        private static bool _laserDamageModified;

        public static void Laser_FixedUpdate_Prefix(Laser __instance)
        {
            _laserDamageModified = false;

            try
            {
                // Only modify if the laser's current target is a surface unit
                var traverse = Traverse.Create(__instance);
                Unit currentTarget = traverse.Field("currentTarget").GetValue<Unit>();
                if (currentTarget == null) return;
                if (currentTarget is Aircraft || currentTarget is Missile) return;

                // It's a surface target — scale damage down
                float scale = Mathf.Clamp01(COILModPlugin.MedusaLaserGroundDamagePercent.Value / 100f);

                _savedBlastDamage = traverse.Field("blastDamage").GetValue<float>();
                _savedFireDamage = traverse.Field("fireDamage").GetValue<float>();

                traverse.Field("blastDamage").SetValue(_savedBlastDamage * scale);
                traverse.Field("fireDamage").SetValue(_savedFireDamage * scale);
                _laserDamageModified = true;
            }
            catch { }
        }

        public static void Laser_FixedUpdate_Postfix(Laser __instance)
        {
            if (!_laserDamageModified) return;

            try
            {
                var traverse = Traverse.Create(__instance);
                traverse.Field("blastDamage").SetValue(_savedBlastDamage);
                traverse.Field("fireDamage").SetValue(_savedFireDamage);
                _laserDamageModified = false;
            }
            catch { }
        }

        // =====================================================================
        // PATCH: NewUnitPanel.Awake PREFIX
        // Clear the static unitProviders cache so the mission editor rebuilds
        // its unit list from Encyclopedia, picking up our ABM-L vehicle.
        // =====================================================================

        public static void NewUnitPanel_Awake_Prefix(object __instance)
        {
            try
            {
                Log(">>> NewUnitPanel.Awake PREFIX firing <<<");

                // Ensure ABM-L vehicle definition exists before the editor caches the list.
                // This handles the case where QoL wasn't ready during AfterLoad but is now.
                if (COILModPlugin.EnableABMLTrailer.Value)
                {
                    ABMLTrailer.TryCreateVehicleDefinition();
                    var vehDef = ABMLTrailer.GetVehicleDefinition();
                    Log($"ABM-L VehicleDefinition: {(vehDef != null ? vehDef.unitName : "NULL")}");

                    // Verify it's in Encyclopedia.i.vehicles
                    if (vehDef != null && Encyclopedia.i != null)
                    {
                        bool inList = Encyclopedia.i.vehicles.Contains(vehDef);
                        Log($"ABM-L in Encyclopedia.i.vehicles: {inList} (total vehicles: {Encyclopedia.i.vehicles.Count})");
                        if (!inList)
                        {
                            Encyclopedia.i.vehicles.Add(vehDef);
                            Log("Force-added ABM-L to Encyclopedia.i.vehicles");
                        }
                    }
                }

                var field = AccessTools.Field(__instance.GetType(), "unitProviders");
                if (field == null)
                {
                    LogError("unitProviders field not found on NewUnitPanel!");
                    return;
                }
                var dict = field.GetValue(null) as System.Collections.IDictionary;
                if (dict != null)
                {
                    Log($"unitProviders has {dict.Count} entries — clearing to force rebuild");
                    dict.Clear();
                }
                else
                {
                    Log("unitProviders dict is null — will be built fresh by Awake");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"NewUnitPanel_Awake_Prefix error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // =====================================================================
        // PATCH: Spawner.SpawnFromUnitDefinitionInEditor POSTFIX
        // Detect when the mission editor spawns our ABM-L VehicleDefinition
        // and flag the spawned LaserTrailer1 for COIL stat application.
        // =====================================================================

        public static void Spawner_SpawnFromUnitDefinitionInEditor_Postfix(
            UnitDefinition placingDefinition, Unit __result)
        {
            try
            {
                if (placingDefinition == null) return;
                var abmlDef = ABMLTrailer.GetVehicleDefinition();
                if (abmlDef == null || placingDefinition != abmlDef) return;

                // Flag this spawn for COIL stats
                ABMLTrailer.OnEditorSpawnDetected();

                // Also try to apply directly if the unit is already enabled
                if (__result != null)
                    ABMLTrailer.OnUnitSpawned(__result);
            }
            catch (System.Exception ex)
            {
                LogError($"Spawner_SpawnFromUnitDefinitionInEditor_Postfix error: {ex.Message}");
            }
        }
    }
}
