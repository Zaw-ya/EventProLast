using EventPro.DAL.Models;
using System;

namespace EventPro.DAL.ViewModels
{

    public class GuestVM
    {
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
            CongratulationMsgStatus = GetCongratulationMsgStatus(guest);
            Response = GetResponse(guest);
            ResponseTime = guest.WaresponseTime.ToString();
            NoOfMembers = guest.NoOfMembers?.ToString();
            PrimaryContact = guest.PrimaryContactNo?.ToString();
            SecondaryContact = guest.SecondaryContactNo?.ToString();
            AdditionalText = guest.AdditionalText?.ToString();
            IsValidPhoneNumber = guest.IsPhoneNumberValid;
        }
        public int Id { get; set; }
        public string EventId { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string ScannedInfo { get; set; }
        public string ConfirmationMsgStatus { get; set; }
        public string CardMsgStatus { get; set; }
        public string EventLocationMsgStatus { get; set; }
        public string ReminderMsgStatus { get; set; }
        public string CongratulationMsgStatus { get; set; }
        public string Response { get; set; }
        public string ResponseTime { get; set; }
        public string NoOfMembers { get; set; }
        public string PrimaryContact { get; set; }
        public string SecondaryContact { get; set; }
        public string AdditionalText { get; set; }
        public bool? IsValidPhoneNumber { get; set; }

        public string GetScannedInfo(vwGuestInfo guest)
        {

            return guest.Scanned + " of " + guest.NoOfMembers;
        }

        public string GetConfirmationMsgStatus(vwGuestInfo guest)
        {
            if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
            {
                return "?? ???? ???????";
            }
            else if (Convert.ToBoolean(guest.TextRead))
            {
                return "???? ???????";
            }
            else if (Convert.ToBoolean(guest.TextDelivered))
            {
                return "???? ???????";
            }
            else if (Convert.ToBoolean(guest.TextSent))
            {
                if (guest.whatsappMessageId != null)
                {
                    return "?? ??? ???????";
                }
                else
                {
                    return "????? ???????";
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
                    return "??? ???????";
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
                    return "?????? ???????";
                }
            }
        }

        public string GetCardMsgStatus(vwGuestInfo guest)
        {
            if (guest.ImgFailed != null && Convert.ToBoolean(guest.ImgFailed))
            {
                return "?? ???? ???????";
            }
            else if (Convert.ToBoolean(guest.ImgRead))
            {
                return "???? ??????";
            }
            else if (Convert.ToBoolean(guest.ImgDelivered))
            {
                return "???? ?????? ";
            }
            else if (Convert.ToBoolean(guest.ImgSent))
            {
                if (guest.whatsappMessageImgId != null)
                {
                    if (Convert.ToBoolean(guest.TextDelivered) || Convert.ToBoolean(guest.TextRead))
                    {
                        return "????? ??????";
                    }
                    else
                    {
                        return "?? ??? ??????";
                    }

                }
                else
                {
                    return "????? ??????";
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
                    return "??? ???????";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "?? ???? ???????";
                }
                else
                {
                    if (@guest.Response == "??????" || @guest.Response == "Decline" || @guest.Response == "???????? ?? ??????")
                    {
                        return "?? ??? ??????? ??????? ?????";
                    }
                    else
                    {
                        if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                        {
                            return "?? ???? ???????";
                        }
                        else
                        {
                            return "?????? ???????";
                        }

                    }

                }
            }
        }

        public string GetEventLocationMsgStatus(vwGuestInfo guest)
        {
            if (guest.EventLocationFailed != null && Convert.ToBoolean(guest.EventLocationFailed))
            {
                return "?? ???? ???????";
            }
            else if (Convert.ToBoolean(guest.EventLocationRead))
            {
                return "??? ???? ????????";
            }
            else if (Convert.ToBoolean(guest.EventLocationDelivered))
            {
                return "??? ???? ????????";
            }
            else if (Convert.ToBoolean(guest.EventLocationSent))
            {
                if (guest.whatsappWatiEventLocationId != null)
                {
                    if (Convert.ToBoolean(guest.TextDelivered) || Convert.ToBoolean(guest.TextRead))
                    {
                        return "???? ???? ????????";
                    }
                    else
                    {
                        return "?? ??? ???? ????????";
                    }

                }
                else
                {
                    return "???? ???? ????????";
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
                    return "??? ???????";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "?? ???? ???????";
                }
                else
                {
                    if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                    {
                        return "?? ???? ???????";
                    }
                    else
                    {
                        return "?????? ???????";
                    }

                }
            }
        }

        public string GetReminderMsgStatus(vwGuestInfo guest)
        {
            if (guest.ReminderMessageFailed != null && Convert.ToBoolean(guest.ReminderMessageFailed))
            {
                return "?? ???? ???????";
            }
            else if (Convert.ToBoolean(guest.ReminderMessageRead))
            {
                return "???? ??????? ?????????";
            }
            else if (Convert.ToBoolean(guest.ReminderMessageDelivered))
            {
                return "???? ??????? ?????????";
            }
            else if (Convert.ToBoolean(guest.ReminderMessageSent))
            {
                if (guest.ReminderMessageWatiId != null)
                {
                    if (Convert.ToBoolean(guest.TextDelivered) || Convert.ToBoolean(guest.TextRead))
                    {
                        return "????? ??????? ?????????";
                    }
                    else
                    {
                        return "?? ??? ??????? ?????????";
                    }

                }
                else
                {
                    return "????? ??????? ?????????";
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
                    return "??? ???????";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "?? ???? ???????";
                }
                else
                {
                    if (guest.ReminderMessageFailed != null && Convert.ToBoolean(guest.ReminderMessageFailed))
                    {
                        return "?? ???? ???????";
                    }
                    else
                    {
                        return "?????? ???????";
                    }

                }
            }
        }


        public string GetCongratulationMsgStatus(vwGuestInfo guest)
        {
            if (guest.ConguratulationMsgFailed != null && Convert.ToBoolean(guest.ConguratulationMsgFailed))
            {
                return "?? ???? ???????";
            }
            else if (Convert.ToBoolean(guest.ConguratulationMsgRead))
            {
                return "???? ????? ???????";
            }
            else if (Convert.ToBoolean(guest.ConguratulationMsgDelivered))
            {
                return "    ???? ????? ???????";
            }
            else if (Convert.ToBoolean(guest.ConguratulationMsgSent))
            {
                if (guest.WatiConguratulationMsgId != null)
                {
                    if (Convert.ToBoolean(guest.TextDelivered) || Convert.ToBoolean(guest.TextRead))
                    {
                        return "????? ????? ???????";
                    }
                    else
                    {
                        return "?? ??? ????? ???????";
                    }

                }
                else
                {
                    return "????? ????? ???????";
                }

            }
            else if (guest.ConguratulationMsgId != null)
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "Whatsapp Not exists";
                }
                else
                {
                    return "??? ???????";
                }
            }
            else
            {
                if (guest.Response == "Whatsapp Not exists")
                {
                    return "?? ???? ???????";
                }
                else
                {
                    if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                    {
                        return "?? ???? ???????";
                    }
                    else
                    {
                        return "?????? ???????";
                    }

                }
            }
        }

        public string GetResponse(vwGuestInfo guest)
        {
            if (guest.TextRead == true || guest.TextDelivered == true || guest.TextSent == true)
            {

                if (@guest.Response == "Message Processed Successfully")
                {
                    if (guest.TextFailed != null && Convert.ToBoolean(guest.TextFailed))
                    {
                        return "???? ???? ???? ????? ?? ??????? ??????? ???? ????? ????????";
                    }
                    else if (guest.ImgRead == true || guest.ImgDelivered == true || guest.ImgSent == true)
                    {
                        if (guest.Response == "??????" || guest.Response == "Decline" || guest.Response == "???????? ?? ??????")
                        {
                            return "??????";
                        }
                        else if (guest.Response == "?????" || guest.Response == "Confirm" || guest.Response == "???? ??????")
                        {
                            return "?????";
                        }

                    }


                    else
                    {
                        if (guest.whatsappMessageId != null)
                        {
                            if (!Convert.ToBoolean(guest.TextDelivered) && !Convert.ToBoolean(guest.TextRead))
                            {
                                return " ?? ????? ??????? ?? ??????";
                            }
                            else
                            {
                                return " ?????? ?? ???";
                            }

                        }
                        else
                        {
                            return " ?????? ?? ???";
                        }

                    }

                }
                else if (guest.ImgRead == true || guest.ImgDelivered == true || guest.ImgSent == true)
                {
                    if (guest.Response == "??????" || guest.Response == "Decline" || guest.Response == "???????? ?? ??????")
                    {
                        return "??????";
                    }
                    else
                    {
                        return "?????";
                    }
                }
                else
                {
                    if (guest.Response == "??????" || guest.Response == "Decline" || guest.Response == "???????? ?? ??????")
                    {
                        return "??????";
                    }
                    else if (guest.Response == "?????" || guest.Response == "Confirm" || guest.Response == "???? ??????")
                    {
                        return "?????";
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
                        return "???? ???? ???? ????? ?? ??????? ??????? ???? ????? ????????";
                    }
                    else
                    {
                        return "??? ????? ???????";
                    }

                }
                else
                {
                    if (guest.Response == "??????" || guest.Response == "Decline" || guest.Response == "???????? ?? ??????")
                    {
                        return "??????";
                    }
                    else if (guest.Response == "?????" || guest.Response == "Confirm" || guest.Response == "???? ??????")
                    {
                        return "?????";
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
