using BupaBooks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IBookCategorizationService, BookCategorizationService>();

var bookOwnersUrl = builder.Configuration["BupaApi:BookOwnersUrl"]
    ?? throw new InvalidOperationException("BupaApi:BookOwnersUrl is missing from configuration.");

builder.Services.AddHttpClient<IBupaBooksClient, BupaBooksClient>(client =>
{
    client.BaseAddress = new Uri(bookOwnersUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("FrontendDev");
app.MapControllers();
app.Run();
