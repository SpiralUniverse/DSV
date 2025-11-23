using CommunityToolkit.Mvvm.ComponentModel;

namespace DSV.Models;

public partial class GridSettings : ObservableObject
{
    private static GridSettings? _instance;
    public static GridSettings Instance => _instance ??= new GridSettings();
    private GridSettings(){}
    
    [ObservableProperty]
    private int _rows = 20;
    [ObservableProperty]
    private int _columns = 20;
    [ObservableProperty]
    private int _dotSize = 3;
    [ObservableProperty]
    private int _spacing = 25;
    [ObservableProperty]
    private bool _isDynamic = false;
}
