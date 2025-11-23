using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace DSV.Models.FieldEffects;

/// <summary>
/// Manages all field effects in the system
/// Coordinates effect calculation, optimization, and animations
/// </summary>
public class FieldEffectManager
{
    private readonly List<FieldEffectBase> _fieldEffects = new();
    private readonly Dictionary<string, FieldEffectBase> _effectsById = new();
    
    /// <summary>
    /// Event fired when field effects change and dots need to be recalculated
    /// </summary>
    public event EventHandler<Rect>? FieldEffectsChanged;
    
    /// <summary>
    /// Add a field effect to the system
    /// </summary>
    public void AddFieldEffect(FieldEffectBase effect)
    {
        _fieldEffects.Add(effect);
        _effectsById[effect.Id] = effect;
        
        // Notify that the affected area needs updating
        NotifyFieldChanged(effect.GetAffectedBounds(20)); // TODO: Get actual dot spacing
    }
    
    /// <summary>
    /// Remove a field effect from the system
    /// </summary>
    public bool RemoveFieldEffect(string effectId)
    {
        if (!_effectsById.TryGetValue(effectId, out var effect))
            return false;
            
        var affectedBounds = effect.GetAffectedBounds(20); // TODO: Get actual dot spacing
        
        _fieldEffects.Remove(effect);
        _effectsById.Remove(effectId);
        
        // Notify that the affected area needs updating
        NotifyFieldChanged(affectedBounds);
        return true;
    }
    
    /// <summary>
    /// Get a field effect by ID
    /// </summary>
    public FieldEffectBase? GetFieldEffect(string effectId)
    {
        _effectsById.TryGetValue(effectId, out var effect);
        return effect;
    }
    
    /// <summary>
    /// Update all field effects for animation
    /// </summary>
    public void Update(double deltaTime)
    {
        foreach (var effect in _fieldEffects)
        {
            effect.Update(deltaTime);
        }
    }
    
    /// <summary>
    /// Calculate the combined effect of all fields on a specific dot
    /// </summary>
    public CombinedFieldEffect CalculateCombinedEffect(Point dotPosition, double dotSpacing)
    {
        var result = new CombinedFieldEffect();
        
        foreach (var effect in _fieldEffects.Where(e => e.IsActive))
        {
            // Quick bounds check for optimization
            if (!effect.GetAffectedBounds(dotSpacing).Contains(dotPosition))
                continue;
                
            var fieldResult = effect.CalculateEffect(dotPosition, dotSpacing);
            if (fieldResult.HasEffect)
            {
                result.AddEffect(fieldResult);
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Get the combined bounds of all active field effects
    /// Used for optimization when determining which dots to recalculate
    /// </summary>
    public Rect GetCombinedAffectedBounds(double dotSpacing)
    {
        if (_fieldEffects.Count == 0)
            return new();
            
        var activeEffects = _fieldEffects.Where(e => e.IsActive).ToList();
        if (activeEffects.Count == 0)
            return new();
            
        var firstBounds = activeEffects.First().GetAffectedBounds(dotSpacing);
        var combinedBounds = firstBounds;
        
        foreach (var effect in activeEffects.Skip(1))
        {
            combinedBounds = combinedBounds.Union(effect.GetAffectedBounds(dotSpacing));
        }
        
        return combinedBounds;
    }
    
    /// <summary>
    /// Clear all field effects
    /// </summary>
    public void Clear()
    {
        var allBounds = GetCombinedAffectedBounds(20); // TODO: Get actual dot spacing
        
        _fieldEffects.Clear();
        _effectsById.Clear();
        
        if (allBounds.Width > 0 && allBounds.Height > 0)
        {
            NotifyFieldChanged(allBounds);
        }
    }
    
    /// <summary>
    /// Get all active field effects
    /// </summary>
    public IEnumerable<FieldEffectBase> GetActiveEffects()
    {
        return _fieldEffects.Where(e => e.IsActive);
    }
    
    private void NotifyFieldChanged(Rect affectedBounds)
    {
        FieldEffectsChanged?.Invoke(this, affectedBounds);
    }
}

/// <summary>
/// Represents the combined result of multiple field effects on a single dot
/// </summary>
public class CombinedFieldEffect
{
    public bool HasAnyEffect { get; private set; }
    public Vector TotalDisplacement { get; private set; }
    public double TotalSizeMultiplier { get; private set; } = 1.0;
    public Color? FinalColor { get; private set; }
    public double TotalOpacityMultiplier { get; private set; } = 1.0;
    public int HighestRing { get; private set; }
    public double MinNormalizedDistance { get; private set; } = double.MaxValue;
    
    private readonly List<FieldEffectResult> _effects = new();
    
    public void AddEffect(FieldEffectResult effect)
    {
        if (!effect.HasEffect) return;
        
        _effects.Add(effect);
        HasAnyEffect = true;
        
        // Combine displacements (additive)
        TotalDisplacement += effect.Displacement;
        
        // Combine size multipliers (multiplicative)
        TotalSizeMultiplier *= effect.SizeMultiplier;
        
        // Combine opacity multipliers (multiplicative)
        TotalOpacityMultiplier *= effect.OpacityMultiplier;
        
        // Use the color from the strongest/closest effect
        if (effect.Color.HasValue && (FinalColor == null || effect.NormalizedDistance < MinNormalizedDistance))
        {
            FinalColor = effect.Color;
            MinNormalizedDistance = effect.NormalizedDistance;
        }
        
        // Track highest ring
        HighestRing = Math.Max(HighestRing, effect.Ring);
    }
    
    /// <summary>
    /// Get all individual effects for debugging
    /// </summary>
    public IReadOnlyList<FieldEffectResult> GetIndividualEffects() => _effects;
}
