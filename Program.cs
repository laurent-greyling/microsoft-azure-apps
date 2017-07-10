﻿using System;
using System.Diagnostics;
using backup_storage.Shared;
using CommandLine;
using Microsoft.WindowsAzure.Storage;

namespace backup_storage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var options = new Options();

            if (!Parser.Default.ParseArguments(args, options)) return;
            
            var storageAccount = CloudStorageAccount.Parse(options.StorageConnectionString); //CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            var destStorageAccount = CloudStorageAccount.Parse(options.DestStorageConnectionString); //CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DestStorageConnectionString"));
            
            var swTable = new Stopwatch();
            var swblob = new Stopwatch();

            if (options.FillStorage)
            {
                //Run this to n to create n tables and blobs, comment the for loop in if want to create bunch op info for copy
                //for (var i = 0; i < 10; i++)
                //{
                //Create and populate stuff
                Console.WriteLine("Creating and populating some more dummy tables....");
                CreateTableStorage.CreateAndPopulateTable(storageAccount);
                Console.WriteLine("Finished Creating and populating some more dummy tables....");

                Console.WriteLine("Creating and populating some more dummy blobs....");
                CreateBlobStorage.CreateAndPopulateBlob(storageAccount);
                Console.WriteLine("Finished Creating and populating some more dummy blobs....");
                //}
            }

            if (options.BackUp)
            {
                //Copy and backup table storage
                Console.WriteLine($"{Environment.NewLine}TABLE STORAGE");
                Console.WriteLine("Start copying table storage to new destination storage");
                swTable.Start();
                //BackupTableStorage.CopyTableStorage(storageAccount, destStorageAccount); //This is only in paralell
                BackupTableStorage.CopyAndBackUpTableStorage(storageAccount, destStorageAccount).Wait(); //This is with dataflow - this is the quicker copy method
                Console.WriteLine($"Finished copying table storage to new destination storage - {swTable.Elapsed}");
                swTable.Stop();

                Console.WriteLine($"{Environment.NewLine}BLOB STORAGE");
                //Copy and backup blob
                Console.WriteLine("Start copying blob to new destination storage");
                swblob.Start();
                //BackupBlobStorage.CopyBlobStorage(storageAccount, destStorageAccount); //This is only in paralell
                BackupBlobStorage.BackupBlobToStorage(storageAccount, destStorageAccount).Wait(); //This is only in DataFlow - somewhat faster, but currently by not much
                Console.WriteLine($"Finished copying blob to new destination storage - {swblob.Elapsed}");
                swblob.Stop();
            }

            if (options.Restore)
            {
                var x = "";
                //do stuff
            }
            Console.ReadKey();
        }
    }
}
