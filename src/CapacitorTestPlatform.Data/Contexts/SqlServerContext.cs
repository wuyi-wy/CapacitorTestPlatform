using Microsoft.Data.SqlClient;
using System.Data;

namespace CapacitorTestPlatform.Data.Contexts;

public class SqlServerContext : IDisposable
{
    private readonly string _readConnectionString;
    private readonly string _writeConnectionString;
    private SqlConnection? _readConnection;
    private SqlConnection? _writeConnection;

    public bool IsReadAvailable { get; private set; }
    public bool IsWriteAvailable { get; private set; }

    public SqlServerContext(string readConnectionString, string writeConnectionString)
    {
        _readConnectionString = readConnectionString;
        _writeConnectionString = writeConnectionString;
    }

    public IDbConnection? ReadConnection
    {
        get
        {
            if (!IsReadAvailable)
            {
                if (!TryOpenConnection(ref _readConnection, _readConnectionString))
                    return null;
                IsReadAvailable = true;
            }
            return _readConnection;
        }
    }

    public IDbConnection? WriteConnection
    {
        get
        {
            if (!IsWriteAvailable)
            {
                if (!TryOpenConnection(ref _writeConnection, _writeConnectionString))
                    return null;
                IsWriteAvailable = true;
            }
            return _writeConnection;
        }
    }

    private bool TryOpenConnection(ref SqlConnection? connection, string connectionString)
    {
        try
        {
            connection ??= new SqlConnection(connectionString);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return true;
        }
        catch
        {
            connection?.Dispose();
            connection = null;
            return false;
        }
    }

    public void Dispose()
    {
        _readConnection?.Dispose();
        _writeConnection?.Dispose();
    }
}
