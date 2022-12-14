var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TestDockerAspDb>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Npgsql")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TestDockerAspDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/", (HttpResponse response) => response.Redirect("/swagger/index.html", true, true));
app.MapGet("/coin", async (TestDockerAspDb db) => await db.Coins.ToListAsync());

app.MapGet("/coin/{uid}", async (Guid uid, TestDockerAspDb db) =>
    await db.Coins.FirstOrDefaultAsync(x => x.Id == uid) is Coin coin ? Results.Ok(coin) : Results.NoContent());

app.MapPost("/coin", async ([FromBody] Coin coin, TestDockerAspDb db) => {
    await db.Coins.AddAsync(coin);
    await db.SaveChangesAsync();
    return Results.Created($"/coin/{coin.Id}", coin);
});

app.MapPut("/coin/", async ([FromBody] Coin coin, TestDockerAspDb db) =>
{
    var coinFromDb = await db.Coins.FindAsync(new object[] { coin.Id });
    if (coinFromDb == default) 
        return Results.NotFound("Uid not found");

    coinFromDb.Name = coin.Name;
    await db.SaveChangesAsync();
    return Results.Ok();
})
    .Accepts<Coin>("application/json")
    .Produces<Coin>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("UpdateCoin");

app.MapDelete("/coin/{uid}", async (Guid uid, TestDockerAspDb db) =>
{
    var fcoin = await db.Coins.FindAsync(new object[] { uid });
    if (fcoin == default)
        return Results.NotFound("Uid not found");
    db.Coins.Remove(fcoin);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.UseHttpsRedirection();
app.Run();