using System.Globalization;
using System.IO.Compression;
using System.Text;
using Azure;
using Azure.Storage.Blobs;
using ClosedXML.Excel;
using CsvHelper;
using ExcelDataReader;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.EnableForHttps = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

string blobConnectionString = "";
bool isClear = true;

app.MapGet("/normal", () =>
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("awb");
        BlobClient blobClient = containerClient.GetBlobClient("test.xlsx");
        List<string> dataTemp = new();
        if (blobClient.Exists())
        {
            #region ExcelDataReader is work, but isn't method Writer
            using (var blobStream = blobClient.OpenReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(blobStream))
                {
                    List<string> tracking = new();
                    int index = 0;
                    do
                    {
                        Console.WriteLine($"WorkSheet: {reader.Name}");
                        while (reader.Read())
                        {
                            for (int col = 0; col < reader.FieldCount; col++)
                            {
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                Console.Write($"{reader.GetValue(col)}\t");
                            }
                            index++;
                            Console.WriteLine();
                            if (isClear)
                                tracking.Clear();
                        }
                    } while (reader.NextResult());
                    Console.WriteLine($"Max records: {index}");
                }
            }
            #endregion
            
            #region ClosedXML is not work for large data

            // using (var blobStream =  blobClient.OpenReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            // using (var workbook = new XLWorkbook(blobStream))
            // {
            //     var worksheet = workbook.Worksheets.FirstOrDefault();
            //     foreach (var row in worksheet.RowsUsed())
            //     {
            //         foreach (var col in row.CellsUsed())
            //         {
            //             // dataTemp.Add(col.Value.ToString());
            //             Console.WriteLine($"{col.Value}\t");
            //         }
            //             
            //     }
            //     Console.WriteLine();
            // }
            // Console.WriteLine("End reader");

            #endregion
            
        }
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/normalClosedXMLCSV", async () =>
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("awb");
        BlobClient blobClient = containerClient.GetBlobClient("test.csv.gz");
        List<string> dataTemp = new();
        if (blobClient.Exists())
        {
            #region ClosedXML is not work for large data

            ProcessManagement processManagement = new();
            await processManagement.ProcessExcel(blobClient);

            #endregion
        }
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("NormalClosedXMLCSV")
    .WithOpenApi();

app.MapGet("/normalClosedXMLExcel", async () =>
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("awb");
        BlobClient blobClient = containerClient.GetBlobClient("test.xlsx.gz");
        List<string> dataTemp = new();
        if (blobClient.Exists())
        {
            #region ClosedXML is not work for large data

            ProcessManagement processManagement = new();
            await processManagement.ProcessExcel(blobClient);

            #endregion
        }
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("NormalClosedXMLExcel")
    .WithOpenApi();

app.MapGet("/gzip", () =>
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("awb");
        BlobClient blobClient = containerClient.GetBlobClient("test.xlsx.gz");
        List<string> dataTemp = new();
        if (blobClient.Exists())
        {
            #region ExcelDataReader is work, but isn't method Writer
            using (var blobStream = blobClient.OpenReadAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            using (MemoryStream memoryStream = new MemoryStream())
            using (var decompres = new GZipStream(blobStream, CompressionMode.Decompress))
            {
                decompres.CopyTo(memoryStream);
                // memoryStream.Seek(0, SeekOrigin.Begin);
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(memoryStream))
                {
                    List<string> tracking = new();
                    int index = 0;
                    do
                    {
                        Console.WriteLine($"WorkSheet: {reader.Name}");
                        while (reader.Read())
                        {
                            for (int col = 0; col < reader.FieldCount; col++)
                            {
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                tracking.Add(reader.GetString(col));
                                Console.Write($"{reader.GetValue(col)}\t");
                            }
                            index++;
                            Console.WriteLine();
                            if (isClear)
                                tracking.Clear();
                        }
                    } while (reader.NextResult());
                    Console.WriteLine($"Max records: {index}");
                }
            }
            #endregion
        }
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("Gzip")
    .WithOpenApi();

app.MapGet("/compressGzip",  async context =>
    {
        BlobServiceClient blobServiceClient = new BlobServiceClient(blobConnectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("awb");
        BlobClient blobClient = containerClient.GetBlobClient("test.xlsx");
        if (blobClient.Exists())
        {
            using (var blobStream = await blobClient.OpenReadAsync())
            using (MemoryStream compressedFileStream = new ())
            using (var compressor = new GZipStream(compressedFileStream, CompressionLevel.Optimal, leaveOpen: true))
            {
                await blobStream.CopyToAsync(compressor);
                await compressor.FlushAsync();
                compressedFileStream.Seek(0, SeekOrigin.Begin);
                context.Response.ContentType = "application/octet-stream";
                context.Response.Headers.ContentLength = compressedFileStream.Length;
                context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=\"mytest.xlsx.gz\"");
                await compressedFileStream.CopyToAsync(context.Response.Body);
            }
        }
        else
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Not found");
        }
    })
    .WithName("CompressGzip")
    .WithOpenApi();

app.MapGet("/flushMemory", () =>
    {
        GC.Collect();
    })
    .WithName("FlushMemory")
    .WithOpenApi();

app.MapPost("/uploadStream/directory/{directory}/fileName/{fileName}", async (string directory, string fileName,HttpRequest request, IWebHostEnvironment env) =>
    {
        // Check if the request stream of file
        if (request.ContentType is null || !request.ContentType.StartsWith("application/octet-stream"))
        {
            return Results.BadRequest("Invalid content type. Expected: application/octet-stream");
        }

        // Save the file to a directory
        var uploadsPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, directory);
        Directory.CreateDirectory(uploadsPath);

        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await request.Body.CopyToAsync(stream);
        }

        return Results.Ok(new { FilePath = filePath, Message = "File uploaded successfully." });
    })
    .WithName("UploadFile")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record ProcessManagement()
{
    public async Task ProcessCsv(BlobClient blobClient)
    {
        using (var blobStream = await blobClient.OpenReadAsync())
        using (MemoryStream memoryStream = new ())
        using (var decompres = new GZipStream(blobStream, CompressionMode.Decompress))
        {
            decompres.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using (StreamReader streamReader = new(memoryStream, Encoding.UTF8))
            using (var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<dynamic>();
                foreach (var record in records)
                {
                    var recordDict = (IDictionary<string, object>)record;
                    foreach (var property in recordDict)
                        Console.Write($"{property.Value}\t");
                    Console.WriteLine();
                }
            }
        }
        Console.WriteLine("End reader");
    }

    public async Task ProcessExcel(BlobClient blobClient)
    {
        using (var blobStream = await blobClient.OpenReadAsync())
        using (MemoryStream memoryStream = new MemoryStream())
        using (var decompres = new GZipStream(blobStream, CompressionMode.Decompress))
        {
            decompres.CopyTo(memoryStream);
            using (var workbook = new XLWorkbook(memoryStream))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault();
                foreach (var row in worksheet.RowsUsed())
                {
                    foreach (var col in row.CellsUsed())
                    {
                        Console.Write($"{col.Value}\t");
                    }
                    Console.WriteLine();       
                }
            }
        }
        Console.WriteLine("End reader");
    }
}
