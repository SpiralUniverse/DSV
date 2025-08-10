using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DSV.ViewModels;

namespace DSV.Controls;

public class DotCanvas : Control
{
    public static readonly StyledProperty<CanvasViewModel> ViewModelProperty =
        AvaloniaProperty.Register<DotCanvas, CanvasViewModel>(nameof(ViewModel));

    // Cache brushes to avoid creating them every frame
    private static readonly IBrush DotBrush = new SolidColorBrush(Color.FromRgb(0xdd, 0xb9, 0xf7));

    public CanvasViewModel ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (ViewModel?.Dots == null)
            return;

        // Much simpler and faster - just use the dot's cached size
        foreach (var dot in ViewModel.Dots)
        {
            double radius = dot.size * 0.5;
            context.DrawEllipse(DotBrush, null, new Point(dot.PositionX, dot.PositionY), radius, radius);
        }
    }
}