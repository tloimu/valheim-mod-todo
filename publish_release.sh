#!/bin/bash

PACKAGE=ValheimModToDo/Package
PLUGINS=ValheimModToDo/Package/plugins
DLL=$PLUGINS/ValheimModToDo.dll
FILES=( "$PACKAGE/$README" "$PACKAGE/CHANGELOG.md" "$PACKAGE/icon.png" "$PACKAGE/manifest.json" )
#TRANSLATIONS=Translations

VERSION=$1

# Check that source files exist and are readable
if [ ! -f "$DLL" ]; then
    echo "Error: $DLL does not exist or is not readable."
    exit 1
fi

# Check that target directory exists and is writable
if [ ! -d "$PLUGINS" ]; then
    echo "Error: $PLUGINS directory does not exist."
    exit 1
fi

if [ ! -w "$PLUGINS" ]; then
    echo "Error: $PLUGINS directory is not writable."
    exit 1
fi

cp ${FILES[@]} $PLUGINS
#cp -rf "$TRANSLATIONS" "$PLUGINS/"  || { echo "Error: Failed to copy Translations"; exit 1; }

ZIPDESTINATION="valheim-mod-todo-list.$VERSION.zip"

cd "$PLUGINS"
if [ ! -z "$VERSION" ]; then
    VERSION=".$VERSION"
fi

zip -r "../$ZIPDESTINATION" .
