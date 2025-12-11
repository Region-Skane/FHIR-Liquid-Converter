USE [flc-transformer];

CREATE TABLE dbo.flcImplementationGuide (
  id					INT IDENTITY(1,1) CONSTRAINT PK_flcImplementationGuide_Id PRIMARY KEY,
  implementationGuideId	NVARCHAR(64)  NULL,
  [url]					NVARCHAR(350) NOT NULL,
  [version]				NVARCHAR(64)  NOT NULL,
  packageId				NVARCHAR(64)  NOT NULL,
  createdAt				DATETIME2(3)  NOT NULL,
  updatedAt				DATETIME2(3)  NOT NULL,    
  urlVersion			AS (CONVERT(NVARCHAR(415), url + N'|' + [version])) PERSISTED,
  resourceJson			NVARCHAR(MAX) NULL,
  CONSTRAINT CK_flcImplementationGuide_implementationGuideId_Chars CHECK ([implementationGuideId] IS NULL OR (LEN([implementationGuideId]) BETWEEN 1 AND 64 AND [implementationGuideId] NOT LIKE N'%[^-0-9A-Za-z.]%')),
  CONSTRAINT CK_flcImplementationGuide_packageId_Chars CHECK (LEN(packageId) BETWEEN 1 AND 64 AND packageId NOT LIKE N'%[^-0-9A-Za-z.]%'),
  CONSTRAINT CK_flcImplementationGuide_resourceJson_IsJson CHECK (resourceJson IS NULL OR ISJSON(resourceJson) = 1)
);
GO
CREATE UNIQUE INDEX UX_flcImplementationGuide_Url_Version ON dbo.flcImplementationGuide (url, [version]);
GO

CREATE TABLE dbo.flcLibrary (
  id                        INT IDENTITY(1,1) CONSTRAINT PK_flcLibrary_Id PRIMARY KEY,
  libraryId					NVARCHAR(64) NULL,
  [url]                     NVARCHAR(350) NOT NULL,
  [version]                 NVARCHAR(64)  NOT NULL,
  flcImplementationGuideId  INT NOT NULL CONSTRAINT FK_flcLibrary_flcImplementationGuideId REFERENCES dbo.flcImplementationGuide(id),
  storageRoot               NVARCHAR(400) NULL,
  checksum                  VARBINARY(32) NULL,
  createdAt                 DATETIME2(3)  NOT NULL,
  updatedAt					DATETIME2(3)  NOT NULL,    
  urlVersion                AS (CONVERT(NVARCHAR(415), url + N'|' + [version])) PERSISTED,
  resourceJson              NVARCHAR(MAX) NULL,
  CONSTRAINT CK_flcLibrary_resourceJson_IsJson CHECK (resourceJson IS NULL OR ISJSON(resourceJson) = 1),  
  CONSTRAINT CK_flcLibrary_libraryId_Chars CHECK ([libraryId] IS NULL OR (LEN([libraryId]) BETWEEN 1 AND 64 AND [libraryId] NOT LIKE N'%[^-0-9A-Za-z.]%'))
);
GO

CREATE UNIQUE INDEX UX_flcLibrary_Url_Version ON dbo.flcLibrary (url, [version]);
CREATE INDEX IX_flcLibrary_flcImplementationGuideId ON dbo.flcLibrary (flcImplementationGuideId);
GO

CREATE TABLE dbo.flcStructureMap (
  id                        INT IDENTITY(1,1) CONSTRAINT PK_flcStructureMap_Id PRIMARY KEY,
  structureMapId            NVARCHAR(64)  NULL,
  [url]                     NVARCHAR(350) NOT NULL,
  [version]                 NVARCHAR(64)  NOT NULL,
  flcImplementationGuideId  INT NOT NULL CONSTRAINT FK_flcStructureMap_flcImplementationGuideId REFERENCES dbo.flcImplementationGuide(id),
  [source]                  NVARCHAR(350) NULL,
  sourceVersion             NVARCHAR(64)  NULL,
  [target]                  NVARCHAR(350) NULL,
  targetVersion             NVARCHAR(64)  NULL,
  flcLibraryId              INT NOT NULL CONSTRAINT FK_flcStructureMap_flcLibraryId REFERENCES dbo.flcLibrary(id),
  entryTemplate             NVARCHAR(200) NULL,
  createdAt                 DATETIME2(3)  NOT NULL,
  updatedAt                 DATETIME2(3)  NOT NULL,
  urlVersion                AS (CONVERT(NVARCHAR(415), url + N'|' + [version])) PERSISTED,
  resourceJson              NVARCHAR(MAX) NULL,
  CONSTRAINT CK_flcStructureMap_resourceJson_IsJson CHECK (resourceJson IS NULL OR ISJSON(resourceJson) = 1),  
  CONSTRAINT CK_flcStructureMap_structureMapId_Chars CHECK ([structureMapId] IS NULL OR (LEN([structureMapId]) BETWEEN 1 AND 64 AND [structureMapId] NOT LIKE N'%[^-0-9A-Za-z.]%'))
);
GO

CREATE UNIQUE INDEX UX_flcStructureMap_Url_Version ON dbo.flcStructureMap (url, [version]);
CREATE INDEX IX_flcStructureMap_flcImplementationGuideId ON dbo.flcStructureMap (flcImplementationGuideId);
CREATE INDEX IX_flcStructureMap_flcLibraryId ON dbo.flcStructureMap(flcLibraryId)
GO
