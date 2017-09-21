﻿using Microsoft.Build.Framework;

namespace Final.BackupTool.Common.Models
{
    public class OperationalModel
    {
        [Required]
        public string ProductionStorageConnectionString { get; set; }
        [Required]
        public string BackupStorageConnectionString { get; set; }
        [Required]
        public string OperationalStorageConnectionString { get; set; }

        public bool BackupTables { get; set; }
        public bool BackupBlobs { get; set; }
        public bool RestoreTables { get; set; }
        public bool RestoreBlobs { get; set; }
        public bool Force { get; set; }
        public string Start { get; set; }
        public string ContainerName { get; set; }
        public string TableName { get; set; }
        public string BlobName { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}