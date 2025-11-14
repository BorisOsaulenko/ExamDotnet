using Azure;
using Azure.AI.ContentSafety;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models;
using Npgsql;
using Options;
using Repositories;
using Services.Azure;
using Services.Image;
using Services.ImageCollection;
using Services.Storage;
using Serilog;
using Services.User;

async Task SeedRolesAsync(IServiceProvider sp)
{
    var roles = new[] { "Admin", "Moderator", "BasicUser" };
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var rawConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    var connectionString = string.IsNullOrWhiteSpace(rawConnection)
        ? "Host=localhost;Database=app;Username=postgres;Password=postgres"
        : NormalizePostgresConnectionString(rawConnection);

    options.UseNpgsql(connectionString);
});

builder.Services
    .AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddScoped<ImageRepository>();
builder.Services.AddScoped<IImageRepository>(sp => sp.GetRequiredService<ImageRepository>());

builder.Services.AddScoped<ImageMetadataRepository>();
builder.Services.AddScoped<IImageMetadataRepository>(sp =>
    sp.GetRequiredService<ImageMetadataRepository>()
);

builder.Services.AddScoped<ImageStatsRepository>();
builder.Services.AddScoped<IImageStatsRepository>(sp =>
    sp.GetRequiredService<ImageStatsRepository>()
);
builder.Services.AddScoped<ImageCommentRepository>();
builder.Services.AddScoped<IImageCommentRepository>(sp =>
    sp.GetRequiredService<ImageCommentRepository>()
);
builder.Services.AddScoped<ImageCollectionRepository>();
builder.Services.AddScoped<IImageCollectionRepository>(sp =>
    sp.GetRequiredService<ImageCollectionRepository>()
);
builder.Services.AddScoped<UserPreferencesRepository>();
builder.Services.AddScoped<IUserPreferencesRepository>(sp =>
    sp.GetRequiredService<UserPreferencesRepository>()
);
builder.Services.AddScoped<UserConsumerHistoryRepository>();
builder.Services.AddScoped<IUserConsumerHistoryRepository>(sp =>
    sp.GetRequiredService<UserConsumerHistoryRepository>()
);
builder.Services.AddScoped<UserProducerHistoryRepository>();
builder.Services.AddScoped<IUserProducerHistoryRepository>(sp =>
    sp.GetRequiredService<UserProducerHistoryRepository>()
);
builder.Services.AddScoped<ImageAllowedUserRepository>();
builder.Services.AddScoped<IImageAllowedUserRepository>(sp =>
    sp.GetRequiredService<ImageAllowedUserRepository>()
);
builder.Services.AddScoped<ImageCollectionAllowedUserRepository>();
builder.Services.AddScoped<IImageCollectionAllowedUserRepository>(sp =>
    sp.GetRequiredService<ImageCollectionAllowedUserRepository>()
);
builder.Services.AddScoped<UserFavoriteTagRepository>();
builder.Services.AddScoped<IUserFavoriteTagRepository>(sp =>
    sp.GetRequiredService<UserFavoriteTagRepository>()
);
builder.Services.AddScoped<ImageTagRepository>();
builder.Services.AddScoped<IImageTagRepository>(sp => sp.GetRequiredService<ImageTagRepository>());
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<UserRepository>());

builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IImageMetadataService, ImageMetadataService>();
builder.Services.AddScoped<IImageStatsService, ImageStatsService>();
builder.Services.AddScoped<IImageCommentService, ImageCommentService>();
builder.Services.AddScoped<IImageAllowedUserService, ImageAllowedUserService>();
builder.Services.AddScoped<IImageTagService, ImageTagService>();
builder.Services.AddScoped<IImageCollectionService, ImageCollectionService>();
builder.Services.AddScoped<IImageCollectionAllowedUserService, ImageCollectionAllowedUserService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IUserConsumerHistoryService, UserConsumerHistoryService>();
builder.Services.AddScoped<IUserProducerHistoryService, UserProducerHistoryService>();
builder.Services.AddScoped<IUserFavoriteTagService, UserFavoriteTagService>();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection("BlobStorage"));
builder.Services.Configure<ComputerVisionOptions>(
    builder.Configuration.GetSection("ComputerVision")
);

builder.Services.AddSingleton(provider =>
{
    var options = provider.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
    var credential = new StorageSharedKeyCredential(options.AccountName, options.AccountKey);
    var serviceUri = new Uri($"https://{options.AccountName}.blob.core.windows.net");
    return new BlobServiceClient(serviceUri, credential);
});

builder.Services.AddSingleton<BlobContainerClients>();
builder.Services.AddKeyedSingleton<BlobContainerClient>(
    "PublicImages",
    (sp, _) => sp.GetRequiredService<BlobContainerClients>().Container
);
builder.Services.AddKeyedSingleton<IBlobContainerClient>(
    "PublicImages",
    (sp, _) => new BlobContainerClientAdapter(sp.GetRequiredService<BlobContainerClients>().Container)
);

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<ComputerVisionOptions>>().Value;

    if (string.IsNullOrWhiteSpace(options.Endpoint) || string.IsNullOrWhiteSpace(options.ApiKey))
    {
        throw new InvalidOperationException("Computer Vision settings are missing.");
    }

    return new ContentSafetyClient(
        new Uri(options.Endpoint),
        new AzureKeyCredential(options.ApiKey)
    );
});
builder.Services.AddScoped<IComputerVision, ComputerVisionService>();

var app = builder.Build();
app.MapControllers();

using (var scope = app.Services.CreateScope())
    await SeedRolesAsync(scope.ServiceProvider);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.Run();

static string NormalizePostgresConnectionString(string connectionString)
{
    if (
        !connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        && !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
    )
    {
        return connectionString;
    }

    var databaseUri = new Uri(connectionString);

    var userInfo = databaseUri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
        Username = username,
        Password = password,
        Database = databaseUri.AbsolutePath.TrimStart('/'),
    };

    builder.SslMode = SslMode.Require;

    var query = databaseUri.Query.TrimStart('?');
    if (!string.IsNullOrWhiteSpace(query))
    {
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

            switch (key.ToLowerInvariant())
            {
                case "sslmode":
                    builder.SslMode = Enum.Parse<SslMode>(value, true);
                    break;
                case "channel_binding":
                    builder["Channel Binding"] = value;
                    break;
                default:
                    builder[key] = value;
                    break;
            }
        }
    }

    return builder.ConnectionString;
}
