/* I just used the scripting option in SSMS, so some of this might be wrong. */

USE [master]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE DATABASE [Modulr]
    CONTAINMENT = NONE
GO

USE [Modulr]
GO

CREATE TABLE [Tester].[Users]
(
    [id]              [int] IDENTITY (1,1) NOT NULL,
    [google_id]       [nvarchar](64)       NOT NULL,
    [username]        [nvarchar](32)       NOT NULL,
    [name]            [nvarchar](32)       NOT NULL,
    [email]           [nvarchar](128)      NOT NULL,
    [tests_remaining] [int]                NOT NULL,
    [tests_timeout]   [datetimeoffset](7)  NOT NULL,
    [role]            [tinyint]            NOT NULL,
    CONSTRAINT [Users_pk] PRIMARY KEY NONCLUSTERED
        (
         [id] ASC
            ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Tester].[Users]
    ADD DEFAULT ((3)) FOR [tests_remaining]
GO

ALTER TABLE [Tester].[Users]
    ADD DEFAULT (sysdatetimeoffset()) FOR [tests_timeout]
GO

ALTER TABLE [Tester].[Users]
    ADD DEFAULT ((0)) FOR [role]
GO

CREATE TABLE [Tester].[Stipulatables]
(
    [id]          [int] IDENTITY (1,1) NOT NULL,
    [name]        [nvarchar](255)      NOT NULL,
    [testers]     [nvarchar](4000)     NOT NULL,
    [required]    [nvarchar](4000)     NULL,
    [included]    [nvarchar](4000)     NULL,
    [description] [nvarchar](2048)     NOT NULL,
    CONSTRAINT [Stipulatables_pk] PRIMARY KEY NONCLUSTERED
        (
         [id] ASC
            ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Tester].[Stipulatables]
    ADD DEFAULT ('[]') FOR [testers]
GO

ALTER TABLE [Tester].[Stipulatables]
    ADD DEFAULT ('[]') FOR [required]
GO

ALTER TABLE [Tester].[Stipulatables]
    ADD DEFAULT ('[]') FOR [included]
GO

ALTER TABLE [Tester].[Stipulatables]
    ADD DEFAULT ('') FOR [description]
GO
