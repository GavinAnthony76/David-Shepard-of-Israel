# David: Shepherd of Israel

A Unity 2D exploration prototype built around the **2D Game Kit 5.0**. Search hidden routes, collect inventory markers, solve the original switches and gates, and travel between connected areas across seven David-themed chapters.

The current direction preserves the kit's artwork, animated player, dialogue canvas, menus, HUD, combat and puzzles. Generated character art, custom wildlife and extra UI overlays were removed because they clashed with the template. The kit's hero, portrait and mechanical final guardian remain placeholders; this is not a finished biblical art conversion.

## Open the Unity version

1. Use Unity **6000.5.2f1** and open the `UnityGame` folder.
2. Acquire/import Unity Technologies' free [2D Game Kit / 2D Sample Project](https://assetstore.unity.com/packages/templates/packs/2d-game-kit-2d-sample-project-107098), version **5.0**, through **Package Manager → My Assets**. Keep its `Assets/2DGamekit` path and original asset GUIDs. Import the complete kit so its settings and dependencies are available. Missing `Gamekit2D` compilation errors before import are expected.
3. Allow package resolution and script compilation to finish. Choose **David → Rebuild Template Campaign**.
4. Open `Assets/David/Scenes/Start.unity`, press Play, then **BEGIN**.

The repository stores our source, Unity settings and scene generator. The original Store package and generated scene copies are not redistributed here. Rebuilding creates the seven campaign scenes and rewritten phrase assets locally; the kit originals remain unchanged.

## Controls and progression

A/D move, Space jumps, S+Space drops through platforms, K/O use the kit's melee/ranged weapons after their pickup, E uses passages, Escape opens the native pause menu.

The ravine connects to the hollow and cistern. Find three keys and return to its central gate. Later searches require three supply markers and five stone markers. The final arena retains the kit's original guardian. All collectible types deliberately reuse the kit's glowing key artwork for visual consistency.

## Current evidence

Verified in Unity Play mode: native Start button, left/right/jump input, all seven scene loads, physical collectible triggers, persistent keys on return trips, required-item gates, native five-item HUD, and travel to the final arena. These checks reposition the player and invoke native transitions; they do not substitute for a complete human walkthrough or boss balance pass.

- [Play and route guide](docs/PLAY-David-Template-Edition.md)
- [Owned assets and free Store review](docs/Unity-Asset-Review.md)
- [Verification and limits](docs/Template-Verification.md)
- [0.4.1 rendering and UI fixes](docs/Bugfix-0.4.1.md)

`Tools/` contains Unity Pipeline editor scripts for verification and Windows building. They require a running Unity Editor; this Unity C# project is not a Node application that can run directly in Replit. Earlier web prototypes remain in their existing branches.

To build manually, use Unity's Windows build profile with the generated Start and seven chapter scenes, plus the kit's UIMenus and Loading scenes. The generator configures the scene list.
