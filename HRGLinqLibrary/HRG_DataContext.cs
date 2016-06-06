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
        string QueryProc(string procName, string argumentJson);
        string QueryProc(string procName);
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
            throw new Exception("ERROR, QueryProc not finished!");
        }

        //无参形式的存储过程，一般来说应该是查询存储过程
        public string QueryProc(string procName)
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


        private string GetProcText(string procName)
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
                return reader.GetValue(0).ToString();
            }
            else
                return "";
        }


        //无参形式的存储过程，一般来说应该是查询存储过程
        public string QueryProc(string procName)
        {
            try
            {
                if (procName.IndexOf("SEARCH") < 0)
                    throw new Exception("ERROR, QueryProc name invalid!");

                #region 读取数据
                //创建数据库命令,通过procName取出实际的proc命令
                OleDbCommand command = _conn.CreateCommand();

                command.CommandText = GetProcText(procName);
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
                throw new Exception(String.Format("ERROR, Mysql DataContext error while do QueryProc: {0}", procName));
            }
            finally
            {
                //do nothing yet
            }
        }

        public string Query(object queryObj)
        {
            throw new Exception("ERROR,Query not finished!");
        }

        public string QueryProc(string procName, string argumentJson)
        {
            throw new Exception("ERROR, QueryProc not finished!");
        }

        private OleDbConnection _conn;
    }
    #endregion

}
