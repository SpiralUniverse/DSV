# [[Home]] > [[Architecture/MVVM_Pattern|Architecture]] > MVVM Pattern

> [!INFO] Purpose
> Explains how the Model-View-ViewModel (MVVM) pattern is implemented in DSV using the CommunityToolkit.Mvvm library.

## 🏗️ Overview
DSV strictly follows the MVVM pattern to separate UI logic from business logic.

### 📦 Components
1. **Models**: Pure data classes (e.g., [[Components/Node|Node]], [[Components/Link|Link]]). They implement `ObservableObject` to notify changes.
2. **ViewModels**: The glue between Views and Models (e.g., [[Components/CanvasViewModel|CanvasViewModel]]). They expose properties and commands.
3. **Views**: The UI definitions (AXAML) and code-behind (e.g., [[Components/CanvasView|CanvasView]]). They bind to ViewModels.

## 🔄 Data Flow
1. **Binding**: Views bind to ViewModel properties using `{Binding PropertyName}`.
2. **Notification**: Models and ViewModels raise `PropertyChanged` events when data changes.
3. **Commands**: User actions (clicks, etc.) are often handled via Commands or event handlers that delegate to the ViewModel.

> [!TIP] CommunityToolkit.Mvvm
> We use source generators like `[ObservableProperty]` to reduce boilerplate code.

---
[[Home]] | [[Architecture/Project_Structure|Project Structure]]
