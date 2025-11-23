using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DSV.Models;

namespace DSV.Views;

public partial class NodeView : UserControl
{
    private bool _isDragging;
    private bool _isResizing;
    private ResizeMode _resizeMode = ResizeMode.None;
    
    // Avalonia Thumb pattern - store initial states
    private Point _dragStartPosition;
    private Point _initialNodePosition;
    private Size _initialNodeSize;
    
    public NodeView()
    {
        InitializeComponent();
        
        // Add event handlers for mouse interactions
        this.PointerPressed += OnPointerPressed;
        this.PointerMoved += OnPointerMoved;
        this.PointerReleased += OnPointerReleased;
        
        // Add event handlers for resize handles
        SetupResizeHandles();
    }

    private void SetupResizeHandles()
    {
        // Corner handles
        ResizeTopLeft.PointerPressed += (s, e) => StartResize(ResizeMode.TopLeft, e);
        ResizeTopRight.PointerPressed += (s, e) => StartResize(ResizeMode.TopRight, e);
        ResizeBottomLeft.PointerPressed += (s, e) => StartResize(ResizeMode.BottomLeft, e);
        ResizeBottomRight.PointerPressed += (s, e) => StartResize(ResizeMode.BottomRight, e);
        
        // Edge handles
        ResizeTop.PointerPressed += (s, e) => StartResize(ResizeMode.Top, e);
        ResizeBottom.PointerPressed += (s, e) => StartResize(ResizeMode.Bottom, e);
        ResizeLeft.PointerPressed += (s, e) => StartResize(ResizeMode.Left, e);
        ResizeRight.PointerPressed += (s, e) => StartResize(ResizeMode.Right, e);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not Node node) return;
        
        // Get parent canvas position for proper calculation
        var canvasPosition = e.GetPosition(this.Parent as Canvas);
        
        // Store initial states (Avalonia Thumb pattern)
        _dragStartPosition = canvasPosition;
        _initialNodePosition = new(node.PositionX, node.PositionY);
        _initialNodeSize = new(node.Width, node.Height);
        
        // Select the node
        node.IsSelected = true;
        
        // Start dragging if clicking on the main area (not resize handles)
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !_isResizing)
        {
            _isDragging = true;
            node.IsDragging = true;
            this.Cursor = new Cursor(StandardCursorType.DragMove);
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not Node node) return;
        
        if (_isDragging)
        {
            // Avalonia Thumb pattern: calculate delta from initial position
            var currentPosition = e.GetPosition(this.Parent as Canvas);
            var deltaX = currentPosition.X - _dragStartPosition.X;
            var deltaY = currentPosition.Y - _dragStartPosition.Y;
            
            // Update position based on initial position + delta (avoids accumulation errors)
            node.PositionX = _initialNodePosition.X + deltaX;
            node.PositionY = _initialNodePosition.Y + deltaY;
            
            e.Handled = true;
        }
        else if (_isResizing)
        {
            // Resize using delta calculation
            var currentPosition = e.GetPosition(this.Parent as Canvas);
            var deltaX = currentPosition.X - _dragStartPosition.X;
            var deltaY = currentPosition.Y - _dragStartPosition.Y;
            
            ResizeNode(node, deltaX, deltaY);
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is not Node node) return;
        
        if (_isDragging)
        {
            _isDragging = false;
            node.IsDragging = false;
            this.Cursor = new Cursor(StandardCursorType.Arrow);
        }
        
        if (_isResizing)
        {
            _isResizing = false;
            node.IsResizing = false;
            _resizeMode = ResizeMode.None;
            this.Cursor = new Cursor(StandardCursorType.Arrow);
        }
        
        e.Handled = true;
    }

    private void StartResize(ResizeMode mode, PointerPressedEventArgs e)
    {
        if (DataContext is not Node node) return;
        
        _isResizing = true;
        _resizeMode = mode;
        node.IsResizing = true;
        
        // Store initial state for resize operations
        var canvasPosition = e.GetPosition(this.Parent as Canvas);
        _dragStartPosition = canvasPosition;
        _initialNodePosition = new(node.PositionX, node.PositionY);
        _initialNodeSize = new(node.Width, node.Height);
        
        e.Handled = true;
    }

    private void ResizeNode(Node node, double deltaX, double deltaY)
    {
        const double minWidth = 50;
        const double minHeight = 30;

        switch (_resizeMode)
        {
            case ResizeMode.TopLeft:
                var newWidth = _initialNodeSize.Width - deltaX;
                var newHeight = _initialNodeSize.Height - deltaY;
                if (newWidth >= minWidth)
                {
                    node.Width = newWidth;
                    node.PositionX = _initialNodePosition.X + deltaX;
                }
                if (newHeight >= minHeight)
                {
                    node.Height = newHeight;
                    node.PositionY = _initialNodePosition.Y + deltaY;
                }
                break;
                
            case ResizeMode.TopRight:
                var widthTR = _initialNodeSize.Width + deltaX;
                var heightTR = _initialNodeSize.Height - deltaY;
                if (widthTR >= minWidth)
                    node.Width = widthTR;
                if (heightTR >= minHeight)
                {
                    node.Height = heightTR;
                    node.PositionY = _initialNodePosition.Y + deltaY;
                }
                break;
                
            case ResizeMode.BottomLeft:
                var widthBL = _initialNodeSize.Width - deltaX;
                var heightBL = _initialNodeSize.Height + deltaY;
                if (widthBL >= minWidth)
                {
                    node.Width = widthBL;
                    node.PositionX = _initialNodePosition.X + deltaX;
                }
                if (heightBL >= minHeight)
                    node.Height = heightBL;
                break;
                
            case ResizeMode.BottomRight:
                var widthBR = _initialNodeSize.Width + deltaX;
                var heightBR = _initialNodeSize.Height + deltaY;
                if (widthBR >= minWidth)
                    node.Width = widthBR;
                if (heightBR >= minHeight)
                    node.Height = heightBR;
                break;
                
            case ResizeMode.Top:
                var heightT = _initialNodeSize.Height - deltaY;
                if (heightT >= minHeight)
                {
                    node.Height = heightT;
                    node.PositionY = _initialNodePosition.Y + deltaY;
                }
                break;
                
            case ResizeMode.Bottom:
                var heightB = _initialNodeSize.Height + deltaY;
                if (heightB >= minHeight)
                    node.Height = heightB;
                break;
                
            case ResizeMode.Left:
                var widthL = _initialNodeSize.Width - deltaX;
                if (widthL >= minWidth)
                {
                    node.Width = widthL;
                    node.PositionX = _initialNodePosition.X + deltaX;
                }
                break;
                
            case ResizeMode.Right:
                var widthR = _initialNodeSize.Width + deltaX;
                if (widthR >= minWidth)
                    node.Width = widthR;
                break;
        }
    }
}

public enum ResizeMode
{
    None,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Top,
    Bottom,
    Left,
    Right
}