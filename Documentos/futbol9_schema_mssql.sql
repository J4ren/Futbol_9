-- Futbol_9 Application Database Schema
-- SQL Server Database for Football/Soccer Tournament Management System

-- Campeonatos table: Stores championship/tournament information
CREATE TABLE Campeonatos (
    CampeonatoId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(255) NOT NULL UNIQUE,
    FechaInicioCampeonato DATETIME2 DEFAULT GETDATE(),
    FechaFinCampeonato DATETIME2,
    InscripcionInicio DATETIME2 DEFAULT GETDATE(),
    InscripcionFin DATETIME2,
    MontoInscripcion DECIMAL(10,2) DEFAULT 100.00,
    PermiteCuotas BIT DEFAULT 1,
    Activo BIT DEFAULT 1
);

-- Equipos table: Stores team information
CREATE TABLE Equipos (
    EquipoId INT IDENTITY(1,1) PRIMARY KEY,
    CampeonatoId INT NOT NULL,
    Nombre NVARCHAR(255) NOT NULL UNIQUE,
    DelegadoNombre NVARCHAR(255) NOT NULL,
    DelegadoDocumento NVARCHAR(255),
    DelegadoTelefono NVARCHAR(50),
    FechaInicioInscripcion DATETIME2 DEFAULT GETDATE(),
    FechaFinInscripcion DATETIME2,
    CONSTRAINT FK_Equipos_Campeonatos FOREIGN KEY (CampeonatoId) REFERENCES Campeonatos(CampeonatoId) ON DELETE CASCADE
);

-- Grupos table: Stores group information for tournaments
CREATE TABLE Grupos (
    GrupoId INT IDENTITY(1,1) PRIMARY KEY,
    CampeonatoId INT NOT NULL,
    Nombre NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Grupos_Campeonatos FOREIGN KEY (CampeonatoId) REFERENCES Campeonatos(CampeonatoId) ON DELETE CASCADE
);

-- EquiposGrupos table: Junction table to link teams to groups
CREATE TABLE EquiposGrupos (
    EquipoGrupoId INT IDENTITY(1,1) PRIMARY KEY,
    GrupoId INT NOT NULL,
    EquipoId INT NOT NULL,
    CONSTRAINT FK_EquiposGrupos_Grupos FOREIGN KEY (GrupoId) REFERENCES Grupos(GrupoId) ON DELETE CASCADE,
    CONSTRAINT FK_EquiposGrupos_Equipos FOREIGN KEY (EquipoId) REFERENCES Equipos(EquipoId) ON DELETE CASCADE,
    CONSTRAINT UK_EquiposGrupos_GrupoEquipo UNIQUE(GrupoId, EquipoId)
);

-- Jugadores table: Stores player information
CREATE TABLE Jugadores (
    JugadorId INT IDENTITY(1,1) PRIMARY KEY,
    EquipoId INT NOT NULL,
    NombreCompleto NVARCHAR(255) NOT NULL,
    Documento NVARCHAR(50) NOT NULL,
    TelefonoMovil NVARCHAR(50) NOT NULL,
    NumeroCamiseta INT NOT NULL,
    CONSTRAINT FK_Jugadores_Equipos FOREIGN KEY (EquipoId) REFERENCES Equipos(EquipoId) ON DELETE CASCADE
);

-- TiposPlanPago table: Enum-like table for payment plan types
CREATE TABLE TiposPlanPago (
    TipoPlanPagoId INT PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(255)
);

-- Insert default payment plan types
INSERT INTO TiposPlanPago (TipoPlanPagoId, Nombre, Descripcion) VALUES
(0, 'PagoUnico', 'Pago único del total'),
(1, 'AnticipoMasSaldo', 'Anticipo más saldo'),
(2, 'TresCuotas', 'Tres cuotas');

-- PlanesPago table: Stores payment plans for teams
CREATE TABLE PlanesPago (
    PlanPagoId INT IDENTITY(1,1) PRIMARY KEY,
    EquipoId INT NOT NULL,
    Tipo INT NOT NULL DEFAULT 0,
    MontoTotal DECIMAL(10,2) NOT NULL DEFAULT 100.00,
    FechaCreacion DATETIME2 DEFAULT GETDATE(),
    LimitePago DATETIME2,
    MontoPagado DECIMAL(10,2) DEFAULT 0.00,
    CONSTRAINT FK_PlanesPago_Equipos FOREIGN KEY (EquipoId) REFERENCES Equipos(EquipoId) ON DELETE CASCADE,
    CONSTRAINT FK_PlanesPago_TiposPlanPago FOREIGN KEY (Tipo) REFERENCES TiposPlanPago(TipoPlanPagoId)
);

-- Cuotas table: Stores individual payment installments
CREATE TABLE Cuotas (
    CuotaId INT IDENTITY(1,1) PRIMARY KEY,
    PlanPagoId INT NOT NULL,
    Numero INT NOT NULL,
    Importe DECIMAL(10,2) NOT NULL,
    FechaVencimiento DATETIME2,
    Pagada BIT DEFAULT 0,
    FechaPago DATETIME2,
    CONSTRAINT FK_Cuotas_PlanesPago FOREIGN KEY (PlanPagoId) REFERENCES PlanesPago(PlanPagoId) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IX_Equipos_Campeonato ON Equipos(CampeonatoId);
CREATE INDEX IX_Jugadores_Equipo ON Jugadores(EquipoId);
CREATE INDEX IX_Grupos_Campeonato ON Grupos(CampeonatoId);
CREATE INDEX IX_EquiposGrupos_Grupo ON EquiposGrupos(GrupoId);
CREATE INDEX IX_EquiposGrupos_Equipo ON EquiposGrupos(EquipoId);
CREATE INDEX IX_Cuotas_PlanPago ON Cuotas(PlanPagoId);
CREATE INDEX IX_Cuotas_Pagada ON Cuotas(Pagada);
CREATE INDEX IX_Equipos_Nombre ON Equipos(Nombre);
CREATE INDEX IX_Jugadores_Documento ON Jugadores(Documento);