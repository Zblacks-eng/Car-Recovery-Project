using CST2550;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5050");

builder.Services.AddControllers();

// Registered as singletons so the same BST and database manager
// instance is reused across all requests rather than creating a new one each time
builder.Services.AddSingleton<RecoveryTree>();
builder.Services.AddSingleton<DatabaseManager>();

var app = builder.Build();

// Run startup tasks before the app starts accepting requests
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<DatabaseManager>();
	var tree = scope.ServiceProvider.GetRequiredService<RecoveryTree>();

	db.EnsureDatabaseExists();      // create CarRecoveryDB if it doesn't exist yet
    db.EnsureCarRecoveriesTableExists();
    db.EnsureAuthTableExists();     // create the Users table if it doesn't exist yet
	db.EnsureCarRecoveriesSchema(); // make sure BreakdownTime is DATETIME2 not just DATE
	db.LoadFromDatabase(tree);      // load all existing records into the BST
}

// UseDefaultFiles means the app will serve index.html automatically
// when someone visits the root URL
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
