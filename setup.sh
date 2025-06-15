#!/usr/bin/env bash
set -e

# Install dotnet SDK if not already installed
if ! command -v dotnet >/dev/null 2>&1; then
    echo "Installing .NET SDK..."
    # Add Microsoft package signing key and repository
    wget -q https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    sudo apt-get update
    sudo apt-get install -y dotnet-sdk-8.0
fi

# Restore NuGet packages
if [ -f "FaerieTables/FaerieTables.sln" ]; then
    dotnet restore FaerieTables/FaerieTables.sln
else
    dotnet restore
fi
