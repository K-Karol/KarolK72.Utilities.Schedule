using System;
using System.Collections.Generic;
using System.Text;

namespace KarolK72.Utilities.Schedule.Events
{
    public class ThreadStatusChangedEventArgs : EventArgs
    {
        private ThreadStatusEnum _status;
        public ThreadStatusEnum Status { get => _status; }

        public ThreadStatusChangedEventArgs(ThreadStatusEnum status)
        {
            _status = status;
        }
    }

    public delegate void ThreadStatusChangedHandler(object sender, ThreadStatusChangedEventArgs e);
}
