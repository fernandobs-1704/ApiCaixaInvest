using ApiCaixaInvest.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiCaixaInvest.Tests;

public abstract class TestBase
{
    protected ApiCaixaInvestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApiCaixaInvestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApiCaixaInvestDbContext(options);
    }
}
