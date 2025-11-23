using System;
using Avalonia.Threading;

namespace DSV.Services;

public class UpdateService
{
    private readonly DispatcherTimer _updateTimer;
    public event Action? Tick;

    public UpdateService(TimeSpan interval)
    {
        _updateTimer = new DispatcherTimer { Interval = interval };
        _updateTimer.Tick += (s, e) => Tick?.Invoke();
    }
    
    public void Start() => _updateTimer.Start();
    public void Stop() => _updateTimer.Stop();
    
    public void Subscribe(Action action) =>  Tick += action;
    public void Unsubscribe(Action action) => Tick -= action;
}