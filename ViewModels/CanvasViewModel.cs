using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using DSV.Models;


namespace DSV.ViewModels;

public class CanvasViewModel : ObservableObject
{
    public GridSettings GridSettings { get; set; } = new();


    public ObservableCollection<Dot> Dots { get; } = new();

    public CanvasViewModel()
    {
        GenerateGrid();
    }



    public void GenerateGrid()
    {
        Dots.Clear();
        for (int i = 0; i < GridSettings.Rows; i++)
            for (int j = 0; j < GridSettings.Columns; j++)
                AddDot(i, j);
    }

    private void AddDot(int y, int x)
    {
        Dots.Add(new Dot
        {
            PositionX = x * GridSettings.Spacing,
            PositionY = y * GridSettings.Spacing,
            Size = GridSettings.DotSize
        });
    }
}
