# [[Home]] > [[Architecture/MVVM_Pattern|Architecture]] > Project Structure

> [!INFO] Purpose
> Overview of the file and folder organization in the DSV project.

## 📂 Directory Layout

### `/Views`
Contains the UI definitions (`.axaml`) and code-behind (`.axaml.cs`).
- [[Components/MainWindow|MainWindow]]: The main application window.
- [[Components/CanvasView|CanvasView]]: The primary drawing surface.
- [[Components/NodeView|NodeView]]: Visual representation of a node.
- [[Components/LinkView|LinkView]]: Visual representation of a link.

### `/ViewModels`
Contains the application logic and state.
- [[Components/MainWindowViewModel|MainWindowViewModel]]: Manages the main window state.
- [[Components/CanvasViewModel|CanvasViewModel]]: Manages the canvas, nodes, and links.
- `ViewModelBase`: Base class for all ViewModels.

### `/Models`
Contains the data structures.
- [[Components/Node|Node]]: Represents a graph node.
- [[Components/Link|Link]]: Represents a connection between nodes.
- `Dot`: Represents a background grid dot.

### `/Controls`
Custom UI controls.
- [[Components/DotCanvas|DotCanvas]]: Optimized canvas for rendering thousands of dots.

### `/Services`
Helper services (e.g., `FieldEffectManager`).

---
[[Architecture/MVVM_Pattern|MVVM Pattern]] | [[Home]]
