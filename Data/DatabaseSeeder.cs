using Bloomy.Models;
using Microsoft.EntityFrameworkCore;

namespace BloomyBE.Data
{
    public static class DatabaseSeeder
    {
        public static readonly EventType[] DefaultEventTypes =
        [
            new() { Id = 1, Name = "Sinh nhật", Description = "Trang trí tiệc sinh nhật", IconUrl = "" },
            new() { Id = 2, Name = "Tiệc cưới", Description = "Trang trí tiệc cưới, lễ cưới", IconUrl = "" },
            new() { Id = 3, Name = "Khai trương", Description = "Trang trí khai trương, kỷ niệm", IconUrl = "" },
            new() { Id = 4, Name = "Hội nghị", Description = "Trang trí hội nghị, sự kiện doanh nghiệp", IconUrl = "" },
            new() { Id = 5, Name = "Baby shower", Description = "Trang trí tiệc baby shower", IconUrl = "" },
        ];

        public static async Task SeedAsync(BloomyDbContext context)
        {
            if (await context.EventTypes.AnyAsync())
                return;

            await context.EventTypes.AddRangeAsync(DefaultEventTypes);
            await context.SaveChangesAsync();
        }
    }
}
