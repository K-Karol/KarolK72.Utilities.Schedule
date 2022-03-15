using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Schedule.Info
{
    public class JobScheduleInfo
    {
        public DateTime? NextExecutionTime { get; set; }
        public Guid JobGuid { get; set; }
        public string JobName { get; set; }
        public string JobInstanceID { get; set; }
        public string JobGroupID { get; set; }
        public Type JobConfigurationType { get; set; }
    }
}
