#!/usr/bin/env bash
set -exuo pipefail

ps -A | grep godot | awk '{print $1}' | xargs kill
