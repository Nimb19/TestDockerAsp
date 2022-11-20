public class TestDockerAspDb : DbContext
{
    public TestDockerAspDb(DbContextOptions<TestDockerAspDb> options) : base(options) {}
    public DbSet<Coin> Coins { get; set; }
}
