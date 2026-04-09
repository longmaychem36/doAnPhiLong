USE MakerSpot;
GO

/* =========================================================
   FORUM: Bảng ForumPosts
   ========================================================= */
IF OBJECT_ID(N'dbo.ForumPosts', N'U') IS NULL
BEGIN
    CREATE TABLE ForumPosts (
        ForumPostId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        Tag NVARCHAR(50) NULL,
        ViewCount INT NOT NULL CONSTRAINT DF_ForumPosts_ViewCount DEFAULT 0,
        IsPinned BIT NOT NULL CONSTRAINT DF_ForumPosts_IsPinned DEFAULT 0,
        IsLocked BIT NOT NULL CONSTRAINT DF_ForumPosts_IsLocked DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ForumPosts_CreatedAt DEFAULT SYSDATETIME(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_ForumPosts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
    );
END
GO

/* =========================================================
   FORUM: Bảng ForumReplies
   ========================================================= */
IF OBJECT_ID(N'dbo.ForumReplies', N'U') IS NULL
BEGIN
    CREATE TABLE ForumReplies (
        ForumReplyId INT IDENTITY(1,1) PRIMARY KEY,
        ForumPostId INT NOT NULL,
        UserId INT NOT NULL,
        Content NVARCHAR(MAX) NOT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_ForumReplies_IsDeleted DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ForumReplies_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT FK_ForumReplies_ForumPosts FOREIGN KEY (ForumPostId) REFERENCES ForumPosts(ForumPostId) ON DELETE CASCADE,
        CONSTRAINT FK_ForumReplies_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
    );
END
GO
