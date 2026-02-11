# COIL Weapon Switching Debug Summary

## Changes Made

### 1. Added Hardpoint.SpawnMount Postfix Patch
Added a postfix patch to `Hardpoint.SpawnMount` to debug and ensure the COIL laser weapon is properly registered when spawned.

**Location**: `NuclearOptionCOILMod/COILLaserWeapon.cs` - `Hardpoint_SpawnMount_Postfix` method

**What it does**:
- Detects when the COIL weapon mount is spawned
- Finds the `COILLaser` component on the spawned prefab
- Verifies the weapon is attached to the aircraft unit
- Checks if the weapon is registered in the aircraft's weapon stations
- Manually registers the weapon if it's not found in any station
- Logs detailed debug information about the spawning process

### 2. Improved Error Logging
Added better error logging to the main plugin to identify if patches fail to apply.

**Location**: `NuclearOptionCOILMod/NuclearOptionCOILMod.cs` - `Awake` method

## Testing Instructions

### 1. Launch the Game
Start Nuclear Option with BepInEx console enabled.

### 2. Check the Log
Look for these messages in the BepInEx console:
```
[Info   :Nuclear Option COIL Laser] Successfully patched WeaponSelector.PopulateOptions
[Info   :Nuclear Option COIL Laser] Successfully patched Hardpoint.SpawnMount
```

If you see error messages instead, note them for debugging.

### 3. Select Darkreach with COIL Laser
1. Go to the aircraft selection menu
2. Select the Darkreach bomber
3. In the loadout screen, select "ABM COIL Laser" from the Inner Bay dropdown
4. Verify that Forward Bay, Rear Bay, and Outer Bay are locked to "Empty"
5. Spawn the aircraft

### 4. Check for Debug Messages
When the aircraft spawns, look for these debug messages in the console:
```
[COIL Mod] SpawnMount postfix - COIL weapon detected!
[COIL Mod] Spawned prefab: COIL_Laser_Prefab, active: True
[COIL Mod] Found COILLaser component on COIL_Laser_Prefab
[COIL Mod] COIL laser found in weapon station #X
```

### 5. Try to Switch to the Weapon
Once in the aircraft:
1. Press the weapon cycle key (default: `,` or `.`)
2. Check if you can select the COIL laser weapon
3. Look for the weapon name in the HUD

### 6. If Weapon Still Not Switchable
Check the console for these potential issues:

**Issue 1: COILLaser component not found**
```
[COIL Mod] COILLaser component not found on spawned prefab!
```
This means the prefab structure is wrong.

**Issue 2: Weapon not in any station**
```
[COIL Mod] COIL laser not found in any weapon station! Manually registering...
[COIL Mod] Manually registered COIL laser with weapon manager
```
This means the weapon wasn't automatically registered and we tried to fix it manually.

**Issue 3: No debug messages at all**
If you don't see any "[COIL Mod]" messages when spawning, the SpawnMount patch isn't being called. This could mean:
- The weapon mount isn't being spawned
- The QoL mod's patch is preventing ours from running
- There's a conflict with another mod

## Current Configuration

The COIL laser is configured with these default values (can be changed in `BepInEx/config/com.nuclearoption.coil.cfg`):

- **Max Range**: 100,000m (100km)
- **Max Shots**: 50
- **Shot Duration**: 5 seconds per shot
- **Damage Per Shot**: 2000
- **Firing Arc**: 360° (full sphere)
- **Power Consumption**: 0 (chemical laser)
- **Cost**: $2 million per shot ($100 million total)
- **Weight**: 5 tons (allows 100% fuel load)

## Next Steps

After testing, report back with:
1. Whether the weapon appears in the selection menu ✓ (Already working)
2. Whether you can switch to the weapon in-flight (Current issue)
3. Any error messages or warnings in the console
4. Screenshots of the console output when spawning the aircraft

## Known Issues

- **Weapon appears but can't switch to it**: This is the current issue we're debugging. The weapon is being added to the loadout menu but may not be properly registered with the weapon manager for in-flight use.

## Compatibility

- **QoL Mod**: Compatible - no conflicts detected
- **Other Mods**: Should be compatible with most mods that don't modify weapon systems
