CREATE TABLE [dbo].[MeterReadings]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [AccountId] INT NULL, 
    [ReadingDateTime] DATETIME NULL, 
    [ReadValue] INT NULL, 
    CONSTRAINT [FK_MeterReadings_ToTable] FOREIGN KEY ([AccountId]) REFERENCES Accounts([Id]),
    
)
