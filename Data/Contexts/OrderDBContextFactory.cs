using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data.Contexts;

public class OrderDBContextFactory : IDesignTimeDbContextFactory<OrderDBContext>
{
    public OrderDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDBContext>();
        optionsBuilder.UseSqlServer("");

        return new OrderDBContext(optionsBuilder.Options);
    }
}
