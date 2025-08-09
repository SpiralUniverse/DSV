using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using DSV.ViewModels;
using System;
using DSV.Controls;

namespace DSV.Views;

public partial class CanvasView : UserControl
{
    public CanvasView()
    {
        InitializeComponent();
        DataContext = new CanvasViewModel();
        dotCanvas.PointerMoved += OnPointerMoved;
        this.SizeChanged += OnCanvasSizeChanged;
    }

    private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is CanvasViewModel viewModel)
        {
            viewModel.SetViewport(
                x: 0,
                y: 0,
                width: e.NewSize.Width,
                height: e.NewSize.Height
            );

            Console.WriteLine($"Canvas size changed: {e.NewSize.Width}x{e.NewSize.Height}");
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        //FIXME: way more laggy than expected
        if (DataContext is CanvasViewModel viewModel && sender is DotCanvas dotCanvas)
        {
            var position = e.GetPosition(this);
            viewModel.UpdatePointer(position.X, position.Y);
            dotCanvas.InvalidateVisual();
            Console.WriteLine($"Pointer moved to: {position}");
        }
    }
}