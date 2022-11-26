USE [VanTai_TransactionDB]
GO
/****** Object:  Table [dbo].[CostIncurredApplyTranActuals]    Script Date: 12/6/2021 3:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CostIncurredApplyTranActuals](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TransportActualID] [int] NULL,
	[CostIncurredID] [int] NULL,
	[CostName] [nvarchar](255) NULL,
	[CostDate] [datetime] NULL,
	[PartnerID] [int] NULL,
	[FirstPriceOption] [float] NULL,
	[SecondPriceOption] [float] NULL,
	[ThirdPriceOption] [float] NULL,
 CONSTRAINT [PK_CostIncurredInTranActuals] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CostIncurreds]    Script Date: 12/6/2021 3:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CostIncurreds](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CostName] [nvarchar](255) NULL,
	[CostDate] [datetime] NULL,
	[PartnerID] [int] NOT NULL,
	[FirstPriceOption] [float] NULL,
	[SecondPriceOption] [float] NULL,
	[ThirdPriceOption] [float] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[ModifiedBy] [int] NULL,
	[ModifiedDate] [datetime] NULL,
	[IsRemove] [bit] NULL,
 CONSTRAINT [PK_CostIncurreds] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Employees]    Script Date: 12/6/2021 3:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employees](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](255) NULL,
	[DateBorn] [datetime] NULL,
	[Mobile] [nvarchar](50) NULL,
	[OftenAddress] [nvarchar](255) NULL,
	[IDNumber] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [int] NULL,
	[CreateCardDay] [datetime] NULL,
	[CreateCardLocation] [nvarchar](255) NULL,
	[DrivingLicenseNumber] [nvarchar](255) NULL,
	[LicenseNumberDate] [datetime] NULL,
	[LicenseNumberLocation] [nvarchar](255) NULL,
	[LicenseNumberLevel] [nvarchar](50) NULL,
	[InsuranceNumber] [nvarchar](50) NULL,
	[CurrentAddress] [nvarchar](255) NULL,
 CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Folders]    Script Date: 12/6/2021 3:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Folders](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FolderName] [nvarchar](500) NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedBy] [int] NULL,
	[ModifiedDate] [datetime] NULL,
	[Type] [int] NULL,
	[IsRemove] [bit] NULL,
 CONSTRAINT [PK_Folders] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ImageUploads]    Script Date: 12/6/2021 3:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ImageUploads](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ImageLink] [nvarchar](500) NULL,
	[FolderID] [int] NULL,
	[CreatedBy] [int] NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedBy] [int] NULL,
	[ModifiedDate] [datetime] NULL,
	[ImageName] [nvarchar](255) NULL,
	[IsRemove] [bit] NULL,
 CONSTRAINT [PK_ImageUploads] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VehicleJoinEmployees]    Script Date: 12/6/2021 3:28:51 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VehicleJoinEmployees](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[VehicleID] [int] NULL,
	[EmployeeID] [int] NULL,
	[StartDate] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [int] NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [int] NULL,
 CONSTRAINT [PK_VehicleJoinEmployee] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WebModules] DROP CONSTRAINT [FK_WebModules_ContentTypes]
GO
/****** Object:  Table [dbo].[WebModules]    Script Date: 12/6/2021 5:00:41 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WebModules]') AND type in (N'U'))
DROP TABLE [dbo].[WebModules]
GO
/****** Object:  Table [dbo].[WebModules]    Script Date: 12/6/2021 5:00:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WebModules](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Body] [ntext] NULL,
	[Image] [nvarchar](255) NULL,
	[ParentID] [int] NULL,
	[ContentTypeID] [varchar](50) NULL,
	[URL] [nvarchar](255) NULL,
	[SeoTitle] [nvarchar](255) NULL,
	[MetaTitle] [nvarchar](255) NULL,
	[MetaKeywords] [nvarchar](500) NULL,
	[MetaDescription] [nvarchar](500) NULL,
	[Order] [int] NULL,
	[UID] [nvarchar](255) NULL,
	[IndexView] [nvarchar](255) NULL,
	[IndexLayout] [nvarchar](255) NULL,
	[PublishIndexView] [nvarchar](255) NULL,
	[PublishIndexLayout] [nvarchar](255) NULL,
	[PublishDetailView] [nvarchar](255) NULL,
	[PublishDetailLayout] [nvarchar](255) NULL,
	[Status] [int] NULL,
	[SubQuerys] [nvarchar](255) NULL,
	[CreatedBy] [nvarchar](255) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedBy] [nvarchar](255) NULL,
	[ModifiedDate] [datetime] NULL,
	[ShowOnMenu] [bit] NULL,
	[ShowOnAdmin] [bit] NULL,
	[Culture] [nvarchar](50) NULL,
	[Icon] [nvarchar](255) NULL,
	[CodeColor] [nvarchar](255) NULL,
	[ActiveArticle] [nvarchar](255) NULL,
	[Target] [nvarchar](255) NULL,
	[Img] [nvarchar](255) NULL,
	[ImgActive] [nvarchar](255) NULL,
	[Type] [nvarchar](50) NULL,
 CONSTRAINT [PK_WebModules] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[WebModules] ON 

INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (86, N'Tổng Quan', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/home', NULL, N'tong-quan', NULL, NULL, 1, N'8d891f9225aec5c', NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:50:48.223' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-tongquan.png', N'/Content/themes/admin/img/Vector-tongquanactive.png', NULL)
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (87, N'Kế hoạch chạy xe', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/driveplan', NULL, N'ke-hoach-chay-xe', NULL, NULL, 2, N'8d891f93f54ba5c', NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:52:18.320' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-kehoach.png', N'/Content/themes/admin/img/Vector-kehoachactive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (88, N'Bảng kê', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/Enumeration', NULL, N'bang-ke', NULL, NULL, 3, N'8d891f94b24a66b', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:52:25.857' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-bangke.png', N'/Content/themes/admin/img/Vector-bangkeActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (89, N'Quản lý đối tác', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManagePartner', NULL, N'quan-ly-doi-tac', NULL, NULL, 4, N'8d891f96986ae4d', NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:52:47.623' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-quanly.png', N'/Content/themes/admin/img/Vector-quanlyActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (90, N'Quản lý xe', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageCar', NULL, N'quan-ly-xe', NULL, NULL, 5, N'8d891f973962fdb', NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:53:04.503' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-xe.png', N'/Content/themes/admin/img/Vector-xeActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (91, N'Quản lý địa điểm', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageAddress', NULL, N'quan-ly-dia-diem', NULL, NULL, 6, N'8d891f97f37fe09', NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:11.900' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-diadiem.png', N'/Content/themes/admin/img/Vector-diadiemActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (92, N'Quản lý lộ trình', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageRoute', NULL, N'quan-ly-lo-trinh', NULL, NULL, 7, N'8d891f98ab653a7', NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:29.647' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-lotrinh.png', N'/Content/themes/admin/img/Vector-lotrinhActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (93, N'Quản lý bảng giá', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManagePriceList', NULL, N'quan-ly-bang-gia', NULL, NULL, 8, N'8d891f995594633', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-banggia.png', N'/Content/themes/admin/img/Vector-bangiaActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1086, N'Phân quyền', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/Role', NULL, N'role', NULL, NULL, 22, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-phanquyen.png', N'/Content/themes/admin/img/Vector-phanquyenActive.png', N'Hệ thống')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1087, N'Người dùng', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/User', NULL, N'user', NULL, NULL, 23, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-nguoidung.png', N'/Content/themes/admin/img/Vector-nguoidungActive.png', N'Hệ thống')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1088, N'Quản lý sửa chữa', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageRepair', NULL, N'quan-ly-sua-chua', NULL, NULL, 9, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/QLSuaChua.png', N'/Content/themes/admin/img/QLSuaChuaActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1089, N'Quản lý lái xe chi', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageDriverPay', NULL, N'quan-ly-lai-xe-chi', NULL, NULL, 10, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/LaiXeChi.png', N'/Content/themes/admin/img/LaiXeChiActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1090, N'Danh mục sửa chữa', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageCategoryRepair', NULL, N'quan-ly-danh-muc-sua-chua', NULL, NULL, 11, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/DanhMucSC.png', N'/Content/themes/admin/img/DanhMucSCActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1091, N'Giá dầu', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManagePriceOil', NULL, N'quan-ly-gia-dau', NULL, NULL, 12, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/GiaDau.png', N'/Content/themes/admin/img/GiaDauActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1092, N'Quản lý dầu xe', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageOil', NULL, N'thong-ke-dau', NULL, NULL, 15, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/DauXe.png', N'/Content/themes/admin/img/DauXeActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1093, N'Vé ngày', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageDayTicket', NULL, N'quan-ly-ve-ngay', NULL, NULL, 13, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/VeNgay.png', N'/Content/themes/admin/img/VeNgayActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1094, N'Vé tháng', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageMonthTicket', NULL, N'quan-ly-ve-thang', NULL, NULL, 14, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/VeThang.png', N'/Content/themes/admin/img/VeThangActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1095, N'Bảng lương', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageSalary', NULL, N'thong-ke-bang-luong', NULL, NULL, 16, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/BangLuong.png', N'/Content/themes/admin/img/BangLuongActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1096, N'Chi phí khác', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageOtherCost', NULL, N'thong-ke-chi-phi-khac', NULL, NULL, 17, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/Vector-xe.png', N'/Content/themes/admin/img/Vector-xeActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (1097, N'Chi phí theo nhân sự', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/StatisticForDriver', NULL, N'thong-ke-chi-phi-theo-lai-xe', NULL, NULL, 19, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/ChiPhiTheoNS.png', N'/Content/themes/admin/img/ChiPhiTheoNSActive.png', N'Thống kê')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (2098, N'Gửi xe', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageParkingCost', NULL, N'chi-phi-gui-xe', NULL, NULL, 18, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/ChiPhiTheoNS.png', N'/Content/themes/admin/img/ChiPhiTheoNSActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (2099, N'Chi phí theo xe', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/StatisticForVehicle', NULL, N'thong-ke-chi-phi-theo-xe', NULL, NULL, 20, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/ChiPhiTheoXe.png', N'/Content/themes/admin/img/ChiPhiTheoXeActive.png', N'Thống kê')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (2100, N'Quản lý chi chí phát sinh', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageCostIncurred', NULL, N'chi-phi-phat-sinh', NULL, NULL, 26, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/QLChiPhiPhatSinh.png', N'/Content/themes/admin/img/QLChiPhiPhatSinhActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (2101, N'Quản lý tải trọng', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageVehicleWeight', NULL, N'quan-ly-tai-trong', NULL, NULL, 25, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/QLTaiTrong.png', N'/Content/themes/admin/img/QLTaiTrongActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (2102, N'Quản lý nhân sự', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageEmployee', NULL, N'quan-ly-nhan-su', NULL, NULL, 23, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/QLNhanSu.png', N'/Content/themes/admin/img/QLNhanSuActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (2103, N'Quản lý ghép xe', NULL, NULL, NULL, NULL, N'OnePage', N'/admin/ManageVehicleJoinEmployee', NULL, N'quan-ly-ghep-xe', NULL, NULL, 27, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/QLGhepXe.png', N'/Content/themes/admin/img/QLGhepXeActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (3103, N'Quản lý media', NULL, NULL, NULL, NULL, N'OnePage', N'', NULL, N'quan-ly-media', NULL, NULL, 28, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/QLMedia.png', N'/Content/themes/admin/img/QLMediaActive.png', N'Vận hành')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (3104, N'Hình ảnh tài xế', NULL, NULL, NULL, 3103, N'OnePage', N'/admin/ManageMediaDriver', NULL, N'quan-ly-media-tai-xe', NULL, NULL, 29, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/ChiPhiTheoXe.png', N'/Content/themes/admin/img/ChiPhiTheoXeActive.png', N'Quản trị kinh doanh')
INSERT [dbo].[WebModules] ([ID], [Title], [Description], [Body], [Image], [ParentID], [ContentTypeID], [URL], [SeoTitle], [MetaTitle], [MetaKeywords], [MetaDescription], [Order], [UID], [IndexView], [IndexLayout], [PublishIndexView], [PublishIndexLayout], [PublishDetailView], [PublishDetailLayout], [Status], [SubQuerys], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate], [ShowOnMenu], [ShowOnAdmin], [Culture], [Icon], [CodeColor], [ActiveArticle], [Target], [Img], [ImgActive], [Type]) VALUES (3105, N'Hình ảnh xe', NULL, NULL, NULL, 3103, N'OnePage', N'/admin/ManageMediaVehicle', NULL, N'quan-ly-media-xe', NULL, NULL, 30, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, CAST(N'2020-11-26T10:58:03.427' AS DateTime), 1, 1, N'vi-VN', NULL, NULL, NULL, N'_self', N'/Content/themes/admin/img/ChiPhiTheoXe.png', N'/Content/themes/admin/img/ChiPhiTheoXeActive.png', N'Quản trị kinh doanh')
SET IDENTITY_INSERT [dbo].[WebModules] OFF
GO
ALTER TABLE [dbo].[WebModules]  WITH CHECK ADD  CONSTRAINT [FK_WebModules_ContentTypes] FOREIGN KEY([ContentTypeID])
REFERENCES [dbo].[ContentTypes] ([ID])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[WebModules] CHECK CONSTRAINT [FK_WebModules_ContentTypes]
GO

ALTER TABLE [dbo].[CostIncurredApplyTranActuals]  WITH CHECK ADD  CONSTRAINT [FK_CostIncurredApplyTranActuals_Partners] FOREIGN KEY([PartnerID])
REFERENCES [dbo].[Partners] ([ID])
GO
ALTER TABLE [dbo].[CostIncurredApplyTranActuals] CHECK CONSTRAINT [FK_CostIncurredApplyTranActuals_Partners]
GO
ALTER TABLE [dbo].[CostIncurredApplyTranActuals]  WITH CHECK ADD  CONSTRAINT [FK_CostIncurredInTranActuals_CostIncurreds] FOREIGN KEY([CostIncurredID])
REFERENCES [dbo].[CostIncurreds] ([ID])
GO
ALTER TABLE [dbo].[CostIncurredApplyTranActuals] CHECK CONSTRAINT [FK_CostIncurredInTranActuals_CostIncurreds]
GO
ALTER TABLE [dbo].[CostIncurredApplyTranActuals]  WITH CHECK ADD  CONSTRAINT [FK_CostIncurredInTranActuals_TransportActuals] FOREIGN KEY([TransportActualID])
REFERENCES [dbo].[TransportActuals] ([ID])
GO
ALTER TABLE [dbo].[CostIncurredApplyTranActuals] CHECK CONSTRAINT [FK_CostIncurredInTranActuals_TransportActuals]
GO
ALTER TABLE [dbo].[CostIncurreds]  WITH CHECK ADD  CONSTRAINT [FK_CostIncurreds_Partners] FOREIGN KEY([PartnerID])
REFERENCES [dbo].[Partners] ([ID])
GO
ALTER TABLE [dbo].[CostIncurreds] CHECK CONSTRAINT [FK_CostIncurreds_Partners]
GO
ALTER TABLE [dbo].[ImageUploads]  WITH CHECK ADD  CONSTRAINT [FK_ImageUploads_ImageUploads] FOREIGN KEY([FolderID])
REFERENCES [dbo].[Folders] ([ID])
GO
ALTER TABLE [dbo].[ImageUploads] CHECK CONSTRAINT [FK_ImageUploads_ImageUploads]
GO
ALTER TABLE [dbo].[VehicleJoinEmployees]  WITH CHECK ADD  CONSTRAINT [FK_VehicleJoinEmployee_Employees] FOREIGN KEY([EmployeeID])
REFERENCES [dbo].[Employees] ([ID])
GO
ALTER TABLE [dbo].[VehicleJoinEmployees] CHECK CONSTRAINT [FK_VehicleJoinEmployee_Employees]
GO
ALTER TABLE [dbo].[VehicleJoinEmployees]  WITH CHECK ADD  CONSTRAINT [FK_VehicleJoinEmployee_Vehicles] FOREIGN KEY([VehicleID])
REFERENCES [dbo].[Vehicles] ([ID])
GO
ALTER TABLE [dbo].[VehicleJoinEmployees] CHECK CONSTRAINT [FK_VehicleJoinEmployee_Vehicles]
GO

ALTER TABLE [dbo].[DriverPays]
    ADD CONSTRAINT [FK_DriverPays_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

ALTER TABLE [dbo].[ManageOils]
ADD [AdjustOil] float;

ALTER TABLE [dbo].[ManageOils]
    ADD CONSTRAINT [FK_ManageOils_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

ALTER TABLE [dbo].[ManageSalarys]
    ADD CONSTRAINT [FK_ManageSalarys_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

ALTER TABLE [dbo].[ManageTickets]
    ADD CONSTRAINT [FK_ManageTickets_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

sp_rename '[dbo].[OtherCosts].OtherCosts', '[FirstVehicleOtherCosts]', 'COLUMN';

ALTER TABLE [dbo].[OtherCosts]
ADD [SecondVehicleOtherCosts]  FLOAT (53)     NULL,
    [ThirdVehicleOtherCosts]   FLOAT (53)     NULL,
    [InsuranceCosts]           FLOAT (53)     NULL,
    [ShipperCost]              FLOAT (53)     NULL,
    [OfficeCost]               FLOAT (53)     NULL,
    [FirstEmployeeOtherCosts]  FLOAT (53)     NULL,
    [SecondEmployeeOtherCosts] FLOAT (53)     NULL,
    [ThirdEmployeeOtherCosts]  FLOAT (53)     NULL,
    [EmployeeOfficeCosts] FLOAT (53)     NULL;
GO

ALTER TABLE [dbo].[OtherCosts]
ADD CONSTRAINT [FK__OtherCost__Vehic__64CCF2AE] FOREIGN KEY ([VehicleID]) REFERENCES [dbo].[Vehicles] ([ID]);
GO

ALTER TABLE [dbo].[OtherCosts]
ADD CONSTRAINT [FK_OtherCosts_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

ALTER TABLE [dbo].[ParkingCosts]
ADD Content   NVARCHAR (500)     NULL;
GO

ALTER TABLE [dbo].[ParkingCosts]
    ADD CONSTRAINT [FK__ParkingCo__Drive__6E565CE8] FOREIGN KEY ([VehicleID]) REFERENCES [dbo].[Vehicles] ([ID]);
GO

ALTER TABLE [dbo].[ParkingCosts]
    ADD CONSTRAINT [FK_ParkingCosts_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

ALTER TABLE [dbo].[PriceChangeLogs]
    ADD CONSTRAINT [FK__PriceChan__Prici__607251E5] FOREIGN KEY ([PricingTableID]) REFERENCES [dbo].[PricingTables] ([ID]);
GO

ALTER TABLE [dbo].[PricingTables]
    ADD CONSTRAINT [FK__PricingTa__Desti__51300E55] FOREIGN KEY ([DestinationPartnerID]) REFERENCES [dbo].[Partners] ([ID]);
GO

ALTER TABLE [dbo].[PricingTables]
    ADD CONSTRAINT [FK__PricingTa__Sourc__503BEA1C] FOREIGN KEY ([SourcePartnerID]) REFERENCES [dbo].[Partners] ([ID]);
GO

ALTER TABLE [dbo].[PricingTables]
    ADD CONSTRAINT [FK__PricingTa__Weigh__4E53A1AA] FOREIGN KEY ([WeightID]) REFERENCES [dbo].[VehicleWeights] ([ID]);
GO

ALTER TABLE [dbo].[PricingTables]
    ADD CONSTRAINT [PK__PricingT__3214EC27B27BB998] PRIMARY KEY CLUSTERED ([ID] ASC);
GO

ALTER TABLE [dbo].[RepairVehicles]
    ADD CONSTRAINT [FK_RepairVehicles_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

ALTER TABLE [dbo].[TransportActuals]
ADD  [FirstOtherCostPrice]  FLOAT (53)     NULL,
    [SecondOtherCostPrice] FLOAT (53)     NULL,
    [ThirdOtherCostPrice]  FLOAT (53)     NULL;
GO

ALTER TABLE [dbo].[TransportActuals]
    ADD CONSTRAINT [FK_TransportActuals_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

ALTER TABLE [dbo].[TransportPlans]
    ADD CONSTRAINT [FK_TransportPlans_Employees] FOREIGN KEY ([DriverID]) REFERENCES [dbo].[Employees] ([ID]);
GO

ALTER TABLE [dbo].[UserProfile]
ADD   [PartnerID] INT   ;
GO
  
  ALTER TABLE [dbo].[UserProfile]
    ADD CONSTRAINT [FK_UserProfile_Partners] FOREIGN KEY ([PartnerID]) REFERENCES [dbo].[Partners] ([ID]);
GO

sp_rename '[dbo].[Vehicles].WeightID', '[WeightPayID]', 'COLUMN';

ALTER TABLE [dbo].[Vehicles]
ADD [OilLevel]             FLOAT (53)     NULL,
    [CarBrach]             NVARCHAR (50)  NULL,
    [WeightRegistrationID] INT            NULL,
    [RegistrationDeadline] DATETIME       NULL,
    [BarrelWidth]          FLOAT (53)     NULL,
    [BarrelHeight]         FLOAT (53)     NULL,
    [BarrelLength]         FLOAT (53)     NULL,
    [CarNumber]            FLOAT (53)     NULL,
    [FirstOther]           NVARCHAR (500) NULL,
    [SecondOther]          NVARCHAR (500) NULL,
    [ThridOther]           NVARCHAR (500) NULL;
GO

ALTER TABLE [dbo].[Vehicles]
    ADD CONSTRAINT [FK_Vehicles_VehicleWeights] FOREIGN KEY ([WeightRegistrationID]) REFERENCES [dbo].[VehicleWeights] ([ID]);
GO

ALTER TABLE [dbo].[VehicleWeights]
ADD [CreatedBy]    INT           NULL,
    [CreatedDate]  DATETIME      NULL,
    [ModifiedBy]   INT           NULL,
    [ModifiedDate] DATETIME      NULL;
GO

SET IDENTITY_INSERT [dbo].[Employees] ON

insert into [dbo].[Employees](ID, FullName )
select distinct ID, CarOwerName
from [dbo].[Vehicles];
  
SET IDENTITY_INSERT [dbo].[Employees] OFF


SET IDENTITY_INSERT [dbo].[VehicleJoinEmployees] ON

insert into [dbo].[VehicleJoinEmployees](ID, VehicleID, EmployeeID )
select distinct ID, ID, ID
from [dbo].[Vehicles];

UPDATE [dbo].[VehicleJoinEmployees]
SET StartDate = GETDATE();
  
SET IDENTITY_INSERT [dbo].[VehicleJoinEmployees] OFF

