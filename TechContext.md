# Technical Context: [Your Game Name]

## Development Environment
- **Unity Version:** Unity 2022.3.33f1 LTS
- **Scripting Backend:** Mono (Development) / IL2CPP (Production build target)
- **C# Version:** C# 9.0 (Unity 2022.3 default)
- **Render Pipeline:** Built-in Render Pipeline / Universal Render Pipeline (URP)

## Core Architectural Systems
- **Input System:** "Classic Input Manager (Input.GetAxis)"
- **UI System:** TextMeshPro (UGUI Canvas)
- **Physics Layout:** Standard Unity 3D Physics / 2D Physics

## Code Architecture & Modularisation Rules
1. **Single Responsibility Principle (SRP):** Scripts must do one thing well (e.g., input handles input, movement handles movement, health handles health). Avoid monolithic mega-scripts.
2. **Component Lifecycle Checks:** Always include null-safety checks or `RequireComponent` attributes for dependencies. Cache `GetComponent` references in `Awake()` or `Start()`; never call them inside `Update()`.
3. **No Unity 6 Features:** Do not use modern `Awaitable` syntax or features introduced in Unity 6. Stick to stable `Coroutines` (`IEnumerator`) for asynchronous timing.
4. **Naming Conventions:** Use PascalCase for public methods/classes and camelCase (or `_camelCase`) for private fields.