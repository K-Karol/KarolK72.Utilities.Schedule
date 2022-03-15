using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Utilities.Schedule
{
    /// <summary>
    /// The <see cref="IJobSchedule{TJob}"/> is the abstraction for an implementation class that will provide the functionality for creating the <see cref="IJob"/>.
    /// This class contains all of the information about the <see cref="TJob"/> and should be able to create an instance of the <see cref="TJob"/> & be able to calculate its <see cref="NextExecutionTime"/>.
    /// An example of a valid implementation is the <see cref="CronJobSchedule{TJob}"/> that will use cron the schedule the job.
    /// </summary>
    /// <typeparam name="TJob">Type of the <see cref="IJob"/> that this schedule is representing</typeparam>
    public interface IJobSchedule
    {
        Guid JobGuid { get; }
        Type JobConfigurationType { get; }
        string JobName { get; }
        string JobInstanceID { get; }
        string JobGroupID { get; }

        DateTime? NextExecutionTime { get; }
        void CalculateNextExecutionTime();
        IJob CreateInstance();
    }
}
