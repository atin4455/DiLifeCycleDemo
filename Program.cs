using DiLifeCycleDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 【實驗 A：暫時性】
builder.Services.AddTransient<ICounter, Counter>();
builder.Services.AddTransient<ISomeService, SomeService>();

// 【實驗 B：範圍性】
// builder.Services.AddScoped<ICounter, Counter>();
// builder.Services.AddScoped<ISomeService, SomeService>();

// 【實驗 C：單例性】
// builder.Services.AddSingleton<ICounter, Counter>();
// builder.Services.AddSingleton<ISomeService, SomeService>();

// 別忘了註冊控制器支援
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.MapGet("/test", (ICounter counterA, ICounter counterB) =>
{
    counterA.Increment();
    counterB.Increment();

    return new
    {
        CounterA = new { Id = counterA.Id, Value = counterA.Value },
        CounterB = new { Id = counterB.Id, Value = counterB.Value }
    };
});


app.Run();
