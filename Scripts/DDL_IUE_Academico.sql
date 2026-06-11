-- Script DDL para SQL Server (Sistema académico IUE)
-- Ejecutar en una base de datos vacía de SQL Server.

IF OBJECT_ID('dbo.VideoClases', 'U') IS NOT NULL
    DROP TABLE dbo.VideoClases;
IF OBJECT_ID('dbo.Excusas', 'U') IS NOT NULL
    DROP TABLE dbo.Excusas;
IF OBJECT_ID('dbo.EstudiantesMaterias', 'U') IS NOT NULL
    DROP TABLE dbo.EstudiantesMaterias;
IF OBJECT_ID('dbo.Materias', 'U') IS NOT NULL
    DROP TABLE dbo.Materias;
IF OBJECT_ID('dbo.Usuarios', 'U') IS NOT NULL
    DROP TABLE dbo.Usuarios;

CREATE TABLE dbo.Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Correo NVARCHAR(150) NOT NULL UNIQUE,
    Rol NVARCHAR(20) NOT NULL,
    CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('Profesor', 'Estudiante'))
);

CREATE TABLE dbo.Materias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    ProfesorId INT NOT NULL,
    CONSTRAINT FK_Materias_Usuarios FOREIGN KEY (ProfesorId)
        REFERENCES dbo.Usuarios(Id)
);

CREATE TABLE dbo.EstudiantesMaterias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId INT NOT NULL,
    MateriaId INT NOT NULL,
    CONSTRAINT UQ_EstudiantesMaterias UNIQUE (EstudianteId, MateriaId),
    CONSTRAINT FK_EstudiantesMaterias_Estudiante FOREIGN KEY (EstudianteId)
        REFERENCES dbo.Usuarios(Id),
    CONSTRAINT FK_EstudiantesMaterias_Materia FOREIGN KEY (MateriaId)
        REFERENCES dbo.Materias(Id)
);

CREATE TABLE dbo.Excusas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EstudianteId INT NOT NULL,
    MateriaId INT NOT NULL,
    Estado NVARCHAR(20) NOT NULL,
    Motivo NVARCHAR(MAX) NOT NULL,
    FechaSolicitud DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_Excusas_Estado CHECK (Estado IN ('Pendiente', 'Aprobada')),
    CONSTRAINT FK_Excusas_Estudiante FOREIGN KEY (EstudianteId)
        REFERENCES dbo.Usuarios(Id),
    CONSTRAINT FK_Excusas_Materia FOREIGN KEY (MateriaId)
        REFERENCES dbo.Materias(Id)
);

CREATE TABLE dbo.VideoClases (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MateriaId INT NOT NULL,
    VideoUrl NVARCHAR(MAX) NULL,
    ResumenIA NVARCHAR(MAX) NULL,
    FechaPublicacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_VideoClases_Materia FOREIGN KEY (MateriaId)
        REFERENCES dbo.Materias(Id)
);

GO

-- Opcional: índices para consultas frecuentes
CREATE INDEX IX_Materias_ProfesorId ON dbo.Materias(ProfesorId);
CREATE INDEX IX_EstudiantesMaterias_EstudianteId ON dbo.EstudiantesMaterias(EstudianteId);
CREATE INDEX IX_EstudiantesMaterias_MateriaId ON dbo.EstudiantesMaterias(MateriaId);
CREATE INDEX IX_Excusas_EstudianteId ON dbo.Excusas(EstudianteId);
CREATE INDEX IX_Excusas_MateriaId ON dbo.Excusas(MateriaId);
CREATE INDEX IX_VideoClases_MateriaId ON dbo.VideoClases(MateriaId);
