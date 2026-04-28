IF DB_ID(N'DotnetRagApiDb') IS NULL
BEGIN
    CREATE DATABASE [DotnetRagApiDb];
END
GO

USE [DotnetRagApiDb];
GO

SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[dbo].[DotnetRagApiLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DotnetRagApiLogs]
    (
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [Usuario] [nvarchar](100) NOT NULL,
        [Rol] [nvarchar](50) NOT NULL,
        [Pregunta] [nvarchar](max) NOT NULL,
        [Respuesta] [nvarchar](max) NULL,
        [Modelo] [nvarchar](100) NULL,
        [TiempoMs] [int] NULL,
        [CostoEstimado] [decimal](18, 6) NULL,
        [ChunksUsadosJson] [nvarchar](max) NULL,
        [Ok] [bit] NOT NULL,
        [ErrorMessage] [nvarchar](max) NULL,
        [FechaAlta] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_DotnetRagApiLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
END
GO

IF OBJECT_ID(N'[DF_DotnetRagApiLogs_FechaAlta]', N'D') IS NULL
BEGIN
    ALTER TABLE [dbo].[DotnetRagApiLogs]
        ADD CONSTRAINT [DF_DotnetRagApiLogs_FechaAlta]
        DEFAULT (sysutcdatetime()) FOR [FechaAlta];
END
GO