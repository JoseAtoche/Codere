-- Crear la base de datos 'Shows' si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Shows')
BEGIN
    CREATE DATABASE Shows;
END
GO

-- Usar la base de datos 'Shows'
USE Shows;
GO

-- Crear las tablas solo si no existen
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Country')
BEGIN
    -- Crea la tabla Country
    CREATE TABLE Country (
        Id INT NOT NULL PRIMARY KEY IDENTITY,
        Name NVARCHAR(MAX) NOT NULL,
        Code NVARCHAR(MAX) NOT NULL,
        Timezone NVARCHAR(MAX) NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Externals')
BEGIN
    -- Crea la tabla Externals
    CREATE TABLE Externals (
        Id INT NOT NULL PRIMARY KEY IDENTITY,
        Tvrage INT NOT NULL,
        Thetvdb INT NOT NULL,
        Imdb NVARCHAR(MAX) NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Image')
BEGIN
    -- Crea la tabla Image
    CREATE TABLE Image (
        Id INT NOT NULL PRIMARY KEY IDENTITY,
        Medium NVARCHAR(MAX) NOT NULL,
        Original NVARCHAR(MAX) NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Link')
BEGIN
    -- Crea la tabla Link
    CREATE TABLE Link (
        Id INT NOT NULL PRIMARY KEY IDENTITY,
        SelfHref NVARCHAR(MAX) NOT NULL,
        PreviousepisodeHref NVARCHAR(MAX) NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Network')
BEGIN
    -- Crea la tabla Network
    CREATE TABLE Network (
        Id INT NOT NULL PRIMARY KEY IDENTITY,
        Name NVARCHAR(MAX) NOT NULL,
        idCountry INT NOT NULL,
        OfficialSite NVARCHAR(MAX) NOT NULL,
        CONSTRAINT FK_Network_Country_idCountry FOREIGN KEY (idCountry) REFERENCES Country (Id) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Schedule')
BEGIN
    -- Crea la tabla Schedule
    CREATE TABLE Schedule (
        Id INT NOT NULL PRIMARY KEY IDENTITY,
        Time NVARCHAR(MAX) NOT NULL,
        Days NVARCHAR(MAX) NOT NULL
    );
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Show')
BEGIN
    -- Crea la tabla Show
    CREATE TABLE Show (
        Id INT NOT NULL PRIMARY KEY IDENTITY,
        Url NVARCHAR(MAX) NOT NULL,
        Name NVARCHAR(MAX) NOT NULL,
        Type NVARCHAR(MAX) NOT NULL,
        Language NVARCHAR(MAX) NOT NULL,
        Genres NVARCHAR(MAX) NULL,
        Status NVARCHAR(MAX) NOT NULL,
        Runtime INT NOT NULL,
        AverageRuntime INT NOT NULL,
        Premiered DATETIME NOT NULL,
        Ended DATETIME NOT NULL,
        OfficialSite NVARCHAR(MAX) NOT NULL,
        IdSchedule INT NULL,
        Rating FLOAT NOT NULL,
        Weight INT NOT NULL,
        idNetwork INT NULL,
        WebChannel NVARCHAR(MAX) NOT NULL,
        DvdCountry NVARCHAR(MAX) NOT NULL,
        idExternals INT NULL,
        idImage INT NULL,
        Summary NVARCHAR(MAX) NOT NULL,
        Updated BIGINT NOT NULL,
        idLink INT NULL,
        CONSTRAINT FK_Show_Externals_idExternals FOREIGN KEY (idExternals) REFERENCES Externals (Id),
        CONSTRAINT FK_Show_Image_idImage FOREIGN KEY (idImage) REFERENCES Image (Id),
        CONSTRAINT FK_Show_Link_idLink FOREIGN KEY (idLink) REFERENCES Link (Id),
        CONSTRAINT FK_Show_Network_idNetwork FOREIGN KEY (idNetwork) REFERENCES Network (Id),
        CONSTRAINT FK_Show_Schedule_IdSchedule FOREIGN KEY (IdSchedule) REFERENCES Schedule (Id)
    );
END

-- Deshabilita temporalmente las restricciones de clave externa
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- Crear  ndices si no existen
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Network_idCountry')
BEGIN
    CREATE INDEX IX_Network_idCountry ON Network (idCountry);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Show_idExternals')
BEGIN
    CREATE UNIQUE INDEX IX_Show_idExternals ON Show (idExternals);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Show_idImage')
BEGIN
    CREATE UNIQUE INDEX IX_Show_idImage ON Show (idImage);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Show_idLink')
BEGIN
    CREATE UNIQUE INDEX IX_Show_idLink ON Show (idLink);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Show_idNetwork')
BEGIN
    CREATE INDEX IX_Show_idNetwork ON Show (idNetwork);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Show_IdSchedule')
BEGIN
    CREATE UNIQUE INDEX IX_Show_IdSchedule ON Show (IdSchedule);
END

-- Habilita nuevamente las restricciones de clave externa
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';




