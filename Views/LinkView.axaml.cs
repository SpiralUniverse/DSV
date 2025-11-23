using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DSV.Models;
using System;

namespace DSV.Views;

public partial class LinkView : UserControl
{
    private Link? _link;

    public LinkView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is Link link)
        {
            _link = link;
            // Subscribe to node changes
            if (_link.Source != null) _link.Source.PropertyChanged += OnNodeChanged;
            if (_link.Target != null) _link.Target.PropertyChanged += OnNodeChanged;
            
            UpdateLink();
        }
    }

    private void OnNodeChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Node.PositionX) || 
            e.PropertyName == nameof(Node.PositionY) ||
            e.PropertyName == nameof(Node.Width) ||
            e.PropertyName == nameof(Node.Height))
        {
            UpdateLink();
        }
    }

    private void UpdateLink()
    {
        if (_link?.Source == null || _link?.Target == null) return;

        var startCenter = new Point(_link.Source.PositionX + _link.Source.Width / 2, _link.Source.PositionY + _link.Source.Height / 2);
        var endCenter = new Point(_link.Target.PositionX + _link.Target.Width / 2, _link.Target.PositionY + _link.Target.Height / 2);

        var start = GetIntersectionPoint(startCenter, endCenter, _link.Source);
        var end = GetIntersectionPoint(endCenter, startCenter, _link.Target);

        // Calculate control points for cubic bezier
        var deltaX = Math.Abs(end.X - start.X) / 2;
        var p1 = new Point(start.X + deltaX, start.Y);
        var p2 = new Point(end.X - deltaX, end.Y);

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(start, false);
            context.CubicBezierTo(p1, p2, end);
            context.EndFigure(false);
        }

        LinkPath.Data = geometry;
    }

    private Point GetIntersectionPoint(Point center, Point target, Node node)
    {
        var dir = target - center;
        var angle = Math.Atan2(dir.Y, dir.X);
        var w = node.Width / 2;
        var h = node.Height / 2;

        if (node.IsCircle)
        {
            return center + new Point(Math.Cos(angle) * w, Math.Sin(angle) * w); // Assuming circle is width/2 radius
        }
        else if (node.IsRectangle)
        {
            // Rectangle intersection
            // https://math.stackexchange.com/questions/524853/find-intersection-of-line-and-rectangle
            var absCos = Math.Abs(Math.Cos(angle));
            var absSin = Math.Abs(Math.Sin(angle));
            
            var xDist = w / absCos;
            var yDist = h / absSin;

            var dist = Math.Min(xDist, yDist);
            return center + new Point(Math.Cos(angle) * dist, Math.Sin(angle) * dist);
        }
        else
        {
            // Fallback for Triangle/Hexagon (treat as Rectangle for now or Circle)
            // Treating as Circle is safer for arbitrary shapes
             return center + new Point(Math.Cos(angle) * w, Math.Sin(angle) * h);
        }
    }
}
