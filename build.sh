#!/bin/bash
# Build script for Nuclear Option COIL Laser Mod (Linux/Mac)

echo "========================================"
echo "Nuclear Option COIL Laser Mod Builder"
echo "========================================"
echo ""

# Check if NUCLEAR_OPTION_DIR is set
if [ -z "$NUCLEAR_OPTION_DIR" ]; then
    echo "ERROR: NUCLEAR_OPTION_DIR environment variable not set!"
    echo ""
    echo "Please set it to your Nuclear Option installation directory:"
    echo "  export NUCLEAR_OPTION_DIR=\"\$HOME/.steam/steam/steamapps/common/Nuclear Option\""
    echo ""
    echo "Add it to your ~/.bashrc or ~/.zshrc to make it permanent."
    exit 1
fi

echo "Game Directory: $NUCLEAR_OPTION_DIR"
echo ""

# Check if game directory exists
if [ ! -d "$NUCLEAR_OPTION_DIR" ]; then
    echo "ERROR: Game directory does not exist!"
    echo "Path: $NUCLEAR_OPTION_DIR"
    echo ""
    echo "Please verify your NUCLEAR_OPTION_DIR environment variable."
    exit 1
fi

# Check if BepInEx is installed
if [ ! -d "$NUCLEAR_OPTION_DIR/BepInEx" ]; then
    echo "WARNING: BepInEx folder not found!"
    echo "Please install BepInEx before using this mod."
    echo ""
    echo "Download from: https://github.com/BepInEx/BepInEx/releases"
    echo ""
    read -p "Press Enter to continue..."
fi

# Navigate to project directory
cd "$(dirname "$0")/NuclearOptionCOILMod"

echo "Building COIL Laser Mod..."
echo ""

# Restore dependencies
echo "[1/3] Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to restore packages!"
    exit 1
fi
echo ""

# Build in Release mode
echo "[2/3] Building in Release mode..."
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo "ERROR: Build failed!"
    exit 1
fi
echo ""

# Copy to plugins folder
echo "[3/3] Copying to BepInEx plugins folder..."
if [ ! -d "$NUCLEAR_OPTION_DIR/BepInEx/plugins" ]; then
    echo "Creating plugins folder..."
    mkdir -p "$NUCLEAR_OPTION_DIR/BepInEx/plugins"
fi

cp -f "bin/Release/net46/NuclearOptionCOILMod.dll" "$NUCLEAR_OPTION_DIR/BepInEx/plugins/"
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to copy DLL!"
    exit 1
fi
echo ""

echo "========================================"
echo "Build Complete!"
echo "========================================"
echo ""
echo "DLL Location: $NUCLEAR_OPTION_DIR/BepInEx/plugins/NuclearOptionCOILMod.dll"
echo ""
echo "You can now launch Nuclear Option to test the mod."
echo "Check BepInEx console for loading messages."
echo ""
