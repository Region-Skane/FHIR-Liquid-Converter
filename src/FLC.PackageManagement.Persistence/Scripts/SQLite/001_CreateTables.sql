CREATE TABLE IF NOT EXISTS flcImplementationGuide (
  id					INTEGER PRIMARY KEY,
  implementationGuideId	TEXT	NULL,
  url					TEXT    NOT NULL,
  version				TEXT    NOT NULL,
  packageId				TEXT    NOT NULL,
  createdAt				TEXT    NOT NULL,
  updatedAt				TEXT    NOT NULL,
  urlVersion			TEXT GENERATED ALWAYS AS (url || '|' || version) VIRTUAL,
  resourceJson			TEXT    NULL,
  CHECK (implementationGuideId IS NULL OR (length(implementationGuideId) between 1 and 64 and implementationGuideId NOT GLOB '*[^A-Za-z0-9.-]*')),
  CHECK (length(packageId)  between 1 and 64 and packageId  NOT GLOB '*[^A-Za-z0-9.-]*'),
  CHECK (resourceJson IS NULL OR json_valid(resourceJson)=1)
);

CREATE UNIQUE INDEX IF NOT EXISTS UX_flcImplementationGuide_Url_Version ON flcImplementationGuide (url COLLATE NOCASE, version COLLATE NOCASE);

CREATE TABLE IF NOT EXISTS flcLibrary (
  id						INTEGER PRIMARY KEY AUTOINCREMENT,
  libraryId					TEXT    NULL,
  url						TEXT    NOT NULL,
  version					TEXT    NOT NULL,
  flcImplementationGuideId	INTEGER NOT NULL,
  storageRoot				TEXT    NULL,
  checksum					BLOB    NULL,
  createdAt					TEXT    NOT NULL,
  updatedAt					TEXT    NOT NULL,
  urlVersion				TEXT GENERATED ALWAYS AS (url || '|' || version) VIRTUAL,
  resourceJson				TEXT    NULL,
  CHECK (libraryId IS NULL OR (length(libraryId) between 1 and 64 and libraryId NOT GLOB '*[^A-Za-z0-9.-]*')),
  CHECK (resourceJson IS NULL OR json_valid(resourceJson)=1),
  FOREIGN KEY (flcImplementationGuideId) REFERENCES flcImplementationGuide(id) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX IF NOT EXISTS UX_flcLibrary_Url_Version ON flcLibrary (url COLLATE NOCASE, version COLLATE NOCASE);
CREATE INDEX IF NOT EXISTS IX_flcLibrary_flcImplementationGuideId ON flcLibrary (flcImplementationGuideId);
-- Make flcLibrary PK start on 1001
-- Initialize AUTOINCREMENT starting point if not already present
INSERT INTO sqlite_sequence (name, seq)
SELECT 'flcLibrary', 1000
WHERE NOT EXISTS (SELECT 1 FROM sqlite_sequence WHERE name = 'flcLibrary');

CREATE TABLE IF NOT EXISTS flcStructureMap (
  id						INTEGER PRIMARY KEY AUTOINCREMENT,
  structureMapId			TEXT    NULL,
  url						TEXT    NOT NULL,
  version					TEXT    NOT NULL,
  flcImplementationGuideId	INTEGER NOT NULL,
  source					TEXT    NULL,
  sourceVersion				TEXT    NULL,
  target					TEXT    NULL,
  targetVersion				TEXT    NULL,
  flcLibraryId				INTEGER NOT NULL,
  entryTemplate				TEXT    NULL,
  createdAt					TEXT    NOT NULL,
  updatedAt					TEXT    NOT NULL,
  urlVersion				TEXT GENERATED ALWAYS AS (url || '|' || version) VIRTUAL,
  resourceJson				TEXT    NULL,
  CHECK (structureMapId IS NULL OR (length(structureMapId) between 1 and 64 and structureMapId NOT GLOB '*[^A-Za-z0-9.-]*')),
  CHECK (resourceJson IS NULL OR json_valid(resourceJson)=1),
  FOREIGN KEY (flcImplementationGuideId) REFERENCES flcImplementationGuide(id) ON DELETE NO ACTION,
  FOREIGN KEY (flcLibraryId) REFERENCES flcLibrary(id) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX IF NOT EXISTS UX_flcStructureMap_Ig_Url_Version ON flcStructureMap (url COLLATE NOCASE, version COLLATE NOCASE);
CREATE INDEX IF NOT EXISTS IX_flcStructureMap_flcImplementationGuideId ON flcStructureMap (flcImplementationGuideId);
CREATE INDEX IF NOT EXISTS IX_flcStructureMap_flcLibraryId ON flcStructureMap (flcLibraryId);

-- Make flcStructureMap PK start on 20001
-- Initialize AUTOINCREMENT starting point if not already present
INSERT INTO sqlite_sequence (name, seq)
SELECT 'flcStructureMap', 20000
WHERE NOT EXISTS (SELECT 1 FROM sqlite_sequence WHERE name = 'flcStructureMap');
