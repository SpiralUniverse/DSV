using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace DSV.Models.FieldEffects;

/// <summary>
/// A field effect that combines multiple behavior strategies
/// Implements the Strategy Pattern for composable effects
/// </summary>
public class CompositeFieldEffect : FieldEffectBase
{
    /// <summary>
    /// Collection of behaviors to apply
    /// </summary>
    public List<IFieldBehavior> Behaviors { get; } = new();
    
    public CompositeFieldEffect()
    {
        Id = Guid.NewGuid().ToString();
    }
    
    /// <summary>
    /// Calculate the combined effect of all behaviors
    /// </summary>
    public override FieldEffectResult CalculateEffect(Point dotPosition, double dotSpacing)
    {
        if (!IsActive || Behaviors.Count == 0)
            return new FieldEffectResult { HasEffect = false };
            
        // Calculate normalized distance from field center
        var sourceCenter = new Point(Position.X + Size.Width / 2, Position.Y + Size.Height / 2);
        var vector = new Vector(dotPosition.X - sourceCenter.X, dotPosition.Y - sourceCenter.Y);
        var distance = vector.Length;
        var normalizedDistance = distance / dotSpacing;
        
        // Check if point is within max distance
        if (normalizedDistance > MaxDistance)
            return new FieldEffectResult { HasEffect = false };
            
        // Calculate base strength with falloff
        var baseStrength = Math.Max(0, 1.0 - (normalizedDistance / MaxDistance)) * Strength;
        
        // Apply all behaviors and combine results
        var combinedResult = new CombinedFieldEffectResult();
        
        foreach (var behavior in Behaviors)
        {
            var behaviorResult = behavior.Apply(dotPosition, Position, Size, normalizedDistance, baseStrength);
            if (behaviorResult.HasEffect)
            {
                combinedResult.AddBehaviorResult(behaviorResult);
            }
        }
        
        return combinedResult.ToFieldEffectResult();
    }
}

/// <summary>
/// Helper class to combine multiple behavior results
/// </summary>
public class CombinedFieldEffectResult
{
    private readonly List<FieldEffectResult> _results = new();
    
    public void AddBehaviorResult(FieldEffectResult result)
    {
        if (result.HasEffect)
        {
            _results.Add(result);
        }
    }
    
    public FieldEffectResult ToFieldEffectResult()
    {
        if (_results.Count == 0)
            return new FieldEffectResult { HasEffect = false };
            
        var combined = new FieldEffectResult { HasEffect = true };
        
        // Combine size multipliers (multiplicative)
        combined.SizeMultiplier = 1.0;
        foreach (var result in _results)
        {
            combined.SizeMultiplier *= result.SizeMultiplier;
        }
        
        // Combine opacity multipliers (multiplicative)
        combined.OpacityMultiplier = 1.0;
        foreach (var result in _results)
        {
            combined.OpacityMultiplier *= result.OpacityMultiplier;
        }
        
        // Combine displacements (additive)
        combined.Displacement = new Vector(0, 0);
        foreach (var result in _results)
        {
            combined.Displacement += result.Displacement;
        }
        
        // Use the last non-null color (behaviors can override each other)
        foreach (var result in _results)
        {
            if (result.Color.HasValue)
            {
                combined.Color = result.Color;
            }
        }
        
        // Use maximum ring value
        foreach (var result in _results)
        {
            if (result.Ring > combined.Ring)
            {
                combined.Ring = result.Ring;
                combined.NormalizedDistance = result.NormalizedDistance;
            }
        }
        
        return combined;
    }
}
