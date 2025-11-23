using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DSV.Models;

public partial class Link : ObservableObject
{
    [ObservableProperty]
    private Node _source;

    [ObservableProperty]
    private Node _target;

    [ObservableProperty]
    private bool _isDirectional = true;

    public Link(Node source, Node target)
    {
        _source = source;
        _target = target;
    }
}
