USE MakerSpot;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Conversations')
BEGIN
    CREATE TABLE Conversations (
        ConversationId INT IDENTITY(1,1) PRIMARY KEY,
        User1Id INT NOT NULL,
        User2Id INT NOT NULL,
        LastMessageAt DATETIME2 NOT NULL CONSTRAINT DF_Conversations_LastMsg DEFAULT SYSDATETIME(),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Conversations_Created DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Conversations_User1 FOREIGN KEY (User1Id) REFERENCES Users(UserId),
        CONSTRAINT FK_Conversations_User2 FOREIGN KEY (User2Id) REFERENCES Users(UserId),
        CONSTRAINT CHK_Users_Different CHECK (User1Id < User2Id)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Messages')
BEGIN
    CREATE TABLE Messages (
        MessageId INT IDENTITY(1,1) PRIMARY KEY,
        ConversationId INT NOT NULL,
        SenderId INT NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        IsRead BIT NOT NULL CONSTRAINT DF_Messages_IsRead DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Messages_Created DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Messages_Conversation FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId) ON DELETE CASCADE,
        CONSTRAINT FK_Messages_Sender FOREIGN KEY (SenderId) REFERENCES Users(UserId) 
    );
    
    -- Insert sample data
    INSERT INTO Conversations (User1Id, User2Id, LastMessageAt)
    VALUES (1, 2, SYSDATETIME());

    DECLARE @ConvId INT = SCOPE_IDENTITY();
    IF @ConvId IS NOT NULL
    BEGIN
        INSERT INTO Messages (ConversationId, SenderId, Content, IsRead, CreatedAt)
        VALUES 
        (@ConvId, 1, N'Chào bạn, tính năng xịn quá!', 1, DATEADD(minute, -10, SYSDATETIME())),
        (@ConvId, 2, N'Cảm ơn bạn nhé!', 0, SYSDATETIME());
    END
END
GO
