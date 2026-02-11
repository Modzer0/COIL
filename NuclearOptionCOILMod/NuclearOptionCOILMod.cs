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
        // Configuration entries
        public static ConfigEntry<float> MaxRange;
        public static ConfigEntry<int> MaxShots;
        public static ConfigEntry<float> ShotDuration;
        public static ConfigEntry<float> DamagePerShot;
        public static ConfigEntry<float> FiringArc;
        public static ConfigEntry<float> PowerConsumption;
        public static ConfigEntry<bool> EnableCOIL;

        private Harmony _harmony;

        private void Awake()
        {
            // Configuration setup
            MaxRange = Config.Bind("COIL Laser", "MaxRange", 52202f,
                "Maximum effective range of the COIL laser in meters (default: ~52km, calculated for 50km horizontal + 15km altitude)");
            
            MaxShots = Config.Bind("COIL Laser", "MaxShots", 30,
                "Number of shots before depleted");
            
            ShotDuration = Config.Bind("COIL Laser", "ShotDuration", 5f,
                "Duration of each shot in seconds (default: 5 seconds)");
            
            DamagePerShot = Config.Bind("COIL Laser", "DamagePerShot", 2000f,
                "Total damage dealt during one complete shot (default: 2000)");
            
            FiringArc = Config.Bind("COIL Laser", "FiringArc", 180f,
                "Maximum angle from aircraft nose for firing (default: 180 degrees = forward hemisphere)");
            
            PowerConsumption = Config.Bind("COIL Laser", "PowerConsumption", 0f,
                "Power consumption per second (0 for chemical laser - no electrical power needed)");
            
            EnableCOIL = Config.Bind("COIL Laser", "EnableCOIL", true,
                "Enable or disable the COIL laser mod");

            if (EnableCOIL.Value)
            {
                _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                _harmony.PatchAll(typeof(COILModPlugin).Assembly);
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION} loaded!");
                Logger.LogInfo($"COIL Laser Configuration:");
                Logger.LogInfo($"  Max Range: {MaxRange.Value}m");
                Logger.LogInfo($"  Max Shots: {MaxShots.Value}");
                Logger.LogInfo($"  Shot Duration: {ShotDuration.Value}s");
                Logger.LogInfo($"  Damage/Shot: {DamagePerShot.Value}");
                Logger.LogInfo($"  Firing Arc: {FiringArc.Value}°");
                Logger.LogInfo($"  DPS: {DamagePerShot.Value / ShotDuration.Value}");
                
                // Log Harmony patch status
                var patches = _harmony.GetPatchedMethods().ToList();
                Logger.LogInfo($"Harmony applied {patches.Count} patches");
                foreach (var method in patches)
                {
                    Logger.LogInfo($"  Patched: {method.DeclaringType?.Name}.{method.Name}");
                }
            }
            else
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is disabled in config");
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
