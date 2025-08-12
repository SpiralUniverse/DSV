using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DSV.ViewModels;
using DSV.Models;

namespace DSV.Controls;

public class DotCanvas : Control
{
    public static readonly StyledProperty<CanvasViewModel> ViewModelProperty =
        AvaloniaProperty.Register<DotCanvas, CanvasViewModel>(nameof(ViewModel));

    // Cache brushes to avoid creating them every frame
    private static readonly IBrush DefaultDotBrush = new SolidColorBrush(Color.FromRgb(0xdd, 0xb9, 0xf7));
    
    // Dirty regions system - 20x20 = 400 ultra-fine regions
    private const int REGIONS_X = 20;
    private const int REGIONS_Y = 20;
    private DirtyRegion[,] _dirtyRegions = new DirtyRegion[REGIONS_X, REGIONS_Y];
    private bool _regionsInitialized = false;
    private int _currentFrame = 0;
    private bool _forceFullRender = true; // Force full render initially

    public CanvasViewModel ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        InitializeDirtyRegions();
    }

    private void InitializeDirtyRegions()
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;
        
        var regionWidth = Bounds.Width / REGIONS_X;
        var regionHeight = Bounds.Height / REGIONS_Y;
        
        for (int x = 0; x < REGIONS_X; x++)
        {
            for (int y = 0; y < REGIONS_Y; y++)
            {
                var rect = new Rect(
                    x * regionWidth,
                    y * regionHeight,
                    regionWidth,
                    regionHeight
                );
                _dirtyRegions[x, y] = new DirtyRegion(x, y, rect);
            }
        }
        
        _regionsInitialized = true;
        MarkAllRegionsDirty(); // Force full redraw after resize
    }

    /// <summary>
    /// Mark specific regions as dirty for surgical updates
    /// </summary>
    public void MarkRegionsDirty(Rect affectedArea)
    {
        if (!_regionsInitialized) return;
        
        _currentFrame++;
        
        for (int x = 0; x < REGIONS_X; x++)
        {
            for (int y = 0; y < REGIONS_Y; y++)
            {
                if (_dirtyRegions[x, y].Intersects(affectedArea))
                {
                    _dirtyRegions[x, y].MarkDirty(_currentFrame);
                }
            }
        }
        
        // Trigger Avalonia's dirty region system
        InvalidateVisual();
    }

    /// <summary>
    /// Invalidate only a specific rectangular region
    /// More efficient than InvalidateVisual() for localized changes
    /// </summary>
    public void InvalidateRect(Rect rect)
    {
        MarkRegionsDirty(rect);
    }

    /// <summary>
    /// Mark all regions dirty (full redraw)
    /// </summary>
    public void MarkAllRegionsDirty()
    {
        if (!_regionsInitialized) return;
        
        _currentFrame++;
        
        for (int x = 0; x < REGIONS_X; x++)
        {
            for (int y = 0; y < REGIONS_Y; y++)
            {
                _dirtyRegions[x, y].MarkDirty(_currentFrame);
            }
        }
        
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        // var sw = System.Diagnostics.Stopwatch.StartNew();
        base.Render(context);

        if (ViewModel?.Dots == null)
            return;

        // Optimized dirty region rendering
        if (_regionsInitialized && !_forceFullRender)
        {
            // Only render dots in dirty regions for maximum performance
            RenderDotsInDirtyRegions(context);
        }
        else
        {
            // Fallback: render all visible dots (initial load or error recovery)
            foreach (var dot in ViewModel.Dots)
            {
                RenderDot(context, dot);
            }
        }

        // Clean all dirty regions after render
        if (_regionsInitialized)
        {
            for (int x = 0; x < REGIONS_X; x++)
            {
                for (int y = 0; y < REGIONS_Y; y++)
                {
                    _dirtyRegions[x, y].MarkClean();
                }
            }
        }

        _forceFullRender = false;
        // sw.Stop();
        // Console.WriteLine($"Render took {sw.ElapsedMilliseconds} ms");
    }

    /// <summary>
    /// Render only dots that are within dirty regions for optimal performance
    /// </summary>
    private void RenderDotsInDirtyRegions(DrawingContext context)
    {
        int dotsRendered = 0;
        var renderedDots = new HashSet<Dot>(); // Avoid rendering the same dot multiple times
        
        // Check each dirty region and render dots within it
        for (int x = 0; x < REGIONS_X; x++)
        {
            for (int y = 0; y < REGIONS_Y; y++)
            {
                var region = _dirtyRegions[x, y];
                if (!region.IsDirty) continue;
                
                // Find dots within this dirty region
                foreach (var dot in ViewModel.Dots)
                {
                    if (renderedDots.Contains(dot)) continue; // Skip already rendered dots
                    
                    if (region.Contains(dot.VisualPosition))
                    {
                        RenderDot(context, dot);
                        renderedDots.Add(dot);
                        dotsRendered++;
                    }
                }
            }
        }
        
        // // Performance debugging - show how many dots we actually rendered
        // if (dotsRendered > 0)
        // {
        //     Console.WriteLine($"Dirty region optimization: Rendered {dotsRendered} dots (out of {ViewModel.Dots.Count} visible)");
        // }
        // else
        // {
        //     Console.WriteLine("No dirty regions - skipped rendering entirely (dots remain visible from previous frame)");
        // }
    }

    private void RenderDot(DrawingContext context, Dot dot)
    {
        var brush = DefaultDotBrush;
        var radius = dot.size * dot.SizeMultiplier * 0.5;
        
        // Apply gravity field color if present
        if (dot.HasGravityEffect && dot.GravityColor.HasValue)
        {
            brush = new SolidColorBrush(dot.GravityColor.Value);
        }
        
        context.DrawEllipse(brush, null, dot.VisualPosition, radius, radius);
    }
}