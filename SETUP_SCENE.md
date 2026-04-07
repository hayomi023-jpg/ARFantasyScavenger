# Unity Scene Setup Guide

This guide walks through creating the ARHunt scene from scratch.

## Step 1: Create New Scene

1. Open Unity Editor
2. **File → New Scene → Basic (Built-in)**
3. Save immediately: **File → Save As** → `Assets/_Project/Scenes/ARHunt.unity`
4. Delete **Main Camera** from Hierarchy (AR Camera replaces it)

---

## Step 2: Set Up XR AR Foundation

### Add XR Origin (Mobile AR)
1. Right-click in Hierarchy → **XR → XR Origin (Mobile AR)**
2. This creates:
   - `XR Origin` (root)
     - `Camera Offset`
       - `Main Camera` (AR Camera)

### Add AR Session
1. Right-click in Hierarchy → **XR → AR Session**

### Add AR Plane Manager
1. Select `XR Origin` in Hierarchy
2. **Add Component** → Search `AR Plane Manager` → Add

### Add AR Raycast Manager
1. Still on `XR Origin`
2. **Add Component** → Search `AR Raycast Manager` → Add

---

## Step 3: Create Manager GameObjects

Create an empty GameObject `=== MANAGERS ===` (with the === markers for organization)

Create these children under MANAGERS:

### GameManager
1. Create empty child named `GameManager`
2. Add Component: `GameManager (ARFantasy.Core)`

### AudioManager
1. Create empty child named `AudioManager`
2. Add Component: `AudioManager (ARFantasy.Core)`

### PlayerProgressManager
1. Create empty child named `PlayerProgressManager`
2. Add Component: `PlayerProgressManager (ARFantasy.Core)`

### ARSessionController
1. Create empty child named `ARSessionController`
2. Add Component: `ARSessionController (ARFantasy.AR)`
3. Wire up Inspector references:
   - AR Session: (drag AR Session from Hierarchy)
   - AR Session Origin: (drag XR Origin)
   - AR Plane Manager: (drag from XR Origin)
   - AR Raycast Manager: (drag from XR Origin)

### PlaneDetectionManager
1. Create empty child named `PlaneDetectionManager`
2. Add Component: `PlaneDetectionManager (ARFantasy.AR)`

### ItemSpawner
1. Create empty child named `ItemSpawner`
2. Add Component: `ItemSpawner (ARFantasy.Gameplay)`

### AdvancedItemSpawner
1. Create empty child named `AdvancedItemSpawner`
2. Add Component: `AdvancedItemSpawner (ARFantasy.Gameplay)`
3. Wire up ItemDatabase reference when created

### HuntManager
1. Create empty child named `HuntManager`
2. Add Component: `HuntManager (ARFantasy.Gameplay)`
3. Wire up Inspector references:
   - Item Spawner: (drag ItemSpawner)
   - Advanced Item Spawner: (drag AdvancedItemSpawner)
   - Plane Detection Manager: (drag PlaneDetectionManager)

### TouchInputHandler
1. Create empty child named `TouchInputHandler`
2. Add Component: `TouchInputHandler (ARFantasy.Gameplay)`
3. Set Layer Mask for collectibles

---

## Step 4: Create UI Canvas

1. Right-click → **UI → Canvas**
2. Select Canvas, set **Render Mode**: `Screen Space - Overlay`
3. Add **Canvas Scaler** component:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1080 x 1920` (portrait mobile)

### Add UIManager
1. Add `UIManager (ARFantasy.UI)` to Canvas
2. Wire up all panel references in Inspector

### Create UI Panels (children of Canvas)

Create empty children under Canvas:

| Panel Name | Purpose |
|------------|---------|
| `MainMenuPanel` | Start Hunt, Settings, Quit buttons |
| `ScanningPanel` | "Scan the floor..." text |
| `HUDPanel` | Score, items counter, pause button |
| `PausePanel` | Resume, Restart, Main Menu buttons |
| `WinPanel` | Congratulations, score, play again |

Wire each panel to UIManager in Inspector.

### Add HUD Controller
1. Add `HUDController (ARFantasy.UI)` to HUDPanel
2. Wire up TextMeshProUGUI references (scoreText, itemsText, timerText)

---

## Step 5: Create Hunt Selection UI

1. Create empty child under Canvas named `HuntSelectionPanel`
2. Add `HuntSelectionUI (ARFantasy.UI)` component
3. Wire up references in Inspector:
   - Available Hunts: (assign HuntConfig ScriptableObjects)
   - Progress Manager: (drag PlayerProgressManager)
   - Hunt Manager: (drag HuntManager)

---

## Step 6: Create Collection Journal UI

1. Create empty child under Canvas named `CollectionJournalPanel`
2. Add `CollectionJournalUI (ARFantasy.UI)` component
3. Wire up references in Inspector

---

## Step 7: Create AR Visuals

Create empty `=== AR VISUALS ===` parent:

### Placement Indicator
1. Create empty child named `PlacementIndicator`
2. Add `ARPlacementIndicator (ARFantasy.UI)`
3. Create visual prefab (circular ground indicator)

---

## Step 8: Create Collectible Prefab

1. Create **Cube** in scene
2. Scale: `(0.3, 0.5, 0.3)`
3. Rotate 45° on Y
4. Add **Capsule Collider**
5. Add **CollectibleItem** component
6. Configure: Item Name, Point Value, Float settings

### Create Material
1. Create Material in `Assets/_Project/Materials/`
2. Name: `Crystal_Glow`
3. Shader: `Universal Render Pipeline/Lit`
4. Enable **Emission**, set Color: Cyan `(0, 1, 1)`
5. Apply to crystal model

### Add Particle Effects
1. Add **Particle System** as child
2. Set Loop on, Emission: 10/sec, Shape: Sphere
3. Save as prefab: `Assets/_Project/Prefabs/Crystal.prefab`

### Assign to Spawners
1. Select ItemSpawner in Hierarchy
2. Add Crystal.prefab to Collectible Prefabs array
3. Same for AdvancedItemSpawner

---

## Step 9: Create ScriptableObjects

### ItemDatabase
1. Right-click → **Create → AR Fantasy → Item Database**
2. Name: `ItemDatabase`
3. Populate `All Items` list with ItemData assets

### ItemData Assets
Create one for each item type:
1. Right-click → **Create → AR Fantasy → Item Data**
2. Configure: itemId, displayName, pointValue, rarity, modelPrefab

### HuntConfig Assets
Create hunts with unlock chain:
1. Right-click → **Create → AR Fantasy → Hunt Config**
2. Configure: huntId, huntName, itemCount, timeLimit, unlocksHunt

---

## Step 10: Build Settings

### Android
1. **File → Build Settings → Android → Switch Platform**
2. **Player Settings**:
   - Minimum API Level: **API Level 24**
   - XR Settings → ARCore: **Enabled**
3. Connect device → **Build And Run**

### iOS
1. **File → Build Settings → iOS → Switch Platform**
2. **Player Settings**:
   - Target minimum iOS Version: **14.0**
   - Camera Usage Description: "This app uses the camera for AR"
3. Click **Build** → Open in Xcode → Sign and run

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| AR not working | Check device supports ARCore/ARKit |
| Items not spawning | Verify AR Plane Manager enabled, ItemSpawner has prefabs |
| Touch not detecting | Check Collectible has Collider, TouchInputHandler layer mask |
| NullReference errors | Wire up all SerializeField references in Inspector |
