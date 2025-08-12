using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using DSV.Models;
using DSV.Models.FieldEffects;


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

    // Field effects management
    public FieldEffectManager FieldEffectManager { get; } = new();
    
    // Track field effects by their source shapes for dynamic updates
    private readonly Dictionary<Node, string> _nodeFieldEffects = new();
    private readonly Dictionary<Circle, string> _circleFieldEffects = new();

    // UI Collections  
    public ObservableCollection<Circle> Circles { get; } = new();

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
        
        // Add test circle with wave effect
        var testCircle = new Circle { PositionX = 500, PositionY = 150, Radius = 60 };
        Circles.Add(testCircle);
        
        // Create wave field effect for the circle
        var waveEffect = new WaveFieldEffect
        {
            Position = new(testCircle.PositionX - testCircle.Radius, testCircle.PositionY - testCircle.Radius),
            Size = new(testCircle.Radius * 2, testCircle.Radius * 2),
            MaxDistance = 4.0,
            Frequency = 2.5,
            Amplitude = 1.2,
            WaveSpeed = 3.0,
            FadeWithDistance = true
        };
        FieldEffectManager.AddFieldEffect(waveEffect);
        
        // Track this field effect for dynamic updates
        _circleFieldEffects[testCircle] = waveEffect.Id;
        
        // Subscribe to circle changes for real-time updates
        testCircle.PropertyChanged += (s, e) => OnCircleChanged();
        
        // Add gravity effects for nodes
        AddGravityEffectsForNodes();
        
        // Subscribe to field effect changes
        FieldEffectManager.FieldEffectsChanged += (sender, affectedArea) => OnGravityFieldsChanged(affectedArea);
        
        // Subscribe to node changes for real-time gravity updates
        node1.PropertyChanged += (s, e) => OnNodeChanged();
        node2.PropertyChanged += (s, e) => OnNodeChanged();
        
        // Initial gravity calculation
        UpdateGravityFields();
    }

    private void OnNodeChanged()
    {
        // Update field effect positions for all nodes
        UpdateNodeFieldEffects();
        
        // Recalculate dot effects with new positions
        UpdateGravityFields();
        OnGravityFieldsChanged();
    }
    
    private void OnCircleChanged()
    {
        // Update field effect positions for all circles
        UpdateCircleFieldEffects();
        
        // Recalculate dot effects with new positions
        UpdateGravityFields();
        OnGravityFieldsChanged();
    }
    
    /// <summary>
    /// Update field effect positions when nodes move
    /// </summary>
    private void UpdateNodeFieldEffects()
    {
        foreach (var kvp in _nodeFieldEffects)
        {
            var node = kvp.Key;
            var effectId = kvp.Value;
            
            var fieldEffect = FieldEffectManager.GetFieldEffect(effectId);
            if (fieldEffect != null)
            {
                // Update position and size to match current node
                fieldEffect.Position = new(node.PositionX, node.PositionY);
                fieldEffect.Size = new(node.Width, node.Height);
            }
        }
    }
    
    /// <summary>
    /// Update field effect positions when circles move
    /// </summary>
    private void UpdateCircleFieldEffects()
    {
        foreach (var kvp in _circleFieldEffects)
        {
            var circle = kvp.Key;
            var effectId = kvp.Value;
            
            var fieldEffect = FieldEffectManager.GetFieldEffect(effectId);
            if (fieldEffect != null)
            {
                // Update position and size to match current circle
                fieldEffect.Position = new(circle.PositionX - circle.Radius, circle.PositionY - circle.Radius);
                fieldEffect.Size = new(circle.Radius * 2, circle.Radius * 2);
            }
        }
    }

    /// <summary>
    /// Event to notify canvas that gravity fields have changed and redraw is needed
    /// </summary>
    public event EventHandler<Rect>? GravityFieldsChanged;
    
    private void OnGravityFieldsChanged()
    {
        // Calculate combined affected area for dirty regions
        var affectedArea = CalculateGravityAffectedArea();
        OnGravityFieldsChanged(affectedArea);
    }
    
    private void OnGravityFieldsChanged(Rect affectedArea)
    {
        GravityFieldsChanged?.Invoke(this, affectedArea);
    }
    
    /// <summary>
    /// Add gravity field effects for all current nodes
    /// </summary>
    private void AddGravityEffectsForNodes()
    {
        foreach (var node in Nodes)
        {
            var gravityEffect = new GravityFieldEffect
            {
                Position = new(node.PositionX, node.PositionY),
                Size = new(node.Width, node.Height)
            };
            FieldEffectManager.AddFieldEffect(gravityEffect);
            
            // Track this field effect so we can update it when the node moves
            _nodeFieldEffects[node] = gravityEffect.Id;
        }
    }

    /// <summary>
    /// Update gravity field effects on all dots based on current node positions
    /// <summary>
    /// Update field effects on all dots using the unified field effects system
    /// </summary>
    private void UpdateGravityFields()
    {
        // Reset all previously affected dots
        foreach (var dot in _lastGravityAffectedDots)
        {
            dot.ResetGravityEffect();
        }
        _lastGravityAffectedDots.Clear();

        // Update field effect manager with current time (for animations)
        FieldEffectManager.Update(0.016); // ~60fps

        // Get combined affected area from all field effects
        var combinedBounds = FieldEffectManager.GetCombinedAffectedBounds(GridSettings.Spacing);
        
        if (combinedBounds.Width <= 0 || combinedBounds.Height <= 0)
            return;

        // Apply field effects to dots in the affected area
        ApplyFieldEffectsToArea(combinedBounds);
    }

    private void ApplyFieldEffectsToArea(Rect affectedArea)
    {
        // Get grid bounds for the affected area
        int colMin = Math.Max(0, (int)(affectedArea.Left / GridSettings.Spacing));
        int colMax = Math.Min(199, (int)(affectedArea.Right / GridSettings.Spacing));
        int rowMin = Math.Max(0, (int)(affectedArea.Top / GridSettings.Spacing));
        int rowMax = Math.Min(199, (int)(affectedArea.Bottom / GridSettings.Spacing));

        // Check each dot in the affected area
        for (int row = rowMin; row <= rowMax; row++)
        {
            for (int col = colMin; col <= colMax; col++)
            {
                if (_dotLookup.TryGetValue((col, row), out var dot))
                {
                    // Calculate combined effect from all field sources
                    var combinedEffect = FieldEffectManager.CalculateCombinedEffect(
                        dot.OriginalPosition, 
                        GridSettings.Spacing
                    );

                    if (combinedEffect.HasAnyEffect)
                    {
                        // Apply combined effects to the dot
                        var newPosition = dot.OriginalPosition + combinedEffect.TotalDisplacement;
                        dot.VisualPosition = newPosition;
                        dot.SizeMultiplier = combinedEffect.TotalSizeMultiplier;
                        dot.GravityColor = combinedEffect.FinalColor;
                        dot.HasGravityEffect = true;
                        dot.GravityRing = combinedEffect.HighestRing;
                        
                        _lastGravityAffectedDots.Add(dot);
                    }
                }
            }
        }
    }

    private Rect CalculateGravityAffectedArea()
    {
        // Use the field effect manager to get the combined affected bounds
        return FieldEffectManager.GetCombinedAffectedBounds(GridSettings.Spacing);
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
