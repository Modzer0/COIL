# COIL Laser Mod - Updates Summary

## Changes Made

### 1. New Configuration Parameters

Replaced `DamagePerSecond` with two new parameters for better control:

#### ShotDuration
- **Type**: Float
- **Default**: 5 seconds
- **Description**: Duration of each continuous firing sequence
- **Purpose**: Allows users to configure how long they must hold the fire button
- **Range**: 1-10 seconds recommended

#### DamagePerShot
- **Type**: Float
- **Default**: 2000
- **Description**: Total damage dealt during one complete shot
- **Purpose**: Direct control over damage output per shot
- **Calculation**: DPS = DamagePerShot / ShotDuration

### Benefits of New System

**Old System:**
- `DamagePerSecond = 400`
- Fixed 5-second shot duration
- Total damage = 400 × 5 = 2000

**New System:**
- `ShotDuration = 5` (configurable)
- `DamagePerShot = 2000` (configurable)
- DPS = 2000 / 5 = 400 (calculated)

**Advantages:**
- Users can adjust shot length independently
- Direct control over total damage per shot
- More intuitive configuration
- Enables rapid-fire mode (short duration)
- Enables sustained beam mode (long duration)

### 2. Weapon Icon Implementation

Created a procedurally generated weapon icon:

**Design:**
- Green perimeter box (bright green #00FF00)
- "COIL" text in center
- 256x256 texture size
- Matches game's weapon icon style

**Features:**
- Generated at runtime (no external assets needed)
- Simple block letter font
- High visibility green color
- Professional appearance

**Implementation:**
- `CreateCOILIcon()` method generates Texture2D
- Converts to Sprite for WeaponInfo
- Draws border and text using pixel manipulation
- Helper methods for each letter (C, O, I, L)

### 3. Code Updates

#### NuclearOptionCOILMod.cs
- Added `ShotDuration` configuration entry
- Renamed `DamagePerSecond` to `DamagePerShot`
- Updated logging to show calculated DPS
- Updated configuration descriptions

#### COILLaserWeapon.cs
- Added `CreateCOILIcon()` method
- Added letter drawing methods (DrawC, DrawO, DrawI, DrawL)
- Updated damage calculation to use DamagePerShot / ShotDuration
- Assigned generated icon to WeaponInfo

#### COILLaser.cs
- Added `_shotDuration` field
- Loads ShotDuration from config in Start()
- Updated damage calculation to use new formula
- Updated logging to show shot duration

### 4. Documentation Updates

#### com.nuclearoption.coil.cfg
- Updated parameter names
- Updated descriptions
- Updated default values
- Added ShotDuration parameter

#### CONFIGURATION.md
- Complete rewrite of parameter sections
- Added ShotDuration documentation
- Updated DamagePerShot documentation
- Updated all preset configurations
- Updated damage calculations
- Added DPS calculation examples

#### CONFIG_EXAMPLE.txt
- Updated all 8 presets with new parameters
- Added ShotDuration to each preset
- Updated damage calculations
- Added DPS values to each preset
- Updated descriptions

#### QUICK_CONFIG_REFERENCE.txt
- Updated parameter table
- Updated preset comparison table
- Updated damage calculator
- Updated firing time calculator
- Updated quick adjustments
- Added weapon icon information

## Configuration Examples

### Default Configuration
```ini
[COIL Laser]
MaxRange = 50000
MaxShots = 30
ShotDuration = 5
DamagePerShot = 2000
PowerConsumption = 1000
EnableCOIL = true
```

**Result:**
- 30 shots of 5 seconds each
- 2000 damage per shot
- 400 DPS (2000 / 5)
- 150 seconds total firing time

### Rapid Fire Configuration
```ini
ShotDuration = 2
DamagePerShot = 1000
MaxShots = 60
```

**Result:**
- 60 quick shots of 2 seconds each
- 1000 damage per shot
- 500 DPS (1000 / 2)
- 120 seconds total firing time
- Tactical rapid engagement

### Sustained Beam Configuration
```ini
ShotDuration = 8
DamagePerShot = 4000
MaxShots = 15
```

**Result:**
- 15 long shots of 8 seconds each
- 4000 damage per shot
- 500 DPS (4000 / 8)
- 120 seconds total firing time
- Realistic YAL-1 sustained beam

## Weapon Icon Appearance

```
┌────────────────────────────┐
│                            │
│                            │
│        ┌──────────┐        │
│        │          │        │
│        │   COIL   │        │
│        │          │        │
│        └──────────┘        │
│                            │
│                            │
└────────────────────────────┘
```

- Green border (8 pixels wide)
- 20 pixel margin from edges
- Block letter text
- Centered in icon
- 256x256 resolution

## Backward Compatibility

**Breaking Change:** Configuration file format changed

**Migration:**
- Old: `DamagePerSecond = 400`
- New: `ShotDuration = 5` and `DamagePerShot = 2000`

**User Action Required:**
- Delete old config file
- Let mod regenerate with new format
- Or manually update to new parameter names

## Testing Checklist

### Configuration Testing
- [ ] Test default values
- [ ] Test ShotDuration range (1-10 seconds)
- [ ] Test DamagePerShot range (500-5000)
- [ ] Verify DPS calculation
- [ ] Test all 8 presets
- [ ] Verify config file generation

### Weapon Icon Testing
- [ ] Icon appears in weapon selection
- [ ] Icon is correct size
- [ ] Green color is visible
- [ ] Text is readable
- [ ] Icon matches other weapons

### Gameplay Testing
- [ ] Short duration shots (2s)
- [ ] Long duration shots (8s)
- [ ] Damage calculation correct
- [ ] Shot tracking works
- [ ] Ammo depletion correct

## Files Modified

### Source Code
1. `NuclearOptionCOILMod/NuclearOptionCOILMod.cs` - Configuration
2. `NuclearOptionCOILMod/COILLaserWeapon.cs` - Icon generation
3. `NuclearOptionCOILMod/COILLaser.cs` - Damage calculation

### Documentation
4. `NuclearOptionCOILMod/com.nuclearoption.coil.cfg` - Example config
5. `NuclearOptionCOILMod/CONFIGURATION.md` - Complete guide
6. `NuclearOptionCOILMod/CONFIG_EXAMPLE.txt` - Preset examples
7. `NuclearOptionCOILMod/QUICK_CONFIG_REFERENCE.txt` - Quick reference

## Summary

Successfully implemented:
- ✅ Configurable shot duration (ShotDuration parameter)
- ✅ Configurable total damage (DamagePerShot parameter)
- ✅ Procedurally generated weapon icon (green box with COIL text)
- ✅ Updated all documentation
- ✅ Updated all configuration presets
- ✅ Maintained backward compatibility in code (breaking config change)

The mod now offers more flexible configuration and a distinctive visual identity in the weapon selection menu.
