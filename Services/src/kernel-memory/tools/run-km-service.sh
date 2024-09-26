#!/usr/bin/env bash

# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT License.

set -e

cd "$(dirname "${BASH_SOURCE[0]:-$0}")"
cd ../service/Service

dotnet clean
dotnet build -c Debug -p "SolutionName=KernelMemory"
ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build --no-restore
