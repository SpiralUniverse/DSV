using System;
using Avalonia;
using Avalonia.Media;

namespace DSV.Models.FieldEffects;

/// <summary>
/// Wave field effect that creates ripple patterns around the source
/// Demonstrates animation capabilities and complex visual effects
/// </summary>
public class WaveFieldEffect : FieldEffectBase
{
    /// <summary>
    /// Wave frequency in Hz
    /// </summary>
    public double Frequency { get; set; } = 3.0;
    
    /// <summary>
    /// Wave amplitude multiplier
    /// </summary>
    public double Amplitude { get; set; } = 1.0;
    
    /// <summary>
    /// Wave speed (how fast waves propagate outward)
    /// </summary>
    public double WaveSpeed { get; set; } = 2.0;
    
    /// <summary>
    /// Whether waves should fade with distance
    /// </summary>
    public bool FadeWithDistance { get; set; } = true;
    
    /// <summary>
    /// Number of wave rings to display
    /// </summary>
    public int WaveRings { get; set; } = 3;
    
    private double _time = 0;
    
    public override FieldEffectResult CalculateEffect(Point dotPosition, double dotSpacing)
    {
        if (!IsActive)
            return FieldEffectResult.None;
            
        // Calculate distance from source center
        var sourceCenter = new Point(Position.X + Size.Width / 2, Position.Y + Size.Height / 2);
        var vectorFromSource = new Vector(dotPosition.X - sourceCenter.X, dotPosition.Y - sourceCenter.Y);
        var distance = vectorFromSource.Length;
        var normalizedDistance = distance / dotSpacing;
        
        if (normalizedDistance > MaxDistance)
        {
            return FieldEffectResult.None;
        }
        
        // Calculate wave effect
        var wavePhase = (_time * Frequency * 2 * Math.PI) - (normalizedDistance * WaveSpeed);
        var waveValue = Math.Sin(wavePhase) * Amplitude * Strength;
        
        // Apply distance fading if enabled
        if (FadeWithDistance)
        {
            var distanceFactor = 1.0 - (normalizedDistance / MaxDistance);
            waveValue *= distanceFactor;
        }
        
        // Create ripple displacement (perpendicular to radius)
        var tangentDirection = distance > 0 ? new Vector(-vectorFromSource.Y, vectorFromSource.X) / distance : new Vector(1, 0);
        var displacement = tangentDirection * waveValue * dotSpacing * 0.2; // 20% max displacement
        
        // Size and color effects based on wave
        var sizeMultiplier = 1.0 + (Math.Abs(waveValue) * 0.4);
        var colorIntensity = (byte)(255 * Math.Max(0, Math.Min(1, Math.Abs(waveValue))));
        
        // Cycle through colors based on wave phase
        var hue = (wavePhase / (2 * Math.PI)) % 1.0;
        var color = HSVtoRGB(hue, 0.8, colorIntensity / 255.0);
        
        return new FieldEffectResult
        {
            HasEffect = true,
            Displacement = displacement,
            SizeMultiplier = sizeMultiplier,
            Color = color,
            OpacityMultiplier = 1.0,
            Ring = (int)(normalizedDistance / (MaxDistance / WaveRings)) + 1,
            NormalizedDistance = normalizedDistance
        };
    }
    
    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);
        _time += deltaTime;
    }
    
    private static Color HSVtoRGB(double h, double s, double v)
    {
        var c = v * s;
        var x = c * (1 - Math.Abs((h * 6) % 2 - 1));
        var m = v - c;
        
        double r, g, b;
        if (h < 1.0/6) { r = c; g = x; b = 0; }
        else if (h < 2.0/6) { r = x; g = c; b = 0; }
        else if (h < 3.0/6) { r = 0; g = c; b = x; }
        else if (h < 4.0/6) { r = 0; g = x; b = c; }
        else if (h < 5.0/6) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }
        
        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255)
        );
    }
}
