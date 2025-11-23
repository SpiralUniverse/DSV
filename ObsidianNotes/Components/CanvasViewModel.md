# [[Home]] > [[Components/CanvasView|Components]] > CanvasViewModel

> [!INFO] Purpose
> The brain of the canvas. Manages the state of nodes, links, and the background grid.

## 💻 Code Analysis
### Properties
- `Nodes`: Collection of [[Components/Node|Node]] objects.
- `Links`: Collection of [[Components/Link|Link]] objects.
- `SelectedObject`: The currently selected item (Node or global settings).
- `Circles`: Collection of background circles (for effects).

### Methods
- `ConnectNodes(Node source, Node target)`: Creates a new link between two nodes if valid.
- `SelectObject(object? obj)`: Updates the selection state.
- `SetViewport`: Updates the visible area for dot optimization.
- `UpdateVisibleDots`: Calculates which dots should be rendered.

## 🔗 Relations
- Used by: [[Components/CanvasView|CanvasView]], [[Components/MainWindowViewModel|MainWindowViewModel]]
- Uses: [[Components/Node|Node]], [[Components/Link|Link]]

---
[[Components/LinkView|LinkView]] | [[Home]] | [[Components/MainWindowViewModel|MainWindowViewModel]]
