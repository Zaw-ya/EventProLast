-- =============================================
-- Create/Alter View: Vw_Events
-- Description: View combining Events with EventCategory and Users
--              Includes soft-delete columns (IsDeleted, DeletedBy, DeletedOn, DeletedBy_FirstName, DeletedBy_LastName)
-- =============================================

-- Drop view if exists
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[Vw_Events]'))
    DROP VIEW [dbo].[Vw_Events]
GO

-- Create the view
CREATE VIEW [dbo].[Vw_Events]
AS
SELECT
    e.Id,
    e.EventCode,
    e.EventTitle,
    e.SystemEventTitle,
    e.Type,
    e.EventFrom,
    e.EventTo,
    e.EventVenue,
    CONCAT('E', REPLACE(STR(e.Id, 5), SPACE(1), '0')) AS GmapCode,
    e.GmapCode AS GLocation,
    ec.Icon,
    e.EventDescription,
    e.CreatedBy,
    e.CreatedFor,
    e.CreatedOn,
    e.ModifiedBy,
    e.ModifiedOn,
    e.IsArchived,
    e.Status,
    u.FirstName,
    u.LastName,
    ec.Category,
    e.LinkedEvent,

    -- Soft Delete columns
    e.IsDeleted,
    e.DeletedBy,
    e.DeletedOn,
    deletedUser.FirstName AS DeletedBy_FirstName,
    deletedUser.LastName AS DeletedBy_LastName

FROM
    Events (NOLOCK) e
    INNER JOIN EventCategory (NOLOCK) ec ON e.Type = ec.EventId
    INNER JOIN Users (NOLOCK) u ON e.CreatedBy = u.UserId
    LEFT JOIN Users (NOLOCK) deletedUser ON e.DeletedBy = deletedUser.UserId
GO

-- Verify the view
SELECT TOP 10 * FROM Vw_Events
GO
