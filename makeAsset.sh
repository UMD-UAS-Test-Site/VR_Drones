#!/bin/bash

UNITY_EXEC="/mnt/d/Program Files/Editor"
VIDEO_LOCATION="/mnt/d/Files/DroneSwarm/Video"
export UNITY_ASSET_BUNDLE_PATH="/mnt/d/Files/DroneSwarm/Assets"

name="tmp"

${UNITY_EXEC} -batchmode -quit -createProject
