using KarolK72.Utilities.Schedule.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KarolK72.Utilities.Schedule;

public class Scheduler : IDisposable
{
    #region private members
    private ConcurrentQueue<IJobSchedule> _jobScheduleQueue;
    private List<IJobSchedule> _jobsSchedules;
    private ILoggerFactory _loggerFactory;
    private ILogger<Scheduler> _logger;
    private ThreadStatusEnum _status = ThreadStatusEnum.NotInit;
    private List<ExecutionThread> _executionThreads;
    private Thread _pollingThread;
    private Exception _threadException;
    private bool _cancellationRequested = false;
    private int _executionThreadCount = 4;
    private event ThreadStatusChangedHandler _threadStatusChanged;
    private EventWaitHandle _waitHandle = new AutoResetEvent(false);
    //private CancellationToken _cancellationToken;
    #endregion
    #region locks
    //locks
    private object _statusLock = new object();
    private object _cancellationLock = new object();
    private object _exceptionLock = new object();
    private object _executionThreadLock = new object();
    private object _threadStatusChangedLock = new object();
    //private object _cancellationTokenLock = new object();
    private object _waitHandleLock = new object();
    private bool disposedValue;
    #endregion
    #region private properties
    private ThreadStatusEnum _PollingThreadStatus
    {
        get
        {
            lock (_statusLock)
            {
                return _status;
            }
        }
        set
        {
            lock (_statusLock)
            {
                _status = value;
            }
            _threadStatusChanged?.Invoke(this, new ThreadStatusChangedEventArgs(value));
        }
    }
    private Exception _ThreadException
    {
        get
        {
            lock (_exceptionLock)
            {
                return _threadException;
            }
        }
        set
        {
            lock (_exceptionLock)
            {
                _threadException = value;
            }
        }
    }
    private List<ExecutionThread> _ExecutionThreads
    {
        get
        {
            lock (_executionThreadLock)
            {
                return _executionThreads;
            }
        }
        set
        {
            lock (_executionThreadLock)
            {
                _executionThreads = value;
            }
        }
    }

    private EventWaitHandle _WaitHandle
    {
        get
        {
            lock (_waitHandleLock)
            {
                return _waitHandle;
            }
        }
        set
        {
            lock (_waitHandleLock)
            {
                _waitHandle = value;
            }
        }
    }

    //private CancellationToken _CancellationToken
    //{
    //    get
    //    {
    //        lock (_cancellationTokenLock)
    //        {
    //            return _cancellationToken;
    //        }
    //    }
    //    set
    //    {
    //        lock (_cancellationTokenLock)
    //        {
    //            _cancellationToken = value;
    //        }
    //    }
    //}
    #endregion

    public ThreadStatusEnum PollingThreadStatus { get => _PollingThreadStatus; }
    public bool CancellationRequested
    {
        get
        {
            lock (_cancellationLock)
            {
                return _cancellationRequested;
            }
        }
        set
        {
            lock (_cancellationLock)
            {
                _cancellationRequested = value;
            }
        }
    }
    public int ExecutionThreadCount { get => _executionThreadCount; }
    public List<ExecutionThread> ExecutionThreads
    {
        get => _ExecutionThreads;
    }

    public event ThreadStatusChangedHandler ThreadStatusChanged
    {
        add
        {
            lock (_threadStatusChangedLock)
            {
                _threadStatusChanged += value;
            }
        }
        remove
        {
            lock (_threadStatusChangedLock)
            {
                _threadStatusChanged -= value;
            }
        }
    }

    public void AddExecutionThreadStatusChangedHandler(ThreadStatusChangedHandler handler)
        => _ExecutionThreads.ForEach(et => et.ThreadStatusChanged += handler);

    public void RemoveExecutionthreadStatusChangedHandler(ThreadStatusChangedHandler handler)
        => _ExecutionThreads.ForEach(et => et.ThreadStatusChanged -= handler);

    public Scheduler(ILoggerFactory loggerFactory, int execThreadCount = 4)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<Scheduler>();
        _executionThreadCount = execThreadCount;
        _jobsSchedules = new List<IJobSchedule>();
        _jobScheduleQueue = new ConcurrentQueue<IJobSchedule>();
        _executionThreads = new List<ExecutionThread>(ExecutionThreadCount);
        for (int i = 0; i < ExecutionThreadCount; i++)
        {
            _executionThreads.Add(new ExecutionThread(_loggerFactory.CreateLogger<ExecutionThread>(), _jobScheduleQueue));
        }



    }

    public void AddJob(IJobSchedule job)
    {
        job.CalculateNextExecutionTime();
        _jobsSchedules.Add(job);
    }

    public void AddJob(IEnumerable<IJobSchedule> jobs)
    {
        jobs.ToList().ForEach(job => job.CalculateNextExecutionTime());
        _jobsSchedules.AddRange(jobs);
    }

    public void Start()
    {
        _WaitHandle.Reset();
        CancellationRequested = false;
        _logger.LogInformation("Starting Scheduler...");
        if (PollingThreadStatus != ThreadStatusEnum.NotInit && PollingThreadStatus != ThreadStatusEnum.Stopped)
            throw new Exception("Polling thread is already running a thread");

        _executionThreads.ForEach(thread => thread.Start());

        ThreadStart threadStart = new ThreadStart(Poll);
        threadStart += () =>
        {
            if (_ThreadException != null)
                _PollingThreadStatus = ThreadStatusEnum.Faulted;
            else
                _PollingThreadStatus = ThreadStatusEnum.Stopped;
        };

        _pollingThread = new Thread(threadStart) { IsBackground = false };

        _pollingThread.Start();
        _PollingThreadStatus = ThreadStatusEnum.Starting;
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        //_CancellationToken = cancellationToken;
        _WaitHandle.Reset();
        CancellationRequested = false;
        _logger.LogInformation("Starting Scheduler...");
        if (PollingThreadStatus != ThreadStatusEnum.NotInit && PollingThreadStatus != ThreadStatusEnum.Stopped)
            throw new Exception("Polling thread is already running a thread");

        _executionThreads.ForEach(thread => thread.Start());

        ThreadStart threadStart = new ThreadStart(Poll);
        threadStart += () =>
        {
            if (_ThreadException != null)
                _PollingThreadStatus = ThreadStatusEnum.Faulted;
            else
                _PollingThreadStatus = ThreadStatusEnum.Stopped;
        };

        _pollingThread = new Thread(threadStart) { IsBackground = false };

        _pollingThread.Start();
        _PollingThreadStatus = ThreadStatusEnum.Starting;

        cancellationToken.Register(() => Stop());

        await Task.Run(() =>
        {
            _WaitHandle.WaitOne();
        });




        //await Task.
    }

    public void Stop()
    {
        _logger.LogInformation("Stop called. Stopping...");

        _PollingThreadStatus = _PollingThreadStatus != ThreadStatusEnum.Stopped ? ThreadStatusEnum.Stopping : _PollingThreadStatus;
        List<Task> stopTasks = new List<Task>();

        foreach (var et in ExecutionThreads)
        {
            stopTasks.Add(Task.Run(() => et.Stop()));
        }

        Task.WaitAll(stopTasks.ToArray());

        if (_pollingThread != null)
        {
            if (_pollingThread.IsAlive)
            {
                CancellationRequested = true;
                _pollingThread.Join();
                _PollingThreadStatus = ThreadStatusEnum.Stopped;
            }
            _pollingThread = null;
        }
        CancellationRequested = false;

        _WaitHandle.Set();

        GC.Collect();
    }

    private void Poll()
    {
        if (PollingThreadStatus == ThreadStatusEnum.Starting)
            _PollingThreadStatus = ThreadStatusEnum.Running;

        _logger.LogInformation("{SchdT} Polling...");

        _jobsSchedules.ForEach(job => job.CalculateNextExecutionTime()); //remove this?

        while (!_cancellationRequested)
        {



            try
            {

                if (!ExecutionThreads.Where(et => et.Status == ThreadStatusEnum.Running).Any())
                {
                    throw new Exception("No execution threads available to use");
                }

                foreach (var job in _jobsSchedules)
                {
                    if (job.NextExecutionTime <= DateTime.Now)
                    {
                        _logger.LogInformation($"{{SchdT}} Queuing job: [{job.JobName} | {job.JobInstanceID} | {job.JobGroupID}]");
                        _jobScheduleQueue.Enqueue(job);
                        job.CalculateNextExecutionTime();
                    }
                }

                Thread.Sleep(500);
            }
            catch (Exception e)
            {
                _ThreadException = e;
                _PollingThreadStatus = ThreadStatusEnum.Stopping;
                return;
            }
        }

        _logger.LogInformation("Polling thread stopping...");
        return;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            foreach (var execThread in _executionThreads)
            {
                execThread.Dispose();
            }

            _executionThreads.Clear();

            if (_pollingThread != null)
            {
                if (_pollingThread.IsAlive)
                {
                    CancellationRequested = true;
                    _pollingThread.Join();
                }
            }

            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~Scheduler()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}