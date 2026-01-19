-- ============================================
-- Script: Create ProcScanSummary Stored Procedure
-- Description: Gets scan summary for recent events (top 200)
-- Date: 2026-01-19
-- By: AliHani
-- ============================================

-- Drop the procedure if it exists
IF EXISTS (SELECT * FROM sys.procedures WHERE object_id = OBJECT_ID(N'[dbo].[ProcScanSummary]'))
BEGIN
    DROP PROCEDURE [dbo].[ProcScanSummary]
END
GO

-- Create the stored procedure
CREATE PROCEDURE [dbo].[ProcScanSummary]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 200
        e.id,
        e.LinkedEvent,
        e.EventTitle,
        e.SystemEventTitle ,
        

        -- Total guests (sum of NoOfMembers)
        (
            SELECT ISNULL(SUM(g.NoOfMembers), 0)
            FROM Guest g
            WHERE g.EventId = e.id
        ) AS TotalGuests,

        -- Count of allowed scans
        COUNT(CASE WHEN sch.ResponseCode = 'Allowed' THEN 1 END) AS 'Allowed',

        -- Count of declined scans
        COUNT(CASE WHEN sch.ResponseCode = 'Declined' THEN 1 END) AS 'Declined'

    FROM Events e
        LEFT JOIN Guest g ON e.Id = g.EventId
        LEFT JOIN ScanHistory sch ON sch.GuestId = g.GuestId

    WHERE e.EventFrom <= GETDATE() + 1

    GROUP BY
        e.id,
        e.LinkedEvent,
        e.EventTitle , 
        e.SystemEventTitle

    ORDER BY e.id DESC
END
GO


