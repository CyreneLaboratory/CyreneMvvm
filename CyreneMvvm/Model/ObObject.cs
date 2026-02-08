using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CyreneMvvm.Model;

public abstract class ObObject : INotifyPropertyChanged, INotifyCallback
{
    private readonly Dictionary<object, Action> ParentObservers = [];
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnParentChanged()
    {
        foreach (var callback in ParentObservers.Values.ToArray()) callback();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propName));
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
        OnParentChanged();
    }

    public void RegisterParent(object owner, Action callback)
    {
        ParentObservers[owner] = callback;
    }

    public void UnregisterParent(object owner)
    {
        ParentObservers.Remove(owner);
    }
}
