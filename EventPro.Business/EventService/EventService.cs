using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EventPro.DAL.Models;

namespace EventPro.Business.EventService
{
    public class EventService : IEventService
    {
        private readonly IConfiguration _configuration;
        private readonly EventProContext db;
        public EventService(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new EventProContext(configuration);
        }

        public async Task<List<Events>> GetAllEvents()
        {
            return await db.Events.ToListAsync();
        }
    }
}
