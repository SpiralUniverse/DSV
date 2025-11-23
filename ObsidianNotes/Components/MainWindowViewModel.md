# [[Home]] > [[Components/CanvasView|Components]] > MainWindowViewModel

> [!INFO] Purpose
> The top-level ViewModel for the main window.

## 💻 Code Analysis
### Properties
- `CanvasViewModel`: The shared instance of the canvas logic.
- `SelectedObject`: Delegates to `CanvasViewModel.SelectedObject` to expose selection to the main window (e.g., for the inspector panel).

## 🔗 Relations
- Used by: [[Components/MainWindow|MainWindow]]
- Uses: [[Components/CanvasViewModel|CanvasViewModel]]

---
[[Components/CanvasViewModel|CanvasViewModel]] | [[Home]] | [[Components/Node|Node]]
