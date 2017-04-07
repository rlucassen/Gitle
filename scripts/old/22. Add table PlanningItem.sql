USE [Gitle]
GO

/****** Object:  Table [dbo].[PlanningItem]    Script Date: 17-2-2017 15:47:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PlanningItem](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NULL,
	[IsActive] [bit] NULL,
	[Start] [datetime] NULL,
	[End] [datetime] NULL,
	[Resource] [nvarchar](255) NULL,
	[User_id] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PlanningItem]  WITH CHECK ADD  CONSTRAINT [FKDF388E3B2303E129] FOREIGN KEY([User_id])
REFERENCES [dbo].[User] ([Id])
GO

ALTER TABLE [dbo].[PlanningItem] CHECK CONSTRAINT [FKDF388E3B2303E129]
GO


