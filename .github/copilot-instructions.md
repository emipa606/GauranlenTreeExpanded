# GitHub Copilot Instructions for Gauranlen Tree Expanded (Continued)

Welcome to the Gauranlen Tree Expanded (Continued) mod for RimWorld! This mod aims to update and enhance the original Gauranlen Tree mod, providing new features and systems for a more immersive and dynamic experience. Below are detailed instructions to help you understand the mod's architecture, coding patterns, and how to integrate with Copilot for development.

## Mod Overview and Purpose

The Gauranlen Tree Expanded (Continued) mod is a continuation and update of the `busted_bunnys` mod. It focuses on expanding the functionality of the Gauranlen Tree, allowing for more connections, replanting options, and new rituals to manage tree connections. The mod is designed to enhance the gameplay experience by providing more control and options for players.

## Key Features and Systems

### 1. **Multiple Connections per Tree**
   - **Description**: The mod allows up to 4 pawns to be connected to a single Gauranlen Tree, enabling shared responsibilities and benefits.
   - **Implementation**: This feature is primarily implemented through custom components and logic in C#.

### 2. **Replanting Gauranlen Trees**
   - **Description**: Players can replant Gauranlen Trees while retaining the existing connections, allowing for maintenance and base aesthetics.
   - **Implementation**: This involves modifying tree growth and replanting logic within the mod's C# files.

### 3. **Ritual to Tear Connection**
   - **Description**: A new ritual allows players to sever the connection between a tree and connected pawns, with potential consequences.
   - **Implementation**: This feature is defined in XML and implemented through C# logic for handling ritual outcomes.

### 4. **Dryad Protection Gizmo**
   - **Description**: A gizmo is added to protect Gauranlen Trees from enemies by summoning dryads to defend it.
   - **Implementation**: This involves creating new gizmos in C# and integrating them with the existing game logic.

## Coding Patterns and Conventions

### 1. **C# Structure**
   - **Components**: Custom components (e.g., `CompDryadCocoonExpanded`) are used to extend the functionality of game objects like trees and pawns.
   - **Gizmos**: New gizmos are created by extending the `IGizmo` interface and adding them to the game's gizmo list.
   - **Rituals**: Rituals are implemented using custom behavior and outcome effect classes, defined in XML and instantiated in C#.

### 2. **XML Integration**
   - **Def Files**: Key game elements like trees, jobs, duties, and rituals are defined in XML files.
   - **Patches**: Custom patches are applied using XML to modify existing game behavior.

### 3. **Harmony Patching**
   - **Purpose**: Harmony is used to patch existing game methods to add or modify behavior without altering the original codebase.
   - **Usage**: Patches are defined in XML and implemented in C#, using the Harmony library to apply changes.

## Suggestions for Copilot

### 1. **C# Development**
   - **Component Creation**: When creating new components, use Copilot to generate the basic structure and methods required (e.g., `CompInspectStringExtra`, `PostSpawnSetup`).
   - **Gizmo Implementation**: Use Copilot to generate the code for new gizmos, including their inspection strings and interaction logic.
   - **Ritual Logic**: When implementing new rituals, use Copilot to scaffold the behavior and outcome effect classes.

### 2. **XML Configuration**
   - **Def File Generation**: Use Copilot to generate the XML structure for new def files, including elements like `dutyDef`, `JobDef`, and `RitualBehaviorDef`.
   - **Patch Configuration**: Use Copilot to help draft the XML patches that modify existing game behavior.

### 3. **Harmony Patching**
   - **Patch Definition**: Use Copilot to generate the XML patch definitions that specify which methods to patch.
   - **Patch Implementation**: Use Copilot to scaffold the C# code that applies the Harmony patches.

## Conclusion

The Gauranlen Tree Expanded (Continued) mod is a comprehensive update to the original Gauranlen Tree mod, offering new features and systems for a more dynamic and immersive experience. By following the coding patterns and conventions outlined above, and leveraging the suggestions for Copilot, you can effectively contribute to and extend the mod's functionality.

Feel free to reach out to the mod's community or contributors for further assistance and support. Happy modding!

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

