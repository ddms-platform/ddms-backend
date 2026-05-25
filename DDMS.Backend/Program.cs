var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// Swagger config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "BoatTour API",
        Version = "v1",
        Description = "API for BoatTour Backend"
    });
});

var app = builder.Build();

// Enable Swagger
if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BoatTour API v1");
        options.RoutePrefix = ""; // mở swagger tại /
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
