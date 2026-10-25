# Technical specification

Vite + vanilla JavaScript + Canvas 2D. Fixed logical viewport 960×540. Horizontal camera follows David in a scrolling world. Game loop uses requestAnimationFrame. Keyboard input and basic touch controls are required; the current implementation is an early prototype and is not yet mobile-first.

## Planned refactor

Separate input, physics, combat, enemies, level data, rendering, UI and persistence modules. Add deterministic fixed-timestep updates, automated tests, collision tests, sprite animation and accessibility settings. CI must run npm install and npm run build.
