using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarolK72.Utilities.Schedule;
public enum ThreadStatusEnum
{
    NotInit = 0,
    Starting = 1,
    Running = 2,
    Stopped = 3,
    Stopping = 4,
    Faulted = 5
}

public enum JobStatusEnum
{
    NotInit = 0,
    Dequeued = 1,
    Running = 2,
    Finished = 3,
    Stopped = 4,
    Stopping = 5,
    Faulted = 6
}