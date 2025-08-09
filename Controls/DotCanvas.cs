using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DSV.ViewModels;

namespace DSV.Controls;

public class DotCanvas : Control
{
    public static readonly StyledProperty<CanvasViewModel> ViewModelProperty =
        AvaloniaProperty.Register<DotCanvas, CanvasViewModel>(nameof(ViewModel));

    public CanvasViewModel ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (ViewModel == null || ViewModel.GridSettings == null)
            return;

        var brush = new SolidColorBrush(Color.FromRgb(0xdd, 0xb9, 0xf7));

        double pointerX = ViewModel.PointerX, pointerY = ViewModel.PointerY;
        double focusRadiusSquared = ViewModel.FocusRadius * ViewModel.FocusRadius;

        foreach (var dot in ViewModel.Dots)
        {
            double dx =  dot.PositionX - pointerX;
            double dy = dot.PositionY - pointerY;
            double distSq = dx * dx + dy * dy;

            double size = (distSq <= focusRadiusSquared)
                ? ViewModel.GridSettings.DotSize * 2
                : ViewModel.GridSettings.DotSize;

            context.DrawEllipse(brush, null, new Point(dot.PositionX, dot.PositionY), size / 2, size / 2);
        }
    }
}