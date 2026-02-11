# Preclusion Logic and Graphics Fix Summary

## Changes Made

### 1. Fixed Preclusion Logic (Dynamic Instead of Permanent)

**Problem**: The previous implementation permanently modified the HardpointSet preclusion lists, which meant other bays were always blocked even when COIL wasn't selected.

**Solution**: Implemented dynamic preclusion using a postfix patch on `HardpointSet.BlockedByOtherHardpoint`.

**How it works**:
- When COIL laser is selected in the Inner Bay → Forward, Rear, and Outer bays are blocked
- When Forward, Rear, or Outer bays have weapons → Inner Bay is blocked
- When COIL is NOT selected → Normal behavior, all bays work independently

**Implementation**:
- Added `HardpointSet_BlockedByOtherHardpoint_Postfix` method in `COILLaserWeapon.cs`
- Checks the current loadout at runtime to determine if COIL is selected
- Only applies blocking when COIL is actually equipped
- Removed the permanent preclusion list modification code

### 2. Fixed Graphics Glitch

**Problem**: Visual artifacts or rendering issues when the weapon was equipped.

**Potential causes addressed**:
- LineRenderer using wrong shader
- Visual components not properly disabled on initialization
- GameObjects active when they should be inactive

**Solutions implemented**:
- Changed shader from "Sprites/Default" to "Unlit/Color" with fallback to "Standard"
- Set all visual GameObjects (beam, light, particles) to inactive on creation
- Added `useWorldSpace = true` to LineRenderer for proper positioning
- Modified `EnableBeam()` to properly activate/deactivate GameObjects, not just components
- Ensured all visual elements start in disabled state

**Files modified**:
- `NuclearOptionCOILMod/COILLaser.cs` - `InitializeVisuals()` and `EnableBeam()` methods

### 3. Removed "Press Any Key" from Build Script

**Change**: Removed all `pause` commands from `build.cmd` so the script completes without user interaction.

**Files modified**:
- `build.cmd`

## Testing Instructions

### 1. Test Preclusion Logic

**Scenario A: COIL Selected**
1. Select Darkreach bomber
2. Select "ABM COIL Laser" in Inner Bay
3. Verify Forward Bay, Rear Bay, and Outer Bay are disabled/grayed out
4. Try to select weapons in those bays - should not be possible

**Scenario B: Other Weapons Selected**
1. Select Darkreach bomber
2. Select a weapon in Forward Bay (e.g., missiles)
3. Verify Inner Bay is disabled/grayed out
4. Cannot select COIL laser while other bays have weapons

**Scenario C: Normal Operation**
1. Select Darkreach bomber
2. Leave all bays empty or select non-COIL weapons
3. Verify all bays work normally
4. Can select weapons in any bay independently

### 2. Test Graphics

**Check for glitches**:
1. Spawn Darkreach with COIL laser equipped
2. Look at the aircraft from different angles
3. Check for:
   - Visible lines or beams when weapon is not firing
   - Strange lighting effects
   - Particle systems running when they shouldn't
   - Any visual artifacts around the aircraft

**Expected behavior**:
- No visible beam, light, or particles when weapon is not firing
- Clean aircraft appearance with no visual glitches
- Beam only appears when actually firing the weapon

### 3. Test Weapon Functionality

1. Spawn with COIL laser
2. Try to switch to the weapon (cycle weapons key)
3. Check if weapon appears in HUD
4. Check BepInEx console for debug messages

## Debug Messages to Look For

When selecting COIL in loadout menu:
```
[COIL Mod] PopulateOptions called for hardpoint: 'Inner Bay'
[COIL Mod] Inner bay detected!
[COIL Mod] Added ABM COIL Laser to Inner Bay
```

When other bays are blocked:
```
[COIL Mod] Blocking [Bay Name] because COIL is selected in inner bay
```

When inner bay is blocked:
```
[COIL Mod] Blocking inner bay because [Bay Name] has a weapon
```

When spawning aircraft:
```
[COIL Mod] SpawnMount postfix - COIL weapon detected!
[COIL Mod] Found COILLaser component on COIL_Laser_Prefab
[COIL Mod] COIL laser found in weapon station #X
```

## Known Issues

- **Weapon switching**: The weapon appears in the loadout but may still not be switchable in-flight. This is being investigated.
- **Graphics performance**: The beam renderer and lighting may impact performance when firing. Monitor FPS during use.

## Configuration

Current default settings (in `BepInEx/config/com.nuclearoption.coil.cfg`):
- Max Range: 100,000m
- Max Shots: 50
- Shot Duration: 5s per shot
- Damage Per Shot: 2000
- Firing Arc: 360°
- Power Consumption: 0
- Cost: $2M per shot ($100M total)
- Weight: 5 tons

## Next Steps

After testing, report:
1. Does the preclusion logic work correctly? (bays blocked only when COIL selected)
2. Are there any graphics glitches remaining?
3. Can you switch to the weapon in-flight?
4. Any error messages in the console?
5. Performance impact when firing?
