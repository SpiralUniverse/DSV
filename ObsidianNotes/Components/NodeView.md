# [[Home]] > [[Components/CanvasView|Components]] > NodeView

> [!INFO] Purpose
> Visual representation of a [[Components/Node|Node]] in the graph.

## 🎨 Visuals
- **Shape**: Renders as a Rectangle, Circle, Triangle, or Hexagon based on `Node.Shape`.
- **Resize Handles**: Allows resizing the node from corners and edges.
- **Link Handle**: A small circle on the right side for creating connections.

## 💻 Code Analysis
### Resources
- `HalfConverter`: Used to vertically center the Link Handle.

### Interaction
- **Dragging**: Handled by Avalonia behaviors (or parent canvas).
- **Resizing**: Handled by specific border elements.
- **Linking**: The `LinkHandle` border triggers the linking process in [[Components/CanvasView|CanvasView]].

## 🔗 Relations
- Used by: [[Components/CanvasView|CanvasView]]
- Uses: [[Components/Node|Node]]

---
[[Components/CanvasView|CanvasView]] | [[Home]] | [[Components/LinkView|LinkView]]
