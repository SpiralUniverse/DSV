using System;
using System.Collections.Generic;
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
    private List<Dot> _lastFocusedDots = new List<Dot>();

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
        float offset = 15f;

        int snappedX = (int)Math.Round((x - offset) / spacing) * spacing;
        int snappedY = (int)Math.Round((y - offset) / spacing) * spacing;

        float focusRadius = 50f; // You can tweak this

        float focusRadiusSquared = focusRadius * focusRadius;

        foreach (var dot in _lastFocusedDots)
            dot.Size = GridSettings.DotSize;

        _lastFocusedDots.Clear();

        // Determine search range in grid coordinates
        int colMin = Math.Max(0, (snappedX - (int)focusRadius) / spacing);
        int colMax = Math.Min(GridSettings.Columns - 1, (snappedX + (int)focusRadius) / spacing);
        int rowMin = Math.Max(0, (snappedY - (int)focusRadius) / spacing);
        int rowMax = Math.Min(GridSettings.Rows - 1, (snappedY + (int)focusRadius) / spacing);

        for (int row = rowMin; row <= rowMax; row++)
        {
            for (int col = colMin; col <= colMax; col++)
            {
                var dot = _dotGrid[row, col];

                int dx = dot.PositionX - snappedX;
                int dy = dot.PositionY - snappedY;

                if ((dx * dx + dy * dy) <= focusRadiusSquared)
                {
                    dot.Size = GridSettings.DotSize * 2;
                    _lastFocusedDots.Add(dot);
                }
            }
        }
    }

}
