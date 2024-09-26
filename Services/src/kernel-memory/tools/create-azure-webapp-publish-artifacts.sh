#!/usr/bin/env bash

# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT License.

set -e

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/"
cd "$HERE"

cd ../dotnet/Service

dotnet publish -c Release -o ./bin/Publish
