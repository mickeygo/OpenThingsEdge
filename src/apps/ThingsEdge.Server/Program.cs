using Serilog;
using ThingsEdge.Application;
using ThingsEdge.Router;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMasaBlazor(builder =>
{
    builder.ConfigureTheme(theme =>
    {
        theme.Themes.Light.Primary = "#4318FF";
        theme.Themes.Light.Accent = "#4318FF";
    });
}).AddI18nForServer("wwwroot/i18n");
builder.Services.AddHttpContextAccessor();

builder.Services.AddGlobalForServer();

// 自定义服务配置 
builder.Services.AddThingsEdgeApplication();
builder.Host.AddThingsEdgeRouter()
    .AddDeviceFileProvider()
    .AddHttpForwarder(options => { options.BaseAddress = "https://localhost:7214"; })
    .AddOpsProvider()
    .AddEventBus();

builder.Host.UseWindowsService(); // 可设置为 Window Service。
builder.Host.UseSerilog((hostingContext, loggerConfiguration) => 
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)); // 使用 Serilog，并从配置文件中读取配置。

// End 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();
