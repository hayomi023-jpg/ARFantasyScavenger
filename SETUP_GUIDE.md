# AR Fantasy Scavenger Hunt - Unity Setup Guide

## Prerequisites

- Unity Hub (latest version)
- Unity 2022.3 LTS (or newer)
- AR-compatible device (iOS or Android)

## Step 1: Create Unity Project

1. Open **Unity Hub**
2. Click **New Project**
3. Select **3D (URP)** or **3D Core** template
4. Name: `ARFantasyScavenger`
5. Choose location and create

## Step 2: Install Required Packages

1. Go to **Window → Package Manager**
2. Switch to **Unity Registry**
3. Install these packages:
   - `AR Foundation` (v5.1.0 or newer)
   - `ARCore XR Plugin` (for Android)
   - `ARKit XR Plugin` (for iOS)
   - `XR Plugin Management`
   - `TextMeshPro` (usually pre-installed)

## Step 3: Configure Project Settings

### XR Plug-in Management
1. Go to **Edit → Project Settings → XR Plug-in Management**
2. For **Android**: Check **ARCore**
3. For **iOS**: Check **ARKit**

### Android Settings (if building for Android)
1. Go to **File → Build Settings**
2. Switch to **Android** platform
3. Click **Player Settings**
4. Set **Minimum API Level** to **API Level 24 (Android 7.0)**
5. Under **XR Settings**, ensure **ARCore** is enabled

### iOS Settings (if building for iOS)
1. Go to **File → Build Settings**
2. Switch to **iOS** platform
3. Click **Player Settings**
4. Set **Target minimum iOS Version** to **14.0**
5. Set **Camera Usage Description**: "This app uses the camera for AR"

## Step 4: Import Project Files

1. Copy the `Assets/_Project` folder into your Unity project's `Assets` directory
2. Wait for Unity to import all assets
3. Check Console for any errors

## Step 5: Create AR Scene

### Create New Scene
1. Go to **File → New Scene**
2. Select **Basic (Built-in)** template
3. Save as `Assets/_Project/Scenes/ARHunt.unity`

### Scene Setup

#### 1. Delete Main Camera
- Select **Main Camera** in Hierarchy
- Press **Delete**

#### 2. Add XR Origin
1. Right-click in Hierarchy → **XR**
2. Select **XR Origin (Mobile AR)**
3. This creates:
   - XR Origin
   - Camera Offset
   - Main Camera (AR Camera)

#### 3. Add AR Session
1. Right-click in Hierarchy → **XR**
2. Select **AR Session**

#### 4. Add AR Plane Manager
1. Select **XR Origin** in Hierarchy
2. Click **Add Component**
3. Search for **AR Plane Manager**
4. Add it

#### 5. Add AR Raycast Manager
1. Still on **XR Origin**
2. Click **Add Component**
3. Search for **AR Raycast Manager**
4. Add it

#### 6. Create Game Managers Empty Object
1. Right-click in Hierarchy → **Create Empty**
2. Name it `=== MANAGERS ===`
3. Add child objects:

**GameManager** (child of MANAGERS)
```
- Add Component: GameManager (ARFantasy.Core)
- Add Component: AudioManager (ARFantasy.Core)
```

**ARSessionController** (child of MANAGERS)
```
- Add Component: ARSessionController (ARFantasy.AR)
- Assign in Inspector:
  - AR Session: (drag AR Session from Hierarchy)
  - AR Session Origin: (drag XR Origin from Hierarchy)
  - AR Plane Manager: (drag from XR Origin)
  - AR Raycast Manager: (drag from XR Origin)
```

**PlaneDetectionManager** (child of MANAGERS)
```
- Add Component: PlaneDetectionManager (ARFantasy.AR)
```

**ItemSpawner** (child of MANAGERS)
```
- Add Component: ItemSpawner (ARFantasy.Gameplay)
- Assign Collectible Prefabs: (see Step 6)
```

**HuntManager** (child of MANAGERS)
```
- Add Component: HuntManager (ARFantasy.Gameplay)
- Assign references in Inspector
```

**TouchInputHandler** (child of MANAGERS)
```
- Add Component: TouchInputHandler (ARFantasy.Gameplay)
- Set Layer Mask to include your collectible layer
```

#### 7. Create UI Canvas

1. Right-click → **UI → Canvas**
2. Set Render Mode to **Screen Space - Overlay**
3. Add Canvas Scaler component
4. Set UI Scale Mode: **Scale With Screen Size**
5. Reference Resolution: **1080 x 1920** (portrait mobile)

**Create UI Panels** (all as children of Canvas):

- **MainMenuPanel** (buttons: Start Hunt, Settings, Quit)
- **ScanningPanel** (text: "Scan the floor to find magical items...")
- **HUDPanel** (score, item counter, pause button)
- **PausePanel** (Resume, Restart, Main Menu buttons)
- **WinPanel** (Congratulations, Score, Play Again button)

Add **UIManager** script to Canvas and assign all panels.
Add **HUDController** script to HUDPanel.

#### 8. Create AR Visuals

Create empty object `=== AR VISUALS ===` with children:

**PlacementIndicator**
- Add Component: ARPlacementIndicator
- Create visual prefab (circular indicator on ground)

## Step 6: Create Collectible Prefab

### Create Crystal Model

1. Create **Cube** in scene
2. Scale to `(0.3, 0.5, 0.3)`
3. Rotate 45 degrees on Y
4. Add **Capsule Collider** (for better touch detection)

### Add Scripts

1. Add **CollectibleItem** component
2. Configure:
   - Item Name: "Mystic Crystal"
   - Point Value: 100
   - Float Amplitude: 0.1
   - Float Speed: 2

### Create Material

1. Create new Material in `Assets/_Project/Materials`
2. Name: `Crystal_Glow`
3. Shader: **Universal Render Pipeline/Lit**
4. Enable **Emission**
5. Set Emission Color: Bright Cyan `(0, 1, 1)`
6. Apply to crystal model

### Add Particle Effects

1. Add **Particle System** as child
2. Set to **Loop**
3. Emission: 10 particles/sec
4. Shape: Sphere, Radius 0.2
5. Color over Lifetime: Cyan to transparent
6. Save as prefab: `Assets/_Project/Prefabs/Crystal.prefab`

### Assign to ItemSpawner

1. Select **ItemSpawner** in Hierarchy
2. In Inspector, find "Collectible Prefabs"
3. Add element and drag Crystal prefab

## Step 7: Build and Test

### Android Build
1. **File → Build Settings**
2. Switch to **Android**
3. Connect Android device
4. Click **Build And Run**

### iOS Build
1. **File → Build Settings**
2. Switch to **iOS**
3. Click **Build**
4. Open generated Xcode project
5. Sign with Apple ID
6. Build to device

## Troubleshooting

### AR Not Working
- Ensure device supports ARCore/ARKit
- Check camera permissions
- Verify XR Plug-in Management settings

### Items Not Spawning
- Check AR Plane Manager is enabled
- Ensure floors are well-lit and textured
- Verify ItemSpawner has prefabs assigned

### Touch Not Working
- Check Collectible has Collider
- Verify TouchInputHandler Layer Mask
- Ensure items aren't behind UI

## Script Reference

All scripts are in `Assets/_Project/Scripts/`:

- **Core/**: GameManager, AudioManager
- **AR/**: ARSessionController, PlaneDetectionManager
- **Gameplay/**: CollectibleItem, ItemSpawner, HuntManager, TouchInputHandler
- **UI/**: UIManager, HUDController, ARPlacementIndicator
- **Data/**: ItemData (ScriptableObject), HuntConfig (ScriptableObject)

## Next Steps

1. Add more item types (create new ScriptableObjects)
2. Create different hunt configurations
3. Add save/load system (PlayerPrefs or JSON)
4. Add sound effects and music
5. Polish UI with animations
6. Add particle effects for collection
