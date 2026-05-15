using backend_api.Data;
using backend_api.Inerfaces.Services;
using backend_api.Interfaces.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using backend_api.Interfaces.Repositories;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IVersionRepository, VersionRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IOnlyOfficeService, OnlyOfficeService>();
builder.Services.AddScoped<IDocumentDiffService, DocumentDiffService>();
// Add services to the container.
builder.Services.AddSignalR();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/7eb749a1-a2ec-4260-bb87-c77c7a0a0b7a";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,   
            ValidateIssuer = true,
        };
    });
builder.Services.AddAuthorization();
// Add controllers with Newtonsoft.Json support for JObject
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Configure CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // React default port
               .AllowAnyHeader()
               .AllowAnyMethod()
         .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create Storage Directory if it doesn't exist
var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "document-storage");
if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}


app.UseStaticFiles(); // default (keep it)

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "document-storage")),
    RequestPath = "/document-storage"
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowReactApp");
app.MapHub<backend_api.Hubs.DocumentHub>("/documentHub");

// Typically OnlyOffice calls back internally without HTTPS/authentication when in dev setup
// But we keep it simple here.
// app.UseHttpsRedirection(); // Commented out to match the tutorial's HTTP port 5000.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
