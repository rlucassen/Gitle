/****** Object:  Table [dbo].[FilterPreset]    Script Date: 2/19/2015 11:09:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FilterPreset](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NULL,
	[IsActive] [bit] NULL,
	[Name] [nvarchar](255) NULL,
	[FilterString] [nvarchar](255) NULL,
	[User_id] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[FilterPreset]  WITH CHECK ADD  CONSTRAINT [FKEA8C755C2303E129] FOREIGN KEY([User_id])
REFERENCES [dbo].[User] ([Id])
GO

ALTER TABLE [dbo].[FilterPreset] CHECK CONSTRAINT [FKEA8C755C2303E129]
GO


