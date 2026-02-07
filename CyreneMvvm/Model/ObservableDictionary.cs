using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CyreneMvvm.Model;

public class ObservableDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>,
    IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>,
    IReadOnlyCollection<KeyValuePair<TKey, TValue>>, INotifyCollectionChanged, INotifyPropertyChanged where TKey : notnull
{
    #region Internal

    private readonly Dictionary<TKey, TValue> Internal;

    public ObservableDictionary()
    {
        Internal = [];
    }

#pragma warning disable IDE0028

    public ObservableDictionary(int capacity)
    {
        Internal = new(capacity);
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        Internal = new(dictionary);
    }

#pragma warning restore IDE0028

    public TValue this[TKey key]
    {
        get => Internal[key];
        set
        {
            var isNewKey = !Internal.ContainsKey(key);
            var oldValue = isNewKey ? default : Internal[key];
            Internal[key] = value;

            if (isNewKey)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, oldValue!)));
        }
    }

    public ICollection<TKey> Keys => Internal.Keys;
    public ICollection<TValue> Values => Internal.Values;
    public int Count => Internal.Count;
    public bool IsReadOnly => false;

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Add(TKey key, TValue value)
    {
        Internal.Add(key, value);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
    }

    public void Clear()
    {
        Internal.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)Internal).Contains(item);
    }

    public bool ContainsKey(TKey key)
    {
        return Internal.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)Internal).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return Internal.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Remove(TKey key)
    {
        return Remove(key, out _);
    }

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var removed = Internal.Remove(key, out value);
        if (removed) OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value!)));
        return removed;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        var removed = ((ICollection<KeyValuePair<TKey, TValue>>)Internal).Remove(item);
        if (removed) OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, item));
        return removed;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return Internal.TryGetValue(key, out value);
    }

    public bool TryAdd(TKey key, TValue value)
    {
        var added = Internal.TryAdd(key, value);
        if (added) OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
        return added;
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
        OnPropertyChanged(nameof(Keys));
        OnPropertyChanged(nameof(Values));
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
