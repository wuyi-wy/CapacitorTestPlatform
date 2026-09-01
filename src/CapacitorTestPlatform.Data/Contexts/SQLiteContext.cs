using System;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace CapacitorTestPlatform.Data.Contexts;

/// <summary>
/// SQLite 本地数据库上下文，负责创建连接和初始化表结构。
/// 使用 Dapper 作为 ORM，所有表名和字段名均采用 PascalCase 命名规范。
/// </summary>
public class SQLiteContext
{
    private readonly string _connectionString;
    private readonly string _dbPath;

    public SQLiteContext(string dbPath = null)
    {
        _dbPath = dbPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.db");
        _connectionString = $"Data Source={_dbPath}";
        InitializeDatabase();
    }

    /// <summary>
    /// 创建并打开一个新的 SQLite 连接，调用方需自行 using 释放。
    /// </summary>
    public SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    /// <summary>
    /// 初始化本地数据库：创建4张业务表及索引（如已存在则跳过）。
    /// - PlanCache: 远程计划本地缓存
    /// - CheckData: 检测数据主表
    /// - DeviceConfig: 设备参数配置
    /// - ConnectionLog: 串口连接日志
    /// </summary>
    private void InitializeDatabase()
    {
        using var conn = CreateConnection();

        // 计划缓存表 — 从远程拉取的检测计划保存在本地，供测试页面选择
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS PlanCache (
                PlanNo TEXT,
                Lot TEXT,
                Station TEXT,
                DeviceId TEXT,
                ItemNo TEXT,
                SpecName TEXT,
                SpecValue TEXT,
                SpecMin TEXT,
                SpecMax TEXT,
                SpecUnit TEXT,
                CreateTime TEXT DEFAULT (datetime('now','localtime'))
            )");

        // 检测数据表 — 存储每次设备采集的测量结果，是核心业务数据表
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS CheckData (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlanNo TEXT,
                Lot TEXT,
                DeviceId TEXT,
                DeviceType TEXT,
                ItemNo TEXT,
                SpecName TEXT,
                CheckName TEXT,
                CheckValue TEXT,
                Result TEXT,
                TestTime TEXT DEFAULT (datetime('now','localtime')),
                Operator TEXT,
                Remark TEXT,
                SyncStatus INTEGER DEFAULT 0
            )");

        // 设备参数配置表 — 持久化各型号设备的测试参数（当前未启用，预留）
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS DeviceConfig (
                DeviceType TEXT NOT NULL,
                ParamName TEXT NOT NULL,
                ParamValue TEXT,
                UpdateTime TEXT DEFAULT (datetime('now','localtime')),
                PRIMARY KEY (DeviceType, ParamName)
            )");

        // 连接日志表 — 记录串口连接/断开事件（当前未启用，预留）
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS ConnectionLog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DeviceType TEXT,
                Port TEXT,
                BaudRate INTEGER,
                Status TEXT,
                Message TEXT,
                LogTime TEXT DEFAULT (datetime('now','localtime'))
            )");

        // 索引：加速按批次号、计划号、同步状态的查询
        conn.Execute("CREATE INDEX IF NOT EXISTS IX_CheckData_Lot ON CheckData(Lot)");
        conn.Execute("CREATE INDEX IF NOT EXISTS IX_CheckData_PlanNo ON CheckData(PlanNo)");
        conn.Execute("CREATE INDEX IF NOT EXISTS IX_CheckData_SyncStatus ON CheckData(SyncStatus)");
        conn.Execute("CREATE INDEX IF NOT EXISTS IX_PlanCache_Lot ON PlanCache(Lot)");
    }
}
