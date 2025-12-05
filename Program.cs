using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using StoreBlazor.Components;
using StoreBlazor.Data;
using StoreBlazor.Services.Admin.Implementations;
using StoreBlazor.Services.Admin.Interfaces;
using StoreBlazor.Services.Client.Implementations;
using StoreBlazor.Services.Client.Interfaces;
using StoreBlazor.Services.Payment.Implementations;
using StoreBlazor.Services.Payment;
using System;

var builder = WebApplication.CreateBuilder(args);

// instance _dbcontext object
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString)));

// instance IService
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IOrderManagerService, OrderManagerService>();
builder.Services.AddScoped<IOrderStaffService, OrderStaffService>();
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IMoMoService, MoMoService>();

builder.Services.AddScoped<IProductManagerService, ProductManagerService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10MB
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();