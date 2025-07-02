# Tiny Walnut Games - Hidden Object Game Template

This repository contains the in-progress Hidden Object Game Template developed under the Tiny Walnut Games namespace. The project is structured for extensibility, localization, and modern Unity UI workflows, and is intended as a foundation for future game development.

## Project Overview

- **Namespace Organization:** All scripts and assets are organized under the `TinyWalnutGames` root namespace for clarity and scalability.
- **UI & UX:** Utilizes Unity UI Toolkit (UIElements) for modern, flexible user interfaces.
- **Localization:** Includes infrastructure for XLIFF-based localization, with an editor tool for converting XLIFF files to ScriptableObject tables.
- **Audio:** Integrates a modular audio manager for UI and gameplay sound effects.
- **TextMesh Pro:** Example scripts and assets for advanced text rendering and interaction.
- **Minigames:** Includes a sample "Cork Board" minigame with persistent progress and reset functionality.
- **Font Support:** Bundled with Noto Sans font families for multilingual support.

## Work In Progress (WIP) Status

This project is currently under active development. Major systems are scaffolded, but several features are incomplete or in a prototype state.

### Current Milestone

**Level Selection and First Playable Level**

- The level selection UI now displays all level cards and allows entering the first level.
- The first level spawns hidden objects in both the play area and the list of items to find.
- **Known Issues:**
  - Spawned hidden objects have correct locations, but their size and rotation are incorrect.
  - Clicking items in the "to find" list incorrectly logs them as found (should only respond to play area clicks).
  - Level selection and play area support mouse wheel scrolling, but dragging/swiping does not work yet.
  - The settings menu is not positioning itself correctly.

### Roadblock

The main roadblock to completing the core gameplay loop is finalizing the logic and data flow for the level selection screen. This includes:

- Ensuring level data is loaded and displayed correctly in the UI.
- Handling level unlock progression and user interaction.
- Integrating level selection with scene management and save state.

## Getting Started

1. **Unity Version:** This project targets Unity 2021.3 LTS or newer.
2. **.NET Target:** .NET Framework 4.7.1, C# 9.0.
3. **Open the Project:** Clone the repository and open it in Unity Hub.
4. **Localization:** Use the provided editor tool under `Tools > Localization > XLIFF to Scriptable Table Converter` to import localization files.
5. **Level Data:** Assign `LevelCard` assets in the `LevelManager` inspector to populate the level selection screen.

## Contributing

Contributions are welcome! Please open issues or pull requests for bug fixes, improvements, or new features.

## License

All code is provided under the MIT License. Font assets are included under their respective open licenses (see `/Fonts` for details).

---

**Status:** 🚧 Work In Progress  
**Finished Milestone:** Level selection screen population and integration
**Next Steps:** Finalize level selection logic, fix known issues, and polish first playable level.
**Next Milestone:** Complete core gameplay loop with level selection and first level interaction, unlocking the next level.
