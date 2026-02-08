using System;

namespace CyreneMvvm.Model;

public interface INotifyCallback
{
    void RegisterParent(object owner, Action callback);
    void UnregisterParent(object owner);
}