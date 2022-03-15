using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Utilities.Schedule.Events
{
    public class JobExecutionStatusEventArgs : EventArgs
    {
        private IJobSchedule _schedule;
        private JobStatusEnum _status;
        public IJobSchedule Schedule => _schedule;
        public JobStatusEnum Status => _status;

        public JobExecutionStatusEventArgs(IJobSchedule schedule, JobStatusEnum status)
        {
            _schedule = schedule;
            _status = status;
        }
    }

    public delegate void JobExecutionStatusHandler(object sender, JobExecutionStatusEventArgs e);
}
