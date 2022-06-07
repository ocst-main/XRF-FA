using System;
using System.Windows.Forms;

namespace XRF_FA
{
    public partial class DbFilenameDialog : Form
    {
        public string Filename { get; set; }
        public bool IsCancel { get; set; }

        public DbFilenameDialog(string InitFileName, string BtnFunction)
        {
            InitializeComponent();
            TxbDbName.Text = InitFileName;
            BtnExit.Text = BtnFunction;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Filename = TxbDbName.Text;
            IsCancel = false;
            this.Close();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            if (BtnExit.Text == "Exit")
            {

                Application.Exit();
                Environment.Exit(0);
            }
            else
            {
                IsCancel = true;
                this.Close();
            }
        }

    }
}
