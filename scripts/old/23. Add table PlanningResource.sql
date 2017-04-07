USE [Gitle]
GO

/****** Object:  Table [dbo].[PlanningResource]    Script Date: 17-2-2017 15:47:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PlanningResource](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NULL,
	[IsActive] [bit] NULL,
	[Year] [int] NULL,
	[Week] [int] NULL,
	[Project_id] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PlanningResource]  WITH CHECK ADD  CONSTRAINT [FKD71A8A9DC4985D11] FOREIGN KEY([Project_id])
REFERENCES [dbo].[Project] ([Id])
GO

ALTER TABLE [dbo].[PlanningResource] CHECK CONSTRAINT [FKD71A8A9DC4985D11]
GO


