CREATE PROCEDURE [dbo].[GetData]
WITH EXECUTE AS CALLER
AS
select * from Data
