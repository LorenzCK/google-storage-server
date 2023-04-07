using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;

var storageBucketName = Environment.GetEnvironmentVariable("GOOGLE_STORAGE_BUCKET_NAME");
if(string.IsNullOrEmpty(storageBucketName)) {
    throw new ArgumentException("Google Storage bucket name must be set through GOOGLE_STORAGE_BUCKET_NAME environment variable");
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(builder => {
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()
            .Build();
    });
});

var app = builder.Build();
app.UseCors();

app.MapGet("favicon.ico", () => Results.NotFound());

app.MapGet("/{*path}", async ([FromRoute] string path, HttpContext httpContext, CancellationToken cancellationToken) => {
    app.Logger.LogInformation("Accessing {0}:{1}", storageBucketName, path);

    var memoryStream = new MemoryStream();

    try {
        var client = StorageClient.Create();
        var data = await client.DownloadObjectAsync(storageBucketName, path, memoryStream, cancellationToken: cancellationToken);

        memoryStream.Position = 0;
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.ContentType = data.ContentType;
        await memoryStream.CopyToAsync(httpContext.Response.Body, cancellationToken: cancellationToken);

        app.Logger.LogDebug("Served {0} bytes as {1}", memoryStream.Length, data.ContentType);
    }
    catch (Exception ex) {
        app.Logger.LogError(ex, "Failed to serve file");
    }
});

app.Run();
