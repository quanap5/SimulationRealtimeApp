#!/bin/bash

echo "====================================="
echo "Real-Time Simulation Web Application"
echo "====================================="
echo ""

echo "Checking .NET SDK..."
if ! command -v dotnet &> /dev/null
then
    echo "ERROR: .NET SDK not found!"
    echo "Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

dotnet --version
echo ""

echo "Restoring packages..."
dotnet restore

echo ""
echo "Building application..."
dotnet build

echo ""
echo "Starting application..."
echo ""
echo "The application will be available at:"
echo "  - HTTP:  http://localhost:5000"
echo "  - HTTPS: https://localhost:7001"
echo ""
echo "Open your browser to https://localhost:7001/index.html"
echo ""
echo "Press Ctrl+C to stop the application"
echo ""

dotnet run
