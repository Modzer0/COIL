# Configuration System Summary

## What Was Added

Complete BepInEx configuration system with comprehensive documentation for the COIL Laser mod.

## Files Created

### 1. Example Configuration File
**File**: `NuclearOptionCOILMod/com.nuclearoption.coil.cfg`
- BepInEx-formatted configuration file
- Default values for all parameters
- Comments explaining each setting
- Ready to copy to `BepInEx/config/`

### 2. Complete Configuration Guide
**File**: `NuclearOptionCOILMod/CONFIGURATION.md`
- Detailed explanation of each parameter
- Configuration file location and usage
- 5 preset configurations (Balanced, High Power, Training, Realistic, Overpowered)
- Troubleshooting section
- Advanced configuration tips
- Performance considerations
- Future configuration options

### 3. Configuration Examples
**File**: `NuclearOptionCOILMod/CONFIG_EXAMPLE.txt`
- 8 ready-to-use preset configurations:
  1. Balanced (Default)
  2. High Power
  3. Training Mode
  4. Realistic YAL-1
  5. Overpowered (Fun Mode)
  6. Challenge Mode
  7. Anti-Ship Specialist
  8. Point Defense
- Configuration tips for each parameter
- Damage calculations
- Target HP reference
- Troubleshooting guide

### 4. Quick Reference Card
**File**: `NuclearOptionCOILMod/QUICK_CONFIG_REFERENCE.txt`
- One-page quick reference
- Parameter table with defaults
- Preset comparison table
- Quick adjustment suggestions
- Damage calculator
- Target HP reference
- Troubleshooting checklist

## Configuration Parameters

### MaxRange
- **Type**: Float (decimal number)
- **Default**: 50000 (50 kilometers)
- **Range**: 10000 - 100000
- **Description**: Maximum effective range of the COIL laser in meters
- **Game Balance**: 30-75km recommended for fair gameplay

### MaxShots
- **Type**: Integer (whole number)
- **Default**: 30
- **Range**: 10 - 100
- **Description**: Number of 5-second firing sequences before depletion
- **Total Time**: MaxShots × 5 seconds (default: 150 seconds)

### DamagePerSecond
- **Type**: Float (decimal number)
- **Default**: 400
- **Range**: 200 - 800
- **Description**: Damage dealt per second of laser fire
- **Per Shot**: DamagePerSecond × 5 seconds (default: 2000 damage)

### PowerConsumption
- **Type**: Float (decimal number)
- **Default**: 1000 (megawatts)
- **Range**: 500 - 2000
- **Description**: Power consumption per second (currently cosmetic)
- **Future**: May integrate with aircraft power systems

### EnableCOIL
- **Type**: Boolean (true/false)
- **Default**: true
- **Description**: Master switch to enable or disable the mod
- **Usage**: Set to false to disable without uninstalling

## How Users Configure

### Step 1: Initial Setup
1. Install mod in `BepInEx/plugins/`
2. Run game once (config file auto-created)
3. Close game

### Step 2: Edit Configuration
1. Navigate to `BepInEx/config/`
2. Open `com.nuclearoption.coil.cfg` in text editor
3. Modify values as desired
4. Save file

### Step 3: Apply Changes
1. Restart Nuclear Option
2. Changes take effect immediately
3. Check BepInEx console for confirmation

## Preset Configurations

### Balanced (Default)
```ini
MaxRange = 50000
MaxShots = 30
DamagePerSecond = 400
```
- Normal gameplay
- One-shot kills on aircraft
- Tactical ammo management

### High Power
```ini
MaxRange = 75000
MaxShots = 20
DamagePerSecond = 600
```
- Extended range
- Higher damage
- Limited ammunition

### Training Mode
```ini
MaxRange = 30000
MaxShots = 100
DamagePerSecond = 200
```
- Shorter range (easier)
- Lots of ammo (practice)
- Lower damage (forgiving)

### Realistic YAL-1
```ini
MaxRange = 100000
MaxShots = 15
DamagePerSecond = 800
```
- Long range (real specs)
- Very limited shots
- High damage

### Overpowered
```ini
MaxRange = 100000
MaxShots = 100
DamagePerSecond = 800
```
- Maximum everything
- Casual fun mode
- Not balanced

## Documentation Updates

### Updated Files
1. **README.md** (root) - Added configuration section
2. **QUICKSTART.md** - Added quick config guide
3. **NuclearOptionCOILMod/README.md** - Expanded configuration section
4. **CHANGELOG.md** - Added configuration documentation entries
5. **PROJECT_SUMMARY.md** - Updated file structure and documentation list

### New Documentation
- Complete configuration guide (CONFIGURATION.md)
- 8 preset examples (CONFIG_EXAMPLE.txt)
- Quick reference card (QUICK_CONFIG_REFERENCE.txt)
- Example config file (com.nuclearoption.coil.cfg)

## User Benefits

### Easy Configuration
- Auto-generated config file
- Clear parameter names
- Helpful comments
- Default values work well

### Multiple Presets
- 8 ready-to-use configurations
- Copy-paste ready
- Different gameplay styles
- Easy to customize

### Comprehensive Documentation
- Step-by-step instructions
- Parameter explanations
- Troubleshooting help
- Quick reference available

### Flexible Gameplay
- Adjust difficulty
- Change weapon characteristics
- Balance for multiplayer
- Create custom scenarios

## Technical Implementation

### BepInEx Configuration System
```csharp
MaxRange = Config.Bind("COIL Laser", "MaxRange", 50000f,
    "Maximum effective range of the COIL laser in meters (default: 50km)");
```

### Features
- Persistent across game sessions
- Type-safe (enforces correct data types)
- Automatic file generation
- Comments included in file
- Hot-reloadable (with game restart)

### File Format
```ini
## Settings file was created by plugin Nuclear Option COIL Laser v1.0.0
## Plugin GUID: com.nuclearoption.coil

[COIL Laser]

## Maximum effective range of the COIL laser in meters (default: 50km)
# Setting type: Single
# Default value: 50000
MaxRange = 50000
```

## Testing Recommendations

### Configuration Testing
1. Test with default values
2. Test each preset configuration
3. Test extreme values (min/max)
4. Test invalid values (negative, text)
5. Test with missing config file

### User Experience Testing
1. First-time user (auto-generation)
2. Configuration modification
3. Game restart behavior
4. Error handling
5. Documentation clarity

## Future Enhancements

### Planned Configuration Options
- CooldownTime: Time between shots
- HeatGeneration: Heat per shot
- CoolingRate: Heat dissipation
- BeamWidth: Visual beam thickness
- BeamColor: Laser color customization
- AudioVolume: Sound effect volume
- TargetingMode: Auto-aim assistance
- PowerDrain: Actual power consumption

### Advanced Features
- In-game configuration UI
- Profile system (multiple configs)
- Per-aircraft configurations
- Mission-specific overrides
- Multiplayer synchronization

## Summary

Successfully implemented a complete, user-friendly configuration system with:
- ✅ 5 configurable parameters
- ✅ Auto-generated config file
- ✅ 8 preset configurations
- ✅ Comprehensive documentation (4 files)
- ✅ Quick reference materials
- ✅ Troubleshooting guides
- ✅ Example configurations
- ✅ Updated all documentation

Users can now easily customize the COIL laser to their preferred gameplay style with clear, well-documented configuration options.
