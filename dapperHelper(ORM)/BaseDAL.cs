using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using Dapper;
using System.Linq;
using Dapper.Contrib.Extensions;

namespace DapperHelper
{
    public class BaseDAL
    {
        /// <summary>
        /// 資料庫連線字串
        /// </summary>
        protected string ConnStr = WebConfigurationManager.AppSettings["ROG_ConnStr"];
        /// <summary>
        /// 資料庫連線設定
        /// </summary>
        protected int _connectionTimeout = 20;

        public BaseDAL()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }

        #region Dapper-Query系列

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="T">回傳的資料類型</typeparam>
        /// <param name="querySql">SQL</param>
        /// <param name="param">查詢參數物件</param>
        /// <param name="commandType">類型</param>
        /// <returns></returns>
        public IEnumerable<T> DapperQuery<T>(string querySql, object param = null, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            {
                return con.Query<T>(querySql, param, null, true, _connectionTimeout, commandType);
            }
        }
        /// <summary>
        /// 查詢第一筆資料 (無結果回傳Null)
        /// </summary>
        /// <typeparam name="T">回傳的資料類型</typeparam>
        /// <param name="querySql">SQL</param>
        /// <param name="param">查詢參數物件</param>
        /// <param name="commandType">類型</param>
        /// <returns></returns>
        public T DapperQueryFirstOrDefault<T>(string querySql, object param = null, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            {
                return con.QueryFirstOrDefault<T>(querySql, param, null, _connectionTimeout, commandType);
            }
        }
        public T DapperQueryFirstOrDefaultMyTrans<T>(string querySql, object param, IDbConnection con, IDbTransaction trans)
        {
            return con.QueryFirstOrDefault<T>(querySql, param, trans, _connectionTimeout, CommandType.Text);
        }
        public IEnumerable<T> DapperQueryMyTrans<T>(string querySql, object param, IDbConnection con, IDbTransaction trans)
        {
            return con.Query<T>(querySql, param, trans, true, _connectionTimeout, CommandType.Text);
        }
        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <typeparam name="T">資料封裝的物件類型</typeparam>
        /// <param name="keyEntity">Where的物件</param>
        /// <returns></returns>
        public IEnumerable<T> DapperQuery<T>(DynamicParameters keyEntity = null)
           where T : new()
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            List<string> keyProperties = new List<string>();
            List<string> queryProperties = new List<string>();
            string querySql = string.Empty;

            foreach (var entityMember in new T().GetType().GetProperties())
            {
                queryProperties.Add($"[{entityMember.Name}]");
            }

            querySql += $"select {string.Join(", ", queryProperties)} from [{typeof(T).Name }] WITH (NOLOCK) ";

            if (keyEntity != null && keyEntity.ParameterNames.Count() > 0)
            {
                var parametersLookup = (SqlMapper.IParameterLookup)keyEntity;

                foreach (var keyentityParameterName in keyEntity.ParameterNames)
                {
                    var pValue = parametersLookup[keyentityParameterName];

                    if (pValue.GetType().IsArray)
                    {
                        keyProperties.Add($"[{keyentityParameterName}] in @key_{keyentityParameterName}");
                    }
                    else
                    {
                        if (pValue is string && pValue.ToString().Contains("%"))
                        {
                            keyProperties.Add($"[{keyentityParameterName}] like @key_{keyentityParameterName}");
                        }
                        else
                        {
                            keyProperties.Add($"[{keyentityParameterName}] = @key_{keyentityParameterName}");
                        }
                    }

                    dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                }

                querySql += $"where {string.Join(" and ", keyProperties)} ";
            }

            using (IDbConnection con = GetDbConnection())
            {
                return con.Query<T>(querySql, dynamicParameters, null, true, _connectionTimeout, CommandType.Text);
            }
        }
        /// <summary>
        /// 查詢第一筆資料 (無結果回傳Null)
        /// </summary>
        /// <typeparam name="T">資料封裝的物件類型</typeparam>
        /// <param name="keyEntity"></param>
        /// <returns></returns>
        public T DapperQueryFirstOrDefault<T>(DynamicParameters keyEntity = null)
          where T : new()
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            List<string> keyProperties = new List<string>();
            List<string> queryProperties = new List<string>();
            string querySql = string.Empty;

            foreach (var entityMember in new T().GetType().GetProperties())
            {
                queryProperties.Add($"[{entityMember.Name}]");
            }

            querySql += $"select {string.Join(", ", queryProperties)} from [{typeof(T).Name}] WITH (NOLOCK) ";

            if (keyEntity != null && keyEntity.ParameterNames.Count() > 0)
            {
                var parametersLookup = (SqlMapper.IParameterLookup)keyEntity;

                foreach (var keyentityParameterName in keyEntity.ParameterNames)
                {
                    var pValue = parametersLookup[keyentityParameterName];

                    if (pValue.GetType().IsArray)
                    {
                        keyProperties.Add($"[{keyentityParameterName}] in @key_{keyentityParameterName}");
                    }
                    else
                    {
                        if (pValue is string && pValue.ToString().Contains("%"))
                        {
                            keyProperties.Add($"[{keyentityParameterName}] like @key_{keyentityParameterName}");
                        }
                        else
                        {
                            keyProperties.Add($"[{keyentityParameterName}] = @key_{keyentityParameterName}");
                        }
                    }

                    dynamicParameters.Add("key_" + keyentityParameterName, pValue);
                }

                querySql += $"where {string.Join(" and ", keyProperties)} ";
            }

            using (IDbConnection con = GetDbConnection())
            {
                return con.QueryFirstOrDefault<T>(querySql, dynamicParameters, null, _connectionTimeout, CommandType.Text);
            }
        }

        public Tuple<T, IEnumerable<U>> DapperSingleMultiQuery<T, U>(string querySql, object param = null, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            using (var multi = con.QueryMultiple(querySql, param))
            {
                var Single_One = multi.Read<T>().FirstOrDefault();
                var Multi_Two = multi.Read<U>().ToList();

                return new Tuple<T, IEnumerable<U>>(Single_One, Multi_Two);
            }
        }

        public Tuple<T, U, IEnumerable<E>> DapperDoubleSingleMultiQuery<T, U, E>(string querySql, object param = null, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            using (var multi = con.QueryMultiple(querySql, param))
            {
                var Single_One = multi.Read<T>().FirstOrDefault();
                var Single_Two = multi.Read<U>().FirstOrDefault();
                var Multi_Three = multi.Read<E>().ToList();

                return new Tuple<T, U, IEnumerable<E>>(Single_One, Single_Two, Multi_Three);
            }
        }

        public Tuple<T, IEnumerable<U>, IEnumerable<E>> DapperSingleOneDoubleMultiQuery<T, U, E>(string querySql, object param = null, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            using (var multi = con.QueryMultiple(querySql, param))
            {
                var Single_One = multi.Read<T>().FirstOrDefault();
                var Multi_Two = multi.Read<U>().ToList();
                var Multi_Three = multi.Read<E>().ToList();

                return new Tuple<T, IEnumerable<U>, IEnumerable<E>>(Single_One, Multi_Two, Multi_Three);
            }
        }

        public Tuple<IEnumerable<T>, IEnumerable<U>> DapperMultiQuery<T, U>(string querySql, object param = null, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            using (var multi = con.QueryMultiple(querySql, param))
            {
                var Multi_One = multi.Read<T>().ToList();
                var Multi_Two = multi.Read<U>().ToList();

                return new Tuple<IEnumerable<T>, IEnumerable<U>>(Multi_One, Multi_Two);
            }
        }

        #endregion

        #region Dapper-Excute系列

        /// <summary>
        /// Excute Non-Query SQL，允許一次傳入多道SQL指令
        /// </summary>
        /// <param name="excuteSql">SQL</param>
        /// <param name="param">參數物件</param>
        /// <param name="enableTransaction">包Transaction執行</param>
        /// <param name="commandType">類型</param>
        /// <returns>影響資料筆數</returns>
        public int DapperExecuteNonQuery(string excuteSql, object param = null, bool enableTransaction = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            {
                if (!enableTransaction)
                {
                    return con.Execute(excuteSql, param, null, _connectionTimeout, commandType);
                }
                else
                {
                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            var result = con.Execute(excuteSql, param, trans, _connectionTimeout, commandType);
                            trans.Commit();
                            return result;
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ExecuteScalar，執行查詢並傳回第一個資料列的第一個資料行中查詢所傳回的結果
        /// </summary>
        /// <param name="excuteSql">SQL</param>
        /// <param name="param">參數物件</param>
        /// <param name="enableTransaction">包Transaction執行</param>
        /// <param name="commandType">類型</param>
        /// <returns>執行回覆結果</returns>
        public object DapperExecuteScalar(string excuteSql, object param = null, bool enableTransaction = false, CommandType commandType = CommandType.Text)
        {
            using (IDbConnection con = GetDbConnection())
            {
                if (!enableTransaction)
                {
                    return con.ExecuteScalar(excuteSql, param, null, _connectionTimeout, commandType);
                }
                else
                {
                    using (var trans = con.BeginTransaction())
                    {
                        try
                        {
                            var result = con.ExecuteScalar(excuteSql, param, trans, _connectionTimeout, commandType);
                            trans.Commit();
                            return result;
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        #endregion

        #region Dapper-Insert系列

        public long DapperInsertRunMyConn<T>(T insertEntity, IDbConnection con, IDbTransaction trans)
             where T : class
        {
            return con.Insert(insertEntity, trans, _connectionTimeout);
        }
        public bool DapperInsertRunMyConnReturnBool_SQL<T>(T insertEntity, IDbConnection con, IDbTransaction trans)
            where T : class
        {
            IEnumerable<string> fields = typeof(T).GetProperties()
                                                                        .SkipWhile(p => p.CustomAttributes
                                                                                                .Any(a => a.AttributeType == typeof(KeyAttribute)))
                                                                                                .Select(p => p.Name); // 資料實體中的所有屬性(欄位)名稱、除了標有 [Key] 自訂屬性的欄位外
            string tableName = ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), true).First()).Name; // 資料實體對應的資料表名稱
            string fieldNames = string.Join(", ", fields);
            string fieldParameters = string.Join(", @", fields);
            string sql = $"INSERT {tableName}({fieldNames}) values(@{fieldParameters})";

            return con.Execute(sql, insertEntity, trans, _connectionTimeout) > 0;
        }
        public long DapperInsertRunMyConnReturnLong<T>(T insertEntity, IDbConnection con, IDbTransaction trans)
            where T : class
        {
            return con.Insert(insertEntity, trans, _connectionTimeout);
        }
        public int DapperInsertRunMyConnReturnInt<T>(T insertEntity, IDbConnection con, IDbTransaction trans)
            where T : class
        {
            return Convert.ToInt32(con.Insert(insertEntity, trans, _connectionTimeout));
        }
        public bool DapperInsertRunMyConnReturnBool<T>(T insertEntity, IDbConnection con, IDbTransaction trans)
            where T : class
        {
            return con.Insert(insertEntity, trans, _connectionTimeout) > 0;
        }
        public bool DapperInsertReturnBool<T>(T insertEntity)
             where T : class
        {
            IEnumerable<string> fields = typeof(T).GetProperties()
                                                                        .SkipWhile(p => p.CustomAttributes
                                                                                                .Any(a => a.AttributeType == typeof(KeyAttribute)))
                                                                                                .Select(p => p.Name); // 資料實體中的所有屬性(欄位)名稱、除了標有 [Key] 自訂屬性的欄位外
            string tableName = ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), true).First()).Name; // 資料實體對應的資料表名稱
            string fieldNames = string.Join(", ", fields);
            string fieldParameters = string.Join(", @", fields);
            string sql = $"INSERT {tableName}({fieldNames}) values(@{fieldParameters})";

            using (IDbConnection con = GetDbConnection())
            {
                return con.Execute(sql, insertEntity, null, _connectionTimeout) > 0;
            }
        }

        /// <summary>
        /// 新增單筆
        /// </summary>
        /// <typeparam name="T">新增資料物件類型</typeparam>
        /// <param name="insertEntity">新增物件</param>
        /// <returns>The ID(primary key) of the newly inserted record if it is identity using the defined type, otherwise null</returns>
        public long DapperInsertDueToLong<T>(T insertEntity)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return con.Insert(insertEntity, null, _connectionTimeout);
            }
        }

        /// <summary>
        /// 新增單筆或多筆
        /// </summary>
        /// <typeparam name="T">新增資料物件Type or IEnumable</typeparam>
        /// <param name="insertEntity">新增物件</param>
        /// <param name="enableTransaction">是否使用Transaction</param>
        /// <returns>The ID(primary key) of the newly inserted record if it is identity using the defined type, otherwise null</returns>
        public long DapperInsert<T>(T insertEntity, bool enableTransaction = false)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                if (!enableTransaction)
                {
                    return con.Insert(insertEntity, null, _connectionTimeout);
                }
                else
                {
                    using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        try
                        {
                            var insertResult = con.Insert(insertEntity, transaction, _connectionTimeout);
                            transaction.Commit();
                            return insertResult;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 新增多筆資料
        /// </summary>
        /// <typeparam name="T">資料物件類別</typeparam>
        /// <param name="entities">資料物件集合</param>
        /// <returns>新增資料筆數</returns>
        public int DapperBulkInsert<T>(IEnumerable<T> entities)
            where T : class
        {
            int insertedRows = -1;

            using (IDbConnection conn = GetDbConnection())
            {
                using (IDbTransaction transaction = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        IEnumerable<string> fields = typeof(T).GetProperties()
                                                                                    .SkipWhile(p => p.CustomAttributes
                                                                                                            .Any(a => a.AttributeType == typeof(KeyAttribute)))
                                                                                                            .Select(p => p.Name); // 資料實體中的所有屬性(欄位)名稱、除了標有 [Key] 自訂屬性的欄位外
                        string tableName = ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), true).First()).Name; // 資料實體對應的資料表名稱
                        string fieldNames = string.Join(", ", fields);
                        string fieldParameters = string.Join(", @", fields);
                        string sql = $"INSERT {tableName}({fieldNames}) values(@{fieldParameters})";

                        insertedRows = conn.Execute(sql, entities, transaction, _connectionTimeout);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return insertedRows;
        }

        public bool MyTransactionInsert<T1, T2, T3, T4>(T1 insertT1, T2 insertT2, T3 insertT3, T4 insertT4)
            where T1 : class where T2 : class where T3 : class where T4 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            {
                using (var trans = con.BeginTransaction())
                {
                    try
                    {
                        if (insertT1 != null) isOk = this.DapperInsertRunMyConnReturnBool_SQL(insertT1, con, trans);
                        if (insertT2 != null && isOk) isOk = this.DapperInsertRunMyConnReturnBool_SQL(insertT2, con, trans);
                        if (insertT3 != null && isOk) isOk = this.DapperInsertRunMyConnReturnBool_SQL(insertT3, con, trans);
                        if (insertT4 != null && isOk) isOk = this.DapperInsertRunMyConnReturnBool_SQL(insertT4, con, trans);

                        if (!isOk) trans.Rollback();
                        else trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                    }
                }
            }
            return isOk;
        }

        #endregion

        #region Dapper-Update系列

        /// <summary>
        /// 更新單筆資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <param name="updateEntity">更新物件</param>
        /// <returns></returns>
        public bool DapperUpdate<T>(T updateEntity)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return con.Update(updateEntity, null, _connectionTimeout);
            }
        }

        /// <summary>
        /// 更新兩筆不同類型資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <typeparam name="T2">更新資料物件Type2</typeparam>
        /// <param name="updateEntity">更新物件</param>
        /// <param name="updateEntity2">更新物件2</param>
        /// <returns></returns>
        public bool DapperUpdateMutil<T, T2>(T updateEntity, T2 updateEntity2)
            where T : class
            where T2 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            {
                using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        if (con.Update(updateEntity, transaction, _connectionTimeout))
                        {
                            if (con.Update(updateEntity2, transaction, _connectionTimeout))
                            {
                                transaction.Commit();
                                isOk = true;
                            }
                        }

                        if (!isOk)
                        {
                            transaction.Rollback();
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }

            return isOk;
        }

        /// <summary>
        /// 更新資料
        /// </summary>
        /// <param name="tableName">更新Table名稱</param>
        /// <param name="key">指定updateEntity裡面的Where Condition,如果多個key用","分隔, key must in updateEntity property</param>
        /// <param name="updateEntity">更新的物件</param>
        /// <returns></returns>
        public bool DapperUpdate(string tableName, string key, object updateEntity)
        {
            if (string.IsNullOrWhiteSpace(key) || updateEntity == null)
            {
                return false;
            }

            string[] keyList = key.Split(',');
            List<string> keyFilter = new List<string>();
            List<string> updateProperties = new List<string>();
            foreach (var pro in updateEntity.GetType().GetProperties())
            {
                if (keyList.Contains(pro.Name))
                {
                    keyFilter.Add($"[{pro.Name}] {(pro.PropertyType.IsArray ? "in" : "=")} @{pro.Name}");
                }
                else
                {
                    updateProperties.Add($"[{pro.Name}] = @{pro.Name}");
                }
            }

            string sql = $"update [{tableName}] set {string.Join(", ", updateProperties)} where {string.Join(" and ", keyFilter)}";

            using (IDbConnection con = GetDbConnection())
            {
                return con.Execute(sql, updateEntity, null, _connectionTimeout, CommandType.Text) > 0;
            }
        }

        /// <summary>
        /// 更新資料
        /// </summary>
        /// <param name="tableName">更新Table名稱</param>
        /// <param name="keyEntity">Where的物件</param>
        /// <param name="updateEntity">更新的物件</param>
        /// <returns></returns>
        public bool DapperUpdate(string tableName, object keyEntity, object updateEntity)
        {
            if (keyEntity == null || updateEntity == null)
            {
                return false;
            }

            DynamicParameters dynamicParameters = new DynamicParameters();
            List<string> keyProperties = new List<string>();
            List<string> updateProperties = new List<string>();

            foreach (var entityMember in updateEntity.GetType().GetProperties())
            {
                dynamicParameters.Add("set_" + entityMember.Name, entityMember.GetValue(updateEntity));
                updateProperties.Add($"[{entityMember.Name}] = @set_{entityMember.Name}");
            }

            foreach (var keyentityMember in keyEntity.GetType().GetProperties())
            {
                dynamicParameters.Add("key_" + keyentityMember.Name, keyentityMember.GetValue(keyEntity));
                keyProperties.Add($"[{keyentityMember.Name}] {(keyentityMember.PropertyType.IsArray ? "in" : "=")} @key_{keyentityMember.Name}");
            }

            string sql = $"update [{tableName}] set {string.Join(", ", updateProperties)} where {string.Join(" and ", keyProperties)}";

            using (IDbConnection con = GetDbConnection())
            {
                return con.Execute(sql, dynamicParameters, null, _connectionTimeout, CommandType.Text) > 0;
            }
        }
        /// <summary>
        /// 更新資料
        /// </summary>
        /// <param name="tableName">更新Table名稱</param>
        /// <param name="keyEntity">Where的物件</param>
        /// <param name="updateEntity">更新的物件</param>
        /// <param name="con">DB</param>
        /// <param name="transaction">transaction</param>
        /// <returns></returns>
        public bool DapperUpdateWithTrans(string tableName, object keyEntity, object updateEntity, IDbConnection con, IDbTransaction transaction)
        {
            if (keyEntity == null || updateEntity == null)
            {
                return false;
            }

            DynamicParameters dynamicParameters = new DynamicParameters();
            List<string> keyProperties = new List<string>();
            List<string> updateProperties = new List<string>();

            foreach (var entityMember in updateEntity.GetType().GetProperties())
            {
                dynamicParameters.Add("set_" + entityMember.Name, entityMember.GetValue(updateEntity));
                updateProperties.Add($"[{entityMember.Name}] = @set_{entityMember.Name}");
            }

            foreach (var keyentityMember in keyEntity.GetType().GetProperties())
            {
                dynamicParameters.Add("key_" + keyentityMember.Name, keyentityMember.GetValue(keyEntity));
                keyProperties.Add($"[{keyentityMember.Name}] {(keyentityMember.PropertyType.IsArray ? "in" : "=")} @key_{keyentityMember.Name}");
            }

            string sql = $"update [{tableName}] set {string.Join(", ", updateProperties)} where {string.Join(" and ", keyProperties)}";

            return con.Execute(sql, dynamicParameters, transaction, _connectionTimeout, CommandType.Text) > 0;
        }
        /// <summary>
        /// 更新資料
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <param name="updateEntities">更新資料物件s</param>
        /// <returns></returns>
        public bool DapperUpdate<T>(IEnumerable<T> updateEntities)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            using (var trans = con.BeginTransaction())
            {
                try
                {
                    foreach (var updateEntity in updateEntities)
                    {
                        if (!con.Update(updateEntity, trans, _connectionTimeout))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region Dapper-Update Then Insert Or Insert Then Delete系列

        /// <summary>
        /// 更新一物件、新增一物件
        /// </summary>
        /// <typeparam name="T">更新資料物件Type</typeparam>
        /// <typeparam name="T1">新增資料物件Type</typeparam>
        /// <param name="updateEnties">更新物件</param>
        /// <param name="insertEnties">新增物件</param>
        /// <returns>是否成功</returns>
        public bool DapperUpdateSingleInsertSingle<T, T1>(T updateEnties, T1 insertEnties)
            where T : class
            where T1 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (con.Update(updateEnties, transaction, _connectionTimeout))
                    {
                        if (con.Insert(insertEnties, transaction, _connectionTimeout) > 0)
                        {
                            transaction.Commit();
                            isOk = true;
                        }
                    }

                    if (!isOk)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return isOk;
        }
        public bool DapperUpdateMutilInsertSingle<T, T2, T3>(T updateEntity, T2 updateEntity2, T3 insertEntity)
            where T : class
            where T2 : class
            where T3 : class
        {
            bool IsOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    IsOk = con.Update(updateEntity, transaction, _connectionTimeout);
                    if (IsOk) IsOk = con.Update(updateEntity2, transaction, _connectionTimeout);
                    if (IsOk) IsOk = (con.Insert(insertEntity, transaction, _connectionTimeout) > 0);

                    if (!IsOk) transaction.Rollback();
                    else transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                }
            }
            return IsOk;
        }

        /// <summary>
        /// 新增一物件、刪除一物件
        /// </summary>
        /// <typeparam name="T">新增資料物件Type</typeparam>
        /// <typeparam name="T2">刪除資料物件Type</typeparam>
        /// <param name="insertEnties">新增物件</param>
        /// <param name="deleteEnties">刪除物件</param>
        /// <returns>是否成功</returns>
        public bool DapperInsertSingleDeleteSingle<T, T2>(T insertEnties, T2 deleteEnties)
            where T : class
            where T2 : class
        {
            bool isOk = false;
            using (IDbConnection con = GetDbConnection())
            using (var transaction = con.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                try
                {
                    if (con.Insert(insertEnties, transaction, _connectionTimeout) > 0)
                    {
                        if (con.Delete(deleteEnties, transaction, _connectionTimeout))
                        {
                            transaction.Commit();
                            isOk = true;
                        }
                    }

                    if (!isOk)
                    {
                        transaction.Rollback();
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return isOk;
        }

        #endregion

        #region Dapper-Delete系列

        /// <summary>
        /// 刪除單筆資料
        /// </summary>
        /// <typeparam name="T">刪除資料物件Type</typeparam>
        /// <param name="deleteEntity">刪除物件</param>
        /// <returns></returns>
        public bool DapperDelete<T>(T deleteEntity)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            {
                return con.Delete(deleteEntity, null, _connectionTimeout);
            }
        }

        /// <summary>
        /// 刪除多筆資料
        /// </summary>
        /// <typeparam name="T">刪除多筆資料物件Type</typeparam>
        /// <param name="deleteEntities">刪除資料物件s</param>
        /// <returns></returns>
        public bool DapperDeleteList<T>(IEnumerable<T> deleteEntities)
            where T : class
        {
            using (IDbConnection con = GetDbConnection())
            using (var trans = con.BeginTransaction())
            {
                try
                {
                    foreach (var deleteEntity in deleteEntities)
                    {
                        if (!con.Delete(deleteEntity, trans, _connectionTimeout))
                        {
                            trans.Rollback();
                            return false;
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        public bool DapperDeleteListMyTrans<T>(IEnumerable<T> deleteEntities, IDbConnection con, IDbTransaction trans)
        {
            return con.Delete(deleteEntities, trans, _connectionTimeout);
        }

        #endregion

        #region Dapper-取得資料庫連線並開啟
        /// <summary>
        /// Dapper取得資料庫連線並開啟
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetDbConnection()
        {
            IDbConnection dBConnection;

            dBConnection = new SqlConnection(ConnStr);

            if (dBConnection.State != ConnectionState.Open)
            {
                dBConnection.Open();
            }

            return dBConnection;
        }
        #endregion
    }
}