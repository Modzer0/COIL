# Technology Stack

## Core Technologies

- **Language**: C#
- **Framework**: BepInEx (Unity mod framework)
- **Target Game**: Nuclear Option
- **Unity Version**: Match the game's Unity version

## BepInEx Conventions

- Use `[BepInPlugin]` attribute with unique GUID, name, and version
- Inherit from `BaseUnityPlugin` for main plugin class
- Use Harmony for patching game methods when necessary
- Leverage BepInEx configuration system for user settings
- Follow BepInEx logging conventions using `Logger` property

## C# Best Practices

- Use PascalCase for public members, camelCase for private fields
- Prefix private fields with underscore (e.g., `_fieldName`)
- Use meaningful names that reflect purpose
- Keep methods focused and single-purpose
- Use `null` checks and defensive programming for game object references
- Avoid blocking operations in Unity's main thread

## Common Patterns

- Use Harmony patches (`[HarmonyPatch]`) to modify existing game behavior
- Cache frequently accessed components and references
- Use coroutines for time-based or multi-frame operations
- Implement proper cleanup in `OnDestroy` or plugin disable
- Use configuration files for tunable AI parameters

## Build & Development Commands

### Building
- Build using Visual Studio or `dotnet build`
- Output DLL should be placed in `BepInEx/plugins/` folder

### Testing
- Test in-game with BepInEx console enabled
- Use BepInEx LogLevel.Debug for detailed logging during development
- Verify compatibility with base game updates

### Debugging
- Enable BepInEx console for runtime logging
- Use dnSpy or similar tools for game assembly inspection
- Test with various unit types and scenarios

## Code Quality

- Follow standard C# formatting conventions
- Use XML documentation comments for public APIs
- Handle Unity lifecycle events properly
- Minimize performance impact on game loop
- Test with multiple units to ensure scalability
