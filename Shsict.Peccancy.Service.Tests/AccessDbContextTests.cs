﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.OleDb;
using Shsict.Peccancy.Service.DbHelper;

namespace Shsict.Peccancy.Service.Tests
{
    [TestClass()]
    public class AccessDbTests
    {
        [Ignore]
        [TestMethod()]
        public void AccessDbNativeTest()
        {
            //连接Access字符串
            string conStr = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\SqlData\Peccancy.mdb;Persist Security Info=False";

            DataSet ds = new DataSet();
            var sql = "SELECT * FROM Truck_Record";

            using (var conn = new OleDbConnection(conStr))
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                using (var adapter = new OleDbDataAdapter(sql, conn))
                {
                    adapter.Fill(ds);
                }
            }

            if (ds.Tables[0].Rows.Count > 0)
            {
                var dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    var id = dr["ID"];
                    var truckNo = dr["TruckNo"];
                    var createTime = dr["CreateTime"];
                    var remark = dr["Remark"];
                }
            }
        }

        [TestMethod()]
        public void AccessDbHelperTest()
        {
            const string conStr = @"Provider=Microsoft.Jet.OLEDB.4.0; 
                    Data Source=C:\Projects\ShsictPeccancy\Shsict.Peccancy.Mvc\App_Data\ITCClient2.mdb;
                    Persist Security Info=False";

            var sql = @"SELECT * FROM [MVC_DATA] WHERE [ABSTIME] > #2018/01/01 01:01:01# ";

            var ds = AccessHelper.ExecuteDataset(conStr, CommandType.Text, sql);

            if (ds.Tables[0].Rows.Count > 0)
            {
                DataTable dt = ds.Tables[0];

                Assert.IsTrue(dt.Rows.Count > 0);
            }
        }
    }
}
