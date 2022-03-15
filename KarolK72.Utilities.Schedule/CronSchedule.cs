using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Schedule
{
    [Scheduler("CronScheduler")]
    public class CronJobSchedule<TJob> : IJobSchedule<TJob> where TJob : IJob
    {
        #region Private Fields
        private DateTime? _nextExecutionTime = null;
        private DateTime? _startOfExecution = null;
        private DateTime? _endOfExecution = null;
        private Guid _jobGuid = Guid.Empty;
        private string _jobName = string.Empty;
        private string _jobInstanceID = string.Empty;
        private string _jobGroupID = string.Empty;
        private Type? _jobConfigurationType = null;
        private object? _jobConfiguration = null;
        #endregion

        #region Interface Properties
        public DateTime? NextExecutionTime => _nextExecutionTime;
        public Guid JobGuid => _jobGuid;
        public string JobName => _jobName;
        public string JobInstanceID => _jobInstanceID;
        public string JobGroupID => _jobGroupID;
        public Type? JobConfigurationType => _jobConfigurationType;
        #endregion

        #region Interface Methods
        public void CalculateNextExecutionTime()
        {
            throw new NotImplementedException();
        }

        public IJob CreateInstance()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
