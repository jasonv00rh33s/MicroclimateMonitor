using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Microclimate.Data;

public class DesignTimeFactory : IDesignTimeDbContextFactory<MicroclimateDbContext>
{
    public MicroclimateDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MicroclimateDbContext>()
            .UseSqlite("Data Source=microclimate.db").Options;
        return new MicroclimateDbContext(options);
    }
}