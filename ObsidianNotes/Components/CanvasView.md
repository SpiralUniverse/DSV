# [[Home]] > [[Components/CanvasView|Components]] > CanvasView

> [!INFO] Purpose
> The main drawing surface for the application. It hosts the [[Components/DotCanvas|DotCanvas]], nodes, and links.

## 🎨 Visuals
- **Dot Grid**: Renders the background grid of dots.
- **Node Layer**: Hosts `NodeView` elements.
- **Link Layer**: Hosts `LinkView` elements.
- **Interaction**: Handles mouse events for panning, zooming (future), and linking.

## 💻 Code Analysis
### Properties
- `ViewModel`: The backing [[Components/CanvasViewModel|CanvasViewModel]].
- `TempLinkPath`: A `Path` element used to visualize the link connection while dragging.

### Methods
- `OnDataContextChanged`: Wires up the ViewModel and initializes the viewport.
- `OnPointerPressed`: Handles clicks on nodes (selection) and the Link Handle (linking).
- `OnPointerMoved`: Updates the temporary link path and optimized dot rendering.
- `OnPointerReleased`: Finalizes link creation.

## 🔗 Relations
- Used by: [[Components/MainWindow|MainWindow]]
- Uses: [[Components/CanvasViewModel|CanvasViewModel]], [[Components/DotCanvas|DotCanvas]], [[Components/NodeView|NodeView]]

---
[[Components/MainWindow|MainWindow]] | [[Home]] | [[Components/NodeView|NodeView]]
