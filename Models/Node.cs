using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DSV.Models;

public partial class Node : ObservableObject
{
    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private double _positionX;

    [ObservableProperty]
    private double _positionY;

    [ObservableProperty]
    private double _width = 150;

    [ObservableProperty]
    private double _height = 80;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isDragging;

    [ObservableProperty]
    private bool _isResizing;

    [ObservableProperty]
    private NodeShape _shape = NodeShape.Rectangle;

    [ObservableProperty]
    private string _color = "#313244"; // Default SurfaceBase

    public bool IsRectangle => Shape == NodeShape.Rectangle;
    public bool IsCircle => Shape == NodeShape.Circle;
    public bool IsTriangle => Shape == NodeShape.Triangle;
    public bool IsHexagon => Shape == NodeShape.Hexagon;

    partial void OnShapeChanged(NodeShape value)
    {
        OnPropertyChanged(nameof(IsRectangle));
        OnPropertyChanged(nameof(IsCircle));
        OnPropertyChanged(nameof(IsTriangle));
        OnPropertyChanged(nameof(IsHexagon));
    }
}

public enum NodeShape
{
    Rectangle,
    Circle,
    Triangle,
    Hexagon
}
