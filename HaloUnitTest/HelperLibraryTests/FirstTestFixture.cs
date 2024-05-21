using HaloUnitTest.Mocks;
using Microsoft.EntityFrameworkCore;

namespace HaloUnitTest.HelperLibraryTests;

public class FirstTestFixture {
    
    [Test]
    public async Task TestIfAnything() {
        //var dbContextMock = HalogenDbContextMock.Instance();
        //await dbContextMock.WithData(new Dictionary<string, List<object>>());
        var dbContext = await HalogenDbContextMock.Instance().MemoryDbContext();

        var some = await dbContext.Accounts.Take(1).Include(a => a.Roles).SelectMany(a => a.Roles.ToArray()).ToArrayAsync();
    }
}