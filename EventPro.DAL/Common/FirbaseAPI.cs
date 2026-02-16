using FirebaseAdmin.Messaging;
using EventPro.DAL.Common.Interfaces;
using EventPro.DAL.Models;
using Serilog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventPro.DAL.Common
{
    /// <summary>
    /// Firebase Cloud Messaging (FCM) service.
    /// Sends push notifications to mobile devices via the Firebase Admin SDK (V1 API).
    /// Requires Firebase Admin SDK to be initialized in Startup.cs before use.
    ///
    /// Supports two messaging patterns:
    ///   1. Topic or single-token messaging  (NotifyTopicOrTokenAsync)
    ///   2. Multicast to multiple tokens      (NotifyTokensAsync)
    ///
    /// Firebase Console: https://console.firebase.google.com/u/0/project/myinvite-uat
    /// </summary>
    public class FirbaseAPI : IFirbaseAPI
    {
        #region Topic / Single-Token Notification

        /// <summary>
        /// Sends a push notification to a single device token or a topic.
        /// If <see cref="MessageRequest.Tokens"/> is empty, sends to <see cref="MessageRequest.Topic"/> (city-based).
        /// Otherwise sends directly to the specified device token.
        /// </summary>
        /// <param name="request">The notification payload containing title, body, and target (token or topic).</param>
        /// <returns>True if the message was sent successfully; false otherwise.</returns>
        public async Task<bool> NotifyTopicOrTokenAsync(MessageRequest request)
        {
            var target = string.IsNullOrEmpty(request.Tokens) ? $"Topic={request.Topic}" : $"Token={request.Tokens}";
            Log.Information("FCM SendToTopicOrToken: Title={Title}, Body={Body}, Target={Target}",
                request.Title, request.Body, target);

            try
            {
                var message = new Message()
                {
                    Notification = new Notification
                    {
                        Title = request.Title,
                        Body = request.Body
                    },
                };

                // Route to topic (e.g., city ID) or direct device token
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

                Log.Information("FCM SendToTopicOrToken SUCCESS: Target={Target}, Result={Result}", target, result);
                return !string.IsNullOrEmpty(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "FCM SendToTopicOrToken FAILED: Target={Target}, Error={Error}", target, ex.Message);
                return false;
            }
        }

        #endregion

        #region Multicast Notification (Multiple Tokens)

        /// <summary>
        /// Sends a push notification to multiple device tokens individually.
        /// Google removed the legacy /batch endpoint, so we send each token separately via SendAsync.
        /// Used by the scheduled notification service to remind gatekeepers about upcoming events.
        /// </summary>
        /// <param name="request">The notification payload with a list of device tokens.</param>
        /// <returns>True if all messages were delivered successfully (no failures); false otherwise.</returns>
        public async Task<bool> NotifyTokensAsync(MessageRequestTokens request)
        {
            var multicastMessage = new MulticastMessage()
            //    request.EventID, request.Title, request.Body, request.Tokens?.Count ?? 0,
            //    string.Join(", ", request.Tokens?.Select(t => t?.Length > 20 ? t.Substring(0, 20) + "..." : t ?? "NULL") ?? Array.Empty<string>()));

            //var messaging = FirebaseMessaging.DefaultInstance;
            //int successCount = 0;
            //int failureCount = 0;
            //var failedTokens = new List<string>();

            //foreach (var token in request.Tokens)
            //{
            //    try
            //    {
            //        var message = new Message()
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
                //    Log.Information("FCM SendEach SUCCESS: EventID={EventID}, Token={Token}, Result={Result}",
                //        request.EventID, token?.Length > 20 ? token.Substring(0, 20) + "..." : token, result);
                //}
                //catch (Exception ex)
                //{
                //    failureCount++;
                //    failedTokens.Add(token);
                //    Log.Warning("FCM SendEach FAILED: EventID={EventID}, Token={Token}, Error={Error}",
                //        request.EventID, token?.Length > 20 ? token.Substring(0, 20) + "..." : token, ex.Message);
              //  }
           // }

            //Log.Information("FCM SendEach RESULT: EventID={EventID}, SuccessCount={SuccessCount}, FailureCount={FailureCount}",
            //    request.EventID, successCount, failureCount);

            //return failureCount == 0;
        }

        #endregion
    }
}
