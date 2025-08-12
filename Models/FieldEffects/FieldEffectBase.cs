using System;
using Avalonia;
using Avalonia.Media;

namespace DSV.Models.FieldEffects;

/// <summary>
/// Base class for all field effects that can influence dots
/// Supports nodes, circles, boxes, and any future controls
/// Designed for extensibility and animation support
/// </summary>
public abstract class FieldEffectBase
{
    /// <summary>
    /// Unique identifier for this field effect
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Position of the field source
    /// </summary>
    public Point Position { get; set; }
    
    /// <summary>
    /// Size of the field source (for rectangular sources)
    /// </summary>
    public Size Size { get; set; }
    
    /// <summary>
    /// Maximum distance this field can affect dots (in dot spacings)
    /// </summary>
    public double MaxDistance { get; set; } = 3.0;
    
    /// <summary>
    /// Strength multiplier for this field effect
    /// </summary>
    public double Strength { get; set; } = 1.0;
    
    /// <summary>
    /// Whether this field effect is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Animation properties for future animation system
    /// </summary>
    public FieldAnimation? Animation { get; set; }
    
    /// <summary>
    /// Calculate the effect this field has on a specific dot
    /// </summary>
    /// <param name="dotPosition">Position of the dot</param>
    /// <param name="dotSpacing">Spacing between dots in the grid</param>
    /// <returns>Field effect data for the dot</returns>
    public abstract FieldEffectResult CalculateEffect(Point dotPosition, double dotSpacing);
    
    /// <summary>
    /// Get the bounds of the area this field can affect
    /// Used for optimization when determining which dots to check
    /// </summary>
    public virtual Rect GetAffectedBounds(double dotSpacing)
    {
        var maxPixelDistance = MaxDistance * dotSpacing;
        return new Rect(
            Position.X - maxPixelDistance,
            Position.Y - maxPixelDistance,
            Size.Width + (maxPixelDistance * 2),
            Size.Height + (maxPixelDistance * 2)
        );
    }
    
    /// <summary>
    /// Update the field effect for the current frame
    /// Used for animations and time-based effects
    /// </summary>
    public virtual void Update(double deltaTime)
    {
        Animation?.Update(deltaTime);
    }
}

/// <summary>
/// Result of a field effect calculation on a single dot
/// </summary>
public struct FieldEffectResult
{
    /// <summary>
    /// Whether this field has any effect on the dot
    /// </summary>
    public bool HasEffect { get; set; }
    
    /// <summary>
    /// Visual displacement from original position
    /// </summary>
    public Vector Displacement { get; set; }
    
    /// <summary>
    /// Size multiplier (1.0 = normal size)
    /// </summary>
    public double SizeMultiplier { get; set; }
    
    /// <summary>
    /// Color override (null = use default color)
    /// </summary>
    public Color? Color { get; set; }
    
    /// <summary>
    /// Opacity multiplier (1.0 = normal opacity)
    /// </summary>
    public double OpacityMultiplier { get; set; }
    
    /// <summary>
    /// Field ring/zone identifier for visual effects
    /// </summary>
    public int Ring { get; set; }
    
    /// <summary>
    /// Distance from field source (normalized by spacing)
    /// </summary>
    public double NormalizedDistance { get; set; }
    
    public static FieldEffectResult None => new() { HasEffect = false, SizeMultiplier = 1.0, OpacityMultiplier = 1.0 };
}

/// <summary>
/// Animation properties for field effects
/// Placeholder for future animation system
/// </summary>
public class FieldAnimation
{
    public bool IsActive { get; set; }
    public double Duration { get; set; }
    public double ElapsedTime { get; set; }
    public AnimationType Type { get; set; }
    
    public virtual void Update(double deltaTime)
    {
        if (!IsActive) return;
        
        ElapsedTime += deltaTime;
        if (ElapsedTime >= Duration)
        {
            ElapsedTime = Duration;
            IsActive = false;
        }
    }
}

public enum AnimationType
{
    None,
    Pulse,
    Wave,
    Spiral,
    Fade,
    Scale
}
