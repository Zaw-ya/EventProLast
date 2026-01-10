using EventPro.DAL.Enum;
using EventPro.DAL.Models;
using EventPro.Services.AuditLogService.Interface;

namespace EventPro.Services.AuditLogService.implementation
{
    public class AuditLogService : IAuditLogService
    {
        readonly EventProContext _context;
        public AuditLogService(EventProContext context)
        {
            _context = context;
        }

        public async Task AddAsync(int? UserId, int? eventId, ActionEnum? action = ActionEnum.AddEvent, int? RelatedId = null, string? notes = null)
        {
            await _context.AuditLog.AddAsync(new AuditLog
            {
                UserId = UserId,
                EventId = eventId,
                Action = action,
                RelatedId = RelatedId,
                Notes = notes
            });
            await _context.SaveChangesAsync();
        }
    }
}