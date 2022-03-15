using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Schedule.Info
{
    public class ExecutionThreadInfo
    {
        public Guid Guid { get; set; }
        public ThreadStatusEnum ThreadStatus { get; set; }
        public JobScheduleInfo JobScheduleInfo { get; set; }
    }
}
