﻿using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using NLog.Targets;

namespace Final.BackupTool.Common.Operational
{
    public class StoreLogFile
    {
        public async Task Save()
        {
            var azureOperations = new AzureOperations();
            var container = await azureOperations.CreateOperationsContainerAsync(OperationalDictionary.LogContainer);

            var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName("f");
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            var filePath = Path.GetFullPath(fileTarget.FileName.Render(logEventInfo));

            var blob = container.GetAppendBlobReference(Path.GetFileName(filePath));

            if (!blob.Exists())
            {
                await blob.CreateOrReplaceAsync();
            }

            using (var file = File.OpenRead(filePath))
            {
                await blob.AppendFromStreamAsync(file);
            }

            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
