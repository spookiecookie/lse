﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkShareEasyModel;
using Connections;
using System.Data.SqlClient;

namespace LinkShareEasyADO
{
    public class ADONumericToken
    {
        public long Insert(IToken numericToken)
        { 
            using (var c = Connections.GetConnections.GetConnection())
            using (var cmd = c.CreateCommand())
            using (var t = c.BeginTransaction(System.Data.IsolationLevel.Serializable))
            { 
                c.Open();
                cmd.CommandText = "INSERT INTO NumericTokens (NumericTokenN, Used) VALUES (@1, @2); SELECT SCOPE_IDENTITY()";

                cmd.Parameters.AddWithValue("@1", numericToken.TokenText);
                cmd.Parameters.AddWithValue("@2", 1);

                var id = Convert.ToInt32(cmd.ExecuteScalar());
                return id; 
            }
        }

        /// <summary>
        /// Tells whether numeric token in already used for some link.
        /// </summary>
        /// <param name="numericToken"></param>
        /// <returns></returns>
        public bool IsAvailable(IToken token)
        {
            using (var c = Connections.GetConnections.GetConnection())
            using (var cmd = c.CreateCommand())
            {
                c.Open();

                cmd.CommandText = "SELECT TOP 1 NumericTokenId FROM NumericTokens WHERE NumericTokenN = @2 AND Used = @1";
                cmd.Parameters.AddWithValue("@1", false);
                cmd.Parameters.AddWithValue("@2", Convert.ToInt32(token.TokenText));

                using (var reader = cmd.ExecuteReader())
                {
                    return reader.HasRows;
                }
            }
            throw new Exception("Numeric token find operation failed.");
        }

        public IToken Take()
        {
            NumericToken nt = GetAvailable();
            return SetUsed(nt, true);
        }

        /// <summary>
        /// Returns available numeric token.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public NumericToken GetAvailable()
        {
            using (var c = Connections.GetConnections.GetConnection())
            using (var cmd = c.CreateCommand())
            {
                c.Open();

                cmd.CommandText = "SELECT TOP 1 NumericTokenId, NumericTokenN, Used FROM NumericTokens WHERE Used = @1 ORDER BY NEWID()";
                cmd.Parameters.AddWithValue("@1", false);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        return new NumericToken()
                        { 
                            NumericTokenId = reader.GetInt32(reader.GetOrdinal("NumericTokenId"))
                            , NumericTokenN = reader.GetInt32(reader.GetOrdinal("NumericTokenN"))
                            , Used = reader.GetBoolean(reader.GetOrdinal("Used"))
                        };
                    }
                    else
                    {
                        throw new Exception("Available numeric tokens not found"); //TODO account for this situation
                    }
                }
            } 
        }

        public NumericToken Find(long numericTokenId)
        {
            using (var c = Connections.GetConnections.GetConnection())
            using (var cmd = c.CreateCommand())
            {
                c.Open();

                cmd.CommandText = "SELECT TOP 1 NumericTokenId, NumericTokenN, Used FROM NumericTokens WHERE NumericTokenId = @1";
                cmd.Parameters.AddWithValue("@1", numericTokenId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return new NumericToken()
                        {
                            NumericTokenId = reader.GetInt32(reader.GetOrdinal("NumericTokenId"))
                            , NumericTokenN = reader.GetInt32(reader.GetOrdinal("NumericTokenN"))
                            , Used = reader.GetBoolean(reader.GetOrdinal("Used"))
                        };
                    }
                    else
                    {
                        throw new Exception(String.Format("Numeric token usage not found with id={0:d}", numericTokenId));
                    }
                }
            } 
        }

        public NumericToken FindByNumber(int n)
        {
            using (var c = Connections.GetConnections.GetConnection())
            using (var cmd = c.CreateCommand())
            {
                c.Open();

                cmd.CommandText = "SELECT TOP 1 NumericTokenId, NumericTokenN, Used FROM NumericTokens WHERE NumericTokenN = @1";
                cmd.Parameters.AddWithValue("@1", n);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return new NumericToken()
                        {
                            NumericTokenId = reader.GetInt32(reader.GetOrdinal("NumericTokenId"))
                            , NumericTokenN = reader.GetInt32(reader.GetOrdinal("NumericTokenN"))
                            , Used = reader.GetBoolean(reader.GetOrdinal("Used"))
                        };
                    }
                    else
                    {
                        throw new Exception(String.Format("Numeric token '{0}' not found.", n));
                    }
                }
            }
        }

        public NumericToken SetUsed(String tokenText, bool used)
        {
            return SetUsed(FindByNumber(Convert.ToInt32(tokenText)), used);
        }

        public NumericToken SetUsed(NumericToken nt, bool used)
        {
            return SetUsed(nt.NumericTokenId, used);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numericTokenId"></param>
        /// <returns></returns>
        public NumericToken SetUsed(long numericTokenId, bool used)
        {
            NumericToken nt = Find(numericTokenId);

            using (var c = Connections.GetConnections.GetConnection())
            using (var cmd = c.CreateCommand())
            {
                c.Open();

                cmd.CommandText = "UPDATE NumericTokens SET Used = @2 WHERE NumericTokenId = @1";
                cmd.Parameters.AddWithValue("@1", numericTokenId);
                cmd.Parameters.AddWithValue("@2", used);
                cmd.ExecuteNonQuery();
            }

            return Find(numericTokenId);
        }
    }
}
