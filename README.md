# LangBiasTester

`LangBiasTester` is a specialized C# Windows Forms application designed for conducting psychoacoustic experiments focused on language bias. It provides a robust framework for managing audio stimuli, generating balanced trial sequences, and capturing high-precision participant responses.

## Key Features

- **Advanced Audio Processing**: leverages [NAudio](https://github.com/naudio/NAudio) for RMS normalization, real-time mixing, and static mix generation.
- **T1I1 Sequence Generation**: Implements a Type-1 Index-1 algorithm to ensure stimulus sequences are balanced and resistant to positional or order bias.
- **Dynamic Configuration**: All experiment parameters, including language ratios, trial counts, and UI layout, are controlled via an external JSON configuration.
- **Multi-Stage Experiment Flow**: Built-in support for multiple experiment stages: Instructions -> Calibration -> Trials -> Conclusion.
- **Structured Data Export**: Automatically exports participant data, trial results, and experiment settings to structured JSON files for downstream analysis.

---

## Software Architecture & Design

The project follows a modular, layered architecture to ensure maintainability and extensibility:

### 1. Infrastructure Layer
Contains the core domain models and management logic.
- **Managers**: `LangAudioManager`, `StimulusManager`, and `TrialManager` handle the lifecycle of audio resources and trial logic.
- **Models**: Defines the data structures for `Stimulus`, `Trial`, `ParticipantInfo`, and `Settings`.
- **Abstracts**: Provides base classes (`Experiment`, `Instructions`, `Audio`) that allow for high flexibility in implementing new experiment types.

### 2. Operators Layer
Handles the functional aspects and data flow.
- **T1I1 Algorithm**: A dedicated implementation for generating statistically sound stimulus sequences.
- **Data Handlers**: Utilities for settings extraction, file system management, and batch processing (e.g., `Util.cs`).
- **Input Handlers**: `KeyboardManager` for managing physical key mappings and response normalization across different keyboard layouts.

### 3. View Layer
Manages the user interface and interaction.
- **ExperimentView**: The primary WinForms container orchestrating the visual flow of the experiment.
- **FormManager**: Utility classes for dynamically generating UI elements (labels, grids, response buttons) based on configuration.
- **SettingsForm**: A developer/researcher interface for managing audio metadata and pre-generating static mixes.

---

## Project Structure

```text
LangBiasTester/
├── Infrastructure/       # Core models, managers, and abstractions
│   ├── Abstracts/        # Base classes for experiments, audio, and instructions
│   ├── Managers/         # Logic for trials, stimulus buffering, and audio mixing
│   └── Models/           # Data transfer objects and domain models
├── Operators/            # Functional logic and data handling
│   ├── DataHandlers/     # IO utilities and export logic
│   ├── InputHandlers/    # Keyboard and user input management
│   └── T1I1.cs           # Stimulus sequence generation algorithm
├── View/                 # WinForms UI components and management
├── config/               # Configuration files and audio assets
│   ├── Settings.json     # Main experiment configuration
│   └── audio/            # Directory for raw and mixed audio (.wav)
└── outputs/              # Exported JSON data from experiment runs
```

---

## Configuration

### Settings.json
The experiments behavior is governed by `config/Settings.json`. Key configurable areas include:
- **Languages**: Definition of languages, their associated keys, and primary/secondary designations.
- **Experiment Specs**: Number of trials, block size, and stimulus ratios.
- **Paths**: Relative paths to audio folders and output directories.

### Audio Assets
For the application to correctly parse and mix audio, the `config/audio/` directory must follow a specific hierarchy:
```text
config/audio/
├── {LangID}/             # e.g., EN, FR, CN
│   ├── Female/           # Female voice samples
│   └── Male/             # Male voice samples
└── Mixed/                # Pre-generated static mixes (optional)
```

---

## Getting Started

### Prerequisites
- **.NET 8.0 SDK** (Windows Desktop)
- **Visual Studio 2022** (recommended)

### Installation
1. Clone the repository.
2. Ensure [NAudio](https://www.nuget.org/packages/NAudio/) is restored via NuGet.
3. Place your audio stimuli in `config/audio/` following the required structure.
4. Update `config/Settings.json` with your experiment parameters.
5. Build and run the project.

---

## Data Export
Upon completion of an experiment, a JSON file is generated in the `outputs/` directory. This file contains:
- Participant demographics and language preferences.
- Full record of every trial (stimulus IDs, response times, confidence scores).
- A snapshot of the settings used for that specific run.

---

## License
This project is for academic research purposes.