-- =============================================
-- Create View: vw_EventGatekeeper
-- Description: View combining Users (GateKeepers) with Roles and EventGatekeeperMapping
-- =============================================

-- Drop view if exists
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_EventGatekeeper]'))
    DROP VIEW [dbo].[vw_EventGatekeeper]
GO

-- Create the view
CREATE VIEW [dbo].[vw_EventGatekeeper]
AS
SELECT
    -- User Information
    CONCAT(u.FirstName, ' ', u.LastName) AS FullName,
    r.RoleName,
    u.UserId,
    u.UserName,
    u.Password,
    u.Email,
    u.Gender,
    u.FirstName,
    u.LastName,
    u.Address,
    u.PrimaryContactNo,
    u.SecondaryContantNo,
    u.ModeOfCommunication,
    u.CreatedOn,
    u.CreatedBy,
    u.ModifiedOn,
    u.ModifiedBy,
    u.LoginAttempt,
    u.TemporaryPass,
    u.IsActive,
    u.Approved,
    u.LockedOn,
    u.PreferedTimeZone,
    u.Role,
    u.BankAccountNo,
    u.Ibnnumber,
    u.BankName,

    -- Mapping Information
    egm.TaskId,
    egm.EventId
FROM
    Users u
    LEFT JOIN Roles r ON u.Role = r.Id
    LEFT JOIN EventGatekeeperMapping egm ON u.UserId = egm.GateKeeperId
WHERE
    r.RoleName = 'GateKeeper'
GO

-- Verify the view
SELECT TOP 10 * FROM vw_EventGatekeeper
GO
