var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSprocRunner(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Database")!;
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSprocRunner();

app.UseAuthorization();

app.MapControllers();

app.Run();
