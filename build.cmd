@echo off
REM Build script for Nuclear Option COIL Laser Mod

echo ========================================
echo Nuclear Option COIL Laser Mod Builder
echo ========================================
echo.

REM Check if NUCLEAR_OPTION_DIR is set
if "%NUCLEAR_OPTION_DIR%"=="" (
    echo ERROR: NUCLEAR_OPTION_DIR environment variable not set!
    echo.
    echo Please set it to your Nuclear Option installation directory:
    echo   setx NUCLEAR_OPTION_DIR "D:\SteamLibrary\steamapps\common\Nuclear Option"
    echo.
    echo Then restart this command prompt and try again.
    exit /b 1
)

echo Game Directory: %NUCLEAR_OPTION_DIR%
echo.

REM Check if game directory exists
if not exist "%NUCLEAR_OPTION_DIR%" (
    echo ERROR: Game directory does not exist!
    echo Path: %NUCLEAR_OPTION_DIR%
    echo.
    echo Please verify your NUCLEAR_OPTION_DIR environment variable.
    exit /b 1
)

REM Check if BepInEx is installed
if not exist "%NUCLEAR_OPTION_DIR%\BepInEx" (
    echo WARNING: BepInEx folder not found!
    echo Please install BepInEx before using this mod.
    echo.
    echo Download from: https://github.com/BepInEx/BepInEx/releases
    echo.
)

REM Navigate to project directory
cd /d "%~dp0NuclearOptionCOILMod"

echo Building COIL Laser Mod...
echo.

REM Restore dependencies
echo [1/3] Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Failed to restore packages!
    exit /b 1
)
echo.

REM Build in Release mode
echo [2/3] Building in Release mode...
dotnet build -c Release
if errorlevel 1 (
    echo ERROR: Build failed!
    exit /b 1
)
echo.

REM Copy to plugins folder
echo [3/3] Copying to BepInEx plugins folder...
if not exist "%NUCLEAR_OPTION_DIR%\BepInEx\plugins" (
    echo Creating plugins folder...
    mkdir "%NUCLEAR_OPTION_DIR%\BepInEx\plugins"
)

copy /Y "bin\Release\net46\NuclearOptionCOILMod.dll" "%NUCLEAR_OPTION_DIR%\BepInEx\plugins\"
if errorlevel 1 (
    echo ERROR: Failed to copy DLL!
    exit /b 1
)
echo.

echo ========================================
echo Build Complete!
echo ========================================
echo.
echo DLL Location: %NUCLEAR_OPTION_DIR%\BepInEx\plugins\NuclearOptionCOILMod.dll
echo.
echo You can now launch Nuclear Option to test the mod.
echo Check BepInEx console for loading messages.
echo.
