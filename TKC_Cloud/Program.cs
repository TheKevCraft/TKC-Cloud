using TKC_Cloud.Extensions;
using TKC_Cloud.Services.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

#region Builder

var config = CloudBootstrapper.CreateConfiguration(builder.Environment);

builder.Services.AddCloudConfiguration(config);

builder.Services.AddCloudServer(builder, config);

// Register Swagger services to generate API documentation.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable MVC controllers.
builder.Services.AddControllers();

builder.Services.AddCloudAuthentication();

builder.Services.AddCloudDatabase(config);

// Bind upload cleanup settings.
//
// Exapmle:
// "UploadCleanup": {
//   "ExpirationHoures": 24
// }
builder.Services.Configure<UploadCleanupSettings>(
    builder.Configuration.GetSection("UploadCleanup"));

builder.Services.AddCloudServices();

builder.Services.AddBackgroundServices();

builder.Services.AddStorageServices();

#endregion

var app = builder.Build();

#region App

app.UseCloudPipeline();

await app.InitializeApplicationAsync();

#endregion

// Start the web application.
app.Run();