USE MakerSpot;
GO

-- Kiểm tra xem cột ImageUrl đã có chưa, nếu chưa thì thêm vào
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ImageUrl' AND Object_ID = Object_ID(N'dbo.Messages'))
BEGIN
    ALTER TABLE Messages ADD ImageUrl NVARCHAR(255) NULL;
END
GO

-- Kiểm tra xem cột SharedProductId đã có chưa, nếu chưa thì thêm vào
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SharedProductId' AND Object_ID = Object_ID(N'dbo.Messages'))
BEGIN
    ALTER TABLE Messages ADD SharedProductId INT NULL;
    
    -- Thêm khoá ngoại
    ALTER TABLE Messages 
    ADD CONSTRAINT FK_Messages_SharedProduct FOREIGN KEY (SharedProductId) REFERENCES Products(ProductId);
END
GO
