using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Utilities.Schedule.CronScheduler
{
    public class CronJobSchedulerConfiguration : IJobSchedulerConfiguration
    {
        public string CronExpression { get; set; }
        public DateTime? StartOfExecution { get; set; }
        public DateTime? EndOfExecution { get; set; }
        public string JobInstanceID { get; set; }
        public string JobGroupID { get; set; }
    }
}
