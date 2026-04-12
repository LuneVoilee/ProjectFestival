# Project Festival

中文说明: [README.md](README.md)

## Overview

`Project Festival` is a Unity-based 3D festival management simulation project.  
Players place buildings, manage money and happiness, unlock new areas, and scale up the event.

## Gameplay Highlights

- Grid-based building placement with live placement validation.
- Economy loop driven by visitor interactions (money and happiness changes).
- Progression system with level-based building unlocks and area unlocks.
- Map visualization modes: grid, occupied cells, and crowd/path traffic view.
- Visitor AI powered by `NavMesh + Context Steering + FSM`.
- Data-driven UI with panel/data binding and event-based decoupling.

## Tech Stack

- Unity `6000.2.8f1` (Unity 6)
- URP (`com.unity.render-pipelines.universal`)
- Input System (`com.unity.inputsystem`)
- AI Navigation (`com.unity.ai.navigation`)
- TextMeshPro / UGUI

## Getting Started

1. Open the project in Unity Hub (recommended Editor version: `6000.2.8f1`).
2. Open scene `Assets/Scenes/0.unity`.
3. Press Play and click `Start` in the main menu.

## Controls (Keyboard + Mouse)

- `W/A/S/D` or arrow keys: move camera
- `Q/E`: rotate camera
- Mouse wheel: zoom
- Left click building card, then left click terrain: place building
- Right click: cancel current building selection
- `ESC`: open settings panel
- Click dialogue panel to continue conversation

## Project Structure

```text
Assets/
  Scripts/
    Tool/       # Shared utility layer (singleton, FSM, reactive data)
    Core/       # Core systems (input, audio, global events, shared data)
    GamePlay/   # Gameplay systems (building, grid, area, value, AI, camera)
    UI/         # UI systems (panels, UI manager, AutoUIBinder)
  Data/         # ScriptableObject configs (buildings, conversations)
  Resources/    # Runtime-loaded resources (UI prefabs, audio)
  Scenes/       # Scenes (0: Main Menu, 1: In-Game)
```

## Assembly Architecture

- `Tool`: base utilities, no project assembly dependency
- `Core`: depends on `Tool`, provides input/audio/events/shared data
- `GamePlay`: depends on `Tool` + `Core`, contains game logic
- `UI`: depends on `Tool` + `Core`, contains interface logic

## Credits

- Context steering concept reference: Game AI Pro 2, Chapter 18
- Some models/assets are from public asset sources (for example Unity Asset Store and PolyPerfect)
