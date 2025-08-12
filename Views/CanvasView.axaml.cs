using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using DSV.ViewModels;
using System;
using DSV.Controls;
using DSV.Services;

namespace DSV.Views;

public partial class CanvasView : UserControl
{
    private CanvasViewModel? _viewModel;

    public CanvasView()
    {
        InitializeComponent();
        
        _viewModel = new();
        DataContext = _viewModel;
        
        // dotCanvas.PointerMoved += OnPointerMoved;
        this.SizeChanged += OnCanvasSizeChanged;
        
        // Wire up gravity field changes to canvas dirty regions
        _viewModel.GravityFieldsChanged += OnGravityFieldsChanged;
        
        // Set the canvas ViewModel reference for proper binding
        dotCanvas.ViewModel = _viewModel;
        
        // Ensure initial full render by marking all regions dirty
        dotCanvas.Loaded += (s, e) => dotCanvas.MarkAllRegionsDirty();
    }

    private void OnGravityFieldsChanged(object? sender, Rect affectedArea)
    {
        // Mark the affected area as dirty for surgical updates
        dotCanvas.MarkRegionsDirty(affectedArea);
    }

    private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.SetViewport(
                x: 0,
                y: 0,
                width: e.NewSize.Width,
                height: e.NewSize.Height
            );

            Console.WriteLine($"Canvas size changed: {e.NewSize.Width}x{e.NewSize.Height}");
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_viewModel == null) return;
        if(sender is Panel { Name: "PanelLayer" })
        {
            var position = e.GetPosition(this);
        
            // Calculate movement bounds for optimized invalidation
            var movementBounds = _viewModel.GetMouseMovementBounds(position.X, position.Y);
        
            _viewModel.UpdatePointer(position.X, position.Y);
        
            // Optimized invalidation - only redraw affected area instead of entire canvas
            dotCanvas.InvalidateRect(movementBounds);
        
        }
    }

}