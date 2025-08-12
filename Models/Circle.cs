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
}