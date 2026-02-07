using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace CyreneMvvm.Model;

public class ObservableList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>,
    IReadOnlyCollection<T>, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    #region Internal

    private readonly List<T> Internal;

    public ObservableList()
    {
        Internal = [];
    }

    public ObservableList(IEnumerable<T> collection)
    {
        Internal = [.. collection];
    }

    public T this[int index]
    {
        get => Internal[index];
        set
        {
            var oldItem = Internal[index];
            Internal[index] = value;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
        }
    }

    public int Count => Internal.Count;
    public bool IsReadOnly => false;

    public void Add(T item)
    {
        Internal.Add(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, Internal.Count - 1));
    }

    public void AddRange(IEnumerable<T> items)
    {
        Internal.AddRange(items);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Clear()
    {
        Internal.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item)
    {
        return Internal.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Internal.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Internal.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return Internal.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        Internal.Insert(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    public bool Remove(T item)
    {
        var index = Internal.IndexOf(item);
        var removed = Internal.Remove(item);
        if (removed) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        return removed;
    }

    public void RemoveAt(int index)
    {
        var item = Internal[index];
        Internal.RemoveAt(index);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
    }

    public int RemoveAll(Predicate<T> match)
    {
        var removed = Internal.RemoveAll(match);
        if (removed > 0) OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        return removed;
    }

    #endregion

    internal string? ParentPropertyName;
    internal Action<string?>? ParentPropertyChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected virtual void OnPropertyChanged(string? propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        ParentPropertyChanged?.Invoke(ParentPropertyName);
    }

    public void RegisterParentProp(Action<string?> parent, string? propName = null)
    {
        if (ParentPropertyChanged != null) return;

        ParentPropertyName = propName;
        ParentPropertyChanged = parent;
    }

    public void UnregisterParentProp()
    {
        ParentPropertyName = null;
        ParentPropertyChanged = null;
    }
}
