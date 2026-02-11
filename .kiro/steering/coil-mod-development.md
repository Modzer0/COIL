---
title: COIL Laser Mod Development Guide
inclusion: manual
tags: [modding, weapons, coil, development]
---

# COIL Laser Mod Development Guide

This guide covers the development process and architecture of the Nuclear Option COIL Laser mod.

## Project Overview

The COIL (Chemical Oxygen Iodine Laser) mod adds a megawatt-class directed energy weapon to the Darkreach bomber, based on the real YAL-1 Airborne Laser program.

## Architecture

### Core Components

1. **COILModPlugin** (`NuclearOptionCOILMod.cs`)
   - Main BepInEx plugin entry point
   - Manages configuration
   - Initializes Harmony patches
   - Handles plugin lifecycle

2. **COILLaserPatch** (`COILLaserWeapon.cs`)
   - Harmony patches for weapon injection
   - Creates WeaponInfo and WeaponMount ScriptableObjects
   - Integrates with Darkreach bomber hardpoints
   - Manages weapon registration

3. **COILLaser** (`COILLaser.cs`)
   - Custom Laser weapon implementation
   - Handles firing mechanics
   - Manages ammunition (shots)
   - Renders beam visuals
   - Applies damage to targets

### Design Patterns

#### Harmony Patching
```csharp
[HarmonyPatch(typeof(WeaponManager), "SpawnWeapons")]
[HarmonyPostfix]
public static void SpawnWeapons_Postfix(WeaponManager __instance)
```

- Uses postfix patches to inject COIL laser after normal weapon spawning
- Non-destructive - doesn't interfere with base game functionality
- Checks aircraft type before adding weapon

#### ScriptableObject Creation
```csharp
_coilWeaponInfo = ScriptableObject.CreateInstance<WeaponInfo>();
```

- Creates runtime ScriptableObjects for weapon data
- Avoids need for asset bundles
- Allows dynamic configuration

#### Component-Based Architecture
```csharp
public class COILLaser : Laser
```

- Extends base game Laser class
- Inherits targeting and firing infrastructure
- Overrides specific behaviors (ammo, damage, visuals)

## Implementation Details

### Weapon Characteristics

The COIL laser is configured to match real-world YAL-1 specifications:

- **Range**: 50km (game-balanced from 300-600km real range)
- **Power**: Megawatt-class (1000MW consumption)
- **Damage**: 400 DPS × 5 seconds = 2000 total damage per shot
- **Shots**: 30 shots (represents chemical fuel capacity)

### Damage Model

```csharp
float damageThisTick = COILModPlugin.DamagePerSecond.Value * Time.fixedDeltaTime;
float fireDamage = damageThisTick * 0.3f; // 30% thermal
float blastDamage = damageThisTick * 0.7f; // 70% kinetic
```

- Continuous damage application during firing
- Mixed damage types (thermal + kinetic)
- High enough to destroy any target in 5 seconds

### Shot Tracking

```csharp
private const float SHOT_DURATION = 5f;
private int _shotsRemaining;
private float _shotStartTime;
```

- Each "shot" is 5 seconds of continuous fire
- Tracks firing duration per shot
- Decrements ammo after each complete shot

## Configuration System

### BepInEx Config

```csharp
MaxRange = Config.Bind("COIL Laser", "MaxRange", 50000f, "Maximum effective range...");
```

- User-configurable parameters
- Persistent across game sessions
- Hot-reloadable (requires game restart)

### Configuration Parameters

| Parameter | Default | Purpose |
|-----------|---------|---------|
| MaxRange | 50000m | Effective range limit |
| MaxShots | 30 | Ammunition capacity |
| DamagePerSecond | 400 | Damage output |
| PowerConsumption | 1000MW | Power draw (future use) |
| EnableCOIL | true | Master enable switch |

## Integration Points

### Darkreach Detection

```csharp
string aircraftName = aircraft.definition.unitName;
if (!aircraftName.Contains("Darkreach"))
    return;
```

- Checks aircraft name for "Darkreach"
- Only adds COIL to appropriate aircraft
- Case-insensitive matching

### Hardpoint Integration

```csharp
foreach (Hardpoint hardpoint in hardpointSet.hardpoints)
{
    if (hardpoint.bayDoors != null && hardpoint.bayDoors.Length > 0)
    {
        hasInternalBay = true;
        break;
    }
}
```

- Searches for internal weapon bays
- Identifies hardpoints with bay doors
- Mounts COIL in first suitable bay

### Weapon Registration

```csharp
weaponManager.RegisterWeapon(coilLaser, CreateCOILWeaponMount(), hardpoint);
```

- Registers with aircraft weapon manager
- Creates weapon station
- Enables weapon selection and firing

## Visual Effects

### Beam Rendering

```csharp
_beamRenderer = beamObj.AddComponent<LineRenderer>();
_beamRenderer.startWidth = 0.5f;
_beamRenderer.endWidth = 0.3f;
_beamRenderer.startColor = new Color(1f, 0.3f, 0.1f, 0.8f); // Orange-red
```

- LineRenderer for beam visualization
- Orange-red color (characteristic of COIL wavelength)
- Tapered beam (wider at source)

### Lighting

```csharp
_beamLight = lightObj.AddComponent<Light>();
_beamLight.type = LightType.Spot;
_beamLight.intensity = 8f;
```

- Spot light for environmental illumination
- Orange tint matching beam color
- Enhances visual impact

### Particle Effects

```csharp
_muzzleFlash = particleObj.AddComponent<ParticleSystem>();
var main = _muzzleFlash.main;
main.startColor = new Color(1f, 0.5f, 0.2f);
```

- Particle system for muzzle flash
- Continuous emission during firing
- Orange-yellow particles

## Testing Strategy

### Unit Testing

1. **Configuration Loading**
   - Verify all config values load correctly
   - Test default values
   - Test invalid values

2. **Weapon Creation**
   - Verify WeaponInfo creation
   - Check WeaponMount properties
   - Validate ScriptableObject initialization

3. **Damage Application**
   - Test damage calculation
   - Verify damage types
   - Check range limitations

### Integration Testing

1. **Aircraft Detection**
   - Test with Darkreach bomber
   - Test with other aircraft (should not add COIL)
   - Test with multiple aircraft

2. **Hardpoint Mounting**
   - Verify internal bay detection
   - Check weapon registration
   - Test weapon station creation

3. **Firing Mechanics**
   - Test continuous fire
   - Verify shot duration tracking
   - Check ammo depletion

### In-Game Testing

1. **Basic Functionality**
   - Spawn Darkreach bomber
   - Verify COIL appears in weapon list
   - Test weapon selection

2. **Combat Testing**
   - Fire at stationary targets
   - Fire at moving targets
   - Test at various ranges

3. **Edge Cases**
   - Fire with 0 ammo
   - Rearm at airbase
   - Test with multiple Darkreach aircraft

## Performance Considerations

### Optimization Techniques

1. **Object Pooling**
   - Reuse visual effect objects
   - Cache component references
   - Minimize GameObject creation

2. **Update Frequency**
   - Use FixedUpdate for physics/damage
   - Minimize per-frame calculations
   - Cache frequently accessed values

3. **Raycast Optimization**
   - Single raycast per fixed update
   - Layer mask filtering
   - Distance culling

### Performance Metrics

- **Target**: <1ms per frame impact
- **Memory**: <10MB additional allocation
- **Draw Calls**: +3 per active COIL laser

## Debugging

### Logging Strategy

```csharp
Debug.Log($"[COIL Mod] COIL Laser firing - {_shotsRemaining} shots remaining");
```

- Prefix all logs with `[COIL Mod]`
- Log key events (initialization, firing, depletion)
- Use appropriate log levels

### Common Issues

1. **COIL Not Appearing**
   - Check aircraft name detection
   - Verify hardpoint detection
   - Check BepInEx console for errors

2. **No Damage**
   - Verify raycast hitting target
   - Check damage calculation
   - Ensure IDamageable interface present

3. **Visual Issues**
   - Check LineRenderer enabled state
   - Verify material assignment
   - Check camera distance

## Future Enhancements

### Planned Features

1. **Audio System**
   - Custom laser firing sound
   - Charging sound
   - Overheating warnings

2. **Advanced Visuals**
   - Beam distortion shader
   - Heat shimmer effects
   - Impact effects

3. **Gameplay Mechanics**
   - Cooling system
   - Power management
   - Targeting computer

4. **Multi-Aircraft Support**
   - Add to other large aircraft
   - Configurable aircraft list
   - Per-aircraft customization

### Technical Debt

- Audio clips not implemented
- Basic visual effects
- No power system integration
- Limited error handling

## Contributing

### Code Style

- Follow C# naming conventions
- Use XML documentation comments
- Keep methods under 50 lines
- Add unit tests for new features

### Pull Request Process

1. Fork repository
2. Create feature branch
3. Implement changes
4. Add tests
5. Update documentation
6. Submit PR

## Resources

### External Documentation

- [BepInEx Documentation](https://docs.bepinex.dev/)
- [Harmony Documentation](https://harmony.pardeike.net/)
- [Unity Scripting Reference](https://docs.unity3d.com/ScriptReference/)

### YAL-1 References

- [Wikipedia: Boeing YAL-1](https://en.wikipedia.org/wiki/Boeing_YAL-1)
- [Chemical Oxygen Iodine Laser](https://en.wikipedia.org/wiki/Chemical_oxygen_iodine_laser)
- Defense Technical Information Center reports

## License

This mod is provided as-is for Nuclear Option. Not affiliated with the game developers.
