-- ============================================
-- Script: Create vw_ScannedInfo View
-- Description: This view aggregates guest information with scan history
-- Date: 2026-01-19
-- By: AliHani
-- ============================================

-- Drop the view if it exists
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ScannedInfo]'))
BEGIN
    DROP VIEW [dbo].[vw_ScannedInfo]
END
GO

-- Create the view
CREATE VIEW [dbo].[vw_ScannedInfo] AS
SELECT
    -- Guest Basic Information
    g.GuestId,
    ISNULL(g.GuestArchieved, 0) AS GuestArchieved,
    g.EventId,
    g.FirstName,
    g.LastName,
    g.Address,
    g.PrimaryContactNo,
    g.SecondaryContactNo,
    g.EmailAddress,
    g.ModeOfCommunication,
    g.NoOfMembers,
    g.CreatedOn,
    g.CreatedBy,
    g.Source,
    g.WAResponseTime,
    g.GateKeeper,
    g.MessageId,
    g.AdditionalText,
    g.Cypertext,
    g.IsPhoneNumberValid,
    g.WhatsappStatus,
    g.Response,

    -- WhatsApp Message IDs
    g.whatsappMessageId,
    g.whatsappMessageImgId,
    g.ImgSentMsgId,

    -- Text Message Status
    g.TextSent,
    g.TextDelivered,
    g.TextRead,
    g.TextFailed,

    -- Image Message Status
    g.ImgSent,
    g.ImgDelivered,
    g.ImgRead,
    g.ImgFailed,

    -- Event Location Message Status
    g.EventLocationSent,
    g.EventLocationRead,
    g.EventLocationDelivered,
    g.EventLocationFailed,
    g.waMessageEventLocationForSendingToAll,
    g.whatsappWatiEventLocationId,

    -- Congratulation Message Status
    g.ConguratulationMsgSent,
    g.ConguratulationMsgDelivered,
    g.ConguratulationMsgRead,
    g.ConguratulationMsgFailed,
    g.ConguratulationMsgId,
    g.WatiConguratulationMsgId,

    -- Reminder Message Status
    g.ReminderMessageId,
    g.ReminderMessageSent,
    g.ReminderMessageDelivered,
    g.ReminderMessageRead,
    g.ReminderMessageFailed,
    g.ReminderMessageWatiId,

    -- Message Timestamps
    (SELECT MAX(createdon) FROM WhatsappResponseLogs WITH (NOLOCK) WHERE WAKey = g.MessageId) AS TextTime,
    (SELECT MAX(createdon) FROM WhatsappResponseLogs WITH (NOLOCK) WHERE WAKey = g.ImgSentMsgId) AS ImageTime,

    -- Scan Information
    SUM(sh.ScanId) AS ScanId,
    COUNT(*) - 1 AS Scanned

FROM
    Guest g WITH (NOLOCK)
    LEFT JOIN ScanHistory sh WITH (NOLOCK)
        ON g.GuestId = sh.GuestId
        AND sh.ResponseCode IS NOT NULL
        AND sh.ResponseCode = 'Allowed'

GROUP BY
    g.GuestId,
    g.GuestArchieved,
    g.EventId,
    g.FirstName,
    g.LastName,
    g.Address,
    g.PrimaryContactNo,
    g.SecondaryContactNo,
    g.EmailAddress,
    g.ModeOfCommunication,
    g.NoOfMembers,
    g.CreatedOn,
    g.CreatedBy,
    g.Source,
    g.WAResponseTime,
    g.GateKeeper,
    g.MessageId,
    g.AdditionalText,
    g.Cypertext,
    g.IsPhoneNumberValid,
    g.WhatsappStatus,
    g.Response,
    g.whatsappMessageId,
    g.whatsappMessageImgId,
    g.ImgSentMsgId,
    g.TextSent,
    g.TextDelivered,
    g.TextRead,
    g.TextFailed,
    g.ImgSent,
    g.ImgDelivered,
    g.ImgRead,
    g.ImgFailed,
    g.EventLocationSent,
    g.EventLocationRead,
    g.EventLocationDelivered,
    g.EventLocationFailed,
    g.waMessageEventLocationForSendingToAll,
    g.whatsappWatiEventLocationId,
    g.ConguratulationMsgSent,
    g.ConguratulationMsgDelivered,
    g.ConguratulationMsgRead,
    g.ConguratulationMsgFailed,
    g.ConguratulationMsgId,
    g.WatiConguratulationMsgId,
    g.ReminderMessageId,
    g.ReminderMessageSent,
    g.ReminderMessageDelivered,
    g.ReminderMessageRead,
    g.ReminderMessageFailed,
    g.ReminderMessageWatiId
GO

-- Grant permissions (adjust as needed)
-- GRANT SELECT ON [dbo].[vw_ScannedInfo] TO [YourRole]
-- GO

PRINT 'View vw_ScannedInfo created successfully!'
GO
