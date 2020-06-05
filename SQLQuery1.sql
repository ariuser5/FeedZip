

CREATE DATABASE MyDB2;
USE MyDB2;

CREATE LOGIN feedzipapp
	WITH PASSWORD = '123'
GO

CREATE USER feedzipapp FOR LOGIN feedzipapp
ALTER ROLE db_owner ADD MEMBER feedzipapp;


CREATE TABLE Records
(
	dirName VARCHAR(32) NOT NULL PRIMARY KEY,
	path VARCHAR(255) NOT NULL
);

CREATE TABLE ImageFiles
(
	id INT NOT NULL PRIMARY KEY,
	path VARCHAR(255) NOT NULL,
	time VARCHAR(32),
	date VARCHAR(32),
	dir VARCHAR(32),
	FOREIGN KEY (dir) REFERENCES Records(dirName)
);
GO

CREATE PROCEDURE InsertRecord 
	@DirName nvarchar(32), 
	@Path nvarchar(255)
AS
	INSERT INTO Records VALUES(@DirName, @Path)
GO

CREATE PROCEDURE InsertFile 
	@Id nvarchar(32), 
	@Path nvarchar(255),
	@Time nvarchar(32),
	@Date nvarchar(32),
	@Dir nvarchar(32)
AS
	INSERT INTO ImageFiles VALUES(@Id, @Path, @Time, @Date, @Dir)
GO



