# Baked

A Unity game created for GMTK 2025 GameJam, focused on surviving the messy day and night loop life of our protagonist.

## Overview

Baked is a small gameplay loop where the player moves through a day/night cycle and completes tasks in two different environments: the house and the club. Each task has a countdown, and the player must keep up with the increasin speed of the loop to avoid failure. 

## Core Gameplay

- Manage task bars in different locations.
- Complete required actions before counters reach zero.
- Survive the loop long enough to keep progressing without triggering a fail state.

## Controls

- P: Interact with objects.
- F: Restart after a defeat state.

## Where to Play

You can play the game here: https://fenrirspk.itch.io/baked

## Project Highlights

- **ScriptableObjects(SO):** The project uses Unity ScriptableObjects to separate reusable data from gameplay logic and keep tuning easy to manage.

- **Custom Editor Scripts:** The project also includes editor tooling to improve inspector usability and workflow.

## Repository Structure

```text
Assets/
  Scripts/
    ClubEnterTriggerBehavior.cs
    Enums.cs
    GameManagerBehavior.cs
    Helpers.cs
    SO_GameResources.cs
    SO_GameSettings.cs
    TriggerBehavior.cs
    Editor/
      GameManagerEditor.cs
```

## Getting Started

1. Open the project in Unity.
2. Load the main scene.
3. Press Play to test it.
4. Use the Inspector to adjust ScriptableObject values and tune gameplay balance.

## Future Improvements

This project is still in development, and there are several areas that will be improved in future iterations, including:

- Code cleanup and refactoring
- More robust state management
- Implementation of new Unity's new UI system
- Implementation of new Input System Package.
- More polished transitions and feedback