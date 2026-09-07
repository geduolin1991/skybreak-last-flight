#!/bin/zsh
TASK_ROOT="${0:A:h}"
open -a '/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app' --args -projectPath "$TASK_ROOT" -openfile "$TASK_ROOT/Assets/Scenes/Skybreak.unity"
