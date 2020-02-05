using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;


//using EFOptimization.DataModel;

namespace SpecialJobs.Helpers
{
	public static class Update
	{
		//public static void BySaveChanges(int count)
		//{
		//	//DatabaseOps.ClearPrimaryTable();
		//	Insert.BySqlBulkCopy(count, false);            

  //          using (var context = new MethodsEntities())
		//	{
		//		var entities = context.Orders.ToList();

		//		entities.ForEach(entity => entity.Date = DateTime.Now.AddYears(666));

		//		var stopwatch = new Stopwatch();
		//		stopwatch.Start();

		//		context.SaveChanges();
				
		//		stopwatch.Stop();

		//		Console.WriteLine("SaveChanges({0}) = {1} s", count, stopwatch.Elapsed.TotalSeconds);
		//	}
		//}

		//public static void ByMerge(int count)
        public static void CopyAndMerge(List<MEASURING_DATA> entities)
        {
            try
            {
                DatabaseOps.CreateTempTable();
                //DatabaseOps.ClearTempTable();
            }
            catch (Exception e)
            { MessageBox.Show("Ошибка создания временной таблицы.   " + e.Message); }
            try
            {
                Insert.BulkCopy(entities, "MEASURING_DATA_TEMP");
            }
            catch (Exception e)
            { MessageBox.Show("Ошибка копирования во временную таблицу.   " + e.Message); }
            try
            {
                DatabaseOps.Merge();
            }
            catch (Exception e)
            { MessageBox.Show("Ошибка слияния.   " + e.InnerException.Message); }

            try
            {
                DatabaseOps.DropTempTable();
            }
            catch (Exception e)
            { MessageBox.Show("Ошибка удаления временной таблицы.   " + e.Message); }

        }
    }
}
