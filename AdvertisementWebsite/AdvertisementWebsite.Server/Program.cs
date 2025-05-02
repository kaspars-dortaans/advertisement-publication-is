using BusinessLogic.Authorization;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.BackgroundJobs;
using BusinessLogic.Helpers.CookieSettings;
using BusinessLogic.Helpers.EmailClient;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using BusinessLogic.Services;
using Hangfire;
using Hangfire.PostgreSql;
using ImageMagick;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using AdvertisementWebsite.Server.BackgroundJobs;
using AdvertisementWebsite.Server.Filters;
using AdvertisementWebsite.Server.Hubs;
using AdvertisementWebsite.Server.OpenApi;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

//CORS
var devCORSPolicy = "DevCORSPolicy";
services.AddCors(options =>
{
    options.AddPolicy(name: devCORSPolicy,
        policy =>
        {
            policy.WithOrigins("https://localhost:55243")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders([HeaderNames.ContentDisposition]);
        });
});

//Add db
var connectionString = builder.Configuration.GetConnectionString("Db");
services.AddDbContext<Context>(options =>
    options.UseNpgsql(connectionString)
);

if (builder.Environment.IsDevelopment())
{
    services.AddDatabaseDeveloperPageExceptionFilter();
}

//Add identity
services.AddIdentityApiEndpoints<User>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<Role>()
    .AddEntityFrameworkStores<Context>();

//Authentication
var tokenExpirationInMinutes = builder.Configuration.GetSection("AuthToken:TokenExpirationTimeInMinutes").Get<int>();
var refreshTokenExpirationInMinutes = builder.Configuration.GetSection("AuthToken:RefreshTokenExpirationTimeInMinutes").Get<int>();
services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, options =>
{
    options.BearerTokenExpiration = TimeSpan.FromMinutes(tokenExpirationInMinutes);
    options.RefreshTokenExpiration = TimeSpan.FromMinutes(refreshTokenExpirationInMinutes);

    // Docs: https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-9.0
    // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
    // due to a limitation in Browser APIs. We restrict it to only calls to the
    // SignalR hub in this code.
    options.Events = new BearerTokenEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/api/hubs/messages")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

//Authorization 
services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

//Controllers
services
    .AddControllers(options =>
        {
            //Add request filters
            options.Filters.Add<ApiExceptionFilter>();
        })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Add OpenApi
if (builder.Environment.IsDevelopment())
{
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(o =>
    {
        o.UseAllOfToExtendReferenceSchemas();

        var bearerScheme = new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT"
        };

        o.AddSecurityDefinition("Bearer", bearerScheme);
        o.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
        });

        ////Mappings
        o.MapType(typeof(IFormFile), () => new OpenApiSchema() { Type = "file", Format = "binary" });

        ////Filters
        o.OperationFilter<AddFromFormDtoOperationFilter>();
        o.DocumentFilter<IncludeDocumentFilter>();
    });
}

//Add Hangfire
services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Db"))));

services.AddHangfireServer();

//Add Options
services.AddOptions<StorageOptions>().Bind(builder.Configuration.GetSection("Storage"));
services.AddOptions<EmailOptions>().Bind(builder.Configuration.GetSection("Email"));

//Add localization
services.AddLocalization(opts => opts.ResourcesPath = "Resources");

//Register services here
services.AddSignalR();

services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
services.AddScoped<IUserService, UserService>();
services.AddScoped<IAdvertisementService, AdvertisementService>();
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<IFileService, FileService>();
services.AddScoped<IChatService, ChatService>();
services.AddScoped<IAdvertisementNotificationSubscriptionService, AdvertisementNotificationSubscriptionService>();
services.AddScoped<IAttributeValidatorService, AttributeValidatorService>();
services.AddScoped<IPaymentService, PaymentService>();

//Helpers
services.AddScoped<IStorage, LocalFileStorage>();
services.AddScoped<IFilePathResolver, FilePathResolver>();
services.AddScoped<IEmailClient, MailKitClient>();
services.AddScoped<CookieSettingsHelper>();
services.AddScoped<ImageHelper>();
services.AddScoped<IAdvertisementNotificationSender, AdvertisementNotificationSender>();

//Db seeding
if (builder.Environment.IsDevelopment())
{
    services.AddScoped<DbSeeder>();
}

//AutoMapper
services.AddAutoMapper(Assembly.GetExecutingAssembly());

//Initialize libs
MagickNET.Initialize();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //Seed db
    var scope = app.Services.CreateScope();
    var dbSeeder = scope.ServiceProvider.GetService<DbSeeder>();
    dbSeeder?.Seed();
}

// Configure the HTTP request pipeline.
// Docs: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-8.0#middleware-order
app.UseHttpsRedirection();

app.UseCors(devCORSPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapHangfireDashboard();

app.UseRequestLocalization(opts =>
{
    opts.RequestCultureProviders = [new CookieRequestCultureProvider()];
    opts.SupportedCultures = [
        new CultureInfo("en"),
        new CultureInfo("lv"),
    ];
    opts.SupportedUICultures = [
        new CultureInfo("en"),
        new CultureInfo("lv"),
    ];
    opts.DefaultRequestCulture = new RequestCulture(culture: "en", uiCulture: "en");
});
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapHub<MessageHub>("/api/hubs/messages");

app.Run();
