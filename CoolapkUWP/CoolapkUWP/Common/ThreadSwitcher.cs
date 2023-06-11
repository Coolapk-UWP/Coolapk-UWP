using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.System.Threading;
using Windows.UI.Core;
using ThreadPool = Windows.System.Threading.ThreadPool;

namespace CoolapkUWP.Common
{
    public readonly struct DispatcherThreadSwitcher : INotifyCompletion
    {
        private readonly CoreDispatcher dispatcher;

        public bool IsCompleted => dispatcher.HasThreadAccess;

        internal DispatcherThreadSwitcher(CoreDispatcher dispatcher) => this.dispatcher = dispatcher;

        public void GetResult() { }

        public DispatcherThreadSwitcher GetAwaiter() => this;

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation();
            }
            else
            {
                _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => continuation());
            }
        }
    }

    public readonly struct ThreadPoolThreadSwitcher : INotifyCompletion
    {
        public bool IsCompleted => SynchronizationContext.Current == null;

        public void GetResult() { }

        public ThreadPoolThreadSwitcher GetAwaiter() => this;

        public void OnCompleted(Action continuation) => _ = ThreadPool.RunAsync(_ => continuation(), WorkItemPriority.Normal);
    }

    public static class ThreadSwitcher
    {
        public static DispatcherThreadSwitcher ResumeForegroundAsync(this CoreDispatcher dispatcher) => new DispatcherThreadSwitcher(dispatcher);

        public static ThreadPoolThreadSwitcher ResumeBackgroundAsync() => new ThreadPoolThreadSwitcher();
    }
}