# Nuclear Option COIL Laser Mod

This mod adds the YAL-1 COIL (Chemical Oxygen Iodine Laser) weapon system to the Darkreach bomber in Nuclear Option.

## About the YAL-1 COIL

The YAL-1 was a real megawatt-class airborne laser system mounted on a modified Boeing 747-400F. It was designed to destroy tactical ballistic missiles during their boost phase at ranges up to 300-600km. The system used a Chemical Oxygen Iodine Laser (COIL) operating at 1315nm wavelength.

## Features

- **Megawatt-class laser weapon** for the Darkreach bomber
- **Configurable parameters** via BepInEx configuration
- **Realistic implementation** based on YAL-1 specifications
- **High damage output** - 5 seconds of continuous fire destroys any target
- **Limited ammunition** - Chemical laser has finite shots
- **Internal weapon bay mounting** - Takes up all weapon bays on Darkreach

## Installation

1. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases) for Nuclear Option
2. Download the latest release of this mod
3. Extract `NuclearOptionCOILMod.dll` to `BepInEx/plugins/` folder
4. Launch Nuclear Option

## Configuration

Configuration file is automatically created at: `BepInEx/config/com.nuclearoption.coil.cfg`

### Quick Configuration

1. Run the game once with the mod installed
2. Close the game
3. Edit `BepInEx/config/com.nuclearoption.coil.cfg`
4. Restart the game

### Settings

| Parameter | Default | Description |
|-----------|---------|-------------|
| **MaxRange** | 50000 | Maximum range in meters (50km) |
| **MaxShots** | 30 | Number of 5-second shots |
| **DamagePerSecond** | 400 | Damage output (2000 per shot) |
| **PowerConsumption** | 1000 | Power draw in MW (cosmetic) |
| **EnableCOIL** | true | Enable/disable mod |

### Example Configurations

**Balanced (Default)**
```ini
MaxRange = 50000
MaxShots = 30
DamagePerSecond = 400
```

**High Power**
```ini
MaxRange = 75000
MaxShots = 20
DamagePerSecond = 600
```

**Training Mode**
```ini
MaxRange = 30000
MaxShots = 100
DamagePerSecond = 200
```

See [CONFIGURATION.md](CONFIGURATION.md) for detailed configuration guide with presets and troubleshooting.

## Usage

1. Select the Darkreach bomber in the aircraft selection menu
2. The COIL laser will automatically be equipped in the internal weapon bays
3. Select the COIL laser weapon station (cycle with weapon selection key)
4. Aim at target and fire
5. Hold fire button for continuous beam (each 5-second burst counts as one shot)
6. Rearm at airbase to replenish shots

## Technical Details

### Weapon Characteristics

- **Type**: Energy weapon (continuous beam)
- **Wavelength**: 1315nm (infrared)
- **Damage Type**: 70% blast, 30% thermal
- **Fire Rate**: Continuous (0s interval)
- **Shot Duration**: 5 seconds per shot
- **Armor Penetration**: Highly effective against all armor types
- **Visibility**: Very high when firing (100 visibility units)

### System Mass

- **Empty System**: 15,000 kg (15 tons)
  - Realistic for YAL-1 laser module and chemical tanks
- **Per-Shot Mass**: 0 kg (energy weapon)

### Cost

- **System Cost**: $5,000,000
- **Per-Shot Cost**: $50,000
  - Represents chemical fuel and maintenance

## Building from Source

### Prerequisites

- .NET Framework 4.6 or higher
- BepInEx 5.x
- Nuclear Option game files

### Build Steps

1. Set environment variable `NUCLEAR_OPTION_DIR` to your game installation path
   ```
   set NUCLEAR_OPTION_DIR=D:\SteamLibrary\steamapps\common\Nuclear Option
   ```

2. Build the project:
   ```
   dotnet build NuclearOptionCOILMod.csproj
   ```

3. The DLL will automatically copy to `BepInEx/plugins/` if the environment variable is set

## Compatibility

- **Game Version**: Nuclear Option (latest)
- **BepInEx Version**: 5.4.x or higher
- **Unity Version**: 2021.3.x

## Known Issues

- Audio clips are not implemented (silent firing)
- Visual effects are basic (can be enhanced with custom shaders)
- Only works on Darkreach bomber (by design)

## Future Enhancements

- Custom audio for laser firing
- Enhanced visual effects (beam distortion, heat shimmer)
- Power system integration
- Targeting computer UI
- Cooling system mechanics
- Multiple aircraft support

## Credits

- Based on the real YAL-1 Airborne Laser program
- Developed for Nuclear Option by the modding community

## License

This mod is provided as-is for Nuclear Option. Not affiliated with the game developers.

## Changelog

### Version 1.0.0
- Initial release
- COIL laser weapon implementation
- Darkreach bomber integration
- Configurable parameters
- Basic visual and damage systems
