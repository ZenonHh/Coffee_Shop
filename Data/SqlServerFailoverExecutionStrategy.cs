using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoffeeShop.Data;

public sealed class SqlServerFailoverExecutionStrategy : SqlServerRetryingExecutionStrategy
{
    private readonly string _secondaryConnectionString;

    public SqlServerFailoverExecutionStrategy(
        ExecutionStrategyDependencies dependencies,
        string secondaryConnectionString)
        : base(
            dependencies,
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)
    {
        _secondaryConnectionString = secondaryConnectionString;
    }

    protected override bool ShouldRetryOn(Exception exception)
    {
        if (!base.ShouldRetryOn(exception))
        {
            return false;
        }

        if (Dependencies.CurrentContext.Context.Database.GetDbConnection() is SqlConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }

            connection.ConnectionString = _secondaryConnectionString;
        }

        return true;
    }
}
