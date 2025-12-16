#!/usr/bin/env bash
set -euo pipefail

find . -type d \( -name "node_modules" -o -name ".git" \) -prune -o -type d \( -name "bin" -o -name "obj" \) -prune -exec rm -rf {} +
