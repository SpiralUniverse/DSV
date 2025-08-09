using CommunityToolkit.Mvvm.ComponentModel;
namespace DSV.Models;

public partial class Dot : ObservableObject
{
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    [ObservableProperty]
    public int size;

    [ObservableProperty]
    public bool isVisible = true;
}