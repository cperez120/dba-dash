﻿CREATE TABLE [Switch].[CPU] (
    [InstanceID]         INT           NOT NULL,
    [EventTime]          DATETIME2 (3) NOT NULL,
    [SQLProcessCPU]      TINYINT       NOT NULL,
    [SystemIdleCPU]      TINYINT       NOT NULL,
    [OtherCPU]           AS            ((100)-([SQLProcessCPU]+[SystemIdleCPU])),
    [TotalCPU]           AS            ((100)-[SystemIdleCPU]),
    [SampleCount]        AS            ((1)),
    [MaxSQLProcessCPU]   AS            ([SQLProcessCPU]),
    [MaxOtherProcessCPU] AS            ((100)-([SQLProcessCPU]+[SystemIdleCPU])),
    [MaxTotalCPU]        AS            ((100)-[SystemIdleCPU]),
    [SumOtherCPU]        AS            ((100)-([SQLProcessCPU]+[SystemIdleCPU])),
    [SumTotalCPU]        AS            ((100)-[SystemIdleCPU]),
    [SumSQLProcessCPU]   AS            ([SQLProcessCPU]),
    CONSTRAINT [PK_CPU] PRIMARY KEY CLUSTERED ([InstanceID] ASC, [EventTime] ASC) WITH (DATA_COMPRESSION = PAGE)
);



