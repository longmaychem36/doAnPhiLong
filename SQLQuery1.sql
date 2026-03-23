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
INSERT INTO UserRoles (UserId, RoleId)
VALUES (SCOPE_IDENTITY(), 2);