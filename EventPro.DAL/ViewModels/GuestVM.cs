using System;

using EventPro.DAL.Models;

namespace EventPro.DAL.ViewModels
{
    public class GuestVM
    {
        public int Id { get; set; }
        public string EventId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string ScannedInfo { get; set; }
        public string ConfirmationMsgStatus { get; set; }
        public string CardMsgStatus { get; set; }
        public string EventLocationMsgStatus { get; set; }
        public string ReminderMsgStatus { get; set; }
        public string CongratulationMsgStatus { get; set; }  // Corrected spelling
        public string Response { get; set; }
        public string ResponseTime { get; set; }
        public string NoOfMembers { get; set; }
        public string PrimaryContact { get; set; }
        public string SecondaryContact { get; set; }
        public string AdditionalText { get; set; }
        public bool? IsValidPhoneNumber { get; set; }
        public GuestVM(vwGuestInfo guest)
        {
            Id = guest.GuestId;
            EventId = "E00000" + guest.EventId.ToString();
            Name = guest.FirstName;
            PhoneNumber = "+" + guest.SecondaryContactNo + guest.PrimaryContactNo;
            ScannedInfo = GetScannedInfo(guest);
            ConfirmationMsgStatus = GetConfirmationMsgStatus(guest);
            CardMsgStatus = GetCardMsgStatus(guest);
            EventLocationMsgStatus = GetEventLocationMsgStatus(guest);
            ReminderMsgStatus = GetReminderMsgStatus(guest);
            CongratulationMsgStatus = GetCongratulationMsgStatus(guest);  // Corrected spelling
            Response = GetResponse(guest);
            ResponseTime = guest.WaresponseTime.ToString(); // WhatsApp ResponseTime
            NoOfMembers = guest.NoOfMembers?.ToString();
            PrimaryContact = guest.PrimaryContactNo?.ToString();
            SecondaryContact = guest.SecondaryContactNo?.ToString();
            AdditionalText = guest.AdditionalText?.ToString();
            IsValidPhoneNumber = guest.IsPhoneNumberValid;
        }
        public string GetScannedInfo(vwGuestInfo guest)
        {
            // Gharabawy : We Calc it from the database view (group all guests with the assoiciated parent GuestId) and then take the related group for the current selected guest
            return guest.Scanned + " of " + guest.NoOfMembers;
            //return "Test Scanned" + " of " + guest.NoOfMembers;
        }
        public string GetConfirmationMsgStatus(vwGuestInfo guest)
        {
            if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
            {
                return "فشل في الإرسال";
            }
            else if (Convert.ToBoolean(guest.TextRead))
            {
                return "مقروء";
            }
            else if (Convert.ToBoolean(guest.TextDelivered))
            {
                return "تم التوصيل";
            }
            else if (Convert.ToBoolean(guest.TextSent))
            {
                if (guest.whatsappMessageId != null)
                {
                    return "في قائمة الانتظار";
                }
                else
                {
                    return "مرسل";
                }
            }
            else if (guest.MessageId != null)
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "Whatsapp Not exists";
                }
                else
                {
                    return "في الانتظار";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "Whatsapp Not exists";
                }
                else
                {
                    return "لم يتم الإرسال";
                }
            }
        }
        public string GetCardMsgStatus(vwGuestInfo guest)
        {
            if (guest.ImgFailed != null && Convert.ToBoolean(guest.ImgFailed))
            {
                return "فشل في الإرسال";
            }
            else if (Convert.ToBoolean(guest.ImgRead))
            {
                return "مقروء";
            }
            else if (Convert.ToBoolean(guest.ImgDelivered))
            {
                return "تم التوصيل";
            }
            else if (Convert.ToBoolean(guest.ImgSent))
            {
                if (guest.whatsappMessageImgId != null)
                {
                    if (Convert.ToBoolean(guest.TextDelivered) || Convert.ToBoolean(guest.TextRead))
                    {
                        return "مرسل";
                    }
                    else
                    {
                        return "في قائمة الانتظار";
                    }
                }
                else
                {
                    return "مرسل";
                }
            }
            else if (guest.ImgSentMsgId != null)
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "Whatsapp Not exists";
                }
                else
                {
                    return "في الانتظار";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "فشل في الإرسال";
                }
                else
                {
                    if (guest.Response == "رفض" || guest.Response == "Decline" || guest.Response == "اعتذار عن الحضور")
                    {
                        return "في قائمة الانتظار بسبب رفض الضيف";
                    }
                    else
                    {
                        if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                        {
                            return "فشل في الإرسال";
                        }
                        else
                        {
                            return "لم يتم الإرسال";
                        }
                    }
                }
            }
        }
        public string GetEventLocationMsgStatus(vwGuestInfo guest)
        {
            if (guest.EventLocationFailed != null && Convert.ToBoolean(guest.EventLocationFailed))
            {
                return "فشل في الإرسال";
            }
            else if (Convert.ToBoolean(guest.EventLocationRead))
            {
                return "مقروء موقع الحدث";
            }
            else if (Convert.ToBoolean(guest.EventLocationDelivered))
            {
                return "تم التوصيل موقع الحدث";
            }
            else if (Convert.ToBoolean(guest.EventLocationSent))
            {
                if (guest.whatsappWatiEventLocationId != null)
                {
                    if (Convert.ToBoolean(guest.TextDelivered) || Convert.ToBoolean(guest.TextRead))
                    {
                        return "مرسل موقع الحدث";
                    }
                    else
                    {
                        return "في قائمة الانتظار موقع الحدث";
                    }
                }
                else
                {
                    return "مرسل موقع الحدث";
                }
            }
            else if (guest.waMessageEventLocationForSendingToAll != null)
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "Whatsapp Not exists";
                }
                else
                {
                    return "في الانتظار";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "فشل في الإرسال";
                }
                else
                {
                    if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                    {
                        return "فشل في الإرسال";
                    }
                    else
                    {
                        return "لم يتم الإرسال";
                    }
                }
            }
        }
        public string GetReminderMsgStatus(vwGuestInfo guest)
        {
            if (guest.ReminderMessageFailed != null && Convert.ToBoolean(guest.ReminderMessageFailed))
            {
                return "فشل في الإرسال";
            }
            else if (Convert.ToBoolean(guest.ReminderMessageRead))
            {
                return "مقروء رسالة التذكير";
            }
            else if (Convert.ToBoolean(guest.ReminderMessageDelivered))
            {
                return "تم التوصيل رسالة التذكير";
            }
            else if (Convert.ToBoolean(guest.ReminderMessageSent))
            {
                if (guest.ReminderMessageWatiId != null)
                {
                    if (Convert.ToBoolean(guest.TextDelivered) || Convert.ToBoolean(guest.TextRead))
                    {
                        return "مرسل رسالة التذكير";
                    }
                    else
                    {
                        return "في قائمة الانتظار رسالة التذكير";
                    }
                }
                else
                {
                    return "مرسل رسالة التذكير";
                }
            }
            else if (guest.ReminderMessageId != null)
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "Whatsapp Not exists";
                }
                else
                {
                    return "في الانتظار";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "فشل في الإرسال";
                }
                else
                {
                    if (guest.ReminderMessageFailed != null && Convert.ToBoolean(guest.ReminderMessageFailed))
                    {
                        return "فشل في الإرسال";
                    }
                    else
                    {
                        return "لم يتم الإرسال";
                    }
                }
            }
        }
        public string GetCongratulationMsgStatus(vwGuestInfo guest)  // Corrected spelling
        {
            // If msg faild while sending it
            if (guest.ConguratulationMsgFailed != null && Convert.ToBoolean(guest.ConguratulationMsgFailed))
            {
                return "فشل في الإرسال";
            }
            // Check if the message sent
            else if (Convert.ToBoolean(guest.ConguratulationMsgSent))
            {
                // If wait of another service (such as in the queue)
                if (guest.WatiConguratulationMsgId != null)
                {
                    // If the mesaage already delevired or read by the guest
                    if (Convert.ToBoolean(guest.TextDelivered) || Convert.ToBoolean(guest.TextRead))
                    {
                        return "مرسل رسالة التهنئة";
                    }
                    // here means the message still stuk or wait in the queue
                    else
                    {
                        return "في قائمة الانتظار رسالة التهنئة";
                    }
                }
                // If it is does not waiting in the queue
                else
                {
                    return "مرسل رسالة التهنئة";
                }
            }
            // If msg deliverd to guest
            else if (Convert.ToBoolean(guest.ConguratulationMsgDelivered))
            {
                return "تم التوصيل رسالة التهنئة";
            }
            // If guest read the msg
            else if (Convert.ToBoolean(guest.ConguratulationMsgRead))
            {
                return "مقروء رسالة التهنئة";
            }
            else if (guest.ConguratulationMsgId != null)
            {
                // This is already returned in somewhere 
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "Whatsapp Not exists";
                }
                else
                {
                    return "في الانتظار";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "فشل في الإرسال";
                }
                else
                {
                    if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                    {
                        return "فشل في الإرسال";
                    }
                    else
                    {
                        return "لم يتم الإرسال";
                    }
                }
            }
        }
        public string GetResponse(vwGuestInfo guest)
        {
            if (guest.TextRead == true || guest.TextDelivered == true || guest.TextSent == true)
            {
                if (guest.Response == "Message Processed Successfully")
                {
                    if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                    {
                        return "حدث خطأ غير معروف أثناء معالجة الرسالة رجاءً تحقق من الإعدادات";
                    }
                    else if (guest.ImgRead == true || guest.ImgDelivered == true || guest.ImgSent == true)
                    {
                        if (guest.Response == "رفض" || guest.Response == "Decline" || guest.Response == "اعتذار عن الحضور")
                        {
                            return "مرفوض";
                        }
                        else if (guest.Response == "تأكيد" || guest.Response == "Confirm" || guest.Response == "سأحضر الحفل")
                        {
                            return "مؤكد";
                        }
                    }
                    else
                    {
                        if (guest.whatsappMessageId != null)
                        {
                            if (!Convert.ToBoolean(guest.TextDelivered) && !Convert.ToBoolean(guest.TextRead))
                            {
                                return "لا يوجد رد بعد على الرسالة";
                            }
                            else
                            {
                                return "بانتظار الرد";
                            }
                        }
                        else
                        {
                            return "بانتظار الرد";
                        }
                    }
                }
                else if (guest.ImgRead == true || guest.ImgDelivered == true || guest.ImgSent == true)
                {
                    if (guest.Response == "رفض" || guest.Response == "Decline" || guest.Response == "اعتذار عن الحضور")
                    {
                        return "مرفوض";
                    }
                    else
                    {
                        return "مؤكد";
                    }
                }
                else
                {
                    if (guest.Response == "رفض" || guest.Response == "Decline" || guest.Response == "اعتذار عن الحضور")
                    {
                        return "مرفوض";
                    }
                    else if (guest.Response == "تأكيد" || guest.Response == "Confirm" || guest.Response == "سأحضر الحفل")
                    {
                        return "مؤكد";
                    }
                    else
                    {
                        return guest.Response ?? "";
                    }
                }
            }
            else
            {
                if (guest.Response == "Message Processed Successfully")
                {
                    if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                    {
                        return "حدث خطأ غير معروف أثناء معالجة الرسالة رجاءً تحقق من الإعدادات";
                    }
                    else
                    {
                        return "لا يوجد رد بعد";
                    }
                }
                else
                {
                    if (guest.Response == "رفض" || guest.Response == "Decline" || guest.Response == "اعتذار عن الحضور")
                    {
                        return "مرفوض";
                    }
                    else if (guest.Response == "تأكيد" || guest.Response == "Confirm" || guest.Response == "سأحضر الحفل")
                    {
                        return "مؤكد";
                    }
                    else
                    {
                        return guest.Response;
                    }
                }
            }
            return "";
        }
    }
}