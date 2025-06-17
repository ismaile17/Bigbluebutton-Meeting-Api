using Amazon.Runtime;
using Amazon.S3;
using Application.Accounts.Services;
using Application.Cupon.Service;
using Application.FileSystem.Models;
using Application.FileSystem.Services;
using Application.Packages.Services;
using Application.Services.ClientInfoService;
using Application.Shared.Extensions;
using BigBlueButtonAPI.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Payment.Helper;
using Shared.Jwt;
using Shared.Payment;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.SetIsOriginAllowed(origin => true)
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();



builder.Services.AddSwaggerGen();




builder.Services.InjectApplicationServices();
builder.Services.AddApplicationIntegrator();
builder.Services.AddMyContext(builder.Configuration);
builder.Services.Configure<PaymentConfig>(builder.Configuration.GetSection("Payment"));
builder.Services.AddExternalApplication();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(new TelegramService("", ""));

builder.Services.AddScoped<IClientInfoService, ClientInfoService>();
builder.Services.AddScoped<IDeleteFilesService, DeleteFilesService>();
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();


builder.Services.AddScoped<BigBlueButtonAPISettings>();

builder.Services.AddMemoryCache();


builder.Services.AddHttpClient<BigBlueButtonAPIClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });


builder.Services.AddScoped<BigBlueButtonAPIClient>();

builder.Services.AddHttpClient<BigBlueButtonAPIClient>();

builder.Services.AddScoped<CuponCheckService>();
builder.Services.AddScoped<FileSystemAuthControlService>();
builder.Services.AddScoped<GetPackageByIdCheckService>();



builder.Services.Configure<AwsSettingsDto>(builder.Configuration.GetSection("AWS"));


builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var awsSettings = sp.GetRequiredService<IOptions<AwsSettingsDto>>().Value;
    var credentials = new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey);
    var region = Amazon.RegionEndpoint.GetBySystemName(awsSettings.Region);
    return new AmazonS3Client(credentials, region);
});





AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);



builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.Authority = builder.Configuration["Jwt:ValidIssuer"];
    o.Audience = builder.Configuration["Jwt:ValidAudience"];
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
    o.TokenValidationParameters.ValidateAudience = true;
    o.TokenValidationParameters.ValidateIssuer = true;
    o.TokenValidationParameters.ValidateLifetime = true;
    o.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]));
    o.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:ValidIssuer"];
    o.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:ValidAudience"];
    o.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
    o.Configuration = new OpenIdConnectConfiguration();
});
builder.Services.AddAuthorization();
// Configure the HTTP request pipeline.
var app = builder.Build();

builder.WebHost.UseUrls("http://0.0.0.0:1000");

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseFileServer();
app.MapControllers();

app.Run();
