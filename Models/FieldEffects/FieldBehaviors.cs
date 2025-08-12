using System;
using Avalonia;
using Avalonia.Media;

namespace DSV.Models.FieldEffects;

/// <summary>
/// Strategy interface for custom field effect behaviors
/// Allows composition of different effect behaviors
/// </summary>
public interface IFieldBehavior
{
    /// <summary>
    /// Apply this behavior to modify a field effect result
    /// </summary>
    /// <param name="dotPosition">Position of the dot being affected</param>
    /// <param name="sourcePosition">Position of the field source</param>
    /// <param name="sourceSize">Size of the field source</param>
    /// <param name="normalizedDistance">Distance from source normalized by dot spacing</param>
    /// <param name="baseStrength">Base strength of the effect (0.0 to 1.0)</param>
    /// <returns>Modified field effect result</returns>
    FieldEffectResult Apply(Point dotPosition, Point sourcePosition, Size sourceSize, 
                           double normalizedDistance, double baseStrength);
}

/// <summary>
/// Behavior that magnifies dot size
/// </summary>
public class MagnificationBehavior : IFieldBehavior
{
    public double MagnificationMultiplier { get; set; } = 2.0;
    
    public FieldEffectResult Apply(Point dotPosition, Point sourcePosition, Size sourceSize, 
                                  double normalizedDistance, double baseStrength)
    {
        return new FieldEffectResult
        {
            HasEffect = true,
            SizeMultiplier = 1.0 + (baseStrength * 0.5 * MagnificationMultiplier), // Apply magnification
            OpacityMultiplier = 1.0
        };
    }
}

/// <summary>
/// Behavior that applies grayscale coloring based on intensity
/// </summary>
public class GrayscaleBehavior : IFieldBehavior
{
    public FieldEffectResult Apply(Point dotPosition, Point sourcePosition, Size sourceSize, 
                                  double normalizedDistance, double baseStrength)
    {
        var grayValue = (byte)(255 * Math.Min(1.0, baseStrength));
        var grayColor = Color.FromRgb(grayValue, grayValue, grayValue);
        
        return new FieldEffectResult
        {
            HasEffect = true,
            Color = grayColor,
            OpacityMultiplier = 1.0
        };
    }
}

/// <summary>
/// Behavior that creates wave ripple effects with color cycling
/// </summary>
public class WaveBehavior : IFieldBehavior
{
    public double Frequency { get; set; } = 3.0;
    public double Amplitude { get; set; } = 1.0;
    public double WaveSpeed { get; set; } = 2.0;
    public bool FadeWithDistance { get; set; } = true;
    public int WaveRings { get; set; } = 3;
    
    private double _time = 0;
    
    public FieldEffectResult Apply(Point dotPosition, Point sourcePosition, Size sourceSize, 
                                  double normalizedDistance, double baseStrength)
    {
        // Update time for animation (approximate)
        _time += 0.016; // ~60fps
        
        // Calculate wave effect
        var wavePhase = (_time * Frequency * 2 * Math.PI) - (normalizedDistance * WaveSpeed);
        var waveValue = Math.Sin(wavePhase) * Amplitude;
        
        // Apply distance fading if enabled
        if (FadeWithDistance)
        {
            waveValue *= baseStrength;
        }
        
        // Create ripple displacement (perpendicular to radius)
        var sourceCenter = new Point(sourcePosition.X + sourceSize.Width / 2, sourcePosition.Y + sourceSize.Height / 2);
        var vectorFromSource = new Vector(dotPosition.X - sourceCenter.X, dotPosition.Y - sourceCenter.Y);
        var distance = vectorFromSource.Length;
        var tangentDirection = distance > 0 ? new Vector(-vectorFromSource.Y, vectorFromSource.X) / distance : new Vector(1, 0);
        var displacement = tangentDirection * waveValue * 0.2; // 20% max displacement
        
        // Color cycling based on wave phase
        var hue = (wavePhase / (2 * Math.PI)) % 1.0;
        var color = HSVtoRGB(hue, 0.8, Math.Max(0, Math.Min(1, Math.Abs(waveValue))));
        
        return new FieldEffectResult
        {
            HasEffect = true,
            Displacement = displacement,
            Color = color,
            SizeMultiplier = 1.0,
            OpacityMultiplier = 1.0,
            Ring = (int)(normalizedDistance / (4.0 / WaveRings)) + 1,
            NormalizedDistance = normalizedDistance
        };
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
