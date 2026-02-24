using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI1.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using WebAPI1.Context;

var options  = new WebApplicationOptions
{
    WebRootPath = "wwwroot"  // 這樣才是正確設定 web root 的方式
};

var builder = WebApplication.CreateBuilder(options);


var jwtSettings = builder.Configuration.GetSection("JwtSettings");

//redis settings
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var redisConnectionString = builder.Configuration.GetValue<string>("ConnectionStrings:Redis");
    if (string.IsNullOrWhiteSpace(redisConnectionString))
        throw new ArgumentNullException(nameof(redisConnectionString));
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "績效指標資料庫後台", Version = "v1" });

    // ✅ 加入 JWT Bearer 支援
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "請輸入 JWT Token，格式為：Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    // ✅ 全域要求加上 Bearer 權限
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
//sentry log
// builder.WebHost.UseSentry(options =>
// {
//     options.Dsn = "https://e4acbce33ad370e3c0fb2dacfb2f0309@o4509036673826816.ingest.us.sentry.io/4509070034403328";
//     options.Debug = true;
// });

builder.Services.AddDbContext<ISHAuditDbcontext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("WebDatabase"));
    options.AddInterceptors(sp.GetRequiredService<DataChangeLogInterceptor>()); // ✅ 掛進 EF
});

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
            ClockSkew = TimeSpan.Zero // ✅ 建議設為 0，不然有預設 5 分鐘容錯
        };

        // ✅ 加入事件來處理過期 token
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
    options.AddPolicy("Permission:kpi-approve", policy => policy.RequireClaim("permission", "kpi-approve"));
    // ➕ 依你資料庫還有哪些 Permission.Key 自動加上
});

builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = null; // 禁用 HTTPS 重定向
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


builder.Services.AddControllers();

builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IKpiService, KpiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISuggestService, SuggestService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IImprovementService, ImprovementService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DataChangeLogInterceptor>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();


var redisConnectionString = builder.Configuration.GetValue<string>("ConnectionStrings:Redis");
        
Console.WriteLine($"Redis 連線字串: {redisConnectionString}");
// 添加數據庫服務
var connectionString = builder.Configuration.GetValue<string>("ConnectionStrings:WebDatabase");
Console.WriteLine($"SQL 連線字串: {connectionString}");

var app = builder.Build();
// app.Urls.Add("http://0.0.0.0:8080");

// QuestPDF 社群授權 + 中文字體（楷書）
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var kaiuFontPath = Path.Combine(app.Environment.WebRootPath, "fonts", "kaiu.ttf");
using var kaiuFontStream = System.IO.File.OpenRead(kaiuFontPath);
QuestPDF.Drawing.FontManager.RegisterFontWithCustomName("KaiU", kaiuFontStream);

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseRouting();
// app.UseHttpsRedirection();
app.UseAuthentication();  // **⚠️ 確保這行存在**
app.UseAuthorization();


// app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.WebRootPath, "images")),
    RequestPath = "/images"  // 訪問路徑: /images/photo.jpg
});
app.UseCors("AllowAll");


app.MapControllers();

app.Run();
