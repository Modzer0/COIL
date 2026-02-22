using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NuclearOptionCOILMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class COILModPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<float> MaxRange;
        public static ConfigEntry<int> MaxShots;
        public static ConfigEntry<float> DamagePerShot;
        public static ConfigEntry<float> FiringArc;
        public static ConfigEntry<float> PowerConsumption;
        public static ConfigEntry<bool> EnableCOIL;
        public static ConfigEntry<bool> EnableABMLTrailer;
        public static ConfigEntry<bool> AutoFireDefault;
        public static ConfigEntry<bool> AutoFireAllowed;
        public static ConfigEntry<string> AutoFireToggleKey;
        public static ConfigEntry<float> AutoFireSwitchDelay;
        public static ConfigEntry<float> GroundDamagePercent;
        public static ConfigEntry<float> MedusaLaserGroundDamagePercent;
        public static ConfigEntry<bool> ABMLUseAmmo;
        public static ConfigEntry<float> ABMLFireSeconds;

        private Harmony _harmony;

        private void Awake()
        {
            MaxRange = Config.Bind("COIL Laser", "MaxRange", 111120f, "Maximum effective range in meters (~60 nautical miles)");
            MaxShots = Config.Bind("COIL Laser", "MaxShots", 120, "Ammo capacity (drains 1 per second while firing)");
            DamagePerShot = Config.Bind("COIL Laser", "DamagePerShot", 400f, "Damage dealt per second while firing");
            FiringArc = Config.Bind("COIL Laser", "FiringArc", 180f, "Max angle from aircraft nose (180 = forward hemisphere)");
            PowerConsumption = Config.Bind("COIL Laser", "PowerConsumption", 0f, "Power consumption per second (0 for chemical laser)");
            EnableCOIL = Config.Bind("COIL Laser", "EnableCOIL", true, "Enable or disable the COIL laser mod");
            EnableABMLTrailer = Config.Bind("ABM-L Trailer", "EnableABMLTrailer", false, "Enable ABM-L ground laser trailer (requires QoL mod to be installed)");
            AutoFireDefault = Config.Bind("COIL Laser", "AutoFireDefault", false, "Start with auto-fire enabled (auto-engages missiles/bombs)");
            AutoFireAllowed = Config.Bind("COIL Laser", "AutoFireAllowed", false, "Allow COIL auto-fire capability. When false, auto-fire is completely disabled and the toggle key does nothing.");
            AutoFireToggleKey = Config.Bind("COIL Laser", "AutoFireToggleKey", "B", "Key to toggle COIL auto-fire on/off when COIL station is selected");
            AutoFireSwitchDelay = Config.Bind("COIL Laser", "AutoFireSwitchDelay", 1f, "Seconds to wait before switching to a new auto-fire target (balance tuning)");
            GroundDamagePercent = Config.Bind("COIL Laser", "GroundDamagePercent", 2.5f, "Damage percentage against surface targets — ground vehicles, ships, and buildings (0-100). Full damage applies to aircraft and missiles only.");
            MedusaLaserGroundDamagePercent = Config.Bind("Medusa Laser", "MedusaLaserGroundDamagePercent", 0f, "Damage percentage for the Medusa's laser against surface targets — ground vehicles, ships, and buildings (0-100)");
            ABMLUseAmmo = Config.Bind("ABM-L Trailer", "ABMLUseAmmo", true, "When enabled, the ABM-L trailer has limited fire time and must be resupplied by ammo trucks or containers");
            ABMLFireSeconds = Config.Bind("ABM-L Trailer", "ABMLFireSeconds", 120f, "Total seconds of laser fire time before the ABM-L trailer needs resupply");

            if (!EnableCOIL.Value) { Logger.LogInfo("Plugin " + PluginInfo.PLUGIN_NAME + " disabled"); return; }

            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            COILLaserPatch.SetLogger(Logger);
            COILLaser.SetLogger(Logger);
            ABMLTrailer.SetLogger(Logger);
            ABMLTargetOverride.SetLogger(Logger);

            try
            {
                PatchIt(typeof(Encyclopedia), "AfterLoad", System.Type.EmptyTypes, "Encyclopedia_AfterLoad_Prefix", true);
                PatchIt(typeof(Encyclopedia), "AfterLoad", System.Type.EmptyTypes, "Encyclopedia_AfterLoad_Postfix", false);
                PatchIt(typeof(WeaponSelector), "PopulateOptions", new[] { typeof(Airbase), typeof(FactionHQ), typeof(HardpointSet), typeof(bool) }, "WeaponSelector_PopulateOptions_Prefix", true);
                PatchIt(typeof(WeaponChecker), "VetLoadout", null, "WeaponChecker_VetLoadout_Prefix", true);
                PatchIt(typeof(WeaponManager), "InitializeWeaponManager", System.Type.EmptyTypes, "WeaponManager_InitializeWeaponManager_Prefix", true);
                PatchIt(typeof(WeaponManager), "SpawnWeapons", System.Type.EmptyTypes, "WeaponManager_SpawnWeapons_Prefix", true);
                PatchIt(typeof(HardpointSet), "BlockedByOtherHardpoint", null, "HardpointSet_BlockedByOtherHardpoint_Postfix", false);
                PatchIt(typeof(Hardpoint), "SpawnMount", null, "Hardpoint_SpawnMount_Postfix", false);
                PatchIt(typeof(WeaponManager), "OrganizeWeaponStations", System.Type.EmptyTypes, "WeaponManager_OrganizeWeaponStations_Postfix", false);
                PatchIt(typeof(WeaponManager), "NextWeaponStation", null, "WeaponManager_NextWeaponStation_Postfix", false);
                PatchIt(typeof(WeaponManager), "PreviousWeaponStation", null, "WeaponManager_PreviousWeaponStation_Postfix", false);
                PatchIt(typeof(Unit), "OnEnable", System.Type.EmptyTypes, "Unit_OnEnable_Postfix", false);
                PatchIt(typeof(MountedCargo), "Fire", null, "MountedCargo_Fire_Postfix", false);
                PatchIt(typeof(Laser), "FixedUpdate", System.Type.EmptyTypes, "Laser_FixedUpdate_Prefix", true);
                PatchIt(typeof(Laser), "FixedUpdate", System.Type.EmptyTypes, "Laser_FixedUpdate_Postfix", false);

                // Mission editor patches — use AccessTools to find types by name
                // since NewUnitPanel is in a nested namespace
                var newUnitPanelType = AccessTools.TypeByName("NuclearOption.MissionEditorScripts.Buttons.NewUnitPanel");
                if (newUnitPanelType != null)
                {
                    PatchIt(newUnitPanelType, "Awake", System.Type.EmptyTypes, "NewUnitPanel_Awake_Prefix", true);
                    Logger.LogInfo("NewUnitPanel.Awake patch registered successfully");
                }
                else
                {
                    Logger.LogError("FAILED to find NewUnitPanel type — mission editor integration will not work!");
                    Logger.LogError("Tried: NuclearOption.MissionEditorScripts.Buttons.NewUnitPanel");
                    // Dump all types containing "NewUnit" or "UnitPanel" for debugging
                    foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            foreach (var t in asm.GetTypes())
                            {
                                if (t.Name.Contains("NewUnit") || t.Name.Contains("UnitPanel"))
                                    Logger.LogInfo($"  Found type: {t.FullName} in {asm.GetName().Name}");
                            }
                        }
                        catch { }
                    }
                }

                var spawnerType = typeof(Spawner);
                var spawnEditorMethod = AccessTools.Method(spawnerType, "SpawnFromUnitDefinitionInEditor");
                if (spawnEditorMethod != null)
                {
                    var patch = AccessTools.Method(typeof(COILLaserPatch), "Spawner_SpawnFromUnitDefinitionInEditor_Postfix");
                    _harmony.Patch(spawnEditorMethod, postfix: new HarmonyMethod(patch));
                    Logger.LogInfo("Spawner.SpawnFromUnitDefinitionInEditor postfix registered");
                }
                else
                {
                    Logger.LogError("FAILED to find Spawner.SpawnFromUnitDefinitionInEditor method");
                }
            }
            catch (System.Exception ex) { Logger.LogError("Harmony patches failed: " + ex.Message + "\n" + ex.StackTrace); }

            Logger.LogInfo("Plugin " + PluginInfo.PLUGIN_NAME + " v" + PluginInfo.PLUGIN_VERSION + " loaded!");
            var patches = _harmony.GetPatchedMethods().ToList();
            Logger.LogInfo("Harmony applied " + patches.Count + " patches");
            foreach (var method in patches)
                Logger.LogInfo("  Patched: " + method.DeclaringType?.Name + "." + method.Name);
        }

        private void PatchIt(System.Type targetType, string targetMethod, System.Type[] paramTypes, string patchMethod, bool isPrefix)
        {
            var target = paramTypes != null ? AccessTools.Method(targetType, targetMethod, paramTypes) : AccessTools.Method(targetType, targetMethod);
            var patch = AccessTools.Method(typeof(COILLaserPatch), patchMethod);
            if (target == null) { Logger.LogError("Target not found: " + targetType.Name + "." + targetMethod); return; }
            if (patch == null) { Logger.LogError("Patch not found: " + patchMethod); return; }
            if (isPrefix) _harmony.Patch(target, prefix: new HarmonyMethod(patch));
            else _harmony.Patch(target, postfix: new HarmonyMethod(patch));
            Logger.LogInfo("Patched " + targetType.Name + "." + targetMethod + " (" + (isPrefix ? "prefix" : "postfix") + ")");
        }

        private void OnDestroy() { _harmony?.UnpatchSelf(); }
    }
}