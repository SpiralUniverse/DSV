# [[Home]] > [[Components/CanvasView|Components]] > Node

> [!INFO] Purpose
> Data model representing a node in the graph.

## 💻 Code Analysis
### Properties
- `Title`: The display name of the node.
- `PositionX`, `PositionY`: Coordinates on the canvas.
- `Width`, `Height`: Dimensions of the node.
- `Shape`: Enum (`Rectangle`, `Circle`, `Triangle`, `Hexagon`).
- `Color`: Hex color string.
- `IsSelected`: Selection state.

### Helpers
- `IsRectangle`, `IsCircle`, etc.: Boolean helpers for XAML bindings.

## 🔗 Relations
- Used by: [[Components/CanvasViewModel|CanvasViewModel]], [[Components/NodeView|NodeView]], [[Components/Link|Link]]

---
[[Components/MainWindowViewModel|MainWindowViewModel]] | [[Home]] | [[Components/Link|Link]]
