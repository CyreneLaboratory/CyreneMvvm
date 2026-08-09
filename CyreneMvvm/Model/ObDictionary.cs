using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CyreneMvvm.Model;

public class ObDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>,
    IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>,
    IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>,
    INotifyCollectionChanged, INotifyCallback where TKey : notnull
{
    private readonly object SyncRoot = new();

    #region Internal

    private readonly Dictionary<TKey, TValue> Internal;

    public ObDictionary()
    {
        Internal = [];
    }

#pragma warning disable IDE0028

    public ObDictionary(IDictionary<TKey, TValue> dictionary)
    {
        Internal = new(dictionary);
        foreach (var item in Internal.Values)
        {
            if (TryIncrementCount(item)) RegisterValue(item);
        }
    }

    public ObDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    {
        Internal = new(collection);
        foreach (var item in Internal.Values)
        {
            if (TryIncrementCount(item)) RegisterValue(item);
        }
    }

    public ObDictionary(IEqualityComparer<TKey>? comparer)
    {
        Internal = new(comparer);
    }

    public ObDictionary(int capacity)
    {
        Internal = new(capacity);
    }

    public ObDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer)
    {
        Internal = new(dictionary, comparer);
        foreach (var item in Internal.Values)
        {
            if (TryIncrementCount(item)) RegisterValue(item);
        }
    }

    public ObDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer)
    {
        Internal = new(collection, comparer);
        foreach (var item in Internal.Values)
        {
            if (TryIncrementCount(item)) RegisterValue(item);
        }
    }

    public ObDictionary(int capacity, IEqualityComparer<TKey>? comparer)
    {
        Internal = new(capacity, comparer);
    }

#pragma warning restore IDE0028

    public TValue this[TKey key]
    {
        get
        {
            lock (SyncRoot)
            {
                return Internal[key];
            }
        }
        set
        {
            bool containsKey;
            TValue oldValue;
            var shouldUnregister = false;
            bool shouldRegister;
            lock (SyncRoot)
            {
                containsKey = Internal.ContainsKey(key);
                oldValue = containsKey ? Internal[key] : default!;
                if (containsKey && EqualityComparer<TValue>.Default.Equals(oldValue, value)) return;

                Internal[key] = value;
                
                if (containsKey) shouldUnregister = TryDecrementCount(oldValue!);
                shouldRegister = TryIncrementCount(value);
                if (shouldUnregister) EnqueueUnregisterValue(oldValue!);
                if (shouldRegister) EnqueueRegisterValue(value);
            }

            DrainPendingValueCallbacks();

            if (!containsKey)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, oldValue!)));
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            lock (SyncRoot)
            {
                return [.. Internal.Keys];
            }
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            lock (SyncRoot)
            {
                return [.. Internal.Values];
            }
        }
    }

    public IEqualityComparer<TKey> Comparer
    {
        get
        {
            return Internal.Comparer;
        }
    }

    public int Count
    {
        get
        {
            lock (SyncRoot)
            {
                return Internal.Count;
            }
        }
    }

    public bool IsReadOnly => false;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
    {
        get
        {
            lock (SyncRoot)
            {
                return [.. Internal.Keys];
            }
        }
    }

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
    {
        get
        {
            lock (SyncRoot)
            {
                return [.. Internal.Values];
            }
        }
    }

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public void Add(TKey key, TValue value)
    {
        bool shouldRegister;
        lock (SyncRoot)
        {
            Internal.Add(key, value);
            shouldRegister = TryIncrementCount(value);
            if (shouldRegister) EnqueueRegisterValue(value);
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
    }

    public void Clear()
    {
        lock (SyncRoot)
        {
            if (Internal.Count == 0) return;

            foreach (var sub in CallbackCounts.Keys) EnqueueUnregisterValue(sub);
            CallbackCounts.Clear();
            Internal.Clear();
        }

        DrainPendingValueCallbacks();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        lock (SyncRoot)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Internal).Contains(item);
        }
    }

    public bool ContainsKey(TKey key)
    {
        lock (SyncRoot)
        {
            return Internal.ContainsKey(key);
        }
    }

    public bool ContainsValue(TValue value)
    {
        lock (SyncRoot)
        {
            return Internal.ContainsValue(value);
        }
    }

    public int EnsureCapacity(int capacity)
    {
        lock (SyncRoot)
        {
            return Internal.EnsureCapacity(capacity);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        List<KeyValuePair<TKey, TValue>> snapshot;
        lock (SyncRoot)
        {
            snapshot = [.. Internal];
        }

        return snapshot.GetEnumerator();
    }

    public bool Remove(TKey key) => Remove(key, out _);

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        bool removed;
        var shouldUnregister = false;
        lock (SyncRoot)
        {
            removed = Internal.Remove(key, out value);
            if (removed)
            {
                shouldUnregister = TryDecrementCount(value!);
                if (shouldUnregister) EnqueueUnregisterValue(value!);
            }
        }

        if (removed)
        {
            DrainPendingValueCallbacks();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value!)));
        }
        return removed;
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        bool removed;
        var shouldUnregister = false;
        lock (SyncRoot)
        {
            removed = ((ICollection<KeyValuePair<TKey, TValue>>)Internal).Remove(item);
            if (removed)
            {
                shouldUnregister = TryDecrementCount(item.Value);
                if (shouldUnregister) EnqueueUnregisterValue(item.Value);
            }
        }

        if (removed)
        {
            DrainPendingValueCallbacks();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }
        return removed;
    }

    public void TrimExcess()
    {
        lock (SyncRoot)
        {
            Internal.TrimExcess();
        }
    }

    public void TrimExcess(int capacity)
    {
        lock (SyncRoot)
        {
            Internal.TrimExcess(capacity);
        }
    }

    public bool TryAdd(TKey key, TValue value)
    {
        bool added;
        var shouldRegister = false;
        lock (SyncRoot)
        {
            added = Internal.TryAdd(key, value);
            if (added)
            {
                shouldRegister = TryIncrementCount(value);
                if (shouldRegister) EnqueueRegisterValue(value);
            }
        }

        if (added)
        {
            DrainPendingValueCallbacks();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
        }
        return added;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        lock (SyncRoot)
        {
            return Internal.TryGetValue(key, out value);
        }
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        lock (SyncRoot)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)Internal).CopyTo(array, arrayIndex);
        }
    }

    #endregion

    private readonly ConcurrentDictionary<object, Action> ParentObservers = [];
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    private readonly Dictionary<INotifyCallback, int> CallbackCounts = new(ReferenceEqualityComparer.Instance);
    private readonly Queue<Action> PendingValueCallbacks = new();
    private bool IsDrainingValueCallbacks;

    private bool TryIncrementCount(TValue item)
    {
        if (item is INotifyCallback sub)
        {
            CallbackCounts.TryGetValue(sub, out var count);
            CallbackCounts[sub] = count + 1;
            return count == 0;
        }
        return false;
    }

    private bool TryDecrementCount(TValue item)
    {
        if (item is INotifyCallback sub)
        {
            if (CallbackCounts.TryGetValue(sub, out var count))
            {
                if (count <= 1)
                {
                    CallbackCounts.Remove(sub);
                    return true;
                }
                CallbackCounts[sub] = count - 1;
            }
        }
        return false;
    }

    protected virtual void OnParentChanged()
    {
        foreach (var callback in ParentObservers.Values.ToArray()) callback();
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
        OnParentChanged();
    }

    public void RegisterParent(object owner, Action callback)
    {
        ParentObservers[owner] = callback;
    }

    public void UnregisterParent(object owner)
    {
        ParentObservers.TryRemove(owner, out _);
    }

    private void RegisterValue(TValue item)
    {
        if (item is INotifyCallback sub)
            sub.RegisterParent(this, OnParentChanged);
    }

    private void EnqueueRegisterValue(TValue item)
    {
        if (item is INotifyCallback sub)
            PendingValueCallbacks.Enqueue(() => sub.RegisterParent(this, OnParentChanged));
    }

    private void EnqueueUnregisterValue(TValue item)
    {
        if (item is INotifyCallback sub)
            PendingValueCallbacks.Enqueue(() => sub.UnregisterParent(this));
    }

    private void EnqueueUnregisterValue(INotifyCallback sub)
    {
        PendingValueCallbacks.Enqueue(() => sub.UnregisterParent(this));
    }

    private void DrainPendingValueCallbacks()
    {
        Action action;
        lock (SyncRoot)
        {
            if (IsDrainingValueCallbacks || PendingValueCallbacks.Count == 0) return;

            IsDrainingValueCallbacks = true;
            action = PendingValueCallbacks.Dequeue();
        }

        try
        {
            while (true)
            {
                action();

                lock (SyncRoot)
                {
                    if (PendingValueCallbacks.Count == 0)
                    {
                        IsDrainingValueCallbacks = false;
                        return;
                    }

                    action = PendingValueCallbacks.Dequeue();
                }
            }
        }
        catch
        {
            lock (SyncRoot)
            {
                IsDrainingValueCallbacks = false;
            }

            throw;
        }
    }
}
