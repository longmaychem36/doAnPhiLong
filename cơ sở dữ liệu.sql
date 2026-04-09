/* =========================================================
   MAKERSPOT DATABASE - FULL SQL SERVER SCRIPT
   Dán toàn bộ vào SSMS và chạy 1 lần
   ========================================================= */

-- 1) Tạo database
IF DB_ID(N'MakerSpot') IS NOT NULL
BEGIN
    ALTER DATABASE MakerSpot SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MakerSpot;
END
GO

CREATE DATABASE MakerSpot;
GO

USE MakerSpot;
GO

/* =========================================================
   2) TABLE: Users
   ========================================================= */
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    AvatarUrl NVARCHAR(255) NULL,
    Bio NVARCHAR(500) NULL,
    WebsiteUrl NVARCHAR(255) NULL,
    TwitterUrl NVARCHAR(255) NULL,
    LinkedinUrl NVARCHAR(255) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    IsVerified BIT NOT NULL CONSTRAINT DF_Users_IsVerified DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
GO

/* =========================================================
   3) TABLE: Roles
   ========================================================= */
CREATE TABLE Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL,
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);
GO

/* =========================================================
   4) TABLE: UserRoles
   ========================================================= */
CREATE TABLE UserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL CONSTRAINT DF_UserRoles_AssignedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

/* =========================================================
   5) TABLE: Topics
   ========================================================= */
CREATE TABLE Topics (
    TopicId INT IDENTITY(1,1) PRIMARY KEY,
    TopicName NVARCHAR(100) NOT NULL,
    Slug NVARCHAR(120) NOT NULL,
    Description NVARCHAR(300) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Topics_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Topics_TopicName UNIQUE (TopicName),
    CONSTRAINT UQ_Topics_Slug UNIQUE (Slug)
);
GO

/* =========================================================
   6) TABLE: Products
   ========================================================= */
CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProductName NVARCHAR(150) NOT NULL,
    Slug NVARCHAR(180) NOT NULL,
    Tagline NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    LogoUrl NVARCHAR(255) NULL,
    WebsiteUrl NVARCHAR(255) NOT NULL,
    DemoUrl NVARCHAR(255) NULL,
    LaunchDate DATE NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Products_Status DEFAULT N'Pending',
    IsFeatured BIT NOT NULL CONSTRAINT DF_Products_IsFeatured DEFAULT 0,
    ViewCount INT NOT NULL CONSTRAINT DF_Products_ViewCount DEFAULT 0,
    UpvoteCount INT NOT NULL CONSTRAINT DF_Products_UpvoteCount DEFAULT 0,
    CommentCount INT NOT NULL CONSTRAINT DF_Products_CommentCount DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT UQ_Products_Slug UNIQUE (Slug),
    CONSTRAINT FK_Products_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT CK_Products_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected', N'Hidden'))
);
GO

/* =========================================================
   7) TABLE: ProductMedia
   ========================================================= */
CREATE TABLE ProductMedia (
    MediaId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    MediaType NVARCHAR(20) NOT NULL,
    MediaUrl NVARCHAR(255) NOT NULL,
    ThumbnailUrl NVARCHAR(255) NULL,
    DisplayOrder INT NOT NULL CONSTRAINT DF_ProductMedia_DisplayOrder DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProductMedia_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_ProductMedia_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT CK_ProductMedia_MediaType CHECK (MediaType IN (N'Image', N'Video'))
);
GO

/* =========================================================
   8) TABLE: ProductTopics
   ========================================================= */
CREATE TABLE ProductTopics (
    ProductId INT NOT NULL,
    TopicId INT NOT NULL,
    CONSTRAINT PK_ProductTopics PRIMARY KEY (ProductId, TopicId),
    CONSTRAINT FK_ProductTopics_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_ProductTopics_Topics FOREIGN KEY (TopicId) REFERENCES Topics(TopicId)
);
GO

/* =========================================================
   9) TABLE: ProductMakers
   ========================================================= */
CREATE TABLE ProductMakers (
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    MakerRole NVARCHAR(50) NULL,
    CONSTRAINT PK_ProductMakers PRIMARY KEY (ProductId, UserId),
    CONSTRAINT FK_ProductMakers_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_ProductMakers_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* =========================================================
   10) TABLE: ProductUpvotes
   ========================================================= */
CREATE TABLE ProductUpvotes (
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ProductUpvotes_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_ProductUpvotes PRIMARY KEY (ProductId, UserId),
    CONSTRAINT FK_ProductUpvotes_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_ProductUpvotes_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* =========================================================
   11) TABLE: Comments
   ========================================================= */
CREATE TABLE Comments (
    CommentId INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    ParentCommentId INT NULL,
    Content NVARCHAR(1000) NOT NULL,
    IsEdited BIT NOT NULL CONSTRAINT DF_Comments_IsEdited DEFAULT 0,
    IsDeleted BIT NOT NULL CONSTRAINT DF_Comments_IsDeleted DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Comments_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_Comments_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Comments_Parent FOREIGN KEY (ParentCommentId) REFERENCES Comments(CommentId)
);
GO

/* =========================================================
   12) TABLE: CommentVotes
   ========================================================= */
CREATE TABLE CommentVotes (
    CommentId INT NOT NULL,
    UserId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_CommentVotes_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_CommentVotes PRIMARY KEY (CommentId, UserId),
    CONSTRAINT FK_CommentVotes_Comments FOREIGN KEY (CommentId) REFERENCES Comments(CommentId) ON DELETE CASCADE,
    CONSTRAINT FK_CommentVotes_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* =========================================================
   13) TABLE: Followers
   ========================================================= */
CREATE TABLE Followers (
    FollowerId INT NOT NULL,
    FollowingId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Followers_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Followers PRIMARY KEY (FollowerId, FollowingId),
    CONSTRAINT FK_Followers_Follower FOREIGN KEY (FollowerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Followers_Following FOREIGN KEY (FollowingId) REFERENCES Users(UserId),
    CONSTRAINT CK_Followers_NotSelf CHECK (FollowerId <> FollowingId)
);
GO

/* =========================================================
   14) TABLE: Collections
   ========================================================= */
CREATE TABLE Collections (
    CollectionId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CollectionName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NULL,
    IsPublic BIT NOT NULL CONSTRAINT DF_Collections_IsPublic DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Collections_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Collections_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* =========================================================
   15) TABLE: CollectionItems
   ========================================================= */
CREATE TABLE CollectionItems (
    CollectionId INT NOT NULL,
    ProductId INT NOT NULL,
    AddedAt DATETIME2 NOT NULL CONSTRAINT DF_CollectionItems_AddedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_CollectionItems PRIMARY KEY (CollectionId, ProductId),
    CONSTRAINT FK_CollectionItems_Collections FOREIGN KEY (CollectionId) REFERENCES Collections(CollectionId) ON DELETE CASCADE,
    CONSTRAINT FK_CollectionItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

/* =========================================================
   16) TABLE: Notifications
   ========================================================= */
CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    ReferenceId INT NULL,
    Message NVARCHAR(255) NOT NULL,
    IsRead BIT NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* =========================================================
   17) TABLE: Reports
   ========================================================= */
CREATE TABLE Reports (
    ReportId INT IDENTITY(1,1) PRIMARY KEY,
    ReporterUserId INT NOT NULL,
    TargetType NVARCHAR(30) NOT NULL,
    TargetId INT NOT NULL,
    Reason NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Reports_Status DEFAULT N'Pending',
    ReviewedBy INT NULL,
    ReviewedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Reports_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Reports_Reporter FOREIGN KEY (ReporterUserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Reports_ReviewedBy FOREIGN KEY (ReviewedBy) REFERENCES Users(UserId),
    CONSTRAINT CK_Reports_TargetType CHECK (TargetType IN (N'Product', N'Comment', N'User')),
    CONSTRAINT CK_Reports_Status CHECK (Status IN (N'Pending', N'Reviewed', N'Rejected'))
);
GO

/* =========================================================
   18) TABLE: AuditLogs
   ========================================================= */
CREATE TABLE AuditLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    ActionName NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(100) NOT NULL,
    RecordId INT NULL,
    OldData NVARCHAR(MAX) NULL,
    NewData NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* =========================================================
   19) INDEXES
   ========================================================= */
CREATE INDEX IX_Products_Status_CreatedAt
ON Products(Status, CreatedAt DESC);
GO

CREATE INDEX IX_Products_UserId
ON Products(UserId);
GO

CREATE INDEX IX_ProductUpvotes_UserId
ON ProductUpvotes(UserId);
GO

CREATE INDEX IX_Comments_ProductId_CreatedAt
ON Comments(ProductId, CreatedAt DESC);
GO

CREATE INDEX IX_Notifications_UserId_IsRead
ON Notifications(UserId, IsRead);
GO

CREATE INDEX IX_Followers_FollowingId
ON Followers(FollowingId);
GO

CREATE INDEX IX_ProductTopics_TopicId
ON ProductTopics(TopicId);
GO

CREATE INDEX IX_ProductMakers_UserId
ON ProductMakers(UserId);
GO

/* =========================================================
   20) SAMPLE DATA
   ========================================================= */

-- Roles
INSERT INTO Roles (RoleName)
VALUES (N'Admin'), (N'Moderator'), (N'Member');
GO

-- Users
INSERT INTO Users
(
    Username, Email, PasswordHash, FullName, AvatarUrl, Bio,
    WebsiteUrl, TwitterUrl, LinkedinUrl, IsActive, IsVerified
)
VALUES
(
    N'admin',
    N'admin@makerspot.com',
    N'hashed_password_admin',
    N'Admin MakerSpot',
    N'/images/avatars/admin.png',
    N'Quản trị viên hệ thống MakerSpot',
    N'https://makerspot.com',
    N'https://twitter.com/makerspot_admin',
    N'https://linkedin.com/in/makerspot-admin',
    1, 1
),
(
    N'namnguyen',
    N'nam@example.com',
    N'hashed_password_nam',
    N'Nguyễn Văn Nam',
    N'/images/avatars/nam.png',
    N'Yêu thích startup và sản phẩm công nghệ.',
    N'https://namportfolio.com',
    N'https://twitter.com/namnguyen',
    N'https://linkedin.com/in/namnguyen',
    1, 1
),
(
    N'linhtran',
    N'linh@example.com',
    N'hashed_password_linh',
    N'Trần Khánh Linh',
    N'/images/avatars/linh.png',
    N'Designer và maker sản phẩm số.',
    N'https://linhdesign.com',
    N'https://twitter.com/linhtran',
    N'https://linkedin.com/in/linhtran',
    1, 1
),
(
    N'minhle',
    N'minh@example.com',
    N'hashed_password_minh',
    N'Lê Quang Minh',
    N'/images/avatars/minh.png',
    N'Sinh viên đam mê xây dựng web app.',
    N'https://minhdev.com',
    N'https://twitter.com/minhle',
    N'https://linkedin.com/in/minhle',
    1, 0
);
GO

-- UserRoles
INSERT INTO UserRoles (UserId, RoleId)
VALUES
(1, 1),
(2, 3),
(3, 3),
(4, 3);
GO

-- Topics
INSERT INTO Topics (TopicName, Slug, Description)
VALUES
(N'AI', N'ai', N'Công cụ và sản phẩm trí tuệ nhân tạo'),
(N'SaaS', N'saas', N'Phần mềm dịch vụ trên nền tảng web'),
(N'Education', N'education', N'Sản phẩm hỗ trợ học tập'),
(N'Productivity', N'productivity', N'Công cụ tăng năng suất'),
(N'Design', N'design', N'Công cụ thiết kế và sáng tạo');
GO

-- Products
INSERT INTO Products
(
    UserId, ProductName, Slug, Tagline, Description, LogoUrl,
    WebsiteUrl, DemoUrl, LaunchDate, Status, IsFeatured, ViewCount, UpvoteCount, CommentCount
)
VALUES
(
    2,
    N'StudyFlow',
    N'studyflow',
    N'Nền tảng quản lý học tập cho sinh viên',
    N'StudyFlow giúp sinh viên quản lý môn học, deadline, ghi chú và lập kế hoạch học tập trên một giao diện trực quan.',
    N'/images/products/studyflow-logo.png',
    N'https://studyflow.app',
    N'https://studyflow.app/demo',
    '2026-03-01',
    N'Approved',
    1,
    120,
    2,
    2
),
(
    3,
    N'DesignSpark',
    N'designspark',
    N'Công cụ tạo bộ màu và font cho designer',
    N'DesignSpark giúp nhà thiết kế tạo nhanh color palette, font pairing và lưu bộ nhận diện cho dự án.',
    N'/images/products/designspark-logo.png',
    N'https://designspark.app',
    N'https://designspark.app/demo',
    '2026-03-05',
    N'Approved',
    0,
    80,
    1,
    1
),
(
    4,
    N'PitchMate',
    N'pitchmate',
    N'Tạo pitch deck nhanh cho startup',
    N'PitchMate hỗ trợ startup tạo pitch deck theo mẫu có sẵn, gợi ý nội dung và xuất file trình chiếu.',
    N'/images/products/pitchmate-logo.png',
    N'https://pitchmate.app',
    N'https://pitchmate.app/demo',
    '2026-03-08',
    N'Pending',
    0,
    35,
    0,
    0
);
GO

-- ProductMedia
INSERT INTO ProductMedia (ProductId, MediaType, MediaUrl, ThumbnailUrl, DisplayOrder)
VALUES
(1, N'Image', N'/images/products/studyflow-1.png', N'/images/products/studyflow-1-thumb.png', 1),
(1, N'Image', N'/images/products/studyflow-2.png', N'/images/products/studyflow-2-thumb.png', 2),
(1, N'Video', N'https://youtube.com/watch?v=studyflowdemo', N'/images/products/studyflow-video-thumb.png', 3),
(2, N'Image', N'/images/products/designspark-1.png', N'/images/products/designspark-1-thumb.png', 1);
GO

-- ProductTopics
INSERT INTO ProductTopics (ProductId, TopicId)
VALUES
(1, 3), -- Education
(1, 4), -- Productivity
(2, 5), -- Design
(2, 2), -- SaaS
(3, 2), -- SaaS
(3, 4); -- Productivity
GO

-- ProductMakers
INSERT INTO ProductMakers (ProductId, UserId, MakerRole)
VALUES
(1, 2, N'Founder'),
(1, 4, N'Developer'),
(2, 3, N'Founder'),
(3, 4, N'Founder');
GO

-- ProductUpvotes
INSERT INTO ProductUpvotes (ProductId, UserId)
VALUES
(1, 3),
(1, 4),
(2, 2);
GO

-- Comments
INSERT INTO Comments (ProductId, UserId, ParentCommentId, Content, IsEdited, IsDeleted)
VALUES
(1, 3, NULL, N'Giao diện đẹp và rất phù hợp cho sinh viên.', 0, 0),
(1, 2, 1, N'Cảm ơn bạn, mình sẽ cập nhật thêm tính năng calendar sync.', 0, 0),
(2, 4, NULL, N'Tool này khá hữu ích cho team design nhỏ.', 0, 0);
GO

-- CommentVotes
INSERT INTO CommentVotes (CommentId, UserId)
VALUES
(1, 2),
(1, 4),
(3, 3);
GO

-- Followers
INSERT INTO Followers (FollowerId, FollowingId)
VALUES
(3, 2),
(4, 2),
(2, 3);
GO

-- Collections
INSERT INTO Collections (UserId, CollectionName, Description, IsPublic)
VALUES
(2, N'Tools yêu thích', N'Danh sách các công cụ tôi đánh giá cao', 1),
(3, N'Best for Designers', N'Bộ sưu tập dành cho designer', 1),
(4, N'Build later', N'Các sản phẩm muốn tham khảo sau', 0);
GO

-- CollectionItems
INSERT INTO CollectionItems (CollectionId, ProductId)
VALUES
(1, 2),
(2, 1),
(3, 1);
GO

-- Notifications
INSERT INTO Notifications (UserId, Type, ReferenceId, Message, IsRead)
VALUES
(2, N'NewUpvote', 1, N'Sản phẩm StudyFlow vừa nhận thêm một upvote mới.', 0),
(2, N'NewComment', 1, N'Có bình luận mới trên sản phẩm StudyFlow.', 0),
(3, N'NewFollower', 2, N'Nguyễn Văn Nam vừa theo dõi bạn.', 1);
GO

-- Reports
INSERT INTO Reports (ReporterUserId, TargetType, TargetId, Reason, Description, Status, ReviewedBy, ReviewedAt)
VALUES
(4, N'Product', 3, N'Nội dung chưa rõ ràng', N'Mô tả sản phẩm còn sơ sài, cần bổ sung thêm ảnh demo.', N'Pending', NULL, NULL);
GO

-- AuditLogs
INSERT INTO AuditLogs (UserId, ActionName, TableName, RecordId, OldData, NewData)
VALUES
(1, N'INSERT', N'Products', 1, NULL, N'StudyFlow created'),
(1, N'INSERT', N'Products', 2, NULL, N'DesignSpark created');
GO

/* =========================================================
   21) TRIGGER: Tự cập nhật UpvoteCount khi insert/delete upvote
   ========================================================= */
CREATE TRIGGER TR_ProductUpvotes_AfterInsert
ON ProductUpvotes
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.UpvoteCount = (
        SELECT COUNT(*)
        FROM ProductUpvotes pu
        WHERE pu.ProductId = p.ProductId
    )
    FROM Products p
    INNER JOIN (SELECT DISTINCT ProductId FROM inserted) i
        ON p.ProductId = i.ProductId;
END
GO

CREATE TRIGGER TR_ProductUpvotes_AfterDelete
ON ProductUpvotes
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.UpvoteCount = (
        SELECT COUNT(*)
        FROM ProductUpvotes pu
        WHERE pu.ProductId = p.ProductId
    )
    FROM Products p
    INNER JOIN (SELECT DISTINCT ProductId FROM deleted) d
        ON p.ProductId = d.ProductId;
END
GO

/* =========================================================
   22) TRIGGER: Tự cập nhật CommentCount khi insert/delete comment
   ========================================================= */
CREATE TRIGGER TR_Comments_AfterInsert
ON Comments
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.CommentCount = (
        SELECT COUNT(*)
        FROM Comments c
        WHERE c.ProductId = p.ProductId
          AND c.IsDeleted = 0
    )
    FROM Products p
    INNER JOIN (SELECT DISTINCT ProductId FROM inserted) i
        ON p.ProductId = i.ProductId;
END
GO

CREATE TRIGGER TR_Comments_AfterDelete
ON Comments
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.CommentCount = (
        SELECT COUNT(*)
        FROM Comments c
        WHERE c.ProductId = p.ProductId
          AND c.IsDeleted = 0
    )
    FROM Products p
    INNER JOIN (SELECT DISTINCT ProductId FROM deleted) d
        ON p.ProductId = d.ProductId;
END
GO

/* =========================================================
   23) KIỂM TRA DỮ LIỆU
   ========================================================= */
SELECT N'Users' AS TableName, COUNT(*) AS TotalRows FROM Users
UNION ALL
SELECT N'Roles', COUNT(*) FROM Roles
UNION ALL
SELECT N'UserRoles', COUNT(*) FROM UserRoles
UNION ALL
SELECT N'Topics', COUNT(*) FROM Topics
UNION ALL
SELECT N'Products', COUNT(*) FROM Products
UNION ALL
SELECT N'ProductMedia', COUNT(*) FROM ProductMedia
UNION ALL
SELECT N'ProductTopics', COUNT(*) FROM ProductTopics
UNION ALL
SELECT N'ProductMakers', COUNT(*) FROM ProductMakers
UNION ALL
SELECT N'ProductUpvotes', COUNT(*) FROM ProductUpvotes
UNION ALL
SELECT N'Comments', COUNT(*) FROM Comments
UNION ALL
SELECT N'CommentVotes', COUNT(*) FROM CommentVotes
UNION ALL
SELECT N'Followers', COUNT(*) FROM Followers
UNION ALL
SELECT N'Collections', COUNT(*) FROM Collections
UNION ALL
SELECT N'CollectionItems', COUNT(*) FROM CollectionItems
UNION ALL
SELECT N'Notifications', COUNT(*) FROM Notifications
UNION ALL
SELECT N'Reports', COUNT(*) FROM Reports
UNION ALL
SELECT N'AuditLogs', COUNT(*) FROM AuditLogs;
GO

-- Thêm tài khoản moderator
INSERT INTO Users
(
    Username,
    Email,
    PasswordHash,
    FullName,
    AvatarUrl,
    Bio,
    WebsiteUrl,
    TwitterUrl,
    LinkedinUrl,
    IsActive,
    IsVerified
)
VALUES
(
    N'moderator1',
    N'moderator1@makerspot.com',
    N'hashed_password_moderator1',
    N'Moderator MakerSpot',
    N'/images/avatars/moderator1.png',
    N'Kiểm duyệt nội dung hệ thống MakerSpot',
    N'https://makerspot.com',
    NULL,
    NULL,
    1,
    1
);

-- Gán role Moderator (RoleId = 2)
VALUES (SCOPE_IDENTITY(), 2);
GO

/* =========================================================
   20) TABLE: Conversations (Hệ thống Chat)
   ========================================================= */
CREATE TABLE Conversations (
    ConversationId INT IDENTITY(1,1) PRIMARY KEY,
    User1Id INT NOT NULL,
    User2Id INT NOT NULL,
    LastMessageAt DATETIME2 NOT NULL CONSTRAINT DF_Conversations_LastMsg DEFAULT SYSDATETIME(),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Conversations_Created DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Conversations_User1 FOREIGN KEY (User1Id) REFERENCES Users(UserId),
    CONSTRAINT FK_Conversations_User2 FOREIGN KEY (User2Id) REFERENCES Users(UserId),
    CONSTRAINT CHK_Users_Different CHECK (User1Id < User2Id) -- Đảm bảo không trùng lặp chiều chat (Vd: 1-2, không có 2-1)
);
GO

/* =========================================================
   21) TABLE: Messages (Nội dung Chat)
   ========================================================= */
CREATE TABLE Messages (
    MessageId INT IDENTITY(1,1) PRIMARY KEY,
    ConversationId INT NOT NULL,
    SenderId INT NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    IsRead BIT NOT NULL CONSTRAINT DF_Messages_IsRead DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Messages_Created DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Messages_Conversation FOREIGN KEY (ConversationId) REFERENCES Conversations(ConversationId) ON DELETE CASCADE,
    CONSTRAINT FK_Messages_Sender FOREIGN KEY (SenderId) REFERENCES Users(UserId) 
    -- Không dùng ON DELETE CASCADE cho SenderId vì có thể gây cycle cascade path
);
GO

-- CHÈN DỮ LIỆU MẪU CHO CHAT
INSERT INTO Conversations (User1Id, User2Id, LastMessageAt)
VALUES (1, 2, SYSDATETIME());
GO

DECLARE @ConvId INT = SCOPE_IDENTITY();
IF @ConvId IS NOT NULL
BEGIN
    INSERT INTO Messages (ConversationId, SenderId, Content, IsRead, CreatedAt)
    VALUES 
    (@ConvId, 1, N'Chào bạn, tính năng xịn quá!', 1, DATEADD(minute, -10, SYSDATETIME())),
    (@ConvId, 2, N'Cảm ơn bạn nhé!', 0, SYSDATETIME());
END
GO

/* =========================================================
   Phase 13) TABLE: ForumPosts — Diễn đàn thảo luận
   ========================================================= */
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
GO

/* =========================================================
   Phase 13) TABLE: ForumReplies — Trả lời bài viết forum
   ========================================================= */
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
GO