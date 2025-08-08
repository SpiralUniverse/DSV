using Avalonia;
using Avalonia.Controls;

namespace DSV.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.AttachDevTools();
        InitializeComponent();
    }
}