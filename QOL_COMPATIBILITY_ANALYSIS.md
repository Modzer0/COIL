# QoL Mod Compatibility Analysis

## Summary
The COIL mod is **COMPATIBLE** with the QoL mod. No conflicts detected.

## Analysis Date
February 10, 2026

## QoL Mod Version Analyzed
Version 1.1.6.2b2 (GUID: "com.offiry.qol")

## Harmony Patch Comparison

### COIL Mod Patches
- **WeaponManager.SpawnWeapons** - Postfix patch
  - Adds COIL laser to Darkreach bomber after weapons spawn
  - Does not modify existing behavior, only adds new weapon

### QoL Mod Weapon-Related Patches
1. **Hardpoint.SpawnMount** - Prefix and Postfix patches
   - Handles gun activation and fuel tank setup
   - Does not conflict with COIL laser injection

2. **WeaponMount.Initialize** - Prefix patch
   - Diagnostic logging only
   - Does not modify behavior

3. **SavedLoadout.CreateLoadout** - Patch for loadout system
   - Handles saved loadouts
   - Does not affect runtime weapon spawning

4. **WeaponSelector.PopulateOptions** - UI patch
   - Client-side only (notDedicatedServer check)
   - Affects weapon selection menu
   - Does not conflict with weapon spawning

5. **Gun.AttachToHardpoint** - Prefix patch
   - Activates gun prefabs
   - COIL laser is not a Gun, so this doesn't apply

6. **Various HUD patches** - Display patches only
   - HUDLaserGuidedState.UpdateWeaponDisplay
   - HUDBoresightState.UpdateWeaponDisplay
   - CombatHUD.ShowWeaponStation
   - These are UI-only and don't affect weapon functionality

## Patch Execution Order

### When Darkreach Spawns:
1. **WeaponManager.SpawnWeapons** is called
2. QoL mod patches (if any) execute on individual weapon spawns
3. **COIL mod Postfix** executes AFTER all base game and QoL processing
4. COIL laser is added to internal bay

### Why This Works:
- COIL mod uses **Postfix** patch, which runs AFTER all other processing
- COIL mod only ADDS weapons, doesn't modify existing spawn logic
- QoL mod patches focus on specific weapon types (Gun, FuelTank) that don't apply to COIL
- No shared state or conflicting modifications

## Potential Issues: NONE IDENTIFIED

### Checked For:
- ✅ Conflicting Harmony patch targets
- ✅ Patch priority conflicts
- ✅ Shared state modifications
- ✅ Hardpoint spawn interference
- ✅ WeaponMount initialization conflicts
- ✅ Weapon registration conflicts

### Not Issues:
- QoL's Hardpoint.SpawnMount patches run for each individual hardpoint
- COIL mod creates its own GameObject and registers it properly
- Both mods use proper Unity component lifecycle
- No race conditions in patch execution

## Load Order
**Load order does not matter** - both mods can load in any order because:
- Different patch targets (QoL patches individual weapon types, COIL patches WeaponManager)
- COIL uses Postfix which always runs after Prefix patches
- No shared configuration or state

## Testing Recommendations

### Basic Functionality Test:
1. Load both mods simultaneously
2. Spawn Darkreach bomber in-game
3. Verify COIL laser appears in weapon bay
4. Verify all QoL features still work (copilots, liveries, etc.)
5. Fire COIL laser and verify damage/range/arc functionality

### Edge Cases to Test:
1. **Saved Loadouts**: Create loadout with COIL, save, reload
2. **Multiplayer**: Host with both mods, client joins
3. **Multiple Darkreach**: Spawn multiple Darkreach bombers
4. **QoL Aircraft Names**: Verify AI Darkreach get random names
5. **QoL Liveries**: Verify AI Darkreach get random liveries

## Configuration Compatibility

### COIL Mod Config:
- `com.nuclearoption.coil.cfg`
- No conflicts with QoL config

### QoL Mod Config:
- `com.offiry.qol.cfg`
- Contains settings for various features
- None affect weapon spawning directly

## Conclusion
**Status: FULLY COMPATIBLE**

The COIL mod and QoL mod can be used together without any modifications. The mods operate on different aspects of the game:
- **COIL mod**: Adds new weapon to specific aircraft
- **QoL mod**: Enhances graphics, UI, AI behavior, and existing weapon behavior

No patch conflicts, no shared state issues, no load order requirements.

## Recommendations
1. ✅ Use both mods together without concerns
2. ✅ No special configuration needed
3. ✅ No load order requirements
4. ✅ Test in-game to verify (standard practice)

## Technical Notes

### Why Postfix Patch is Safe:
```csharp
[HarmonyPatch(typeof(WeaponManager), "SpawnWeapons")]
[HarmonyPostfix]
public static void SpawnWeapons_Postfix(WeaponManager __instance)
```

- Runs AFTER all base game logic
- Runs AFTER all other mods' Prefix patches
- Only adds new content, doesn't modify existing
- Uses proper Unity component registration

### QoL Mod Patch Style:
- Mostly Prefix patches for early interception
- Postfix patches for post-processing
- No Transpiler patches that would conflict
- Well-structured with proper null checks

## Future Considerations
If QoL mod updates to patch `WeaponManager.SpawnWeapons` directly, re-test compatibility. Current version (1.1.6.2b2) does not patch this method.
