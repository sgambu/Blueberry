
CREATE DATABASE Blueberry;
GO

USE Blueberry;
GO

-- 1. Category
CREATE TABLE Category
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Tps BIT NOT NULL,
    Tvq BIT NOT NULL,
    IsActive BIT NOT NULL
);

-- 2. Product
CREATE TABLE Product
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    PriceDefault DECIMAL(10,2) NOT NULL,
    CategoryId INT NOT NULL,
    IsActive BIT NOT NULL,

    CONSTRAINT FK_Product_Category
        FOREIGN KEY (CategoryId)
        REFERENCES Category(Id)
);

-- 3. Merchant
CREATE TABLE Merchant
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Adresse NVARCHAR(100),
    Phone NVARCHAR(20)
);

-- 4. MerchantProduct
CREATE TABLE MerchantProduct
(
    MerchantId INT NOT NULL,
    ProductId INT NOT NULL,

    Price DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL,

    CONSTRAINT PK_MerchantProduct
        PRIMARY KEY (MerchantId, ProductId),

    CONSTRAINT FK_MerchantProduct_Merchant
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant(Id),

    CONSTRAINT FK_MerchantProduct_Product
        FOREIGN KEY (ProductId)
        REFERENCES Product(Id),

);

-- 5. MerchantCategory
CREATE TABLE MerchantCategory
(
    MerchantId INT NOT NULL,
    CategoryId INT NOT NULL,

    CONSTRAINT PK_MerchantCategory
        PRIMARY KEY (MerchantId, CategoryId),

    CONSTRAINT FK_MerchantCategory_Merchant
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant(Id),

    CONSTRAINT FK_MerchantCategory_Category
        FOREIGN KEY (CategoryId)
        REFERENCES Category(Id)
);


-- 6. Sale
CREATE TABLE Sale
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MerchantId INT NOT NULL,
    Date DATETIME2 NOT NULL,
    Total DECIMAL(10,2) NOT NULL,
    TransNumber NVARCHAR(20),
    AuthNumber NVARCHAR(20),

    CONSTRAINT FK_Sale_Merchant
        FOREIGN KEY (MerchantId)
        REFERENCES Merchant(Id)
);

-- 7. SaleItem
CREATE TABLE SaleItem
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SaleId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    TPSAmount DECIMAL(10,2) NOT NULL,
    TVQAmount DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_SaleItem_Sale
        FOREIGN KEY (SaleId)
        REFERENCES Sale(Id),

    CONSTRAINT FK_SaleItem_Product
        FOREIGN KEY (ProductId)
        REFERENCES Product(Id)
);
