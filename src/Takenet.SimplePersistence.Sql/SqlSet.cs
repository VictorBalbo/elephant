﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.SimplePersistence.Sql.Mapping;
using static Takenet.SimplePersistence.Sql.SqlHelper;

namespace Takenet.SimplePersistence.Sql
{
    public abstract class SqlSet<T> : StorageBase<T>, ISet<T>
    {
        protected SqlSet(ITable table, string connectionString)
            : base(table, connectionString)
        {

        }

        #region ISet<T> Members

        public async Task AddAsync(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var cancellationToken = CreateCancellationToken();
            var columnValues = GetColumnValues(value);
            var keyColumnValues = GetKeyColumnValues(columnValues);

            using (var connection = await GetConnectionAsync(cancellationToken).ConfigureAwait(false))
            {
                using (var command = connection.CreateTextCommand(
                    SqlTemplates.DeleteAndInsertWhereNotExists,
                    new
                    {
                        tableName = Table.Name.AsSqlIdentifier(),
                        columns = columnValues.Keys.Select(c => c.AsSqlIdentifier()).ToCommaSepparate(),
                        values = columnValues.Keys.Select(v => v.AsSqlParameterName()).ToCommaSepparate(),
                        filter = GetAndEqualsStatement(keyColumnValues.Keys.ToArray())
                    },
                    columnValues.Select(c => c.ToSqlParameter())))
                {
                    if (await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 0)
                    {
                        throw new Exception("The database operation failed");
                    }
                }
            }
        }

        public async Task<bool> TryRemoveAsync(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var keyColumnValues = GetKeyColumnValues(value);
            var cancellationToken = CreateCancellationToken();
            using (var connection = await GetConnectionAsync(cancellationToken).ConfigureAwait(false))
            {
                return await TryRemoveAsync(keyColumnValues, connection, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IAsyncEnumerable<T>> AsEnumerableAsync()
        {
            var cancellationToken = CreateCancellationToken();
            var connection = await GetConnectionAsync(cancellationToken).ConfigureAwait(false);            
            var selectColumns = Table.Columns.Keys.ToArray();
            var command = connection.CreateTextCommand(
                SqlTemplates.Select,
                new
                {
                    columns = selectColumns.Select(c => c.AsSqlIdentifier()).ToCommaSepparate(),
                    tableName = Table.Name.AsSqlIdentifier(),
                    filter = "1 = 1"  // Yes, I know.
                });
            return new SqlDataReaderAsyncEnumerable<T>(command, Mapper, selectColumns);
        }

        public async Task<bool> ContainsAsync(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            var keyColumnValues = GetKeyColumnValues(value);
            var cancellationToken = CreateCancellationToken();
            using (var connection = await GetConnectionAsync(cancellationToken).ConfigureAwait(false))
            {
                return await ContainsAsync(keyColumnValues, connection, cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
