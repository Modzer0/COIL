# Changelog

All notable changes to the Nuclear Option COIL Laser mod will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-02-10

### Added
- Initial release of COIL Laser mod
- YAL-1 COIL laser weapon implementation for Darkreach bomber
- Configurable parameters via BepInEx configuration system
  - MaxRange: Maximum effective range (default 50km)
  - MaxShots: Number of shots before depletion (default 30)
  - DamagePerSecond: Damage output (default 400)
  - PowerConsumption: Power draw (default 1000MW)
  - EnableCOIL: Master enable/disable switch
- Configuration documentation
  - CONFIGURATION.md: Complete configuration guide
  - CONFIG_EXAMPLE.txt: 8 preset configurations
  - com.nuclearoption.coil.cfg: Example config file
- Custom laser weapon class extending base game Laser
- Harmony patches for weapon injection
- Visual effects system
  - Orange-red laser beam (LineRenderer)
  - Spot light for environmental illumination
  - Muzzle flash particle system
- Damage mechanics
  - Continuous beam damage application
  - Mixed damage types (70% blast, 30% thermal)
  - 5-second shot duration
  - High armor penetration
- Ammunition system
  - 30 shots per loadout
  - Shot tracking and depletion
  - Rearm support at airbases
- Internal weapon bay mounting
  - Automatic detection of Darkreach bomber
  - Integration with hardpoint system
  - Weapon station registration
- Comprehensive documentation
  - README with installation and usage instructions
  - Technical reference guide
  - Development guide
  - Build and testing guide

### Technical Details
- Based on real YAL-1 Airborne Laser specifications
- Megawatt-class chemical oxygen iodine laser
- 1315nm wavelength (represented as orange-red visible beam)
- 15-ton system mass (realistic for YAL-1)
- $5M system cost, $50K per shot
- Effective against all target types
- Range balanced for gameplay (50km vs 300-600km real-world)

### Known Limitations
- Audio system not implemented (silent firing)
- Basic visual effects (no advanced shaders)
- Only supports Darkreach bomber
- No power system integration
- No thermal management system

### Dependencies
- BepInEx 5.4.x or higher
- Nuclear Option (latest version)
- .NET Framework 4.6
- Harmony 2.x (included with BepInEx)

## [Unreleased]

### Planned Features
- Custom audio for laser firing, charging, and shutdown
- Enhanced visual effects
  - Beam distortion shader
  - Heat shimmer effects
  - Atmospheric scattering
  - Impact effects
- Power management system
  - Integration with aircraft power
  - Capacitor charging mechanics
  - Power priority system
- Thermal management
  - Heat accumulation
  - Cooling system
  - Performance degradation when overheated
- Targeting computer
  - Lead calculation
  - Optimal firing solution
  - Target priority system
- Multi-aircraft support
  - Configurable aircraft list
  - Per-aircraft customization
  - Different COIL variants
- Gameplay enhancements
  - Weather effects on beam
  - Atmospheric compensation
  - Countermeasure systems
- UI improvements
  - Custom weapon HUD
  - Heat gauge
  - Power status display

### Under Consideration
- Multiplayer compatibility testing
- VR support
- Custom missions featuring COIL laser
- AI usage of COIL laser
- Defensive countermeasures (smoke, reflective coating)

## Version History

### Version Numbering
- **Major.Minor.Patch** (e.g., 1.2.3)
- **Major**: Breaking changes, major features
- **Minor**: New features, backwards compatible
- **Patch**: Bug fixes, minor improvements

### Release Schedule
- **Patch releases**: As needed for critical bugs
- **Minor releases**: Monthly for new features
- **Major releases**: Quarterly for significant updates

## Support

For issues, feature requests, or questions:
- GitHub Issues: [Repository URL]
- Discord: [Server invite]
- Steam Discussions: [Workshop page]

## Credits

- Mod Development: [Your name/team]
- Based on YAL-1 Airborne Laser program
- Nuclear Option by Killerfish Games
- BepInEx modding framework
- Harmony patching library

## License

This mod is provided as-is for Nuclear Option. Not affiliated with Killerfish Games.
