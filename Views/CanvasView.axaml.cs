using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using DSV.ViewModels;
using System;

namespace DSV.Views;

public partial class CanvasView : UserControl
{
    public CanvasView()
    {
        InitializeComponent();
        DataContext = new CanvasViewModel();
        this.PointerMoved += OnPointerMoved;
        this.SizeChanged += OnCanvasSizeChanged;
    }

    private void OnCanvasSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is CanvasViewModel viewModel)
        {
            // Update the grid settings based on the new canvas size
            viewModel.GridSettings.Rows = (int)(e.NewSize.Height / viewModel.GridSettings.Spacing);
            viewModel.GridSettings.Columns = (int)(e.NewSize.Width / viewModel.GridSettings.Spacing);
            viewModel.GenerateGrid();
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is CanvasViewModel viewModel)
        {
            viewModel.UpdateGridFocus(e.GetPosition(this).X, e.GetPosition(this).Y);
        }
    }
}