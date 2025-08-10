using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DSV.Models;

namespace DSV.Views;

public partial class NodeView : UserControl
{
    private bool _isDragging;
    private bool _isResizing;
    private Point _lastPointerPosition;
    private ResizeMode _resizeMode = ResizeMode.None;
    
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
        
        var position = e.GetPosition(this);
        _lastPointerPosition = position;
        
        // Select the node
        node.IsSelected = true;
        
        // Start dragging if clicking on the main area (not resize handles)
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !_isResizing)
        {
            _isDragging = true;
            node.IsDragging = true;
            this.Cursor = new Cursor(StandardCursorType.DragMove);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not Node node) return;
        
        var currentPosition = e.GetPosition(this);
        var deltaX = currentPosition.X - _lastPointerPosition.X;
        var deltaY = currentPosition.Y - _lastPointerPosition.Y;

        if (_isDragging)
        {
            // Move the node
            node.PositionX += deltaX;
            node.PositionY += deltaY;
            _lastPointerPosition = currentPosition;
            e.Handled = true;
        }
        else if (_isResizing)
        {
            // Resize the node based on resize mode
            ResizeNode(node, deltaX, deltaY);
            _lastPointerPosition = currentPosition;
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
        _lastPointerPosition = e.GetPosition(this);
        e.Handled = true;
    }

    private void ResizeNode(Node node, double deltaX, double deltaY)
    {
        const double minWidth = 50;
        const double minHeight = 30;

        switch (_resizeMode)
        {
            case ResizeMode.TopLeft:
                var newWidth = node.Width - deltaX;
                var newHeight = node.Height - deltaY;
                if (newWidth >= minWidth)
                {
                    node.Width = newWidth;
                    node.PositionX += deltaX;
                }
                if (newHeight >= minHeight)
                {
                    node.Height = newHeight;
                    node.PositionY += deltaY;
                }
                break;
                
            case ResizeMode.TopRight:
                if (node.Width + deltaX >= minWidth)
                    node.Width += deltaX;
                if (node.Height - deltaY >= minHeight)
                {
                    node.Height -= deltaY;
                    node.PositionY += deltaY;
                }
                break;
                
            case ResizeMode.BottomLeft:
                if (node.Width - deltaX >= minWidth)
                {
                    node.Width -= deltaX;
                    node.PositionX += deltaX;
                }
                if (node.Height + deltaY >= minHeight)
                    node.Height += deltaY;
                break;
                
            case ResizeMode.BottomRight:
                if (node.Width + deltaX >= minWidth)
                    node.Width += deltaX;
                if (node.Height + deltaY >= minHeight)
                    node.Height += deltaY;
                break;
                
            case ResizeMode.Top:
                if (node.Height - deltaY >= minHeight)
                {
                    node.Height -= deltaY;
                    node.PositionY += deltaY;
                }
                break;
                
            case ResizeMode.Bottom:
                if (node.Height + deltaY >= minHeight)
                    node.Height += deltaY;
                break;
                
            case ResizeMode.Left:
                if (node.Width - deltaX >= minWidth)
                {
                    node.Width -= deltaX;
                    node.PositionX += deltaX;
                }
                break;
                
            case ResizeMode.Right:
                if (node.Width + deltaX >= minWidth)
                    node.Width += deltaX;
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