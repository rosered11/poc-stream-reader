using Azure.Storage.Blobs;
using ClosedXML.Excel;
using ExcelDataReader;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var blobConnectionString = "";
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
                    do
                    {
                        Console.WriteLine($"WorkSheet: {reader.Name}");
                        while (reader.Read())
                        {
                            for(int col = 0; col < reader.FieldCount; col++)
                                Console.Write($"{reader.GetValue(col)}\t");
                            Console.WriteLine();
                        }
                    } while (reader.NextResult());
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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}