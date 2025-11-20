-- Script de creación de base de datos para Sistema Académico
-- Proyecto #2 - PAW

-- Crear la base de datos

CREATE DATABASE SistemaAcademico;

USE SistemaAcademico;
GO

-- Tabla de Roles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        RolId INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(50) NOT NULL,
        Descripcion NVARCHAR(200),
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Tabla de Permisos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permisos' AND xtype='U')
BEGIN
    CREATE TABLE Permisos (
        PermisoId INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(200),
        Modulo NVARCHAR(50) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1
    );
END
GO

-- Tabla intermedia RolPermisos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RolPermisos' AND xtype='U')
BEGIN
    CREATE TABLE RolPermisos (
        RolPermisoId INT IDENTITY(1,1) PRIMARY KEY,
        RolId INT NOT NULL,
        PermisoId INT NOT NULL,
        CONSTRAINT FK_RolPermisos_Rol FOREIGN KEY (RolId) REFERENCES Roles(RolId),
        CONSTRAINT FK_RolPermisos_Permiso FOREIGN KEY (PermisoId) REFERENCES Permisos(PermisoId)
    );
END
GO

-- Tabla de Usuarios
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
BEGIN
    CREATE TABLE Usuarios (
        UsuarioId INT IDENTITY(1,1) PRIMARY KEY,
        NombreCompleto NVARCHAR(100) NOT NULL,
        Correo NVARCHAR(100) NOT NULL,
        Contrasena NVARCHAR(256) NOT NULL,
        Identificacion NVARCHAR(20),
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        Activo BIT NOT NULL DEFAULT 1,
        UltimoAcceso DATETIME NULL,
        RolId INT NOT NULL,
        CONSTRAINT FK_Usuarios_Rol FOREIGN KEY (RolId) REFERENCES Roles(RolId),
        CONSTRAINT UQ_Usuarios_Correo UNIQUE (Correo),
        CONSTRAINT UQ_Usuarios_Identificacion UNIQUE (Identificacion)
    );
END
GO

-- Tabla de Cursos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cursos' AND xtype='U')
BEGIN
    CREATE TABLE Cursos (
        CursoId INT IDENTITY(1,1) PRIMARY KEY,
        Codigo NVARCHAR(20) NOT NULL,
        Nombre NVARCHAR(150) NOT NULL,
        Descripcion NVARCHAR(500),
        Creditos INT NOT NULL,
        Cuatrimestre NVARCHAR(50) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        CreadoPorId INT NULL,
        FechaModificacion DATETIME NULL,
        ModificadoPorId INT NULL,
        CONSTRAINT UQ_Cursos_Codigo UNIQUE (Codigo)
    );
END
GO

-- Tabla de asignación de docentes a cursos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CursoDocentes' AND xtype='U')
BEGIN
    CREATE TABLE CursoDocentes (
        CursoDocenteId INT IDENTITY(1,1) PRIMARY KEY,
        CursoId INT NOT NULL,
        DocenteId INT NOT NULL,
        FechaAsignacion DATETIME NOT NULL DEFAULT GETDATE(),
        Horario NVARCHAR(20),
        Activo BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_CursoDocentes_Curso FOREIGN KEY (CursoId) REFERENCES Cursos(CursoId),
        CONSTRAINT FK_CursoDocentes_Docente FOREIGN KEY (DocenteId) REFERENCES Usuarios(UsuarioId)
    );
END
GO

-- Tabla de Inscripciones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Inscripciones' AND xtype='U')
BEGIN
    CREATE TABLE Inscripciones (
        InscripcionId INT IDENTITY(1,1) PRIMARY KEY,
        EstudianteId INT NOT NULL,
        CursoId INT NOT NULL,
        FechaInscripcion DATETIME NOT NULL DEFAULT GETDATE(),
        Estado NVARCHAR(20) NOT NULL DEFAULT 'Activo',
        NotaFinal DECIMAL(5,2) NULL,
        Aprobado BIT NULL,
        FechaFinalizacion DATETIME NULL,
        CONSTRAINT FK_Inscripciones_Estudiante FOREIGN KEY (EstudianteId) REFERENCES Usuarios(UsuarioId),
        CONSTRAINT FK_Inscripciones_Curso FOREIGN KEY (CursoId) REFERENCES Cursos(CursoId)
    );
END
GO

-- Tabla de Calificaciones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Calificaciones' AND xtype='U')
BEGIN
    CREATE TABLE Calificaciones (
        CalificacionId INT IDENTITY(1,1) PRIMARY KEY,
        EstudianteId INT NOT NULL,
        CursoId INT NOT NULL,
        TipoEvaluacion NVARCHAR(50) NOT NULL,
        Descripcion NVARCHAR(200),
        Nota DECIMAL(5,2) NOT NULL,
        Porcentaje DECIMAL(5,2) NOT NULL,
        FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        Observaciones NVARCHAR(500),
        CONSTRAINT FK_Calificaciones_Estudiante FOREIGN KEY (EstudianteId) REFERENCES Usuarios(UsuarioId),
        CONSTRAINT FK_Calificaciones_Curso FOREIGN KEY (CursoId) REFERENCES Cursos(CursoId)
    );
END
GO

-- Tabla de Bitácora
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Bitacoras' AND xtype='U')
BEGIN
    CREATE TABLE Bitacoras (
        BitacoraId INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioId INT NULL,
        Accion NVARCHAR(100) NOT NULL,
        Modulo NVARCHAR(50) NOT NULL,
        Descripcion NVARCHAR(500),
        FechaHora DATETIME NOT NULL DEFAULT GETDATE(),
        DireccionIP NVARCHAR(50),
        Navegador NVARCHAR(200),
        EntidadAfectada NVARCHAR(100),
        EntidadId INT NULL,
        DatosAnteriores NVARCHAR(MAX),
        DatosNuevos NVARCHAR(MAX),
        CONSTRAINT FK_Bitacoras_Usuario FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioId)
    );
END
GO

-- Insertar roles por defecto
IF NOT EXISTS (SELECT * FROM Roles WHERE Nombre = 'Administrador')
BEGIN
    INSERT INTO Roles (Nombre, Descripcion, Activo) VALUES
    ('Administrador', 'Administrador del sistema con acceso completo', 1),
    ('Coordinador', 'Coordinador académico con acceso a reportes y gestión', 1),
    ('Docente', 'Docente con acceso a sus cursos y estudiantes', 1),
    ('Estudiante', 'Estudiante con acceso a su historial académico', 1);
END
GO

-- Insertar permisos por defecto
IF NOT EXISTS (SELECT * FROM Permisos WHERE Modulo = 'Usuarios')
BEGIN
    INSERT INTO Permisos (Nombre, Descripcion, Modulo, Activo) VALUES
    -- Permisos de Usuarios
    ('VerUsuarios', 'Ver lista de usuarios', 'Usuarios', 1),
    ('CrearUsuarios', 'Crear nuevos usuarios', 'Usuarios', 1),
    ('EditarUsuarios', 'Editar usuarios existentes', 'Usuarios', 1),
    ('EliminarUsuarios', 'Eliminar usuarios', 'Usuarios', 1),
    ('GestionarRoles', 'Gestionar roles y permisos', 'Usuarios', 1),

    -- Permisos de Cursos
    ('VerCursos', 'Ver lista de cursos', 'Cursos', 1),
    ('CrearCursos', 'Crear nuevos cursos', 'Cursos', 1),
    ('EditarCursos', 'Editar cursos existentes', 'Cursos', 1),
    ('EliminarCursos', 'Eliminar cursos', 'Cursos', 1),
    ('AsignarDocentes', 'Asignar docentes a cursos', 'Cursos', 1),

    -- Permisos de Historial Académico
    ('VerHistorialPropio', 'Ver historial académico propio', 'Historial', 1),
    ('VerHistorialEstudiantes', 'Ver historial de estudiantes', 'Historial', 1),
    ('RegistrarCalificaciones', 'Registrar calificaciones', 'Historial', 1),

    -- Permisos de Bitácora
    ('VerBitacora', 'Ver registros de bitácora', 'Bitacora', 1),
    ('ExportarBitacora', 'Exportar registros de bitácora', 'Bitacora', 1);
END
GO

-- Asignar todos los permisos al rol Administrador
IF NOT EXISTS (SELECT * FROM RolPermisos WHERE RolId = 1)
BEGIN
    INSERT INTO RolPermisos (RolId, PermisoId)
    SELECT 1, PermisoId FROM Permisos;
END
GO

-- Asignar permisos al rol Coordinador
IF NOT EXISTS (SELECT * FROM RolPermisos WHERE RolId = 2)
BEGIN
    INSERT INTO RolPermisos (RolId, PermisoId)
    SELECT 2, PermisoId FROM Permisos
    WHERE Nombre IN ('VerUsuarios', 'VerCursos', 'VerHistorialEstudiantes', 'VerBitacora', 'AsignarDocentes');
END
GO

-- Asignar permisos al rol Docente
IF NOT EXISTS (SELECT * FROM RolPermisos WHERE RolId = 3)
BEGIN
    INSERT INTO RolPermisos (RolId, PermisoId)
    SELECT 3, PermisoId FROM Permisos
    WHERE Nombre IN ('VerCursos', 'VerHistorialEstudiantes', 'RegistrarCalificaciones');
END
GO

-- Asignar permisos al rol Estudiante
IF NOT EXISTS (SELECT * FROM RolPermisos WHERE RolId = 4)
BEGIN
    INSERT INTO RolPermisos (RolId, PermisoId)
    SELECT 4, PermisoId FROM Permisos
    WHERE Nombre IN ('VerHistorialPropio');
END
GO

-- Crear usuario administrador por defecto (contraseña: Admin123!)
-- La contraseña está hasheada con SHA256
IF NOT EXISTS (SELECT * FROM Usuarios WHERE Correo = 'admin@sistema.edu')
BEGIN
    INSERT INTO Usuarios (NombreCompleto, Correo, Contrasena, Identificacion, RolId, Activo)
    VALUES ('Administrador del Sistema', 'admin@sistema.edu',
            '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121', -- Admin123!
            'ADMIN001', 1, 1);
END
GO

-- Crear índices para mejorar rendimiento
CREATE INDEX IX_Usuarios_RolId ON Usuarios(RolId);
CREATE INDEX IX_Inscripciones_EstudianteId ON Inscripciones(EstudianteId);
CREATE INDEX IX_Inscripciones_CursoId ON Inscripciones(CursoId);
CREATE INDEX IX_Calificaciones_EstudianteId ON Calificaciones(EstudianteId);
CREATE INDEX IX_Calificaciones_CursoId ON Calificaciones(CursoId);
CREATE INDEX IX_Bitacoras_UsuarioId ON Bitacoras(UsuarioId);
CREATE INDEX IX_Bitacoras_FechaHora ON Bitacoras(FechaHora);
CREATE INDEX IX_CursoDocentes_CursoId ON CursoDocentes(CursoId);
CREATE INDEX IX_CursoDocentes_DocenteId ON CursoDocentes(DocenteId);
GO

PRINT 'Base de datos SistemaAcademico creada exitosamente';
GO
