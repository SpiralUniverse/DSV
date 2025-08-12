using System;
using Avalonia;
using Avalonia.Media;

namespace DSV.Models.FieldEffects;

/// <summary>
/// Magnetism field effect that attracts dots toward the source
/// Opposite of gravity field - pulls dots in instead of pushing away
/// </summary>
public class MagnetismFieldEffect : FieldEffectBase
{
    /// <summary>
    /// Attraction strength multiplier
    /// </summary>
    public double AttractionStrength { get; set; } = 1.5;
    
    /// <summary>
    /// Maximum displacement as percentage of dot spacing
    /// </summary>
    public double MaxDisplacementPercent { get; set; } = 0.4; // 40%
    
    /// <summary>
    /// Whether to use pulsing effect
    /// </summary>
    public bool EnablePulse { get; set; } = true;
    
    /// <summary>
    /// Pulse frequency in Hz
    /// </summary>
    public double PulseFrequency { get; set; } = 2.0;
    
    private double _pulseTime = 0;
    
    public override FieldEffectResult CalculateEffect(Point dotPosition, double dotSpacing)
    {
        if (!IsActive)
            return FieldEffectResult.None;
            
        // Calculate vector from source center to dot
        var sourceCenter = new Point(Position.X + Size.Width / 2, Position.Y + Size.Height / 2);
        var vectorToSource = new Vector(sourceCenter.X - dotPosition.X, sourceCenter.Y - dotPosition.Y);
        var distance = vectorToSource.Length;
        var normalizedDistance = distance / dotSpacing;
        
        if (normalizedDistance > MaxDistance)
        {
            return FieldEffectResult.None;
        }
        
        // Calculate attraction force (stronger when closer)
        var distanceFactor = 1.0 - (normalizedDistance / MaxDistance);
        var baseStrength = AttractionStrength * distanceFactor * Strength;
        
        // Apply pulse effect if enabled
        var pulseMultiplier = EnablePulse ? (1.0 + 0.3 * Math.Sin(_pulseTime * PulseFrequency * 2 * Math.PI)) : 1.0;
        var finalStrength = baseStrength * pulseMultiplier;
        
        // Calculate displacement toward source
        var direction = distance > 0 ? vectorToSource / distance : new Vector(0, 0);
        var displacement = direction * finalStrength * dotSpacing * MaxDisplacementPercent;
        
        // Visual effects: Red/pink color with size increase
        var sizeMultiplier = 1.0 + (finalStrength * 0.3);
        var colorIntensity = (byte)(255 * Math.Min(1.0, finalStrength));
        var color = Color.FromRgb(colorIntensity, 0, (byte)(colorIntensity * 0.5));
        
        return new FieldEffectResult
        {
            HasEffect = true,
            Displacement = displacement,
            SizeMultiplier = sizeMultiplier,
            Color = color,
            OpacityMultiplier = 1.0,
            Ring = normalizedDistance <= MaxDistance / 2 ? 1 : 2,
            NormalizedDistance = normalizedDistance
        };
    }
    
    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);
        _pulseTime += deltaTime;
    }
}
