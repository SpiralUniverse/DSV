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
}