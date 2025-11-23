using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Media;
using DSV.ViewModels;
using System;
using DSV.Controls;
using DSV.Services;
using DSV.Models;
using Avalonia.VisualTree;

namespace DSV.Views;

public partial class CanvasView : UserControl
{
    private CanvasViewModel? _viewModel;

    public CanvasView()
    {
        InitializeComponent();
        
        // dotCanvas.PointerMoved += OnPointerMoved;
        this.SizeChanged += OnCanvasSizeChanged;
        
        // Ensure initial full render by marking all regions dirty
        dotCanvas.Loaded += (s, e) => dotCanvas.MarkAllRegionsDirty();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is CanvasViewModel vm)
        {
            // Unsubscribe from old if needed (not strictly necessary if we assume one-time binding, but good practice)
            if (_viewModel != null)
            {
                _viewModel.GravityFieldsChanged -= OnGravityFieldsChanged;
            }

            _viewModel = vm;
            
            // Wire up gravity field changes to canvas dirty regions
            _viewModel.GravityFieldsChanged += OnGravityFieldsChanged;
            
            // Set the canvas ViewModel reference for proper binding
            dotCanvas.ViewModel = _viewModel;

            // Initialize viewport if we already have a size
            if (Bounds.Width > 0 && Bounds.Height > 0)
            {
                _viewModel.SetViewport(0, 0, Bounds.Width, Bounds.Height);
            }
        }
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

    private Node? _linkingSourceNode;
    private bool _isLinking;

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_viewModel == null) return;

        var position = e.GetPosition(this);

        if (_isLinking && _linkingSourceNode != null)
        {
            // Update temporary link visualization
            UpdateTempLink(_linkingSourceNode, position);
            return;
        }

        if(sender is Panel { Name: "PanelLayer" })
        {
            // Calculate movement bounds for optimized invalidation
            var movementBounds = _viewModel.GetMouseMovementBounds(position.X, position.Y);
        
            _viewModel.UpdatePointer(position.X, position.Y);
        
            // Optimized invalidation - only redraw affected area instead of entire canvas
            dotCanvas.InvalidateRect(movementBounds);
        
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_viewModel == null) return;

        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            var source = e.Source as Visual;
            
            // Check if we clicked the Link Handle
            if (source is Border { Name: "LinkHandle" })
            {
                var nodeView = FindAncestor<NodeView>(source);
                if (nodeView != null && nodeView.DataContext is Node node)
                {
                    _linkingSourceNode = node;
                    _isLinking = true;
                    e.Handled = true;
                    return;
                }
            }

            // Check if we clicked a Node
            // Traverse up visual tree to find NodeView
            var clickedNodeView = FindAncestor<NodeView>(source);
            if (clickedNodeView != null && clickedNodeView.DataContext is Node clickedNode)
            {
                _viewModel.SelectObject(clickedNode);
                e.Handled = true; // Prevent bubbling to canvas
            }
            else
            {
                // Clicked on empty space (or DotCanvas/PanelLayer)
                _viewModel.SelectObject(null);
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isLinking && _linkingSourceNode != null && _viewModel != null)
        {
            var source = e.Source as Visual;
            var targetNodeView = FindAncestor<NodeView>(source);
            
            if (targetNodeView != null && targetNodeView.DataContext is Node targetNode)
            {
                // Create the link
                _viewModel.ConnectNodes(_linkingSourceNode, targetNode);
            }

            // Reset linking state
            _isLinking = false;
            _linkingSourceNode = null;
            TempLinkPath.Data = null;
        }
    }

    private void UpdateTempLink(Node source, Point targetPoint)
    {
        var startPoint = new Point(source.PositionX + source.Width, source.PositionY + source.Height / 2);
        
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(startPoint, false);
            
            // Simple bezier curve
            var p1 = new Point(startPoint.X + 50, startPoint.Y);
            var p2 = new Point(targetPoint.X - 50, targetPoint.Y);
            
            context.CubicBezierTo(p1, p2, targetPoint);
            context.EndFigure(false);
        }
        
        TempLinkPath.Data = geometry;
    }

    private T? FindAncestor<T>(Visual? visual) where T : Visual
    {
        while (visual != null)
        {
            if (visual is T t) return t;
            visual = visual.GetVisualParent() as Visual;
        }
        return null;
    }
}