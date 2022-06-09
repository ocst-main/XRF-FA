using IniParser;
using IniParser.Model;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace XRF_FA
{
    public class MssqlConnect
    {
        string MssqlIniPath = Application.StartupPath + @"\Config\Mssql.ini";
        FileIniDataParser parser = new FileIniDataParser();

        string DATABASEIP = string.Empty;
        string DATABASE = string.Empty;
        string USER = string.Empty;
        string PASSWORD = string.Empty;

        private SqlConnection _MssqlCnn = new SqlConnection();
        public SqlConnection MssqlCnn
        {
            get
            {
                if (_MssqlCnn.State == ConnectionState.Closed)
                {
                    Open();
                }
                return _MssqlCnn;
            }
        }

        private static MssqlConnect _instance;
        public static MssqlConnect Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MssqlConnect();
                }
                return _instance;
            }
        }

        #region [ Check DB File & Structure ]
        public void CheckDB()
        {
            IniData data = parser.ReadFile(MssqlIniPath);
            DATABASEIP = data["CONFIG"]["DATABASEIP"];
            DATABASE = data["CONFIG"]["DATABASEIP"];
            USER = data["CONFIG"]["USER"];
            PASSWORD = data["CONFIG"]["PASSWORD"];
            bool CnnStatus = Open();

        }
        #endregion

        #region [ Open & Close Connection ]
        public bool Open()
        {
            if (_MssqlCnn.State == ConnectionState.Closed)
            {
                IniData data = parser.ReadFile(MssqlIniPath);
                DATABASEIP = data["CONFIG"]["DATABASEIP"];
                DATABASE = data["CONFIG"]["DATABASEIP"];
                USER = data["CONFIG"]["USER"];
                PASSWORD = data["CONFIG"]["PASSWORD"];
                string cnn = $@"Data Source={DATABASEIP};Initial Catalog={DATABASE};Persist Security Info=True;User ID={USER};Password={PASSWORD}";
                _MssqlCnn = new SqlConnection(cnn);
                try
                {
                    _MssqlCnn.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "MSSQL Error");
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void Close()
        {
            try
            {
                _MssqlCnn.Close();
            }
            catch
            {

            }

        }
        #endregion

        #region [ ExecuteNonQuery ]
        /// <summary>
        /// Execute query command without get result
        /// </summary>
        /// <param name="query">string Query Command</param>
        /// <returns></returns>
        public bool ExecuteNonQuery(string query)
        {
            if (_MssqlCnn.State == ConnectionState.Closed)
            {
                Open();
            }
            try
            {
                SqlCommand command = _MssqlCnn.CreateCommand();
                command.CommandText = query;
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SqLite Error");
                return false;
            }
        }
        #endregion

        #region [ ExecuteDataTable ]
        /// <summary>
        /// Execute query and get DataTable result
        /// </summary>
        /// <param name="query"> Query command</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string query)
        {
            if (_MssqlCnn.State == ConnectionState.Closed)
            {
                Open();
            }

            SqlDataReader sqlite_datareader;
            SqlCommand sqlite_cmd = _MssqlCnn.CreateCommand();
            sqlite_cmd.CommandText = query;

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(sqlite_datareader);
            return dt;
        }
        #endregion
    }
}
