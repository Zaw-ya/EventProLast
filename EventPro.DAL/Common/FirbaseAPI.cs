using FirebaseAdmin.Messaging;
using EventPro.DAL.Common.Interfaces;
using EventPro.DAL.Models;
using System.Threading.Tasks;

namespace EventPro.DAL.Common
{
    public class FirbaseAPI : IFirbaseAPI
    {
        public async Task<bool> NotifyTopicOrTokenAsync(MessageRequest request)
        {
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = request.Title,
                    Body = request.Body
                },
            };
            if (string.IsNullOrEmpty(request.Tokens))
            {
                message.Topic = request.Topic;
            }
            else
            {
                message.Token = request.Tokens;
            }
            var messaging = FirebaseMessaging.DefaultInstance;
            var result = await messaging.SendAsync(message);
            return !string.IsNullOrEmpty(result);
        }
        public async Task<bool> NotifyTokensAsync(MessageRequestTokens request)
        {
            var multicastMessage = new MulticastMessage()
            {
                Notification = new Notification
                {
                    Title = request.Title,
                    Body = request.Body
                },
                Tokens = request.Tokens
            };
            var messaging = FirebaseMessaging.DefaultInstance;
            var result = await messaging.SendMulticastAsync(multicastMessage);
            return result.FailureCount == 0;
        }
    }
}