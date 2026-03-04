using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace FADemo.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    /// <summary>
    /// 发送消息
    /// </summary>
    protected void Send<TMessage>(TMessage message) where TMessage : class
        => WeakReferenceMessenger.Default.Send(message);

    /// <summary>
    /// 注册消息接收
    /// </summary>
    protected void Register<TMessage>(Action<object, TMessage> handler) where TMessage : class
        => WeakReferenceMessenger.Default.Register<TMessage>(this, (r, m) => handler(r, m));
}