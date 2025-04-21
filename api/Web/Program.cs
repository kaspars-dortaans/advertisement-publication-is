using BusinessLogic.Authentication;
using BusinessLogic.Authentication.Jwt;
using BusinessLogic.Authorization;
using BusinessLogic.Entities;
using BusinessLogic.Helpers;
using BusinessLogic.Helpers.CookieSettings;
using BusinessLogic.Helpers.FilePathResolver;
using BusinessLogic.Helpers.Storage;
using BusinessLogic.Services;
using ImageMagick;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Web.Filters;
using Web.Hubs;
using Web.OpenApi;

var builder = WebApplication.CreateBuilder(args);

//CORS
var devCORSPolicy = "DevCORSPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: devCORSPolicy,
        policy =>
        {
            policy.WithOrigins("https://localhost:5173", "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders([HeaderNames.ContentDisposition]);
        });
});

//Authentication
var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtAudience = builder.Configuration.GetSection("Jwt:Audience").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:SecretKey").Get<string>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtAudience,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
     };
     options.MapInboundClaims = false;

     // Based on: https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-9.0
     // Sending the access token in the query string is required when using WebSockets or ServerSentEvents
     // due to a limitation in Browser APIs. We restrict it to only calls to the
     // SignalR hub in this code.
     options.Events = new JwtBearerEvents
     {
         OnMessageReceived = context =>
         {
             var accessToken = context.Request.Query["access_token"];

             // If the request is for our hub...
             var path = context.HttpContext.Request.Path;
             if (!string.IsNullOrEmpty(accessToken) &&
                 (path.StartsWithSegments("/hubs/messages")))
             {
                 // Read the token out of the query string
                 context.Token = accessToken;
             }
             return Task.CompletedTask;
         }
     };
 });
//UserId provider for signalR
builder.Services.AddSingleton<IUserIdProvider, JwtTokenBasedUserIdProvider>();

//Authorization
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Bearer", p => p
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser());
    o.DefaultPolicy = o.GetPolicy("Bearer")!;
});
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

builder.Services
    .AddControllers(options =>
        {
            //Add request filters
            options.Filters.Add<ApiExceptionFilter>();
        })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Add OpenApi
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
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

//Add db
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(connectionString)
);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Add identity
builder.Services.AddIdentity<User, Role>()
    .AddRoles<Role>()
    .AddEntityFrameworkStores<Context>();

builder.Services.Configure<IdentityOptions>(options =>
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
});

//Add Options
builder.Services.AddOptions<JwtProviderOptions>().Bind(builder.Configuration.GetSection("Jwt"));
builder.Services.AddOptions<StorageOptions>().Bind(builder.Configuration.GetSection("Storage"));

//Providers
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

//Register services here
builder.Services.AddSignalR();

builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdvertisementService, AdvertisementService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAdvertisementNotificationSubscriptionService, AdvertisementNotificationSubscriptionService>();
builder.Services.AddScoped<IAttributeValidatorService, AttributeValidatorService>();

//Helpers
builder.Services.AddScoped<IStorage, LocalFileStorage>();
builder.Services.AddScoped<IFilePathResolver, FilePathResolver>();
builder.Services.AddScoped<CookieSettingsHelper>();
builder.Services.AddScoped<ImageHelper>();

//Db seeding
builder.Services.AddScoped<DbSeeder>();

//AutoMapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

//Initialize libs
MagickNET.Initialize();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Docs: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-8.0#middleware-order
app.UseHttpsRedirection();

app.UseCors(devCORSPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/hubs/messages");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //Seed db
    var scope = app.Services.CreateScope();
    var dbSeeder = scope.ServiceProvider.GetService<DbSeeder>();
    dbSeeder?.Seed();
}

app.Run();
