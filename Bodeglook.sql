USE [master]
GO
/****** Object:  Database [Bodeglook]    Script Date: 4/11/2026 10:27:22 ******/
CREATE DATABASE [Bodeglook]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Bodeglook', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\Bodeglook.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Bodeglook_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\Bodeglook_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [Bodeglook] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Bodeglook].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Bodeglook] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Bodeglook] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Bodeglook] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Bodeglook] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Bodeglook] SET ARITHABORT OFF 
GO
ALTER DATABASE [Bodeglook] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Bodeglook] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Bodeglook] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Bodeglook] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Bodeglook] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Bodeglook] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Bodeglook] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Bodeglook] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Bodeglook] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Bodeglook] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Bodeglook] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Bodeglook] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Bodeglook] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Bodeglook] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Bodeglook] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Bodeglook] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Bodeglook] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Bodeglook] SET RECOVERY FULL 
GO
ALTER DATABASE [Bodeglook] SET  MULTI_USER 
GO
ALTER DATABASE [Bodeglook] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Bodeglook] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Bodeglook] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Bodeglook] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Bodeglook] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'Bodeglook', N'ON'
GO
ALTER DATABASE [Bodeglook] SET QUERY_STORE = OFF
GO
USE [Bodeglook]
GO
/****** Object:  User [alumno]    Script Date: 4/11/2026 10:27:22 ******/
CREATE USER [alumno] FOR LOGIN [alumno] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  UserDefinedFunction [dbo].[DistanciaKm]    Script Date: 4/11/2026 10:27:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ===== Fórmula de Haversine: distancia en km entre dos puntos ===== */
CREATE   FUNCTION [dbo].[DistanciaKm]
(
    @Latitud1 FLOAT,
    @Longitud1 FLOAT,
    @Latitud2 FLOAT,
    @Longitud2 FLOAT
)
RETURNS FLOAT
AS
BEGIN
    DECLARE @DifLatitud FLOAT = RADIANS(@Latitud2 - @Latitud1);
    DECLARE @DifLongitud FLOAT = RADIANS(@Longitud2 - @Longitud1);

    DECLARE @a FLOAT = SIN(@DifLatitud / 2) * SIN(@DifLatitud / 2)
                     + COS(RADIANS(@Latitud1)) * COS(RADIANS(@Latitud2))
                     * SIN(@DifLongitud / 2) * SIN(@DifLongitud / 2);

    -- 6371 es el radio de la Tierra en km. Si falta alguna coordenada devuelve NULL.
    RETURN 6371 * 2 * ATN2(SQRT(@a), SQRT(1 - @a));
END
GO
/****** Object:  Table [dbo].[Barrios]    Script Date: 4/11/2026 10:27:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Barrios](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
	[Latitud] [decimal](9, 6) NULL,
	[Longitud] [decimal](9, 6) NULL,
 CONSTRAINT [PK_Barrios] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Busquedas]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Busquedas](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[Texto] [varchar](100) NOT NULL,
	[Fecha] [datetime] NOT NULL,
 CONSTRAINT [PK_Busquedas] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categorias]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categorias](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Categorias] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CompaniasUsuarios]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CompaniasUsuarios](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[Compania] [varchar](30) NOT NULL,
 CONSTRAINT [PK_CompaniasUsuarios] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CuentasRestaurantes]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CuentasRestaurantes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[Contraseña] [varchar](50) NOT NULL,
	[FechaRegistro] [datetime] NOT NULL,
	[idRestaurante] [int] NOT NULL,
 CONSTRAINT [PK_CuentasRestaurantes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Favoritos]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Favoritos](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idRestaurante] [int] NOT NULL,
	[idUsuario] [int] NOT NULL,
 CONSTRAINT [PK_Favoritos] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Grupos]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Grupos](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
	[Foto] [varchar](100) NULL,
	[FechaCreacion] [datetime] NOT NULL,
 CONSTRAINT [PK_Grupos] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mensajes]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mensajes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Contenido] [varchar](max) NOT NULL,
	[FechaHora] [datetime] NOT NULL,
	[idGrupo] [int] NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idPlan] [int] NULL,
 CONSTRAINT [PK_Mensajes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Planes]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Planes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Titulo] [varchar](50) NOT NULL,
	[FechaCierreVotacion] [datetime] NOT NULL,
	[FechaSalida] [datetime] NOT NULL,
	[Estado] [tinyint] NOT NULL,
	[idGrupo] [int] NOT NULL,
	[idRestauranteElegido] [int] NULL,
	[idUsuarioCreador] [int] NOT NULL,
 CONSTRAINT [PK_Planes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlanesHorarios]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanesHorarios](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idPlan] [int] NOT NULL,
	[idRestaurante] [int] NOT NULL,
	[FechaHoraPropuesta] [datetime] NOT NULL,
 CONSTRAINT [PK_PlanesHorarios] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlanesRestaurantes]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlanesRestaurantes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idPlan] [int] NOT NULL,
	[idRestaurante] [int] NOT NULL,
 CONSTRAINT [PK_PlanesRestaurantes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reseñas]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reseñas](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Puntaje] [int] NOT NULL,
	[Comentario] [varchar](max) NULL,
	[Foto] [varchar](100) NULL,
	[Fecha] [datetime] NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idRestaurante] [int] NOT NULL,
 CONSTRAINT [PK_Reseñas] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Restaurantes]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Restaurantes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[DireccionCalle] [varchar](50) NOT NULL,
	[DireccionNumero] [int] NOT NULL,
	[DireccionCodigoPostal] [int] NOT NULL,
	[Descripcion] [varchar](max) NOT NULL,
	[Telefono] [varchar](50) NOT NULL,
	[HorarioApertura] [datetime] NOT NULL,
	[HorarioCerrado] [datetime] NOT NULL,
	[RangoPrecio] [int] NOT NULL,
	[Foto] [varchar](100) NULL,
	[idBarrio] [int] NOT NULL,
	[idCategoria] [int] NOT NULL,
	[EsPro] [bit] NOT NULL,
	[SitioWeb] [varchar](200) NULL,
	[IdGeoapify] [varchar](300) NULL,
	[Latitud] [decimal](9, 6) NULL,
	[Longitud] [decimal](9, 6) NULL,
	[Origen] [varchar](20) NOT NULL,
 CONSTRAINT [PK_Restaurantes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Restricciones]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Restricciones](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Restricciones] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RestriccionesRestaurantes]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RestriccionesRestaurantes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idRestaurante] [int] NOT NULL,
	[idRestriccion] [int] NOT NULL,
 CONSTRAINT [PK_RestriccionesRestaurantes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RestriccionesUsuarios]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RestriccionesUsuarios](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idRestricciones] [int] NOT NULL,
 CONSTRAINT [PK_RestriccionesUsuarios] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Usuarios]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Usuarios](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[NombreCompleto] [varchar](100) NOT NULL,
	[NombreUsuario] [varchar](50) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[Contraseña] [varchar](50) NOT NULL,
	[FotoPerfil] [varchar](100) NULL,
	[Biografía] [varchar](max) NULL,
	[FechaRegistro] [datetime] NOT NULL,
	[idBarrio] [int] NULL,
	[Frecuencia] [varchar](30) NULL,
	[SoloApto] [bit] NULL,
 CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UsuariosGrupos]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UsuariosGrupos](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idGrupo] [int] NOT NULL,
	[UltimaLectura] [datetime] NULL,
 CONSTRAINT [PK_UsuariosGrupos] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Votos]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Votos](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idPlan] [int] NOT NULL,
	[idRestaurante] [int] NOT NULL,
	[idHorarioVotado] [int] NULL,
 CONSTRAINT [PK_Votos] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ZonasUsuarios]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ZonasUsuarios](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idBarrio] [int] NOT NULL,
 CONSTRAINT [PK_ZonasUsuarios] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Barrios] ON 

INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (1, N'Palermo', CAST(-34.588900 AS Decimal(9, 6)), CAST(-58.430600 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (2, N'Recoleta', CAST(-34.587500 AS Decimal(9, 6)), CAST(-58.397400 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (3, N'Belgrano', CAST(-34.562700 AS Decimal(9, 6)), CAST(-58.458300 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (4, N'San Telmo', CAST(-34.621800 AS Decimal(9, 6)), CAST(-58.373100 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (5, N'Caballito', CAST(-34.618600 AS Decimal(9, 6)), CAST(-58.441700 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (6, N'Villa Crespo', CAST(-34.599300 AS Decimal(9, 6)), CAST(-58.438000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (7, N'Almagro', CAST(-34.609200 AS Decimal(9, 6)), CAST(-58.420600 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (8, N'Núñez', CAST(-34.544500 AS Decimal(9, 6)), CAST(-58.463700 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (9, N'Puerto Madero', CAST(-34.611800 AS Decimal(9, 6)), CAST(-58.363600 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (10, N'Colegiales', CAST(-34.574000 AS Decimal(9, 6)), CAST(-58.449000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (12, N'Agronomía', CAST(-34.595300 AS Decimal(9, 6)), CAST(-58.493000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (13, N'Balvanera', CAST(-34.609600 AS Decimal(9, 6)), CAST(-58.400000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (14, N'Barracas', CAST(-34.645800 AS Decimal(9, 6)), CAST(-58.378900 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (15, N'Boedo', CAST(-34.625200 AS Decimal(9, 6)), CAST(-58.418000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (16, N'Chacarita', CAST(-34.587900 AS Decimal(9, 6)), CAST(-58.454000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (17, N'Coghlan', CAST(-34.560000 AS Decimal(9, 6)), CAST(-58.475000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (18, N'Constitución', CAST(-34.627000 AS Decimal(9, 6)), CAST(-58.383000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (19, N'Flores', CAST(-34.628000 AS Decimal(9, 6)), CAST(-58.463000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (20, N'Floresta', CAST(-34.631000 AS Decimal(9, 6)), CAST(-58.485000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (21, N'La Boca', CAST(-34.634500 AS Decimal(9, 6)), CAST(-58.363100 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (22, N'La Paternal', CAST(-34.600000 AS Decimal(9, 6)), CAST(-58.469000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (23, N'Liniers', CAST(-34.643000 AS Decimal(9, 6)), CAST(-58.520000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (24, N'Mataderos', CAST(-34.660000 AS Decimal(9, 6)), CAST(-58.503000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (25, N'Monte Castro', CAST(-34.615000 AS Decimal(9, 6)), CAST(-58.512000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (26, N'Montserrat', CAST(-34.612000 AS Decimal(9, 6)), CAST(-58.380000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (27, N'Nueva Pompeya', CAST(-34.650000 AS Decimal(9, 6)), CAST(-58.416000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (28, N'Parque Avellaneda', CAST(-34.648000 AS Decimal(9, 6)), CAST(-58.481000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (29, N'Parque Chacabuco', CAST(-34.635000 AS Decimal(9, 6)), CAST(-58.435000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (30, N'Parque Chas', CAST(-34.585000 AS Decimal(9, 6)), CAST(-58.479000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (31, N'Parque Patricios', CAST(-34.636000 AS Decimal(9, 6)), CAST(-58.400000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (32, N'Retiro', CAST(-34.592000 AS Decimal(9, 6)), CAST(-58.375000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (33, N'Saavedra', CAST(-34.552000 AS Decimal(9, 6)), CAST(-58.485000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (34, N'San Cristóbal', CAST(-34.622000 AS Decimal(9, 6)), CAST(-58.402000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (35, N'San Nicolás', CAST(-34.604000 AS Decimal(9, 6)), CAST(-58.380000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (36, N'Vélez Sarsfield', CAST(-34.628000 AS Decimal(9, 6)), CAST(-58.493000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (37, N'Versalles', CAST(-34.629000 AS Decimal(9, 6)), CAST(-58.520000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (38, N'Villa del Parque', CAST(-34.605000 AS Decimal(9, 6)), CAST(-58.496000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (39, N'Villa Devoto', CAST(-34.600000 AS Decimal(9, 6)), CAST(-58.515000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (40, N'Villa General Mitre', CAST(-34.608000 AS Decimal(9, 6)), CAST(-58.468000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (41, N'Villa Lugano', CAST(-34.684000 AS Decimal(9, 6)), CAST(-58.470000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (42, N'Villa Luro', CAST(-34.635000 AS Decimal(9, 6)), CAST(-58.505000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (43, N'Villa Ortúzar', CAST(-34.580000 AS Decimal(9, 6)), CAST(-58.465000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (44, N'Villa Pueyrredón', CAST(-34.583000 AS Decimal(9, 6)), CAST(-58.505000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (45, N'Villa Real', CAST(-34.620000 AS Decimal(9, 6)), CAST(-58.520000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (46, N'Villa Riachuelo', CAST(-34.685000 AS Decimal(9, 6)), CAST(-58.460000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (47, N'Villa Santa Rita', CAST(-34.617000 AS Decimal(9, 6)), CAST(-58.477000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (48, N'Villa Soldati', CAST(-34.670000 AS Decimal(9, 6)), CAST(-58.440000 AS Decimal(9, 6)))
INSERT [dbo].[Barrios] ([ID], [Nombre], [Latitud], [Longitud]) VALUES (49, N'Villa Urquiza', CAST(-34.573000 AS Decimal(9, 6)), CAST(-58.490000 AS Decimal(9, 6)))
SET IDENTITY_INSERT [dbo].[Barrios] OFF
GO
SET IDENTITY_INSERT [dbo].[Busquedas] ON 

INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (2, 1, N'Sushi Bar Nami', CAST(N'2026-11-03T20:12:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (3, 1, N'Parrillas en Palermo', CAST(N'2026-11-03T20:15:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (4, 1, N'vinoteca', CAST(N'2026-11-01T18:40:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (5, 1, N'sin tacc', CAST(N'2026-10-19T17:05:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (6, 2, N'parrilla recoleta', CAST(N'2026-10-30T21:00:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (7, 4, N'sin tacc san telmo', CAST(N'2026-10-18T13:30:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (8, 4, N'Vinos del Sur', CAST(N'2026-10-18T13:32:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (9, 6, N'shawarma', CAST(N'2026-10-31T19:45:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (10, 8, N'vegano', CAST(N'2026-10-28T12:20:00.000' AS DateTime))
INSERT [dbo].[Busquedas] ([ID], [idUsuario], [Texto], [Fecha]) VALUES (11, 8, N'La Huerta de Almagro', CAST(N'2026-10-28T12:22:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[Busquedas] OFF
GO
SET IDENTITY_INSERT [dbo].[Categorias] ON 

INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (1, N'Vinoteca')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (2, N'Bar de vinos')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (3, N'Parrilla')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (4, N'Japonesa')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (5, N'Italiana')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (6, N'Vegana')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (7, N'Bodegón')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (8, N'Brunch')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (9, N'Árabe')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (10, N'Cervecería')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (11, N'Pizzería')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (12, N'Heladería')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (13, N'Restaurante')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (14, N'Bar')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (15, N'Cafetería')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (16, N'Mexicana')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (17, N'Peruana')
INSERT [dbo].[Categorias] ([ID], [Nombre]) VALUES (18, N'Hamburguesería')
SET IDENTITY_INSERT [dbo].[Categorias] OFF
GO
SET IDENTITY_INSERT [dbo].[CompaniasUsuarios] ON 

INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (1, 1, N'Amigos')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (2, 1, N'Pareja')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (3, 2, N'Amigos')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (4, 2, N'Compañeros de trabajo')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (5, 4, N'Amigos')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (6, 4, N'Familia')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (7, 3, N'Amigos')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (8, 5, N'Amigos')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (9, 5, N'Familia')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (10, 6, N'Compañeros de trabajo')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (11, 7, N'Pareja')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (12, 8, N'Amigos')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (13, 10, N'Familia')
INSERT [dbo].[CompaniasUsuarios] ([ID], [idUsuario], [Compania]) VALUES (14, 9, N'Pareja')
SET IDENTITY_INSERT [dbo].[CompaniasUsuarios] OFF
GO
SET IDENTITY_INSERT [dbo].[CuentasRestaurantes] ON 

INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (2, N'hola@vinosdelsur.example.com', N'8fE246ua5AII7mIo6VUvZ6ivUaDOgUyQ', CAST(N'2026-05-10T11:30:00.000' AS DateTime), 2)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (3, N'reservas@asadordonpepe.example.com', N'Ja+LpAIyRndX17POnoB4JrdMzmkbIDyr', CAST(N'2026-05-18T12:15:00.000' AS DateTime), 3)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (4, N'info@sushibarnami.example.com', N'kh5p69oiw9p90lYTSyWzmEkRCKUqHtBc', CAST(N'2026-06-01T09:40:00.000' AS DateTime), 4)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (5, N'hola@trattoriabellini.example.com', N'AjuCLHZ8/znh/rnJs6bMi2IDIhkb+poA', CAST(N'2026-06-12T16:20:00.000' AS DateTime), 5)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (6, N'contacto@huertaalmagro.example.com', N'34IDFHukWwmh13hDTDhg8QtH6fn3Aua4', CAST(N'2026-06-25T10:05:00.000' AS DateTime), 6)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (7, N'elpibe@bodegonelpibe.example.com', N'8P1CXDPqGp9TROcaQl73h+CVKgRk4oML', CAST(N'2026-07-03T13:50:00.000' AS DateTime), 7)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (8, N'hola@cafeparadiso.example.com', N'80GRy4OaaWhEp9eNUU6tHR/5n2lKPHSl', CAST(N'2026-07-15T08:45:00.000' AS DateTime), 8)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (9, N'info@shawarmabeirut.example.com', N'1X0FMc+z4KFwkG1BrZRtcpC8h6R6cKlg', CAST(N'2026-07-28T17:10:00.000' AS DateTime), 9)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (10, N'birra@cervecerialupulo.example.com', N'jr7er/PI+1rgL6TXalbcKvnDMEqBOk/9', CAST(N'2026-08-09T18:30:00.000' AS DateTime), 10)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (11, N'hola@lafugazza.example.com', N'8qtS7ymCvkfSKfj8+Q8csHGc37BWchYU', CAST(N'2026-08-20T12:00:00.000' AS DateTime), 11)
INSERT [dbo].[CuentasRestaurantes] ([ID], [Email], [Contraseña], [FechaRegistro], [idRestaurante]) VALUES (12, N'info@nievepura.example.com', N'NVLUXzp/ewsm2X7ef0I0Uxf4YGzqK/Nq', CAST(N'2026-09-02T15:30:00.000' AS DateTime), 12)
SET IDENTITY_INSERT [dbo].[CuentasRestaurantes] OFF
GO
SET IDENTITY_INSERT [dbo].[Favoritos] ON 

INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (3, 2, 4)
INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (4, 3, 2)
INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (5, 4, 1)
INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (6, 5, 1)
INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (7, 6, 8)
INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (8, 8, 1)
INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (9, 8, 4)
INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (10, 9, 6)
INSERT [dbo].[Favoritos] ([ID], [idRestaurante], [idUsuario]) VALUES (11, 10, 3)
SET IDENTITY_INSERT [dbo].[Favoritos] OFF
GO
SET IDENTITY_INSERT [dbo].[Grupos] ON 

INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (1, N'Grupo Colegio', NULL, CAST(N'2026-02-10T18:00:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (2, N'Amigas de la facu', NULL, CAST(N'2026-03-05T20:30:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (3, N'After office', NULL, CAST(N'2026-04-18T19:00:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (4, N'Los del vino', NULL, CAST(N'2026-05-02T21:10:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (5, N'Club del sushi', NULL, CAST(N'2026-05-20T22:00:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (6, N'Brunch de domingo', NULL, CAST(N'2026-06-07T10:30:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (7, N'Familia Ortega', NULL, CAST(N'2026-06-15T13:00:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (8, N'Fútbol de los martes', NULL, CAST(N'2026-07-01T23:00:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (9, N'Viaje a Mendoza', NULL, CAST(N'2026-08-12T17:45:00.000' AS DateTime))
INSERT [dbo].[Grupos] ([ID], [Nombre], [Foto], [FechaCreacion]) VALUES (10, N'Cumple de Fran', NULL, CAST(N'2026-09-01T16:20:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[Grupos] OFF
GO
SET IDENTITY_INSERT [dbo].[Mensajes] ON 

INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (1, N'¿Arrancamos el plan para el cumple de Sofi? Les mando la encuesta.', CAST(N'2026-10-20T18:30:00.000' AS DateTime), 1, 1, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (2, N'Cena de cumpleaños de Sofi', CAST(N'2026-10-20T18:31:00.000' AS DateTime), 1, 1, 1)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (3, N'¡Dale! Yo voté por la bodega, tiene opciones sin TACC.', CAST(N'2026-10-20T18:36:00.000' AS DateTime), 1, 4, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (4, N'Votamos todos y gana la bodega. ¡Confirmá, Cami!', CAST(N'2026-10-20T18:42:00.000' AS DateTime), 1, 2, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (5, N'Yo voté Vinos del Sur, pero me sumo igual.', CAST(N'2026-10-20T18:45:00.000' AS DateTime), 1, 3, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (6, N'Armé el plan para el asado de fin de año, voten.', CAST(N'2026-11-02T19:00:00.000' AS DateTime), 1, 2, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (7, N'Asado de fin de año', CAST(N'2026-11-02T19:01:00.000' AS DateTime), 1, 2, 2)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (8, N'Jueves de sushi, ¿quién viene?', CAST(N'2026-11-03T12:10:00.000' AS DateTime), 5, 3, 3)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (9, N'Yo voy, pero a las 20:30 me queda mejor.', CAST(N'2026-11-03T12:30:00.000' AS DateTime), 5, 5, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (10, N'Brunch de cumple de Martina', CAST(N'2026-09-20T10:00:00.000' AS DateTime), 2, 5, 4)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (11, N'¡Gracias por venir, chicas! El brunch estuvo increíble.', CAST(N'2026-09-27T15:00:00.000' AS DateTime), 2, 5, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (12, N'After del viernes', CAST(N'2026-10-06T17:00:00.000' AS DateTime), 3, 6, 5)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (13, N'Hoy after, ¿quién se suma?', CAST(N'2026-10-09T17:30:00.000' AS DateTime), 3, 6, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (14, N'Degustación en Cañuelas', CAST(N'2026-08-20T21:00:00.000' AS DateTime), 4, 2, 6)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (15, N'Cena del cumple de Fran', CAST(N'2026-09-10T18:00:00.000' AS DateTime), 10, 4, 7)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (16, N'Brunch dominguero', CAST(N'2026-11-04T09:00:00.000' AS DateTime), 6, 9, 8)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (17, N'Tercer tiempo', CAST(N'2026-10-24T23:00:00.000' AS DateTime), 8, 10, 9)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (18, N'Ganó la cervecería, nos vemos ahí.', CAST(N'2026-10-26T18:05:00.000' AS DateTime), 8, 10, NULL)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (19, N'Previa del viaje a Mendoza', CAST(N'2026-11-03T21:00:00.000' AS DateTime), 9, 9, 10)
INSERT [dbo].[Mensajes] ([ID], [Contenido], [FechaHora], [idGrupo], [idUsuario], [idPlan]) VALUES (20, N'¿Almorzamos el domingo en lo de la abuela?', CAST(N'2026-11-03T21:10:00.000' AS DateTime), 7, 4, NULL)
SET IDENTITY_INSERT [dbo].[Mensajes] OFF
GO
SET IDENTITY_INSERT [dbo].[Planes] ON 

INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (1, N'Cena de cumpleaños de Sofi', CAST(N'2026-10-23T20:00:00.000' AS DateTime), CAST(N'2026-10-24T21:00:00.000' AS DateTime), 0, 1, NULL, 1)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (2, N'Asado de fin de año', CAST(N'2026-12-10T20:00:00.000' AS DateTime), CAST(N'2026-12-12T21:00:00.000' AS DateTime), 0, 1, NULL, 2)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (3, N'Sushi de los jueves', CAST(N'2026-11-11T18:00:00.000' AS DateTime), CAST(N'2026-11-12T21:00:00.000' AS DateTime), 0, 5, NULL, 3)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (4, N'Brunch de cumple de Martina', CAST(N'2026-09-26T20:00:00.000' AS DateTime), CAST(N'2026-09-27T11:30:00.000' AS DateTime), 1, 2, 8, 5)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (5, N'After del viernes', CAST(N'2026-10-08T18:00:00.000' AS DateTime), CAST(N'2026-10-09T20:00:00.000' AS DateTime), 1, 3, 10, 6)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (6, N'Degustación en Cañuelas', CAST(N'2026-08-27T20:00:00.000' AS DateTime), CAST(N'2026-08-29T13:00:00.000' AS DateTime), 0, 4, NULL, 2)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (7, N'Cena del cumple de Fran', CAST(N'2026-09-17T20:00:00.000' AS DateTime), CAST(N'2026-09-19T21:00:00.000' AS DateTime), 1, 10, 7, 4)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (8, N'Brunch dominguero', CAST(N'2026-11-13T20:00:00.000' AS DateTime), CAST(N'2026-11-15T11:00:00.000' AS DateTime), 0, 6, NULL, 9)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (9, N'Tercer tiempo', CAST(N'2026-10-26T18:00:00.000' AS DateTime), CAST(N'2026-10-27T21:30:00.000' AS DateTime), 1, 8, 10, 10)
INSERT [dbo].[Planes] ([ID], [Titulo], [FechaCierreVotacion], [FechaSalida], [Estado], [idGrupo], [idRestauranteElegido], [idUsuarioCreador]) VALUES (10, N'Previa del viaje a Mendoza', CAST(N'2026-11-18T20:00:00.000' AS DateTime), CAST(N'2026-11-20T20:30:00.000' AS DateTime), 0, 9, NULL, 9)
SET IDENTITY_INSERT [dbo].[Planes] OFF
GO
SET IDENTITY_INSERT [dbo].[PlanesHorarios] ON 

INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (4, 1, 2, CAST(N'2026-10-24T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (5, 1, 2, CAST(N'2026-10-24T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (6, 1, 2, CAST(N'2026-10-24T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (7, 1, 3, CAST(N'2026-10-24T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (8, 1, 3, CAST(N'2026-10-24T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (9, 1, 3, CAST(N'2026-10-24T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (10, 2, 3, CAST(N'2026-12-12T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (11, 2, 3, CAST(N'2026-12-12T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (12, 2, 3, CAST(N'2026-12-12T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (13, 2, 7, CAST(N'2026-12-12T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (14, 2, 7, CAST(N'2026-12-12T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (15, 2, 7, CAST(N'2026-12-12T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (16, 2, 10, CAST(N'2026-12-12T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (17, 2, 10, CAST(N'2026-12-12T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (18, 2, 10, CAST(N'2026-12-12T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (19, 3, 4, CAST(N'2026-11-12T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (20, 3, 4, CAST(N'2026-11-12T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (21, 3, 4, CAST(N'2026-11-12T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (22, 3, 5, CAST(N'2026-11-12T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (23, 3, 5, CAST(N'2026-11-12T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (24, 3, 5, CAST(N'2026-11-12T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (25, 3, 6, CAST(N'2026-11-12T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (26, 3, 6, CAST(N'2026-11-12T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (27, 3, 6, CAST(N'2026-11-12T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (28, 4, 5, CAST(N'2026-09-27T11:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (29, 4, 5, CAST(N'2026-09-27T11:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (30, 4, 5, CAST(N'2026-09-27T12:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (31, 4, 6, CAST(N'2026-09-27T11:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (32, 4, 6, CAST(N'2026-09-27T11:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (33, 4, 6, CAST(N'2026-09-27T12:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (34, 4, 8, CAST(N'2026-09-27T11:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (35, 4, 8, CAST(N'2026-09-27T11:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (36, 4, 8, CAST(N'2026-09-27T12:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (37, 5, 7, CAST(N'2026-10-09T19:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (38, 5, 7, CAST(N'2026-10-09T20:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (39, 5, 7, CAST(N'2026-10-09T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (40, 5, 9, CAST(N'2026-10-09T19:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (41, 5, 9, CAST(N'2026-10-09T20:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (42, 5, 9, CAST(N'2026-10-09T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (43, 5, 10, CAST(N'2026-10-09T19:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (44, 5, 10, CAST(N'2026-10-09T20:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (45, 5, 10, CAST(N'2026-10-09T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (49, 6, 2, CAST(N'2026-08-29T12:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (50, 6, 2, CAST(N'2026-08-29T13:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (51, 6, 2, CAST(N'2026-08-29T13:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (52, 6, 5, CAST(N'2026-08-29T12:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (53, 6, 5, CAST(N'2026-08-29T13:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (54, 6, 5, CAST(N'2026-08-29T13:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (55, 7, 3, CAST(N'2026-09-19T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (56, 7, 3, CAST(N'2026-09-19T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (57, 7, 3, CAST(N'2026-09-19T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (58, 7, 7, CAST(N'2026-09-19T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (59, 7, 7, CAST(N'2026-09-19T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (60, 7, 7, CAST(N'2026-09-19T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (61, 7, 9, CAST(N'2026-09-19T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (62, 7, 9, CAST(N'2026-09-19T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (63, 7, 9, CAST(N'2026-09-19T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (64, 8, 5, CAST(N'2026-11-15T10:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (65, 8, 5, CAST(N'2026-11-15T11:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (66, 8, 5, CAST(N'2026-11-15T11:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (67, 8, 6, CAST(N'2026-11-15T10:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (68, 8, 6, CAST(N'2026-11-15T11:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (69, 8, 6, CAST(N'2026-11-15T11:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (70, 8, 8, CAST(N'2026-11-15T10:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (71, 8, 8, CAST(N'2026-11-15T11:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (72, 8, 8, CAST(N'2026-11-15T11:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (73, 9, 3, CAST(N'2026-10-27T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (74, 9, 3, CAST(N'2026-10-27T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (75, 9, 3, CAST(N'2026-10-27T22:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (76, 9, 7, CAST(N'2026-10-27T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (77, 9, 7, CAST(N'2026-10-27T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (78, 9, 7, CAST(N'2026-10-27T22:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (79, 9, 10, CAST(N'2026-10-27T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (80, 9, 10, CAST(N'2026-10-27T21:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (81, 9, 10, CAST(N'2026-10-27T22:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (85, 10, 2, CAST(N'2026-11-20T20:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (86, 10, 2, CAST(N'2026-11-20T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (87, 10, 2, CAST(N'2026-11-20T21:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (88, 10, 5, CAST(N'2026-11-20T20:00:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (89, 10, 5, CAST(N'2026-11-20T20:30:00.000' AS DateTime))
INSERT [dbo].[PlanesHorarios] ([ID], [idPlan], [idRestaurante], [FechaHoraPropuesta]) VALUES (90, 10, 5, CAST(N'2026-11-20T21:00:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[PlanesHorarios] OFF
GO
SET IDENTITY_INSERT [dbo].[PlanesRestaurantes] ON 

INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (2, 1, 2)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (3, 1, 3)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (4, 2, 3)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (5, 2, 7)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (6, 2, 10)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (7, 3, 4)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (8, 3, 5)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (9, 3, 6)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (12, 4, 5)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (11, 4, 6)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (10, 4, 8)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (15, 5, 7)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (14, 5, 9)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (13, 5, 10)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (17, 6, 2)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (18, 6, 5)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (21, 7, 3)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (19, 7, 7)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (20, 7, 9)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (24, 8, 5)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (23, 8, 6)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (22, 8, 8)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (27, 9, 3)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (26, 9, 7)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (25, 9, 10)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (28, 10, 2)
INSERT [dbo].[PlanesRestaurantes] ([ID], [idPlan], [idRestaurante]) VALUES (30, 10, 5)
SET IDENTITY_INSERT [dbo].[PlanesRestaurantes] OFF
GO
SET IDENTITY_INSERT [dbo].[Reseñas] ON 

INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (4, 5, N'La carta de vinos es enorme y te recomiendan bien.', NULL, CAST(N'2026-09-14T23:10:00.000' AS DateTime), 1, 2)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (5, 5, N'Picada abundante y buena música. Volvemos seguro.', NULL, CAST(N'2026-09-20T22:45:00.000' AS DateTime), 3, 2)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (6, 5, N'Ideal para ir con amigas un jueves.', NULL, CAST(N'2026-09-25T21:30:00.000' AS DateTime), 5, 2)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (7, 5, N'Los vinos naranjos, una sorpresa.', NULL, CAST(N'2026-10-02T23:00:00.000' AS DateTime), 6, 2)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (8, 4, N'Muy lindo, pero un poco ruidoso el sábado.', NULL, CAST(N'2026-10-11T00:15:00.000' AS DateTime), 7, 2)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (9, 4, N'Para no comer carne está la provoleta y las verduras a la leña, bien hechas.', NULL, CAST(N'2026-08-16T22:00:00.000' AS DateTime), 1, 3)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (10, 5, N'El vacío, de los mejores de Recoleta.', NULL, CAST(N'2026-08-30T14:10:00.000' AS DateTime), 2, 3)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (11, 5, N'El omakase vale cada peso. Tienen rolls veggie muy buenos.', NULL, CAST(N'2026-07-10T22:30:00.000' AS DateTime), 1, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (12, 5, N'Pescado fresquísimo.', NULL, CAST(N'2026-07-17T22:00:00.000' AS DateTime), 3, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (13, 5, N'Atención impecable en la barra.', NULL, CAST(N'2026-07-24T21:40:00.000' AS DateTime), 5, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (14, 5, N'El mejor sushi de Belgrano.', NULL, CAST(N'2026-08-07T23:05:00.000' AS DateTime), 6, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (15, 5, N'Me explicaron cada pieza, una experiencia.', NULL, CAST(N'2026-08-14T22:20:00.000' AS DateTime), 7, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (16, 5, N'Tuvieron opciones veganas sin que las pidiera dos veces.', NULL, CAST(N'2026-08-21T21:50:00.000' AS DateTime), 8, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (17, 5, N'Fui por un cumple y nos atendieron re bien.', NULL, CAST(N'2026-08-28T22:35:00.000' AS DateTime), 2, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (18, 5, N'Avisé que era celíaca y usaron salsa sin TACC.', NULL, CAST(N'2026-09-04T21:15:00.000' AS DateTime), 4, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (19, 5, N'Caro pero lo vale.', NULL, CAST(N'2026-09-11T22:50:00.000' AS DateTime), 10, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (20, 4, N'Muy rico, aunque hubo que esperar mesa.', NULL, CAST(N'2026-09-18T23:30:00.000' AS DateTime), 9, 4)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (21, 4, N'El risotto de hongos, buenísimo. El servicio algo lento.', NULL, CAST(N'2026-10-09T23:00:00.000' AS DateTime), 1, 5)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (22, 5, N'El tiramisú es de otro planeta.', NULL, CAST(N'2026-10-12T22:15:00.000' AS DateTime), 5, 5)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (23, 4, N'Pastas frescas de verdad, porciones justas.', NULL, CAST(N'2026-10-16T21:40:00.000' AS DateTime), 6, 5)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (24, 4, N'Todo vegano y nadie lo extrañó. El budín de banana, top.', NULL, CAST(N'2026-09-27T14:00:00.000' AS DateTime), 1, 6)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (25, 5, N'Por fin un lugar donde puedo pedir cualquier cosa.', NULL, CAST(N'2026-10-04T13:30:00.000' AS DateTime), 8, 6)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (26, 4, N'Rico y sano, el local es chiquito.', NULL, CAST(N'2026-10-08T20:45:00.000' AS DateTime), 7, 6)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (27, 4, N'La tortilla babé es un clásico. Para vegetarianos hay pocas opciones.', NULL, CAST(N'2026-09-19T23:30:00.000' AS DateTime), 1, 7)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (28, 4, N'Milanesa enorme, se comparte.', NULL, CAST(N'2026-09-19T23:45:00.000' AS DateTime), 3, 7)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (29, 5, N'El flan con dulce de leche, como el de la abuela.', NULL, CAST(N'2026-09-20T00:10:00.000' AS DateTime), 6, 7)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (30, 5, N'El brunch del cumple de Martina fue un éxito. Hay medialunas sin TACC.', NULL, CAST(N'2026-09-27T13:00:00.000' AS DateTime), 1, 8)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (31, 5, N'Pastelería sin TACC que no parece sin TACC.', NULL, CAST(N'2026-09-27T13:20:00.000' AS DateTime), 4, 8)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (32, 4, N'Café rico, se llena mucho los domingos.', NULL, CAST(N'2026-10-05T12:30:00.000' AS DateTime), 9, 8)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (33, 4, N'Buen tostado de palta, faltan opciones veganas dulces.', NULL, CAST(N'2026-10-12T11:45:00.000' AS DateTime), 8, 8)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (34, 5, N'Lindo para ir a trabajar un rato.', NULL, CAST(N'2026-10-19T10:15:00.000' AS DateTime), 10, 8)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (35, 4, N'El falafel está muy bien condimentado.', NULL, CAST(N'2026-10-10T00:20:00.000' AS DateTime), 1, 9)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (36, 5, N'Shawarma barato y abundante.', NULL, CAST(N'2026-10-14T21:00:00.000' AS DateTime), 6, 9)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (37, 4, N'El hummus, de lo mejor de Villa Crespo.', NULL, CAST(N'2026-10-21T20:30:00.000' AS DateTime), 3, 9)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (38, 3, N'Buena birra, pero para comer veggie solo hay papas.', NULL, CAST(N'2026-10-10T01:00:00.000' AS DateTime), 1, 10)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (39, 4, N'Ideal para el tercer tiempo.', NULL, CAST(N'2026-10-28T00:30:00.000' AS DateTime), 10, 10)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (40, 4, N'La IPA de la casa está muy bien.', NULL, CAST(N'2026-10-28T00:45:00.000' AS DateTime), 3, 10)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (41, 3, N'Las hamburguesas llegaron tibias.', NULL, CAST(N'2026-10-28T01:00:00.000' AS DateTime), 6, 10)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (42, 4, N'La fugazzeta rellena, un clásico. Hay opción vegetariana.', NULL, CAST(N'2026-08-22T22:30:00.000' AS DateTime), 1, 11)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (43, 4, N'Barata y abundante, ideal después del cine.', NULL, CAST(N'2026-08-29T23:10:00.000' AS DateTime), 5, 11)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (44, 5, N'La mejor pizza al molde de Almagro.', NULL, CAST(N'2026-09-05T22:40:00.000' AS DateTime), 6, 11)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (45, 5, N'Tienen sabores veganos y el de pistacho es increíble.', NULL, CAST(N'2026-10-04T17:30:00.000' AS DateTime), 1, 12)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (46, 5, N'Cucurucho sin TACC, ¡por fin!', NULL, CAST(N'2026-10-11T18:15:00.000' AS DateTime), 4, 12)
INSERT [dbo].[Reseñas] ([ID], [Puntaje], [Comentario], [Foto], [Fecha], [idUsuario], [idRestaurante]) VALUES (47, 4, N'Rico, aunque los veganos se terminan rápido.', NULL, CAST(N'2026-10-18T19:00:00.000' AS DateTime), 8, 12)
SET IDENTITY_INSERT [dbo].[Reseñas] OFF
GO
SET IDENTITY_INSERT [dbo].[Restaurantes] ON 

INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (2, N'Vinos del Sur', N'Defensa', 1020, 1065, N'Bar de vinos con más de 200 etiquetas y picadas para compartir.', N'+54 11 4362-1020', CAST(N'1900-01-01T18:00:00.000' AS DateTime), CAST(N'1900-01-01T23:30:00.000' AS DateTime), 3, NULL, 4, 2, 1, N'https://ejemplo.com/vinos-del-sur', NULL, CAST(-34.620500 AS Decimal(9, 6)), CAST(-58.371800 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (3, N'Asador Don Pepe', N'Av. Pueyrredón', 1720, 1119, N'Parrilla de barrio con cortes a la leña y provoleta.', N'+54 11 4823-1720', CAST(N'1900-01-01T12:00:00.000' AS DateTime), CAST(N'1900-01-01T23:30:00.000' AS DateTime), 3, NULL, 2, 3, 0, NULL, NULL, CAST(-34.590600 AS Decimal(9, 6)), CAST(-58.398600 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (4, N'Sushi Bar Nami', N'Juramento', 2380, 1428, N'Barra de sushi omakase y rolls de autor.', N'+54 11 4783-2380', CAST(N'1900-01-01T19:30:00.000' AS DateTime), CAST(N'1900-01-01T23:30:00.000' AS DateTime), 4, NULL, 3, 4, 1, N'https://ejemplo.com/sushi-bar-nami', NULL, CAST(-34.561800 AS Decimal(9, 6)), CAST(-58.456900 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (5, N'Trattoria Bellini', N'Gorriti', 4850, 1414, N'Cocina italiana de trattoria: risottos, pastas frescas y tiramisú.', N'+54 11 4831-4850', CAST(N'1900-01-01T12:00:00.000' AS DateTime), CAST(N'1900-01-01T23:30:00.000' AS DateTime), 3, NULL, 1, 5, 0, NULL, NULL, CAST(-34.590100 AS Decimal(9, 6)), CAST(-58.429200 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (6, N'La Huerta de Almagro', N'Bulnes', 850, 1176, N'Cocina 100% vegana con ingredientes de huerta propia.', N'+54 11 4862-0850', CAST(N'1900-01-01T10:00:00.000' AS DateTime), CAST(N'1900-01-01T22:00:00.000' AS DateTime), 2, NULL, 7, 6, 0, NULL, NULL, CAST(-34.604300 AS Decimal(9, 6)), CAST(-58.418800 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (7, N'Bodegón El Pibe', N'José María Moreno', 350, 1424, N'Bodegón porteño: milanesas, tortilla y flan casero.', N'+54 11 4901-0350', CAST(N'1900-01-01T12:00:00.000' AS DateTime), CAST(N'1900-01-01T23:30:00.000' AS DateTime), 2, NULL, 5, 7, 0, NULL, NULL, CAST(-34.620900 AS Decimal(9, 6)), CAST(-58.434100 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (8, N'Café Paradiso', N'Zapiola', 1500, 1426, N'Brunch todo el día, café de especialidad y pastelería sin TACC.', N'+54 11 4552-1500', CAST(N'1900-01-01T08:30:00.000' AS DateTime), CAST(N'1900-01-01T19:00:00.000' AS DateTime), 2, NULL, 10, 8, 0, NULL, NULL, CAST(-34.573100 AS Decimal(9, 6)), CAST(-58.451200 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (9, N'Shawarma Beirut', N'Scalabrini Ortiz', 600, 1414, N'Shawarma, falafel y hummus de receta libanesa.', N'+54 11 4857-0600', CAST(N'1900-01-01T12:00:00.000' AS DateTime), CAST(N'1900-01-01T23:00:00.000' AS DateTime), 1, NULL, 6, 9, 0, NULL, NULL, CAST(-34.598700 AS Decimal(9, 6)), CAST(-58.436100 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (10, N'Cervecería Lúpulo', N'Honduras', 5700, 1414, N'Cervecería artesanal con hamburguesas y papas con cheddar.', N'+54 11 4776-5700', CAST(N'1900-01-01T17:00:00.000' AS DateTime), CAST(N'1900-01-01T23:59:00.000' AS DateTime), 2, NULL, 1, 10, 0, NULL, NULL, CAST(-34.582900 AS Decimal(9, 6)), CAST(-58.432700 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (11, N'Pizzería La Fugazza', N'Av. Corrientes', 4120, 1195, N'Pizza al molde y fugazzeta rellena, como en los 80.', N'+54 11 4867-4120', CAST(N'1900-01-01T12:00:00.000' AS DateTime), CAST(N'1900-01-01T23:59:00.000' AS DateTime), 1, NULL, 7, 11, 0, NULL, NULL, CAST(-34.603500 AS Decimal(9, 6)), CAST(-58.421000 AS Decimal(9, 6)), N'Manual')
INSERT [dbo].[Restaurantes] ([ID], [Nombre], [DireccionCalle], [DireccionNumero], [DireccionCodigoPostal], [Descripcion], [Telefono], [HorarioApertura], [HorarioCerrado], [RangoPrecio], [Foto], [idBarrio], [idCategoria], [EsPro], [SitioWeb], [IdGeoapify], [Latitud], [Longitud], [Origen]) VALUES (12, N'Heladería Nieve Pura', N'Av. Santa Fe', 1980, 1123, N'Helado artesanal con sabores veganos y sin TACC.', N'+54 11 4811-1980', CAST(N'1900-01-01T11:00:00.000' AS DateTime), CAST(N'1900-01-01T23:30:00.000' AS DateTime), 1, NULL, 2, 12, 0, NULL, NULL, CAST(-34.595200 AS Decimal(9, 6)), CAST(-58.396000 AS Decimal(9, 6)), N'Manual')
SET IDENTITY_INSERT [dbo].[Restaurantes] OFF
GO
SET IDENTITY_INSERT [dbo].[Restricciones] ON 

INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (1, N'Sin TACC')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (2, N'Vegetariano')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (3, N'Vegano')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (4, N'Sin lactosa')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (5, N'Sin frutos secos')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (6, N'Sin mariscos')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (7, N'Kosher')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (8, N'Halal')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (9, N'Sin huevo')
INSERT [dbo].[Restricciones] ([ID], [Nombre]) VALUES (10, N'Sin cerdo')
SET IDENTITY_INSERT [dbo].[Restricciones] OFF
GO
SET IDENTITY_INSERT [dbo].[RestriccionesRestaurantes] ON 

INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (3, 2, 2)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (4, 3, 1)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (5, 4, 4)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (6, 5, 2)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (7, 6, 3)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (8, 6, 2)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (9, 6, 4)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (10, 8, 1)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (11, 8, 2)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (12, 9, 10)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (13, 9, 8)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (14, 7, 5)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (15, 11, 2)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (16, 12, 1)
INSERT [dbo].[RestriccionesRestaurantes] ([ID], [idRestaurante], [idRestriccion]) VALUES (17, 12, 3)
SET IDENTITY_INSERT [dbo].[RestriccionesRestaurantes] OFF
GO
SET IDENTITY_INSERT [dbo].[RestriccionesUsuarios] ON 

INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (1, 4, 1)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (2, 1, 2)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (3, 7, 2)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (4, 8, 3)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (5, 5, 4)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (6, 8, 4)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (7, 3, 5)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (8, 10, 6)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (9, 9, 7)
INSERT [dbo].[RestriccionesUsuarios] ([ID], [idUsuario], [idRestricciones]) VALUES (10, 6, 10)
SET IDENTITY_INSERT [dbo].[RestriccionesUsuarios] OFF
GO
SET IDENTITY_INSERT [dbo].[Usuarios] ON 

INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (1, N'Camila Fernández', N'camilaf', N'camila.fernandez@example.com', N'afVs05isWWjCCM/c/KDURJtNbRQNV8dG', NULL, N'Vegetariana, fan de las vinotecas y de armar planes con amigos.', CAST(N'2026-02-01T10:00:00.000' AS DateTime), 1, N'Una vez por semana', 0)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (2, N'Julián Pérez', N'julianperez', N'julian.perez@example.com', N'HIGlRbapNOrwQ9jYvbWq2LBLYHmk1565', NULL, N'Malbec y tabla de quesos, siempre.', CAST(N'2026-02-03T19:20:00.000' AS DateTime), 2, N'Varias veces por semana', 0)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (3, N'Fran Molina', N'franmolina', N'fran.molina@example.com', N'V79s/5zbZJ+LQFqh+VC3b7DcF7GpGhWi', NULL, N'Sushi los jueves, asado los domingos.', CAST(N'2026-02-08T12:40:00.000' AS DateTime), 3, N'Cada tanto', 0)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (4, N'Sofi Ortega', N'sofiortega', N'sofi.ortega@example.com', N'eMMkc0QruxdAVIIHPhB8AK9Dld/nIDpA', NULL, N'Celíaca: busco lugares sin TACC de verdad.', CAST(N'2026-02-05T21:15:00.000' AS DateTime), 4, N'Una vez por semana', 1)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (5, N'Martina Giménez', N'martinag', N'martina.gimenez@example.com', N'2sETeUozBmzWIKdIvpchs0PrNS2y2qp0', NULL, N'Brunch lover y cazadora de cafés lindos.', CAST(N'2026-02-11T09:05:00.000' AS DateTime), 5, N'Una vez por semana', 0)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (6, N'Nico Ramos', N'nicoramos', N'nico.ramos@example.com', N'EMRFo9wpDb+ZIQUHaqkmMro5Hcy8dHe0', NULL, N'After office y cervezas artesanales.', CAST(N'2026-03-01T18:30:00.000' AS DateTime), 6, N'Varias veces por semana', 0)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (7, N'Lucía Paz', N'lucipaz', N'lucia.paz@example.com', N'2vusOuH+kbXdcnvuUnaxn1SnWP6x3Tva', NULL, N'Pruebo un lugar nuevo cada semana.', CAST(N'2026-03-10T16:00:00.000' AS DateTime), 7, N'Una vez por semana', 0)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (8, N'Lucía Benítez', N'lucib', N'lucia.benitez@example.com', N'7uwGQlW4UfVzI8ezh56914JbNo9h0VmQ', NULL, N'Vegana hace cinco años.', CAST(N'2026-03-15T11:25:00.000' AS DateTime), 8, N'Cada tanto', 1)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (9, N'Delfina Castro', N'delficastro', N'delfina.castro@example.com', N'n5MDJ/pz62LPjMEuJDB/dpWw/y2CCoSf', NULL, N'Organizo el viaje a Mendoza, pregúntenme por bodegas.', CAST(N'2026-04-20T14:45:00.000' AS DateTime), 9, N'Cada tanto', 0)
INSERT [dbo].[Usuarios] ([ID], [NombreCompleto], [NombreUsuario], [Email], [Contraseña], [FotoPerfil], [Biografía], [FechaRegistro], [idBarrio], [Frecuencia], [SoloApto]) VALUES (10, N'Tomás Aguirre', N'tomiaguirre', N'tomas.aguirre@example.com', N'mCbjuXHUTNKvhh6ZXnhncTo4jfFHEhrs', NULL, N'Fútbol los martes y después tercer tiempo.', CAST(N'2026-04-02T22:10:00.000' AS DateTime), 10, N'Una vez por semana', 0)
SET IDENTITY_INSERT [dbo].[Usuarios] OFF
GO
SET IDENTITY_INSERT [dbo].[UsuariosGrupos] ON 

INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (1, 1, 1, CAST(N'2026-10-20T18:42:00.000' AS DateTime))
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (2, 4, 1, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (3, 3, 1, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (4, 2, 1, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (5, 1, 2, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (6, 5, 2, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (7, 7, 2, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (8, 4, 2, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (9, 1, 3, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (10, 6, 3, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (11, 2, 3, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (12, 8, 3, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (13, 1, 4, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (14, 2, 4, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (15, 9, 4, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (16, 1, 5, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (17, 3, 5, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (18, 5, 5, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (19, 4, 6, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (20, 8, 6, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (21, 10, 6, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (22, 9, 6, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (23, 4, 7, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (24, 10, 7, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (25, 3, 8, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (26, 6, 8, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (27, 10, 8, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (28, 2, 9, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (29, 9, 9, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (30, 5, 9, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (31, 3, 10, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (32, 1, 10, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (33, 4, 10, NULL)
INSERT [dbo].[UsuariosGrupos] ([ID], [idUsuario], [idGrupo], [UltimaLectura]) VALUES (34, 6, 10, NULL)
SET IDENTITY_INSERT [dbo].[UsuariosGrupos] OFF
GO
SET IDENTITY_INSERT [dbo].[Votos] ON 

INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (4, 3, 1, 2, 6)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (5, 2, 2, 3, 11)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (6, 1, 2, 3, 12)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (7, 4, 2, 7, 14)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (8, 5, 3, 4, 19)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (9, 3, 3, 4, 20)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (10, 7, 4, 6, 32)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (11, 1, 4, 8, 35)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (12, 5, 4, 8, 35)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (13, 4, 4, 8, 36)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (14, 1, 5, 9, 42)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (15, 6, 5, 10, 44)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (16, 2, 5, 10, 44)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (17, 8, 5, 10, 44)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (20, 1, 6, 2, 51)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (21, 3, 7, 7, 59)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (22, 6, 7, 7, 59)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (23, 1, 7, 7, 59)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (24, 4, 7, 9, 62)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (25, 8, 8, 6, 68)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (26, 4, 8, 8, 71)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (27, 6, 9, 7, 76)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (28, 10, 9, 10, 80)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (29, 3, 9, 10, 80)
INSERT [dbo].[Votos] ([ID], [idUsuario], [idPlan], [idRestaurante], [idHorarioVotado]) VALUES (30, 2, 10, 2, 86)
SET IDENTITY_INSERT [dbo].[Votos] OFF
GO
SET IDENTITY_INSERT [dbo].[ZonasUsuarios] ON 

INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (1, 1, 1)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (2, 1, 2)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (3, 1, 4)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (4, 2, 2)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (5, 2, 1)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (6, 4, 4)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (7, 4, 9)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (8, 3, 3)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (9, 3, 8)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (10, 5, 5)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (11, 5, 7)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (12, 6, 6)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (13, 8, 8)
INSERT [dbo].[ZonasUsuarios] ([ID], [idUsuario], [idBarrio]) VALUES (14, 8, 10)
SET IDENTITY_INSERT [dbo].[ZonasUsuarios] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ_CuentasRestaurantes_Email]    Script Date: 4/11/2026 10:27:23 ******/
ALTER TABLE [dbo].[CuentasRestaurantes] ADD  CONSTRAINT [UQ_CuentasRestaurantes_Email] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ_CuentasRestaurantes_Restaurante]    Script Date: 4/11/2026 10:27:23 ******/
ALTER TABLE [dbo].[CuentasRestaurantes] ADD  CONSTRAINT [UQ_CuentasRestaurantes_Restaurante] UNIQUE NONCLUSTERED 
(
	[idRestaurante] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ_PlanesRestaurantes]    Script Date: 4/11/2026 10:27:23 ******/
ALTER TABLE [dbo].[PlanesRestaurantes] ADD  CONSTRAINT [UQ_PlanesRestaurantes] UNIQUE NONCLUSTERED 
(
	[idPlan] ASC,
	[idRestaurante] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_Restaurantes_IdGeoapify]    Script Date: 4/11/2026 10:27:23 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Restaurantes_IdGeoapify] ON [dbo].[Restaurantes]
(
	[IdGeoapify] ASC
)
WHERE ([IdGeoapify] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ_Votos]    Script Date: 4/11/2026 10:27:23 ******/
ALTER TABLE [dbo].[Votos] ADD  CONSTRAINT [UQ_Votos] UNIQUE NONCLUSTERED 
(
	[idUsuario] ASC,
	[idPlan] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CuentasRestaurantes] ADD  CONSTRAINT [DF_CuentasRestaurantes_FechaRegistro]  DEFAULT (getdate()) FOR [FechaRegistro]
GO
ALTER TABLE [dbo].[Restaurantes] ADD  CONSTRAINT [DF_Restaurantes_EsPro]  DEFAULT ((0)) FOR [EsPro]
GO
ALTER TABLE [dbo].[Restaurantes] ADD  CONSTRAINT [DF_Restaurantes_Origen]  DEFAULT ('Manual') FOR [Origen]
GO
ALTER TABLE [dbo].[Busquedas]  WITH CHECK ADD  CONSTRAINT [FK_Busquedas_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[Busquedas] CHECK CONSTRAINT [FK_Busquedas_Usuarios]
GO
ALTER TABLE [dbo].[CompaniasUsuarios]  WITH CHECK ADD  CONSTRAINT [FK_CompaniasUsuarios_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[CompaniasUsuarios] CHECK CONSTRAINT [FK_CompaniasUsuarios_Usuarios]
GO
ALTER TABLE [dbo].[CuentasRestaurantes]  WITH CHECK ADD  CONSTRAINT [FK_CuentasRestaurantes_Restaurantes] FOREIGN KEY([idRestaurante])
REFERENCES [dbo].[Restaurantes] ([ID])
GO
ALTER TABLE [dbo].[CuentasRestaurantes] CHECK CONSTRAINT [FK_CuentasRestaurantes_Restaurantes]
GO
ALTER TABLE [dbo].[Favoritos]  WITH CHECK ADD  CONSTRAINT [FK_Favoritos_Restaurantes] FOREIGN KEY([idRestaurante])
REFERENCES [dbo].[Restaurantes] ([ID])
GO
ALTER TABLE [dbo].[Favoritos] CHECK CONSTRAINT [FK_Favoritos_Restaurantes]
GO
ALTER TABLE [dbo].[Favoritos]  WITH CHECK ADD  CONSTRAINT [FK_Favoritos_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[Favoritos] CHECK CONSTRAINT [FK_Favoritos_Usuarios]
GO
ALTER TABLE [dbo].[Mensajes]  WITH CHECK ADD  CONSTRAINT [FK_Mensajes_Grupos] FOREIGN KEY([idGrupo])
REFERENCES [dbo].[Grupos] ([ID])
GO
ALTER TABLE [dbo].[Mensajes] CHECK CONSTRAINT [FK_Mensajes_Grupos]
GO
ALTER TABLE [dbo].[Mensajes]  WITH CHECK ADD  CONSTRAINT [FK_Mensajes_Planes] FOREIGN KEY([idPlan])
REFERENCES [dbo].[Planes] ([ID])
GO
ALTER TABLE [dbo].[Mensajes] CHECK CONSTRAINT [FK_Mensajes_Planes]
GO
ALTER TABLE [dbo].[Mensajes]  WITH CHECK ADD  CONSTRAINT [FK_Mensajes_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[Mensajes] CHECK CONSTRAINT [FK_Mensajes_Usuarios]
GO
ALTER TABLE [dbo].[Planes]  WITH CHECK ADD  CONSTRAINT [FK_Planes_Grupos] FOREIGN KEY([idGrupo])
REFERENCES [dbo].[Grupos] ([ID])
GO
ALTER TABLE [dbo].[Planes] CHECK CONSTRAINT [FK_Planes_Grupos]
GO
ALTER TABLE [dbo].[Planes]  WITH CHECK ADD  CONSTRAINT [FK_Planes_Restaurantes] FOREIGN KEY([idRestauranteElegido])
REFERENCES [dbo].[Restaurantes] ([ID])
GO
ALTER TABLE [dbo].[Planes] CHECK CONSTRAINT [FK_Planes_Restaurantes]
GO
ALTER TABLE [dbo].[PlanesHorarios]  WITH CHECK ADD  CONSTRAINT [FK_PlanesHorarios_Planes] FOREIGN KEY([idPlan])
REFERENCES [dbo].[Planes] ([ID])
GO
ALTER TABLE [dbo].[PlanesHorarios] CHECK CONSTRAINT [FK_PlanesHorarios_Planes]
GO
ALTER TABLE [dbo].[PlanesHorarios]  WITH CHECK ADD  CONSTRAINT [FK_PlanesHorarios_Restaurantes] FOREIGN KEY([idRestaurante])
REFERENCES [dbo].[Restaurantes] ([ID])
GO
ALTER TABLE [dbo].[PlanesHorarios] CHECK CONSTRAINT [FK_PlanesHorarios_Restaurantes]
GO
ALTER TABLE [dbo].[PlanesRestaurantes]  WITH CHECK ADD  CONSTRAINT [FK_PlanesRestaurantes_Planes] FOREIGN KEY([idPlan])
REFERENCES [dbo].[Planes] ([ID])
GO
ALTER TABLE [dbo].[PlanesRestaurantes] CHECK CONSTRAINT [FK_PlanesRestaurantes_Planes]
GO
ALTER TABLE [dbo].[PlanesRestaurantes]  WITH CHECK ADD  CONSTRAINT [FK_PlanesRestaurantes_Restaurantes] FOREIGN KEY([idRestaurante])
REFERENCES [dbo].[Restaurantes] ([ID])
GO
ALTER TABLE [dbo].[PlanesRestaurantes] CHECK CONSTRAINT [FK_PlanesRestaurantes_Restaurantes]
GO
ALTER TABLE [dbo].[Reseñas]  WITH CHECK ADD  CONSTRAINT [FK_Reseñas_Restaurantes] FOREIGN KEY([idRestaurante])
REFERENCES [dbo].[Restaurantes] ([ID])
GO
ALTER TABLE [dbo].[Reseñas] CHECK CONSTRAINT [FK_Reseñas_Restaurantes]
GO
ALTER TABLE [dbo].[Reseñas]  WITH CHECK ADD  CONSTRAINT [FK_Reseñas_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[Reseñas] CHECK CONSTRAINT [FK_Reseñas_Usuarios]
GO
ALTER TABLE [dbo].[Restaurantes]  WITH CHECK ADD  CONSTRAINT [FK_Restaurantes_Barrios] FOREIGN KEY([idBarrio])
REFERENCES [dbo].[Barrios] ([ID])
GO
ALTER TABLE [dbo].[Restaurantes] CHECK CONSTRAINT [FK_Restaurantes_Barrios]
GO
ALTER TABLE [dbo].[Restaurantes]  WITH CHECK ADD  CONSTRAINT [FK_Restaurantes_Categorias] FOREIGN KEY([idCategoria])
REFERENCES [dbo].[Categorias] ([ID])
GO
ALTER TABLE [dbo].[Restaurantes] CHECK CONSTRAINT [FK_Restaurantes_Categorias]
GO
ALTER TABLE [dbo].[RestriccionesRestaurantes]  WITH CHECK ADD  CONSTRAINT [FK_RestriccionesRestaurantes_Restaurantes] FOREIGN KEY([idRestaurante])
REFERENCES [dbo].[Restaurantes] ([ID])
GO
ALTER TABLE [dbo].[RestriccionesRestaurantes] CHECK CONSTRAINT [FK_RestriccionesRestaurantes_Restaurantes]
GO
ALTER TABLE [dbo].[RestriccionesRestaurantes]  WITH CHECK ADD  CONSTRAINT [FK_RestriccionesRestaurantes_Restricciones] FOREIGN KEY([idRestriccion])
REFERENCES [dbo].[Restricciones] ([ID])
GO
ALTER TABLE [dbo].[RestriccionesRestaurantes] CHECK CONSTRAINT [FK_RestriccionesRestaurantes_Restricciones]
GO
ALTER TABLE [dbo].[RestriccionesUsuarios]  WITH CHECK ADD  CONSTRAINT [FK_RestriccionesUsuarios_Restricciones] FOREIGN KEY([idRestricciones])
REFERENCES [dbo].[Restricciones] ([ID])
GO
ALTER TABLE [dbo].[RestriccionesUsuarios] CHECK CONSTRAINT [FK_RestriccionesUsuarios_Restricciones]
GO
ALTER TABLE [dbo].[RestriccionesUsuarios]  WITH CHECK ADD  CONSTRAINT [FK_RestriccionesUsuarios_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[RestriccionesUsuarios] CHECK CONSTRAINT [FK_RestriccionesUsuarios_Usuarios]
GO
ALTER TABLE [dbo].[Usuarios]  WITH CHECK ADD  CONSTRAINT [FK_Usuarios_Barrios] FOREIGN KEY([idBarrio])
REFERENCES [dbo].[Barrios] ([ID])
GO
ALTER TABLE [dbo].[Usuarios] CHECK CONSTRAINT [FK_Usuarios_Barrios]
GO
ALTER TABLE [dbo].[UsuariosGrupos]  WITH CHECK ADD  CONSTRAINT [FK_UsuariosGrupos_Grupos] FOREIGN KEY([idGrupo])
REFERENCES [dbo].[Grupos] ([ID])
GO
ALTER TABLE [dbo].[UsuariosGrupos] CHECK CONSTRAINT [FK_UsuariosGrupos_Grupos]
GO
ALTER TABLE [dbo].[UsuariosGrupos]  WITH CHECK ADD  CONSTRAINT [FK_UsuariosGrupos_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[UsuariosGrupos] CHECK CONSTRAINT [FK_UsuariosGrupos_Usuarios]
GO
ALTER TABLE [dbo].[Votos]  WITH CHECK ADD  CONSTRAINT [FK_Votos_Planes] FOREIGN KEY([idPlan])
REFERENCES [dbo].[Planes] ([ID])
GO
ALTER TABLE [dbo].[Votos] CHECK CONSTRAINT [FK_Votos_Planes]
GO
ALTER TABLE [dbo].[Votos]  WITH CHECK ADD  CONSTRAINT [FK_Votos_PlanesHorarios] FOREIGN KEY([idHorarioVotado])
REFERENCES [dbo].[PlanesHorarios] ([ID])
GO
ALTER TABLE [dbo].[Votos] CHECK CONSTRAINT [FK_Votos_PlanesHorarios]
GO
ALTER TABLE [dbo].[Votos]  WITH CHECK ADD  CONSTRAINT [FK_Votos_PlanesRestaurantes] FOREIGN KEY([idPlan], [idRestaurante])
REFERENCES [dbo].[PlanesRestaurantes] ([idPlan], [idRestaurante])
GO
ALTER TABLE [dbo].[Votos] CHECK CONSTRAINT [FK_Votos_PlanesRestaurantes]
GO
ALTER TABLE [dbo].[Votos]  WITH CHECK ADD  CONSTRAINT [FK_Votos_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[Votos] CHECK CONSTRAINT [FK_Votos_Usuarios]
GO
ALTER TABLE [dbo].[ZonasUsuarios]  WITH CHECK ADD  CONSTRAINT [FK_ZonasUsuarios_Barrios] FOREIGN KEY([idBarrio])
REFERENCES [dbo].[Barrios] ([ID])
GO
ALTER TABLE [dbo].[ZonasUsuarios] CHECK CONSTRAINT [FK_ZonasUsuarios_Barrios]
GO
ALTER TABLE [dbo].[ZonasUsuarios]  WITH CHECK ADD  CONSTRAINT [FK_ZonasUsuarios_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuarios] ([ID])
GO
ALTER TABLE [dbo].[ZonasUsuarios] CHECK CONSTRAINT [FK_ZonasUsuarios_Usuarios]
GO
/****** Object:  StoredProcedure [dbo].[ActualizarPerfilUsuario]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ActualizarPerfilUsuario]
    @idUsuario INT,
    @NombreCompleto VARCHAR(100),
    @Biografía VARCHAR(MAX),
    @idBarrio INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Barrios WHERE ID = @idBarrio)
    BEGIN
        UPDATE Usuarios 
        SET NombreCompleto = @NombreCompleto, 
            Biografía = @Biografía, 
            idBarrio = @idBarrio
        WHERE ID = @idUsuario;
        
        SELECT 0 AS Error; -- Todo OK
    END
    ELSE
    BEGIN
        SELECT 1 AS Error; -- El barrio no existe
    END
END
GO
/****** Object:  StoredProcedure [dbo].[AgregarFavorito]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AgregarFavorito]
    @idUsuario     INT,
    @idRestaurante INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Favoritos
                   WHERE idUsuario = @idUsuario AND idRestaurante = @idRestaurante)
        INSERT INTO Favoritos (idRestaurante, idUsuario)
        VALUES (@idRestaurante, @idUsuario);
END
GO
/****** Object:  StoredProcedure [dbo].[AgregarHorarioAPlan]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AgregarHorarioAPlan]
    @idPlan INT,
    @idRestaurante INT,
    @FechaHoraPropuesta DATETIME
AS
BEGIN
    INSERT INTO PlanesHorarios (idPlan, idRestaurante, FechaHoraPropuesta)
    VALUES (@idPlan, @idRestaurante, @FechaHoraPropuesta);
END
GO
/****** Object:  StoredProcedure [dbo].[AgregarRestauranteAPlan]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AgregarRestauranteAPlan]
    @idPlan INT,
    @idRestaurante INT
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM PlanesRestaurantes WHERE idPlan = @idPlan AND idRestaurante = @idRestaurante)
    BEGIN
        INSERT INTO PlanesRestaurantes (idPlan, idRestaurante)
        VALUES (@idPlan, @idRestaurante);
    END
END
GO
/****** Object:  StoredProcedure [dbo].[AgregarUsuarioAGrupo]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AgregarUsuarioAGrupo]
    @idUsuario INT,
    @idGrupo INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Error INT = 0;

    IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE ID = @idUsuario)
    BEGIN
        SET @Error = 1; -- El usuario no existe
    END
    ELSE IF NOT EXISTS (SELECT 1 FROM Grupos WHERE ID = @idGrupo)
    BEGIN
        SET @Error = 2; -- El grupo no existe
    END
    ELSE IF EXISTS (SELECT 1 FROM UsuariosGrupos WHERE idUsuario = @idUsuario AND idGrupo = @idGrupo)
    BEGIN
        SET @Error = 3; -- El usuario ya pertenece a este grupo
    END
    ELSE
    BEGIN
        INSERT INTO UsuariosGrupos (idUsuario, idGrupo)
        VALUES (@idUsuario, @idGrupo);
    END

    SELECT @Error;
END
GO
/****** Object:  StoredProcedure [dbo].[BuscarRestaurantes]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[BuscarRestaurantes]
    @idBarrio INT = NULL,
    @idCategoria INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT R.ID, R.Nombre, R.DireccionCalle, R.DireccionNumero, R.RangoPrecio, R.Foto,
           B.Nombre AS Barrio, C.Nombre AS Categoria,
           ISNULL((SELECT AVG(CAST(Puntaje AS FLOAT)) FROM [Reseñas] WHERE idRestaurante = R.ID), 0) AS PromedioPuntaje
    FROM Restaurantes R
    INNER JOIN Barrios B ON B.ID = R.idBarrio
    INNER JOIN Categorias C ON C.ID = R.idCategoria
    WHERE (@idBarrio IS NULL OR R.idBarrio = @idBarrio)
      AND (@idCategoria IS NULL OR R.idCategoria = @idCategoria)
    ORDER BY PromedioPuntaje DESC, R.Nombre ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[ComprobarEsFavorito]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ComprobarEsFavorito]
    @idUsuario INT,
    @idRestaurante INT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Favoritos WHERE idUsuario = @idUsuario AND idRestaurante = @idRestaurante)
    BEGIN
        SELECT CAST(1 AS BIT) AS EsFavorito; -- True
    END
    ELSE
    BEGIN
        SELECT CAST(0 AS BIT) AS EsFavorito; -- False
    END
END
GO
/****** Object:  StoredProcedure [dbo].[CrearPlan]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CrearPlan]
    @Titulo VARCHAR(50),
    @FechaCierreVotacion DATETIME,
    @FechaSalida DATETIME,
    @idGrupo INT,
    @idUsuarioCreador INT
AS
BEGIN
    INSERT INTO Planes (Titulo, FechaCierreVotacion, FechaSalida, Estado, idGrupo, idRestauranteElegido, idUsuarioCreador)
    VALUES (@Titulo, @FechaCierreVotacion, @FechaSalida, 0, @idGrupo, NULL, @idUsuarioCreador);
    
    SELECT SCOPE_IDENTITY() AS idPlanNuevo;
END
GO
/****** Object:  StoredProcedure [dbo].[CrearResena]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CrearResena]
    @idUsuario     INT,
    @idRestaurante INT,
    @Puntaje       INT,
    @Comentario    VARCHAR(MAX),
    @Foto          VARCHAR(100)
AS
BEGIN
DECLARE @Error INT = 0;

    IF (@Puntaje < 1 OR @Puntaje > 5)
    BEGIN
        SET @Error = 1;  -- EL PUNTAJE TIENE QUE SER DE 1 A 5
    END
    ELSE IF EXISTS (SELECT * FROM [Reseñas] WHERE idUsuario = @idUsuario AND idRestaurante = @idRestaurante)
    BEGIN
        SET @Error = 2;  -- YA ESCRIBIÓ UNA RESEÑA DE ESTE LUGAR
    END
    ELSE
    BEGIN
        INSERT INTO Reseñas (Puntaje, Comentario, Foto, Fecha, idUsuario, idRestaurante)
        VALUES (@Puntaje, @Comentario, @Foto, GETDATE(), @idUsuario, @idRestaurante);
    END

    SELECT @Error;
END
GO
/****** Object:  StoredProcedure [dbo].[InicioSesionUsuario]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[InicioSesionUsuario] 
	@Email varchar(100),
	@Contraseña varchar(100)

AS
BEGIN
	

	SELECT * FROM Usuarios WHERE @Email = Email and @Contraseña = Contraseña;
END
GO
/****** Object:  StoredProcedure [dbo].[ListarBarrios]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ListarBarrios]
	
AS
BEGIN
	
	SELECT ID, Nombre FROM Barrios ORDER BY Nombre ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[ListarCategorias]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ===== Procedimientos nuevos ===== */
CREATE   PROCEDURE [dbo].[ListarCategorias]
AS
BEGIN
    SELECT ID, Nombre FROM Categorias ORDER BY Nombre ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[ListarGruposUsuario]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ListarGruposUsuario]
    @idUsuario INT
AS
BEGIN
    SELECT G.ID, G.Nombre, G.Foto, G.FechaCreacion
    FROM Grupos G
    INNER JOIN UsuariosGrupos UG ON UG.idGrupo = G.ID
    WHERE UG.idUsuario = @idUsuario
    ORDER BY G.Nombre ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[ListarMiembrosGrupo]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ListarMiembrosGrupo]
    @idGrupo INT    
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Error INT = 0;

    IF NOT EXISTS (SELECT 1 FROM Grupos WHERE ID = @idGrupo)
    BEGIN
        SET @Error = 1; -- El grupo especificado no existe
    END
    ELSE
    BEGIN
        SELECT U.ID, U.NombreCompleto, U.NombreUsuario, U.FotoPerfil
        FROM Usuarios U
        INNER JOIN UsuariosGrupos UG ON UG.idUsuario = U.ID
        WHERE UG.idGrupo = @idGrupo
        ORDER BY U.NombreCompleto ASC;
    END
    SELECT @Error;
END
GO
/****** Object:  StoredProcedure [dbo].[ListarResenasRestaurante]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ListarResenasRestaurante]
    @idRestaurante INT
AS
BEGIN
    SELECT R.ID, R.Puntaje, R.Comentario, R.Foto, R.Fecha,
           U.ID AS idUsuario, U.NombreUsuario, U.FotoPerfil
    FROM Reseñas R
    INNER JOIN Usuarios U ON U.ID = R.idUsuario
    WHERE R.idRestaurante = @idRestaurante
    ORDER BY R.Fecha DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[ListarRestriccionesRestaurante]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ListarRestriccionesRestaurante]
    @idRestaurante INT
AS
BEGIN
    SELECT R.*
    FROM RestriccionesRestaurantes RR
    INNER JOIN Restricciones R ON R.ID = RR.idRestriccion
    WHERE RR.idRestaurante = @idRestaurante
    ORDER BY R.Nombre;
END
GO
/****** Object:  StoredProcedure [dbo].[Mostrar4Restaurantes]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Mostrar4Restaurantes]

AS
BEGIN

	SELECT TOP 4 * FROM Restaurantes ORDER BY ID ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[MostrarFavoritos]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[MostrarFavoritos]
	@idUsuario int

AS
BEGIN

	SELECT R.*, B.Nombre, C.Nombre FROM Favoritos F 
	INNER JOIN Restaurantes R ON F.idRestaurante = R.ID 
	INNER JOIN Barrios B ON B.ID = R.idBarrio
    INNER JOIN Categorias C ON C.ID = R.idCategoria
	WHERE F.idUsuario = @idUsuario
	ORDER BY F.ID ASC;
END
GO
/****** Object:  StoredProcedure [dbo].[ObtenerPerfilUsuario]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ObtenerPerfilUsuario]
    @idUsuario INT
AS
BEGIN
    SELECT U.ID, U.NombreCompleto, U.NombreUsuario, U.Email, U.FotoPerfil, U.Biografía, U.FechaRegistro,
           B.Nombre AS Barrio
    FROM Usuarios U
    INNER JOIN Barrios B ON B.ID = U.idBarrio
    WHERE U.ID = @idUsuario;
END
GO
/****** Object:  StoredProcedure [dbo].[ObtenerRestaurante]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ObtenerRestaurante]
    @idRestaurante INT
AS
BEGIN
    SELECT R.ID, R.Nombre, R.DireccionCalle, R.DireccionNumero, R.DireccionCodigoPostal,
           R.Descripcion, R.Telefono, R.HorarioApertura, R.HorarioCerrado,
           R.RangoPrecio, R.Foto,
           B.Nombre AS Barrio,
           C.Nombre AS Categoria,
           (SELECT AVG(CAST(Puntaje AS FLOAT)) FROM [Reseñas]
             WHERE idRestaurante = R.ID) AS Promedio,
           (SELECT COUNT(*) FROM [Reseñas]
             WHERE idRestaurante = R.ID) AS CantidadResenas
    FROM Restaurantes R
    INNER JOIN Barrios B    ON B.ID = R.idBarrio
    INNER JOIN Categorias C ON C.ID = R.idCategoria
    WHERE R.ID = @idRestaurante;
END
GO
/****** Object:  StoredProcedure [dbo].[QuitarFavorito]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[QuitarFavorito]
    @idUsuario     INT,
    @idRestaurante INT
AS
BEGIN
    DELETE FROM Favoritos
    WHERE idUsuario = @idUsuario AND idRestaurante = @idRestaurante;
END
GO
/****** Object:  StoredProcedure [dbo].[RegistrarRestaurante]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* Devuelve 0 si se guardó, 1 si el email ya se usó (en un restaurante o en un usuario),
   2 si el barrio no existe o 3 si la categoría no existe.
   El formulario solo pide nombre, email, contraseña, barrio, categoría y plan:
   los demás datos del local (dirección, teléfono, descripción, horarios) quedan vacíos. */
CREATE   PROCEDURE [dbo].[RegistrarRestaurante]
    @Nombre VARCHAR(100),
    @Email VARCHAR(100),
    @Contraseña VARCHAR(50),
    @idBarrio INT,
    @idCategoria INT,
    @EsPro BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    DECLARE @Error INT = 0;

    IF EXISTS (SELECT 1 FROM CuentasRestaurantes WHERE Email = @Email)
       OR EXISTS (SELECT 1 FROM Usuarios WHERE Email = @Email)
    BEGIN
        SET @Error = 1; -- EL EMAIL YA SE USÓ
    END
    ELSE IF NOT EXISTS (SELECT 1 FROM Barrios WHERE ID = @idBarrio)
    BEGIN
        SET @Error = 2; -- EL BARRIO NO EXISTE
    END
    ELSE IF NOT EXISTS (SELECT 1 FROM Categorias WHERE ID = @idCategoria)
    BEGIN
        SET @Error = 3; -- LA CATEGORÍA NO EXISTE
    END
    ELSE
    BEGIN
        -- El restaurante y su cuenta se guardan juntos: si falla uno, no queda ninguno
        BEGIN TRANSACTION;

        INSERT INTO Restaurantes (Nombre, DireccionCalle, DireccionNumero, DireccionCodigoPostal, Descripcion,
                                  Telefono, HorarioApertura, HorarioCerrado, RangoPrecio, Foto, idBarrio, idCategoria, EsPro)
        VALUES (@Nombre, '', 0, 0, '', '', '19000101', '19000101', 0, NULL, @idBarrio, @idCategoria, @EsPro);

        INSERT INTO CuentasRestaurantes (Email, Contraseña, idRestaurante)
        VALUES (@Email, @Contraseña, SCOPE_IDENTITY());

        COMMIT TRANSACTION;
    END

    SELECT @Error;
END
GO
/****** Object:  StoredProcedure [dbo].[RegistrarUsuario]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RegistrarUsuario]
	@NombreCompleto varchar(100),
    @NombreUsuario varchar(50),
    @Email varchar(100),
    @Contraseña varchar(100),
    @idBarrio int

AS
BEGIN
	
Declare @Error int
    
    IF NOT EXISTS(SELECT * FROM Usuarios WHERE Email = @Email)
    BEGIN
        IF NOT EXISTS(SELECT * FROM Usuarios WHERE NombreUsuario = @NombreUsuario)
        BEGIN
            INSERT INTO Usuarios(NombreCompleto, NombreUsuario, Email, Contraseña, FechaRegistro, idBarrio) 
            VALUES(@NombreCompleto, @NombreUsuario, @Email, @Contraseña, GETDATE(), @idBarrio)
        END
        ELSE
        BEGIN
            SET @Error = 1; -- EL NOMBRE DE USUARIO YA EXISTE
        END
    END
    ELSE
    BEGIN
        SET @Error = 2; -- EL EMAIL YA SE USÓ
    END

SELECT @Error;

END
GO
/****** Object:  StoredProcedure [dbo].[VotarPlanCompleto]    Script Date: 4/11/2026 10:27:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[VotarPlanCompleto]
    @idUsuario INT,
    @idPlan INT,
    @idRestaurante INT,
    @idHorarioDisponibleVotado INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Votos WHERE idUsuario = @idUsuario AND idPlan = @idPlan)
    BEGIN
        UPDATE Votos 
        SET idRestaurante = @idRestaurante,
            idHorarioVotado = @idHorarioDisponibleVotado
        WHERE idUsuario = @idUsuario AND idPlan = @idPlan;
    END
    ELSE
    BEGIN
        INSERT INTO Votos (idUsuario, idPlan, idRestaurante, idHorarioVotado)
        VALUES (@idUsuario, @idPlan, @idRestaurante, @idHorarioDisponibleVotado);
    END
END
GO
USE [master]
GO
ALTER DATABASE [Bodeglook] SET  READ_WRITE 
GO
