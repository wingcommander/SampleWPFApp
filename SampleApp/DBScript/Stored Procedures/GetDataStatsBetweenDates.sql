

CREATE PROCEDURE [dbo].[GetDataStatsBetweenDates]
@Start DATETIME, @End DATETIME
WITH EXECUTE AS CALLER
AS
SELECT AVG(Price) AS Average, STDEV(Price) AS StandardDeviation, MAX(Price) AS MaxPrice, 
MIN(Price) AS MinPrice FROM dbo.Data WHERE Date BETWEEN @Start AND @End
