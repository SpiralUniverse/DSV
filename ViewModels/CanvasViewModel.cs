using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using DSV.Models;
using Tmds.DBus.Protocol;


namespace DSV.ViewModels;

public class CanvasViewModel : ObservableObject
{
    public GridSettings GridSettings { get; set; } = new();


    public ObservableCollection<Dot> Dots { get; } = new();

    private Dot[,] _dotGrid = new Dot[1000, 1000];

    public CanvasViewModel()
    {
        // Canvas c = this.
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
        var dot = new Dot
        {
            PositionX = x * GridSettings.Spacing,
            PositionY = y * GridSettings.Spacing,
            Size = GridSettings.DotSize
        };

        _dotGrid[y, x] = dot;
        Dots.Add(dot);
    }

    public void UpdateGridFocus(double x, double y)
    {
        if (GridSettings == null) return;

        var spacing = GridSettings.Spacing;
        int colCenter = (int)Math.Round(x / spacing);
        int rowCenter = (int)Math.Round(y / spacing);
        float radius = 50f; //TODO: Fix this hard coded value
        float radiusIndex = radius / spacing;

        for (int row = Math.Max(0, rowCenter - (int)radiusIndex);
            row <= Math.Min(GridSettings.Rows - 1, rowCenter + (int)radiusIndex);
            row++)
        {
            for (int col = Math.Max(0, colCenter - (int)radiusIndex);
                col <= Math.Min(GridSettings.Columns - 1, colCenter + (int)radiusIndex);
                col++)
            {
                var dot = _dotGrid[row, col];
                int dx = col - colCenter;
                int dy = row - rowCenter;

                if (dx * dx + dy * dy <= radiusIndex * radiusIndex)
                    dot.Size = GridSettings.DotSize * 2;
                else
                    dot.Size = GridSettings.DotSize; //FIXME: its not resetting the size
            }
        }
    }
}
