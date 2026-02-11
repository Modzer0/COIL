# Nuclear Option COIL Laser Mod

A BepInEx mod that adds the YAL-1 COIL (Chemical Oxygen Iodine Laser) weapon system to the Darkreach bomber in Nuclear Option.

## 🚀 Quick Start

### Installation

1. **Install BepInEx** (if not already installed)
   - Download [BepInEx 5.4.x](https://github.com/BepInEx/BepInEx/releases)
   - Extract to Nuclear Option game folder
   - Run game once to initialize

2. **Build the Mod**
   ```cmd
   # Windows
   setx NUCLEAR_OPTION_DIR "D:\SteamLibrary\steamapps\common\Nuclear Option"
   build.cmd
   
   # Linux/Mac
   export NUCLEAR_OPTION_DIR="$HOME/.steam/steam/steamapps/common/Nuclear Option"
   ./build.sh
   ```

3. **Launch Nuclear Option** and select the Darkreach bomber

See [QUICKSTART.md](QUICKSTART.md) for detailed instructions.

## 📖 Documentation

- **[QUICKSTART.md](QUICKSTART.md)** - 5-minute installation and usage guide
- **[NuclearOptionCOILMod/README.md](NuclearOptionCOILMod/README.md)** - Complete user documentation
- **[NuclearOptionCOILMod/CONFIGURATION.md](NuclearOptionCOILMod/CONFIGURATION.md)** - Configuration guide with presets
- **[PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)** - Project overview and achievements
- **[.kiro/steering/](./kiro/steering/)** - Technical documentation and development guides

## ⚡ Features

- **Megawatt-class laser weapon** based on real YAL-1 specifications
- **50km effective range** (game-balanced)
- **30 shots** of 5-second continuous fire
- **2000 damage per shot** - destroys any target
- **Configurable parameters** via BepInEx config
- **Visual effects** - orange-red beam, particles, lighting
- **Internal weapon bay mounting** on Darkreach bomber

## 🎮 Usage

1. Select **Darkreach bomber** in aircraft selection
2. Spawn in mission or free flight
3. **Cycle weapons** (Tab) to select COIL laser
4. **Aim at target** and hold fire button
5. **Hold for 5 seconds** to destroy target
6. **Rearm at airbase** when depleted

## ⚙️ Configuration

Config file auto-created at: `BepInEx/config/com.nuclearoption.coil.cfg`

### Quick Config

1. Run game once → Close game
2. Edit `BepInEx/config/com.nuclearoption.coil.cfg`
3. Restart game

### Settings

```ini
[COIL Laser]
MaxRange = 50000        # Maximum range (meters)
MaxShots = 30           # Number of shots
DamagePerSecond = 400   # Damage output
PowerConsumption = 1000 # Power draw (MW)
EnableCOIL = true       # Enable/disable mod
```

### Presets

- **Balanced**: Default settings
- **High Power**: `MaxRange=75000, MaxShots=20, DPS=600`
- **Training**: `MaxRange=30000, MaxShots=100, DPS=200`

See [NuclearOptionCOILMod/CONFIGURATION.md](NuclearOptionCOILMod/CONFIGURATION.md) for complete guide.

## 🔬 Technical Details

### Real-World Basis

Based on the Boeing YAL-1 Airborne Laser program:
- Megawatt-class Chemical Oxygen Iodine Laser (COIL)
- Designed to destroy ballistic missiles at 300-600km range
- Mounted on modified Boeing 747-400F
- 1315nm infrared wavelength
- Program cancelled in 2011

### Implementation

- **Language**: C#
- **Framework**: BepInEx 5.x
- **Patching**: Harmony 2.x
- **Target**: Nuclear Option (Unity 2021.3.x)
- **Architecture**: Component-based, extends base game Laser class

## 📁 Project Structure

```
COIL/
├── build.cmd / build.sh        # Build scripts
├── QUICKSTART.md               # Quick start guide
├── PROJECT_SUMMARY.md          # Project overview
├── NuclearOptionCOILMod/       # Mod source code
│   ├── *.cs                    # C# source files
│   ├── *.csproj                # Project file
│   ├── README.md               # User documentation
│   └── CHANGELOG.md            # Version history
└── .kiro/steering/             # Developer documentation
    ├── coil-mod-development.md
    ├── coil-technical-reference.md
    └── coil-build-test.md
```

## 🛠️ Building from Source

### Prerequisites

- .NET Framework 4.6+
- BepInEx 5.4.x
- Nuclear Option game files
- Visual Studio or dotnet CLI

### Build Commands

**Windows:**
```cmd
setx NUCLEAR_OPTION_DIR "path\to\game"
build.cmd
```

**Linux/Mac:**
```bash
export NUCLEAR_OPTION_DIR="path/to/game"
./build.sh
```

**Manual:**
```cmd
cd NuclearOptionCOILMod
dotnet build -c Release
```

Output: `bin/Release/net46/NuclearOptionCOILMod.dll`

## 🧪 Testing

See [.kiro/steering/coil-build-test.md](.kiro/steering/coil-build-test.md) for comprehensive test plan including:
- Installation testing
- Functional testing
- Combat testing
- Edge case testing
- Performance testing

## 📊 Performance

- **FPS Impact**: <5 FPS drop while firing
- **Memory**: <10MB additional allocation
- **Load Time**: <1 second increase
- **System Mass**: 15 tons (realistic)

## 🐛 Known Issues

- Audio not implemented (silent firing)
- Basic visual effects (no advanced shaders)
- Only works on Darkreach bomber
- No power system integration

## 🗺️ Roadmap

### Version 1.1.0 - Audio Update
- Custom laser firing sounds
- Charging and shutdown audio
- Environmental effects

### Version 1.2.0 - Visual Enhancement
- Beam distortion shader
- Heat shimmer effects
- Enhanced particles

### Version 1.3.0 - Gameplay Mechanics
- Power management
- Thermal system
- Cooling mechanics

### Version 2.0.0 - Major Update
- Multi-aircraft support
- Targeting computer UI
- Weather effects
- AI usage

## 🤝 Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Update documentation
6. Submit a pull request

## 📜 License

This mod is provided as-is for Nuclear Option. Not affiliated with Killerfish Games.

## 🙏 Credits

- **Mod Development**: Created for Nuclear Option modding community
- **Based on**: Real YAL-1 Airborne Laser program
- **Game**: Nuclear Option by Killerfish Games
- **Framework**: BepInEx modding framework
- **Patching**: Harmony library

## 📞 Support

- **Issues**: Report bugs via GitHub Issues
- **Documentation**: See docs in `.kiro/steering/`
- **Quick Help**: Check [QUICKSTART.md](QUICKSTART.md)

## 📚 References

- [YAL-1 Wikipedia](https://en.wikipedia.org/wiki/Boeing_YAL-1)
- [COIL Technology](https://en.wikipedia.org/wiki/Chemical_oxygen_iodine_laser)
- [BepInEx Documentation](https://docs.bepinex.dev/)
- [Harmony Documentation](https://harmony.pardeike.net/)

---

**Ready to vaporize targets with megawatt-class laser power!** ⚡🔥
