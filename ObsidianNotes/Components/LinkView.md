# [[Home]] > [[Components/CanvasView|Components]] > LinkView

> [!INFO] Purpose
> Visual representation of a [[Components/Link|Link]] between two nodes.

## 🎨 Visuals
- **Path**: A cubic bezier curve connecting the source and target nodes.
- **Color**: Uses the `AccentBrush` resource.

## 💻 Code Analysis
### Methods
- `UpdateLink`: Calculates the start and end points based on node positions and shapes.
- `GetIntersectionPoint`: Calculates where the link should attach to the node boundary (supporting Rectangles and Circles).

## 🔗 Relations
- Used by: [[Components/CanvasView|CanvasView]]
- Uses: [[Components/Link|Link]], [[Components/Node|Node]]

---
[[Components/NodeView|NodeView]] | [[Home]] | [[Components/CanvasViewModel|CanvasViewModel]]
