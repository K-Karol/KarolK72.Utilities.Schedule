using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Utilities.Schedule
{
    public interface IJobSchedulerConfiguration
    {
        string JobInstanceID { get; set; }
        string JobGroupID { get; set; }
    }
}
