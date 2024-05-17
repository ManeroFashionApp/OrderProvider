using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class OrderDBContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderHistoryEntity> OrderChanges { get; set; }
    public DbSet<OrderProductEntity> OrderProducts { get; set; }
}
