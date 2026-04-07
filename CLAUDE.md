# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AR Fantasy Scavenger Hunt is a Unity AR mobile game using AR Foundation (ARKit/ARCore) where players collect magical items in augmented reality.

## Build & Run Commands

This is a Unity project — development is done in the Unity Editor. Key workflows:
- Open project in Unity 2022.3 LTS, load `Assets/_Project/Scenes/ARHunt.unity`
- Build to device: **File → Build Settings → Build And Run**
- Android: Minimum API Level 24 (Android 7.0), ARCore enabled in XR Plug-in Management
- iOS: Minimum iOS 14.0, ARKit enabled, requires camera permission description

## Architecture

### Script Organization (`Assets/_Project/Scripts/`)
- **Core/**: GameManager (singleton orchestrator), AudioManager, PlayerProgressManager
- **AR/**: ARSessionController (AR lifecycle), PlaneDetectionManager
- **Gameplay/**: ItemSpawner, AdvancedItemSpawner, CollectibleItem, CollectibleItemVariant, HuntManager, TouchInputHandler, VisualEffectsManager
- **UI/**: UIManager, HUDController, ARPlacementIndicator, CollectionJournalUI, HuntSelectionUI, AchievementsUI
- **Data/**: ItemData, HuntConfig, ItemDatabase (ScriptableObjects)

### Game Flow
1. Menu → Player taps "Start Hunt" or selects hunt from HuntSelectionUI
2. Scanning → AR detects planes via AR Plane Manager
3. Playing → Items spawn on planes, player taps to collect
4. Completed → All items collected or time expired, score displayed

### Key Patterns
- GameManager: Singleton that coordinates game state across all managers. Events: OnGameStateChanged, OnScoreChanged, OnItemCollected, OnTimeTick, OnTimeExpired
- ItemSpawner spawns collectibles on detected AR planes
- TouchInputHandler raycasts to detect taps on collectible layer
- HuntManager tracks item collection and win condition
- UIManager coordinates UI panels (menu, scanning, HUD, pause, win)
- PlayerProgressManager handles save/load, achievements, collection tracking
- ItemDatabase provides weighted random item selection by rarity

### Features
- **Time Limits**: HuntConfig.timeLimit enables timer; GameManager tracks time and fires OnTimeExpired
- **Achievements**: PlayerProgressManager.OnAchievementUnlocked event, AchievementsUI displays and shows popups
- **Collection Journal**: CollectionJournalUI shows discovered items with filters by rarity/discovered status
- **Rarity System**: Common → Uncommon → Rare → Epic → Legendary with weighted spawn rates
- **Progression**: Hunts unlock subsequent hunts on completion

## Customization

Create new collectible items:
1. Right-click → Create → AR Fantasy → Item Data
2. Configure name, points, prefab, effects in ScriptableObject
3. Add to Hunt Config or assign to ItemSpawner

## Editor Automation

The `Assets/_Project/Scripts/Editor/` folder contains tools to automate setup:

| Tool | Menu Path | Purpose |
|------|----------|---------|
| **SceneSetup** | Window → AR Fantasy → Setup Scene | Creates ARHunt scene with all managers |
| **CreateSampleScriptableObjects** | Window → AR Fantasy → Create Sample... | Creates ItemData, HuntConfig, ItemDatabase assets |
| **CreateCollectiblePrefabs** | Window → AR Fantasy → Create Crystal Prefab | Creates crystal prefab with material and particles |
| **BuildAutomation** | Window → AR Fantasy → Build → ... | Build APK/AAB for Android or iOS |

### Setup Workflow
1. Copy `Assets/_Project/` into your Unity project's Assets folder
2. In Unity: **Window → AR Fantasy → Setup Scene**
3. **Window → AR Fantasy → Create Sample ItemData**
4. **Window → AR Fantasy → Create Sample HuntConfigs**
5. **Window → AR Fantasy → Create Crystal Prefab**
6. Wire up prefab references in ItemSpawner and AdvancedItemSpawner
7. **Window → AR Fantasy → Build → Android (APK)**
