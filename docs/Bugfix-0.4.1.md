# David template edition 0.4.1

Fixed the rectangular terrain holes shown in the playtest screenshot. Converted transparent sprite materials retained depth writing, so invisible parts of decoration sprites prevented terrain behind them from drawing. A runtime compatibility pass disables depth writing for transparent URP 2D materials, including shared materials used by the kit's prefab pools.

The original material files and artwork stay intact. In the editor, original depth values are restored when leaving Play mode.

Also disabled the old GrabPass-based blur image in the native dialogue/pause UI. The existing panel, portrait, text and controls remain. The compatibility pass checks Images only, avoiding premature font-material creation on inactive TextMesh Pro submeshes during startup.

Corrected Samuel's return hint: finding his key first no longer claims all three keys are collected.

Validation:

- Begin receives real mouse input and opens the first chapter.
- A camera-image regression reproduces and removes the missing-terrain rectangle using the known faulty material.
- Native dialogue text renders; unsupported blur images stay disabled.
- Escape opens the native pause menu and resumes with player control restored.
- All seven chapters, physical pickups and transparent material states were checked.
- Native return travel preserves inventory, and key/supply/stone gates require the correct items.

Tests use player repositioning and native transition calls for route setup. They verify the reported rendering defect and the exercised flows; they do not establish that every possible gameplay bug is eliminated or replace a full human walkthrough of every route and boss.

Windows build: succeeded with zero errors (approximately 245 MB).
