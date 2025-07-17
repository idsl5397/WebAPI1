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
using StackExchange.Redis;
using WebAPI1.Context;

var options  = new WebApplicationOptions
{
    WebRootPath = "wwwroot"  // 這樣才是正確設定 web root 的方式
};

var builder = WebApplication.CreateBuilder(options);


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//sentry log
// builder.WebHost.UseSentry(options =>
// {
//     options.Dsn = "https://e4acbce33ad370e3c0fb2dacfb2f0309@o4509036673826816.ingest.us.sentry.io/4509070034403328";
//     options.Debug = true;
// });
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
builder.Services.AddScoped<IKpiService, KpiService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISuggestService, SuggestService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IImprovementService, ImprovementService>();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();
// app.Urls.Add("http://0.0.0.0:8080");

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();  // **⚠️ 確保這行存在**
app.UseAuthorization();

app.UseCors("AllowAll");


app.MapControllers();

app.Run();
