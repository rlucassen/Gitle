USE [Gitle]
GO

/****** Object:  Table [dbo].[PlanningResourceIssue]    Script Date: 17-2-2017 15:47:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PlanningResourceIssue](
	[PlanningResource_id] [bigint] NOT NULL,
	[Issue_id] [bigint] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PlanningResourceIssue]  WITH CHECK ADD  CONSTRAINT [FK50195C3A7BAF7756] FOREIGN KEY([PlanningResource_id])
REFERENCES [dbo].[PlanningResource] ([Id])
GO

ALTER TABLE [dbo].[PlanningResourceIssue] CHECK CONSTRAINT [FK50195C3A7BAF7756]
GO

ALTER TABLE [dbo].[PlanningResourceIssue]  WITH CHECK ADD  CONSTRAINT [FK50195C3A91F7CAB3] FOREIGN KEY([Issue_id])
REFERENCES [dbo].[Issue] ([Id])
GO

ALTER TABLE [dbo].[PlanningResourceIssue] CHECK CONSTRAINT [FK50195C3A91F7CAB3]
GO


