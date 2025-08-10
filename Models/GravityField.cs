using System;
using Avalonia;
using Avalonia.Media;

namespace DSV.Models;

/// <summary>
/// Represents gravity field effects around nodes
/// Implements 2-iteration field system like electromagnetic fields
/// </summary>
public static class GravityField
{
    /// <summary>
    /// Field strength for anti-gravity effect (pushes dots away from nodes)
    /// </summary>
    public const double ANTI_GRAVITY_STRENGTH = 1.8;
    
    /// <summary>
    /// Maximum distance for gravity field effect (in dot spacings)
    /// </summary>
    public const double MAX_FIELD_DISTANCE = 3.0;
    
    /// <summary>
    /// Ring 1: Inner gravity field (strongest effect)
    /// </summary>
    public const double RING_1_DISTANCE = 1.5;
    
    /// <summary>
    /// Ring 2: Outer gravity field (weaker effect)
    /// </summary>
    public const double RING_2_DISTANCE = 3.0;
    
    /// <summary>
    /// Calculate gravity field effect on a dot based on distance from node
    /// </summary>
    /// <param name="dotPosition">Position of the dot</param>
    /// <param name="nodeRect">Rectangle representing the node bounds</param>
    /// <param name="dotSpacing">Spacing between dots in the grid</param>
    /// <returns>Gravity field effect data</returns>
    public static GravityEffect CalculateEffect(Point dotPosition, Rect nodeRect, double dotSpacing)
    {
        // Calculate distance from dot to nearest edge of node
        var nearestPoint = GetNearestPointOnRect(dotPosition, nodeRect);
        var vectorToNearest = new Vector(dotPosition.X - nearestPoint.X, dotPosition.Y - nearestPoint.Y);
        var distance = vectorToNearest.Length;
        var normalizedDistance = distance / dotSpacing;
        
        if (normalizedDistance > MAX_FIELD_DISTANCE)
        {
            return new GravityEffect { HasEffect = false };
        }
        
        // Determine which ring the dot is in
        var ring = normalizedDistance <= RING_1_DISTANCE ? 1 : 2;
        
        // Calculate anti-gravity displacement (push away from node)
        var direction = distance > 0 ? vectorToNearest / distance : new Vector(1, 0); // Normalize
        var strength = CalculateStrength(normalizedDistance, ring);
        var displacement = direction * strength * dotSpacing * 0.3; // 30% of dot spacing max
        
        // Calculate visual effects
        var sizeMultiplier = 1.0 + (strength * 0.5); // Up to 50% larger
        var color = GetFieldColor(ring, strength);
        
        return new GravityEffect
        {
            HasEffect = true,
            Ring = ring,
            Displacement = displacement,
            SizeMultiplier = sizeMultiplier,
            Color = color,
            Strength = strength
        };
    }
    
    private static double CalculateStrength(double normalizedDistance, int ring)
    {
        if (ring == 1)
        {
            // Ring 1: Strong anti-gravity with smooth falloff
            var factor = 1.0 - (normalizedDistance / RING_1_DISTANCE);
            return Math.Pow(factor, 2) * ANTI_GRAVITY_STRENGTH;
        }
        else
        {
            // Ring 2: Weaker anti-gravity with linear falloff
            var factor = 1.0 - ((normalizedDistance - RING_1_DISTANCE) / (RING_2_DISTANCE - RING_1_DISTANCE));
            return factor * ANTI_GRAVITY_STRENGTH * 0.4; // 40% of ring 1 strength
        }
    }
    
    private static Color GetFieldColor(int ring, double strength)
    {
        if (ring == 1)
        {
            // Ring 1: Purple to bright purple based on strength
            var intensity = (byte)(180 + (strength * 75)); // 180-255 range
            return Color.FromRgb(intensity, 100, intensity);
        }
        else
        {
            // Ring 2: Blue-purple blend
            var intensity = (byte)(120 + (strength * 100)); // 120-220 range  
            return Color.FromRgb(100, intensity, 200);
        }
    }
    
    private static Point GetNearestPointOnRect(Point point, Rect rect)
    {
        var x = Math.Max(rect.Left, Math.Min(point.X, rect.Right));
        var y = Math.Max(rect.Top, Math.Min(point.Y, rect.Bottom));
        return new Point(x, y);
    }
}

/// <summary>
/// Represents the effect of gravity field on a single dot
/// </summary>
public struct GravityEffect
{
    public bool HasEffect { get; set; }
    public int Ring { get; set; }
    public Vector Displacement { get; set; }
    public double SizeMultiplier { get; set; }
    public Color Color { get; set; }
    public double Strength { get; set; }
}
