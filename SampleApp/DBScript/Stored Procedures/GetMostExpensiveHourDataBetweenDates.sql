CREATE PROCEDURE [dbo].[GetMostExpensiveHourDataBetweenDates]
	@Start datetime,
	@End datetime
AS
	SELECT TOP 1 date,ThisHourPrice, NextHalfHrPrice,AndNextHalfHrPrice, (ThisHourPrice + NextHalfHrPrice + AndNextHalfHrPrice) AS TotalHourPrice 
FROM 
	(SELECT date, price AS ThisHourPrice, LEAD (Price, 1, 0) OVER (ORDER BY date ) AS NextHalfHrPrice, 
	LEAD (Price, 2, 0) OVER (ORDER BY date ) AS AndNextHalfHrPrice

		FROM (SELECT max(date) as date, sum(price) AS price FROM data
		GROUP BY DATEPART(YEAR,Date),
		DATEPART(MONTH,Date),
		DATEPART(DAY,Date),
		DATEPART(HOUR,Date),
		DATEPART(MINUTE,Date)) AS a ) AS b

	WHERE DATEPART(MINUTE,Date) = '00'
	AND date between @Start and @End
	ORDER BY TotalHourPrice DESC