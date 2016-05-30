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

namespace HRG_LinqLibrary
{

    //虚拟工厂模式的数据库工厂类，根据传入的参数决定链接那个数据库
    public class HRG_DBFactory : IDisposable
    {
        public HRG_DBFactory(IDbConnection  conn)
        {
            if (!(conn.State == ConnectionState.Closed || conn.State == ConnectionState.Broken))
            {
                throw new Exception(String.Format("Error , connection {0} is already opened !", conn.ConnectionString));
            }
            _conn = conn;
            _conn.Open();
        }

        public HRG_DBFactory(string configFile)
        {
            throw new Exception(String.Format("Error , HRG_DBConnection open by file {0} , function not finished yet!"));
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
                result = new HRG_OleDBContext(_conn));
            }
            else
            {
                result = null;
            }
            return result;
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
