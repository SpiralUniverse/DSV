using CommunityToolkit.Mvvm.ComponentModel;

namespace DSV.Models;

public partial class Circle : ObservableObject
{
    [ObservableProperty]
    private double _positionX;

    [ObservableProperty]
    private double _positionY;

    [ObservableProperty]
    private double _radius = 30;
    
    /// <summary>
    /// Width of the circle (diameter)
    /// </summary>
    public double Width => Radius * 2;
    
    /// <summary>
    /// Height of the circle (diameter)
    /// </summary>
    public double Height => Radius * 2;
}