using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace NuclearOptionCOILMod
{
    /// <summary>
    /// Harmony patch to add COIL laser weapon to Darkreach bomber
    /// </summary>
    [HarmonyPatch]
    public class COILLaserPatch
    {
        private static WeaponInfo _coilWeaponInfo;
        private static WeaponMount _coilWeaponMount;

        /// <summary>
        /// Patch WeaponManager.SpawnWeapons to inject COIL laser
        /// </summary>
        [HarmonyPatch(typeof(WeaponManager), "SpawnWeapons")]
        [HarmonyPostfix]
        public static void SpawnWeapons_Postfix(WeaponManager __instance)
        {
            try
            {
                // Check if this is a Darkreach bomber
                Aircraft aircraft = __instance.GetComponent<Aircraft>();
                if (aircraft == null || aircraft.definition == null)
                    return;

                // Check aircraft name for Darkreach
                string aircraftName = aircraft.definition.unitName;
                if (!aircraftName.Contains("Darkreach") && !aircraftName.Contains("darkreach"))
                    return;

                // Initialize COIL laser if not already done
                if (_coilWeaponInfo == null)
                {
                    InitializeCOILLaser();
                }

                // Add COIL laser to internal weapon bay
                AddCOILToAircraft(__instance, aircraft);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[COIL Mod] Error in SpawnWeapons_Postfix: {ex.Message}");
            }
        }

        private static void InitializeCOILLaser()
        {
            // Create WeaponInfo ScriptableObject
            _coilWeaponInfo = ScriptableObject.CreateInstance<WeaponInfo>();
            _coilWeaponInfo.weaponName = "YAL-1 COIL Laser";
            _coilWeaponInfo.shortName = "COIL";
            _coilWeaponInfo.description = "Megawatt-class Chemical Oxygen Iodine Laser - Continuous beam weapon";
            _coilWeaponInfo.fireInterval = 0f; // Continuous fire
            _coilWeaponInfo.energy = true;
            _coilWeaponInfo.gun = false;
            _coilWeaponInfo.bomb = false;
            _coilWeaponInfo.nuclear = false;
            _coilWeaponInfo.strategic = false;
            _coilWeaponInfo.overHorizon = false;
            _coilWeaponInfo.boresight = true;
            _coilWeaponInfo.laserGuided = false;
            _coilWeaponInfo.muzzleVelocity = 299792458f; // Speed of light
            _coilWeaponInfo.pierceDamage = 0f;
            _coilWeaponInfo.blastDamage = COILModPlugin.DamagePerShot.Value / COILModPlugin.ShotDuration.Value;
            _coilWeaponInfo.armorTierEffectiveness = 10f; // Highly effective against all armor
            _coilWeaponInfo.visibilityWhenFired = 100f; // Very visible when firing
            _coilWeaponInfo.costPerRound = 50000f; // Expensive to operate
            _coilWeaponInfo.massPerRound = 0f; // Energy weapon, no mass per shot
            _coilWeaponInfo.useWeaponDoors = true;
            _coilWeaponInfo.effectiveness = new RoleIdentity 
            { 
                antiSurface = 1f, 
                antiAir = 1f, 
                antiMissile = 1f, 
                antiRadar = 1f 
            }; // Effective against everything
            _coilWeaponInfo.pK = 0.95f; // High probability of kill
            
            // Create weapon icon
            _coilWeaponInfo.weaponIcon = CreateCOILIcon();

            Debug.Log($"[COIL Mod] Initialized COIL Laser WeaponInfo");
        }

        private static Sprite CreateCOILIcon()
        {
            // Create a 256x256 texture for the weapon icon
            int size = 256;
            Texture2D iconTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            // Fill with transparent background
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.clear;
            
            // Draw green perimeter box
            Color greenColor = new Color(0f, 1f, 0f, 1f); // Bright green
            int borderWidth = 8;
            int margin = 20;
            
            // Top border
            for (int x = margin; x < size - margin; x++)
                for (int y = size - margin; y < size - margin + borderWidth; y++)
                    pixels[y * size + x] = greenColor;
            
            // Bottom border
            for (int x = margin; x < size - margin; x++)
                for (int y = margin; y < margin + borderWidth; y++)
                    pixels[y * size + x] = greenColor;
            
            // Left border
            for (int x = margin; x < margin + borderWidth; x++)
                for (int y = margin; y < size - margin; y++)
                    pixels[y * size + x] = greenColor;
            
            // Right border
            for (int x = size - margin - borderWidth; x < size - margin; x++)
                for (int y = margin; y < size - margin; y++)
                    pixels[y * size + x] = greenColor;
            
            // Draw "COIL" text in center (simple pixel font)
            DrawCOILText(pixels, size, greenColor);
            
            iconTexture.SetPixels(pixels);
            iconTexture.Apply();
            
            // Create sprite from texture
            Sprite icon = Sprite.Create(
                iconTexture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f
            );
            
            Debug.Log("[COIL Mod] Created weapon icon");
            return icon;
        }

        private static void DrawCOILText(Color[] pixels, int size, Color color)
        {
            // Simple block letters for "COIL" centered in the icon
            int centerX = size / 2;
            int centerY = size / 2;
            int letterWidth = 20;
            int letterHeight = 40;
            int letterSpacing = 8;
            int strokeWidth = 6;
            
            // Calculate starting X position to center the text
            int totalWidth = (letterWidth * 4) + (letterSpacing * 3);
            int startX = centerX - totalWidth / 2;
            int startY = centerY - letterHeight / 2;
            
            // Draw 'C'
            DrawC(pixels, size, startX, startY, letterWidth, letterHeight, strokeWidth, color);
            
            // Draw 'O'
            DrawO(pixels, size, startX + letterWidth + letterSpacing, startY, letterWidth, letterHeight, strokeWidth, color);
            
            // Draw 'I'
            DrawI(pixels, size, startX + (letterWidth + letterSpacing) * 2, startY, letterWidth, letterHeight, strokeWidth, color);
            
            // Draw 'L'
            DrawL(pixels, size, startX + (letterWidth + letterSpacing) * 3, startY, letterWidth, letterHeight, strokeWidth, color);
        }

        private static void DrawC(Color[] pixels, int size, int x, int y, int w, int h, int stroke, Color color)
        {
            // Top horizontal
            for (int i = x; i < x + w; i++)
                for (int j = y + h - stroke; j < y + h; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
            
            // Bottom horizontal
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + stroke; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
            
            // Left vertical
            for (int i = x; i < x + stroke; i++)
                for (int j = y; j < y + h; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
        }

        private static void DrawO(Color[] pixels, int size, int x, int y, int w, int h, int stroke, Color color)
        {
            // Top horizontal
            for (int i = x; i < x + w; i++)
                for (int j = y + h - stroke; j < y + h; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
            
            // Bottom horizontal
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + stroke; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
            
            // Left vertical
            for (int i = x; i < x + stroke; i++)
                for (int j = y; j < y + h; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
            
            // Right vertical
            for (int i = x + w - stroke; i < x + w; i++)
                for (int j = y; j < y + h; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
        }

        private static void DrawI(Color[] pixels, int size, int x, int y, int w, int h, int stroke, Color color)
        {
            // Center vertical line
            int centerX = x + w / 2 - stroke / 2;
            for (int i = centerX; i < centerX + stroke; i++)
                for (int j = y; j < y + h; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
            
            // Top horizontal
            for (int i = x; i < x + w; i++)
                for (int j = y + h - stroke; j < y + h; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
            
            // Bottom horizontal
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + stroke; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
        }

        private static void DrawL(Color[] pixels, int size, int x, int y, int w, int h, int stroke, Color color)
        {
            // Left vertical
            for (int i = x; i < x + stroke; i++)
                for (int j = y; j < y + h; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
            
            // Bottom horizontal
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + stroke; j++)
                    if (i >= 0 && i < size && j >= 0 && j < size)
                        pixels[j * size + i] = color;
        }

        private static void AddCOILToAircraft(WeaponManager weaponManager, Aircraft aircraft)
        {
            // Find internal weapon bay hardpoints
            HardpointSet[] hardpointSets = weaponManager.hardpointSets;
            if (hardpointSets == null || hardpointSets.Length == 0)
            {
                Debug.LogWarning("[COIL Mod] No hardpoint sets found on Darkreach");
                return;
            }

            // Look for internal bay hardpoints (typically the first few)
            foreach (HardpointSet hardpointSet in hardpointSets)
            {
                if (hardpointSet.hardpoints == null || hardpointSet.hardpoints.Count == 0)
                    continue;

                // Check if this is an internal bay (has bay doors)
                bool hasInternalBay = false;
                foreach (Hardpoint hardpoint in hardpointSet.hardpoints)
                {
                    if (hardpoint.bayDoors != null && hardpoint.bayDoors.Length > 0)
                    {
                        hasInternalBay = true;
                        break;
                    }
                }

                if (hasInternalBay)
                {
                    // Create COIL laser weapon for this hardpoint
                    CreateCOILLaserForHardpoint(hardpointSet, aircraft, weaponManager);
                    Debug.Log($"[COIL Mod] Added COIL laser to Darkreach internal bay: {hardpointSet.name}");
                    break; // Only add to one bay
                }
            }
        }

        private static void CreateCOILLaserForHardpoint(HardpointSet hardpointSet, Aircraft aircraft, WeaponManager weaponManager)
        {
            foreach (Hardpoint hardpoint in hardpointSet.hardpoints)
            {
                // Create COIL laser GameObject
                GameObject laserObj = new GameObject("COIL_Laser");
                laserObj.transform.SetParent(hardpoint.transform);
                laserObj.transform.localPosition = Vector3.zero;
                laserObj.transform.localRotation = Quaternion.identity;

                // Add custom COIL laser component
                COILLaser coilLaser = laserObj.AddComponent<COILLaser>();
                coilLaser.info = _coilWeaponInfo;
                coilLaser.ammo = COILModPlugin.MaxShots.Value;
                coilLaser.AttachToUnit(aircraft);

                // Register with weapon manager
                weaponManager.RegisterWeapon(coilLaser, CreateCOILWeaponMount(), hardpoint);

                Debug.Log($"[COIL Mod] Created COIL laser weapon on hardpoint");
            }
        }

        private static WeaponMount CreateCOILWeaponMount()
        {
            if (_coilWeaponMount != null)
                return _coilWeaponMount;

            _coilWeaponMount = ScriptableObject.CreateInstance<WeaponMount>();
            _coilWeaponMount.mountName = "COIL Laser System";
            _coilWeaponMount.info = _coilWeaponInfo;
            _coilWeaponMount.ammo = COILModPlugin.MaxShots.Value;
            _coilWeaponMount.turret = false;
            _coilWeaponMount.missileBay = false;
            _coilWeaponMount.radar = false;
            _coilWeaponMount.Cargo = false;
            _coilWeaponMount.Troops = false;
            _coilWeaponMount.GearSafety = true;
            _coilWeaponMount.GroundSafety = true;
            _coilWeaponMount.emptyCost = 5000000f; // 5 million - expensive system
            _coilWeaponMount.emptyMass = 15000f; // 15 tons (realistic for YAL-1 system)
            _coilWeaponMount.emptyDrag = 0f; // Internal mount
            _coilWeaponMount.emptyRCS = 0f; // Internal mount

            return _coilWeaponMount;
        }
    }
}
