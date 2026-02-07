using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CyreneMvvm.Model;

public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propName));
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue)) return false;
        field = newValue;
        OnPropertyChanged(propName);
        return true;
    }
}
