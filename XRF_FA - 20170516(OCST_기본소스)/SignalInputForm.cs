using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using opcNet.IF.SEM;
namespace XRF_FA
{
    public partial class SignalInputForm : Form
    {
        private int iFlag = 0;  // 0:XRF_GROUP01, 1:XRF_CONV_LOC, 2:XRF_BUFFER
        private opcMgrClass m_opcSvr = null;
        private string sGroupName = string.Empty;
        private MainForm m_frmMain = null;

        public SignalInputForm(opcMgrClass opcSvr, int index, MainForm frm)
        {
            InitializeComponent();
            m_opcSvr = opcSvr;
            m_frmMain = frm;

            rb1.Checked = true;

            TagNameLoad(0);
        }

        private void TagNameLoad(int iFlag)
        {

            switch (iFlag)
            {
                case 2:
                    sGroupName = "XRF_GROUP01";
                    txtData.Visible = false;
                    cmbData.Visible = true;
                    cmbData.Items.Clear();
                    cmbData.Items.Add("On");
                    cmbData.Items.Add("Off");
                    break;
                case 1:
                    sGroupName = "XRF_BUFFER";
                    txtData.Visible = true;
                    cmbData.Visible = false;
                    break;
                case 0:
                    sGroupName = "XRF_CONV_LOC";
                    txtData.Visible = true;
                    cmbData.Visible = false;
                    break;
                case 3:             // X2
                    sGroupName = "XRF_GROUP02";
                    txtData.Visible = false;
                    cmbData.Visible = true;
                    cmbData.Items.Clear();
                    cmbData.Items.Add("On");
                    cmbData.Items.Add("Off");
                    break;
            }

            PLCDefine plcXml = new PLCDefine();
            plcXml.XmlParserTAG(sGroupName, cmbTag);
            cmbTag.SelectedIndex = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                object[] oWriteData = null;
                oWriteData = new object[1];

                int[] iTagHandle = new int[1];
                iTagHandle[0] = cmbTag.SelectedIndex;

                switch (iFlag)
                {
                    case 3:
                    case 2:
                        if (cmbData.Text == "On")
                        {
                            oWriteData[0] = true;
                        }
                        else
                        {
                            oWriteData[0] = false;
                        }
                        break;
                    case 1:
                    case 0:
                        oWriteData[0] = txtData.Text;
                        m_frmMain.sSPL[cmbTag.SelectedIndex] = txtData.Text;
                        m_frmMain.sbBufGridAppl[cmbTag.SelectedIndex] = txtApplication.Text;
                        break;
                }


                int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcSvr, sGroupName, ref iTagHandle, ref oWriteData);
                if (iResFunc != 1)
                {
                    msgLabel.Text = "Test Data Send Fail " + iResFunc;
                }
                else
                {
                    msgLabel.Text = "Test Data Send OK";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rb1_CheckedChanged(object sender, EventArgs e)
        {
            if (rb1.Checked)
            {
                lblApplication.Visible = false;
                txtApplication.Visible = false;
                TagNameLoad(0);
                iFlag = 0;
            }
            if (rb2.Checked)
            {
                lblApplication.Visible = true;
                txtApplication.Visible = true;
                TagNameLoad(1);
                iFlag = 1;
            }
            if (rb3.Checked)
            {
                lblApplication.Visible = false;
                txtApplication.Visible = false;
                TagNameLoad(2);
                iFlag = 2;
            }
            if (rb4.Checked)
            {
                lblApplication.Visible = false;
                txtApplication.Visible = false;
                TagNameLoad(3);
                iFlag = 3;
            }
        }
    }
}
