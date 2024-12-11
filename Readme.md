# StreamReader

## Check duplicate from Database

### Uniquekey Comparison

```cs
const int BatchSize = 1000;

using (var reader = new StreamReader("largefile.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = new List<MyData>();
    foreach (var record in csv.GetRecords<MyData>())
    {
        records.Add(record);

        if (records.Count == BatchSize)
        {
            CheckDuplicates(records);
            records.Clear();
        }
    }

    if (records.Any())
    {
        CheckDuplicates(records);
    }
}

void CheckDuplicates(List<MyData> batch)
{
    // Convert batch to keys for database query
    var keys = batch.Select(r => r.UniqueKey).ToList();
    var existingKeys = _dbContext.MyTable
        .Where(x => keys.Contains(x.UniqueKey))
        .Select(x => x.UniqueKey)
        .ToHashSet();

    foreach (var record in batch)
    {
        if (existingKeys.Contains(record.UniqueKey))
        {
            Console.WriteLine($"Duplicate found: {record.UniqueKey}");
        }
    }
}
```

### Hashing Comparisons

```cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CsvHelper;
using Microsoft.EntityFrameworkCore;

namespace CSVHashingExample
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Path to the large CSV file
            string csvFilePath = "largefile.csv";

            // Process the CSV file and check for duplicates
            ProcessCsvFile(csvFilePath);
        }

        static void ProcessCsvFile(string filePath)
        {
            const int BatchSize = 1000;
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Batch processing for the large CSV file
            var recordsBatch = new List<MyData>();
            foreach (var record in csv.GetRecords<MyData>())
            {
                recordsBatch.Add(record);

                if (recordsBatch.Count >= BatchSize)
                {
                    CheckForDuplicates(recordsBatch);
                    recordsBatch.Clear();
                }
            }

            // Process any remaining records
            if (recordsBatch.Count > 0)
            {
                CheckForDuplicates(recordsBatch);
            }

            Console.WriteLine("CSV processing completed.");
        }

        static void CheckForDuplicates(List<MyData> recordsBatch)
        {
            // Compute hashes for the batch
            var hashes = recordsBatch.Select(record => new
            {
                Record = record,
                Hash = ComputeHash(record)
            }).ToList();

            // Query database for existing hashes
            using var dbContext = new MyDbContext();
            var existingHashes = dbContext.MyTable
                .Where(item => hashes.Select(h => h.Hash).Contains(item.DataHash))
                .Select(item => item.DataHash)
                .ToHashSet();

            // Check for duplicates
            foreach (var hash in hashes)
            {
                if (existingHashes.Contains(hash.Hash))
                {
                    Console.WriteLine($"Duplicate found for key: {hash.Record.UniqueKey}");
                }
            }
        }

        static string ComputeHash(MyData record)
        {
            using var hasher = SHA256.Create();
            string combinedData = $"{record.UniqueKey}|{record.SomeField}|{record.OtherField}";
            byte[] data = Encoding.UTF8.GetBytes(combinedData);
            return Convert.ToBase64String(hasher.ComputeHash(data));
        }
    }

    public class MyData
    {
        public string UniqueKey { get; set; }
        public string SomeField { get; set; }
        public string OtherField { get; set; }
    }

    public class MyDbContext : DbContext
    {
        public DbSet<MyTableEntity> MyTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Replace with your actual database connection string
            optionsBuilder.UseSqlServer("YourConnectionString");
        }
    }

    public class MyTableEntity
    {
        public int Id { get; set; }
        public string UniqueKey { get; set; }
        public string DataHash { get; set; }
    }
}

```