using System;
using System.Collections.Generic;
using Lokad.Cqrs.TapeStorage;
using Npgsql;

namespace Sample.Storage.Sql
{
    /// <summary>
    /// <para>This is a SQL event storage for MySQL, simplified to demonstrate 
    /// essential principles.
    /// If you need a more robust mySQL implementation, check out Event Store of
    /// Jonathan Oliver</para>
    /// <para>This code is frozen to match IDDD book. For latest practices see Lokad.CQRS Project</para>
    /// </summary>
    public sealed class SqlEventStore : IAppendOnlyStore
    {
        readonly string _connectionString;
        private readonly Guid customerId;


        public SqlEventStore(string connectionString, Guid customerId)
        {
            _connectionString = connectionString;
            this.customerId = customerId;
        }

        public void Initialize()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                const string txt = @"
CREATE TABLE IF NOT EXISTS ES_Events2 (
    Id serial NOT NULL,
    CustomerId uuid NOT NULL,
    Name varchar(50) NOT NULL,
    Version int NOT NULL,
    Data bytea NOT NULL
)";
                using (var cmd = new NpgsqlCommand(txt, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Dispose()
        {
            
        }

        public void Append(string name, byte[] data, long expectedVersion)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    var version = MakeSureLastVersionMatches(name, expectedVersion, conn, tx);

                    const string txt =
                           @"INSERT INTO ES_Events2 (CustomerId, Name, Version, Data) 
                                VALUES(:customerId, :name, :version, :data)";

                    using (var cmd = new NpgsqlCommand(txt, conn, tx))
                    {
                        cmd.Parameters.AddWithValue(":name", name);
                        cmd.Parameters.AddWithValue(":version", version+1);
                        cmd.Parameters.AddWithValue(":data", data);
                        cmd.Parameters.AddWithValue(":customerId", customerId);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }

        private int MakeSureLastVersionMatches(string name, long expectedVersion, NpgsqlConnection conn,
                                                      NpgsqlTransaction tx)
        {
            const string sql =
                @"SELECT COALESCE(MAX(Version),0) 
                            FROM ES_Events2 
                            WHERE Name=:name AND CustomerId=:customerId";
            int version;
            using (var cmd = new NpgsqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue(":name", name); 
                cmd.Parameters.AddWithValue(":customerId", customerId);

                version = (int) cmd.ExecuteScalar();
                if (expectedVersion != -1)
                {
                    if (version != expectedVersion)
                    {
                        throw new AppendOnlyStoreConcurrencyException(version, expectedVersion, name);
                    }
                }
            }
            return version;
        }

        public IEnumerable<DataWithVersion> ReadRecords(string name, long afterVersion, int maxCount)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                const string sql =
                    @"SELECT Data, Version, Id FROM ES_Events2
                        WHERE Name = :name AND Version > :version AND CustomerId = :customerId
                        ORDER BY Version
                        LIMIT :take";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(":name", name);
                    cmd.Parameters.AddWithValue(":version", afterVersion);
                    cmd.Parameters.AddWithValue(":take", maxCount);
                    cmd.Parameters.AddWithValue(":customerId", customerId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = (byte[])reader["Data"];
                            var version = (int)reader["Version"];
                            var id = (int) reader["Id"];
                            yield return new DataWithVersion(version, data, id);
                        }
                    }
                }
            }
        }


        public IEnumerable<DataWithKey> ReadRecords(long afterVersion, int maxCount)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                const string sql =
                    @"SELECT Data, Name, Version, Id  FROM ES_Events2
                        WHERE Id > :after AND CustomerId = :customerId
                        ORDER BY Id
                        LIMIT :take";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(":after", afterVersion);
                    cmd.Parameters.AddWithValue(":take", maxCount);
                    cmd.Parameters.AddWithValue(":customerId", customerId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var data = (byte[])reader["Data"];
                            var name = (string)reader["Name"];
                            var version = (int)reader["Version"];
                            var id = (int)reader["Id"];
                            yield return new DataWithKey(name, data,version, id);
                        }
                    }
                }
            }
        }

        

        public void Close()
        {
            
        }

        public void ResetStore()
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {

                    const string txt =
                        @"DELETE FROM ES_Events2 WHERE CustomerId = :customerId";

                    using (var cmd = new NpgsqlCommand(txt, conn, tx))
                    {
                        cmd.Parameters.AddWithValue(":customerId", customerId);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
        }


        public long GetCurrentVersion()
        {
            throw new NotImplementedException();
        }
    }
}