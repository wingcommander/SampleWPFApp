
CREATE PROCEDURE [dbo].[SaveData]
@Date DATETIME, @Price DECIMAL (18, 8)
WITH EXECUTE AS CALLER
AS
 INSERT INTO [dbo].[Data] (
  [Date],
  [Price]
 ) VALUES (
  @Date,
  @Price
 )
