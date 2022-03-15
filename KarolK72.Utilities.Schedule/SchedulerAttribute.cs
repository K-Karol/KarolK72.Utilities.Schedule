using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Utilities.Schedule
{
    /// <summary>
    /// This attribute is used to give all valid implementations of <see cref="IJobSchedule{TJob}"/> a valid <see cref="SchedulerName"/> that can be used in config files to identify the scheduler this job should use
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SchedulerAttribute : Attribute
    {
        private string _schedulerName;
        public string SchedulerName { get => _schedulerName; }
        public SchedulerAttribute(string schedulerName)
        {
            _schedulerName = schedulerName;
        }
    }
}
