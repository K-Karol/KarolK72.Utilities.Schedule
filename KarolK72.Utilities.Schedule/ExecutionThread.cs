using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KarolK72.Utilities.Schedule
{
    public class ExecutionThread : IDisposable
    {
        #region private members
        private ILogger<ExecutionThread> _logger;
        private Guid _id;
        private Thread _thread;
        private ThreadStatusEnum _status = ThreadStatusEnum.NotInit;
        private JobStatusEnum _jobStatus = JobStatusEnum.NotInit;
        private IJob _currentJob;
        private IJobSchedule _currentSchedule;
        private ConcurrentQueue<IJobSchedule> _jobQueue;
        private Exception _threadException;
        private bool _cancellationRequested = false;
        private event Events.ThreadStatusChangedHandler _threadStatusChanged;
        private event Events.JobExecutionStatusHandler _jobExecutionStatusChanged;
        private bool _disposedValue;
        #endregion
        #region private properties
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
        private ThreadStatusEnum _Status
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
                _threadStatusChanged?.Invoke(this, new Events.ThreadStatusChangedEventArgs(value));
            }
        }

        private JobStatusEnum _JobStatus
        {
            get
            {
                lock (_jobStatusEnumLock)
                {
                    return _jobStatus;
                }
            }
            set
            {
                lock (_jobStatusEnumLock)
                {
                    _jobStatus = value;
                }
                _jobExecutionStatusChanged?.Invoke(this, new Events.JobExecutionStatusEventArgs(CurrentSchedule, value));
            }
        }

        public IJobSchedule _CurrentSchedule
        {
            get
            {
                lock (_jobScheduleLock)
                {
                    return _currentSchedule;
                }
            }
            set
            {
                lock (_jobScheduleLock)
                {
                    _currentSchedule = value; ;
                }
            }

        }
        #endregion
        #region locks
        private object _statusLock = new object();
        private object _cancellationLock = new object();
        private object _exceptionLock = new object();
        private object _jobScheduleLock = new object();
        private object _threadStatusChangedLock = new object();
        private object _jobStatusEventLock = new object();
        private object _jobStatusEnumLock = new object();
        #endregion


        public IJobSchedule CurrentSchedule
        {
            get => _CurrentSchedule;

        }
        public ThreadStatusEnum Status
        {
            get => _Status;
        }
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
        public Guid ExecutionThreadID
        {
            get => _id;
        }

        public event Events.ThreadStatusChangedHandler ThreadStatusChanged
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
        public event Events.JobExecutionStatusHandler JobExecutionStatusChanged
        {
            add
            {
                lock (_jobStatusEventLock)
                {
                    _jobExecutionStatusChanged += value;
                }
            }
            remove
            {
                lock (_jobStatusEventLock)
                {
                    _jobExecutionStatusChanged -= value;
                }
            }
        }


        //public Info.ExecutionThreadInfo ExecutionThreadInfo
        //{
        //    get => new Info.ExecutionThreadInfo() { Guid = _id, ThreadStatus = _status, JobScheduleInfo = _currentSchedule.JobScheduleInfo};
        //}

        public ExecutionThread(ILogger<ExecutionThread> logger, ConcurrentQueue<IJobSchedule> jobQueue)
        {
            _logger = logger;
            _id = Guid.NewGuid();
            _jobQueue = jobQueue;
        }

        public void Start()
        {
            CancellationRequested = false;
            if (_Status != ThreadStatusEnum.NotInit && _Status != ThreadStatusEnum.Stopped)
                throw new Exception("ExecutionThread is already running a thread");

            _logger.LogInformation($"{{ExecT:{_id}}} Staring...");

            ThreadStart threadStart = new ThreadStart(Poll);
            threadStart += threadStopped;
            _thread = new Thread(threadStart) { IsBackground = true };

            _thread.Start();

            _Status = ThreadStatusEnum.Starting;
        }

        private void threadStopped()
        {
            if (_threadException != null)
                _Status = ThreadStatusEnum.Faulted;
            else
                _Status = ThreadStatusEnum.Stopped;
        }

        public void Stop()
        {
            if (_thread != null)
            {

                _Status = ThreadStatusEnum.Stopping;

                if (_thread.IsAlive)
                {
                    CancellationRequested = true;
                    _thread.Join();
                }

                _Status = ThreadStatusEnum.Stopped;
                CancellationRequested = false;

                _thread = null;
            }
        }

        private void Poll()
        {
            if (_Status == ThreadStatusEnum.Starting)
                _Status = ThreadStatusEnum.Running;

            _logger.LogInformation($"{{ExecT:{_id}}} Polling...");

            while (!_cancellationRequested)
            {
                try
                {
                    var jobAvailable = _jobQueue.TryDequeue(out var dequeuedSchedule);
                    if (jobAvailable)
                    {
                        _JobStatus = JobStatusEnum.Dequeued;
                        _CurrentSchedule = dequeuedSchedule;

                        _logger.LogDebug($"{{ExecT:{_id}}} Dequeued [{_CurrentSchedule.JobName} | {_CurrentSchedule.JobInstanceID} | {_CurrentSchedule.JobGroupID}]");
                        _currentJob = _CurrentSchedule.CreateInstance();
                        //Console.WriteLine($"[{_id}] Dequed job. Exe");
                        var jobTask = _currentJob.Execute(null);
                        _JobStatus = JobStatusEnum.Running;
                        jobTask.Wait();
                        _JobStatus = JobStatusEnum.Finished;
                        _logger.LogInformation($"{{ExecT:{_id}}} Finished executing [{_CurrentSchedule.JobName} | {_CurrentSchedule.JobInstanceID} | {_CurrentSchedule.JobGroupID}] ");
                        _currentJob.Dispose();
                        _currentJob = null;
                        _currentSchedule = null;

                    }
                    else
                    {
                        Thread.Sleep(500);
                    }

                }
                catch (Exception e)
                {
                    _ThreadException = e;
                    _Status = ThreadStatusEnum.Stopping;
                    _JobStatus = JobStatusEnum.Faulted;
                    return;
                }
            }

            _Status = ThreadStatusEnum.Stopping;
            return;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null

                if (_thread != null)
                {
                    if (_thread.IsAlive)
                    {
                        CancellationRequested = true;

                        _thread.Join();
                    }
                }

                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ExecutionThread()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
