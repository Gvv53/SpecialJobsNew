using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

//using EFOptimization.DataModel;

namespace SpecialJobs.Helpers
{
	static class DatabaseOps
	{
		//static DatabaseOps()
		//{
		//	using (var context = new MethodsEntities())
		//	{
		//		var connection = context.Database.Connection;

		//		Server server = new Server(new ServerConnection(new SqlConnection(connection.ConnectionString)));

		//		Database = server.Databases[connection.Database];
		//	}
		//}
        private static void currentDatabase()
        {
            using (var context = new MethodsEntities())
            {
                var connection = context.Database.Connection;

                Server server = new Server(new ServerConnection(new SqlConnection(connection.ConnectionString)));

                _Database = server.Databases[connection.Database];
            }
        }

        public static Database _Database;
        private static Database Database
        {
            get {
                if (_Database == null)
                    currentDatabase();
                return _Database;
            }
            set
            { _Database = value; }
        }

		private static Table TempTable
		{
			get
			{
                currentDatabase(); //актуальное состояние БД
                var database = Database;
                var table = database.Tables["MEASURING_DATA_TEMP"];
                return database.Tables["MEASURING_DATA_TEMP"];
			}
		}

		private static Table PrimaryTable
		{
			get
			{
				var database = Database;

				return database.Tables["MEASURING_DATA"];
			}
		}

		public static void DropTempTable()
		{
			if (TempTable != null)
                TempTable.Drop();
		}

		public static void ClearTempTable()
		{
			if (TempTable != null) TempTable.TruncateData();
		}

		public static void ClearPrimaryTable()
		{
			if (PrimaryTable != null) PrimaryTable.TruncateData();
		}

		public static void Merge()
		{
			using (var context = new MethodsEntities())
			{
				context.Merge();
			}
		}
        //Все пользователи, использующие БД Methods
        //public static StringCollection GetAllLogin()
        //{
        //    using (var context = new MethodsEntities())
        //    {
        //        return context.GetAllLogins();
        //    }
        //}
        //public static StringCollection GetActiveLogins()
        //{
        //    using (var context = new MethodsEntities())
        //    {
        //        return context.GetActiveLogins();
        //    }
        //}

        public static void RunSqlScript(string script)
		{
			Database.ExecuteWithResults(script);
		}

        public static void CreateTempTable()
        {
            DropTempTable();

            ScriptingOptions options = new ScriptingOptions
            {
                Default = true,
                NoIdentities = true,
                DriAll = false,
                DriPrimaryKey = false
            };

            StringCollection script = PrimaryTable.Script(options);

            var sb = new StringBuilder();

            foreach (string str in script)
            {
                sb.Append(str);
                sb.Append(Environment.NewLine);
            }

            string tempScript = sb.ToString().Replace("MEASURING_DATA", "MEASURING_DATA_TEMP");
            tempScript = tempScript.ToString().Replace("IDENTITY(1, 1)", "");            

            try {
                RunSqlScript(tempScript);
            }
            catch(Exception e) {  }
        }
	}
}
