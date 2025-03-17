using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI1.Models;


var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<isha_sys_devContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("WebDatabase")));

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

builder.Services.AddAuthorization();

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
