using Avalonia;
using Avalonia.Media;

namespace DSV.Models;

public partial class Dot 
{
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int size;

    /// <summary>
    /// Original position of the dot in the grid (before gravity displacement)
    /// </summary>
    public Point OriginalPosition => new Point(PositionX, PositionY);
    
    /// <summary>
    /// Current visual position after gravity field displacement
    /// </summary>
    public Point VisualPosition { get; set; }
    
    /// <summary>
    /// Current visual size multiplier from gravity effects
    /// </summary>
    public double SizeMultiplier { get; set; } = 1.0;
    
    /// <summary>
    /// Current color from gravity field effects (null = default color)
    /// </summary>
    public Color? GravityColor { get; set; }
    
    /// <summary>
    /// Whether this dot is currently affected by any gravity field
    /// </summary>
    public bool HasGravityEffect { get; set; }
    
    /// <summary>
    /// Gravity field ring this dot is in (1 or 2, 0 if no effect)
    /// </summary>
    public int GravityRing { get; set; }

    public Dot(int x, int y, int dotSize = 2)
    {
        PositionX = x;
        PositionY = y;
        size = dotSize;
        VisualPosition = OriginalPosition;
    }
    
    /// <summary>
    /// Apply gravity field effect to this dot
    /// </summary>
    public void ApplyGravityEffect(GravityEffect effect)
    {
        if (effect.HasEffect)
        {
            HasGravityEffect = true;
            GravityRing = effect.Ring;
            VisualPosition = OriginalPosition + effect.Displacement;
            SizeMultiplier = effect.SizeMultiplier;
            GravityColor = effect.Color;
        }
        else
        {
            ResetGravityEffect();
        }
    }
    
    /// <summary>
    /// Reset dot to original state (no gravity effects)
    /// </summary>
    public void ResetGravityEffect()
    {
        HasGravityEffect = false;
        GravityRing = 0;
        VisualPosition = OriginalPosition;
        SizeMultiplier = 1.0;
        GravityColor = null;
    }
}