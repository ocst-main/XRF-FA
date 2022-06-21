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
        readonly string MssqlIniPath = Application.StartupPath + @"\Config\Mssql.ini";
        readonly FileIniDataParser parser = new FileIniDataParser();

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

        #region [ Check DB Connection ]
        public void CheckDB()
        {
            IniData data = parser.ReadFile(MssqlIniPath);
            DATABASEIP = data["CONFIG"]["DATABASEIP"];
            DATABASE = data["CONFIG"]["DATABASEIP"];
            USER = data["CONFIG"]["USER"];
            PASSWORD = data["CONFIG"]["PASSWORD"];
            bool CnnStatus = Open();
            if (CnnStatus == false)
            {
                DialogResult dialogResult = MessageBox.Show("Can not connect to MSSQL Server. Please verify the connection in Mssql.ini file config.\n[OK] to skip MSSQL.\n[CANCEL] to exit program.", "MSSQL Connection", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.OK)
                {

                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    Application.Exit();
                    Environment.Exit(1);
                }

            }
        }
        #endregion

        #region [ Open & Close Connection ]
        public bool Open()
        {
            if (_MssqlCnn.State == ConnectionState.Closed)
            {
                IniData data = parser.ReadFile(MssqlIniPath);
                DATABASEIP = data["CONFIG"]["DATABASEIP"];
                DATABASE = data["CONFIG"]["DATABASE"];
                USER = data["CONFIG"]["USER"];
                PASSWORD = data["CONFIG"]["PASSWORD"];
                string cnn = $@"Data Source={DATABASEIP};Initial Catalog={DATABASE};Persist Security Info=True;User ID={USER};Password={PASSWORD};MultipleActiveResultSets=true;";
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

    }
}
