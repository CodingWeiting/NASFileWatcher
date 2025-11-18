using System;

namespace NASFileWatcher.Models
{
    public class StatusChangedEventArgs : EventArgs
    {
        public bool IsHealthy { get; set; }
        public string Message { get; set; }
    }
}
