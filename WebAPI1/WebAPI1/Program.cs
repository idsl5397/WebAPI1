using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using WebAPI1.Context;
using WebAPI1.Services;

var options = new WebApplicationOptions
{
    WebRootPath = "wwwroot"
};

var builder = WebApplication.CreateBuilder(options);

// 讀取 JWT 設定
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ISHAuditDbcontext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WebDatabase")));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = jwtSettings.GetValue<bool>("ValidateIssuer"),
            ValidateAudience = jwtSettings.GetValue<bool>("ValidateAudience"),
            ValidateLifetime = jwtSettings.GetValue<bool>("ValidateLifetime"),
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"])),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Fail("Access token 過期");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Permission:view", policy => policy.RequireClaim("permission", "view"));
    options.AddPolicy("Permission:edit", policy => policy.RequireClaim("permission", "edit"));
    options.AddPolicy("Permission:delete", policy => policy.RequireClaim("permission", "delete"));
    options.AddPolicy("Permission:view-ranking", policy => policy.RequireClaim("permission", "view-ranking"));
    options.AddPolicy("Permission:view-report", policy => policy.RequireClaim("permission", "view-report"));
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = null;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 注入 Service
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IKpiService, KpiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISuggestService, SuggestService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IImprovementService, ImprovementService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ✳️ 自動依 Host 判斷是否需要加上 PathBase
app.Use((context, next) =>
{
    if (context.Request.Host.Host.Contains("security.bip.gov.tw"))
    {
        context.Request.PathBase = "/iskpi";
    }
    return next();
});

app.UseRouting();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"{(app.Environment.IsDevelopment() ? "" : "/iskpi")}/swagger/v1/swagger.json", "API V1");
    });
}
app.MapControllers();

app.Run();