﻿CREATE PROCEDURE [dbo].[DeleteData]
WITH EXECUTE AS CALLER
AS
TRUNCATE TABLE dbo.Data
