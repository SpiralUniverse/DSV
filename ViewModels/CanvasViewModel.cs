using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DSV.Models;


namespace DSV.ViewModels;

public class CanvasViewModel : ObservableObject
{
    public GridSettings GridSettings { get; set; } = new();


    // Only visible dots are in this collection (bound to UI)
    public ObservableCollection<Dot> Dots { get; } = new();

    // All dots in the world grid
    private readonly Dictionary<(int col, int row), Dot> _dotLookup = new();

    private (double startX, double startY, double width, double height) _viewport = (0, 0, 800, 600);

    private List<Dot> _lastFocusedDots = new List<Dot>();

    public CanvasViewModel()
    {
        GenerateWorldGrid();
        UpdateVisibleDots();
    }



    public void GenerateWorldGrid()
    {
        _dotLookup.Clear();

        int totalRows = 200;
        int totalCols = 200;

        for (int row = 0; row < totalRows; row++)
            for (int col = 0; col < totalCols; col++)
            {
                var dot = new Dot
                {
                    PositionX = col * GridSettings.Spacing,
                    PositionY = row * GridSettings.Spacing,
                    Size = GridSettings.DotSize,
                    IsVisible = false // Will be set visible by viewport update
                };

                _dotLookup[(col, row)] = dot;
            }

    }


    public void UpdateVisibleDots()
    {
        Dots.Clear();

        int colMin = Math.Max(0, (int)(_viewport.startX / GridSettings.Spacing));
        int colMax = Math.Min((int)((_viewport.startX + _viewport.width) / GridSettings.Spacing), 199);
        int rowMin = Math.Max(0, (int)(_viewport.startY / GridSettings.Spacing));
        int rowMax = Math.Min((int)((_viewport.startY + _viewport.height) / GridSettings.Spacing), 199);

        for (int row = rowMin; row <= rowMax; row++)
        {
            for (int col = colMin; col <= colMax; col++)
            {
                if (_dotLookup.TryGetValue((col, row), out var dot))
                {
                    dot.IsVisible = true;
                    Dots.Add(dot);
                }
            }
        }
    }

    public void UpdateGridFocus(double mouseX, double mouseY)
    {
        float offset = 15f;
        var spacing = GridSettings.Spacing;

        int snappedX = (int)Math.Round((mouseX - offset) / spacing) * spacing;
        int snappedY = (int)Math.Round((mouseY - offset) / spacing) * spacing;

        float focusRadius = 50f;
        float focusRadiusSquared = focusRadius * focusRadius;

        // Reset last focused
        foreach (var dot in _lastFocusedDots)
            dot.Size = GridSettings.DotSize;

        _lastFocusedDots.Clear();

        int colMin = Math.Max(0, (snappedX - (int)focusRadius) / spacing);
        int colMax = Math.Min((snappedX + (int)focusRadius) / spacing, 199);
        int rowMin = Math.Max(0, (snappedY - (int)focusRadius) / spacing);
        int rowMax = Math.Min((snappedY + (int)focusRadius) / spacing, 199);

        for (int row = rowMin; row <= rowMax; row++)
        {
            for (int col = colMin; col <= colMax; col++)
            {
                if (_dotLookup.TryGetValue((col, row), out var dot))
                {
                    int dx = (int)dot.PositionX - snappedX;
                    int dy = (int)dot.PositionY - snappedY;

                    if ((dx * dx + dy * dy) <= focusRadiusSquared)
                    {
                        dot.Size = GridSettings.DotSize * 2;
                        _lastFocusedDots.Add(dot);
                    }
                }
            }
        }
    }

    public void SetViewport(double x, double y, double width, double height)
    {
        _viewport.startX = x;
        _viewport.startY = y;
        _viewport.width = width;
        _viewport.height = height;

        UpdateVisibleDots();
    }

}




// TODO: PERF - Large number of Ellipses (one per dot) causes layout & render lag, especially on resize.
// SOLUTION: Use a single custom-drawn surface (OnRender) to draw all dots in one pass.

// TODO: PERF - Pointer focus update is slow when moving fast due to per-dot property updates triggering UI re-render.
// SOLUTION: Batch size updates or draw directly without binding each dot’s Size property.

// TODO: PERF - Resizing grid rebuilds entire ObservableCollection, causing GC pressure & UI rebuild.
// SOLUTION: Keep fixed-size dot array and recalculate positions, only invalidate visual.

// TODO: FEATURE - Need ability to hide dots under movable objects (e.g., boxes).
// SOLUTION: In custom draw routine, skip drawing dots within bounding box of the object.

// TODO: DESIGN - Mixing thousands of dot UI elements with other controls will hurt performance.
// SOLUTION: Layer approach — bottom layer for dot drawing, top layer for interactive controls.
