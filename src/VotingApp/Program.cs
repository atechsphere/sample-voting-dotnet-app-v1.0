using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Ensure we listen on port 80 inside container
builder.WebHost.UseUrls("http://*:80");

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<VotingDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IVotingService, VotingService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Health check
app.MapGet("/health", () => Results.Json(new { 
    status = "healthy", 
    service = "Voting Application",
    port = 8086,
    timestamp = DateTime.UtcNow
}));

// Initialize database
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VotingDbContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine("✅ Database initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Database warning: {ex.Message}");
}

Console.WriteLine("🚀 Application starting on port 80 (host:8086)...");
app.Run();

public partial class Program { }
