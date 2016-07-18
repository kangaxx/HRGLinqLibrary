/***************************************************************************************************************
 * 哈工大机器人集团 数据库操作中间件
 * 2016. 5. 20
 * 顾欣欣
 * 说明：
 *       ALINQ现在收费啦！所以保险起见还是内部开发一个简单中间件比较好，
 *       将前后台与各种类型（MYSQL, SQLSERVER, SQLITE等)的数据链接起来。
 * 
 * 类列表 : HRG_DBConnection 建立数据库链接
 *          HRG_DataContext 数据库操作功能，类似于System.Data.Linq可以实现linq语法读取数据。
 * 
 * 
 * ************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HRG_BaseLibrary_2012;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.OleDb;


namespace HRG_LinqLibrary
{
    ////查询条件
    //enum SqlQueryType
    //{
    //    TSqlString = 1,
    //    Process = 2
    //}
    ////数据查询信息类
    //class queryObj
    //{
    //    SqlQueryType type;
    //    string condition;
    //    string argument;
    //}
    //数据操作类接口，暂时只有数据查询功能（2016.5.23）
    public interface HRG_IDataContext
    {
        string Query(string queryString);
        string Query(object queryObj);
        IDataReader obj_Query(string queryString);
        int Delete(string deleteString);
        int Update(string updateString);
        int Insert(string insertString);
        string QueryProc(string procName, string argumentJson);
        string QueryProc(string procName);
    }

    //json转到实体类，json内确保有'name'和'value'字符串
    class Proc_Argument
    {
        public string name = "";
        public string value = "";
        public int direct = 1; //存储过程参数类型 1 输入 2 输出 3 返回值
    }

    //mysql功能实现
    #region mysql datacontext
    public class HRG_MysqlDataContext : HRG_IDataContext
    {

        public HRG_MysqlDataContext(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                throw new Exception("ERROR, DataContext construct failed !");
            }
            _conn = (MySqlConnection)conn;
        }

        public IDataReader obj_Query(string queryString)
        {
            try
            {

                #region 读取数据
                //创建数据库命令  
                MySqlCommand cmd = _conn.CreateCommand();
                //创建查询语句  
                cmd.CommandText = queryString;
                cmd.CommandType = CommandType.Text;
                //从数据库中读取数据流存入reader中  
                MySqlDataReader reader = cmd.ExecuteReader();

                //从reader中读取下一行数据,如果没有数据,reader.Read()返回flase  

                if (!reader.HasRows)
                    return null; //一条数据都没有直接返回空字符串
                return reader;
                #endregion
            }
            catch
            {
                throw new Exception(String.Format("ERROR, Mysql DataContext error while do QueryProc: {0}", queryString));
            }
            finally
            {
                //do nothing yet
            }
        }

        public string Query(string queryString)
        {
            throw new Exception("ERROR,Query not finished!");
        }

        public string Query(object queryObj)
        {
            throw new Exception("ERROR,Query not finished!");
        }

        public string QueryProc(string procName, string argumentJson)
        {
            try
            {
                if (procName.IndexOf("SEARCH") < 0)
                    throw new Exception("ERROR, QueryProc name invalid!");



                #region 读取数据
                //创建数据库命令  
                MySqlCommand cmd = _conn.CreateCommand();
                //创建查询语句  
                cmd.CommandText = procName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                //组织存储过程参数,目前只支持输入型参数，输出及return稍后添加
                if (argumentJson.Trim() != "")
                {
                    List<Proc_Argument> args = CommonFunction.DeserializeJsonToList<Proc_Argument>(argumentJson);
                    foreach (Proc_Argument arg in args)
                    {
                        Console.Write(String.Format("name is {0}, value is {1}, direct is {2}", arg.name, arg.value, arg.direct));
                        MySqlParameter paramTemp = new MySqlParameter(arg.name, arg.value);
                        if (arg.direct == GlobalVariables.INT_SQL_PARAM_DIRECTION_INPUT)
                        {
                            paramTemp.Direction = ParameterDirection.Input;
                        }
                        else if (arg.direct == GlobalVariables.INT_SQL_PARAM_DIRECTION_OUTPUT)
                        {
                            paramTemp.Direction = ParameterDirection.Output;
                        }
                        else if (arg.direct == GlobalVariables.INT_SQL_PARAM_DIRECTION_RETURN)
                        {
                            paramTemp.Direction = ParameterDirection.ReturnValue;
                        }
                        cmd.Parameters.Add(paramTemp);
                    }
                }
                //从数据库中读取数据流存入reader中  
                MySqlDataReader reader = cmd.ExecuteReader();

                //从reader中读取下一行数据,如果没有数据,reader.Read()返回flase  

                if (!reader.HasRows)
                    return ""; //一条数据都没有直接返回空字符串
                DynDBHelper dynDb = new DynDBHelper();
                while (reader.Read())
                {
                    List<FieldUnit> row = new List<FieldUnit>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        FieldUnit unitTemp = new FieldUnit();
                        unitTemp.FieldName = reader.GetName(i);
                        unitTemp.FieldValue = reader[i].ToString();
                        row.Add(unitTemp);
                    }
                    dynDb.addRow(row);
                }

                string json = CommonFunction.ToJson(dynDb.getDataList());
                return json;
                #endregion
            }
            catch
            {
                throw new Exception(String.Format("ERROR, Mysql DataContext error while do QueryProc: {0}", procName));
            }
            finally
            {
                //do nothing yet
            }
        }

        public int Delete(string deleteString)
        {
            try
            {
                MySqlCommand cmd = _conn.CreateCommand();
                cmd.CommandText = deleteString;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                return cmd.ExecuteNonQuery();

            }
            catch
            {

                throw new Exception(String.Format("ERROR, Mysql delete error while do command: {0}", deleteString));
            }
            finally
            {
                //do nothing yet;

            }
        }

        public int Update(string updateString)
        {
            try
            {
                MySqlCommand cmd = _conn.CreateCommand();
                cmd.CommandText = updateString;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                return cmd.ExecuteNonQuery();

            }
            catch
            {

                throw new Exception(String.Format("ERROR, Mysql update error while do command: {0}", updateString));
            }
            finally
            {
                //do nothing yet;

            }
        }


        //执行插入语句，成功返回插入条数（0表示成功插入0条），失败返回-1
        public int Insert(string insertString)
        {
            try
            {
                MySqlCommand cmd = _conn.CreateCommand();
                cmd.CommandText = insertString;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                return cmd.ExecuteNonQuery();

            }
            catch
            {

                throw new Exception(String.Format("ERROR, Mysql insert error while do command: {0}", insertString));
            }
            finally
            {
                //do nothing yet;

            }

        }

        //无参形式的存储过程，一般来说应该是查询存储过程
        public string QueryProc(string procName)
        {
            return QueryProc(procName, "");
        }

        private MySqlConnection _conn;
    }
    #endregion

    //OleDb功能实现
    #region OleDb connection
    public class HRG_OleDBContext : HRG_IDataContext
    {
        public HRG_OleDBContext(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                throw new Exception("ERROR, DataContext construct failed !");
            }
            _conn = (OleDbConnection)conn;
        }

        //执行select查询语句
        public string Query(string queryString)
        {
            
            OleDbCommand command = _conn.CreateCommand();

            command.CommandText = queryString;

            OleDbDataReader reader = command.ExecuteReader();

            if (!reader.HasRows)
                return ""; //一条数据都没有直接返回空字符串
            DynDBHelper dynDb = new DynDBHelper();
            while (reader.Read())
            {
                List<FieldUnit> row = new List<FieldUnit>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    FieldUnit unitTemp = new FieldUnit();
                    unitTemp.FieldName = reader.GetName(i);
                    unitTemp.FieldValue = reader[i].ToString();
                    row.Add(unitTemp);
                }
                dynDb.addRow(row);
            }
            string json = CommonFunction.ToJson(dynDb.getDataList());
            return json;
        }

        //执行select查询语句,直接返回reader对象
        public IDataReader obj_Query(string queryString)
        {

            OleDbCommand command = _conn.CreateCommand();

            command.CommandText = queryString;

            OleDbDataReader reader = command.ExecuteReader();

            if (!reader.HasRows)
                return null; //一条数据都没有直接返回空字符串
            else
                return reader;

        }

        private string GetProcText(string procName, string argumentJson)
        {
            //创建数据库命令,通过procName取出实际的proc命令
            OleDbCommand command = _conn.CreateCommand();

            command.CommandText = String.Format("select QueryProcText from ProcList where ProcName = '{0}'", procName);
            //从数据库中读取数据流存入reader中
            OleDbDataReader reader = command.ExecuteReader();
            if (!reader.HasRows)
                return ""; //一条数据都没有直接返回空字符串
            if (reader.Read())
            {
                string result = reader.GetValue(0).ToString();
                if (argumentJson.Trim() != "")
                {
                    List<Proc_Argument> args = CommonFunction.DeserializeJsonToList<Proc_Argument>(argumentJson);
                    foreach (Proc_Argument arg in args)
                    {
                        result = result.Replace(arg.name, arg.value);
                    }
                }
                return result;
            }
            else
                return "";
        }


        //无参形式的存储过程，一般来说应该是查询存储过程
        public string QueryProc(string procName)
        {
            return QueryProc(procName, "");
        }

        public string Query(object queryObj)
        {
            throw new Exception("ERROR,Query not finished!");
        }

        public string QueryProc(string procName, string argumentJson)
        {
            try
            {
                if (procName.IndexOf("SEARCH") < 0 && procName.IndexOf("INSERT") < 0 && procName.IndexOf("UPDATE") < 0)
                    throw new Exception("ERROR, QueryProc name invalid!");

                #region 读取数据
                //创建数据库命令,通过procName取出实际的proc命令
                OleDbCommand command = _conn.CreateCommand();

                command.CommandText = GetProcText(procName,argumentJson);
                //从数据库中读取数据流存入reader中
                OleDbDataReader reader = command.ExecuteReader();



                //从reader中读取下一行数据,如果没有数据,reader.Read()返回flase  

                if (!reader.HasRows)
                    return ""; //一条数据都没有直接返回空字符串
                DynDBHelper dynDb = new DynDBHelper();
                while (reader.Read())
                {
                    List<FieldUnit> row = new List<FieldUnit>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        FieldUnit unitTemp = new FieldUnit();
                        unitTemp.FieldName = reader.GetName(i);
                        unitTemp.FieldValue = reader[i].ToString();
                        row.Add(unitTemp);
                    }
                    dynDb.addRow(row);
                }

                string json = CommonFunction.ToJson(dynDb.getDataList());
                return json;
                #endregion
            }
            catch
            {
                throw new Exception(String.Format("ERROR, OleDB DataContext error while do QueryProc: {0}", procName));
            }
            finally
            {
                //do nothing yet
            }
        }


        public int Delete(string deleteString)
        {
            try
            {
                OleDbCommand cmd = _conn.CreateCommand();
                cmd.CommandText = deleteString;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                return cmd.ExecuteNonQuery();

            }
            catch
            {

                throw new Exception(String.Format("ERROR, OleDB delete error while do command: {0}", deleteString));
            }
            finally
            {
                //do nothing yet;

            }
        }

        public int Update(string updateString)
        {
            try
            {
                OleDbCommand cmd = _conn.CreateCommand();
                cmd.CommandText = updateString;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                return cmd.ExecuteNonQuery();

            }
            catch
            {

                throw new Exception(String.Format("ERROR, OleDB update error while do command: {0}", updateString));
            }
            finally
            {
                //do nothing yet;

            }
        }

        //执行插入语句，成功返回插入条数（0表示成功插入0条），失败返回-1
        public int Insert(string insertString)
        {
            try
            {
                OleDbCommand cmd = _conn.CreateCommand();
                cmd.CommandText = insertString;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();
                return cmd.ExecuteNonQuery();

            }
            catch
            {

                throw new Exception(String.Format("ERROR, OleDB insert error while do command: {0}", insertString));
            }
            finally
            {
                //do nothing yet;

            }
        }
        private OleDbConnection _conn;
    }
    #endregion

}
