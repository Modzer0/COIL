# Nuclear Option COIL Laser Mod - Quick Start

## What is this?

This mod adds the **YAL-1 COIL (Chemical Oxygen Iodine Laser)** weapon to the Darkreach bomber in Nuclear Option. It's a megawatt-class directed energy weapon based on the real Boeing YAL-1 Airborne Laser program.

## Installation (5 minutes)

### Step 1: Install BepInEx

1. Download [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx/releases) (x64 version)
2. Extract to your Nuclear Option game folder:
   ```
   D:\SteamLibrary\steamapps\common\Nuclear Option\
   ```
3. Run the game once (BepInEx will initialize)
4. Close the game

### Step 2: Install COIL Mod

1. Build the mod (see below) or download release
2. Copy `NuclearOptionCOILMod.dll` to:
   ```
   Nuclear Option\BepInEx\plugins\
   ```
3. Launch the game

## Building the Mod (2 minutes)

### Windows

```cmd
# Set your game directory
setx NUCLEAR_OPTION_DIR "D:\SteamLibrary\steamapps\common\Nuclear Option"

# Build
cd NuclearOptionCOILMod
dotnet build -c Release

# DLL automatically copies to BepInEx\plugins\
```

### Verify Installation

Launch Nuclear Option and check the BepInEx console for:
```
[Info: COIL Laser] Plugin Nuclear Option COIL Laser v1.0.0 loaded!
```

## Using the COIL Laser

### In-Game

1. **Select Darkreach bomber** in aircraft selection
2. **Spawn aircraft** in mission or free flight
3. **Cycle weapons** (Tab key) to find "COIL Laser"
4. **Aim at target** and hold fire button
5. **Hold for 5 seconds** to destroy target
6. **30 shots total** before needing to rearm

### Key Features

- **Range**: 50km maximum
- **Damage**: Destroys any aircraft in 5 seconds
- **Ammo**: 30 shots (5 seconds each = 2.5 minutes total)
- **Rearm**: Land at friendly airbase
- **Visual**: Orange-red laser beam with particles

## Configuration

The config file is automatically created at: `BepInEx\config\com.nuclearoption.coil.cfg`

### How to Configure

1. Run game once (config file is created)
2. Close game
3. Edit `BepInEx\config\com.nuclearoption.coil.cfg`
4. Restart game

### Quick Settings

```ini
[COIL Laser]
MaxRange = 50000        # Range in meters (default: 50km)
MaxShots = 30           # Number of shots (default: 30)
DamagePerSecond = 400   # Damage output (default: 400)
EnableCOIL = true       # Enable/disable mod
```

### Example Presets

**More Ammo**: `MaxShots = 50`  
**Longer Range**: `MaxRange = 75000`  
**Higher Damage**: `DamagePerSecond = 600`  
**Training Mode**: `MaxShots = 100, DamagePerSecond = 200`

See `NuclearOptionCOILMod/CONFIGURATION.md` for detailed guide.

## Troubleshooting

### Mod doesn't load
- Check BepInEx is installed correctly
- Verify DLL is in `BepInEx\plugins\`
- Check `BepInEx\LogOutput.log` for errors

### COIL laser doesn't appear
- Make sure you're using the **Darkreach bomber**
- Check logs for "COIL Mod" messages
- Verify mod is enabled in config

### No damage
- Target must be within **50km range**
- Hold fire for full **5 seconds**
- Check line of sight to target

## Quick Reference

| Feature | Value |
|---------|-------|
| Aircraft | Darkreach bomber only |
| Range | 50 km |
| Damage | 2000 per shot (5 sec) |
| Ammo | 30 shots |
| Reload | At airbase |
| Cost | $50,000 per shot |
| Mass | 15 tons |

## What's Next?

- Read `NuclearOptionCOILMod/README.md` for detailed info
- Check `.kiro/steering/` for technical documentation
- Report issues on GitHub
- Suggest features in discussions

## Real-World Background

The YAL-1 was a real U.S. military program that mounted a megawatt-class chemical laser on a Boeing 747. It was designed to shoot down ballistic missiles at ranges up to 600km. The program was cancelled in 2011, but this mod brings the concept to Nuclear Option!

**Key Facts:**
- Megawatt power output
- 1315nm infrared wavelength
- Chemical oxygen iodine laser (COIL)
- Designed for missile defense
- Mounted on Boeing 747-400F

## Support

- **Documentation**: See `NuclearOptionCOILMod/README.md`
- **Technical Details**: See `.kiro/steering/coil-technical-reference.md`
- **Build Guide**: See `.kiro/steering/coil-build-test.md`

---

**Have fun vaporizing targets with your megawatt laser!** ⚡🔥
