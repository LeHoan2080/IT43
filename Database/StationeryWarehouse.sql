-- StationeryWarehouse schema and full data snapshot.
-- Account rows are anonymized and disabled; password hashes are random unusable placeholders.
-- Keep this file private. Do not publish it in a public repository.
-- Restore only into a new or empty database.
USE [master];
GO
IF DB_ID(N'stationery-warehouse') IS NULL
    CREATE DATABASE [stationery-warehouse];
GO
USE [stationery-warehouse];
GO
IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    THROW 51000, 'Restore stopped: target database is not empty.', 1;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;
GO

-- Schema generated from the current EF Core model.
CREATE TABLE [app_role] (
    [id] bigint NOT NULL IDENTITY,
    [code] nvarchar(50) NOT NULL,
    [name] nvarchar(100) NOT NULL,
    [is_active] bit NOT NULL,
    CONSTRAINT [PK_app_role] PRIMARY KEY ([id])
);
GO


CREATE TABLE [supplier] (
    [id] bigint NOT NULL IDENTITY,
    [code] nvarchar(50) NOT NULL,
    [name] nvarchar(255) NOT NULL,
    [type] nvarchar(20) NOT NULL,
    [address] nvarchar(500) NULL,
    [contact_person] nvarchar(150) NULL,
    [phone] nvarchar(30) NULL,
    [email] nvarchar(150) NULL,
    [note] nvarchar(1000) NULL,
    [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [PK_supplier] PRIMARY KEY ([id])
);
GO


CREATE TABLE [warehouse] (
    [Id] bigint NOT NULL IDENTITY,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Address] nvarchar(500) NULL,
    [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [PK_warehouse] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [app_user] (
    [id] bigint NOT NULL IDENTITY,
    [username] nvarchar(50) NOT NULL,
    [password_hash] nvarchar(255) NOT NULL,
    [full_name] nvarchar(100) NOT NULL,
    [role_id] bigint NOT NULL,
    [is_active] bit NOT NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [PK_app_user] PRIMARY KEY ([id]),
    CONSTRAINT [FK_app_user_app_role_role_id] FOREIGN KEY ([role_id]) REFERENCES [app_role] ([id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [product] (
    [id] bigint NOT NULL IDENTITY,
    [product_code] nvarchar(50) NOT NULL,
    [barcode] nvarchar(50) NOT NULL,
    [name] nvarchar(200) NOT NULL,
    [product_type] nvarchar(30) NOT NULL,
    [unit] nvarchar(30) NOT NULL,
    [min_stock] int NOT NULL,
    [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
    [note] nvarchar(1000) NULL,
    [isbn] nvarchar(30) NULL,
    [author] nvarchar(150) NULL,
    [publisher_id] bigint NULL,
    [publication_year] int NULL,
    [category] nvarchar(100) NULL,
    [brand] nvarchar(100) NULL,
    [color] nvarchar(50) NULL,
    [specification] nvarchar(500) NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [PK_product] PRIMARY KEY ([id]),
    CONSTRAINT [FK_product_supplier_publisher_id] FOREIGN KEY ([publisher_id]) REFERENCES [supplier] ([id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [location] (
    [Id] bigint NOT NULL IDENTITY,
    [WarehouseId] bigint NOT NULL,
    [ParentId] bigint NULL,
    [Code] nvarchar(50) NOT NULL,
    [Barcode] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [location_type] nvarchar(20) NOT NULL,
    [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    CONSTRAINT [PK_location] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_location_location_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [location] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_location_warehouse_WarehouseId] FOREIGN KEY ([WarehouseId]) REFERENCES [warehouse] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [inbound_receipt] (
    [Id] bigint NOT NULL IDENTITY,
    [receipt_no] nvarchar(30) NOT NULL,
    [supplier_name] nvarchar(150) NULL,
    [warehouse_id] bigint NOT NULL,
    [receipt_date] date NOT NULL,
    [status] nvarchar(20) NOT NULL,
    [note] nvarchar(1000) NULL,
    [created_by] bigint NOT NULL,
    [created_at] datetime2 NOT NULL,
    [updated_at] datetime2 NULL,
    [completed_at] datetime2 NULL,
    [completed_by] bigint NULL,
    CONSTRAINT [PK_inbound_receipt] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_inbound_receipt_app_user_completed_by] FOREIGN KEY ([completed_by]) REFERENCES [app_user] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_inbound_receipt_app_user_created_by] FOREIGN KEY ([created_by]) REFERENCES [app_user] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_inbound_receipt_warehouse_warehouse_id] FOREIGN KEY ([warehouse_id]) REFERENCES [warehouse] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [outbound_issue] (
    [id] bigint NOT NULL IDENTITY,
    [issue_no] nvarchar(30) NOT NULL,
    [warehouse_id] bigint NOT NULL,
    [issue_reason] nvarchar(30) NOT NULL,
    [issue_date] date NOT NULL,
    [status] nvarchar(20) NOT NULL,
    [note] nvarchar(1000) NULL,
    [created_by] bigint NOT NULL,
    [completed_by] bigint NULL,
    [created_at] datetime2 NOT NULL,
    [completed_at] datetime2 NULL,
    CONSTRAINT [PK_outbound_issue] PRIMARY KEY ([id]),
    CONSTRAINT [FK_outbound_issue_app_user_completed_by] FOREIGN KEY ([completed_by]) REFERENCES [app_user] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_outbound_issue_app_user_created_by] FOREIGN KEY ([created_by]) REFERENCES [app_user] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_outbound_issue_warehouse_warehouse_id] FOREIGN KEY ([warehouse_id]) REFERENCES [warehouse] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [stock_transfer] (
    [id] bigint NOT NULL IDENTITY,
    [transfer_no] nvarchar(30) NOT NULL,
    [source_warehouse_id] bigint NOT NULL,
    [destination_warehouse_id] bigint NOT NULL,
    [transfer_date] date NOT NULL,
    [status] nvarchar(20) NOT NULL,
    [note] nvarchar(1000) NULL,
    [created_by] bigint NOT NULL,
    [created_at] datetime2 NOT NULL,
    [completed_at] datetime2 NULL,
    CONSTRAINT [PK_stock_transfer] PRIMARY KEY ([id]),
    CONSTRAINT [FK_stock_transfer_app_user_created_by] FOREIGN KEY ([created_by]) REFERENCES [app_user] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stock_transfer_warehouse_destination_warehouse_id] FOREIGN KEY ([destination_warehouse_id]) REFERENCES [warehouse] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stock_transfer_warehouse_source_warehouse_id] FOREIGN KEY ([source_warehouse_id]) REFERENCES [warehouse] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [stocktake] (
    [id] bigint NOT NULL IDENTITY,
    [stocktake_no] nvarchar(30) NOT NULL,
    [warehouse_id] bigint NOT NULL,
    [stocktake_date] date NOT NULL,
    [status] nvarchar(20) NOT NULL,
    [note] nvarchar(1000) NULL,
    [created_by] bigint NOT NULL,
    [created_at] datetime2 NOT NULL,
    [completed_by] bigint NULL,
    [completed_at] datetime2 NULL,
    CONSTRAINT [PK_stocktake] PRIMARY KEY ([id]),
    CONSTRAINT [FK_stocktake_app_user_completed_by] FOREIGN KEY ([completed_by]) REFERENCES [app_user] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stocktake_app_user_created_by] FOREIGN KEY ([created_by]) REFERENCES [app_user] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stocktake_warehouse_warehouse_id] FOREIGN KEY ([warehouse_id]) REFERENCES [warehouse] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [inventory_balance] (
    [Id] bigint NOT NULL IDENTITY,
    [ProductId] bigint NOT NULL,
    [LocationId] bigint NOT NULL,
    [Quantity] int NOT NULL,
    [updated_at] datetime2 NOT NULL,
    CONSTRAINT [PK_inventory_balance] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_inventory_balance_location_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [location] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_inventory_balance_product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [product] ([id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [stock_movement] (
    [Id] bigint NOT NULL IDENTITY,
    [ProductId] bigint NOT NULL,
    [LocationId] bigint NOT NULL,
    [movement_type] nvarchar(30) NOT NULL,
    [Quantity] int NOT NULL,
    [reference_type] nvarchar(30) NULL,
    [ReferenceId] bigint NULL,
    [reference_no] nvarchar(30) NULL,
    [PerformedBy] bigint NOT NULL,
    [created_at] datetime2 NOT NULL,
    [Note] nvarchar(500) NULL,
    CONSTRAINT [PK_stock_movement] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_stock_movement_app_user_PerformedBy] FOREIGN KEY ([PerformedBy]) REFERENCES [app_user] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stock_movement_location_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [location] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stock_movement_product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [product] ([id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [inbound_line] (
    [Id] bigint NOT NULL IDENTITY,
    [receipt_id] bigint NOT NULL,
    [product_id] bigint NOT NULL,
    [location_id] bigint NOT NULL,
    [expected_qty] int NOT NULL,
    [received_qty] int NOT NULL,
    [putaway_qty] int NOT NULL,
    [note] nvarchar(500) NULL,
    CONSTRAINT [PK_inbound_line] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_inbound_line_inbound_receipt_receipt_id] FOREIGN KEY ([receipt_id]) REFERENCES [inbound_receipt] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_inbound_line_location_location_id] FOREIGN KEY ([location_id]) REFERENCES [location] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_inbound_line_product_product_id] FOREIGN KEY ([product_id]) REFERENCES [product] ([id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [outbound_line] (
    [id] bigint NOT NULL IDENTITY,
    [issue_id] bigint NOT NULL,
    [product_id] bigint NOT NULL,
    [location_id] bigint NOT NULL,
    [requested_qty] int NOT NULL,
    [picked_qty] int NOT NULL,
    [note] nvarchar(500) NULL,
    CONSTRAINT [PK_outbound_line] PRIMARY KEY ([id]),
    CONSTRAINT [FK_outbound_line_location_location_id] FOREIGN KEY ([location_id]) REFERENCES [location] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_outbound_line_outbound_issue_issue_id] FOREIGN KEY ([issue_id]) REFERENCES [outbound_issue] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_outbound_line_product_product_id] FOREIGN KEY ([product_id]) REFERENCES [product] ([id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [stock_transfer_line] (
    [id] bigint NOT NULL IDENTITY,
    [transfer_id] bigint NOT NULL,
    [product_id] bigint NOT NULL,
    [source_location_id] bigint NOT NULL,
    [destination_location_id] bigint NOT NULL,
    [quantity] int NOT NULL,
    CONSTRAINT [PK_stock_transfer_line] PRIMARY KEY ([id]),
    CONSTRAINT [FK_stock_transfer_line_location_destination_location_id] FOREIGN KEY ([destination_location_id]) REFERENCES [location] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stock_transfer_line_location_source_location_id] FOREIGN KEY ([source_location_id]) REFERENCES [location] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stock_transfer_line_product_product_id] FOREIGN KEY ([product_id]) REFERENCES [product] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stock_transfer_line_stock_transfer_transfer_id] FOREIGN KEY ([transfer_id]) REFERENCES [stock_transfer] ([id]) ON DELETE CASCADE
);
GO


CREATE TABLE [stocktake_line] (
    [id] bigint NOT NULL IDENTITY,
    [stocktake_id] bigint NOT NULL,
    [product_id] bigint NOT NULL,
    [location_id] bigint NOT NULL,
    [system_qty] int NOT NULL,
    [counted_qty] int NOT NULL,
    [difference_qty] int NOT NULL,
    [note] nvarchar(500) NULL,
    CONSTRAINT [PK_stocktake_line] PRIMARY KEY ([id]),
    CONSTRAINT [FK_stocktake_line_location_location_id] FOREIGN KEY ([location_id]) REFERENCES [location] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stocktake_line_product_product_id] FOREIGN KEY ([product_id]) REFERENCES [product] ([id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_stocktake_line_stocktake_stocktake_id] FOREIGN KEY ([stocktake_id]) REFERENCES [stocktake] ([id]) ON DELETE CASCADE
);
GO


CREATE UNIQUE INDEX [IX_app_role_code] ON [app_role] ([code]);
GO


CREATE INDEX [IX_app_user_role_id] ON [app_user] ([role_id]);
GO


CREATE UNIQUE INDEX [IX_app_user_username] ON [app_user] ([username]);
GO


CREATE INDEX [IX_inbound_line_location_id] ON [inbound_line] ([location_id]);
GO


CREATE INDEX [IX_inbound_line_product_id_location_id] ON [inbound_line] ([product_id], [location_id]);
GO


CREATE INDEX [IX_inbound_line_receipt_id] ON [inbound_line] ([receipt_id]);
GO


CREATE INDEX [IX_inbound_receipt_completed_by] ON [inbound_receipt] ([completed_by]);
GO


CREATE INDEX [IX_inbound_receipt_created_by] ON [inbound_receipt] ([created_by]);
GO


CREATE UNIQUE INDEX [IX_inbound_receipt_receipt_no] ON [inbound_receipt] ([receipt_no]);
GO


CREATE INDEX [IX_inbound_receipt_warehouse_id] ON [inbound_receipt] ([warehouse_id]);
GO


CREATE INDEX [IX_inventory_balance_LocationId_ProductId] ON [inventory_balance] ([LocationId], [ProductId]);
GO


CREATE UNIQUE INDEX [IX_inventory_balance_ProductId_LocationId] ON [inventory_balance] ([ProductId], [LocationId]);
GO


CREATE UNIQUE INDEX [IX_location_Barcode] ON [location] ([Barcode]);
GO


CREATE INDEX [IX_location_ParentId] ON [location] ([ParentId]);
GO


CREATE INDEX [IX_location_WarehouseId] ON [location] ([WarehouseId]);
GO


CREATE INDEX [IX_outbound_issue_completed_by] ON [outbound_issue] ([completed_by]);
GO


CREATE INDEX [IX_outbound_issue_created_by] ON [outbound_issue] ([created_by]);
GO


CREATE UNIQUE INDEX [IX_outbound_issue_issue_no] ON [outbound_issue] ([issue_no]);
GO


CREATE INDEX [IX_outbound_issue_warehouse_id_issue_date] ON [outbound_issue] ([warehouse_id], [issue_date]);
GO


CREATE INDEX [IX_outbound_line_issue_id] ON [outbound_line] ([issue_id]);
GO


CREATE INDEX [IX_outbound_line_location_id] ON [outbound_line] ([location_id]);
GO


CREATE INDEX [IX_outbound_line_product_id_location_id] ON [outbound_line] ([product_id], [location_id]);
GO


CREATE UNIQUE INDEX [IX_product_barcode] ON [product] ([barcode]);
GO


CREATE UNIQUE INDEX [IX_product_isbn] ON [product] ([isbn]) WHERE [isbn] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_product_product_code] ON [product] ([product_code]);
GO


CREATE INDEX [IX_product_publisher_id] ON [product] ([publisher_id]);
GO


CREATE INDEX [IX_stock_movement_LocationId] ON [stock_movement] ([LocationId]);
GO


CREATE INDEX [IX_stock_movement_PerformedBy] ON [stock_movement] ([PerformedBy]);
GO


CREATE INDEX [IX_stock_movement_ProductId_created_at] ON [stock_movement] ([ProductId], [created_at]);
GO


CREATE INDEX [IX_stock_transfer_created_by] ON [stock_transfer] ([created_by]);
GO


CREATE INDEX [IX_stock_transfer_destination_warehouse_id] ON [stock_transfer] ([destination_warehouse_id]);
GO


CREATE INDEX [IX_stock_transfer_source_warehouse_id_transfer_date] ON [stock_transfer] ([source_warehouse_id], [transfer_date]);
GO


CREATE UNIQUE INDEX [IX_stock_transfer_transfer_no] ON [stock_transfer] ([transfer_no]);
GO


CREATE INDEX [IX_stock_transfer_line_destination_location_id] ON [stock_transfer_line] ([destination_location_id]);
GO


CREATE INDEX [IX_stock_transfer_line_product_id_source_location_id] ON [stock_transfer_line] ([product_id], [source_location_id]);
GO


CREATE INDEX [IX_stock_transfer_line_source_location_id] ON [stock_transfer_line] ([source_location_id]);
GO


CREATE INDEX [IX_stock_transfer_line_transfer_id] ON [stock_transfer_line] ([transfer_id]);
GO


CREATE INDEX [IX_stocktake_completed_by] ON [stocktake] ([completed_by]);
GO


CREATE INDEX [IX_stocktake_created_by] ON [stocktake] ([created_by]);
GO


CREATE UNIQUE INDEX [IX_stocktake_stocktake_no] ON [stocktake] ([stocktake_no]);
GO


CREATE INDEX [IX_stocktake_warehouse_id_stocktake_date] ON [stocktake] ([warehouse_id], [stocktake_date]);
GO


CREATE INDEX [IX_stocktake_line_location_id] ON [stocktake_line] ([location_id]);
GO


CREATE INDEX [IX_stocktake_line_product_id_location_id] ON [stocktake_line] ([product_id], [location_id]);
GO


CREATE INDEX [IX_stocktake_line_stocktake_id] ON [stocktake_line] ([stocktake_id]);
GO


CREATE UNIQUE INDEX [IX_supplier_code] ON [supplier] ([code]);
GO


CREATE UNIQUE INDEX [IX_warehouse_Code] ON [warehouse] ([Code]);
GO




-- Disable constraints while restoring the full snapshot.
ALTER TABLE [dbo].[app_role] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[app_user] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[inbound_line] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[inbound_receipt] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[inventory_balance] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[location] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[outbound_issue] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[outbound_line] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[product] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stock_movement] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stock_transfer] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stock_transfer_line] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stocktake] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stocktake_line] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[supplier] NOCHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[warehouse] NOCHECK CONSTRAINT ALL;
GO

-- Data snapshot.
SET IDENTITY_INSERT [dbo].[app_role] ON;
GO
    INSERT INTO [dbo].[app_role] ([id], [code], [name], [is_active]) VALUES (1, N'ADMIN', N'Quản trị viên', 1);
GO
    INSERT INTO [dbo].[app_role] ([id], [code], [name], [is_active]) VALUES (2, N'WAREHOUSE_MANAGER', N'Quản lý kho', 1);
GO
    INSERT INTO [dbo].[app_role] ([id], [code], [name], [is_active]) VALUES (3, N'WAREHOUSE_OPERATOR', N'Nhân viên kho', 1);
GO
    INSERT INTO [dbo].[app_role] ([id], [code], [name], [is_active]) VALUES (4, N'VIEWER', N'Người xem', 1);
GO
SET IDENTITY_INSERT [dbo].[app_role] OFF;
GO
-- dbo.app_role: 4 rows
SET IDENTITY_INSERT [dbo].[app_user] ON;
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (1, N'demo.user00001', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 00001', 3, 0, CONVERT(datetime2(7), N'2026-10-01T15:26:32.4328430', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (2, N'demo.user00002', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 00002', 4, 0, CONVERT(datetime2(7), N'2026-10-01T15:30:47.4714700', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10002, N'demo.user10002', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10002', 1, 0, CONVERT(datetime2(7), N'2026-10-05T20:40:49.7676860', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10003, N'demo.user10003', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10003', 2, 0, CONVERT(datetime2(7), N'2026-10-05T20:40:49.8224810', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10004, N'demo.user10004', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10004', 3, 0, CONVERT(datetime2(7), N'2026-10-05T20:40:49.8592550', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10005, N'demo.user10005', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10005', 4, 0, CONVERT(datetime2(7), N'2026-10-05T20:40:49.8963230', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10006, N'demo.user10006', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10006', 2, 0, CONVERT(datetime2(7), N'2026-02-02T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10007, N'demo.user10007', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10007', 3, 0, CONVERT(datetime2(7), N'2026-02-03T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10008, N'demo.user10008', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10008', 4, 0, CONVERT(datetime2(7), N'2026-02-04T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10009, N'demo.user10009', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10009', 3, 0, CONVERT(datetime2(7), N'2026-02-05T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10010, N'demo.user10010', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10010', 2, 0, CONVERT(datetime2(7), N'2026-02-06T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10011, N'demo.user10011', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10011', 4, 0, CONVERT(datetime2(7), N'2026-02-07T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10012, N'demo.user10012', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10012', 2, 0, CONVERT(datetime2(7), N'2026-02-08T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10013, N'demo.user10013', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10013', 3, 0, CONVERT(datetime2(7), N'2026-02-09T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10014, N'demo.user10014', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10014', 4, 0, CONVERT(datetime2(7), N'2026-02-10T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10015, N'demo.user10015', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10015', 3, 0, CONVERT(datetime2(7), N'2026-02-11T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10016, N'demo.user10016', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10016', 2, 0, CONVERT(datetime2(7), N'2026-02-12T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[app_user] ([id], [username], [password_hash], [full_name], [role_id], [is_active], [created_at], [updated_at]) VALUES (10017, N'demo.user10017', N'AQIAAACghgEAEAAAAO9vlPw6SoFHwc8GGIFBRQPzu73OUp+luVH45Bwb6SvN6JOmyBZctz6o1HpjmdvoVg==', N'Demo User 10017', 4, 0, CONVERT(datetime2(7), N'2026-02-13T00:00:00.0000000', 126), NULL);
GO
SET IDENTITY_INSERT [dbo].[app_user] OFF;
GO
-- dbo.app_user: 18 rows
SET IDENTITY_INSERT [dbo].[inbound_line] ON;
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (1, 1, 1, 3, 100, 0, N'Nhập sách Đắc Nhân Tâm', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (2, 1, 2, 4, 50, 0, N'Nhập sách Nhà Giả Kim', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (3, 1, 3, 6, 200, 0, N'Nhập bút bi', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (4, 2, 1, 5, 100, 80, N'Đã nhận 80 cuốn', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (5, 2, 4, 6, 150, 120, N'Đã nhận 120 quyển', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (6, 2, 5, 7, 100, 100, N'Đã nhận đủ', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (7, 3, 1, 3, 50, 0, N'Phiếu đã hủy', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10002, 10002, 6, 14, 10, 0, N'Dòng nhập mẫu', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10003, 10003, 7, 16, 11, 4, N'Dòng nhập mẫu', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10004, 10004, 8, 18, 12, 10, N'Dòng nhập mẫu', 10);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10005, 10005, 9, 20, 13, 0, N'Dòng nhập mẫu', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10006, 10006, 10, 22, 14, 0, N'Dòng nhập mẫu', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10007, 10007, 11, 24, 15, 6, N'Dòng nhập mẫu', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10008, 10008, 12, 26, 16, 14, N'Dòng nhập mẫu', 14);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10009, 10009, 13, 28, 17, 0, N'Dòng nhập mẫu', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10010, 10010, 14, 30, 18, 0, N'Dòng nhập mẫu', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10011, 10011, 15, 32, 19, 8, N'Dòng nhập mẫu', 0);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10012, 10012, 16, 34, 20, 18, N'Dòng nhập mẫu', 18);
GO
    INSERT INTO [dbo].[inbound_line] ([Id], [receipt_id], [product_id], [location_id], [expected_qty], [received_qty], [note], [putaway_qty]) VALUES (10013, 10013, 17, 36, 21, 0, N'Dòng nhập mẫu', 0);
GO
SET IDENTITY_INSERT [dbo].[inbound_line] OFF;
GO
-- dbo.inbound_line: 19 rows
SET IDENTITY_INSERT [dbo].[inbound_receipt] ON;
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (1, N'NK-20261005-0001', N'NXB Trẻ', 1, CONVERT(datetime2(7), N'2026-10-05T00:00:00.0000000', 126), N'DRAFT', N'Phiếu nhập mẫu - trạng thái DRAFT', 10002, CONVERT(datetime2(7), N'2026-10-05T20:40:50.1934890', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (2, N'NK-20261005-0002', N'Fahasa', 1, CONVERT(datetime2(7), N'2026-10-05T00:00:00.0000000', 126), N'RECEIVING', N'Phiếu nhập mẫu - đang nhận hàng', 10002, CONVERT(datetime2(7), N'2026-10-05T20:40:50.2067090', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (3, N'NK-20261002-0005', N'NXB Giáo dục', 1, CONVERT(datetime2(7), N'2026-10-02T00:00:00.0000000', 126), N'CANCELLED', N'Phiếu mẫu đã hủy', 10002, CONVERT(datetime2(7), N'2026-10-05T20:40:50.2068100', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10002, N'DEMO-IN-2026-001', N'Công ty Sách Á Châu', 1, CONVERT(datetime2(7), N'2026-03-01T00:00:00.0000000', 126), N'DRAFT', N'Phiếu nhập dữ liệu demo #01', 10004, CONVERT(datetime2(7), N'2026-03-01T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10003, N'DEMO-IN-2026-002', N'Nhà sách Minh Tâm', 1, CONVERT(datetime2(7), N'2026-03-02T00:00:00.0000000', 126), N'RECEIVING', N'Phiếu nhập dữ liệu demo #02', 10004, CONVERT(datetime2(7), N'2026-03-02T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10004, N'DEMO-IN-2026-003', N'Công ty VPP Hòa Bình', 1, CONVERT(datetime2(7), N'2026-03-03T00:00:00.0000000', 126), N'DONE', N'Phiếu nhập dữ liệu demo #03', 10004, CONVERT(datetime2(7), N'2026-03-03T00:00:00.0000000', 126), NULL, CONVERT(datetime2(7), N'2026-03-03T02:00:00.0000000', 126), 10004);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10005, N'DEMO-IN-2026-004', N'Thiết bị trường học Phương Nam', 1, CONVERT(datetime2(7), N'2026-03-04T00:00:00.0000000', 126), N'CANCELLED', N'Phiếu nhập dữ liệu demo #04', 10004, CONVERT(datetime2(7), N'2026-03-04T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10006, N'DEMO-IN-2026-005', N'NXB Tri Thức', 1, CONVERT(datetime2(7), N'2026-03-05T00:00:00.0000000', 126), N'DRAFT', N'Phiếu nhập dữ liệu demo #05', 10004, CONVERT(datetime2(7), N'2026-03-05T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10007, N'DEMO-IN-2026-006', N'NXB Kim Đồng', 1, CONVERT(datetime2(7), N'2026-03-06T00:00:00.0000000', 126), N'RECEIVING', N'Phiếu nhập dữ liệu demo #06', 10004, CONVERT(datetime2(7), N'2026-03-06T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10008, N'DEMO-IN-2026-007', N'VPP An Phát', 1, CONVERT(datetime2(7), N'2026-03-07T00:00:00.0000000', 126), N'DONE', N'Phiếu nhập dữ liệu demo #07', 10004, CONVERT(datetime2(7), N'2026-03-07T00:00:00.0000000', 126), NULL, CONVERT(datetime2(7), N'2026-03-07T02:00:00.0000000', 126), 10004);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10009, N'DEMO-IN-2026-008', N'Công ty Sách Đại Việt', 1, CONVERT(datetime2(7), N'2026-03-08T00:00:00.0000000', 126), N'CANCELLED', N'Phiếu nhập dữ liệu demo #08', 10004, CONVERT(datetime2(7), N'2026-03-08T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10010, N'DEMO-IN-2026-009', N'VPP Văn Minh', 1, CONVERT(datetime2(7), N'2026-03-09T00:00:00.0000000', 126), N'DRAFT', N'Phiếu nhập dữ liệu demo #09', 10004, CONVERT(datetime2(7), N'2026-03-09T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10011, N'DEMO-IN-2026-010', N'NXB Lao Động', 1, CONVERT(datetime2(7), N'2026-03-10T00:00:00.0000000', 126), N'RECEIVING', N'Phiếu nhập dữ liệu demo #10', 10004, CONVERT(datetime2(7), N'2026-03-10T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10012, N'DEMO-IN-2026-011', N'Nhà phân phối Sao Mai', 1, CONVERT(datetime2(7), N'2026-03-11T00:00:00.0000000', 126), N'DONE', N'Phiếu nhập dữ liệu demo #11', 10004, CONVERT(datetime2(7), N'2026-03-11T00:00:00.0000000', 126), NULL, CONVERT(datetime2(7), N'2026-03-11T02:00:00.0000000', 126), 10004);
GO
    INSERT INTO [dbo].[inbound_receipt] ([Id], [receipt_no], [supplier_name], [warehouse_id], [receipt_date], [status], [note], [created_by], [created_at], [updated_at], [completed_at], [completed_by]) VALUES (10013, N'DEMO-IN-2026-012', N'Công ty Thương mại Bình Minh', 1, CONVERT(datetime2(7), N'2026-03-12T00:00:00.0000000', 126), N'CANCELLED', N'Phiếu nhập dữ liệu demo #12', 10004, CONVERT(datetime2(7), N'2026-03-12T00:00:00.0000000', 126), NULL, NULL, NULL);
GO
SET IDENTITY_INSERT [dbo].[inbound_receipt] OFF;
GO
-- dbo.inbound_receipt: 15 rows
SET IDENTITY_INSERT [dbo].[inventory_balance] ON;
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (1, 1, 5, 200, CONVERT(datetime2(7), N'2026-10-04T20:51:07.2285130', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (2, 1, 7, 300, CONVERT(datetime2(7), N'2026-10-04T20:51:07.2285300', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (3, 2, 4, 80, CONVERT(datetime2(7), N'2026-10-04T20:51:07.2285300', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (4, 3, 7, 120, CONVERT(datetime2(7), N'2026-10-04T20:51:07.2285310', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (5, 4, 6, 8, CONVERT(datetime2(7), N'2026-10-04T20:51:07.2285310', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (6, 5, 6, 5, CONVERT(datetime2(7), N'2026-10-04T20:51:07.2285310', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10002, 6, 14, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10003, 6, 15, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10004, 7, 16, 97, CONVERT(datetime2(7), N'2026-05-02T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10005, 7, 17, 38, CONVERT(datetime2(7), N'2026-05-02T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10006, 8, 18, 107, CONVERT(datetime2(7), N'2026-06-03T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10007, 8, 19, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10008, 9, 20, 95, CONVERT(datetime2(7), N'2026-05-04T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10009, 9, 21, 40, CONVERT(datetime2(7), N'2026-05-04T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10010, 10, 22, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10011, 10, 23, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10012, 11, 24, 97, CONVERT(datetime2(7), N'2026-05-06T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10013, 11, 25, 38, CONVERT(datetime2(7), N'2026-05-06T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10014, 12, 26, 112, CONVERT(datetime2(7), N'2026-06-07T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10015, 12, 27, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10016, 13, 28, 95, CONVERT(datetime2(7), N'2026-05-08T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10017, 13, 29, 40, CONVERT(datetime2(7), N'2026-05-08T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10018, 14, 30, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10019, 14, 31, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10020, 15, 32, 97, CONVERT(datetime2(7), N'2026-05-10T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10021, 15, 33, 38, CONVERT(datetime2(7), N'2026-05-10T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10022, 16, 34, 117, CONVERT(datetime2(7), N'2026-06-11T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10023, 16, 35, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10024, 17, 36, 95, CONVERT(datetime2(7), N'2026-05-12T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10025, 17, 37, 40, CONVERT(datetime2(7), N'2026-05-12T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10026, 18, 14, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10027, 18, 15, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10028, 19, 16, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10029, 19, 17, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10030, 20, 18, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10031, 20, 19, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10032, 21, 14, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10033, 21, 15, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10034, 22, 16, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10035, 22, 17, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10036, 23, 18, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10037, 23, 19, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10038, 24, 20, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10039, 24, 21, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10040, 25, 22, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10041, 25, 23, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10042, 26, 24, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10043, 26, 25, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10044, 27, 26, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10045, 27, 27, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10046, 28, 28, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10047, 28, 29, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10048, 29, 30, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10049, 29, 31, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10050, 30, 32, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10051, 30, 33, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10052, 31, 34, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10053, 31, 35, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10054, 32, 36, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10055, 32, 37, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10056, 33, 14, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10057, 33, 15, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10058, 34, 16, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10059, 34, 17, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10060, 35, 18, 100, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[inventory_balance] ([Id], [ProductId], [LocationId], [Quantity], [updated_at]) VALUES (10061, 35, 19, 35, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126));
GO
SET IDENTITY_INSERT [dbo].[inventory_balance] OFF;
GO
-- dbo.inventory_balance: 66 rows
SET IDENTITY_INSERT [dbo].[location] ON;
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (1, 1, NULL, N'A1', N'LOC-A1', N'Khu sách', N'AREA', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:07.0557430', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (2, 1, NULL, N'B1', N'LOC-B1', N'Khu văn phòng phẩm', N'AREA', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:07.0557830', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (3, 1, 1, N'A1-01', N'BIN-A1-01', N'Kệ A1 - Ô 01', N'BIN', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:07.1637390', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (4, 1, 1, N'A1-02', N'BIN-A1-02', N'Kệ A1 - Ô 02', N'BIN', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:07.1637440', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (5, 1, 1, N'A1-03', N'BIN-A1-03', N'Kệ A1 - Ô 03', N'BIN', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:07.1637440', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (6, 1, 2, N'B1-01', N'BIN-B1-01', N'Kệ B1 - Ô 01', N'BIN', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:07.1637450', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (7, 1, 2, N'B1-02', N'BIN-B1-02', N'Kệ B1 - Ô 02', N'BIN', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:07.1637460', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (8, 2, NULL, N'C1-01', N'BIN-C1-01', N'Kho phụ - Ô 01', N'BIN', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:07.1637470', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (10, 1, NULL, N'A01-09', N'A01-09', N'Kệ 01 adsad', N'BIN', 1, CONVERT(datetime2(7), N'2026-10-04T14:57:44.6089260', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (11, 2, NULL, N'C1', N'LOC-WH02-C1', N'Khu C1', N'AREA', 1, CONVERT(datetime2(7), N'2026-10-05T20:40:50.0500030', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (12, 1, NULL, N'DEMO-A', N'LOC-WH01-DEMO-A', N'Khu demo DEMO-A', N'AREA', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (13, 2, NULL, N'DEMO-B', N'LOC-WH02-DEMO-B', N'Khu demo DEMO-B', N'AREA', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (14, 1, 12, N'DEMO-A-01', N'LOC-DEMO-A-01', N'Bin demo DEMO-A-01', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (15, 2, 13, N'DEMO-B-01', N'LOC-DEMO-B-01', N'Bin demo DEMO-B-01', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (16, 1, 12, N'DEMO-A-02', N'LOC-DEMO-A-02', N'Bin demo DEMO-A-02', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (17, 2, 13, N'DEMO-B-02', N'LOC-DEMO-B-02', N'Bin demo DEMO-B-02', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (18, 1, 12, N'DEMO-A-03', N'LOC-DEMO-A-03', N'Bin demo DEMO-A-03', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (19, 2, 13, N'DEMO-B-03', N'LOC-DEMO-B-03', N'Bin demo DEMO-B-03', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (20, 1, 12, N'DEMO-A-04', N'LOC-DEMO-A-04', N'Bin demo DEMO-A-04', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (21, 2, 13, N'DEMO-B-04', N'LOC-DEMO-B-04', N'Bin demo DEMO-B-04', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (22, 1, 12, N'DEMO-A-05', N'LOC-DEMO-A-05', N'Bin demo DEMO-A-05', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (23, 2, 13, N'DEMO-B-05', N'LOC-DEMO-B-05', N'Bin demo DEMO-B-05', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (24, 1, 12, N'DEMO-A-06', N'LOC-DEMO-A-06', N'Bin demo DEMO-A-06', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (25, 2, 13, N'DEMO-B-06', N'LOC-DEMO-B-06', N'Bin demo DEMO-B-06', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (26, 1, 12, N'DEMO-A-07', N'LOC-DEMO-A-07', N'Bin demo DEMO-A-07', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (27, 2, 13, N'DEMO-B-07', N'LOC-DEMO-B-07', N'Bin demo DEMO-B-07', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (28, 1, 12, N'DEMO-A-08', N'LOC-DEMO-A-08', N'Bin demo DEMO-A-08', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (29, 2, 13, N'DEMO-B-08', N'LOC-DEMO-B-08', N'Bin demo DEMO-B-08', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (30, 1, 12, N'DEMO-A-09', N'LOC-DEMO-A-09', N'Bin demo DEMO-A-09', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (31, 2, 13, N'DEMO-B-09', N'LOC-DEMO-B-09', N'Bin demo DEMO-B-09', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (32, 1, 12, N'DEMO-A-10', N'LOC-DEMO-A-10', N'Bin demo DEMO-A-10', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (33, 2, 13, N'DEMO-B-10', N'LOC-DEMO-B-10', N'Bin demo DEMO-B-10', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (34, 1, 12, N'DEMO-A-11', N'LOC-DEMO-A-11', N'Bin demo DEMO-A-11', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (35, 2, 13, N'DEMO-B-11', N'LOC-DEMO-B-11', N'Bin demo DEMO-B-11', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (36, 1, 12, N'DEMO-A-12', N'LOC-DEMO-A-12', N'Bin demo DEMO-A-12', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[location] ([Id], [WarehouseId], [ParentId], [Code], [Barcode], [Name], [location_type], [is_active], [created_at], [updated_at]) VALUES (37, 2, 13, N'DEMO-B-12', N'LOC-DEMO-B-12', N'Bin demo DEMO-B-12', N'BIN', 1, CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL);
GO
SET IDENTITY_INSERT [dbo].[location] OFF;
GO
-- dbo.location: 36 rows
SET IDENTITY_INSERT [dbo].[outbound_issue] ON;
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (1, N'DEMO-OUT-2026-001', 1, N'INTERNAL', CONVERT(datetime2(7), N'2026-04-01T00:00:00.0000000', 126), N'DRAFT', N'Phiếu xuất dữ liệu demo #01', 10004, NULL, CONVERT(datetime2(7), N'2026-04-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (2, N'DEMO-OUT-2026-002', 1, N'SALE', CONVERT(datetime2(7), N'2026-04-02T00:00:00.0000000', 126), N'PICKING', N'Phiếu xuất dữ liệu demo #02', 10004, NULL, CONVERT(datetime2(7), N'2026-04-02T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (3, N'DEMO-OUT-2026-003', 1, N'INTERNAL', CONVERT(datetime2(7), N'2026-04-03T00:00:00.0000000', 126), N'DONE', N'Phiếu xuất dữ liệu demo #03', 10004, 10004, CONVERT(datetime2(7), N'2026-04-03T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-04-03T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (4, N'DEMO-OUT-2026-004', 1, N'SALE', CONVERT(datetime2(7), N'2026-04-04T00:00:00.0000000', 126), N'CANCELLED', N'Phiếu xuất dữ liệu demo #04', 10004, NULL, CONVERT(datetime2(7), N'2026-04-04T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (5, N'DEMO-OUT-2026-005', 1, N'INTERNAL', CONVERT(datetime2(7), N'2026-04-05T00:00:00.0000000', 126), N'DRAFT', N'Phiếu xuất dữ liệu demo #05', 10004, NULL, CONVERT(datetime2(7), N'2026-04-05T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (6, N'DEMO-OUT-2026-006', 1, N'SALE', CONVERT(datetime2(7), N'2026-04-06T00:00:00.0000000', 126), N'PICKING', N'Phiếu xuất dữ liệu demo #06', 10004, NULL, CONVERT(datetime2(7), N'2026-04-06T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (7, N'DEMO-OUT-2026-007', 1, N'INTERNAL', CONVERT(datetime2(7), N'2026-04-07T00:00:00.0000000', 126), N'DONE', N'Phiếu xuất dữ liệu demo #07', 10004, 10004, CONVERT(datetime2(7), N'2026-04-07T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-04-07T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (8, N'DEMO-OUT-2026-008', 1, N'SALE', CONVERT(datetime2(7), N'2026-04-08T00:00:00.0000000', 126), N'CANCELLED', N'Phiếu xuất dữ liệu demo #08', 10004, NULL, CONVERT(datetime2(7), N'2026-04-08T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (9, N'DEMO-OUT-2026-009', 1, N'INTERNAL', CONVERT(datetime2(7), N'2026-04-09T00:00:00.0000000', 126), N'DRAFT', N'Phiếu xuất dữ liệu demo #09', 10004, NULL, CONVERT(datetime2(7), N'2026-04-09T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (10, N'DEMO-OUT-2026-010', 1, N'SALE', CONVERT(datetime2(7), N'2026-04-10T00:00:00.0000000', 126), N'PICKING', N'Phiếu xuất dữ liệu demo #10', 10004, NULL, CONVERT(datetime2(7), N'2026-04-10T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (11, N'DEMO-OUT-2026-011', 1, N'INTERNAL', CONVERT(datetime2(7), N'2026-04-11T00:00:00.0000000', 126), N'DONE', N'Phiếu xuất dữ liệu demo #11', 10004, 10004, CONVERT(datetime2(7), N'2026-04-11T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-04-11T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[outbound_issue] ([id], [issue_no], [warehouse_id], [issue_reason], [issue_date], [status], [note], [created_by], [completed_by], [created_at], [completed_at]) VALUES (12, N'DEMO-OUT-2026-012', 1, N'SALE', CONVERT(datetime2(7), N'2026-04-12T00:00:00.0000000', 126), N'CANCELLED', N'Phiếu xuất dữ liệu demo #12', 10004, NULL, CONVERT(datetime2(7), N'2026-04-12T00:00:00.0000000', 126), NULL);
GO
SET IDENTITY_INSERT [dbo].[outbound_issue] OFF;
GO
-- dbo.outbound_issue: 12 rows
SET IDENTITY_INSERT [dbo].[outbound_line] ON;
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (1, 1, 6, 14, 2, 0, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (2, 2, 7, 16, 3, 1, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (3, 3, 8, 18, 4, 4, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (4, 4, 9, 20, 5, 0, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (5, 5, 10, 22, 6, 0, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (6, 6, 11, 24, 2, 1, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (7, 7, 12, 26, 3, 3, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (8, 8, 13, 28, 4, 0, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (9, 9, 14, 30, 5, 0, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (10, 10, 15, 32, 6, 3, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (11, 11, 16, 34, 2, 2, N'Dòng xuất mẫu');
GO
    INSERT INTO [dbo].[outbound_line] ([id], [issue_id], [product_id], [location_id], [requested_qty], [picked_qty], [note]) VALUES (12, 12, 17, 36, 3, 0, N'Dòng xuất mẫu');
GO
SET IDENTITY_INSERT [dbo].[outbound_line] OFF;
GO
-- dbo.outbound_line: 12 rows
SET IDENTITY_INSERT [dbo].[product] ON;
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (1, N'BOOK001', N'893850001001', N'Đắc Nhân Tâm', N'BOOK', N'Cuốn', 50, 1, NULL, N'9786041234567', N'Dale Carnegie', 2024, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-10-04T20:51:07.1882320', 126), NULL, 2);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (2, N'BOOK002', N'893850001002', N'Nhà Giả Kim', N'BOOK', N'Cuốn', 30, 1, NULL, N'9786041234568', N'Paulo Coelho', 2024, N'Tiểu thuyết', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-10-04T20:51:07.1882440', 126), NULL, 1);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (3, N'VPP001', N'893850002001', N'Bút bi Thiên Long', N'VPP', N'Cây', 20, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Ngòi 0.5mm', CONVERT(datetime2(7), N'2026-10-04T20:51:07.1882690', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (4, N'VPP002', N'893850002002', N'Vở Campus', N'VPP', N'Quyển', 20, 1, NULL, NULL, NULL, NULL, NULL, N'Campus', N'Trắng', N'200 trang', CONVERT(datetime2(7), N'2026-10-04T20:51:07.1882690', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (5, N'VPP003', N'893850002003', N'Bút chì 2B', N'VPP', N'Cây', 15, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Đen', N'2B', CONVERT(datetime2(7), N'2026-10-04T20:51:07.1882690', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (6, N'DEMO-BOOK-001', N'8938500900001', N'Tư duy nhanh và chậm', N'BOOK', N'Cuốn', 10, 1, NULL, N'9792026000001', N'Tác giả demo 01', 2018, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-01T00:00:00.0000000', 126), NULL, 4);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (7, N'DEMO-BOOK-002', N'8938500900002', N'Tuổi trẻ đáng giá bao nhiêu', N'BOOK', N'Cuốn', 15, 1, NULL, N'9792026000002', N'Tác giả demo 02', 2019, N'Văn học', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-02T00:00:00.0000000', 126), NULL, 6);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (8, N'DEMO-BOOK-003', N'8938500900003', N'Đi tìm lẽ sống', N'BOOK', N'Cuốn', 20, 1, NULL, N'9792026000003', N'Tác giả demo 03', 2020, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-03T00:00:00.0000000', 126), NULL, 8);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (9, N'DEMO-BOOK-004', N'8938500900004', N'Dế Mèn phiêu lưu ký', N'BOOK', N'Cuốn', 25, 1, NULL, N'9792026000004', N'Tác giả demo 04', 2021, N'Văn học', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-04T00:00:00.0000000', 126), NULL, 10);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (10, N'DEMO-BOOK-005', N'8938500900005', N'Cho tôi xin một vé đi tuổi thơ', N'BOOK', N'Cuốn', 30, 1, NULL, N'9792026000005', N'Tác giả demo 05', 2022, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-05T00:00:00.0000000', 126), NULL, 12);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (11, N'DEMO-BOOK-006', N'8938500900006', N'Sapiens: Lược sử loài người', N'BOOK', N'Cuốn', 35, 1, NULL, N'9792026000006', N'Tác giả demo 06', 2023, N'Văn học', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-06T00:00:00.0000000', 126), NULL, 14);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (12, N'DEMO-BOOK-007', N'8938500900007', N'Đời ngắn đừng ngủ dài', N'BOOK', N'Cuốn', 10, 1, NULL, N'9792026000007', N'Tác giả demo 07', 2024, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-07T00:00:00.0000000', 126), NULL, 2);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (13, N'DEMO-BOOK-008', N'8938500900008', N'Nhà lãnh đạo không chức danh', N'BOOK', N'Cuốn', 15, 1, NULL, N'9792026000008', N'Tác giả demo 08', 2025, N'Văn học', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-08T00:00:00.0000000', 126), NULL, 1);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (14, N'DEMO-BOOK-009', N'8938500900009', N'Cây cam ngọt của tôi', N'BOOK', N'Cuốn', 20, 1, NULL, N'9792026000009', N'Tác giả demo 09', 2026, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-09T00:00:00.0000000', 126), NULL, 3);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (15, N'DEMO-BOOK-010', N'8938500900010', N'Không gia đình', N'BOOK', N'Cuốn', 25, 1, NULL, N'9792026000010', N'Tác giả demo 10', 2018, N'Văn học', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-10T00:00:00.0000000', 126), NULL, 4);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (16, N'DEMO-BOOK-011', N'8938500900011', N'Bí mật tư duy triệu phú', N'BOOK', N'Cuốn', 30, 1, NULL, N'9792026000011', N'Tác giả demo 11', 2019, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-11T00:00:00.0000000', 126), NULL, 6);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (17, N'DEMO-BOOK-012', N'8938500900012', N'Thói quen nguyên tử', N'BOOK', N'Cuốn', 35, 1, NULL, N'9792026000012', N'Tác giả demo 12', 2020, N'Văn học', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-12T00:00:00.0000000', 126), NULL, 8);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (18, N'DEMO-BOOK-013', N'8938500900013', N'Totto-chan bên cửa sổ', N'BOOK', N'Cuốn', 10, 1, NULL, N'9792026000013', N'Tác giả demo 13', 2021, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-13T00:00:00.0000000', 126), NULL, 10);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (19, N'DEMO-BOOK-014', N'8938500900014', N'Bắt trẻ đồng xanh', N'BOOK', N'Cuốn', 15, 1, NULL, N'9792026000014', N'Tác giả demo 14', 2022, N'Văn học', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-14T00:00:00.0000000', 126), NULL, 12);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (20, N'DEMO-BOOK-015', N'8938500900015', N'Nghĩ giàu và làm giàu', N'BOOK', N'Cuốn', 20, 0, NULL, N'9792026000015', N'Tác giả demo 15', 2023, N'Kỹ năng sống', NULL, NULL, NULL, CONVERT(datetime2(7), N'2026-01-15T00:00:00.0000000', 126), NULL, 14);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (21, N'DEMO-VPP-001', N'8938501000001', N'Bút gel mực xanh', N'STATIONERY', N'Hộp', 15, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-01T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (22, N'DEMO-VPP-002', N'8938501000002', N'Bút lông bảng', N'STATIONERY', N'Cái', 20, 1, NULL, NULL, NULL, NULL, NULL, N'VPP Demo', N'Nhiều màu', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-02T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (23, N'DEMO-VPP-003', N'8938501000003', N'Bút dạ quang', N'STATIONERY', N'Cái', 25, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-03T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (24, N'DEMO-VPP-004', N'8938501000004', N'Sổ tay lò xo A5', N'STATIONERY', N'Hộp', 30, 1, NULL, NULL, NULL, NULL, NULL, N'VPP Demo', N'Nhiều màu', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-04T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (25, N'DEMO-VPP-005', N'8938501000005', N'Giấy in A4 70gsm', N'STATIONERY', N'Cái', 35, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-05T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (26, N'DEMO-VPP-006', N'8938501000006', N'Giấy note nhiều màu', N'STATIONERY', N'Cái', 15, 1, NULL, NULL, NULL, NULL, NULL, N'VPP Demo', N'Nhiều màu', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-06T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (27, N'DEMO-VPP-007', N'8938501000007', N'Bìa hồ sơ còng', N'STATIONERY', N'Hộp', 20, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-07T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (28, N'DEMO-VPP-008', N'8938501000008', N'Kẹp giấy màu', N'STATIONERY', N'Cái', 25, 1, NULL, NULL, NULL, NULL, NULL, N'VPP Demo', N'Nhiều màu', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-08T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (29, N'DEMO-VPP-009', N'8938501000009', N'Keo dán khô', N'STATIONERY', N'Cái', 30, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-09T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (30, N'DEMO-VPP-010', N'8938501000010', N'Thước kẻ 30cm', N'STATIONERY', N'Hộp', 35, 1, NULL, NULL, NULL, NULL, NULL, N'VPP Demo', N'Nhiều màu', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-10T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (31, N'DEMO-VPP-011', N'8938501000011', N'Gôm trắng', N'STATIONERY', N'Cái', 15, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-11T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (32, N'DEMO-VPP-012', N'8938501000012', N'Dao rọc giấy', N'STATIONERY', N'Cái', 20, 1, NULL, NULL, NULL, NULL, NULL, N'VPP Demo', N'Nhiều màu', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-12T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (33, N'DEMO-VPP-013', N'8938501000013', N'Bấm kim số 10', N'STATIONERY', N'Hộp', 25, 1, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-13T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (34, N'DEMO-VPP-014', N'8938501000014', N'Kim bấm số 10', N'STATIONERY', N'Cái', 30, 1, NULL, NULL, NULL, NULL, NULL, N'VPP Demo', N'Nhiều màu', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-14T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[product] ([id], [product_code], [barcode], [name], [product_type], [unit], [min_stock], [is_active], [note], [isbn], [author], [publication_year], [category], [brand], [color], [specification], [created_at], [updated_at], [publisher_id]) VALUES (35, N'DEMO-VPP-015', N'8938501000015', N'Bìa trình ký', N'STATIONERY', N'Cái', 35, 0, NULL, NULL, NULL, NULL, NULL, N'Thiên Long', N'Xanh', N'Dữ liệu mẫu phục vụ kiểm thử', CONVERT(datetime2(7), N'2026-02-15T00:00:00.0000000', 126), NULL, NULL);
GO
SET IDENTITY_INSERT [dbo].[product] OFF;
GO
-- dbo.product: 35 rows
SET IDENTITY_INSERT [dbo].[stock_movement] ON;
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (1, 8, 18, N'IN', 10, CONVERT(datetime2(7), N'2026-03-03T02:00:00.0000000', 126), 10004, N'Nhập kho từ chứng từ demo', 10004, N'DEMO-IN-2026-003', N'INBOUND');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (2, 12, 26, N'IN', 14, CONVERT(datetime2(7), N'2026-03-07T02:00:00.0000000', 126), 10008, N'Nhập kho từ chứng từ demo', 10004, N'DEMO-IN-2026-007', N'INBOUND');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (3, 16, 34, N'IN', 18, CONVERT(datetime2(7), N'2026-03-11T02:00:00.0000000', 126), 10012, N'Nhập kho từ chứng từ demo', 10004, N'DEMO-IN-2026-011', N'INBOUND');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (4, 8, 18, N'OUT', 4, CONVERT(datetime2(7), N'2026-04-03T01:00:00.0000000', 126), 3, N'Xuất kho từ chứng từ demo', 10004, N'DEMO-OUT-2026-003', N'OUTBOUND');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (5, 12, 26, N'OUT', 3, CONVERT(datetime2(7), N'2026-04-07T01:00:00.0000000', 126), 7, N'Xuất kho từ chứng từ demo', 10004, N'DEMO-OUT-2026-007', N'OUTBOUND');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (6, 16, 34, N'OUT', 2, CONVERT(datetime2(7), N'2026-04-11T01:00:00.0000000', 126), 11, N'Xuất kho từ chứng từ demo', 10004, N'DEMO-OUT-2026-011', N'OUTBOUND');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (7, 7, 16, N'TRANSFER_OUT', 3, CONVERT(datetime2(7), N'2026-05-02T01:00:00.0000000', 126), 2, N'Điều chuyển demo - xuất', 10004, N'DEMO-TR-2026-002', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (8, 7, 17, N'TRANSFER_IN', 3, CONVERT(datetime2(7), N'2026-05-02T01:00:00.0000000', 126), 2, N'Điều chuyển demo - nhập', 10004, N'DEMO-TR-2026-002', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (9, 9, 20, N'TRANSFER_OUT', 5, CONVERT(datetime2(7), N'2026-05-04T01:00:00.0000000', 126), 4, N'Điều chuyển demo - xuất', 10004, N'DEMO-TR-2026-004', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (10, 9, 21, N'TRANSFER_IN', 5, CONVERT(datetime2(7), N'2026-05-04T01:00:00.0000000', 126), 4, N'Điều chuyển demo - nhập', 10004, N'DEMO-TR-2026-004', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (11, 11, 24, N'TRANSFER_OUT', 3, CONVERT(datetime2(7), N'2026-05-06T01:00:00.0000000', 126), 6, N'Điều chuyển demo - xuất', 10004, N'DEMO-TR-2026-006', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (12, 11, 25, N'TRANSFER_IN', 3, CONVERT(datetime2(7), N'2026-05-06T01:00:00.0000000', 126), 6, N'Điều chuyển demo - nhập', 10004, N'DEMO-TR-2026-006', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (13, 13, 28, N'TRANSFER_OUT', 5, CONVERT(datetime2(7), N'2026-05-08T01:00:00.0000000', 126), 8, N'Điều chuyển demo - xuất', 10004, N'DEMO-TR-2026-008', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (14, 13, 29, N'TRANSFER_IN', 5, CONVERT(datetime2(7), N'2026-05-08T01:00:00.0000000', 126), 8, N'Điều chuyển demo - nhập', 10004, N'DEMO-TR-2026-008', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (15, 15, 32, N'TRANSFER_OUT', 3, CONVERT(datetime2(7), N'2026-05-10T01:00:00.0000000', 126), 10, N'Điều chuyển demo - xuất', 10004, N'DEMO-TR-2026-010', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (16, 15, 33, N'TRANSFER_IN', 3, CONVERT(datetime2(7), N'2026-05-10T01:00:00.0000000', 126), 10, N'Điều chuyển demo - nhập', 10004, N'DEMO-TR-2026-010', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (17, 17, 36, N'TRANSFER_OUT', 5, CONVERT(datetime2(7), N'2026-05-12T01:00:00.0000000', 126), 12, N'Điều chuyển demo - xuất', 10004, N'DEMO-TR-2026-012', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (18, 17, 37, N'TRANSFER_IN', 5, CONVERT(datetime2(7), N'2026-05-12T01:00:00.0000000', 126), 12, N'Điều chuyển demo - nhập', 10004, N'DEMO-TR-2026-012', N'TRANSFER');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (19, 8, 18, N'ADJUSTMENT', 1, CONVERT(datetime2(7), N'2026-06-03T01:00:00.0000000', 126), 3, N'Điều chỉnh tăng do kiểm kê demo.', 10004, N'DEMO-ST-2026-003', N'STOCKTAKE');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (20, 12, 26, N'ADJUSTMENT', 1, CONVERT(datetime2(7), N'2026-06-07T01:00:00.0000000', 126), 7, N'Điều chỉnh tăng do kiểm kê demo.', 10004, N'DEMO-ST-2026-007', N'STOCKTAKE');
GO
    INSERT INTO [dbo].[stock_movement] ([Id], [ProductId], [LocationId], [movement_type], [Quantity], [created_at], [ReferenceId], [Note], [PerformedBy], [reference_no], [reference_type]) VALUES (21, 16, 34, N'ADJUSTMENT', 1, CONVERT(datetime2(7), N'2026-06-11T01:00:00.0000000', 126), 11, N'Điều chỉnh tăng do kiểm kê demo.', 10004, N'DEMO-ST-2026-011', N'STOCKTAKE');
GO
SET IDENTITY_INSERT [dbo].[stock_movement] OFF;
GO
-- dbo.stock_movement: 21 rows
SET IDENTITY_INSERT [dbo].[stock_transfer] ON;
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (1, N'DEMO-TR-2026-001', 1, 2, CONVERT(datetime2(7), N'2026-05-01T00:00:00.0000000', 126), N'DRAFT', N'Điều chuyển demo #01', 10004, CONVERT(datetime2(7), N'2026-05-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (2, N'DEMO-TR-2026-002', 1, 2, CONVERT(datetime2(7), N'2026-05-02T00:00:00.0000000', 126), N'DONE', N'Điều chuyển demo #02', 10004, CONVERT(datetime2(7), N'2026-05-02T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-05-02T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (3, N'DEMO-TR-2026-003', 1, 2, CONVERT(datetime2(7), N'2026-05-03T00:00:00.0000000', 126), N'CANCELLED', N'Điều chuyển demo #03', 10004, CONVERT(datetime2(7), N'2026-05-03T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (4, N'DEMO-TR-2026-004', 1, 2, CONVERT(datetime2(7), N'2026-05-04T00:00:00.0000000', 126), N'DONE', N'Điều chuyển demo #04', 10004, CONVERT(datetime2(7), N'2026-05-04T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-05-04T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (5, N'DEMO-TR-2026-005', 1, 2, CONVERT(datetime2(7), N'2026-05-05T00:00:00.0000000', 126), N'DRAFT', N'Điều chuyển demo #05', 10004, CONVERT(datetime2(7), N'2026-05-05T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (6, N'DEMO-TR-2026-006', 1, 2, CONVERT(datetime2(7), N'2026-05-06T00:00:00.0000000', 126), N'DONE', N'Điều chuyển demo #06', 10004, CONVERT(datetime2(7), N'2026-05-06T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-05-06T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (7, N'DEMO-TR-2026-007', 1, 2, CONVERT(datetime2(7), N'2026-05-07T00:00:00.0000000', 126), N'CANCELLED', N'Điều chuyển demo #07', 10004, CONVERT(datetime2(7), N'2026-05-07T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (8, N'DEMO-TR-2026-008', 1, 2, CONVERT(datetime2(7), N'2026-05-08T00:00:00.0000000', 126), N'DONE', N'Điều chuyển demo #08', 10004, CONVERT(datetime2(7), N'2026-05-08T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-05-08T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (9, N'DEMO-TR-2026-009', 1, 2, CONVERT(datetime2(7), N'2026-05-09T00:00:00.0000000', 126), N'DRAFT', N'Điều chuyển demo #09', 10004, CONVERT(datetime2(7), N'2026-05-09T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (10, N'DEMO-TR-2026-010', 1, 2, CONVERT(datetime2(7), N'2026-05-10T00:00:00.0000000', 126), N'DONE', N'Điều chuyển demo #10', 10004, CONVERT(datetime2(7), N'2026-05-10T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-05-10T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (11, N'DEMO-TR-2026-011', 1, 2, CONVERT(datetime2(7), N'2026-05-11T00:00:00.0000000', 126), N'CANCELLED', N'Điều chuyển demo #11', 10004, CONVERT(datetime2(7), N'2026-05-11T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[stock_transfer] ([id], [transfer_no], [source_warehouse_id], [destination_warehouse_id], [transfer_date], [status], [note], [created_by], [created_at], [completed_at]) VALUES (12, N'DEMO-TR-2026-012', 1, 2, CONVERT(datetime2(7), N'2026-05-12T00:00:00.0000000', 126), N'DONE', N'Điều chuyển demo #12', 10004, CONVERT(datetime2(7), N'2026-05-12T00:00:00.0000000', 126), CONVERT(datetime2(7), N'2026-05-12T01:00:00.0000000', 126));
GO
SET IDENTITY_INSERT [dbo].[stock_transfer] OFF;
GO
-- dbo.stock_transfer: 12 rows
SET IDENTITY_INSERT [dbo].[stock_transfer_line] ON;
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (1, 1, 6, 14, 15, 2);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (2, 2, 7, 16, 17, 3);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (3, 3, 8, 18, 19, 4);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (4, 4, 9, 20, 21, 5);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (5, 5, 10, 22, 23, 2);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (6, 6, 11, 24, 25, 3);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (7, 7, 12, 26, 27, 4);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (8, 8, 13, 28, 29, 5);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (9, 9, 14, 30, 31, 2);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (10, 10, 15, 32, 33, 3);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (11, 11, 16, 34, 35, 4);
GO
    INSERT INTO [dbo].[stock_transfer_line] ([id], [transfer_id], [product_id], [source_location_id], [destination_location_id], [quantity]) VALUES (12, 12, 17, 36, 37, 5);
GO
SET IDENTITY_INSERT [dbo].[stock_transfer_line] OFF;
GO
-- dbo.stock_transfer_line: 12 rows
SET IDENTITY_INSERT [dbo].[stocktake] ON;
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (1, N'DEMO-ST-2026-001', 1, CONVERT(datetime2(7), N'2026-06-01T00:00:00.0000000', 126), N'DRAFT', N'Kiểm kê demo #01', 10004, CONVERT(datetime2(7), N'2026-06-01T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (2, N'DEMO-ST-2026-002', 1, CONVERT(datetime2(7), N'2026-06-02T00:00:00.0000000', 126), N'COUNTING', N'Kiểm kê demo #02', 10004, CONVERT(datetime2(7), N'2026-06-02T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (3, N'DEMO-ST-2026-003', 1, CONVERT(datetime2(7), N'2026-06-03T00:00:00.0000000', 126), N'DONE', N'Kiểm kê demo #03', 10004, CONVERT(datetime2(7), N'2026-06-03T00:00:00.0000000', 126), 10004, CONVERT(datetime2(7), N'2026-06-03T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (4, N'DEMO-ST-2026-004', 1, CONVERT(datetime2(7), N'2026-06-04T00:00:00.0000000', 126), N'CANCELLED', N'Kiểm kê demo #04', 10004, CONVERT(datetime2(7), N'2026-06-04T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (5, N'DEMO-ST-2026-005', 1, CONVERT(datetime2(7), N'2026-06-05T00:00:00.0000000', 126), N'DRAFT', N'Kiểm kê demo #05', 10004, CONVERT(datetime2(7), N'2026-06-05T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (6, N'DEMO-ST-2026-006', 1, CONVERT(datetime2(7), N'2026-06-06T00:00:00.0000000', 126), N'COUNTING', N'Kiểm kê demo #06', 10004, CONVERT(datetime2(7), N'2026-06-06T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (7, N'DEMO-ST-2026-007', 1, CONVERT(datetime2(7), N'2026-06-07T00:00:00.0000000', 126), N'DONE', N'Kiểm kê demo #07', 10004, CONVERT(datetime2(7), N'2026-06-07T00:00:00.0000000', 126), 10004, CONVERT(datetime2(7), N'2026-06-07T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (8, N'DEMO-ST-2026-008', 1, CONVERT(datetime2(7), N'2026-06-08T00:00:00.0000000', 126), N'CANCELLED', N'Kiểm kê demo #08', 10004, CONVERT(datetime2(7), N'2026-06-08T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (9, N'DEMO-ST-2026-009', 1, CONVERT(datetime2(7), N'2026-06-09T00:00:00.0000000', 126), N'DRAFT', N'Kiểm kê demo #09', 10004, CONVERT(datetime2(7), N'2026-06-09T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (10, N'DEMO-ST-2026-010', 1, CONVERT(datetime2(7), N'2026-06-10T00:00:00.0000000', 126), N'COUNTING', N'Kiểm kê demo #10', 10004, CONVERT(datetime2(7), N'2026-06-10T00:00:00.0000000', 126), NULL, NULL);
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (11, N'DEMO-ST-2026-011', 1, CONVERT(datetime2(7), N'2026-06-11T00:00:00.0000000', 126), N'DONE', N'Kiểm kê demo #11', 10004, CONVERT(datetime2(7), N'2026-06-11T00:00:00.0000000', 126), 10004, CONVERT(datetime2(7), N'2026-06-11T01:00:00.0000000', 126));
GO
    INSERT INTO [dbo].[stocktake] ([id], [stocktake_no], [warehouse_id], [stocktake_date], [status], [note], [created_by], [created_at], [completed_by], [completed_at]) VALUES (12, N'DEMO-ST-2026-012', 1, CONVERT(datetime2(7), N'2026-06-12T00:00:00.0000000', 126), N'CANCELLED', N'Kiểm kê demo #12', 10004, CONVERT(datetime2(7), N'2026-06-12T00:00:00.0000000', 126), NULL, NULL);
GO
SET IDENTITY_INSERT [dbo].[stocktake] OFF;
GO
-- dbo.stocktake: 12 rows
SET IDENTITY_INSERT [dbo].[stocktake_line] ON;
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (1, 1, 6, 14, 100, 0, 0, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (2, 2, 7, 16, 97, 0, 0, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (3, 3, 8, 18, 106, 107, 1, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (4, 4, 9, 20, 95, 0, 0, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (5, 5, 10, 22, 100, 0, 0, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (6, 6, 11, 24, 97, 0, 0, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (7, 7, 12, 26, 111, 112, 1, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (8, 8, 13, 28, 95, 0, 0, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (9, 9, 14, 30, 100, 0, 0, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (10, 10, 15, 32, 97, 0, 0, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (11, 11, 16, 34, 116, 117, 1, N'Dòng kiểm kê demo');
GO
    INSERT INTO [dbo].[stocktake_line] ([id], [stocktake_id], [product_id], [location_id], [system_qty], [counted_qty], [difference_qty], [note]) VALUES (12, 12, 17, 36, 95, 0, 0, N'Dòng kiểm kê demo');
GO
SET IDENTITY_INSERT [dbo].[stocktake_line] OFF;
GO
-- dbo.stocktake_line: 12 rows
SET IDENTITY_INSERT [dbo].[supplier] ON;
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (1, N'NXB-HNV', N'NXB Hội Nhà Văn', N'PUBLISHER', NULL, NULL, NULL, NULL, NULL, 1, CONVERT(datetime2(7), N'2026-10-06T16:06:26.6500000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (2, N'LEGACY-2', N'NXB Tổng Hợp', N'PUBLISHER', NULL, NULL, NULL, NULL, NULL, 1, CONVERT(datetime2(7), N'2026-10-06T16:06:26.6500000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (3, N'NXB-TRE', N'NXB Trẻ', N'PUBLISHER', NULL, NULL, NULL, NULL, NULL, 1, CONVERT(datetime2(7), N'2026-10-06T23:06:26.9378530', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (4, N'DEMO-SUP-001', N'Công ty Sách Á Châu', N'BOTH', N'Số 10, Quận 1, TP. Hồ Chí Minh', N'Liên hệ 01', N'0900000001', N'demo.supplier01' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-01T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (5, N'DEMO-SUP-002', N'Nhà sách Minh Tâm', N'SUPPLIER', N'Số 11, Quận 2, TP. Hồ Chí Minh', N'Liên hệ 02', N'0900000002', N'demo.supplier02' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-02T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (6, N'DEMO-SUP-003', N'Công ty VPP Hòa Bình', N'PUBLISHER', N'Số 12, Quận 3, TP. Hồ Chí Minh', N'Liên hệ 03', N'0900000003', N'demo.supplier03' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-03T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (7, N'DEMO-SUP-004', N'Thiết bị trường học Phương Nam', N'SUPPLIER', N'Số 13, Quận 4, TP. Hồ Chí Minh', N'Liên hệ 04', N'0900000004', N'demo.supplier04' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-04T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (8, N'DEMO-SUP-005', N'NXB Tri Thức', N'BOTH', N'Số 14, Quận 5, TP. Hồ Chí Minh', N'Liên hệ 05', N'0900000005', N'demo.supplier05' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-05T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (9, N'DEMO-SUP-006', N'NXB Kim Đồng', N'SUPPLIER', N'Số 15, Quận 6, TP. Hồ Chí Minh', N'Liên hệ 06', N'0900000006', N'demo.supplier06' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-06T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (10, N'DEMO-SUP-007', N'VPP An Phát', N'PUBLISHER', N'Số 16, Quận 7, TP. Hồ Chí Minh', N'Liên hệ 07', N'0900000007', N'demo.supplier07' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-07T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (11, N'DEMO-SUP-008', N'Công ty Sách Đại Việt', N'SUPPLIER', N'Số 17, Quận 8, TP. Hồ Chí Minh', N'Liên hệ 08', N'0900000008', N'demo.supplier08' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-08T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (12, N'DEMO-SUP-009', N'VPP Văn Minh', N'BOTH', N'Số 18, Quận 9, TP. Hồ Chí Minh', N'Liên hệ 09', N'0900000009', N'demo.supplier09' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-09T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (13, N'DEMO-SUP-010', N'NXB Lao Động', N'SUPPLIER', N'Số 19, Quận 10, TP. Hồ Chí Minh', N'Liên hệ 10', N'0900000010', N'demo.supplier10' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-10T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (14, N'DEMO-SUP-011', N'Nhà phân phối Sao Mai', N'PUBLISHER', N'Số 20, Quận 11, TP. Hồ Chí Minh', N'Liên hệ 11', N'0900000011', N'demo.supplier11' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-11T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (15, N'DEMO-SUP-012', N'Công ty Thương mại Bình Minh', N'SUPPLIER', N'Số 21, Quận 12, TP. Hồ Chí Minh', N'Liên hệ 12', N'0900000012', N'demo.supplier12' + NCHAR(64) + N'example.test', NULL, 0, CONVERT(datetime2(7), N'2026-01-12T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (16, N'DEMO-SUP-013', N'Fahasa', N'SUPPLIER', N'Số 22, Quận 1, TP. Hồ Chí Minh', N'Liên hệ 13', N'0900000013', N'demo.supplier13' + NCHAR(64) + N'example.test', NULL, 1, CONVERT(datetime2(7), N'2026-01-13T00:00:00.0000000', 126), NULL);
GO
    INSERT INTO [dbo].[supplier] ([id], [code], [name], [type], [address], [contact_person], [phone], [email], [note], [is_active], [created_at], [updated_at]) VALUES (17, N'DEMO-SUP-014', N'NXB Giáo dục', N'PUBLISHER', N'Số 23, Quận 2, TP. Hồ Chí Minh', N'Liên hệ 14', N'0900000014', N'demo.supplier14' + NCHAR(64) + N'example.test', NULL, 0, CONVERT(datetime2(7), N'2026-01-14T00:00:00.0000000', 126), NULL);
GO
SET IDENTITY_INSERT [dbo].[supplier] OFF;
GO
-- dbo.supplier: 17 rows
SET IDENTITY_INSERT [dbo].[warehouse] ON;
GO
    INSERT INTO [dbo].[warehouse] ([Id], [Code], [Name], [Address], [is_active], [created_at], [updated_at]) VALUES (1, N'WH01', N'Kho chính', N'Kho trung tâm', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:06.9119490', 126), NULL);
GO
    INSERT INTO [dbo].[warehouse] ([Id], [Code], [Name], [Address], [is_active], [created_at], [updated_at]) VALUES (2, N'WH02', N'Kho phụ', N'Kho phụ văn phòng', 1, CONVERT(datetime2(7), N'2026-10-04T20:51:06.9119620', 126), NULL);
GO
    INSERT INTO [dbo].[warehouse] ([Id], [Code], [Name], [Address], [is_active], [created_at], [updated_at]) VALUES (3, N'WH01-03', N'Kho phụ', N'phía sau nhà máy bỏ hoang', 1, CONVERT(datetime2(7), N'2026-10-04T14:58:51.8135260', 126), NULL);
GO
SET IDENTITY_INSERT [dbo].[warehouse] OFF;
GO
-- dbo.warehouse: 3 rows

-- Re-enable and validate all foreign keys/check constraints.
ALTER TABLE [dbo].[app_role] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[app_user] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[inbound_line] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[inbound_receipt] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[inventory_balance] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[location] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[outbound_issue] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[outbound_line] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[product] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stock_movement] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stock_transfer] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stock_transfer_line] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stocktake] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[stocktake_line] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[supplier] WITH CHECK CHECK CONSTRAINT ALL;
ALTER TABLE [dbo].[warehouse] WITH CHECK CHECK CONSTRAINT ALL;
GO
COMMIT TRANSACTION;
GO
