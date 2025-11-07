using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Options;
using Repositories;
using Services.Image;
using Services.ImageCollection;
using Services.Identity;
using Services.User;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<ImageRepository>();
builder.Services.AddScoped<ImageStatsRepository>();
builder.Services.AddScoped<ImageCommentRepository>();
builder.Services.AddScoped<ImageCollectionRepository>();
builder.Services.AddScoped<UserPreferencesRepository>();
builder.Services.AddScoped<UserConsumerHistoryRepository>();
builder.Services.AddScoped<UserProducerHistoryRepository>();
builder.Services.AddScoped<ImageAllowedUserRepository>();
builder.Services.AddScoped<ImageCollectionAllowedUserRepository>();
builder.Services.AddScoped<UserFavoriteTagRepository>();
builder.Services.AddScoped<ImageTagRepository>();
builder.Services.AddScoped<UserRepository>();

builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IImageStatsService, ImageStatsService>();
builder.Services.AddScoped<IImageCommentService, ImageCommentService>();
builder.Services.AddScoped<IImageAllowedUserService, ImageAllowedUserService>();
builder.Services.AddScoped<IImageTagService, ImageTagService>();
builder.Services.AddScoped<IImageCollectionService, ImageCollectionService>();
builder.Services.AddScoped<IImageCollectionAllowedUserService, ImageCollectionAllowedUserService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<IUserConsumerHistoryService, UserConsumerHistoryService>();
builder.Services.AddScoped<IUserProducerHistoryService, UserProducerHistoryService>();
builder.Services.AddScoped<IUserFavoriteTagService, UserFavoriteTagService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.Configure<BlobStorageOptions>(builder.Configuration.GetSection("BlobStorage"));

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
    (sp, _) => sp.GetRequiredService<BlobContainerClients>().Public
);
builder.Services.AddKeyedSingleton<BlobContainerClient>(
    "PrivateImages",
    (sp, _) => sp.GetRequiredService<BlobContainerClients>().Private
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

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
