---
title: COIL Laser Technical Reference
inclusion: manual
tags: [coil, technical, reference, yal-1]
---

# COIL Laser Technical Reference

Technical specifications and implementation details for the YAL-1 COIL laser system in Nuclear Option.

## Real-World YAL-1 System

### Overview

The Boeing YAL-1 Airborne Laser Testbed was a megawatt-class directed energy weapon system designed to destroy tactical ballistic missiles during boost phase.

### Technical Specifications

| Specification | Value | Source |
|--------------|-------|--------|
| Platform | Boeing 747-400F | Public domain |
| Laser Type | Chemical Oxygen Iodine Laser (COIL) | Public domain |
| Power Output | ~1 megawatt | Public domain |
| Wavelength | 1315 nm (infrared) | Public domain |
| Theoretical Range | 300-600 km | Public domain |
| Practical Range | "Tens of kilometers" | Public domain |
| Target | Liquid-fuel missiles (600km), Solid-fuel (300km) | Public domain |

### System Components

1. **COIL Module**
   - Chemical oxygen generator
   - Iodine injection system
   - Optical resonator cavity
   - Beam director

2. **Tracking System**
   - Infrared sensors
   - Adaptive optics
   - Target tracking computer

3. **Beam Director**
   - Turret-mounted telescope
   - Adaptive optics for atmospheric compensation
   - Precision pointing system

### Operational Concept

1. **Detection**: Infrared sensors detect missile launch
2. **Tracking**: System locks onto missile plume
3. **Engagement**: Laser beam focused on missile body
4. **Destruction**: Continuous beam heats missile structure until failure

## Game Implementation

### Design Decisions

#### Range Reduction

**Real**: 300-600 km theoretical  
**Game**: 50 km effective

**Rationale**:
- Game balance considerations
- Typical engagement ranges in Nuclear Option
- Prevents overpowered long-range kills
- Maintains tactical gameplay

#### Damage Model

**Configuration**: 400 DPS × 5 seconds = 2000 total damage

**Rationale**:
- Sufficient to destroy any aircraft in one shot
- Requires sustained fire (5 seconds)
- Gives targets time to evade
- Balances power with vulnerability during firing

#### Ammunition System

**Configuration**: 30 shots × 5 seconds = 150 seconds total firing time

**Rationale**:
- Represents chemical fuel capacity
- Limits sustained combat effectiveness
- Requires tactical shot selection
- Forces return to base for rearming

### Physics Model

#### Beam Propagation

```csharp
// Instantaneous hit (speed of light)
_beamRenderer.SetPosition(0, startPos);
_beamRenderer.SetPosition(1, endPos);
```

- No projectile travel time
- Raycast-based hit detection
- Atmospheric effects abstracted

#### Damage Application

```csharp
// Continuous damage over time
float damageThisTick = DamagePerSecond * Time.fixedDeltaTime;
```

- Applied every FixedUpdate (50Hz)
- Accumulates over firing duration
- Mixed damage types (thermal + kinetic)

#### Range Attenuation

```csharp
if (distance > MaxRange)
    return; // No damage beyond max range
```

- Hard cutoff at maximum range
- No damage falloff curve
- Simplified for gameplay

### Visual Representation

#### Beam Characteristics

| Property | Value | Rationale |
|----------|-------|-----------|
| Color | Orange-red (1.0, 0.3, 0.1) | Represents 1315nm IR wavelength |
| Width | 0.5m start, 0.3m end | Visible but not oversized |
| Opacity | 80% start, 40% end | Atmospheric scattering effect |
| Intensity | 8.0 | Bright but not blinding |

#### Particle Effects

- **Muzzle Flash**: Orange-yellow particles
- **Emission Rate**: 100 particles/second
- **Lifetime**: 0.5 seconds
- **Speed**: 5 m/s

### Audio Design (Planned)

#### Sound Profile

1. **Charging Sound** (0-1 second)
   - Low frequency hum
   - Rising pitch
   - Volume: 0.5

2. **Sustained Fire** (1-5 seconds)
   - Continuous high-pitched whine
   - Slight pitch variation
   - Volume: 0.7

3. **Shutdown** (5-6 seconds)
   - Falling pitch
   - Mechanical sounds
   - Volume: 0.4

## Performance Characteristics

### System Mass

| Component | Mass | Notes |
|-----------|------|-------|
| COIL Module | 10,000 kg | Laser cavity and optics |
| Chemical Tanks | 4,000 kg | Oxygen and iodine |
| Beam Director | 1,000 kg | Turret and telescope |
| **Total** | **15,000 kg** | Realistic for YAL-1 |

### Power Requirements

| Phase | Power Draw | Duration |
|-------|------------|----------|
| Standby | 10 MW | Continuous |
| Charging | 500 MW | 1 second |
| Firing | 1000 MW | 5 seconds |
| Cooldown | 100 MW | 10 seconds |

### Thermal Management

**Heat Generation**: 1000 MW × 5 seconds = 5000 MJ per shot

**Cooling Requirements**:
- Passive cooling: 10 seconds between shots
- Active cooling: 5 seconds between shots (future feature)
- Overheating: Reduced damage after 3 consecutive shots (future feature)

## Tactical Employment

### Optimal Use Cases

1. **Anti-Ship**
   - Large, slow targets
   - High damage requirement
   - Long engagement time acceptable

2. **Anti-Aircraft**
   - Heavy bombers
   - Transport aircraft
   - Helicopters

3. **Point Defense**
   - Incoming missiles
   - Quick engagement
   - High probability of kill

### Limitations

1. **Vulnerability During Firing**
   - Must maintain aim for 5 seconds
   - Predictable flight path
   - Exposed to counter-fire

2. **Limited Ammunition**
   - Only 30 shots
   - Must prioritize targets
   - Requires frequent rearming

3. **Range Constraints**
   - 50km maximum
   - Must close to engagement range
   - Vulnerable to long-range missiles

### Counter-Tactics

1. **Evasive Maneuvers**
   - Break laser lock
   - Reduce time on target
   - Force shot wastage

2. **Standoff Weapons**
   - Engage beyond 50km
   - Use long-range missiles
   - Avoid laser envelope

3. **Saturation Attack**
   - Multiple simultaneous targets
   - Exhaust ammunition
   - Overwhelm single-target weapon

## Comparison to Other Weapons

### vs. Missiles

| Aspect | COIL Laser | Missiles |
|--------|-----------|----------|
| Speed | Instant | Subsonic to hypersonic |
| Range | 50 km | 10-200+ km |
| Ammo | 30 shots | 4-20 missiles |
| Reload | At base | At base |
| Cost/Shot | $50,000 | $100,000-$5,000,000 |
| Evasion | Difficult | Countermeasures available |

### vs. Guns

| Aspect | COIL Laser | Guns |
|--------|-----------|------|
| Speed | Instant | 800-1000 m/s |
| Range | 50 km | 2-5 km |
| Ammo | 30 shots | 500-2000 rounds |
| Damage/Hit | 2000 | 50-200 |
| Accuracy | High | Medium |
| Sustained Fire | 5 seconds | 30-60 seconds |

## Future Development

### Planned Enhancements

1. **Adaptive Optics**
   - Atmospheric compensation
   - Improved accuracy at range
   - Weather effects

2. **Power Management**
   - Integration with aircraft power
   - Capacitor charging
   - Power priority system

3. **Thermal System**
   - Heat accumulation
   - Cooling mechanics
   - Performance degradation

4. **Targeting Computer**
   - Lead calculation
   - Optimal firing solution
   - Target priority

### Research Areas

1. **Beam Propagation**
   - Atmospheric scattering
   - Thermal blooming
   - Turbulence effects

2. **Damage Modeling**
   - Material-specific damage
   - Structural failure modes
   - Fire propagation

3. **Countermeasures**
   - Reflective coatings
   - Ablative armor
   - Smoke screens

## References

### Primary Sources

1. **Airborne Laser Program**
   - Missile Defense Agency reports (public domain)
   - Congressional testimony (public domain)
   - Technical papers (public domain)

2. **COIL Technology**
   - Chemical laser physics (public domain)
   - Optical resonator design (public domain)
   - Beam director systems (public domain)

### Implementation References

1. **Unity Documentation**
   - LineRenderer component
   - Particle systems
   - Physics raycasting

2. **BepInEx Documentation**
   - Plugin development
   - Configuration system
   - Harmony patching

3. **Nuclear Option**
   - Decompiled game code (reference only)
   - Weapon system architecture
   - Damage mechanics

## Glossary

**COIL**: Chemical Oxygen Iodine Laser - A type of chemical laser using excited oxygen to pump iodine atoms

**Boost Phase**: The initial phase of missile flight when the rocket motor is burning

**Adaptive Optics**: System that compensates for atmospheric distortion of laser beam

**Beam Director**: Turret-mounted telescope that aims and focuses the laser beam

**Thermal Blooming**: Atmospheric heating that defocuses laser beams

**pK**: Probability of Kill - Likelihood of destroying target with single engagement

**DPS**: Damage Per Second - Rate of damage application

**Fixed Update**: Unity physics update loop running at fixed 50Hz rate

## Appendix: Configuration Examples

### High-Power Configuration
```ini
[COIL Laser]
MaxRange = 75000
MaxShots = 20
DamagePerSecond = 600
PowerConsumption = 1500
```

### Balanced Configuration (Default)
```ini
[COIL Laser]
MaxRange = 50000
MaxShots = 30
DamagePerSecond = 400
PowerConsumption = 1000
```

### Training Configuration
```ini
[COIL Laser]
MaxRange = 30000
MaxShots = 100
DamagePerSecond = 200
PowerConsumption = 500
```

## Version History

### 1.0.0 (Initial Release)
- Basic COIL laser implementation
- Darkreach bomber integration
- Configurable parameters
- Visual effects system
- Damage mechanics
