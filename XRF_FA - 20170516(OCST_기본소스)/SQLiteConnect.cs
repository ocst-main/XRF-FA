using IniParser;
using IniParser.Model;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace XRF_FA
{
    public class SQLiteConnect
    {
        string SQLiteIniPath = Application.StartupPath + @"\Config\SQLite.ini";
        string FileDirectory = string.Empty;
        string FolderDirectory = string.Empty;
        string SQLiteFolderName = "SQLiteDB";
        string ParentFolderName = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string FileName = string.Empty;
        FileIniDataParser parser = new FileIniDataParser();

        string[] TableCheck = { "TB_XRF_FA_DIRECT", "TB_XRF_HISTORY", "TB_XRF_SEQ_TEMP", "TB_XRF_AUTO_HISTORY" };

        private SQLiteConnection _SQLiteCnn = new SQLiteConnection();
        public SQLiteConnection SQLiteCnn
        {
            get
            {
                if (_SQLiteCnn.State == ConnectionState.Closed)
                {
                    Open();
                }
                return _SQLiteCnn;
            }
        }

        private static SQLiteConnect _instance;
        public static SQLiteConnect Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SQLiteConnect();
                }
                return _instance;
            }
        }

        #region [ Init DB Structure ]
        private void InitSqlTable()
        {
            Open();
            string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS ""TB_XRF_AUTO_HISTORY"" (
                ""XRFHSEQ"" INTEGER NULL,
                ""SMPLNO"" INTEGER NULL,
                ""TMBDIV"" INTEGER NULL,
                ""ELEMENTNAME"" INTEGER NULL,
                ""ELEMENTVALUE"" INTEGER NULL,
                ""FBDIV"" INTEGER NULL,
                ""WCDDIV"" INTEGER NULL,
                ""XRFDATE"" INTEGER NULL,
                ""EXNAME"" INTEGER NULL,
                ""RESOURCEVALUE"" INTEGER NULL
            );

            CREATE TABLE IF NOT EXISTS ""TB_XRF_SEQ_TEMP""(
                ""SMPLNO"" INTEGER NULL,
                ""TMBDIV"" INTEGER NULL,
                ""SUJI"" INTEGER NULL,
                ""SMPLLENGTH"" INTEGER NULL,
                ""EXNAME"" INTEGER NULL,
                ""RECHECK"" INTEGER NULL
            );

            CREATE TABLE IF NOT EXISTS ""TB_XRF_FA_DIRECT""(
                ""SMPLNO"" INTEGER NULL,
                ""TMBDIV"" INTEGER NULL,
                ""CARVENUMBER"" INTEGER NULL,
                ""SMPLLENGTH"" INTEGER NULL,
                ""SUJI"" INTEGER NULL,
                ""PROCESSDIV"" INTEGER NULL,
                ""LOADER"" INTEGER NULL,
                ""LOADER_DATE"" INTEGER NULL
            );

            CREATE TABLE IF NOT EXISTS ""TB_XRF_HISTORY""(
                ""XRFHSEQ"" INTEGER NULL,
                ""SMPLNO"" INTEGER NULL,
                ""TMBDIV"" INTEGER NULL,
                ""XRFFRONTW"" INTEGER NULL,
                ""CRFRONTW"" INTEGER NULL,
                ""EXT1FRONTW"" INTEGER NULL,
                ""FEFRONTW"" INTEGER NULL
            );";
            ExecuteNonQuery(createTableQuery);
        }
        #endregion

        #region [ Close Current DB and Create New ]
        public void CreateNewDb()
        {
            IniData data = parser.ReadFile(SQLiteIniPath);
            DbFilenameDialog dbFilenameDialog = new DbFilenameDialog(data["CONFIG"]["FILENAME"], "Cancel");
            dbFilenameDialog.ShowDialog();
            if (dbFilenameDialog.IsCancel == false)
            {
                FileName = $@"{dbFilenameDialog.Filename}";
                dbFilenameDialog.Dispose();
                ParentFolderName = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                FileDirectory = Path.Combine(FolderDirectory, SQLiteFolderName, ParentFolderName, FileName);
                Directory.CreateDirectory(Path.Combine(FolderDirectory, SQLiteFolderName));
                Directory.CreateDirectory(Path.Combine(FolderDirectory, SQLiteFolderName, ParentFolderName));
                data["CONFIG"]["DIRECTORY"] = Path.Combine(FolderDirectory, SQLiteFolderName, ParentFolderName);
                data["CONFIG"]["FILENAME"] = FileName;
                parser.WriteFile(SQLiteIniPath, data);
                Close();
                InitSqlTable();
            }
        }
        #endregion

        #region [ Check DB File & Structure ]
        public void CheckDB()
        {
            IniData data = parser.ReadFile(SQLiteIniPath);
            string InitDirectory = data["CONFIG"]["DIRECTORY"];
            string InitFileName = data["CONFIG"]["FILENAME"];
            FileDirectory = Path.Combine(InitDirectory, InitFileName);
            if (!File.Exists(FileDirectory))
            {
                DialogResult dialog = MessageBox.Show("Database location is not found. Do you want to create new database?" +
                    "\n[Yes]       : Create new database." +
                    "\n[No]       : Select database file." +
                    "\n[Cancel]: Exit program.", "디렉토리", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
                if (dialog == DialogResult.Yes)
                {
                    CreateAndInit();
                }
                else if (dialog == DialogResult.No)
                {
                    OpenFile();
                }
                else
                {
                    Application.Exit();
                    Environment.Exit(0);
                }
            }
            else
            {
                CheckStructure();
            }
        }
        #endregion

        #region [ Create New File With Init structure ]
        public void CreateAndInit()
        {
            IniData data = parser.ReadFile(SQLiteIniPath);
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Where do you want to save database?";
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    FolderDirectory = fbd.SelectedPath;
                }
            }
            if (FolderDirectory.Length == 0)
            {
                Application.Exit();
                Environment.Exit(0);
            }
            DbFilenameDialog dbFilenameDialog = new DbFilenameDialog(data["CONFIG"]["FILENAME"], "Exit");
            dbFilenameDialog.ShowDialog();
            FileName = $@"{dbFilenameDialog.Filename}";
            dbFilenameDialog.Dispose();
            ParentFolderName = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            FileDirectory = Path.Combine(FolderDirectory, SQLiteFolderName, ParentFolderName, FileName);
            Directory.CreateDirectory(Path.Combine(FolderDirectory, SQLiteFolderName));
            Directory.CreateDirectory(Path.Combine(FolderDirectory, SQLiteFolderName, ParentFolderName));
            data["CONFIG"]["DIRECTORY"] = Path.Combine(FolderDirectory, SQLiteFolderName, ParentFolderName);
            data["CONFIG"]["FILENAME"] = FileName;
            parser.WriteFile(SQLiteIniPath, data);
            InitSqlTable();
        }
        #endregion

        #region [ Open DB File ]
        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Database Files|*.db;*.sqlite;";
            DialogResult result = openFileDialog.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                FileDirectory = openFileDialog.FileName;
            }
            else
            {
                Application.Exit();
                Environment.Exit(0);
            }
            CheckStructure();
        }
        #endregion

        #region [ Check DB File Structure ]
        private void CheckStructure()
        {
            string query = "SELECT NAME FROM SQLITE_MASTER";
            DataTable AvaiTable = ExecuteDataTable(query);
            foreach (string item in TableCheck)
            {
                var list = AvaiTable.Rows.OfType<DataRow>().Select(dr => dr.Field<string>("name")).ToList();
                if (!list.Contains(item))
                {
                    MessageBox.Show($@"Table {item} is not found in Database. You need to create a new database", "디렉토리", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
                    CreateAndInit();
                    return;
                }
            }
            IniData data = parser.ReadFile(SQLiteIniPath);
            data["CONFIG"]["DIRECTORY"] = FileDirectory;
            parser.WriteFile(SQLiteIniPath, data);
        }
        #endregion

        #region [ Open & Close Connection ]
        public void Open()
        {
            _SQLiteCnn = new SQLiteConnection($@"Data Source={FileDirectory}; Version = 3;");
            try
            {
                _SQLiteCnn.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SqLite Error");
            }
        }

        public void Close()
        {
            try
            {
                _SQLiteCnn.Close();
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
            if (_SQLiteCnn.State == ConnectionState.Closed)
            {
                Open();
            }
            try
            {
                SQLiteCommand command = _SQLiteCnn.CreateCommand();
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
            if (_SQLiteCnn.State == ConnectionState.Closed)
            {
                Open();
            }
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd = _SQLiteCnn.CreateCommand();
            sqlite_cmd.CommandText = query;

            sqlite_datareader = sqlite_cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(sqlite_datareader);
            return dt;
        }
        #endregion
    }
}
