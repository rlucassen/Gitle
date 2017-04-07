CREATE TABLE [dbo].[PlanningComment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NULL,
	[IsActive] [bit] NULL,
	[Slug] [nvarchar](255) NULL,
	[Comment] [nvarchar](max) NULL,
	[User_id] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [PlanningComment_User] FOREIGN KEY([User_id])
REFERENCES [dbo].[User] ([Id])
GO
