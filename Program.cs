

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("Sqlite");
    opt.UseSqlite(cs);
});


builder.Services.AddSignalR()
    //.AddStackExchangeRedis("<>")
    ;

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionBroadcaster, SignalRTransactionBroadcaster>();
builder.Services.AddScoped<ITransactionProcessor, TransactionProcessor>();


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("dev", p =>
    {
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<TransactionDtoValidator>();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseExceptionHandler();
app.UseRouting();
app.UseCors("dev");

app.MapGet("/", () => Results.Ok(new { status = "ok" }));


app.MapTransactionsRoutes();


app.MapHub<TransactionsHub>("/hubs/transactions");

app.Run();
