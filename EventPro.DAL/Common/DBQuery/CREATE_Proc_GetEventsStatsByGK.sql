-- ============================================
-- Script: Create Proc_GetEventsStatsByGK Stored Procedure
-- Description: Gets events statistics for a specific gatekeeper with pagination
-- Created: 19-01-2026
-- Updated: 19-01-2026 
-- By: AliHani
-- ============================================

-- Drop the procedure if it exists
IF EXISTS (SELECT * FROM sys.procedures WHERE object_id = OBJECT_ID(N'[dbo].[Proc_GetEventsStatsByGK]'))
BEGIN
    DROP PROCEDURE [dbo].[Proc_GetEventsStatsByGK]
END
GO

-- Create the stored procedure
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Proc_GetEventsStatsByGK]
    @GatekeeperId INT NULL,
    @pageNo INT = 1,
    @PageSize INT = 30,
    @NoOfPages INT = 0 OUTPUT
AS
BEGIN
    BEGIN
        SET NOCOUNT ON;

        -- CTE to get event statistics
        WITH cte AS (
            SELECT
                E.id,
                E.ContactName,
                E.ContactPhone,
                RIGHT(CONVERT(VARCHAR, AttendanceTime, 100), 7) AS AttendanceTime,
                RIGHT(CONVERT(VARCHAR, LeaveTime, 100), 7) AS LeaveTime,
                GMapCode,
                Country.CountryName,
                City.CityName,
                E.SystemEventTitle AS EventTitle,  -- Using SystemEventTitle (latest version)
                E.eventFrom,
                E.eventTo,
                E.eventVenue,
                CONCAT('E', FORMAT(e.Id, '00000')) AS EventCode,
                (SELECT COUNT(Allowed.GuestId)) AS scanned
            FROM EventGatekeeperMapping EGKM
                INNER JOIN Events E ON E.Id = EGKM.EventId
                LEFT JOIN guest GH ON GH.EventId = E.Id
                LEFT JOIN ScanHistory Allowed ON Allowed.GuestId = GH.GuestId AND Allowed.ResponseCode = 'Allowed'
                LEFT JOIN City ON City.id = E.CityID
                LEFT JOIN Country ON City.CountryId = Country.Id
            WHERE EGKM.GatekeeperId = @GatekeeperId AND GH.Archived IS NULL
            GROUP BY
                E.ContactName,
                E.ContactPhone,
                E.LeaveTime,
                E.AttendanceTime,
                GMapCode,
                Eventlocation,
                Country.CountryName,
                City.CityName,
                E.id,
                E.SystemEventTitle,
                E.EventFrom,
                E.EventTo,
                E.EventVenue,
                EventCode
        )

        -- Select final results with pagination
        SELECT
            CTE.id,
            CTE.ContactName,
            CTE.ContactPhone,
            CTE.AttendanceTime,
            CTE.LeaveTime,
            'https://maps.app.goo.gl/' + GMapCode AS 'GmapCode',
            CTE.EventTitle,
            CTE.eventFrom,
            CTE.eventTo,
            CTE.eventVenue,
            EventCode,
            scanned,
            CountryName + '|' + CityName AS Eventlocation,
            (SELECT SUM(g.NoOfMembers)) AS 'totalAllocated'
        FROM cte
            LEFT JOIN guest G ON G.EventId = cte.Id
        GROUP BY
            cte.ContactName,
            cte.ContactPhone,
            cte.LeaveTime,
            cte.AttendanceTime,
            GMapCode,
            CountryName,
            CityName,
            cte.id,
            cte.EventTitle,
            cte.EventFrom,
            cte.EventTo,
            cte.EventVenue,
            EventCode,
            cte.scanned,
            GmapCode
        ORDER BY EventFrom DESC
        OFFSET ((@pageNo - 1) * @PageSize) ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END

    -- Calculate total pages count (only on first page request for performance)
    IF (@pageNo = 1)
    BEGIN
        SELECT @NoOfPages = CEILING(CAST(COUNT_BIG(DISTINCT E.Id) AS FLOAT) / @PageSize)
        FROM EventGatekeeperMapping EGKM
            INNER JOIN Events E ON E.Id = EGKM.EventId
        WHERE EGKM.GatekeeperId = @GatekeeperId
    END
END
GO

PRINT 'Stored Procedure Proc_GetEventsStatsByGK created successfully!'
GO

-- Test the procedure (uncomment to test)
/*
DECLARE @Totalcount INT;
EXEC Proc_GetEventsStatsByGK
    @GatekeeperId = 25,
    @pageNo = 1,
    @PageSize = 1,
    @NoOfPages = @Totalcount OUTPUT;
SELECT @Totalcount AS TotalPages;
*/
