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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRGLinqLibrary
{
    public enum HRG_SQLType
    {
        MYSQL = 1,
        SQLSERVER2000 = 2,
        SQLSERVER2005 = 3,
        ORACLE = 4
    }
    public class HRG_DBConnection
    {
        public HRG_DBConnection(string connstr, HRG_SQLType type)
        {

        }

        public HRG_DBConnection(string configFile)
        {

        }


    }

}
