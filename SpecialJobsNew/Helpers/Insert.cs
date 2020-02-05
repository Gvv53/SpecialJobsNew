using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace SpecialJobs.Helpers
{
    public static class GenericListDataReaderExtensions
    {
        public static GenericListDataReader<T> GetDataReader<T>(this IEnumerable<T> list)
        {
            return new GenericListDataReader<T>(list);
        }
    }
    public class GenericListDataReader<T> : IDataReader
    {
        private IEnumerator<T> list = null;
        private List<PropertyInfo> properties = new List<PropertyInfo>();
        private Dictionary<string, int> nameLookup = new Dictionary<string, int>();

        public GenericListDataReader(IEnumerable<T> list)
        {
            this.list = list.GetEnumerator();

            properties.AddRange(
                typeof(T)
                .GetProperties(
                    BindingFlags.GetProperty |
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly
                    ));

            for (int i = 0; i < properties.Count; i++)
            {
                nameLookup[properties[i].Name] = i;
            }
        }

        #region IDataReader Members

        public void Close()
        {
            list.Dispose();
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return list.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return properties.Count; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            return properties[i].PropertyType;
        }

        public float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            return properties[i].Name;
        }

        public int GetOrdinal(string name)
        {
            if (nameLookup.ContainsKey(name))
            {
                return nameLookup[name];
            }
            else
            {
                return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            return properties[i].GetValue(list.Current, null);
        }

        public int GetValues(object[] values)
        {
            int getValues = Math.Max(FieldCount, values.Length);

            for (int i = 0; i < getValues; i++)
            {
                values[i] = GetValue(i);
            }

            return getValues;
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) == null;
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        #endregion
    }
    public static class Insert
	{
	

		//public static void BySqlBulkCopy(List<MEASURING_DATA> entities)
        public static void SqlBulkCopy(List<MEASURING_DATA> entities)
        {
			//DatabaseOps.ClearPrimaryTable();

			//var entities = EntityGenerator.Generate(count);

			//var stopwatch = new Stopwatch();
			//stopwatch.Start();

			BulkCopy(entities, "MEASURING_DATA");

			//stopwatch.Stop();

			//if (output) Console.WriteLine("BulkCopy({0}) = {1} s", entities.Count, stopwatch.Elapsed.TotalSeconds);
		}

		//public static List<MEASURING_DATA> BulkCopy(List<MEASURING_DATA> entities, string table = "MEASURING_DATA")
        public static List<MEASURING_DATA> BulkCopy(List<MEASURING_DATA> entities, string table)
        {
			var context = new MethodsEntities();

			string connectionString = context.Database.Connection.ConnectionString;

			using (IDataReader reader = entities.GetDataReader())
			using (SqlConnection connection = new SqlConnection(connectionString))
			using (SqlBulkCopy bcp = new SqlBulkCopy(connection))
			{
				connection.Open();

				bcp.DestinationTableName = string.Format("[{0}]", table);

				bcp.ColumnMappings.Add("MDA_ID", "MDA_ID");
                bcp.ColumnMappings.Add("MDA_MODE_ID", "MDA_MODE_ID");
                bcp.ColumnMappings.Add("MDA_F", "MDA_F");
                bcp.ColumnMappings.Add("MDA_F_UNIT_ID", "MDA_F_UNIT_ID");
                bcp.ColumnMappings.Add("MDA_I", "MDA_I");
                bcp.ColumnMappings.Add("MDA_ECN_VALUE_IZM", "MDA_ECN_VALUE_IZM");
                bcp.ColumnMappings.Add("MDA_EN_VALUE_IZM", "MDA_EN_VALUE_IZM");
                bcp.ColumnMappings.Add("MDA_U0CN_VALUE_IZM", "MDA_U0CN_VALUE_IZM");
                bcp.ColumnMappings.Add("MDA_U0N_VALUE_IZM", "MDA_U0N_VALUE_IZM");
                bcp.ColumnMappings.Add("MDA_UFCN_VALUE_IZM", "MDA_UFCN_VALUE_IZM");
                bcp.ColumnMappings.Add("MDA_UFN_VALUE_IZM", "MDA_UFN_VALUE_IZM");
                bcp.ColumnMappings.Add("MDA_ES_VALUE_IZM", "MDA_ES_VALUE_IZM");
                bcp.ColumnMappings.Add("MDA_ECN_VALUE_IZM_DB", "MDA_ECN_VALUE_IZM_DB");
                bcp.ColumnMappings.Add("MDA_EN_VALUE_IZM_DB", "MDA_EN_VALUE_IZM_DB");               
                bcp.ColumnMappings.Add("MDA_U0CN_VALUE_IZM_DB", "MDA_U0CN_VALUE_IZM_DB");
                bcp.ColumnMappings.Add("MDA_U0N_VALUE_IZM_DB", "MDA_U0N_VALUE_IZM_DB");
                bcp.ColumnMappings.Add("MDA_UFCN_VALUE_IZM_DB", "MDA_UFCN_VALUE_IZM_DB");
                bcp.ColumnMappings.Add("MDA_UFN_VALUE_IZM_DB", "MDA_UFN_VALUE_IZM_DB");
                bcp.ColumnMappings.Add("MDA_ES_VALUE_IZM_DB", "MDA_ES_VALUE_IZM_DB");
                bcp.ColumnMappings.Add("MDA_ECN_VALUE_IZM_MKV", "MDA_ECN_VALUE_IZM_MKV");
                bcp.ColumnMappings.Add("MDA_EN_VALUE_IZM_MKV", "MDA_EN_VALUE_IZM_MKV");
                bcp.ColumnMappings.Add("MDA_U0CN_VALUE_IZM_MKV", "MDA_U0CN_VALUE_IZM_MKV");
                bcp.ColumnMappings.Add("MDA_U0N_VALUE_IZM_MKV", "MDA_U0N_VALUE_IZM_MKV");
                bcp.ColumnMappings.Add("MDA_UFCN_VALUE_IZM_MKV", "MDA_UFCN_VALUE_IZM_MKV");
                bcp.ColumnMappings.Add("MDA_UFN_VALUE_IZM_MKV", "MDA_UFN_VALUE_IZM_MKV");
                bcp.ColumnMappings.Add("MDA_ES_VALUE_IZM_MKV", "MDA_ES_VALUE_IZM_MKV");
                bcp.ColumnMappings.Add("MDA_EGS_DB", "MDA_EGS_DB");
                bcp.ColumnMappings.Add("MDA_EGS_MKV", "MDA_EGS_MKV");
                bcp.ColumnMappings.Add("MDA_E", "MDA_E");
                bcp.ColumnMappings.Add("MDA_UF", "MDA_UF");
                bcp.ColumnMappings.Add("MDA_U0", "MDA_U0");
                bcp.ColumnMappings.Add("MDA_EGS", "MDA_EGS");
                bcp.ColumnMappings.Add("MDA_KP", "MDA_KP");
                bcp.ColumnMappings.Add("MDA_KPS", "MDA_KPS");
                bcp.ColumnMappings.Add("MDA_RBW", "MDA_RBW");
                bcp.ColumnMappings.Add("MDA_RBW_UNIT_ID", "MDA_RBW_UNIT_ID");
                bcp.ColumnMappings.Add("MDA_TYPE", "MDA_TYPE");
                bcp.ColumnMappings.Add("MDA_KA", "MDA_KA");
                bcp.ColumnMappings.Add("MDA_KF", "MDA_KF");
                bcp.ColumnMappings.Add("MDA_K0", "MDA_K0");
                bcp.ColumnMappings.Add("MDA_F_BEGIN", "MDA_F_BEGIN");
                bcp.ColumnMappings.Add("MDA_F_END", "MDA_F_END");
                bcp.ColumnMappings.Add("MDA_ECN_BEGIN", "MDA_ECN_BEGIN");
                bcp.ColumnMappings.Add("MDA_EN_BEGIN", "MDA_EN_BEGIN");
                bcp.ColumnMappings.Add("MDA_ECN_END", "MDA_ECN_END");
                bcp.ColumnMappings.Add("MDA_EN_END", "MDA_EN_END");
                bcp.ColumnMappings.Add("MDA_ANGLE", "MDA_ANGLE");

                try
                {
                    bcp.WriteToServer(reader);
                }
                catch(Exception e) { }                
			}

            return entities;
		}
	}
}
