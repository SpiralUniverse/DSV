using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using DSV.Models;


namespace DSV.ViewModels;

public class CanvasViewModel : ObservableObject
{
    public GridSettings GridSettings { get; set; } = new();

    private double _pointerX;
    private double _pointerY;

    public double PointerX
    {
        get => _pointerX;
        set => SetProperty(ref _pointerX, value);
    }

    public double PointerY
    {
        get => _pointerY;
        set => SetProperty(ref _pointerY, value);
    }

    public double FocusRadius { get; set; } = 50;

    // Only visible dots are in this collection (bound to UI)
    public ObservableCollection<Dot> Dots { get; } = new();

    // All dots in the world grid
    private readonly Dictionary<(int col, int row), Dot> _dotLookup = new();

    private (double startX, double startY, double width, double height) _viewport = (0, 0, 800, 600);

    private List<Dot> _lastFocusedDots = new List<Dot>();
    private double _lastPointerX = double.NaN;
    private double _lastPointerY = double.NaN;

    // Gravity field tracking
    private List<Dot> _lastGravityAffectedDots = new List<Dot>();

    public ObservableCollection<Node> Nodes { get; } = new();

    public CanvasViewModel()
    {
        GenerateWorldGrid();
        UpdateVisibleDots();

        var node1 = new Node { Title = "Node 1", PositionX = 100, PositionY = 100, Width = 150, Height = 80 };
        var node2 = new Node { Title = "Node 2", PositionX = 300, PositionY = 200, Width = 200, Height = 100 };
        
        Nodes.Add(node1);
        Nodes.Add(node2);
        
        // Subscribe to node changes for real-time gravity updates
        node1.PropertyChanged += (s, e) => OnNodeChanged();
        node2.PropertyChanged += (s, e) => OnNodeChanged();
        
        // Initial gravity calculation
        UpdateGravityFields();
    }

    private void OnNodeChanged()
    {
        UpdateGravityFields();
        OnGravityFieldsChanged();
    }

    /// <summary>
    /// Event to notify canvas that gravity fields have changed and redraw is needed
    /// </summary>
    public event EventHandler<Rect>? GravityFieldsChanged;
    
    private void OnGravityFieldsChanged()
    {
        // Calculate combined affected area for dirty regions
        var affectedArea = CalculateGravityAffectedArea();
        GravityFieldsChanged?.Invoke(this, affectedArea);
    }

    /// <summary>
    /// Update gravity field effects on all dots based on current node positions
    /// </summary>
    private void UpdateGravityFields()
    {
        // Reset all previously affected dots
        foreach (var dot in _lastGravityAffectedDots)
        {
            dot.ResetGravityEffect();
        }
        _lastGravityAffectedDots.Clear();

        // Apply gravity effects from all nodes
        foreach (var node in Nodes)
        {
            var nodeRect = new Rect(node.PositionX, node.PositionY, node.Width, node.Height);
            ApplyNodeGravityField(nodeRect);
        }
    }

    private void ApplyNodeGravityField(Rect nodeRect)
    {
        // Calculate affected area in grid coordinates
        var maxDistance = GravityField.MAX_FIELD_DISTANCE * GridSettings.Spacing;
        var searchArea = nodeRect.Inflate(maxDistance);
        
        // Get grid bounds for the search area
        int colMin = Math.Max(0, (int)(searchArea.Left / GridSettings.Spacing));
        int colMax = Math.Min(199, (int)(searchArea.Right / GridSettings.Spacing));
        int rowMin = Math.Max(0, (int)(searchArea.Top / GridSettings.Spacing));
        int rowMax = Math.Min(199, (int)(searchArea.Bottom / GridSettings.Spacing));

        // Check each dot in the affected area
        for (int row = rowMin; row <= rowMax; row++)
        {
            for (int col = colMin; col <= colMax; col++)
            {
                if (_dotLookup.TryGetValue((col, row), out var dot))
                {
                    var effect = GravityField.CalculateEffect(
                        dot.OriginalPosition, 
                        nodeRect, 
                        GridSettings.Spacing
                    );
                    
                    if (effect.HasEffect)
                    {
                        dot.ApplyGravityEffect(effect);
                        _lastGravityAffectedDots.Add(dot);
                    }
                }
            }
        }
    }

    private Rect CalculateGravityAffectedArea()
    {
        if (!Nodes.Any()) return new Rect();
        
        var combinedRect = new Rect(Nodes.First().PositionX, Nodes.First().PositionY, 
                                   Nodes.First().Width, Nodes.First().Height);
        
        foreach (var node in Nodes.Skip(1))
        {
            var nodeRect = new Rect(node.PositionX, node.PositionY, node.Width, node.Height);
            combinedRect = combinedRect.Union(nodeRect);
        }
        
        // Expand by maximum gravity field distance
        var maxDistance = GravityField.MAX_FIELD_DISTANCE * GridSettings.Spacing;
        return combinedRect.Inflate(maxDistance);
    }



    public void GenerateWorldGrid()
    {
        _dotLookup.Clear();

        int totalRows = 200;
        int totalCols = 200;

        for (int row = 0; row < totalRows; row++)
            for (int col = 0; col < totalCols; col++)
            {
                var dot = new Dot(
                    col * GridSettings.Spacing,
                    row * GridSettings.Spacing,
                    GridSettings.DotSize
                );

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
                    Dots.Add(dot);
                }
            }
        }
    }

    #region Focused Grid Dots
    public void UpdateGridFocus(double mouseX, double mouseY)
    {
        // Skip if pointer hasn't moved significantly (reduces unnecessary calculations)
        double threshold = 2.0; // pixels
        if (Math.Abs(mouseX - _lastPointerX) < threshold && Math.Abs(mouseY - _lastPointerY) < threshold)
            return;

        _lastPointerX = mouseX;
        _lastPointerY = mouseY;

        var spacing = GridSettings.Spacing;

        // Calculate grid-aligned focus area
        int colCenter = (int)Math.Round(mouseX / spacing);
        int rowCenter = (int)Math.Round(mouseY / spacing);
        
        double focusRadius = FocusRadius;
        double focusRadiusSquared = focusRadius * focusRadius;

        // Reset last focused dots
        foreach (var dot in _lastFocusedDots)
            dot.size = GridSettings.DotSize;

        _lastFocusedDots.Clear();

        // Calculate efficient search bounds
        int radiusInCells = (int)Math.Ceiling(focusRadius / spacing);
        int colMin = Math.Max(0, colCenter - radiusInCells);
        int colMax = Math.Min(199, colCenter + radiusInCells);
        int rowMin = Math.Max(0, rowCenter - radiusInCells);
        int rowMax = Math.Min(199, rowCenter + radiusInCells);

        // Only check dots in the focus area
        for (int row = rowMin; row <= rowMax; row++)
        {
            for (int col = colMin; col <= colMax; col++)
            {
                if (_dotLookup.TryGetValue((col, row), out var dot))
                {
                    double dx = dot.PositionX - mouseX;
                    double dy = dot.PositionY - mouseY;
                    double distSq = dx * dx + dy * dy;

                    if (distSq <= focusRadiusSquared)
                    {
                        dot.size = GridSettings.DotSize * 2;
                        _lastFocusedDots.Add(dot);
                    }
                }
            }
        }
    }
    #endregion

    public void UpdatePointer(double x, double y)
    {
        PointerX = x;
        PointerY = y;
        
        // Update focus effect using efficient grid lookup
        UpdateGridFocus(x, y);
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

// TODO: FEATURE - Need ability to hide dots under movable objects (e.g., boxes).
// SOLUTION: In custom draw routine, skip drawing dots within bounding box of the object.

// TODO: DESIGN - Mixing thousands of dot UI elements with other controls will hurt performance.
// SOLUTION: Layer approach — bottom layer for dot drawing, top layer for interactive controls.
