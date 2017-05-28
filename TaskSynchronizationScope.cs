﻿using System;
using System.Threading.Tasks;

namespace WebTorrent
{
    public class TaskSynchronizationScope
    {
        private readonly object _lock = new object();
        private Task _currentTask;

        public Task RunAsync(Func<Task> task)
        {
            return RunAsync<object>(async () =>
            {
                await task();
                return null;
            });
        }

        public Task<T> RunAsync<T>(Func<Task<T>> task)
        {
            lock (_lock)
            {
                if (_currentTask == null)
                {
                    var currentTask = task();
                    _currentTask = currentTask;
                    return currentTask;
                }
                var source = new TaskCompletionSource<T>();
                _currentTask.ContinueWith(t =>
                {
                    var nextTask = task();
                    nextTask.ContinueWith(nt =>
                    {
                        if (nt.IsCompleted)
                            source.SetResult(nt.Result);
                        else if (nt.IsFaulted)
                            source.SetException(nt.Exception);
                        else
                            source.SetCanceled();

                        lock (_lock)
                        {
                            if (_currentTask.Status == TaskStatus.RanToCompletion)
                                _currentTask = null;
                        }
                    });
                });
                _currentTask = source.Task;
                return source.Task;
            }
        }
    }
}