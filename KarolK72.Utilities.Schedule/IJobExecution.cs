using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KarolK72.Schedule
{
    public interface IJobExecution
    {
        Task Execute(CancellationToken cancellationToken);
    }
}
