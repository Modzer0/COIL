---
title: COIL Mod Build and Testing Guide
inclusion: manual
tags: [build, testing, deployment, coil]
---

# COIL Mod Build and Testing Guide

Complete guide for building, testing, and deploying the Nuclear Option COIL Laser mod.

## Prerequisites

### Required Software

1. **Development Tools**
   - Visual Studio 2019 or later (Community Edition sufficient)
   - OR Visual Studio Code with C# extension
   - .NET Framework 4.6 or higher
   - Git (for version control)

2. **Game Requirements**
   - Nuclear Option (Steam version)
   - BepInEx 5.4.x or higher installed
   - Game must run at least once before modding

3. **Optional Tools**
   - dnSpy (for debugging)
   - ILSpy (for code inspection)
   - Unity Asset Bundle Extractor (for assets)

### Environment Setup

#### Windows

```cmd
# Set game directory environment variable
setx NUCLEAR_OPTION_DIR "D:\SteamLibrary\steamapps\common\Nuclear Option"

# Verify BepInEx installation
dir "%NUCLEAR_OPTION_DIR%\BepInEx"
```

#### Linux (Proton/Wine)

```bash
# Set game directory
export NUCLEAR_OPTION_DIR="$HOME/.steam/steam/steamapps/common/Nuclear Option"

# Verify BepInEx installation
ls "$NUCLEAR_OPTION_DIR/BepInEx"
```

## Building the Mod

### Method 1: Visual Studio

1. **Open Project**
   ```
   File → Open → Project/Solution
   Select: NuclearOptionCOILMod.csproj
   ```

2. **Configure Build**
   - Build Configuration: Release
   - Platform: Any CPU
   - Target Framework: .NET Framework 4.6

3. **Build**
   ```
   Build → Build Solution (Ctrl+Shift+B)
   ```

4. **Output Location**
   ```
   bin/Release/net46/NuclearOptionCOILMod.dll
   ```

### Method 2: Command Line

```cmd
# Navigate to project directory
cd NuclearOptionCOILMod

# Restore dependencies
dotnet restore

# Build release version
dotnet build -c Release

# Output: bin/Release/net46/NuclearOptionCOILMod.dll
```

### Method 3: MSBuild

```cmd
# Using MSBuild directly
msbuild NuclearOptionCOILMod.csproj /p:Configuration=Release /p:Platform="Any CPU"
```

### Build Verification

```cmd
# Check DLL was created
dir bin\Release\net46\NuclearOptionCOILMod.dll

# Verify file size (should be ~50-100 KB)
# Verify timestamp is recent
```

## Installation

### Automatic Installation

If `NUCLEAR_OPTION_DIR` environment variable is set, the post-build event automatically copies the DLL:

```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent">
  <Copy SourceFiles="$(TargetPath)" 
        DestinationFolder="$(NUCLEAR_OPTION_DIR)\BepInEx\plugins\" />
</Target>
```

### Manual Installation

```cmd
# Copy DLL to plugins folder
copy bin\Release\net46\NuclearOptionCOILMod.dll "%NUCLEAR_OPTION_DIR%\BepInEx\plugins\"

# Verify installation
dir "%NUCLEAR_OPTION_DIR%\BepInEx\plugins\NuclearOptionCOILMod.dll"
```

### Verify BepInEx Configuration

Check `BepInEx/config/BepInEx.cfg`:

```ini
[Logging.Console]
Enabled = true

[Logging.Disk]
Enabled = true
```

## Testing

### Test Plan

#### Phase 1: Installation Testing

**Objective**: Verify mod loads correctly

1. **Launch Game**
   - Start Nuclear Option
   - BepInEx console should appear
   - Look for mod loading message

2. **Check Logs**
   ```
   BepInEx/LogOutput.log
   ```
   
   Expected output:
   ```
   [Info   : COIL Laser] Plugin Nuclear Option COIL Laser v1.0.0 loaded!
   [Info   : COIL Laser] COIL Laser Configuration:
   [Info   : COIL Laser]   Max Range: 50000m
   [Info   : COIL Laser]   Max Shots: 30
   [Info   : COIL Laser]   Damage/sec: 400
   ```

3. **Check Configuration**
   ```
   BepInEx/config/com.nuclearoption.coil.cfg
   ```
   
   Should be created with default values

**Pass Criteria**:
- ✓ Mod appears in BepInEx plugin list
- ✓ No error messages in log
- ✓ Configuration file created
- ✓ Game launches successfully

#### Phase 2: Functional Testing

**Objective**: Verify COIL laser appears and functions

1. **Aircraft Selection**
   - Start new mission or free flight
   - Select Darkreach bomber
   - Spawn aircraft

2. **Weapon Verification**
   - Open weapon selection (default: Tab key)
   - Cycle through weapons
   - Look for "YAL-1 COIL Laser" or "COIL"

3. **Visual Inspection**
   - Check weapon bay area
   - Verify no visual glitches
   - Check aircraft mass/performance

**Pass Criteria**:
- ✓ COIL laser appears in weapon list
- ✓ Weapon shows 30/30 ammo
- ✓ No visual artifacts
- ✓ Aircraft spawns correctly

#### Phase 3: Combat Testing

**Objective**: Verify weapon fires and deals damage

1. **Target Setup**
   - Spawn enemy aircraft (any type)
   - Position at medium range (~20km)
   - Ensure clear line of sight

2. **Weapon Selection**
   - Select COIL laser weapon station
   - Verify weapon is ready (green indicator)
   - Check ammo count (30)

3. **Firing Test**
   - Aim at target
   - Hold fire button
   - Observe for 5 seconds

4. **Damage Verification**
   - Target should show damage
   - Target should be destroyed after 5 seconds
   - Ammo should decrease to 29

5. **Repeat Test**
   - Fire at multiple targets
   - Verify consistent behavior
   - Check ammo depletion

**Pass Criteria**:
- ✓ Laser beam visible when firing
- ✓ Target takes damage
- ✓ Target destroyed in ~5 seconds
- ✓ Ammo decrements correctly
- ✓ Visual effects appear

#### Phase 4: Edge Case Testing

**Objective**: Test boundary conditions

1. **Range Testing**
   - Fire at target at 60km (beyond max range)
   - Expected: No damage
   - Fire at target at 40km (within range)
   - Expected: Normal damage

2. **Ammo Depletion**
   - Fire all 30 shots
   - Attempt to fire with 0 ammo
   - Expected: No firing, no beam

3. **Rearm Testing**
   - Land at friendly airbase
   - Rearm aircraft
   - Expected: Ammo restored to 30

4. **Multiple Aircraft**
   - Spawn multiple Darkreach bombers
   - Verify each has COIL laser
   - Test simultaneous firing

**Pass Criteria**:
- ✓ Range limits enforced
- ✓ Cannot fire with 0 ammo
- ✓ Rearming works correctly
- ✓ Multiple aircraft supported

#### Phase 5: Performance Testing

**Objective**: Verify acceptable performance

1. **Frame Rate**
   - Monitor FPS while firing
   - Expected: <5 FPS drop
   - Test with multiple lasers firing

2. **Memory Usage**
   - Check memory before/after spawning
   - Expected: <10MB increase
   - Monitor for memory leaks

3. **Load Time**
   - Measure mission load time
   - Expected: <1 second increase
   - Compare with/without mod

**Pass Criteria**:
- ✓ Minimal FPS impact
- ✓ No memory leaks
- ✓ Acceptable load time

### Test Scenarios

#### Scenario 1: Anti-Aircraft

```
Setup:
- Darkreach bomber at 10,000m altitude
- Enemy fighter at 8,000m altitude, 30km range
- Clear weather

Procedure:
1. Select COIL laser
2. Lock target
3. Close to 40km range
4. Fire for 5 seconds
5. Observe results

Expected:
- Fighter destroyed
- 1 shot consumed
- Beam visible throughout engagement
```

#### Scenario 2: Anti-Ship

```
Setup:
- Darkreach bomber at 5,000m altitude
- Enemy destroyer at sea level, 20km range
- Calm seas

Procedure:
1. Select COIL laser
2. Lock target
3. Maintain aim for 5 seconds
4. Observe damage

Expected:
- Ship takes heavy damage
- Multiple shots may be required
- Beam tracks target
```

#### Scenario 3: Rapid Engagement

```
Setup:
- Darkreach bomber at 8,000m altitude
- 5 enemy aircraft in formation, 25km range

Procedure:
1. Select COIL laser
2. Engage first target (5 seconds)
3. Switch to second target
4. Repeat for all targets

Expected:
- Each target destroyed in one shot
- 5 shots consumed
- 25 shots remaining
```

### Debugging

#### Common Issues

**Issue**: Mod doesn't load

```
Check:
1. BepInEx installed correctly?
2. DLL in correct folder?
3. .NET Framework 4.6 installed?
4. Check BepInEx/LogOutput.log for errors
```

**Issue**: COIL laser doesn't appear

```
Check:
1. Using Darkreach bomber?
2. Check logs for "COIL Mod" messages
3. Verify WeaponManager patch applied
4. Check for conflicting mods
```

**Issue**: No damage dealt

```
Check:
1. Target within 50km range?
2. Clear line of sight?
3. Firing for full 5 seconds?
4. Check damage calculation in logs
```

**Issue**: Visual effects missing

```
Check:
1. Graphics settings (effects enabled?)
2. Camera distance from aircraft
3. LineRenderer component created?
4. Material assigned correctly?
```

#### Debug Logging

Enable detailed logging:

```csharp
// In COILModPlugin.cs
Logger.LogLevel = LogLevel.Debug;

// Add debug logs
Logger.LogDebug($"Firing at target: {target.name}");
Logger.LogDebug($"Distance: {distance}m");
Logger.LogDebug($"Damage: {damage}");
```

#### Using dnSpy

1. **Attach Debugger**
   - Open dnSpy
   - Debug → Attach to Process
   - Select NuclearOption.exe

2. **Set Breakpoints**
   - Open NuclearOptionCOILMod.dll
   - Navigate to method
   - Click left margin to set breakpoint

3. **Inspect Variables**
   - Trigger breakpoint in-game
   - Examine local variables
   - Step through code

## Deployment

### Release Checklist

- [ ] All tests passing
- [ ] No debug logging in release build
- [ ] Version number updated
- [ ] README.md updated
- [ ] CHANGELOG.md updated
- [ ] Configuration defaults verified
- [ ] Build in Release mode
- [ ] DLL file size reasonable (<200KB)

### Creating Release Package

```cmd
# Create release directory
mkdir release

# Copy DLL
copy bin\Release\net46\NuclearOptionCOILMod.dll release\

# Copy documentation
copy README.md release\
copy CHANGELOG.md release\

# Create ZIP archive
powershell Compress-Archive -Path release\* -DestinationPath NuclearOptionCOILMod-v1.0.0.zip
```

### Distribution

1. **GitHub Release**
   - Create new release tag (v1.0.0)
   - Upload ZIP file
   - Include changelog in description

2. **Nexus Mods** (if applicable)
   - Create mod page
   - Upload files
   - Add screenshots/videos
   - Write description

3. **Steam Workshop** (if supported)
   - Follow Steam Workshop guidelines
   - Upload mod files
   - Add preview images

## Continuous Integration

### GitHub Actions Example

```yaml
name: Build COIL Mod

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 4.6
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build -c Release
    
    - name: Upload artifact
      uses: actions/upload-artifact@v2
      with:
        name: NuclearOptionCOILMod
        path: bin/Release/net46/NuclearOptionCOILMod.dll
```

## Troubleshooting

### Build Errors

**Error**: Cannot find Assembly-CSharp.dll

```
Solution:
1. Set NUCLEAR_OPTION_DIR environment variable
2. Verify game installation
3. Check .csproj Reference path
```

**Error**: BepInEx.Core not found

```
Solution:
1. Restore NuGet packages: dotnet restore
2. Check internet connection
3. Clear NuGet cache: dotnet nuget locals all --clear
```

### Runtime Errors

**Error**: MissingMethodException

```
Cause: Game version mismatch
Solution:
1. Update game to latest version
2. Recompile mod against current Assembly-CSharp.dll
3. Check Harmony patch signatures
```

**Error**: NullReferenceException

```
Cause: Missing component or reference
Solution:
1. Add null checks
2. Verify object initialization
3. Check Unity lifecycle timing
```

## Performance Profiling

### Unity Profiler

1. **Enable Deep Profiling**
   - Requires Unity Editor with game source
   - Not available for released game

2. **Alternative: Manual Timing**
   ```csharp
   var stopwatch = System.Diagnostics.Stopwatch.StartNew();
   // Code to profile
   stopwatch.Stop();
   Logger.LogInfo($"Execution time: {stopwatch.ElapsedMilliseconds}ms");
   ```

### Memory Profiling

```csharp
// Before operation
long memBefore = GC.GetTotalMemory(false);

// Operation
CreateCOILLaser();

// After operation
long memAfter = GC.GetTotalMemory(false);
Logger.LogInfo($"Memory allocated: {(memAfter - memBefore) / 1024}KB");
```

## Version Control

### Git Workflow

```bash
# Create feature branch
git checkout -b feature/enhanced-visuals

# Make changes
# ...

# Commit changes
git add .
git commit -m "Add enhanced beam visuals"

# Push to remote
git push origin feature/enhanced-visuals

# Create pull request
```

### Semantic Versioning

- **Major** (1.x.x): Breaking changes
- **Minor** (x.1.x): New features, backwards compatible
- **Patch** (x.x.1): Bug fixes

Example:
- 1.0.0: Initial release
- 1.1.0: Add audio system
- 1.1.1: Fix audio bug
- 2.0.0: Rewrite damage system (breaking)

## Support

### User Support

1. **Documentation**
   - Maintain comprehensive README
   - Include troubleshooting section
   - Provide configuration examples

2. **Issue Tracking**
   - Use GitHub Issues
   - Provide issue template
   - Label and prioritize

3. **Community**
   - Discord server
   - Steam discussions
   - Reddit community

### Maintenance

1. **Game Updates**
   - Monitor for game patches
   - Test compatibility
   - Update as needed

2. **Bug Fixes**
   - Prioritize critical bugs
   - Release patches promptly
   - Communicate with users

3. **Feature Requests**
   - Collect user feedback
   - Evaluate feasibility
   - Plan roadmap
