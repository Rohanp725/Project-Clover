#!/usr/bin/env bash
# usage: source AgentScripts/ev.sh ; ev '<C# statements ending in return ...;>' ; key E   (taps a key in the running game)
ev() { unity command script eval "$1" 2>&1 | tail -1 | cut -c1-400; }
key() {
  ev "UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.$1)); return \"down\";" >/dev/null
  ev "UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Keyboard.current, new UnityEngine.InputSystem.LowLevel.KeyboardState()); return \"up\";" >/dev/null
}
