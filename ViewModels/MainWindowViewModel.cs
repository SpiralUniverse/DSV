using DSV.Models;

namespace DSV.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public GridSettings GridSettings => GridSettings.Instance;

    public CanvasViewModel CanvasViewModel { get; } = new();

    public object? SelectedObject => CanvasViewModel.SelectedObject;

    public MainWindowViewModel()
    {
        CanvasViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CanvasViewModel.SelectedObject))
            {
                OnPropertyChanged(nameof(SelectedObject));
            }
        };
    }

}
