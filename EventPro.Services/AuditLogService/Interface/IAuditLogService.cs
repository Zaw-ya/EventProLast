using EventPro.DAL.Enum;

namespace EventPro.Services.AuditLogService.Interface
{
    public interface IAuditLogService
    {
        public Task AddAsync(int? UserId, int? eventId, ActionEnum? action = ActionEnum.AddEvent, int? RelatedId = null, string? notes = null);
    }
}