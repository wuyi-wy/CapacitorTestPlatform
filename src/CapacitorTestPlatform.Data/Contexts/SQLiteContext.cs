using System;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace CapacitorTestPlatform.Data.Contexts;

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

    public SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private void InitializeDatabase()
    {
        using var conn = CreateConnection();

        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS TBL_PLANCACHE (
                PLAN_CODE TEXT,
                LOT TEXT,
                STATION TEXT,
                DEVICE_ID TEXT,
                ITEM_NO TEXT,
                SPEC_NAME TEXT,
                SPEC_VALUE TEXT,
                SPEC_MIN TEXT,
                SPEC_MAX TEXT,
                SPEC_UNIT TEXT,
                CREATE_DT TEXT DEFAULT (datetime('now','localtime'))
            )");

        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS TBL_CHECKDATA (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                PLAN_CODE TEXT,
                LOT TEXT,
                DEVICE_ID TEXT,
                DEVICE_TYPE TEXT,
                ITEM_NO TEXT,
                SPEC_NAME TEXT,
                CHECK_NAME TEXT,
                CHECK_VALUE TEXT,
                RESULT TEXT,
                TEST_DT TEXT DEFAULT (datetime('now','localtime')),
                OPERATOR TEXT,
                REMARK TEXT,
                SYNC_STATUS INTEGER DEFAULT 0
            )");

        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS TBL_DEVICE_CONFIG (
                DEVICE_TYPE TEXT NOT NULL,
                PARAM_NAME TEXT NOT NULL,
                PARAM_VALUE TEXT,
                UPDATE_DT TEXT DEFAULT (datetime('now','localtime')),
                PRIMARY KEY (DEVICE_TYPE, PARAM_NAME)
            )");

        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS TBL_CONNECTION_LOG (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                DEVICE_TYPE TEXT,
                PORT TEXT,
                BAUD_RATE INTEGER,
                STATUS TEXT,
                MESSAGE TEXT,
                LOG_DT TEXT DEFAULT (datetime('now','localtime'))
            )");

        conn.Execute("CREATE INDEX IF NOT EXISTS IDX_CHECKDATA_LOT ON TBL_CHECKDATA(LOT)");
        conn.Execute("CREATE INDEX IF NOT EXISTS IDX_CHECKDATA_PLAN ON TBL_CHECKDATA(PLAN_CODE)");
        conn.Execute("CREATE INDEX IF NOT EXISTS IDX_CHECKDATA_SYNC ON TBL_CHECKDATA(SYNC_STATUS)");
        conn.Execute("CREATE INDEX IF NOT EXISTS IDX_PLANCACHE_LOT ON TBL_PLANCACHE(LOT)");
    }
}
