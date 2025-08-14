using CommunityToolkit.Mvvm.ComponentModel;

namespace DSV.Models;

public class GridSettings
{
    private static GridSettings? _instance;
    public static GridSettings Instance => _instance ??= new GridSettings();
    private GridSettings(){}
    
    public int Rows { get; set; } = 20;
    public int Columns { get; set; } = 20;
    public int DotSize { get; set; } = 2;
    public int Spacing { get; set; } = 20;
}
