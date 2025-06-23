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

**Populating the Level Selection Screen with Level Data**

- The immediate focus is on dynamically generating the level selection UI using data from `LevelCard` assets.
- The `LevelManager` script is being refined to correctly instantiate and populate level cards, including level names, thumbnails, tooltips, and unlock states.

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
**Next Milestone:** Level selection screen population and integration

