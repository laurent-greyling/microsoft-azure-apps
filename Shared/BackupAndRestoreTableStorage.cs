using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace backup_storage.Shared
{
    public class BackupAndRestoreTableStorage
    {
        public static async Task BatchAndMoveTables(CloudStorageAccount storageAccount, CloudStorageAccount destStorageAccount,
            TransformManyBlock<CloudStorageAccount, CloudTable> fromAccountToTables)
        {
            var batchTables = new ActionBlock<CloudTable>(
                async tbl =>
                {
                    var query = new TableQuery();
                    var tblData = tbl.ExecuteQuery(query);

                    var tableClientDest = destStorageAccount.CreateCloudTableClient();
                    var tble = tableClientDest.GetTableReference(tbl.Name);
                    tble.CreateIfNotExists();

                    var batchData = new BatchBlock<TableOperation>(20);
                    foreach (var dtaEntity in tblData)
                    {
                        await batchData.SendAsync(TableOperation.InsertOrMerge(dtaEntity));
                    }

                    var copyTables = new ActionBlock<TableOperation[]>(prc =>
                    {
                        var batchOp = new TableBatchOperation();

                        foreach (var pr in prc)
                        {
                            batchOp.Add(pr);
                        }

                        tble.ExecuteBatch(batchOp);
                    });

                    batchData.LinkTo(copyTables);

                    batchData.Complete();
                    await batchData.Completion;
                    copyTables.Complete();
                    await copyTables.Completion;

                    //return batchOp;
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 10,
                    BoundedCapacity = 40
                });

            fromAccountToTables.LinkTo(batchTables);

            await fromAccountToTables.SendAsync(storageAccount);

            fromAccountToTables.Complete();
            await fromAccountToTables.Completion;
            batchTables.Complete();
            await batchTables.Completion;
        }
    }
}