#!/usr/bin/env bash

# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT License.

set -e

cd "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/"
cd ../service/Service

dotnet clean
dotnet build -c Debug -p "SolutionName=KernelMemory"
ASPNETCORE_ENVIRONMENT=Development dotnet run setup --no-build --no-restore
