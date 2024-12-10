using System.Net.Http.Headers;

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

app.MapPost("/upload", async (HttpRequest request) =>
    {
        // Check if the request contains a file
        if (!request.HasFormContentType)
        {
            return Results.BadRequest("Invalid form data.");
        }

        var form = await request.ReadFormAsync();
        var file = form.Files.FirstOrDefault(x => x.Name == "file");

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest("No file uploaded.");
        }

        // File validation (e.g., size, type)
        if (file.Length > 10 * 1024 * 1024) // 10 MB size limit
        {
            return Results.BadRequest("File size exceeds limit.");
        }

        var allowedExtensions = new[] { ".xlsx", ".xls", ".csv" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return Results.BadRequest("Unsupported file format.");
        }

        using var content = new StreamContent(file.OpenReadStream());
        string fileName = Path.GetRandomFileName() + extension;
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = fileName,
        };
        using HttpClient httpClient = new ();
        HttpResponseMessage response = await httpClient.PostAsync($"http://localhost:5002/upload/directory/upload1/fileName/{fileName}", content);
        
        if (!response.IsSuccessStatusCode)
            return Results.BadRequest();
        return Results.Ok();
    })
    .WithName("UploadFile")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
