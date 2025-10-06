# Project Asset Hierarchy

This folder contains the structured hierarchy for all gameplay content. Use the following conventions when adding new assets:

- **Art** – Visual assets such as textures, materials, 3D models, and animation clips. Sub-folders already exist for each category.
- **Audio** – Sound assets. Use `Music`, `SFX`, and `Voice` to separate long-form tracks, sound effects, and dialogue/voice-over.
- **Prefabs** – Reusable GameObjects and prefab variants.
- **Scenes** – Unity scenes. Existing sample scenes have been moved here.
- **Scripts** – Runtime code, split into `Core`, `Gameplay`, and `UI` layers. Create additional folders beneath these as needed to keep systems organised.
- **Shaders** – Shader Graphs and shader source files.
- **UI** – Interface assets. Use `Fonts` and `Sprites` for typography and 2D artwork.
- **VFX** – Visual effect graphs, particle systems, and related assets.

Create new sub-folders within the appropriate category if a system grows large (for example, `Scripts/Gameplay/Enemies`). Keeping this structure consistent will make navigation easier for the entire team.
