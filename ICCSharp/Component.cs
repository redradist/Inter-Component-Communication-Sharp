using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ICCSharp
{
    /// <summary>
    /// This class represent ActiveObject pattern
    /// </summary>
    public class Component : TaskScheduler, IComponent
    {
        #region Private Fields
        private readonly bool _isFactoryOwner;
        private readonly TaskFactory _factory;
        private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();
        
        private int? _workerThreadId;
        private Thread? _workerThread;
        private volatile bool _stopped;
        private volatile bool _passive;
        #endregion

        /// <summary>
        /// Constructor for creating main Component
        /// </summary>
        public Component()
        {
            _isFactoryOwner = true;
            _factory = new TaskFactory(this);
        }

        /// <summary>
        /// Constructor for creating Component based on provided TaskFactory
        /// </summary>
        /// <param name="factory">TaskFactory for starting Tasks</param>
        public Component(TaskFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Constructor for creating Component based on parent Component
        /// </summary>
        /// <param name="parent">Parent Component</param>
        public Component(IComponent parent)
        {
            _factory = parent.GetTaskFactory();
        }
        
        /// <summary>
        /// Method provides Tasks of TaskScheduler
        /// </summary>
        /// <returns>Enumerable class</returns>
        /// <exception cref="NotSupportedException"></exception>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken)
                {
                    return _tasks;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_tasks);
                }
            }
        }

        /// <summary>
        /// Add Task to internal Tasks Queue
        /// </summary>
        /// <param name="task">Task for adding to Queue</param>
        protected override void QueueTask(Task task)
        {
            _tasks.Enqueue(task);
            if (_passive)
            {
                lock (_tasks)
                {
                    _passive = false;
                    Monitor.Pulse(_tasks);
                }
            }
        }

        /// <summary>
        /// Method call by parent TaskScheduler when it tries to execute task either
        /// on the same Thread or
        /// Thread from ThreadPool
        /// </summary>
        /// <param name="task">Task to execute</param>
        /// <param name="taskWasPreviouslyQueued">Indicates if this task was previously queued</param>
        /// <returns>True - if task executed successfully, false - otherwise</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (task != null)
            {
                int currentThreadId = Thread.CurrentThread.ManagedThreadId;
                if (currentThreadId == _workerThreadId)
                {
                    lock (_tasks)
                    {
                        TryExecuteTask(task);
                        return true;
                    }
                }
            }
            return false;
        }

        public TaskFactory GetTaskFactory()
        {
            return _factory;
        }
        
        /// <summary>
        /// Starts a new Task.
        /// DO NOT WAIT ON TASK RETURNED BY THIS METHOD UNTIL CURRENT OR PARENT COMPONENT STARTED
        /// IN THREAD WHERE COMPONENT SHOULD START !!
        /// </summary>
        /// <param name="function">Delegate to start as Task</param>
        /// <typeparam name="TResult">Type of return value of delegate object</typeparam>
        /// <returns>
        /// Task which indicates if appropriate delegate started as Task.
        /// When this Task is finished it does not mean that delegate finished,
        /// it's just meant that Task started successfully
        /// </returns>
        public Task<TResult> StartTask<TResult>(Func<TResult> function)
        {
            return _factory.StartNew(function);
        }
        
        /// <summary>
        /// Method which run the Component.
        /// BE AWARE THAT THIS METHOD IS BLOCKING UNTIL COMPONENT IS STOPPED !!
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Exception is thrown in case if operation is not supported
        /// </exception>
        public virtual void Run()
        {
            if (!_isFactoryOwner)
            {
                throw new NotSupportedException("Operation is not supported !! You should start parent component instead !!");
            }
            if (_workerThreadId != null)
            {
                throw new NotSupportedException($"Component is already started in thread with id = {_workerThreadId}");
            }
            _workerThreadId = Thread.CurrentThread.ManagedThreadId;
            // Process all available items in the queue.
            while (!_stopped)
            {
                Task item;
                // Get the next item from the queue
                while (_tasks.Count > 0)
                {
                    if (_tasks.TryDequeue(out item))
                    {
                        TryExecuteTaskInline(item, taskWasPreviouslyQueued: true);
                    }
                }
                lock (_tasks)
                {
                    // When there are no more items to be processed,
                    // note that we're done processing, and get out.
                    if (_tasks.Count == 0)
                    {
                        _passive = true;
                        if (!_stopped)
                        {
                            Monitor.Wait(_tasks);
                        }
                    }  
                }
            }
        }

        /// <summary>
        /// Method which run the Component.
        /// BE AWARE THAT THIS METHOD IS BLOCKING UNTIL COMPONENT IS STOPPED !!
        /// </summary>
        /// <param name="isRunInThread">Indicates if this method should start in separate thread</param>
        public virtual void Run(bool isRunInThread)
        {
            if (isRunInThread)
            {
                _workerThread = new Thread(Run);
            }
            else
            {
                Run();
            }
        }

        /// <summary>
        /// Method which stop the Component
        /// </summary>
        public virtual void Stop()
        {
            _stopped = true;
            if (_passive)
            {
                lock (_tasks)
                {
                    Monitor.Pulse(_tasks);
                }
            }
        }

        /// <summary>
        /// This method waiting finishing workerThread if it was created for Component
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public virtual void Join()
        {
            if (_workerThread != null)
            {
                _workerThread.Join();
            }
            else
            {
                throw new NotSupportedException("Worker Thread is not started !!");
            }
        }
    }
}
