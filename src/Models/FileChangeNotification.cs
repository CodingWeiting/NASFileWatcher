using System;
using System.Collections.Generic;

namespace NASFileWatcher.Models
{
    public class FileChangeNotification
    {
        public string EventType { get; set; } // created, modified, deleted, renamed, batch
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string RelativePath { get; set; }
        public DateTime Timestamp { get; set; }
        public string OldName { get; set; } // 只有 renamed 才有值
    }

    public class BatchFileChangeNotification
    {
        public int TotalCount { get; set; }
        public int CreatedCount { get; set; }
        public int ModifiedCount { get; set; }
        public int DeletedCount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CommonPath { get; set; }
        public List<BatchFileInfo> Files { get; set; }
        public List<FolderStructureInfo> FolderStructure { get; set; }
    }

    public class BatchFileInfo
    {
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public string EventType { get; set; }
    }

    public class FolderStructureInfo
    {
        public string FolderPath { get; set; }
        public int TotalFiles { get; set; }
        public int CreatedCount { get; set; }
        public int ModifiedCount { get; set; }
        public int DeletedCount { get; set; }
        public int Depth { get; set; }
    }

    public class NotificationDisplayItem
    {
        public string TimeDisplay { get; set; }
        public string EventTypeDisplay { get; set; }
        public string FileName { get; set; }
        public string RelativePath { get; set; }
        public string OldName { get; set; }
    }
}
