# [[Home]] > [[ChangeLog/2025-11-23_Fixes_and_Linking|Change Log]] > 2025-11-23: Fixes and Linking

> [!SUCCESS] Status: Completed
> **Date**: 2025-11-23
> **Author**: Antigravity

## 🚀 New Features
### Node Linking
- Added a **Link Handle** to the right side of every node.
- Implemented **Drag-to-Link** interaction:
    - Click and drag from the handle to create a connection.
    - A dashed line follows the mouse.
    - Releasing over another node creates a permanent link.
- Added `ConnectNodes` logic to prevent self-loops and duplicate links.

## 🐛 Bug Fixes
### Compilation Errors
- Fixed missing `ResizeTop`, `ResizeBottom`, etc. in `NodeView.axaml`.
- Fixed `SelectedObject` binding error in `MainWindowViewModel`.
- Fixed `StreamGeometry` namespace error in `CanvasView.axaml.cs`.

### Rendering Issues
- Fixed an issue where dots were not rendering on the full viewport.
    - **Cause**: `CanvasView` was creating a local `CanvasViewModel` instead of using the injected one.
    - **Fix**: Refactored `CanvasView` to use `DataContext` correctly and wire up viewport updates.

## 📝 Technical Details
- **New Converter**: `HalfConverter` added to vertically center the link handle.
- **Refactoring**: Moved `ConnectNodes` out of the constructor in `CanvasViewModel` (fixed a syntax error).

---
[[Home]]
