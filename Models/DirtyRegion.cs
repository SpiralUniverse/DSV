using Avalonia;

namespace DSV.Models;

/// <summary>
/// Represents a dirty region for efficient canvas updates
/// Based on Blender's spatial optimization approach
/// </summary>
public class DirtyRegion
{
    public Rect Bounds { get; set; }
    public bool IsDirty { get; set; }
    public int RegionX { get; set; }
    public int RegionY { get; set; }
    
    /// <summary>
    /// Last frame when this region was marked dirty
    /// </summary>
    public long LastDirtyFrame { get; set; }
    
    public DirtyRegion(int regionX, int regionY, Rect bounds)
    {
        RegionX = regionX;
        RegionY = regionY;
        Bounds = bounds;
        IsDirty = true; // Start dirty to ensure initial render
    }
    
    public void MarkDirty(long currentFrame)
    {
        IsDirty = true;
        LastDirtyFrame = currentFrame;
    }
    
    public void MarkClean()
    {
        IsDirty = false;
    }
    
    /// <summary>
    /// Check if a point intersects this region
    /// </summary>
    public bool Contains(Point point)
    {
        return Bounds.Contains(point);
    }
    
    /// <summary>
    /// Check if a rectangle intersects this region
    /// </summary>
    public bool Intersects(Rect rect)
    {
        return Bounds.Intersects(rect);
    }
}
