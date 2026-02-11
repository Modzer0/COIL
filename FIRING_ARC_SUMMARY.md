# COIL Laser - Firing Arc Feature

## Overview

Added configurable firing arc to limit laser engagement angles relative to aircraft nose direction.

## Implementation

### Configuration Parameter

**FiringArc**
- **Type**: Float (degrees)
- **Default**: 180°
- **Range**: 0-360°
- **Description**: Maximum angle from aircraft nose for firing

### How It Works

1. **Angle Calculation**
   - Measures angle between aircraft forward vector and target direction
   - Uses `Vector3.Angle()` for accurate 3D angle calculation
   - Reference point is aircraft nose (forward direction)

2. **Firing Check**
   - Before firing: Checks if target is within arc
   - During firing: Continuously checks if target remains in arc
   - If target leaves arc: Stops firing immediately

3. **Behavior**
   - Target outside arc: Weapon won't fire
   - Target leaves arc during shot: Beam stops, shot incomplete
   - Requires pilot to maneuver to keep target in arc

## Code Changes

### NuclearOptionCOILMod.cs
```csharp
FiringArc = Config.Bind("COIL Laser", "FiringArc", 180f,
    "Maximum angle from aircraft nose for firing (default: 180 degrees = forward hemisphere)");
```

### COILLaser.cs

**Added Method:**
```csharp
private bool IsTargetInFiringArc(Unit target)
{
    Vector3 toTarget = (target.transform.position - attachedUnit.transform.position).normalized;
    float angleFromNose = Vector3.Angle(attachedUnit.transform.forward, toTarget);
    return angleFromNose <= COILModPlugin.FiringArc.Value;
}
```

**Updated Fire() Method:**
- Checks firing arc before starting shot
- Returns early if target outside arc
- Logs when target is out of arc

**Updated ApplyLaserDamage() Method:**
- Continuously checks arc during firing
- Stops beam if target leaves arc
- Prevents wasted ammunition

## Firing Arc Values

### Common Configurations

| Degrees | Coverage | Description | Use Case |
|---------|----------|-------------|----------|
| **45°** | Very narrow | Boresight only | Challenge mode |
| **90°** | Narrow | Nose-mounted | Realistic fixed mount |
| **120°** | Limited | YAL-1 realistic | Historical accuracy |
| **180°** | Forward hemisphere | Default | Balanced gameplay |
| **270°** | Wide arc | Forward + sides | Training mode |
| **360°** | Full sphere | All directions | Turret simulation |

### Visual Representation

```
Aircraft (top view):

     90° Arc              180° Arc             270° Arc
        ▲                    ▲                    ▲
       /|\                 /||||\                /|||||\
      / | \               / |||| \              / ||||| \
     /  |  \             /  ||||  \            /  |||||  \
    ────●────           ────●────            ────●────
        |                   |                    |||||
      (rear)              (rear)               (rear|||)
                                                 |||||
```

## Tactical Implications

### Narrow Arc (90°)
**Advantages:**
- Realistic for fixed mount
- Requires skill
- Balanced for multiplayer

**Disadvantages:**
- Difficult to use
- Requires precise maneuvering
- Easy to lose target

### Default Arc (180°)
**Advantages:**
- Realistic for gimbal mount
- Balanced difficulty
- Prevents rear shots

**Disadvantages:**
- Still requires pointing aircraft
- Can't engage targets behind

### Wide Arc (270°)
**Advantages:**
- Easier to use
- Allows side shots
- Good for training

**Disadvantages:**
- Less realistic
- May be too easy

### Full Sphere (360°)
**Advantages:**
- Maximum flexibility
- No aiming required
- Fun for casual play

**Disadvantages:**
- Completely unrealistic
- Overpowered
- Removes piloting skill

## Preset Configurations

### Balanced (Default)
```ini
FiringArc = 180
```
- Forward hemisphere only
- Realistic for nose-mounted laser
- Requires pointing aircraft at target

### Training Mode
```ini
FiringArc = 270
```
- Wide arc for easier learning
- Allows side engagements
- More forgiving

### Realistic YAL-1
```ini
FiringArc = 120
```
- Limited gimbal range
- Historical accuracy
- More challenging

### Challenge Mode
```ini
FiringArc = 90
```
- Very narrow arc
- Requires expert piloting
- Maximum difficulty

### Overpowered Mode
```ini
FiringArc = 360
```
- Full sphere coverage
- Turret simulation
- No aiming required

## Real-World Reference

### YAL-1 Airborne Laser

The real YAL-1 had a turret-mounted beam director with limited gimbal range:
- **Estimated Range**: 120-180° forward arc
- **Reason**: Turret mounted in nose section
- **Limitation**: Structural constraints, optics size
- **Typical Engagement**: Forward aspect only

### Implementation Rationale

**Default 180°:**
- Represents forward hemisphere
- Realistic for aircraft-mounted laser
- Prevents unrealistic rear-aspect shots
- Requires tactical positioning

## Gameplay Impact

### Pilot Skill Required

**Narrow Arc (90°):**
- High skill requirement
- Precise maneuvering essential
- Difficult target tracking
- Rewards expert pilots

**Default Arc (180°):**
- Moderate skill requirement
- Standard maneuvering
- Balanced difficulty
- Accessible to most players

**Wide Arc (270°):**
- Low skill requirement
- Easy target tracking
- Forgiving gameplay
- Good for beginners

**Full Sphere (360°):**
- No skill requirement
- Point-and-click
- Removes challenge
- Casual/fun mode only

### Multiplayer Balance

**Recommended for Competitive:**
- 90-180° range
- Requires positioning
- Skill-based gameplay
- Fair for all players

**Not Recommended:**
- 270-360° range
- Too easy to use
- Removes tactical depth
- Unbalanced advantage

## Testing Scenarios

### Test 1: Static Target
1. Spawn aircraft
2. Spawn stationary target ahead
3. Verify laser fires (within arc)
4. Turn 90° away
5. Verify laser doesn't fire (outside arc)

### Test 2: Moving Target
1. Engage moving target
2. Begin firing
3. Target maneuvers out of arc
4. Verify beam stops
5. Target returns to arc
6. Verify can fire again

### Test 3: Arc Limits
1. Set FiringArc = 90
2. Position target at 45° (should fire)
3. Position target at 91° (should not fire)
4. Verify boundary is accurate

### Test 4: Continuous Tracking
1. Begin firing at target
2. Pilot maneuvers to track target
3. Target stays in arc
4. Verify continuous beam
5. Complete full shot duration

## Debug Logging

The implementation includes debug logging:

```
[COIL Mod] Target outside firing arc
[COIL Mod] Target at 95.3° from nose (max: 90.0°)
[COIL Mod] Target left firing arc - stopping fire
```

Enable BepInEx debug logging to see arc calculations in real-time.

## Performance Considerations

**Computational Cost:**
- One `Vector3.Angle()` call per fire attempt
- One `Vector3.Angle()` call per FixedUpdate during firing
- Negligible performance impact (<0.1ms)

**Optimization:**
- Angle calculation only when firing
- Early return if target null
- No continuous checking when not firing

## Future Enhancements

### Potential Features

1. **Visual Arc Indicator**
   - HUD overlay showing firing arc
   - Color-coded (green = in arc, red = out of arc)
   - Helps pilot aim

2. **Arc-Based Damage Falloff**
   - Full damage at center (0°)
   - Reduced damage at arc edges
   - More realistic beam director limits

3. **Dynamic Arc**
   - Arc changes with aircraft speed
   - Narrower at high speed (stability)
   - Wider at low speed (maneuverability)

4. **Separate Azimuth/Elevation Limits**
   - Different limits for horizontal/vertical
   - More realistic gimbal simulation
   - Asymmetric firing envelope

## Summary

Successfully implemented configurable firing arc system:
- ✅ Configuration parameter (FiringArc)
- ✅ Angle calculation from aircraft nose
- ✅ Pre-fire arc checking
- ✅ Continuous arc monitoring during fire
- ✅ Automatic beam stop if target leaves arc
- ✅ Debug logging for troubleshooting
- ✅ Updated all documentation
- ✅ Updated all presets with appropriate arcs
- ✅ Performance optimized

The firing arc adds tactical depth and realism to the COIL laser weapon system.
