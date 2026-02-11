# COIL Laser Configuration Guide

## Configuration File Location

After running the mod for the first time, BepInEx automatically creates the configuration file at:

```
Nuclear Option/BepInEx/config/com.nuclearoption.coil.cfg
```

## How to Configure

1. **Launch Nuclear Option** with the mod installed (at least once)
2. **Close the game**
3. **Navigate to** `BepInEx/config/` folder
4. **Open** `com.nuclearoption.coil.cfg` in any text editor
5. **Edit values** as desired
6. **Save the file**
7. **Restart the game** for changes to take effect

## Configuration Parameters

### MaxRange

**Description**: Maximum effective range of the COIL laser in meters

**Type**: Decimal number (float)  
**Default**: `52202` (~52.2 kilometers)  
**Recommended Range**: 10000 - 100000

**Calculation**: Default range calculated to hit targets at maximum altitude (15km) from 50km horizontal distance:
- Range = √(50000² + 15000²) = 52,202 meters
- Allows engagement of high-altitude targets at standoff range

**Examples:**
```ini
MaxRange = 52202    # Default - 52.2km (high-altitude capable)
MaxRange = 75000    # Extended range - 75km
MaxRange = 30000    # Short range - 30km (training)
MaxRange = 100000   # Maximum range - 100km (overpowered)
```

**Notes:**
- Real YAL-1 had 300-600km theoretical range
- Default allows hitting 15km altitude targets at 50km horizontal
- Game balance suggests 30-75km for fair gameplay
- Longer ranges make the weapon very powerful
- Consider map size when adjusting

---

### MaxShots

**Description**: Number of firing sequences before ammunition depletion

**Type**: Integer  
**Default**: `30` shots  
**Recommended Range**: 10 - 100

**Examples:**
```ini
MaxShots = 30     # Default - 30 shots
MaxShots = 50     # Extended ammo - 50 shots
MaxShots = 15     # Limited ammo - 15 shots
MaxShots = 100    # Training mode - 100 shots
```

**Notes:**
- Total firing time = MaxShots × ShotDuration
- Default: 30 shots × 5 seconds = 150 seconds total
- Must rearm at airbase when depleted
- Higher values reduce need for frequent rearming

---

### ShotDuration

**Description**: Duration of each continuous firing sequence in seconds

**Type**: Decimal number (float)  
**Default**: `5` seconds  
**Recommended Range**: 1 - 10

**Examples:**
```ini
ShotDuration = 5     # Default - 5 second shots
ShotDuration = 3     # Quick shots - 3 seconds
ShotDuration = 8     # Long shots - 8 seconds
ShotDuration = 1     # Rapid fire - 1 second shots
```

**Notes:**
- Shorter durations = more tactical, rapid engagements
- Longer durations = sustained beam, easier to use
- Total firing time = MaxShots × ShotDuration
- Affects how long you must hold fire button

---

### DamagePerShot

**Description**: Total damage dealt during one complete shot

**Type**: Decimal number (float)  
**Default**: `2000` damage  
**Recommended Range**: 500 - 5000

**Examples:**
```ini
DamagePerShot = 2000    # Default - destroys most aircraft
DamagePerShot = 3000    # High damage - destroys anything
DamagePerShot = 1000    # Low damage - multiple shots needed
DamagePerShot = 4000    # Extreme damage - instant kills
```

**Damage Per Second Calculation:**
- DPS = DamagePerShot / ShotDuration
- Default: 2000 / 5 = 400 DPS
- Example: 3000 / 5 = 600 DPS

**Target Durability Reference:**
- Fighter aircraft: ~500-1000 HP
- Bomber aircraft: ~1500-2500 HP
- Ships: ~3000-5000 HP

**Notes:**
- 2000 damage is balanced for one-shot kills on aircraft
- Lower values require multiple shots per target
- Higher values are overkill but satisfying
- Damage is applied continuously over ShotDuration

---

### FiringArc

**Description**: Maximum angle from aircraft nose for firing (in degrees)

**Type**: Decimal number (float)  
**Default**: `180` degrees (forward hemisphere)  
**Recommended Range**: 30 - 360

**Explanation:**
- Measures angle from aircraft's forward direction (nose)
- 0° = directly ahead
- 90° = perpendicular to sides
- 180° = forward hemisphere (default)
- 360° = all directions

**Examples:**
```ini
FiringArc = 180    # Default - forward hemisphere only
FiringArc = 90     # Narrow arc - nose-mounted only
FiringArc = 270    # Wide arc - forward and sides
FiringArc = 360    # Full sphere - turret mode
FiringArc = 45     # Very narrow - boresight only
```

**Tactical Implications:**

**180° (Default - Forward Hemisphere)**
- Realistic for nose-mounted laser
- Requires pointing aircraft at target
- Prevents rear-aspect shots
- Balanced gameplay

**90° (Narrow Arc)**
- Very restrictive
- Requires precise aiming
- More challenging
- Realistic for fixed mount

**270° (Wide Arc)**
- Allows side shots
- More flexible engagement
- Less realistic
- Easier to use

**360° (Full Sphere)**
- Unrealistic for aircraft laser
- Allows any-aspect shots
- Very powerful
- Turret simulation

**Notes:**
- Real YAL-1 had limited gimbal range (~120-180°)
- Smaller arcs require better piloting
- Larger arcs make weapon easier to use
- Target must stay in arc during entire shot duration
- Laser stops firing if target leaves arc

---

### PowerConsumption

**Description**: Power consumption per second in megawatts

**Type**: Decimal number (float)  
**Default**: `0` MW  
**Recommended Range**: 0 - 2000

**Examples:**
```ini
PowerConsumption = 0       # Default - chemical laser (no electrical power)
PowerConsumption = 1000    # Simulated electrical power
PowerConsumption = 1500    # High power draw
```

**Notes:**
- Default is 0 because COIL is a chemical laser
- Real YAL-1 used chemical reaction (oxygen + iodine)
- No electrical power required for beam generation
- Currently cosmetic (not integrated with aircraft power)
- Future versions may integrate with power management
- Set to non-zero for gameplay balance if desired

---

### EnableCOIL

**Description**: Master switch to enable or disable the entire mod

**Type**: Boolean (true/false)  
**Default**: `true`

**Examples:**
```ini
EnableCOIL = true     # Mod enabled (default)
EnableCOIL = false    # Mod disabled
```

**Notes:**
- Set to `false` to disable mod without uninstalling
- Useful for testing or temporary disable
- Requires game restart to take effect

## Configuration Presets

### Balanced (Default)
```ini
[COIL Laser]
MaxRange = 52202
MaxShots = 30
ShotDuration = 5
DamagePerShot = 2000
FiringArc = 180
PowerConsumption = 0
EnableCOIL = true
```
- Good for normal gameplay
- One-shot kills on most targets
- Requires tactical ammo management
- Can hit high-altitude targets
- Forward hemisphere only

### High Power
```ini
[COIL Laser]
MaxRange = 75000
MaxShots = 20
ShotDuration = 5
DamagePerShot = 3000
FiringArc = 180
PowerConsumption = 0
EnableCOIL = true
```
- Extended range
- Higher damage
- Limited ammunition (more tactical)

### Training Mode
```ini
[COIL Laser]
MaxRange = 30000
MaxShots = 100
ShotDuration = 3
DamagePerShot = 1000
FiringArc = 270
PowerConsumption = 0
EnableCOIL = true
```
- Shorter range (easier to use)
- Lots of ammunition (practice)
- Shorter shots (rapid fire)
- Lower damage (multiple shots needed)
- Wide firing arc (easier aiming)

### Realistic YAL-1
```ini
[COIL Laser]
MaxRange = 100000
MaxShots = 15
ShotDuration = 8
DamagePerShot = 4000
FiringArc = 120
PowerConsumption = 0
EnableCOIL = true
```
- Long range (closer to real specs)
- Very limited shots (chemical fuel)
- Long sustained beam
- High damage (missile-killer)
- Narrow arc (realistic gimbal limits)

### Overpowered (Fun Mode)
```ini
[COIL Laser]
MaxRange = 100000
MaxShots = 100
ShotDuration = 5
DamagePerShot = 4000
FiringArc = 360
PowerConsumption = 0
EnableCOIL = true
```
- Maximum range
- Unlimited-feeling ammo
- Extreme damage
- Full sphere firing (turret mode)
- Not balanced for competitive play

## Troubleshooting

### Configuration Not Loading

**Problem**: Changes to config file don't take effect

**Solutions:**
1. Make sure you saved the file after editing
2. Restart the game completely (close and reopen)
3. Check for typos in parameter names
4. Verify values are in correct format (numbers without quotes)
5. Check BepInEx console for error messages

### Invalid Values

**Problem**: Game crashes or mod doesn't work after config change

**Solutions:**
1. Restore default values
2. Check for negative numbers (not allowed)
3. Verify MaxShots is an integer (no decimals)
4. Check for missing equals signs
5. Delete config file and let it regenerate

### Config File Missing

**Problem**: Can't find `com.nuclearoption.coil.cfg`

**Solutions:**
1. Run the game at least once with mod installed
2. Check `BepInEx/config/` folder exists
3. Verify mod is installed in `BepInEx/plugins/`
4. Check BepInEx console for mod loading messages
5. Copy example config from mod folder

## Advanced Configuration

### Editing While Game is Running

**Not Recommended**: Changes require game restart

BepInEx loads configuration at startup. To apply changes:
1. Close the game
2. Edit config file
3. Restart the game

### Multiple Configurations

You can create backup configs for different scenarios:

```
BepInEx/config/
├── com.nuclearoption.coil.cfg           # Active config
├── com.nuclearoption.coil.balanced.cfg  # Backup: balanced
├── com.nuclearoption.coil.training.cfg  # Backup: training
└── com.nuclearoption.coil.realistic.cfg # Backup: realistic
```

To switch configs:
1. Rename active config to backup name
2. Rename desired backup to `com.nuclearoption.coil.cfg`
3. Restart game

### Sharing Configurations

To share your config with others:
1. Copy `com.nuclearoption.coil.cfg`
2. Share the file
3. Others place it in their `BepInEx/config/` folder
4. Restart game

## Configuration Tips

### For New Players
- Start with default settings
- Increase MaxShots if running out of ammo
- Decrease MaxRange if weapon feels too powerful

### For Experienced Players
- Reduce MaxShots for more tactical gameplay
- Increase DamagePerSecond for faster kills
- Adjust MaxRange based on mission types

### For Mission Creators
- Use Training preset for tutorial missions
- Use Realistic preset for challenge missions
- Use Balanced preset for standard missions

### For Multiplayer (if supported)
- All players should use same config
- Lower MaxShots for balance
- Moderate MaxRange to prevent spawn camping

## Performance Considerations

Configuration values that may affect performance:

- **MaxRange**: Higher values = longer raycasts (minimal impact)
- **DamagePerSecond**: No performance impact
- **MaxShots**: No performance impact
- **PowerConsumption**: No performance impact (cosmetic)

All settings should have negligible performance impact.

## Future Configuration Options

Planned for future versions:

- **CooldownTime**: Time between shots
- **HeatGeneration**: Heat per shot
- **CoolingRate**: Heat dissipation rate
- **BeamWidth**: Visual beam thickness
- **BeamColor**: Laser color customization
- **AudioVolume**: Sound effect volume
- **TargetingMode**: Auto-aim assistance
- **PowerDrain**: Actual power consumption

## Support

If you have questions about configuration:

1. Check this guide first
2. Review example presets above
3. Check BepInEx console for errors
4. Report issues on GitHub
5. Ask in community Discord

## Example Config File

A complete example config file is included in the mod folder:
```
NuclearOptionCOILMod/com.nuclearoption.coil.cfg
```

You can copy this to `BepInEx/config/` as a starting point.

---

**Remember**: Always restart the game after changing configuration!
