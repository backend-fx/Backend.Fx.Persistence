#!/usr/bin/env bash

dotnet tool install cake.tool
dotnet cake ./build/build.cake "$@"