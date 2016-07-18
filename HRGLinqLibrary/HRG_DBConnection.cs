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
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.OleDb;
using HRG_BaseLibrary_2012;
using System.IO;

namespace HRG_LinqLibrary
{

    //虚拟工厂模式的数据库工厂类，根据传入的参数决定链接那个数据库
    public class HRG_DBFactory : IDisposable
    {
        public HRG_DBFactory(IDbConnection conn)
        {
            if (!(conn.State == ConnectionState.Closed || conn.State == ConnectionState.Broken))
            {
                throw new Exception(String.Format("Error , connection {0} is already opened !", conn.ConnectionString));
            }
            _conn = conn;
            _conn.Open();
        }

        private IDbConnection GetDatabaseConn(string configuration)
        {
            string type = CommonFunction.GetSettingValueByName(configuration, GlobalVariables.STRING_SQLTYPE_FLAG, new char[] { ';' });
            if (type == GlobalVariables.STRING_SQLTYPE_NAME_MYSQL)
            {
                return new MySqlConnection(String.Format("server={0};user id={1};password={2};database={3}",
                    CommonFunction.GetSettingValueByName(configuration, GlobalVariables.STRING_SQL_CONNECTION_TAG_SERVER, new char[] { ';' }),
                    CommonFunction.GetSettingValueByName(configuration, GlobalVariables.STRING_SQL_CONNECTION_TAG_USER, new char[] { ';' }),
                    CommonFunction.GetSettingValueByName(configuration, GlobalVariables.STRING_SQL_CONNECTION_TAG_PASSWORD, new char[] { ';' }),
                    CommonFunction.GetSettingValueByName(configuration, GlobalVariables.STRING_SQL_CONNECTION_TAG_DATABASE, new char[] { ';' })
                    ));

            }
            else if (type == GlobalVariables.STRING_SQLTYPE_NAME_OLEDB)
            {
                return new OleDbConnection(String.Format("Provider={0};Data Source={1}",
                    CommonFunction.GetSettingValueByName(configuration, GlobalVariables.STRING_SQL_CONNECTION_TAG_PROVIDER, new char[] { ';' }),
                    CommonFunction.GetSettingValueByName(configuration, GlobalVariables.STRING_SQL_CONNECTION_TAG_DATASOURCE, new char[] { ';' })
                    ));
            }
            else
            {
                throw new Exception(String.Format("Error, get an unknow database type : [{0}]",type));
            }

        }


        public HRG_DBFactory(string configFile)
        {
            string configuration = "";
            try
            {
                FileStream fs = new FileStream(configFile, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                configuration = SecurityHelper.DecryptDBConn(sr.ReadToEnd());
                _conn = GetDatabaseConn(configuration);
                _conn.Open();
                fs.Close();
            }
            catch(Exception e)
            {
                throw new Exception(String.Format("Error, get DBConnect string in file {0} : {1} , cause by {2}",configFile, configuration,e));
            }
        }

        

        public HRG_IDataContext getDC()
        {
            HRG_IDataContext result;
            if (_conn.GetType() == typeof(MySqlConnection))
            {
                result = new HRG_MysqlDataContext(_conn);
            }
            else if (_conn.GetType() == typeof(OleDbConnection))
            {
                result = new HRG_OleDBContext(_conn);
            }
            else
            {
                result = null;
            }
            return result;
        }

        public IDbConnection getConn()
        {
            return _conn;
        }

        public void Dispose()
        {
            if (_conn.State == ConnectionState.Open)
                _conn.Close();
            Console.WriteLine("conn closed!");
        }

        private IDbConnection _conn;
    }

}
