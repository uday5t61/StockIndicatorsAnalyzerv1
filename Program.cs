using NetHostedService.BackGroundTaskQueue;
using NetHostedService.QueueHostedService;
using StockIndicatorsAnalyzer.BLL;

var builder = WebApplication.CreateBuilder(args);

var configBuilder = new ConfigurationBuilder()
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddJsonFile($"appsettings.{Environments.Development}.json", optional: true, reloadOnChange: true);

IConfiguration config = configBuilder.Build();
// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.Services.AddMemoryCache(cache =>
{
    cache.SizeLimit = 1028;
});

//builder.Services.AddScoped<IStockInformationService, StockInformationService>();
builder.Services.AddScoped<IStockInfoService, StockInfoService>();
//builder.Services.AddSingleton<BackGroundWorkService>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue>(ctx =>
{
    if (!int.TryParse(config.GetValue<string>("QueueuCapacity"), out var queueCapacity))
        queueCapacity = 1;
    return new BackgroundTaskQueue(queueCapacity);
});
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
