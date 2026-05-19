#!/bin/bash
set -e

# Build Release self-contained version for linux-x64
dotnet publish -c Release -r linux-x64 --self-contained true

rm -rf Fidus/publish/usr/

mkdir -p Fidus/publish/usr/bin/fidusterminal

# Copy the built files to Fidus/publish
cp Fidus/bin/Release/net10.0/linux-x64/publish/* Fidus/publish/usr/bin/fidusterminal/

# Rename the main executable to "fidus"
mv Fidus/publish/usr/bin/fidusterminal/Fidus Fidus/publish/usr/bin/fidusterminal/fidus

# Build the deb package
dpkg-deb --build Fidus/publish  Fidus/fidus.deb

# Create a zip archive of the deb package
zip -j Fidus/fidus.deb.zip Fidus/fidus.deb

echo "Build and packaging complete. Output: fidus.deb, fidus.deb.zip"