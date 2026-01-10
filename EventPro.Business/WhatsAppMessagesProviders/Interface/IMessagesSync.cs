using EventPro.DAL.Dto;
using EventPro.DAL.Models;

namespace EventPro.Business.WhatsAppMessagesProviders.Interface
{
    public interface IMessagesSync
    {
        Task UpdateMessagesStatusAsync(List<Guest> guests, Events events);
        Task<List<MessageLog>> GetGuestMessagesAsync(string number, string profileName);
    }
}
