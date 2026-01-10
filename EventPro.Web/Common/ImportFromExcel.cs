using System;
using System.Data;
using System.Data.OleDb;

namespace EventPro.Web.Common
{
    public static class ImportFromExcel
    {
        public static DataSet ImportDataFromExcel(string file, string strconnection)
        {
            try
            {
                OleDbConnection olecn = new OleDbConnection(strconnection + "Data Source=" + file.ToString() + " ;Excel 12.0;HDR=Yes;IMEX=1");
                OleDbCommand olecmd = new OleDbCommand("SELECT distinct * from [sheet1$]", olecn);
                OleDbDataAdapter olead = new OleDbDataAdapter(olecmd);
                DataSet ds = new DataSet();
                olead.Fill(ds);
                return ds;
            }
            catch
            {
                throw;
            }

        }

        public static String[] GetExcelSheetNames(string excelFile, string excelcon)
        {
            OleDbConnection objConn = null;
            DataTable dt = null;

            try
            {
                // Connection String. Change the excel file to the file you
                string strconnection = excelcon;
                strconnection = strconnection + "Data Source=" + excelFile + " ;Excel 12.0;HDR=Yes;IMEX=1";
                String connString = strconnection;
                objConn = new OleDbConnection(connString);
                objConn.Open();
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return null;
                }

                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;

                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString();
                    i++;
                }

                // Loop through all of the sheets if you want too...
                for (int j = 0; j < excelSheets.Length; j++)
                {
                    // Query each excel sheet.
                }

                return excelSheets;
            }
            catch
            {
                throw;
            }
            finally
            {
                // Clean up.
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }
    }
}
