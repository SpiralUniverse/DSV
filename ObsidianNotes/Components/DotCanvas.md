# [[Home]] > [[Components/CanvasView|Components]] > DotCanvas

> [!INFO] Purpose
> A highly optimized custom control for rendering thousands of background dots.

## 🎨 Visuals
- Renders a grid of dots.
- Supports "dirty region" rendering to only redraw changed areas.

## 💻 Code Analysis
### Properties
- `ViewModel`: Reference to [[Components/CanvasViewModel|CanvasViewModel]].

### Optimization
- **Dirty Regions**: The canvas is divided into a 20x20 grid of regions. Only regions marked as "dirty" are redrawn during a render pass.
- **Viewport Culling**: Only dots within the visible viewport are considered for rendering.

## 🔗 Relations
- Used by: [[Components/CanvasView|CanvasView]]
- Uses: [[Components/CanvasViewModel|CanvasViewModel]]

---
[[Components/Link|Link]] | [[Home]]
