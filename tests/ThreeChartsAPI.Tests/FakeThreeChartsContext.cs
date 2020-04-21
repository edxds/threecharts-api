using System;
using Microsoft.EntityFrameworkCore;

namespace ThreeChartsAPI.Tests
{
    public static class FakeThreeChartsContext
    {
        public static ThreeChartsContext BuildInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ThreeChartsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ThreeChartsContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}