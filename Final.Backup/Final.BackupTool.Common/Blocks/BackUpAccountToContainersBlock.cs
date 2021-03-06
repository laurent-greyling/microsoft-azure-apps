﻿using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Final.BackupTool.Common.Operational;

namespace Final.BackupTool.Common.Blocks
{
    public static class BackUpAccountToContainersBlock
    {
        public static IPropagatorBlock<CloudStorageAccount, string> Create()
        {
            var azureOperation = new AzureOperations();
            return new TransformManyBlock<CloudStorageAccount, string>(
                account =>
                {
                    var blobClient = azureOperation.CreateProductionBlobClient();
                    blobClient.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(5), 5);
                    var containers = blobClient.ListContainers()
                    .Where(c =>
                    {
                        var n = c.Name.ToLowerInvariant();
                        return ExcludedContainers(n);
                    })
                    .Select(c => c.Name).ToList();

                    return containers;
                }
            );
        }

        private static bool ExcludedContainers(string n)
        {
            return !n.StartsWith(OperationalDictionary.Wad) &&
                   !n.StartsWith(OperationalDictionary.Azure) &&
                   !n.StartsWith(OperationalDictionary.CacheClusterConfigs) &&
                   !n.StartsWith(OperationalDictionary.ArmTemplates) &&
                   !n.StartsWith(OperationalDictionary.DeploymentLog) &&
                   !n.StartsWith(OperationalDictionary.DataDownloads) &&
                   !n.StartsWith(OperationalDictionary.Downloads) &&
                   !n.StartsWith(OperationalDictionary.StagedDashFiles) &&
                   !n.StartsWith(OperationalDictionary.StagedFiles) &&
                   !n.Contains(OperationalDictionary.StageArtifacts) &&
                   !n.StartsWith(OperationalDictionary.TableBackUpContainerName);
        }
    }
}
