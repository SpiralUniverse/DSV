using System;
using Avalonia;
using Avalonia.Media;

namespace DSV.Models.FieldEffects;

/// <summary>
/// Anti-gravity field effect that pushes dots away from nodes
/// Implements the existing gravity behavior in the new unified system
/// </summary>
public class GravityFieldEffect : FieldEffectBase
{
    /// <summary>
    /// Ring 1: Inner gravity field (strongest effect)
    /// </summary>
    public const double RING_1_DISTANCE = 1.5;
    
    /// <summary>
    /// Ring 2: Outer gravity field (weaker effect)
    /// </summary>
    public const double RING_2_DISTANCE = 3.0;
    
    /// <summary>
    /// Field strength for anti-gravity effect
    /// </summary>
    public double AntiGravityStrength { get; set; } = 1.8;
    
    /// <summary>
    /// Maximum displacement as percentage of dot spacing
    /// </summary>
    public double MaxDisplacementPercent { get; set; } = 0.3; // 30%
    
    /// <summary>
    /// Maximum size increase from field effect
    /// </summary>
    public double MaxSizeIncrease { get; set; } = 0.5; // 50%
    
    public GravityFieldEffect()
    {
        MaxDistance = RING_2_DISTANCE;
    }
    
    public override FieldEffectResult CalculateEffect(Point dotPosition, double dotSpacing)
    {
        if (!IsActive)
            return FieldEffectResult.None;
            
        // Calculate distance from dot to nearest edge of field source
        var nearestPoint = GetNearestPointOnRect(dotPosition, new(Position, Size));
        var vectorToNearest = new Vector(dotPosition.X - nearestPoint.X, dotPosition.Y - nearestPoint.Y);
        var distance = vectorToNearest.Length;
        var normalizedDistance = distance / dotSpacing;
        
        if (normalizedDistance > MaxDistance)
        {
            return FieldEffectResult.None;
        }
        
        // Determine which ring the dot is in
        var ring = normalizedDistance <= RING_1_DISTANCE ? 1 : 2;
        
        // Calculate anti-gravity displacement (push away from node)
        var direction = distance > 0 ? vectorToNearest / distance : new Vector(1, 0);
        var fieldStrength = CalculateStrength(normalizedDistance, ring) * Strength;
        var displacement = direction * fieldStrength * dotSpacing * MaxDisplacementPercent;
        
        // Calculate visual effects
        var sizeMultiplier = 1.0 + (fieldStrength * MaxSizeIncrease);
        var color = GetFieldColor(ring, fieldStrength);
        
        return new FieldEffectResult
        {
            HasEffect = true,
            Displacement = displacement,
            SizeMultiplier = sizeMultiplier,
            Color = color,
            OpacityMultiplier = 1.0,
            Ring = ring,
            NormalizedDistance = normalizedDistance
        };
    }
    
    private double CalculateStrength(double normalizedDistance, int ring)
    {
        double baseStrength;
        double ringMax = ring == 1 ? RING_1_DISTANCE : RING_2_DISTANCE;
        double ringMin = ring == 1 ? 0 : RING_1_DISTANCE;
        
        // Interpolate within ring boundaries
        var ringProgress = (normalizedDistance - ringMin) / (ringMax - ringMin);
        ringProgress = Math.Max(0, Math.Min(1, ringProgress));
        
        if (ring == 1)
        {
            // Inner ring: very strong at center, decreasing
            baseStrength = AntiGravityStrength * (1.0 - ringProgress * 0.3); // 70% minimum
        }
        else
        {
            // Outer ring: medium strength, decreasing to zero
            baseStrength = AntiGravityStrength * 0.7 * (1.0 - ringProgress);
        }
        
        return Math.Max(0, baseStrength);
    }
    
    private Color GetFieldColor(int ring, double strength)
    {
        if (ring == 1)
        {
            // Inner ring: Blue with intensity based on strength
            var intensity = (byte)(255 * strength);
            return Color.FromRgb(0, 0, intensity);
        }
        else
        {
            // Outer ring: Cyan with intensity based on strength
            var intensity = (byte)(255 * strength * 0.7);
            return Color.FromRgb(0, intensity, intensity);
        }
    }
    
    private static Point GetNearestPointOnRect(Point point, Rect rect)
    {
        var x = Math.Max(rect.Left, Math.Min(point.X, rect.Right));
        var y = Math.Max(rect.Top, Math.Min(point.Y, rect.Bottom));
        return new(x, y);
    }
}
