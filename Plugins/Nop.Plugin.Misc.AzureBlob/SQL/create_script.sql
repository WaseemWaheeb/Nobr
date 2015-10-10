IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[ITP_PictureFile]') AND type in (N'U'))
BEGIN

CREATE TABLE [dbo].[ITP_PictureFile](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PictureId] [int] NOT NULL,
	[FileName] [nvarchar](300) NOT NULL,
	[PictureURL] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ITP_PictureFile] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[ITP_PictureFile]  WITH CHECK ADD  CONSTRAINT [FK_ITP_PictureFile_ITP_PictureFile] FOREIGN KEY([PictureId])
REFERENCES [dbo].[Picture] ([Id])

ALTER TABLE [dbo].[ITP_PictureFile] CHECK CONSTRAINT [FK_ITP_PictureFile_ITP_PictureFile]

/****** Object:  Index [U_ITP_PictureFile]    Script Date: 7/27/2015 6:37:45 PM ******/
CREATE NONCLUSTERED INDEX [U_ITP_PictureFile] ON [dbo].[ITP_PictureFile]
(
	[PictureId] ASC,
	[FileName] ASC
)
INCLUDE ( 	[Id],
	[PictureURL]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


END
