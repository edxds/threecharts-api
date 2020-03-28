using System;
using Microsoft.EntityFrameworkCore;
using ThreeChartsAPI.Models;

namespace ThreeChartsAPI.Tests
{
    public static class ThreeChartsTestContext
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