CREATE TABLE [dbo].[Data] (
    [ID]    INT             IDENTITY (1, 1) NOT NULL,
    [Date]  DATETIME        NOT NULL,
    [Price] DECIMAL (18, 8) NOT NULL,
    CONSTRAINT [PrimaryKey_5547a87d-d684-4e23-be31-e0d724312c1d] PRIMARY KEY CLUSTERED ([ID] ASC)
);

