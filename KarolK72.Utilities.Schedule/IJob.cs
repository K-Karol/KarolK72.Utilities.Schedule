using System;
using System.Threading;
using System.Threading.Tasks;

namespace KarolK72.Utilities.Schedule;

public interface IJob : IDisposable
{
    object ConfigurationObject { get; set; }
    Task Execute(CancellationToken? cancellationToken);
}

