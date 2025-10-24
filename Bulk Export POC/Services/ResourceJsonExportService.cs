using Bulk_Export_POC.Helpers;
using Bulk_Export_POC.Models.Enums;
using Bulk_Export_POC.Utilities;
using LeapPatientService.Application.Abstractions;
using LeapPatientService.Application.Models;
using LeapPatientService.Application.Models.Entities.Common;
using LeapPatientService.Infrastructure.Factories;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;

namespace Bulk_Export_POC.Services
{
    public class ResourceJsonExportService
    {
        private readonly FileProcessorEPPlus _fileProcessor;

        private ConcurrentDictionary<ResourceType, Channel<string>> channels = new();

        public ResourceJsonExportService(FileProcessorEPPlus fileProcessor)
        {
            _fileProcessor = fileProcessor;
        }

        public async Task ProcessFileAsync(string inputFilePath, string outputFolderPath, CancellationToken ct, ProcessingScenario scenario = ProcessingScenario.BatchByBatch)
        {
            Console.WriteLine("Start Processing");
            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Input Excel file not found", inputFilePath);

            // Directory.CreateDirectory(outputFolderPath);

            List<Dictionary<string, string>> allRows = _fileProcessor.ReadExcel(inputFilePath);

            if (allRows.Count == 0)
            {
                Console.WriteLine("No data found.");
                return;
            }

            int[] resourceIDs = new[] { 1, 2, 8, 11, 12, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 63 };

            string connectionString = "Server=Release01;Database=fhir_CureMD;User Id=curemd;Password=cure2000;TrustServerCertificate=True;MultipleActiveResultSets=True;";

            List<Task> writerTasks = new List<Task>(resourceIDs.Length);

            foreach (int rid in resourceIDs)
            {
                ct.ThrowIfCancellationRequested();

                ResourceType resourceType = (ResourceType)rid;

                Channel<string> ch = Channel.CreateBounded<string>(new BoundedChannelOptions(1024)
                {
                    SingleWriter = false,
                    SingleReader = true,
                    FullMode = BoundedChannelFullMode.Wait
                });

                channels[resourceType] = ch;
                string key = resourceType.ToString();
                writerTasks.Add(Task.Run(() => WriteJsonToFileAsync(key, ch.Reader, outputFolderPath,ct),ct));
            }

            switch (scenario)
            {
                case ProcessingScenario.RowByRow:
                    await ProcessRowByRowAsync(allRows, resourceIDs, connectionString, ct);
                    break;

                case ProcessingScenario.BatchByBatch:
                    await ProcessBatchByBatchAsync(allRows, resourceIDs, connectionString, ct);
                    break;
            }

            foreach (var ch in channels.Values)
                ch.Writer.Complete();

            await Task.WhenAll(writerTasks);

            Console.WriteLine("Completed.");
        }


        private static async Task WriteJsonToFileAsync(string resourceType, ChannelReader<string> reader, string outputFolderPath, CancellationToken ct)
        {
            string sanitizedName = string.Join("_", resourceType.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(outputFolderPath, $"{sanitizedName}.ndjson");

            //Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Started writer for {resourceType}");

            using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var writer = new StreamWriter(stream);

            await foreach (string jsonLine in reader.ReadAllAsync(ct))
            {
                //Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] writing for {resourceType}");
                await writer.WriteLineAsync(jsonLine.AsMemory(), ct);
            }

            //Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Finished writing {resourceType}");
        }

        private async Task ProcessRowByRowAsync(List<Dictionary<string, string>> allRows, int[] resourceIDs, string connectionString, CancellationToken ct)
        {
            try
            {
                Console.WriteLine("Starting Parallel processing loop");

                await Parallel.ForEachAsync(allRows, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct }, async (row, token) =>
                {
                    if (!int.TryParse(row["PatientID (PMPTXFT.IPATID)"], out var patientId))
                        return;

                    using SqlConnection sqlConnection = new SqlConnection(connectionString);
                    await sqlConnection.OpenAsync(token);

                    HandlerRepositoryFactory handlerRepositoryFactory = new HandlerRepositoryFactory();

                    foreach (int rid in resourceIDs)
                    {
                        ResourceType rt = (ResourceType)rid;
                        IEntityHandler handler = handlerRepositoryFactory.GetPatientResourceHandler(rt, sqlConnection);
                        EntityCollection? entityCollection = await handler.HandleEntity(patientId);
                        if (entityCollection is null) continue;

                        if (!EntityCollectionMap.Dictionary.TryGetValue(rt, out var getList))
                            continue;

                        List<BaseEntity>? items = getList(entityCollection);
                        if (items is null || items.Count == 0) continue;

                        if (!EntityCollectionMap.BaseEntityDictionary.TryGetValue(rt, out var toJson))
                            continue;

                        if (channels.TryGetValue(rt, out var ch))
                        {
                            foreach (var item in items)
                            {
                                string json = toJson(item);
                                await ch.Writer.WriteAsync(json, token);
                            }
                        }
                    }
                });

                Console.WriteLine("Ending Parallel processing loop");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task ProcessBatchByBatchAsync(List<Dictionary<string, string>> allRows, int[] resourceIDs, string connectionString, CancellationToken ct)
        {
            try
            {
                Console.WriteLine("Starting Parallel processing loop");

                var totalRows = allRows.Count;
                int numberOfBatches = 4;
                int rowsPerBatch = (int)Math.Ceiling((double)totalRows / numberOfBatches);

                var batches = Enumerable.Range(0, numberOfBatches)
                    .Select(i => allRows.Skip(i * rowsPerBatch).Take(rowsPerBatch).ToList())
                    .Where(batch => batch.Any())
                    .ToList();

                //await Parallel.ForEachAsync(batches, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct }, async (batch, token) =>
                //{
                //    foreach (var row in batch)
                //    {
                //        if (!int.TryParse(row["PatientID (PMPTXFT.IPATID)"], out var patientId))
                //            continue;

                //        using SqlConnection sqlConnection = new SqlConnection(connectionString);
                //        await sqlConnection.OpenAsync(token);

                //        HandlerRepositoryFactory handlerRepositoryFactory = new HandlerRepositoryFactory();

                //        foreach (int rid in resourceIDs)
                //        {
                //            ResourceType rt = (ResourceType)rid;
                //            IEntityHandler handler = handlerRepositoryFactory.GetPatientResourceHandler(rt, sqlConnection);
                //            EntityCollection? entityCollection = await handler.HandleEntity(patientId);
                //            if (entityCollection is null) continue;

                //            if (!EntityCollectionMap.Dictionary.TryGetValue(rt, out var getList))
                //                continue;

                //            List<BaseEntity>? items = getList(entityCollection);
                //            if (items is null || items.Count == 0) continue;

                //            if (!EntityCollectionMap.BaseEntityDictionary.TryGetValue(rt, out var toJson))
                //                continue;

                //            if (channels.TryGetValue(rt, out var ch))
                //            {
                //                foreach (var item in items)
                //                {
                //                    string json = toJson(item);
                //                    await ch.Writer.WriteAsync(json, token);
                //                }
                //            }
                //        }
                //    }
                //});

                //await Parallel.ForEachAsync(batches, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct }, async (batch, token) =>
                //{
                //    using SqlConnection sqlConnection = new SqlConnection(connectionString);
                //    await sqlConnection.OpenAsync(token);

                //    HandlerRepositoryFactory handlerRepositoryFactory = new HandlerRepositoryFactory();

                //    var rowTasks = batch.Select(async row =>
                //    {
                //        if (!int.TryParse(row["PatientID (PMPTXFT.IPATID)"], out var patientId))
                //            return;

                //        foreach (int rid in resourceIDs)
                //        {
                //            ResourceType rt = (ResourceType)rid;
                //            IEntityHandler handler = handlerRepositoryFactory.GetPatientResourceHandler(rt, sqlConnection);
                //            EntityCollection? entityCollection = await handler.HandleEntity(patientId);
                //            if (entityCollection is null) continue;

                //            if (!EntityCollectionMap.Dictionary.TryGetValue(rt, out var getList))
                //                continue;

                //            List<BaseEntity>? items = getList(entityCollection);
                //            if (items is null || items.Count == 0) continue;

                //            if (!EntityCollectionMap.BaseEntityDictionary.TryGetValue(rt, out var toJson))
                //                continue;

                //            if (channels.TryGetValue(rt, out var ch))
                //            {
                //                foreach (var item in items)
                //                {
                //                    string json = toJson(item);
                //                    await ch.Writer.WriteAsync(json, token);
                //                }
                //            }
                //        }
                //    });

                //    await Task.WhenAll(rowTasks);
                //});


                //Console.WriteLine("Ending Parallel processing loop");
                await Parallel.ForEachAsync(
                batches,
                new ParallelOptions { MaxDegreeOfParallelism = numberOfBatches, CancellationToken = ct },
                async (batch, token) =>
                {
                    await using SqlConnection sqlConnection = new(connectionString);
                    await sqlConnection.OpenAsync(token);

                    var handlerFactory = new HandlerRepositoryFactory();

                    // Limit internal per-batch concurrency to prevent SQL overload
                    using SemaphoreSlim semaphore = new(initialCount: 8); 
                    var tasks = new List<Task>();

                    foreach (var row in batch)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            await semaphore.WaitAsync(token);
                            try
                            {
                                if (!int.TryParse(row["PatientID (PMPTXFT.IPATID)"], out var patientId))
                                    return;

                                foreach (int rid in resourceIDs)
                                {
                                    ResourceType rt = (ResourceType)rid;
                                    IEntityHandler handler = handlerFactory.GetPatientResourceHandler(rt, sqlConnection);
                                    EntityCollection? entityCollection = await handler.HandleEntity(patientId);

                                    if (entityCollection is null) continue;
                                    if (!EntityCollectionMap.Dictionary.TryGetValue(rt, out var getList)) continue;

                                    List<BaseEntity>? items = getList(entityCollection);
                                    if (items is null || items.Count == 0) continue;

                                    if (!EntityCollectionMap.BaseEntityDictionary.TryGetValue(rt, out var toJson)) continue;
                                    if (!channels.TryGetValue(rt, out var ch)) continue;

                                    foreach (var item in items)
                                    {
                                        string json = toJson(item);
                                        await ch.Writer.WriteAsync(json, token);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error processing row: {ex.Message}");
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, token));
                    }

                    await Task.WhenAll(tasks);
                });

                    Console.WriteLine("Completed optimized batch processing.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
