CREATE TABLE [dbo].[LeadsTree] (
    [id]              INT           IDENTITY (1, 1) NOT NULL,
    [title]           VARCHAR (50)  NOT NULL,
    [descript]        VARCHAR (255) NULL,
    [executors]       VARCHAR (255) NULL,
    [lead_status]     SMALLINT      DEFAULT ((0)) NULL,
    [start_date]      DATETIME      NULL,
    [plan]            FLOAT (53)    NULL,
    [lead_time]       FLOAT (53)    NULL,
    [completion_date] DATETIME      NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);
CREATE TABLE [dbo].[LeadsTreePath] (
    [tpid]       INT IDENTITY (1, 1) NOT NULL,
    [ancestor]   INT NOT NULL,
    [descendant] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([tpid] ASC),
    FOREIGN KEY ([ancestor]) REFERENCES [dbo].[LeadsTree] ([id]),
    FOREIGN KEY ([descendant]) REFERENCES [dbo].[LeadsTree] ([id])
);