using C1.Win.C1FlexGrid;
using IniParser;
using IniParser.Model;
using Ino.FrameWork;
using opcNet.IF.SEM;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace XRF_FA
{
    public partial class MainForm : Form
    {
        FileIniDataParser parser = new FileIniDataParser();

        #region [ API ]
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(    // GetIniValue 를 위해
            String section,
            String key,
            String def,
            StringBuilder retVal,
            int size,
            String filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);

        #endregion

        #region [ 멤버객체 및 변수 ]
        private SqlConnection dbConn = null;                      // Oracle 접속 객체
        public static bool ConnectFlag = false;                     // UAI 접속 상태
        private bool m_bFirstTest = true;							// 시험의 처음실행 체크 변수
        private opcMgrClass m_opcMgr = null;                        // OPC 관련 객체
        private Socket client = null;                               // Pan UAI Client 객체
        private Socket client_X2 = null;                            // BRUKER Meas8m Client 객체
        private System.Timers.Timer timer;                          // 정상 통신 설정 (ENQ 신호 전송 타이머)
        private System.Timers.Timer timer_X2;                       // 시험기 상태 STATUS 동작 타이머
        //private Queue<string> ListQueue = new Queue<string>();
        private static string Ver = "[Version 2.04 - 2016-12-29.002]";
        private bool closeFlag = false;

        #region [ 파일경로 경로 ]
        private string sCnfgPath = Application.StartupPath + @"\Config\UsrAppConf.xml";         // OPC Config XMP 파일 Path
        private string sIniPath = Application.StartupPath + @"\Config\ServerIP.ini";            // XRF P/C IP Address
        private string sIniAppl = Application.StartupPath + @"\Config\Application.ini";         // XRF Application Name
        private string sErrLog = Application.StartupPath + @"\Log\";                            // Error Log File
        private string sStartUpIni = Application.StartupPath + @"\Config\StartUp.ini";          // 버퍼 Clear가 정상적으로 되었는지 확인.
        #endregion

        private int removeCnt = 0;
        private int removeMok = 0;
        private int minQty = 3;
        private string sTmpLength = string.Empty;
        private bool m_firstSend = false;
        private string m_DequeData = string.Empty;
        private string m_DequeData_X2 = string.Empty; // for BRUKER
        private int m_BufferCount = 0;
        private int m_BufferCount_X2 = 0; // for BRUKER
        private bool m_ChkAutoStart = false;                                                    // FA 자동시작시 Panalytical 체크박스 상태
        private bool m_ChkAutoStart_X2 = false;                                                 // FA 자동시작시 BRUKER 체크박스 상태
        // Start Flag 가 False면 소재정보 요구가 와도 소재정보를 보내지 않는다.
        // Start Flag는 시작버튼을 누르면 True Stop 버튼을 누르면 False이다.
        private bool m_bFlagStart = false;
        private bool m_bFlagStart_X2 = false; // for BRUKER
        private string sSendMsg_X2 = string.Empty; // only for BRUKER
        private int m_MinBufferSampleCount = 1;                                                 // 동작중 최소한 있어야 하는 샘플 개수
        private int X2minqty = 2;                                                               // 처음시작할때 최소한 있어야 하는 샘플 개수
        private string sLastTestResultSML = string.Empty;                                       // 마지막으로 시험 결과를 읽은 시편번호

        public string[] sSPL = new string[132];
        public string[] sbBufGridAppl = new string[132];

        public static string sSPL_tmp = string.Empty;
        public static string sbBufGridAppl_tmp = string.Empty;
        public static int nSML_POS = 0;

        public static bool bX2Proc_Start = false;
        public static int nX2ProcNum = 0;                                                       // 진행되는 BRUKER 프로세스 갯수 최대 3개
        public static int nX2TGTNum = 0;                                                        // 검사를 진행할 시편의 갯수
        public static int nX2ResultNum = 0;                                                     // 실제 진행한 시편의 갯수
        public string sUnloadSMLName = string.Empty;
        public int nIncNum = 0;
        public string sWorkingSMLName = string.Empty;                                           // 명령을 수행하는 시편의 이름

        public bool[] bAStatus_X2 = new bool[15] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };

        const int nBufferNumber = 132;

        private DataTable m_dtFaInfo = null;                                                    // 자동화장비 진행정보 테이블 
        private DataTable m_dtOrderList = null;                                                 // 지시리스트 저장 테이블
        private DataTable m_dtBufferInfo = null;                                                // 버퍼정보 저장 테이블
        string m_strFilePath = Application.StartupPath + @"\DataInfo\";                         // XML 저장경로
        string m_strBufferFileName = "BufferInfo.xml";                                          // 버퍼 XML 파일명
        string m_strOrderListFileName = "OrderList.xml";                                        // 지시리시트 파일명
        Dictionary<string, string> dicSampleInfo = new Dictionary<string, string>();            // 자동화 진행시 시편번호별 프로그램명 리스트
        List<sSML_STATUS> m_TestList = new List<sSML_STATUS>();

        public static bool bInitState = true;                                                   // Start 버튼 클릭시 작업완료시 초기화 여부를 설정한다

        #endregion

        #region [ BRUKER 시험기에 시험중인 시편정보 타입(sSML_STATUS) ]
        public struct sSML_STATUS
        {
            public string sSPLNAME;
            public string sPROGRAMNAME;
            public int nBufferIndex;

            public sSML_STATUS(string sSPLNAME, string sPROGRAMNAME, int nBufferIndex)
            {
                this.sSPLNAME = sSPLNAME;
                this.sPROGRAMNAME = sPROGRAMNAME;
                this.nBufferIndex = nBufferIndex;
            }
        }
        public sSML_STATUS[] mysSML_STATUS = new sSML_STATUS[3] {
            new sSML_STATUS(string.Empty, string.Empty, 0),
            new sSML_STATUS(string.Empty, string.Empty, 0),
            new sSML_STATUS(string.Empty, string.Empty, 0)
        };
        #endregion

        #region [MainForm : 생성자]
        public MainForm()
        {
            Cursor.Current = Cursors.WaitCursor;

            InitializeComponent();
            InitBufferDataTable();          // 버퍼정보를 가지고 있는 데이터 테이블 생성
            InitOrderListDataTable();       // OrderList정보를 가지고 있는 데이터 테이블 생성

            // 종료시 opcTerminate()가 호출되지 않았으면 호출을 보장하기 위해 Event 핸들러 추가
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);

            // 기본 메인폼 환경 설정
            this.Text = "  XRF_Factory Automation " + Ver;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.WindowState = FormWindowState.Maximized;
            this.MaximizeBox = false;

            tabControl1.TabIndex = 0;

            #region [ Oracle DataBase 접속 ]
            // DataBase ConnectionString Loading
            dbConn = MssqlConnect.Instance.MssqlCnn;
            if (dbConn == null)
            {
                MessageBox.Show("Database연결에 실패하였습니다.\r\n프로그램을 종료합니다.", "Database 접속 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Cursor.Current = Cursors.Default;
                Application.Exit();
            }
            #endregion
            #region [ OPC 서버 실행 체크 및 접속 ]
            //// Remark for Coding 20160927 !           
            // OPC 서버 프로그램이 실행되고 있으면 종료한후 다시 실행한다.
            foreach (Process p in Process.GetProcessesByName("OPCWorkX"))
            {
                if (!p.HasExited)
                {
                    p.Kill();
                }
                p.Close();
            }
            if (!OPCRegServer())
            {
                MessageBox.Show("OPC서버 연결에 실패하였습니다.\nOPCWorkX 프로그램을 실행한후 프로그램을 실행하세요.", "OPC 접속 오류", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cursor.Current = Cursors.Default;
                Application.Exit();
            }
            #endregion
            #region [ BRUKER Meas8m 접속 ]
            ////////////////////////////////////////////////////////////////////////////
            //// Remark for Coding 20160927 !
            IniData data = parser.ReadFile(sIniPath);
            XrfControl.m_Xrf_IP_X2 = data["SERVER_X2"]["IP"];

            IPAddress[] ipAddress_X2 = Dns.GetHostAddresses(XrfControl.m_Xrf_IP_X2);
            IPEndPoint remoteEP_X2 = new IPEndPoint(ipAddress_X2[0], 1904);

            Thread.Sleep(1000);

            // Create a TCP/IP socket.
            client_X2 = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            bool bConnect = SocketClient.Connect_X2(remoteEP_X2, client_X2);
            if (bConnect)
            {
                lbXrf2Stat.Text = "X2 Connected";
                lbXrf2Stat.BackColor = Color.LightGreen;
                Receive_X2(client_X2);
            }
            else
            {
                lbXrf2Stat.Text = "X2 DisConnected";
                lbXrf2Stat.BackColor = Color.Transparent;
            }
            //////////////////////////////////////////////////////////////////////////
            #endregion
            #region [ Panalytical UAI 접속 ]
            ////////////////////////////////////////////////////////////////////////////////
            //// Remark for Coding 20160927 !
            XrfControl.m_Xrf_IP = data["SERVER_X1"]["IP"];

            IPAddress[] ipAddress = Dns.GetHostAddresses(XrfControl.m_Xrf_IP);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress[0], 701);

            Thread.Sleep(1000);

            // Create a TCP/IP socket.
            client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            SocketClient.Connect(remoteEP, client);

            Receive(client);

            XRF_COMMAND xCmd = new XRF_COMMAND();
            string cmd = xCmd.XRF_MAKE_SEND_COMMAND_SYSTEM_STATUS();
            XRF_Send_MSG(cmd);
            ////////////////////////////////////////////////////////////////////////////////
            #endregion

            InitFormatGrid();   // 그리드 초기화
            InitCombo();        // ComboBox Binding! 

            DataTable dtTemp = new DataTable("BUFFERINFO");
            dtTemp.Columns.Add("버퍼정보");
            dtTemp.Columns.Add("시편정보");
            dtTemp.Columns.Add("XRF프로그램명");
            dtTemp.ReadXml(m_strFilePath + m_strBufferFileName);
            m_dtBufferInfo = dtTemp;

            for (int iRow = 0; iRow < m_dtBufferInfo.Rows.Count; iRow++)
            {
                if (!string.IsNullOrEmpty(m_dtBufferInfo.Rows[iRow]["시편정보"].ToString()))
                {
                    if (!dicSampleInfo.ContainsKey(m_dtBufferInfo.Rows[iRow]["시편정보"].ToString().Substring(0, 8).Trim().ToUpper()))
                    {
                        dicSampleInfo.Add(m_dtBufferInfo.Rows[iRow]["시편정보"].ToString().Substring(0, 8).Trim().ToUpper(), m_dtBufferInfo.Rows[iRow]["XRF프로그램명"].ToString().ToUpper());
                    }
                    else
                    {
                        dicSampleInfo[m_dtBufferInfo.Rows[iRow]["시편정보"].ToString().Substring(0, 8).Trim().ToUpper()] = m_dtBufferInfo.Rows[iRow]["XRF프로그램명"].ToString().ToUpper();
                    }
                    sSPL[iRow] = m_dtBufferInfo.Rows[iRow]["시편정보"].ToString();
                    sbBufGridAppl[iRow] = m_dtBufferInfo.Rows[iRow]["XRF프로그램명"].ToString();
                }
            }

            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);  //주기마다 실행되는 이벤트 등록
            //timer.Enabled = false;

            lblXrfApplication.Text = string.Empty;
            Cursor.Current = Cursors.Default;

            // for BRUKER
            timer_X2 = new System.Timers.Timer();
            timer_X2.Interval = 2000;
            timer_X2.Elapsed += new System.Timers.ElapsedEventHandler(timer_X2_Elapsed);  //주기마다 실행되는 이벤트 등록

#if TEST
                this.Text += " [" + "Test Version" + "]";
                MessageBox.Show("프로그램이 TEST MODE로 실행중입니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif
            ////////////////////////////////////////////////////////////////////////////////////////
        }
        #endregion

        delegate void TimerEventFiredDelegate();
        delegate void TimerEventFiredDelegate_X2();
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            BeginInvoke(new TimerEventFiredDelegate(timer_Work));
        }
        void timer_X2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            BeginInvoke(new TimerEventFiredDelegate(timer_Work_X2));
        }
        private void timer_Work()
        {
            if (StateObject.Send_ENQ)
            {
                if (!StateObject.Recv_ACK)
                {
                    XRF_Send_ENQ();
                }
            }
        }
        private void timer_Work_X2()
        {
            XRF_COMMAND xCmd_X2 = new XRF_COMMAND();
            string cmd_X2 = string.Empty;

            cmd_X2 = xCmd_X2.XRF_BRUKER_STATUS();
            XRF_Send_MSG_X2(cmd_X2, false);
        }

        #region [ 컨트롤 이벤트 ]

        #region [ 폼 활성화 이벤트(MainForm_Activated) ]
        private void MainForm_Activated(object sender, EventArgs e)
        {
            //if (!SocketClient.ConnectFlag)
            //{
            //    MessageBox.Show("프로그램을 종료합니다.", "SuperQ 접속 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    Cursor.Current = Cursors.Default;
            //    this.Close();
            //    Application.Exit();
            //}
        }
        #endregion
        #region [ 폼로드(MainForm_Load) ]
        private void MainForm_Load(object sender, EventArgs e)
        {
            #region [ 저장된 지시리스트 로드하여 화면에 표시 ]
            DataTable dtTemp = new DataTable("ORDERLIST");
            dtTemp.Columns.Add("SEQ");
            dtTemp.Columns.Add("시편번호");
            dtTemp.Columns.Add("TMB");
            dtTemp.Columns.Add("길이");
            dtTemp.Columns.Add("취소", typeof(bool));
            dtTemp.Columns.Add("세척유무", typeof(bool));
            dtTemp.Columns.Add("DivType");
            dtTemp.Columns.Add("시험기");
            dtTemp.Columns.Add("Application");
            dtTemp.ReadXml(m_strFilePath + m_strOrderListFileName);
            m_dtOrderList = dtTemp;

            for (int iRow = 0; iRow < m_dtOrderList.Rows.Count; iRow++)
            {
                grdNo.Rows.Count += 1;
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["SEQ"].Index, m_dtOrderList.Rows[iRow]["SEQ"]);
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["시편번호"].Index, m_dtOrderList.Rows[iRow]["시편번호"]);
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["TMB"].Index, m_dtOrderList.Rows[iRow]["TMB"]);
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["길이"].Index, m_dtOrderList.Rows[iRow]["길이"]);
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["취소"].Index, m_dtOrderList.Rows[iRow]["취소"]);
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["세척유무"].Index, m_dtOrderList.Rows[iRow]["세척유무"]);
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["DivType"].Index, m_dtOrderList.Rows[iRow]["DivType"]);
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["시험기"].Index, m_dtOrderList.Rows[iRow]["시험기"]);
                grdNo.SetData(grdNo.Rows.Count - 1, grdNo.Cols["Application"].Index, m_dtOrderList.Rows[iRow]["Application"]);
            }
            #endregion
        }
        #endregion
        #region [ 폼이 닫히기전 이벤트(MainForm_FormClosing) ]
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (e.CloseReason != CloseReason.ApplicationExitCall)
                {
                    if (DialogResult.OK != MessageBox.Show("프로그램을 종료 하시겠습니까?", "알림", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1))
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        if (!closeFlag)
                        {
                            if (dbConn.State != ConnectionState.Closed)
                            {
                                dbConn.Close();
                                dbConn.Dispose();
                            }
                            if (dbConn != null)
                            {
                                dbConn = null;
                            }

                            e.Cancel = true;
                            closeFlag = true;
                        }
                        tsmExit_Click(tsmExit, null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region [ 시험지시 그리드관련(grdNo) ]

        #region [ 시험지시 리스트의 로우가 삭제 되었을때(grdNo_AfterDeleteRow)  ]
        private void grdNo_AfterDeleteRow(object sender, RowColEventArgs e)
        {
            OrderListUpdate();
        }
        #endregion
        #region [ 시험지시 셀값이 변경되었을때(grdNo_CellChanged) ]
        /// <summary>
        /// 시험기 X1, X2가 변경되었을때 시험프로그램명 양식 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdNo_CellChanged(object sender, RowColEventArgs e)
        {
            if (grdNo.Cols[e.Col].Name.Equals("시험기"))
            {
                switch (grdNo.GetDataDisplay(e.Row, e.Col))
                {
                    // 모든 프로그램명은 _(언더바)로 표시한다(2016-12-29 : 김종렬반장)
                    case "X1":
                        //grdNo.SetData(e.Row, "Application", grdNo.GetDataDisplay(e.Row, "Application").Replace("_", "-"));
                        grdNo.SetData(e.Row, "Application", grdNo.GetDataDisplay(e.Row, "Application").Replace("-", "_"));
                        break;
                    case "X2":
                        grdNo.SetData(e.Row, "Application", grdNo.GetDataDisplay(e.Row, "Application").Replace("-", "_"));
                        break;
                }
            }
        }
        #endregion
        #region [ 클립보드 내용을 시험지시 선택 셀에 붙여넣기(grdNo_PreviewKeyDown) ]
        private void grdNo_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                var _flex = sender as C1.Win.C1FlexGrid.C1FlexGrid;
                var _cellrange = _flex.Selection;
                _cellrange.Data = Clipboard.GetText();
            }

        }
        #endregion
        #region [ 지험지시 키 입력(grdNo_KeyDown) ]
        /// <summary>
        /// Insert Key : 선택한 로우 삽입, Delete Key : 선택한 로우 삭제 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdNo_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.KeyCode)
                {
                    case Keys.Insert:
                        if (m_bFlagStart) return;
                        insGrid(grdNo, grdNo.RowSel);
                        break;
                    case Keys.Delete:
                        if (m_bFlagStart) return;
                        GridAutoRowNumber(grdNo, grdNo.RowSel);
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "grdNo_KeyDown");
            }
        }
        #endregion
        #region [ 그리드 체크셀 클릭(grdNo_CellChecked) ]
        private static void grdNo_CellChecked(object sender, RowColEventArgs e)
        {
            C1FlexGrid grid = (sender as C1FlexGrid);
            if (e.Row < grid.Rows.Fixed)
            {
                for (int iRow = grid.Rows.Fixed; iRow < grid.Rows.Count; iRow++)
                {
                    grid.SetCellCheck(iRow, e.Col, grid.GetCellCheck(e.Row, e.Col));
                }
            }
        }

        #endregion

        #endregion

        #region [ 버퍼조정 버튼 클릭 ]

        #region [ X1 버퍼조정 클릭(btnSendBufCount_Click) ]
        private void btnSendBufCount_Click(object sender, EventArgs e)
        {
            object[] oWriteData = new object[] { tbBufInput.Text, tbBufOutput.Text };
            int[] iTagHandle = new int[] { 11, 12 };

            string sGroupName = "XRF_CONV_LOC";
            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, sGroupName, ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                //msgLabel.Text = "Test Data Send Fail " + iResFunc;
            }
            else
            {
                //msgLabel.Text = "Test Data Send OK";
            }
        }
        #endregion
        #region [ X2 버퍼조정 클릭(btnSendBufCountX2_Click) ]
        private void btnSendBufCountX2_Click(object sender, EventArgs e)
        {
            object[] oWriteData = new object[] { tbX2BufInput.Text, tbX2BufOutput.Text };
            int[] iTagHandle = new int[] { 18, 19 };

            string sGroupName = "XRF_CONV_LOC";
            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, sGroupName, ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                //msgLabel.Text = "Test Data Send Fail " + iResFunc;
            }
            else
            {
                //msgLabel.Text = "Test Data Send OK";
            }
        }
        #endregion

        #endregion
        #region [ X1 버퍼투입 수량 변경시(txtX1BufferInputQty_TextChanged) ]
        private void txtX1BufferInputQty_TextChanged(object sender, EventArgs e)
        {
            setText(tbBufInput, txtX1BufferInputQty.Text);
        }
        #endregion
        #region [ X1 버퍼배출 수량 변경시(txtX1OutPutBuffer_TextChanged) ]
        private void txtX1OutPutBuffer_TextChanged(object sender, EventArgs e)
        {
            setText(tbBufOutput, txtX1OutPutBuffer.Text);
        }
        #endregion
        #region [ X2 버퍼투입 수량 변경시(txtX2BufferInputQty_TextChanged) ]
        private void txtX2BufferInputQty_TextChanged(object sender, EventArgs e)
        {
            setText(tbX2BufInput, txtX2BufferInputQty.Text);
        }
        #endregion
        #region [ X2 버퍼배출 수량 변경시(txtX2OutPutBuffer_TextChanged) ]
        private void txtX2OutPutBuffer_TextChanged(object sender, EventArgs e)
        {
            setText(tbX2BufOutput, txtX2OutPutBuffer.Text);
        }
        #endregion
        #region [ 지시 다운로드 클릭(btnOrder_Click) ]
        private void btnOrder_Click(object sender, EventArgs e)
        {
            if (m_bFlagStart)
            {
                MessageBox.Show("기기 가동중에는 지시정보를 불러올수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            RequestOrderList();             // 지시 받아오기
            OrderListUpdate();              // 파일이력 저장
        }
        #endregion
        #region [ 추가지시 가져오기 버튼 클릭(btnOrderAdd_Click) ]
        private void btnOrderAdd_Click(object sender, EventArgs e)
        {
            string[] aData = null;
            CommonDataBase cDb = new CommonDataBase();

            try
            {
                bool bOrderListDupCheck = false;
                SqlDataReader reader = CommonDataBase.GetOrderList(dbConn);
                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        int iRow = grdNo.Rows.Count;
                        while (reader.Read())
                        {
                            bOrderListDupCheck = false;
                            for (int iSearchRow = grdNo.Rows.Fixed; iSearchRow < grdNo.Rows.Count; iSearchRow++)
                            {
                                if (reader.GetValue(0).ToString().Equals(grdNo.GetDataDisplay(iSearchRow, "시편번호")) &&
                                    reader.GetValue(1).ToString().Equals(grdNo.GetDataDisplay(iSearchRow, "TMB")))
                                {
                                    bOrderListDupCheck = true;
                                    break;
                                }
                            }

                            if (bOrderListDupCheck)
                                continue;

                            grdNo.Rows.Count = iRow + 1;
                            grdNo.SetData(iRow, 0, iRow.ToString());
                            grdNo.SetData(iRow, 1, reader.GetValue(0).ToString()); // 시편번호
                            grdNo.SetData(iRow, 2, reader.GetValue(1).ToString()); // TMB
                            grdNo.SetData(iRow, 3, reader.GetValue(6).ToString()); // 길이
                            grdNo.SetData(iRow, 4, false);
                            grdNo.SetData(iRow, 6, reader.GetValue(5).ToString()); // 수지
                            grdNo.SetData(iRow, 7, reader.GetValue(9).ToString()); // 세척유무
                            grdNo.SetData(iRow, 8, reader.GetValue(8).ToString()); // 시험 구분
                            grdNo.SetData(iRow, 9, reader.GetValue(3).ToString()); // 시험기 구분
                            grdNo.SetData(iRow, 12, reader.GetValue(7).ToString().Replace("-", "_")); // NEW 프로그램
                            iRow++;
                        }
                        reader = null;

                        aData = new string[4];
                        aData[0] = "";
                        aData[1] = "";
                        aData[2] = "D";
                        aData[3] = "S";
                        cDb.Execute_BackGroundWorker(aData, 1);  // Data Load를 완료하면 Load한 모든 시편을 'D'로 변경한다.
                        Thread.Sleep(10);
                    }
                    else
                    {
                        MessageBox.Show("시험지시 내용이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("시험지시 내용이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //if (dbConn.State == ConnectionState.Open) dbConn.Close();

            }
        }
        #endregion
        #region [ 종료 메뉴버튼 클릭(tsmExit_Click) ]
        private void tsmExit_Click(object sender, EventArgs e)
        {
            OPCSendSignalClear();
            Thread.Sleep(1000);

            if (m_opcMgr != null)
            {
                m_opcMgr.opcTerminate();
            }

            // OPC Server프로그램 종료한다.
            foreach (Process p in Process.GetProcessesByName("OPCWorkX"))
            {
                if (!p.HasExited)
                {
                    p.Kill();
                }
                p.Close();
            }
            Application.Exit();
        }
        #endregion
        #region [ Meas8m 접속 버튼 클릭(btnX2_Connect_Click) ]
        // for BRUKER
        private void btnX2_Connect_Click(object sender, EventArgs e)
        {
            IPAddress[] ipAddress_X2 = Dns.GetHostAddresses(XrfControl.m_Xrf_IP_X2);
            IPEndPoint remoteEP_X2 = new IPEndPoint(ipAddress_X2[0], 1904);
            if (client_X2.Connected)
            {
                client_X2.Disconnect(true);

                // Create a TCP/IP socket.
                //client_X2 = new Socket(AddressFamily.InterNetwork,
                //    SocketType.Stream, ProtocolType.Tcp);

                bool bConnect = SocketClient.Connect_X2(remoteEP_X2, client_X2);
                if (bConnect)
                {
                    lbXrf2Stat.Text = "X2 Connected";
                    Receive_X2(client_X2);
                }
                else
                {
                    lbXrf2Stat.Text = "X2 DisConnected";
                }
            }
        }
        #endregion
        #region [ UAI 접속버튼 클릭(btnX1_Connect_Click) ]
        private void btnX1_Connect_Click(object sender, EventArgs e)
        {
            IPAddress[] ipAddress = Dns.GetHostAddresses(XrfControl.m_Xrf_IP);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress[0], 701);
            if (client != null && client.Connected)
            {
                client.Disconnect(true);

                // Create a TCP/IP socket.
                //client = new Socket(AddressFamily.InterNetwork,
                //    SocketType.Stream, ProtocolType.Tcp);

                SocketClient.Connect(remoteEP, client);
                Receive(client);

                XRF_COMMAND xCmd = new XRF_COMMAND();
                string cmd = xCmd.XRF_MAKE_SEND_COMMAND_SYSTEM_STATUS();
                XRF_Send_MSG(cmd);
            }
            else
            {
                client = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                SocketClient.Connect(remoteEP, client);
                Thread.Sleep(1000);
                Receive(client);
            }
        }
        #endregion

        #region [ 자동화 장비 작업시작 버튼 클릭(btnActStart_Click) ]
        private void btnActStart_Click(object sender, EventArgs e)
        {
            object[] oWriteData = null;
            int[] iTagHandle = null;

            ///////////////////////////////////////////////////////////
            //// 2016.12.15. 작업완료시 초기화작업 여부를 결정한다 
            if (btnActStart.Text.Trim().ToUpper().Equals("START"))
            {
                if (MessageBox.Show("X1 버퍼를 초기화 후 진행 하시겠습니까?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    bInitState = false;
                }
                else
                {
                    bInitState = true;
                }
            }
            ///////////////////////////////////////////////////////////


            dicSampleInfo.Clear(); // 시편정보 사전.

            string sStart = GetIniValue("STARTUP", "INFO", sStartUpIni);
            string sStart_X2 = GetIniValue("STARTUP_X2", "INFO", sStartUpIni); // for BRUKER

            if (bInitState && btnActStart.Text.Trim().ToUpper().Equals("START"))  // 시편측정이 정상종료가 안되어 버퍼클리어가 수행되지 않았으면...
            {
                X1BufferDataClear();  //시편버퍼 초기화하고 시작한다.
                PutIniValue("STARTUP", "INFO", "", sStartUpIni);
            }

            //if (sStart != "OK")  // 시편측정이 정상종료가 안되어 버퍼클리어가 수행되지 않았으면...
            //{
            //    X1BufferDataClear();  //시편버퍼 초기화하고 시작한다.
            //    PutIniValue("STARTUP", "INFO", "", sStartUpIni);
            //}

            if (string.Compare(btnActStart.Text.ToUpper(), "START") == 0)
            {
                if (grdNo.Rows.Count < 2)
                {
                    if (MessageBox.Show("등록한 시편정보가 없습니다.\r\n진행할까요?.", "알림", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == System.Windows.Forms.DialogResult.No)
                        return;
                }

                //var sData = grdNo.GetData(1, 1);
                //if (sData == null)
                //{
                //    MessageBox.Show("시편 정보를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                //sData = grdNo.GetData(1, 2);
                //if (sData == null)
                //{
                //    MessageBox.Show("TMB 구분을 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                //sData = grdNo.GetData(1, 3);
                //if (sData == null)
                //{
                //    MessageBox.Show("길이를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}


                oWriteData = new object[OPCFunction.TAG_GROUP01];
                oWriteData[11] = false;   // Carry Out
                oWriteData[20] = true;  // Start Off

                int iResFunc = OPCFunction._OPC_Write_Ascyn_Group_Tags(m_opcMgr, "XRF_GROUP01", ref oWriteData);
                if (iResFunc != 1)
                {
                    InsertLog("FA장비 시작신호 전송완료 Write Failed! Error Code:" + iResFunc);
                }
                else
                {
                    InsertLog("FA장비 시작신호 전송완료 Write");
                }

                setText(btnActStart, "Stop");
                m_bFlagStart = true;

            }
            else
            {
                oWriteData = new object[1];
                oWriteData[0] = false;
                iTagHandle = new int[1];
                iTagHandle[0] = 20;


                int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
                if (iResFunc != 1)
                {
                    InsertLog("시작신호 Reset 전송완료 Write Failed! Error Code:" + iResFunc);
                }
                else
                {
                    InsertLog("시작신호 Reset 전송완료 Write");
                }

                m_bFlagStart = false;
                setText(btnActStart, "Start");
            }
        }
        #endregion
        #region [ X1 버퍼 삭제 버튼 클릭(btnX1BufferClear_Click) ]
        /// <summary>
        /// X1 버퍼삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnX1BufferClear_Click(object sender, EventArgs e)
        {
            if (m_bFlagStart)
            {
                MessageBox.Show("기기 가동중에는 Buffer정보를 지울수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("버퍼에 있는 정보를 모두 삭제하시겠습니까?", "Buffer Clear", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                X1BufferDataClear();
            }
        }
        #endregion
        #region [ X2 버퍼 삭제 버튼 클릭(btnX2BufferClear_Click) ]
        /// <summary>
        /// X2 버퍼삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnX2BufferClear_Click(object sender, EventArgs e)
        {
            if (m_bFlagStart)
            {
                MessageBox.Show("기기 가동중에는 Buffer정보를 지울수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("버퍼에 있는 정보를 모두 삭제하시겠습니까?", "Buffer Clear", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                X2BufferDataClear();
            }
        }
        #endregion
        #region [ 버퍼적재 버튼 클릭(bttoBuffer_Click) ]
        /// <summary>
        /// 선택된 지시리스트에 항목을 버퍼에 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bttoBuffer_Click(object sender, EventArgs e)
        {
            // 시작 위치 :
            int nSrtPos = Array.FindLastIndex(sSPL, element => !string.IsNullOrEmpty(element));

            // 버퍼 적재 시작위치 
            // X1, X2
            int iX1StartPos = BufferStartPos("X1");
            int iX2StartPos = BufferStartPos("X2") + XrfControl.m_X1XrfBufferSize;

            object[] oWriteData = null;
            int[] iTagHandle = null;

            oWriteData = new object[0];
            iTagHandle = new int[0];

            for (int k = grdNo.Rows.Fixed; k < grdNo.Rows.Count; k++)
            {
                string sInputData = this.grdNo[k, 1].ToString().ToUpper().PadRight(8, ' ');             // 시편번호
                string strDivType = grdNo.GetData(k, "DivType").ToString();                             // 적재구분
                string strTMB = grdNo.GetData(k, "TMB").ToString();                                     // TBM 구분
                string strApplicationName = grdNo.GetData(k, "Application").ToString();                 // XRF 프로그램명
                string strDeviceCode = grdNo.GetData(k, "시험기").ToString();                           // 시험기 코드

                switch (strDeviceCode)
                {
                    case "X1":
                        for (int j = 0; j < strDivType.Length; j++)
                        {
                            // 중복체크
                            int nFinddup = -1;
                            try
                            {
                                nFinddup = Array.FindIndex(sSPL, element => element.Equals(sInputData + strTMB + strDivType.Substring(j, 1)));
                            }
                            catch { }
                            finally
                            {
                                if (nFinddup < 0)
                                {

                                    Array.Resize(ref iTagHandle, iTagHandle.Length + 1);
                                    Array.Resize(ref oWriteData, oWriteData.Length + 1);

                                    //iTagHandle[iTagHandle.Length - 1] = ++nSrtPos;
                                    iTagHandle[iTagHandle.Length - 1] = ++iX1StartPos;
                                    oWriteData[oWriteData.Length - 1] = sInputData + strTMB + strDivType.Substring(j, 1);

                                    if (!dicSampleInfo.ContainsKey((sInputData + strTMB + strDivType.Substring(j, 1)).Substring(0, 8).Trim().ToUpper()))
                                    {
                                        dicSampleInfo.Add((sInputData + strTMB + strDivType.Substring(j, 1)).Substring(0, 8).Trim().ToUpper(), strApplicationName.ToUpper());
                                    }
                                    else
                                    {
                                        dicSampleInfo[(sInputData + strTMB + strDivType.Substring(j, 1)).Substring(0, 8).Trim().ToUpper()] = strApplicationName.ToUpper();
                                    }
                                    sSPL[iX1StartPos] = sInputData + strTMB + strDivType.Substring(j, 1);                                   // 시편번호
                                    sbBufGridAppl[iX1StartPos] = strApplicationName;                                                        // 3. Application Name
                                }
                            }
                        }
                        break;
                    case "X2":
                        for (int j = 0; j < strDivType.Length; j++)
                        {
                            // 중복체크
                            int nFinddup = -1;
                            try
                            {
                                nFinddup = Array.FindIndex(sSPL, element => element.Equals(sInputData + strTMB + strDivType.Substring(j, 1)));
                            }
                            catch { }
                            finally
                            {
                                if (nFinddup < 0)
                                {

                                    Array.Resize(ref iTagHandle, iTagHandle.Length + 1);
                                    Array.Resize(ref oWriteData, oWriteData.Length + 1);

                                    //iTagHandle[iTagHandle.Length - 1] = ++nSrtPos;
                                    iTagHandle[iTagHandle.Length - 1] = ++iX2StartPos;
                                    oWriteData[oWriteData.Length - 1] = sInputData + strTMB + strDivType.Substring(j, 1);

                                    if (!dicSampleInfo.ContainsKey((sInputData + strTMB + strDivType.Substring(j, 1)).Substring(0, 8).Trim().ToUpper()))
                                    {
                                        dicSampleInfo.Add((sInputData + strTMB + strDivType.Substring(j, 1)).Substring(0, 8).Trim().ToUpper(), strApplicationName.ToUpper());
                                    }
                                    else
                                    {
                                        dicSampleInfo[(sInputData + strTMB + strDivType.Substring(j, 1)).Substring(0, 8).Trim().ToUpper()] = strApplicationName.ToUpper();
                                    }
                                    sSPL[iX2StartPos] = sInputData + strTMB + strDivType.Substring(j, 1);                                   // 시편번호
                                    sbBufGridAppl[iX2StartPos] = strApplicationName;                                                        // 3. Application Name
                                }
                            }
                        }
                        break;
                }
            }

            string sGroupName = string.Empty;
            sGroupName = "XRF_BUFFER";
            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, sGroupName, ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                //msgLabel.Text = "Test Data Send Fail " + iResFunc;
            }
            else
            {
                BufferCountWrite();
                //msgLabel.Text = "Test Data Send OK";
            }
        }
        #endregion
        #region [ X2 시험 시작 버튼 클릭(btnXRF2Start_Click) ]
        /// <summary>
        /// X2 BRUKER XRF Bruker 자동운전 시작
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnXRF2Start_Click(object sender, EventArgs e)
        {
            bX2Proc_Start = false;
            string sCommand = string.Empty;

            if (!PLCDefine.m_RobotRun)
            {
                if (string.Compare(btnXRF2Start.Text, "X2 자동 시험 시작") == 0)
                {
                    MessageBox.Show("로보트가 AUTO 상태가 아닙니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            last_measure_data_X2.Clear();

            int bufCount = 0;
            nX2TGTNum = 0;
            //for (int i = 0; i < nBufferNumber; i++)
            for (int i = XrfControl.m_X1XrfBufferSize; i < XrfControl.m_XrfBuffer.Length; i++)
            {
                if (XrfControl.m_XrfBuffer[i] != null)   //시편 버퍼 데이타가 들어있는 Array for BRUKER
                {
                    bufCount++;
                    nX2TGTNum++;
                }
            }

            if (bufCount < X2minqty && string.Compare(btnXRF2Start.Text, "X2 자동 시험 시작") == 0)
            {
                MessageBox.Show("버퍼에 시편이 " + X2minqty.ToString() + "개 이상 있어야 작업을 시작할수 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (XRF_COMMAND.XRF_AUTO_X2 == false)
            {
                // XRF_COMMAND.XRF_AUTO = true;  List= remote로 옮김
                XRF_COMMAND.XRF_AUTO_X2 = true;

                setText(btnXRF2Start, "Wait.....");

                // BRUKER!
                timer_X2.Enabled = true;
                MeasureMentHistoryLog("Bruker 시험시작======================================================", 2);
            }
            else
            {
                XRF_COMMAND.XRF_AUTO_X2 = false;
                timer_X2.Enabled = false;
                bX2Proc_Start = false;
                setText(btnXRF2Start, "X2 자동 시험 시작");
                m_TestList.Clear();
                DisplaySmlinX2(m_TestList);


                //////////////////////////////////////////////////////////////////////////////////////////////////////////
                //// 2016.12.15. BRUKER BUFFER를 초기화 !!!
                if (MessageBox.Show("BRUKER 버퍼를 초기화하시겠습니까?", "Buffer Init", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    //object[] oWriteData = new object[] { tbBufInput.Text, tbBufOutput.Text, "0", "0"};
                    //int[] iTagHandle = new int[] { 11, 12, 18, 19 };

                    //string sGroupName = "XRF_CONV_LOC";
                    //int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, sGroupName, ref iTagHandle, ref oWriteData);
                    //if (iResFunc != 1)
                    //{
                    //    //msgLabel.Text = "Test Data Send Fail " + iResFunc;
                    //}
                    //else
                    //{
                    //    //msgLabel.Text = "Test Data Send OK";
                    //}

                    //X2BufferDataClear();
                    setClick(btnX2BufferClear);
                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                MeasureMentHistoryLog("Bruker 시험종료======================================================", 2);
            }
        }
        #endregion
        #region [ X1 시험 시작 버튼 클릭(btnXRFStart_Click) ]
        private void btnXRFStart_Click(object sender, EventArgs e)
        {
            string sCommand = string.Empty;

            if (!PLCDefine.m_RobotRun)
            {
                if (string.Compare(btnXRFStart.Text, "X1 자동 시험 시작") == 0)
                {
                    MessageBox.Show("로보트가 AUTO 상태가 아닙니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }

            last_measure_data.Clear();

            int bufCount = 0;
            for (int i = 0; i < XrfControl.m_X1XrfBufferSize; i++)
            {
                if (XrfControl.m_XrfBuffer[i] != null)   //시편 버퍼 데이타가 들어있는 Array
                {
                    bufCount++;
                }

            }

            if (bufCount < minQty && string.Compare(btnXRFStart.Text, "X1 자동 시험 시작") == 0)
            {
                MessageBox.Show("버퍼에 시편이 " + minQty.ToString() + "개 이상 있어야 작업을 시작할수 있습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (XRF_COMMAND.XRF_AUTO == false)
            {
                // XRF_COMMAND.XRF_AUTO = true;  List= remote로 옮김
                XRF_COMMAND.XRF_AUTO = true;

                listBufLoc = 0;
                lineBufLoc = 0;
                m_InputCnt = 0;
                m_mok = 0;
                m_na = 0;
                sampleAddFlag = true;
                tmpFlag = true;
                XrfControl.m_XrfListBufferCount = 0;
                removeMok = 0;
                removeCnt = 0;
                xrf_list_buf = new string[4];

                setText(btnXRFStart, "Wait.....");

                //btnXRFStart.Text = "XRF 자동 시험 종료";

                XRF_COMMAND xcmd = new XRF_COMMAND();
                sCommand = xcmd.XRF_MAKE_SEND_COMMAND_SYSTEM_STATUS();

                XRF_Send_MSG(sCommand);
                MeasureMentHistoryLog("Panalytical 시험시작===================================================", 1);
            }
            else
            {
                XRF_COMMAND.XRF_AUTO = false;
                setText(btnXRFStart, "X1 자동 시험 시작");

                XRF_COMMAND xCmd = new XRF_COMMAND();
                string cmd = xCmd.XRF_MAKE_SEND_COMMAND_SYSTEM_STATUS();
                XRF_Send_MSG(cmd);
                MeasureMentHistoryLog("Panalytical 시험종료(중간)==============================================", 1);
            }
            //bool flag =  XrfControl.XRF_LIST_BUFFER[0];

        }
        #endregion
        #region [ 프로그램명 변경 버튼 클릭(btnAppNameChange_Click) ]
        private void btnAppNameChange_Click(object sender, EventArgs e)
        {
            //if (cmbAppName.Text == "")
            //{
            //    MessageBox.Show("먼저 프로그램을 선택하십시요.", "프로그램선택", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    cmbAppName.Focus();
            //    return;
            //}

            if (txtStart.Text == "")
            {
                MessageBox.Show("시작 순번을 입력하십시요.", "순번입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtStart.Focus();
                return;
            }

            if (txtEnd.Text == "")
            {
                MessageBox.Show("마지막 순번을 입력하십시요.", "순번입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtEnd.Focus();
                return;
            }

            if (Convert.ToInt32(txtStart.Text) > Convert.ToInt32(txtEnd.Text))
            {
                MessageBox.Show("순번 확인 후 다시 입력하십시요.", "순번입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtStart.Focus();
                return;
            }

            if (Convert.ToInt32(txtStart.Text) <= 0 || Convert.ToInt32(txtEnd.Text) <= 0)
            {
                MessageBox.Show("순번 확인 후 다시 입력하십시요.", "순번입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtStart.Focus();
                return;
            }

            if (Convert.ToInt32(txtStart.Text) > grdNo.Rows.Count - grdNo.Rows.Fixed || Convert.ToInt32(txtEnd.Text) > grdNo.Rows.Count - grdNo.Rows.Fixed)
            {
                MessageBox.Show("순번 확인 후 다시 입력하십시요.", "순번입력", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtStart.Focus();
                return;
            }
            string strMessage = "시편번호 : "; //변경실패 메세지
            bool bolCheckFalse = false; //실패한 경우가 있다면
            for (int i = Convert.ToInt32(txtStart.Text); i < Convert.ToInt32(txtEnd.Text) + 1; i++)
            {
                string strSMPLNO = grdNo.Rows[i]["시편번호"].ToString();
                string strTmbdiv = grdNo.Rows[i]["TMB"].ToString();

                string[] strParams = new string[] { strSMPLNO
                                                   ,strTmbdiv
                                                   // 프로그램명 수동변경 처리 요청 (2017-07-19 심정곤)
                                                   //,cmbAppName.Text
                                                   ,txtAppName.Text
                                                    };
                // 사용자 임의 변경에 따른 체크로직 없앰
                //if (CommonDataBase.CheckAppName(strParams))
                //{//시편번호에 맞는 검량선명이 있다면
                strParams = new string[] {//cmbAppName.Text
                                              txtAppName.Text
                                             ,strSMPLNO
                                             ,strTmbdiv
                                             };
                CommonDataBase.UpdateAppName(strParams);//기존 검량선명 수정
                //}
                //else
                //{
                //    bolCheckFalse = true;
                //    strMessage += strSMPLNO + ",";
                //}
            }
            if (bolCheckFalse)
            {//시편번호 XRF시험명이 변경되지 못한경우
                MessageBox.Show(strMessage + "는 XRF 시험 기준정보와 맞지 않아서 변경할수 없습니다");
            }
            //조회 호출
            btOrder.PerformClick();
        }

        #endregion
        #region [ 검량선명 변경시 숫자 입력텍스트(txtStart_KeyPress) ]
        private void txtStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }
        #endregion
        #endregion

        #region [ 사용자 정의 함수 ]

        #region [ Grid의 Format을 정의(InitFormatGrid)]
        private void InitFormatGrid()
        {
            #region [ 시험지시리스트(grdNo) ]
            // 양식은 디자인 툴에 입력 되어 있음
            grdNo.Rows[0].Height = 30;
            grdNo.Rows.Count = 1;
            grdNo.Height = panel19.Height - panel2.Height;

            //// 추가된 시험 지시 리스트 시험구분 및 시험기 COMBOBOX 표시
            DataTable dtDivType = RequestTestList();
            if (dtDivType != null)
            {
                ColumnInCombo(grdNo, "DivType", dtDivType, "CODEVALUE", "CODENAME");
            }
            // 컬럼해더에 CheckBox 표시 코드
            for (int iRow = 0; iRow < grdNo.Rows.Fixed; iRow++)
            {
                CellStyle cs = grdNo.Styles.Add("CheckBoxHeader");
                cs.ImageAlign = ImageAlignEnum.LeftCenter;
                cs.TextAlign = TextAlignEnum.CenterCenter;
                // 취소
                grdNo.SetData(iRow, grdNo.Cols[4].Index, grdNo[iRow, 4], false);
                grdNo.SetCellCheck(iRow, grdNo.Cols[4].Index, CheckEnum.Unchecked);
                grdNo.SetCellStyle(iRow, 4, cs);

                // 세척유무
                grdNo.SetData(iRow, grdNo.Cols[7].Index, grdNo[iRow, 7], false);
                grdNo.SetCellCheck(iRow, grdNo.Cols[7].Index, CheckEnum.Unchecked);
                grdNo.SetCellStyle(iRow, 7, cs);

                grdNo.Rows[iRow].AllowEditing = true;
            }

            grdNo.CellChecked += grdNo_CellChecked;
            grdNo.PreviewKeyDown += new PreviewKeyDownEventHandler(grdNo_PreviewKeyDown);
            #endregion
            #region [ XRF 진행 리스트(cfgrdXrfList) ]
            cfgrdXrfList.Rows[0].Height = 30;
            cfgrdXrfList.Rows.Count = 1;
            cfgrdXrfList.Cols.Count = 30;
            cfgrdXrfList.SetData(0, 0, "Seq");
            cfgrdXrfList.SetData(0, 1, "시편번호");
            cfgrdXrfList.SetData(0, 2, "TMB");
            cfgrdXrfList.SetData(0, 3, "길이");
            cfgrdXrfList.SetData(0, 4, "시험완료일시");

            cfgrdXrfList.SetData(0, 5, "Zn FW");
            cfgrdXrfList.SetData(0, 6, "Zn FC");
            cfgrdXrfList.SetData(0, 7, "Zn FD");
            cfgrdXrfList.SetData(0, 8, "Zn BW");
            cfgrdXrfList.SetData(0, 9, "Zn BC");
            cfgrdXrfList.SetData(0, 10, "Zn BD");

            cfgrdXrfList.SetData(0, 11, "Cr FW");
            cfgrdXrfList.SetData(0, 12, "Cr FC");
            cfgrdXrfList.SetData(0, 13, "Cr FD");
            cfgrdXrfList.SetData(0, 14, "Cr BW");
            cfgrdXrfList.SetData(0, 15, "Cr BC");
            cfgrdXrfList.SetData(0, 16, "Cr BD");

            cfgrdXrfList.SetData(0, 17, "EXT FW");
            cfgrdXrfList.SetData(0, 18, "EXT FC");
            cfgrdXrfList.SetData(0, 19, "EXT FD");
            cfgrdXrfList.SetData(0, 20, "EXT BW");
            cfgrdXrfList.SetData(0, 21, "EXT BC");
            cfgrdXrfList.SetData(0, 22, "EXT BD");

            cfgrdXrfList.SetData(0, 23, "Fe FW");
            cfgrdXrfList.SetData(0, 24, "Fe FC");
            cfgrdXrfList.SetData(0, 25, "Fe FD");
            cfgrdXrfList.SetData(0, 26, "Fe BW");
            cfgrdXrfList.SetData(0, 27, "Fe BC");
            cfgrdXrfList.SetData(0, 28, "Fe BD");

            cfgrdXrfList.SetData(0, 29, "SAMPLE KEY");
            #endregion
            #region [ 시험진행 리스트_제어(cfgrdDirectList) ]
            cfgrdDirectList.Rows[0].Height = 30;
            cfgrdDirectList.Cols.Count = 21;
            cfgrdDirectList.Rows.Count = 1;
            cfgrdDirectList.SetData(0, 0, "Seq");
            cfgrdDirectList.SetData(0, 1, "시편번호");
            cfgrdDirectList.SetData(0, 2, "TMB");
            cfgrdDirectList.SetData(0, 3, "WCD");
            cfgrdDirectList.SetData(0, 4, "길이");
            cfgrdDirectList.SetData(0, 5, "공정구분");
            cfgrdDirectList.SetData(0, 6, "투입");
            cfgrdDirectList.SetData(0, 7, "투입일시");
            cfgrdDirectList.SetData(0, 8, "이송");
            cfgrdDirectList.SetData(0, 9, "이송일시");
            cfgrdDirectList.SetData(0, 10, "프레스");
            cfgrdDirectList.SetData(0, 11, "프레스일시");
            cfgrdDirectList.SetData(0, 12, "타각");
            cfgrdDirectList.SetData(0, 13, "타각일시");
            cfgrdDirectList.SetData(0, 14, "세척");
            cfgrdDirectList.SetData(0, 15, "세척일시");
            cfgrdDirectList.SetData(0, 16, "배출");
            cfgrdDirectList.SetData(0, 17, "배출일시");
            cfgrdDirectList.SetData(0, 18, "버퍼");
            cfgrdDirectList.SetData(0, 19, "버퍼적재일시");
            cfgrdDirectList.SetData(0, 20, "TMP");
            #endregion
            #region [ 버퍼 그리드(grdBuffer) ]
            grdBuffer.Cols.Count = 7;
            grdBuffer.Rows.Count = 23;
            grdBuffer.Cols.Fixed = 1;
            grdBuffer.Rows.Fixed = 1;

            grdBuffer.Cols[0].Width = 50;
            for (int k = grdBuffer.Cols.Fixed; k < grdBuffer.Cols.Count; k++)
            {
                grdBuffer.Cols[k].Width = 100;// 115;
            }
            for (int k = 0; k < grdBuffer.Rows.Count; k++)
            {
                grdBuffer.Rows[k].Height = 35;
            }

            grdBuffer.SetData(0, 1, "X1 버퍼라인1");
            grdBuffer.SetData(0, 2, "X1 버퍼라인2");
            grdBuffer.SetData(0, 3, "X1 버퍼라인3");
            grdBuffer.SetData(0, 4, "X1 버퍼라인4");
            grdBuffer.SetData(0, 5, "X2 버퍼라인1");
            grdBuffer.SetData(0, 6, "X2 버퍼라인2");
            for (int k = grdBuffer.Rows.Fixed; k < grdBuffer.Rows.Count; k++)
            {
                grdBuffer.SetData(k, 0, k.ToString("00"));
            }
            grdBuffer.Cols[0].TextAlign = C1.Win.C1FlexGrid.TextAlignEnum.CenterCenter;
            #endregion
        }
        #endregion
        #region [ 그리드 컬럼에 콤보 적용(ColumnInCombo) ]
        private void ColumnInCombo(C1FlexGrid grid, string strColumnName, DataTable dtSource, string strValueMember, string strDisplayMember)
        {
            ListDictionary ComboItemMap = new ListDictionary();
            if (dtSource != null)
            {
                for (int iRow = 0; iRow < dtSource.Rows.Count; iRow++)
                {
                    ComboItemMap.Add(dtSource.Rows[iRow][strValueMember], dtSource.Rows[iRow][strDisplayMember]);
                }
                CellStyle csCombo = grid.Styles.Add("ComboStyle" + strColumnName);
                csCombo.DataMap = ComboItemMap;

                grid.Cols[strColumnName].Style = csCombo;
            }
        }
        #endregion  // 컬럼에 콤보 적용
        #region [ 콤보박스 초기화(InitCombo) ]
        private void InitCombo()
        {
            // 시험방법(WCD, WD)
            DataTable dtDiv = CommonDataBase.GetDivTypeList(dbConn);

            // 버퍼정보 상단 콤보박스
            cmbDIVTYPE_DIR.ValueMember = "VALUE";
            cmbDIVTYPE_DIR.DisplayMember = "NAME";
            cmbDIVTYPE_DIR.DataSource = dtDiv.Copy();
            if (dtDiv != null && dtDiv.Rows.Count > 0)
                cmbDIVTYPE_DIR.SelectedIndex = 0;
            // 수동등록 팝업 콤보박스
            cmbDIVTYPE.ValueMember = "VALUE";
            cmbDIVTYPE.DisplayMember = "NAME";
            cmbDIVTYPE.DataSource = dtDiv.Copy();
            if (dtDiv != null && dtDiv.Rows.Count > 0)
                cmbDIVTYPE.SelectedIndex = 0;

            // 시험기 리스트(X1, X2)
            cmbXRF.SelectedIndex = 0;
            // TMB 구분(T, M, B)
            cmbTmb.SelectedIndex = 0;

            //2017.04.26 김정건 검량선명 바인딩
            DataTable dtAppName = CommonDataBase.GetAppName(dbConn);
            cmbAppName.ValueMember = "VALUE";
            cmbAppName.DisplayMember = "NAME";
            cmbAppName.DataSource = dtAppName;
            if (dtAppName != null && dtAppName.Rows.Count > 0)
                cmbAppName.SelectedIndex = 0;
        }
        #endregion

        #region [ 쿼리 관련 ]

        #region [RequestOrderList : 시험지시 목록을 보여준다]
        /// <summary>
        /// 시험지시 목록을 보여준다
        /// </summary>
        private void RequestOrderList()
        {
            string[] aData = null;
            CommonDataBase cDb = new CommonDataBase();

            try
            {
                SqlDataReader reader = CommonDataBase.GetOrderList(dbConn);
                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        int iRow = 1;
                        //grdNo.Rows.Count = grdNo.Rows.Fixed;
                        while (reader.Read())
                        {
                            grdNo.Rows.Count = iRow + 1;
                            grdNo.SetData(iRow, 0, iRow.ToString());
                            grdNo.SetData(iRow, 1, reader.GetValue(0).ToString()); // 시편번호
                            grdNo.SetData(iRow, 2, reader.GetValue(1).ToString()); // TMB
                            grdNo.SetData(iRow, 3, reader.GetValue(6).ToString()); // 길이
                            grdNo.SetData(iRow, 4, false);
                            grdNo.SetData(iRow, 6, reader.GetValue(5).ToString()); // 수지
                            grdNo.SetData(iRow, 7, reader.GetValue(9).ToString()); // 세척유무
                            grdNo.SetData(iRow, 8, reader.GetValue(8).ToString()); // 시험 구분
                            grdNo.SetData(iRow, 9, reader.GetValue(3).ToString()); // 시험기 구분
                            grdNo.SetData(iRow, 12, reader.GetValue(7).ToString().Replace("-", "_")); // NEW 프로그램
                            iRow++;
                        }
                        reader = null;

                        aData = new string[4];
                        aData[0] = "";
                        aData[1] = "";
                        aData[2] = "D";
                        aData[3] = "S";
                        cDb.Execute_BackGroundWorker(aData, 1);  // Data Load를 완료하면 Load한 모든 시편을 'D'로 변경한다.
                        Thread.Sleep(10);
                    }
                    else
                    {
                        MessageBox.Show("시험지시 내용이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        grdNo.Rows.Count = grdNo.Rows.Fixed;
                    }
                }
                else
                {
                    MessageBox.Show("시험지시 내용이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    grdNo.Rows.Count = grdNo.Rows.Fixed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //if (dbConn.State == ConnectionState.Open) dbConn.Close();

            }
        }
        #endregion
        #region [RequestTestList : 테스트할 시편 종류 목록을 보여준다]
        /// <summary>
        /// 테스트할 시편 종류 목록을 보여준다
        /// </summary>
        public DataTable RequestTestList()
        {
            DataTable dtDivType = new DataTable();
            SqlCommand command = null;
            SqlDataReader reader = null;
            //OleDbConnection dbCon = new OleDbConnection(OraDataHelper.gConnString);
            SqlConnection dbCon = MssqlConnect.Instance.MssqlCnn;

            dtDivType.Columns.Add("CODEVALUE");
            dtDivType.Columns.Add("CODENAME");

            try
            {
                command = dbConn.CreateCommand();
                command.CommandText = "SELECT CODEVALUE, CODENAME FROM TB_CODE WHERE CODEID = 11100";

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                reader = command.ExecuteReader();
                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DataRow dr = dtDivType.NewRow();
                            dr["CODEVALUE"] = reader.GetValue(0).ToString();
                            dr["CODENAME"] = reader.GetValue(1).ToString();
                            dtDivType.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        MessageBox.Show("시편 적재구분이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("시편 적재구분이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:RequestTestList" + ex.ToString());
                return null;
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }
            return dtDivType;
        }
        #endregion

        #endregion

        #region [ INI 파일정보 가져오기(GetIniValue) ]
        private String GetIniValue(String Section, String Key, string sFileName)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, sFileName);
            return temp.ToString();
        }
        #endregion
        #region [ INI 파일정보 쓰기(PutIniValue) ]
        private void PutIniValue(String Section, String Key, string Value, string sFileName)
        {
            WritePrivateProfileString(Section, Key, Value, sFileName);
        }
        #endregion

        #region [ PLC 정보 변경시 처리 로직(BufferDataWrite) ]
        /// <summary>
        /// 해당 Tag의 내용을 화면에 표시해준다. 
        /// </summary>
        /// <param name="SGroupName"></param>
        /// <param name="idx"></param>
        /// <param name="str"></param>
        private void BufferDataWrite(string SGroupName, int idx, string str)
        {
            string sSmplNo = string.Empty;
            string sTmbDiv = string.Empty;
            string sCarve = string.Empty;
            string sLength = string.Empty;
            string sDate = string.Empty;

            try
            {
                //InsertLog(string.Format("1160 TEMP"));
                int iContPre = idx + 1;
                #region [ XRF_BUFFER ]
                if (string.Compare("XRF_BUFFER", SGroupName) == 0)
                {
                    InsertLog(string.Format("1164 : str={0}, iContPre={1}", str.Trim(), iContPre.ToString()));

                    #region [ 기존코드 ]
                    //Label LabelTarget = (Controls.Find("Buffer" + iContPre.ToString(), true)[0] as Label);
                    //str = str.Trim();

                    //if (str.Length > 1)
                    //{
                    //    LabelTarget.BackColor = Color.Yellow;
                    //    XrfControl.m_XrfBuffer[iContPre - 1] = str;
                    //    m_BufferCount++;
                    //    if (m_BufferCount >= 3 && m_ChkAutoStart && !XRF_COMMAND.XRF_AUTO)
                    //    {
                    //        setClick(btnXRFStart);
                    //    }
                    //}
                    //else
                    //{
                    //    if (m_BufferCount > 0)
                    //    {
                    //        m_BufferCount--;
                    //    }
                    //    LabelTarget.BackColor = Color.Transparent;
                    //    XrfControl.m_XrfBuffer[iContPre - 1] = null;
                    //}
                    //setText(lbBufRealCnt, m_BufferCount.ToString());

                    //setText(LabelTarget, str);

                    //if (iContPre == 80)
                    //{
                    //    BufferCountWrite();
                    //}
                    #endregion
                    if (iContPre < (XrfControl.m_XrfBuffer.Length + 1))
                    {
                        ////Label LabelTarget = (Controls.Find("Buffer" + iContPre.ToString(), true)[0] as Label);
                        str = str.Trim();

                        //// ADD 2016.10.15 !
                        CellStyle csCellStyle = grdBuffer.Styles.Add("CellStyle" + (grdBuffer.Rows.Fixed + idx % 22).ToString() + (grdBuffer.Rows.Fixed + idx / 22).ToString());
                        csCellStyle.BackColor = Color.Yellow;

                        if (str.Length > 1)
                        {
                            ////LabelTarget.BackColor = Color.Yellow;

                            //////////////////////////////////////////////////////
                            //// ADD 2016.10.15 !
                            if (grdBuffer.InvokeRequired)
                            {
                                grdBuffer.BeginInvoke(new Action(() => grdBuffer.SetCellStyle(grdBuffer.Rows.Fixed + idx % 22, grdBuffer.Cols.Fixed + idx / 22, csCellStyle)));
                            }
                            else
                            {
                                grdBuffer.SetCellStyle(grdBuffer.Rows.Fixed + idx % 22, grdBuffer.Cols.Fixed + idx / 22, csCellStyle);
                            }

                            //////////////////////////////////////////////////////////////////////////////////////////////////////////

                            XrfControl.m_XrfBuffer[iContPre - 1] = str;
                            if (iContPre <= 88)
                            {
                                m_BufferCount++;
                                if (m_BufferCount >= 3 && m_ChkAutoStart && !XRF_COMMAND.XRF_AUTO)
                                {
                                    setClick(btnXRFStart);
                                }
                            }
                            else
                            {
                                // FA 자동 스타트시 체크박스로 자동 시험 시작 조건 처리(BRUKER 아직 처리 안됨)
                                //m_BufferCount_X2++;
                                //if (m_BufferCount_X2 >= 2 && m_ChkAutoStart && !XRF_COMMAND.XRF_AUTO_X2)
                                //{
                                //    setClick(btnXRF2Start);
                                //}
                            }

                            if (str.Length >= 8)
                            {
                                dicSampleInfo.TryGetValue(str.Substring(0, 8).Trim(), out sbBufGridAppl[iContPre - 1]);
                                sSPL[iContPre - 1] = str;
                            }
                        }
                        #region [ 버퍼삭제 ]
                        else
                        {
                            if (m_BufferCount > 0)
                            {
                                m_BufferCount--;
                            }
                            XrfControl.m_XrfBuffer[iContPre - 1] = null;

                            //// ADD 2016.10.15 !
                            csCellStyle.BackColor = Color.White;
                            //// ADD 2016.10.15 !
                            if (grdBuffer.InvokeRequired)
                            {
                                grdBuffer.BeginInvoke(new Action(() => grdBuffer.SetCellStyle(grdBuffer.Rows.Fixed + idx % 22, grdBuffer.Cols.Fixed + idx / 22, csCellStyle)));
                            }
                            else
                            {
                                grdBuffer.SetCellStyle(grdBuffer.Rows.Fixed + idx % 22, grdBuffer.Cols.Fixed + idx / 22, csCellStyle);
                            }

                            sbBufGridAppl[iContPre - 1] = string.Empty;
                            sSPL[iContPre - 1] = string.Empty;

                            //dicSampleInfo.Remove(str.Substring(0, 8).Trim()); // 시편정보 삭제.
                        }
                        #endregion

                        /////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //// ADD 2016.10.24 !
                        if (grdBuffer.InvokeRequired)
                        {
                            grdBuffer.BeginInvoke(new Action(() => grdBuffer[grdBuffer.Rows.Fixed + idx % 22, grdBuffer.Cols.Fixed + (idx / 22)] = str));
                        }
                        else
                        {
                            grdBuffer[grdBuffer.Rows.Fixed + idx % 22, grdBuffer.Cols.Fixed + idx / 22] = str;
                        }

                        BufferInfoUpdate();         // 버퍼정보 XML 파일 업데이트
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                    // 전체버퍼정보를 스캔해서 투입수량 적용
                    if (iContPre == nBufferNumber)
                    {
                        // FA 자동으로 구동시 HMI => PLC로 버퍼 변경정보 자동으로 쓰지 않음
                        if (!m_bFlagStart)
                            BufferCountWrite();
                    }
                }
                #endregion
                #region [ XRF_ALARM 처리 ]
                if (string.Compare("XRF_ALARM", SGroupName) == 0)
                {
                    if (Convert.ToInt16(str) != 0)
                    {
                        lbErrorMsg.Text = PLCDefine._PLCAlarm[idx];
                        pnlErrorMsg.Visible = true;
                        return;
                    }
                    else
                    {
                        pnlErrorMsg.Visible = false;
                    }
                }
                #endregion
                #region [ XRF_CONV_LOC 그룹 처리 ]
                try
                {
                    if (string.Compare("XRF_CONV_LOC", SGroupName) == 0)
                    {
                        if (iContPre < 14)
                        {
                            Label LabelTarget = (Controls.Find("LblLoc" + iContPre.ToString(), true)[0] as Label);
                            str = str.Trim();


                            if (str.Length > 0)
                            {
                                if (idx == 0)
                                {
                                    int iLength = Convert.ToInt32(str);
                                    if (iLength > 0)
                                    {
                                        LabelTarget.BackColor = Color.Yellow;
                                        setText(LabelTarget, str);
                                    }
                                    else
                                    {
                                        LabelTarget.BackColor = Color.Transparent;
                                        setText(LabelTarget, "");
                                    }
                                }
                                else
                                {
                                    string[] aData = new string[4];
                                    string[] iData = new string[8];
                                    string[] uData = new string[6];

                                    LabelTarget.BackColor = Color.Yellow;
                                    setText(LabelTarget, str);
                                    sDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    switch (iContPre)
                                    {
                                        case 3:   // 투입
                                            #region [ 투입 ]
                                            sSmplNo = LabelTarget.Text.Substring(0, 8);
                                            sTmbDiv = LabelTarget.Text.Substring(8, 1);
                                            sCarve = "T";   // 'T'emp 임시부여(Table의 Key 조건을 맞추기 위하여)

                                            iData[0] = sSmplNo;
                                            iData[1] = sTmbDiv;
                                            iData[2] = sCarve;
                                            iData[3] = sTmpLength;
                                            iData[4] = "";
                                            iData[5] = "1";
                                            iData[6] = "Y";
                                            iData[7] = sDate;
                                            CommonDataBase cDb2 = new CommonDataBase();
                                            cDb2.Execute_BackGroundWorker(iData, 2);
                                            Thread.Sleep(10);
                                            cfgrdDirectListAddData(sSmplNo, sTmbDiv, sCarve, sTmpLength, "", "1", "Y", sDate);
                                            #endregion
                                            break;
                                        case 4:   // 이송
                                            #region [ 이송 ]
                                            sSmplNo = LabelTarget.Text.Substring(0, 8);
                                            sTmbDiv = LabelTarget.Text.Substring(8, 1);
                                            sCarve = "T";

                                            uData[0] = sSmplNo;
                                            uData[1] = sTmbDiv;
                                            uData[2] = sCarve;
                                            uData[3] = "2";
                                            uData[4] = "Y";
                                            uData[5] = sDate;
                                            CommonDataBase cDb3 = new CommonDataBase();
                                            cDb3.Execute_BackGroundWorker(uData, 3);
                                            Thread.Sleep(10);

                                            cfgrdDirectListAddData(sSmplNo, sTmbDiv, sCarve, sTmpLength, "", "2", "Y", sDate);
                                            #endregion
                                            break;
                                        case 5:   // PRESS
                                            #region [ PRESS ]
                                            sSmplNo = LabelTarget.Text.Substring(0, 8);
                                            sTmbDiv = LabelTarget.Text.Substring(8, 1);
                                            sCarve = LabelTarget.Text.Substring(9, 1);

                                            uData[0] = sSmplNo;
                                            uData[1] = sTmbDiv;
                                            uData[2] = sCarve;
                                            uData[3] = "3";
                                            uData[4] = "Y";
                                            uData[5] = sDate;
                                            CommonDataBase cDb5 = new CommonDataBase();
                                            cDb5.Execute_BackGroundWorker(uData, 3);
                                            Thread.Sleep(10);

                                            cfgrdDirectListAddData(sSmplNo, sTmbDiv, sCarve, sTmpLength, "", "3", "Y", sDate);
                                            #endregion
                                            break;
                                        case 7:   // 타각
                                            #region [ 타각 ]
                                            sSmplNo = LabelTarget.Text.Substring(0, 8);
                                            sTmbDiv = LabelTarget.Text.Substring(8, 1);
                                            sCarve = LabelTarget.Text.Substring(9, 1);

                                            uData[0] = sSmplNo;
                                            uData[1] = sTmbDiv;
                                            uData[2] = sCarve;
                                            uData[3] = "4";
                                            uData[4] = "Y";
                                            uData[5] = sDate;
                                            CommonDataBase cDb7 = new CommonDataBase();
                                            cDb7.Execute_BackGroundWorker(uData, 3);
                                            Thread.Sleep(10);

                                            cfgrdDirectListAddData(sSmplNo, sTmbDiv, sCarve, sTmpLength, "", "4", "Y", sDate);
                                            #endregion
                                            break;
                                        case 8:   // 세척
                                            #region [ 세척 ]
                                            sSmplNo = LabelTarget.Text.Substring(0, 8);
                                            sTmbDiv = LabelTarget.Text.Substring(8, 1);
                                            sCarve = LabelTarget.Text.Substring(9, 1);

                                            uData[0] = sSmplNo;
                                            uData[1] = sTmbDiv;
                                            uData[2] = sCarve;
                                            uData[3] = "5";
                                            uData[4] = "Y";
                                            uData[5] = sDate;
                                            CommonDataBase cDb9 = new CommonDataBase();
                                            cDb9.Execute_BackGroundWorker(uData, 3);
                                            Thread.Sleep(10);

                                            cfgrdDirectListAddData(sSmplNo, sTmbDiv, sCarve, sTmpLength, "", "5", "Y", sDate);
                                            #endregion
                                            break;
                                        case 9:   // 배출
                                            #region [ 배출 ]
                                            sSmplNo = LabelTarget.Text.Substring(0, 8);
                                            sTmbDiv = LabelTarget.Text.Substring(8, 1);
                                            sCarve = LabelTarget.Text.Substring(9, 1);

                                            uData[0] = sSmplNo;
                                            uData[1] = sTmbDiv;
                                            uData[2] = sCarve;
                                            uData[3] = "6";
                                            uData[4] = "Y";
                                            uData[5] = sDate;
                                            CommonDataBase cDb11 = new CommonDataBase();
                                            cDb11.Execute_BackGroundWorker(uData, 3);
                                            Thread.Sleep(10);

                                            cfgrdDirectListAddData(sSmplNo, sTmbDiv, sCarve, sTmpLength, "", "6", "Y", sDate);
                                            #endregion
                                            break;
                                        case 10:   // 버퍼적재
                                            #region [ 버퍼적재 ]
                                            sSmplNo = LabelTarget.Text.Substring(0, 8);
                                            sTmbDiv = LabelTarget.Text.Substring(8, 1);
                                            sCarve = LabelTarget.Text.Substring(9, 1);

                                            uData[0] = sSmplNo;
                                            uData[1] = sTmbDiv;
                                            uData[2] = sCarve;
                                            uData[3] = "7";
                                            uData[4] = "Y";
                                            uData[5] = sDate;
                                            CommonDataBase cDb13 = new CommonDataBase();
                                            cDb13.Execute_BackGroundWorker(uData, 3);
                                            Thread.Sleep(10);

                                            cfgrdDirectListAddData(sSmplNo, sTmbDiv, sCarve, sTmpLength, "", "7", "Y", sDate);
                                            #endregion
                                            break;
                                        case 12:    // 버퍼적재수량
                                            txtX1BufferInputQty.BackColor = Color.Yellow;
                                            setText(txtX1BufferInputQty, str);
                                            break;
                                        case 13:    // XRF 시험완료
                                            txtX1OutPutBuffer.BackColor = Color.Yellow;
                                            setText(txtX1OutPutBuffer, str);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                LabelTarget.BackColor = Color.Transparent;
                                setText(LabelTarget, str);
                            }
                        }
                        else
                        {
                            switch (iContPre)
                            {
                                case 19:
                                    txtX2BufferInputQty.BackColor = Color.Yellow;
                                    setText(txtX2BufferInputQty, str);
                                    break;
                                case 20:
                                    txtX2OutPutBuffer.BackColor = Color.Yellow;
                                    setText(txtX2OutPutBuffer, str);
                                    break;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    WriteLogData(ex.Message, "BufferDataWrite2");
                }
                #endregion
                #region [ XRF_GROUP01 ]
                if (string.Compare("XRF_GROUP01", SGroupName) == 0)
                {
                    switch (idx)
                    {
                        case 0:  // 소재길이 정보 요구 받음
                            if (Convert.ToInt16(str) != 0)
                            {
                                picChange(picItemReq, 2);

                                if (m_bFlagStart)  // 시작버튼이 눌렸으면...
                                {
#if TEST
#else
                                    Send_Item_Info();
#endif
                                }

                            }
                            else
                            {
                                picChange(picItemReq, 0);
                            }
                            break;
                        case 1:   // 소재정보 보냄
                            if (Convert.ToInt16(str) != 0)
                            {
                                picChange(picItemSend, 2);
                            }
                            else
                            {
                                picChange(picItemSend, 0);
                            }
                            break;
                        case 2:
                            if (Convert.ToInt16(str) != 0)
                            {
                                //PLCAddrInfoChange(lblOpen, Color.Yellow);
                            }
                            else
                            {
                                //PLCAddrInfoChange(lblOpen, Color.Transparent);
                            }
                            break;
                        case 3:
                            if (Convert.ToInt16(str) != 0)
                            {
                                //PLCAddrInfoChange(lblClose, Color.Yellow);
                            }
                            else
                            {
                                //PLCAddrInfoChange(lblClose, Color.Transparent);
                            }
                            break;
                        case 4:
                            if (Convert.ToInt16(str) != 0)  // 전면투입허가
                            {
                                picXrfFrInEna.Image = Properties.Resources.SignalBlue;
                                PLCDefine.m_PlcFrontInEnable = true;
                                MeasureMentHistoryLog("전면투입 허가 On", 1);
                            }
                            else
                            {
                                picXrfFrInEna.Image = Properties.Resources.SignalEmpth;
                                PLCDefine.m_PlcFrontInEnable = false;
                                MeasureMentHistoryLog("전면투입 허가 Off", 1);
                            }
                            break;
                        case 5:
                            if (Convert.ToInt16(str) != 0)   // 전면투입완료
                            {
                                picXrfFrInComp.Image = Properties.Resources.SignalBlue;
                                PLCDefine.m_PlcFrontInComplete = true;
                                MeasureMentHistoryLog("전면투입 완료 On", 1);
                                if (XRF_COMMAND.XRF_AUTO)   // 자동 모드일때만
                                {
                                    if (XRF_COMMAND.XFR_FLAG_LIST_START == false)
                                    {
                                        XRF_COMMAND.XFR_FLAG_LIST_START = true;
                                        //XRF_LIST_START();
                                    }

                                    //if (XRF_COMMAND.XRF_TEMP_FLAG == true)
                                    //{
                                    //    XRF_SAMPLE_MEASURE("@SAMPLE_ID=" + ListQueue.Dequeue());
                                    //}

                                    //    if (XrfControl.m_XrfListBufferCount == 4 && XRF_COMMAND.XFR_FLAG_MEASURE_END)
                                    //    {
                                    //        XRF_COMMAND.XFR_FLAG_MEASURE_END = false;
                                    //    }
                                }

                            }
                            else
                            {
                                picXrfFrInComp.Image = Properties.Resources.SignalEmpth;
                                PLCDefine.m_PlcFrontInComplete = false;
                                MeasureMentHistoryLog("전면투입 완료 Off", 1);
                            }
                            break;
                        case 6:
                            if (Convert.ToInt16(str) != 0)   // 전면시험완료
                            {
                                picXrfFrTestComp.Image = Properties.Resources.SignalBlue;
                                PLCDefine.m_PlcFrontInTestComplete = true;
                                MeasureMentHistoryLog("전면시험 완료 On", 1);
                            }
                            else
                            {
                                picXrfFrTestComp.Image = Properties.Resources.SignalEmpth;
                                PLCDefine.m_PlcFrontInTestComplete = false;
                                MeasureMentHistoryLog("전면시험 완료 Off", 1);
                            }
                            break;
                        case 7:
                            if (Convert.ToInt16(str) != 0)  // 이면투입허가
                            {
                                picXrfBkInEna.Image = Properties.Resources.SignalBlue;
                                PLCDefine.m_PlcBackInEnable = true;
                                MeasureMentHistoryLog("이면투입 허가 On", 1);
                            }
                            else
                            {
                                picXrfBkInEna.Image = Properties.Resources.SignalEmpth;
                                PLCDefine.m_PlcBackInEnable = false;
                                MeasureMentHistoryLog("이면투입 허가 Off", 1);
                            }
                            break;
                        case 8:
                            if (Convert.ToInt16(str) != 0)   // 이면투입완료
                            {
                                picXrfBkInComp.Image = Properties.Resources.SignalBlue;
                                PLCDefine.m_PlcBackInComplete = true;
                                MeasureMentHistoryLog("이면투입 완료 On", 1);
                            }
                            else
                            {
                                picXrfBkInComp.Image = Properties.Resources.SignalEmpth;
                                PLCDefine.m_PlcBackInComplete = false;
                                MeasureMentHistoryLog("이면투입 완료 Off", 1);

                                //if (XRF_COMMAND.XRF_AUTO)   // 자동 모드일때만
                                //{
                                //    XRF_SAMPLE_MEASURE("@SAMPLE_ID=" + ListQueue.Dequeue());
                                //}
                            }
                            break;
                        case 9:
                            if (Convert.ToInt16(str) != 0)   // 이면시험완료
                            {
                                picXrfBkTestComp.Image = Properties.Resources.SignalBlue;
                                PLCDefine.m_PlcBackInTestComplete = true;
                                MeasureMentHistoryLog("이면시험 완료 On", 1);
                            }
                            else
                            {
                                picXrfBkTestComp.Image = Properties.Resources.SignalEmpth;
                                PLCDefine.m_PlcBackInTestComplete = false;
                                MeasureMentHistoryLog("이면시험 완료 Off", 1);

                            }
                            break;
                        case 10:
                            if (Convert.ToInt16(str) != 0)
                            {
                                //PLCAddrInfoChange(lblItemReady, Color.Yellow);
                            }
                            else
                            {
                                //PLCAddrInfoChange(lblItemReady, Color.Transparent);
                            }
                            break;
                        case 11:
                            if (Convert.ToInt16(str) != 0)
                            {
                                //PLCAddrInfoChange(lblCarryOut, Color.Yellow);
                            }
                            else
                            {
                                //PLCAddrInfoChange(lblCarryOut, Color.Transparent);
                            }
                            break;
                        case 12:
                            if (Convert.ToInt16(str) != 0)
                            {
                                //PLCAddrInfoChange(lblError, Color.Yellow);
                            }
                            else
                            {
                                //PLCAddrInfoChange(lblError, Color.Transparent);
                            }
                            break;
                        case 19:  // AutoRun
                            if (Convert.ToInt16(str) != 0)
                            {
                                picRun.Image = Properties.Resources.SignalGreen;
                                setText(lblMode, "Auto Run", Color.LightGreen, Color.Black);
                            }
                            else
                            {
                                picRun.Image = Properties.Resources.SignalEmpth;
                                setText(lblMode, "대기 Mode", Color.Transparent, Color.Black);
                            }
                            break;
                        case 20:  // Start
                            if (Convert.ToInt16(str) != 0)
                            {
                                picStart.Image = Properties.Resources.SignalRed3;
                                setText(lblStart, "Start On 상태", Color.LightGreen, Color.Black);
                            }
                            else
                            {
                                picStart.Image = Properties.Resources.SignalEmpth;
                                setText(lblStart, "Start Off 상태", Color.Red, Color.White);
                            }
                            break;
                        case 22:  // CRF CAP ON/OFF ( 1:ON, 0:OFF)
                            if (Convert.ToInt16(str) != 0)
                            {

                                picCapOpen.Image = Properties.Resources.SignalYellow;
                                setText(lbCapOpen, "XRF CAP 열림", Color.Yellow, Color.Black);
                                MeasureMentHistoryLog("XRF CAP 열림", 1);

                                if (XRF_COMMAND.XRF_AUTO)   // 자동 모드일때만
                                {
                                    if (XRF_COMMAND.XFR_FLAG_LIST_START)
                                    {
                                        if (last_measure_data.Count > 0)
                                        {
                                            if (m_InputCnt > 1)
                                            {
                                                m_DequeData = last_measure_data.Dequeue();
                                            }
                                            setText(label120, "Dequeue: " + m_DequeData);
                                            setText(label9, "Count: " + last_measure_data.Count.ToString());
                                        }
                                        else
                                        {
                                            m_DequeData = string.Empty;
                                            setText(label120, "Dequeue: " + "Blank");
                                        }

                                        InputCountDiv();
                                        if (m_mok > 0)
                                        {
                                            if (m_na == 1 || m_na == 2)
                                            {

                                                //if (last_measure_data.Count > 0)
                                                //{
                                                //    m_DequeData = last_measure_data.Dequeue();
                                                //    setText(label120, "Dequeue2: " + m_DequeData);
                                                //    setText(label9, "Count: " + last_measure_data.Count.ToString());
                                                //}

                                                if (m_DequeData.Length >= 10)
                                                {
                                                    XRF_Back_Out_Req();
                                                    MeasureMentHistoryLog("이면배출 신호 보내기", 1);
                                                }
                                                return;
                                            }
                                        }

                                        //if (last_measure_data.Count > 0)    //last_measure_data.Count > 0 
                                        //{
                                        //m_DequeData = last_measure_data.Dequeue();


                                        //setText(label120, "Dequeue: " + m_DequeData);
                                        //setText(label9, "Count: " + last_measure_data.Count.ToString());

                                        bool bsr = false;
                                        //if (PLCDefine.m_in_count == 0 || PLCDefine.m_in_count == 1)
                                        if (m_na == 1 || m_na == 2)
                                        {
                                            #region [ BRUKER 와 병행작업시 작업이후 투입신호 발생문제로 아래 코드로 변경함(2016-11-20 심정곤)
                                            //foreach (string sr in XrfControl.m_XrfBuffer)
                                            //{
                                            //    if (sr != null)
                                            //    {
                                            //        bsr = true;
                                            //        break;
                                            //    }
                                            //}
                                            #endregion

                                            for (int iCnt = 0; iCnt < XrfControl.m_X1XrfBufferSize; iCnt++)
                                            {
                                                if (!string.IsNullOrEmpty(XrfControl.m_XrfBuffer[iCnt]))
                                                {
                                                    bsr = true;
                                                    break;
                                                }
                                            }

                                            if ((m_DequeData.Length >= 10 || m_DequeData.Length == 0) && (bsr))
                                            {
                                                Xrf_Front_in_Enable();  // 전면투입허가
                                                MeasureMentHistoryLog("전면투입 허가신호 보내기", 1);
                                            }

                                        }
                                        else if (m_na == 3 || m_na == 4)
                                        {
                                            if (m_DequeData.Length >= 10)
                                            {
                                                Xrf_Back_in_Enable();   //이면투입허가
                                                MeasureMentHistoryLog("이면투입 허가신호 보내기", 1);
                                            }
                                        }
                                        //}   //last_measure_data.Count > 0 
                                    }
                                }
                            }
                            else
                            {
                                picCapOpen.Image = Properties.Resources.SignalBlue;
                                setText(lbCapOpen, "XRF CAP 닫힘", Color.LightGreen, Color.Black);
                                MeasureMentHistoryLog("XRF CAP 닫힘", 1);
                            }
                            break;
                        case 23:  // 이면배출완료
                            if (Convert.ToInt16(str) != 0)
                            {
                                picBackOut.Image = Properties.Resources.SignalBlue;
                                MeasureMentHistoryLog("이면 배출완료 On", 1);

                                bool bsr = false;
                                for (int iCnt = 0; iCnt < XrfControl.m_X1XrfBufferSize; iCnt++)
                                {
                                    if (!string.IsNullOrEmpty(XrfControl.m_XrfBuffer[iCnt]))
                                    {
                                        bsr = true;
                                        break;
                                    }
                                }
                                //foreach (string sr in XrfControl.m_XrfBuffer)
                                //{
                                //    if (sr != null)
                                //    {
                                //        bsr = true;
                                //        break;
                                //    }
                                //}

                                if (bsr)
                                {
                                    Xrf_Front_in_Enable();
                                    MeasureMentHistoryLog("전면투입 허가신호 보내기", 1);
                                }

                            }
                            else
                            {
                                picBackOut.Image = Properties.Resources.SignalEmpth;
                                MeasureMentHistoryLog("이면 배출완료 Off", 1);
                            }
                            break;
                        case 24:  // 로보트 진공이상(GRIPPER 진공이 2초이내에 생기지 않을경우 Error)
                            if (Convert.ToInt16(str) != 0)
                            {
                                setVisible(pnlRobotError, true);
                                picRobotError.Image = Properties.Resources.SignalRed3;
                                setText(lblRobotError, "ROBOT 진공이상", Color.Red, Color.White);

                                // XRF_COMMAND.XRF_AUTO = false;
                                setText(btnXRFStart, "XRF 자동 시험 시작");
                            }
                            else
                            {
                                picRobotError.Image = Properties.Resources.SignalGreen;
                                setText(lblRobotError, "ROBOT 진공정상", Color.Transparent, Color.Black);
                            }
                            break;

                    }
                }
                #endregion
                #region [ XRF_SIGNAL ]
                if (string.Compare("XRF_SIGNAL", SGroupName) == 0)
                {
                    PictureBox PicTarget = (Controls.Find("Signal" + iContPre.ToString(), true)[0] as PictureBox);
                    str = str.Trim();

                    if (Convert.ToInt16(str) != 0)
                    {
                        if (iContPre == 67)
                        {
                            setText(label54, "ROBOT 운전중", Color.LightGreen, Color.Black);
                            PLCDefine.m_RobotRun = true;
                            pbRobot.Invoke(new Action(() =>
                            {
                                pbRobot.Image = Image.FromFile(Application.StartupPath + @"\Images\ABB_Enable_T.png", true);
                            }));
                        }
                        picChange(PicTarget, 3);
                    }
                    else
                    {
                        if (iContPre == 67)
                        {
                            PLCDefine.m_RobotRun = false;
                            setText(label54, "ROBOT STOP", Color.Red, Color.White);
                            picChange(PicTarget, 4);
                            pbRobot.Invoke(new Action(() =>
                            {
                                pbRobot.Image = Image.FromFile(Application.StartupPath + @"\Images\ABB_Enable_F.png", true);
                            }));
                        }
                        else
                        {
                            picChange(PicTarget, 0);
                        }
                    }
                    switch (iContPre)
                    {
                        case 64:
                            if (Convert.ToInt16(str) != 0)
                            {
                                picChange(Signal64_1, 3);
                            }
                            else
                            {
                                picChange(Signal64_1, 0);
                            }
                            break;

                        case 65:
                            if (Convert.ToInt16(str) != 0)
                            {
                                picChange(Signal65_1, 3);
                            }
                            else
                            {
                                picChange(Signal65_1, 0);
                            }
                            break;

                        case 66:
                            if (Convert.ToInt16(str) != 0)
                            {
                                picChange(Signal66_1, 3);
                            }
                            else
                            {
                                picChange(Signal66_1, 0);
                            }
                            break;
                    }
                }
                #endregion
                #region [ XRF_GROUP02 ]
                if (string.Compare("XRF_GROUP02", SGroupName) == 0)
                {
                    switch (idx)
                    {
                        case 0:  // BRUKER 시편 유
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[0] = true;
                                picCapOpen_X2.Image = Properties.Resources.SignalBlue;
                            }
                            else
                            {
                                bAStatus_X2[0] = false;
                                picCapOpen_X2.Image = Properties.Resources.SignalYellow;
                            }
                            break;
                        case 1:  // BRUKER 시편 무
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[1] = true;
                            }
                            else
                            {
                                bAStatus_X2[1] = false;
                            }

                            break;
                        case 2:  // BRUKER 위치에 DATA 저장요구
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[2] = true;
                            }
                            else
                            {
                                bAStatus_X2[2] = false;
                            }

                            break;
                        case 3:  // BRUKER 전면투입허가(READY)
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[3] = true;
                                picXrfFrInEna_X2.Image = Properties.Resources.SignalBlue;
                                X2_STATUS_MC(5);
                                MeasureMentHistoryLog("전면투입허가 신호 On", 2);
                            }
                            else
                            {
                                bAStatus_X2[3] = false;
                                picXrfFrInEna_X2.Image = Properties.Resources.SignalEmpth;
                                MeasureMentHistoryLog("전면투입허가 신호 Off", 2);
                            }

                            break;
                        case 4:  // 시편정보 비교요구
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[4] = true;
                            }
                            else
                            {
                                bAStatus_X2[4] = false;
                            }

                            break;
                        case 5:  // 시편정보 비교OK
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[5] = true;
                            }
                            else
                            {
                                bAStatus_X2[5] = false;
                            }

                            break;
                        case 6:  // BRUKER 전면투입완료(START)
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[6] = true;
                                picXrfFrInComp_X2.Image = Properties.Resources.SignalBlue;
                                X2_STATUS_MC(10);
                                MeasureMentHistoryLog("전면투입완료 신호 On", 2);
                            }
                            else
                            {
                                bAStatus_X2[6] = false;
                                picXrfFrInComp_X2.Image = Properties.Resources.SignalEmpth;
                                MeasureMentHistoryLog("전면투입완료 신호 Off", 2);
                            }

                            break;
                        case 7:  // BRUKER 전면시험완료
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[7] = true;
                                picXrfFrTestComp_X2.Image = Properties.Resources.SignalBlue;
                            }
                            else
                            {
                                bAStatus_X2[7] = false;
                                picXrfFrTestComp_X2.Image = Properties.Resources.SignalEmpth;
                            }

                            break;
                        case 8:  // BRUKER 이면투입허가(READY)
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[8] = true;
                                picXrfBkInEna_X2.Image = Properties.Resources.SignalBlue;
                                X2_STATUS_MC(15);
                                MeasureMentHistoryLog("이면투입허가 신호 On", 2);
                            }
                            else
                            {
                                bAStatus_X2[8] = false;
                                picXrfBkInEna_X2.Image = Properties.Resources.SignalEmpth;
                                MeasureMentHistoryLog("이면투입허가 신호 off", 2);
                            }

                            break;
                        case 9:  // BRUKER 이면투입완료(START)
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[9] = true;
                                picXrfBkInComp_X2.Image = Properties.Resources.SignalBlue;

                                X2_STATUS_MC(20);
                                MeasureMentHistoryLog("이면투입완료 신호 On", 2);
                            }
                            else
                            {
                                bAStatus_X2[9] = false;
                                picXrfBkInComp_X2.Image = Properties.Resources.SignalEmpth;
                                MeasureMentHistoryLog("이면투입완료 신호 Off", 2);
                            }

                            break;
                        case 10:  // BRUKER 이면시험완료
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[10] = true;
                                picXrfBkTestComp_X2.Image = Properties.Resources.SignalBlue;
                                X2_STATUS_MC(25);
                                MeasureMentHistoryLog("이면시험완료 신호 On", 2);
                            }
                            else
                            {
                                bAStatus_X2[10] = false;
                                picXrfBkTestComp_X2.Image = Properties.Resources.SignalEmpth;
                                MeasureMentHistoryLog("이면시험완료 신호 Off", 2);
                            }

                            break;
                        case 11:  // BRUKER 이면시험PASS
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[11] = true;
                            }
                            else
                            {
                                bAStatus_X2[11] = false;
                            }

                            break;
                        case 12:  // BRUKER 시편 준비완료
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[12] = true;
                                picBackOut_X2.Image = Properties.Resources.SignalBlue;
                                X2_STATUS_MC(30);
                                MeasureMentHistoryLog("배출완료 신호 On", 2);
                            }
                            else
                            {
                                bAStatus_X2[12] = false;
                                picBackOut_X2.Image = Properties.Resources.SignalEmpth;
                                MeasureMentHistoryLog("배출완료 신호 Off", 2);
                            }

                            break;
                        case 13:  // BRUKER 공급소재없음(CARRY OUT)
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[13] = true;
                            }
                            else
                            {
                                bAStatus_X2[13] = false;
                            }

                            break;
                        case 14:  // BRUKER 이상
                            if (Convert.ToInt16(str) != 0)
                            {
                                bAStatus_X2[14] = true;
                            }
                            else
                            {
                                bAStatus_X2[14] = false;
                            }

                            break;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogData(ex.Message, "BufferDataWrite");
            }
        }
        #endregion
        #region [ 시험진행현황 그리드에 데이터(Row) 추가(cfgrdDirectListAddData) ]
        private void cfgrdDirectListAddData(string sSmplNo, string sTmbDiv, string sCarve, string sLength, string sSuji, string sProcDiv, string sProc, string sDate)
        {
            int iRow = 0;
            try
            {
                switch (Convert.ToInt16(sProcDiv))
                {
                    case 1:
                        iRow = cfgrdDirectList.Rows.Count;
                        insGrid(cfgrdDirectList, -1);
                        setGrid(cfgrdDirectList, iRow, 0, iRow.ToString());
                        setGrid(cfgrdDirectList, iRow, 1, sSmplNo);
                        setGrid(cfgrdDirectList, iRow, 2, sTmbDiv);
                        setGrid(cfgrdDirectList, iRow, 3, sCarve);
                        setGrid(cfgrdDirectList, iRow, 4, sLength);
                        setGrid(cfgrdDirectList, iRow, 5, sProcDiv);
                        setGrid(cfgrdDirectList, iRow, 6, sProc);
                        setGrid(cfgrdDirectList, iRow, 7, sDate);
                        setGrid(cfgrdDirectList, iRow, 20, sSmplNo.PadRight(8) + sTmbDiv + sCarve);

                        break;

                    case 2:
                        iRow = cfgrdDirectList.FindRow(sSmplNo.PadRight(8) + sTmbDiv + sCarve, 1, 20, true, true, true);
                        if (iRow < 0)
                        {
                            iRow = cfgrdDirectList.Rows.Count + 1;
                        }
                        setGrid(cfgrdDirectList, iRow, 1, sSmplNo);
                        setGrid(cfgrdDirectList, iRow, 2, sTmbDiv);
                        setGrid(cfgrdDirectList, iRow, 3, sCarve);
                        setGrid(cfgrdDirectList, iRow, 4, sLength);
                        setGrid(cfgrdDirectList, iRow, 5, sProcDiv);
                        setGrid(cfgrdDirectList, iRow, 8, sProc);
                        setGrid(cfgrdDirectList, iRow, 9, sDate);
                        setGrid(cfgrdDirectList, iRow, 20, sSmplNo.PadRight(8) + sTmbDiv + sCarve);

                        break;
                    case 3:
                        iRow = cfgrdDirectList.FindRow(sSmplNo.PadRight(8) + sTmbDiv + "T", 1, 20, true, true, true);
                        if (iRow < 0)
                        {
                            iRow = cfgrdDirectList.Rows.Count + 1;
                        }

                        setGrid(cfgrdDirectList, iRow, 3, sCarve);
                        setGrid(cfgrdDirectList, iRow, 5, sProcDiv);
                        setGrid(cfgrdDirectList, iRow, 10, sProc);
                        setGrid(cfgrdDirectList, iRow, 11, sDate);
                        setGrid(cfgrdDirectList, iRow, 20, sSmplNo.PadRight(8) + sTmbDiv + sCarve);
                        break;
                    case 4:
                        iRow = cfgrdDirectList.FindRow(sSmplNo.PadRight(8) + sTmbDiv + sCarve, 1, 20, true, true, true);
                        if (iRow < 0)
                        {
                            iRow = cfgrdDirectList.Rows.Count + 1;
                        }

                        setGrid(cfgrdDirectList, iRow, 12, sProc);
                        setGrid(cfgrdDirectList, iRow, 13, sDate);
                        break;
                    case 5:
                        iRow = cfgrdDirectList.FindRow(sSmplNo.PadRight(8) + sTmbDiv + sCarve, 1, 20, true, true, true);
                        if (iRow < 0)
                        {
                            iRow = cfgrdDirectList.Rows.Count + 1;
                        }

                        setGrid(cfgrdDirectList, iRow, 14, sProc);
                        setGrid(cfgrdDirectList, iRow, 15, sDate);
                        break;
                    case 6:
                        iRow = cfgrdDirectList.FindRow(sSmplNo.PadRight(8) + sTmbDiv + sCarve, 1, 20, true, true, true);
                        if (iRow < 0)
                        {
                            iRow = cfgrdDirectList.Rows.Count + 1;
                        }

                        setGrid(cfgrdDirectList, iRow, 16, sProc);
                        setGrid(cfgrdDirectList, iRow, 17, sDate);
                        break;
                    case 7:
                        iRow = cfgrdDirectList.FindRow(sSmplNo.PadRight(8) + sTmbDiv + sCarve, 1, 20, true, true, true);
                        if (iRow < 0)
                        {
                            iRow = cfgrdDirectList.Rows.Count + 1;
                        }

                        setGrid(cfgrdDirectList, iRow, 18, sProc);
                        setGrid(cfgrdDirectList, iRow, 19, sDate);
                        setGrid(cfgrdDirectList, iRow, 20, "");
                        break;
                }
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "cfgrdDirectListAddData");
            }

        }
        #endregion
        #region [ 버퍼정보 클리어(X1BufferDataClear) ]
        private void X1BufferDataClear()
        {
            object[] oWriteData = null;
            oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 21;

            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("X1 Buffer Clear Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("X1 All Buffer Clear.");
                for (int iLength = 0; iLength < XrfControl.m_X1XrfBufferSize; iLength++)
                {
                    if (string.IsNullOrEmpty(sSPL[iLength]))
                        sSPL[iLength] = string.Empty;
                    if (string.IsNullOrEmpty(sbBufGridAppl[iLength]))
                        sbBufGridAppl[iLength] = string.Empty;
                }
            }
        }
        #endregion
        #region [ 버퍼정보 클리어(X2BufferDataClear) ]
        private void X2BufferDataClear()
        {
            string sGroupName = string.Empty;
            sGroupName = "XRF_GROUP01";
            int[] iTagHandle = new int[] { 27 };
            object[] oWriteData = new object[] { true };

            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, sGroupName, ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("X2 Buffer Clear Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("X2 All Buffer Clear.");
                for (int iLength = XrfControl.m_X1XrfBufferSize; iLength < XrfControl.m_XrfBuffer.Length; iLength++)
                {
                    if (string.IsNullOrEmpty(sSPL[iLength]))
                        sSPL[iLength] = string.Empty;
                    if (string.IsNullOrEmpty(sbBufGridAppl[iLength]))
                        sbBufGridAppl[iLength] = string.Empty;
                }
            }
        }
        #endregion
        #region [ Panalytical 시험기 진행 정보 PLC에 전송 ]

        #region [ 전면투입 허가(Xrf_Front_in_Enable) ]
        /// <summary>
        /// PLC로 전면투입허가 신호보내기
        /// </summary>
        private void Xrf_Front_in_Enable()
        {

            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 4;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF 전면투입허가신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF 전면투입허가신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }
        #endregion
        #region [ 전면시험 완료(Xrf_Front_Measure_Complete) ]
        /// <summary>
        /// PLC로 전면시험완료 신호보내기
        /// </summary>
        private void Xrf_Front_Measure_Complete()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 6;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF 전면시험완료신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF 전면시험완료신호 전송완료");
            }
        }
        #endregion
        #region [ 이면투입 허가(Xrf_Back_in_Enable) ]
        /// <summary>
        /// PLC로 이면투입허가 신호보내기
        /// </summary>
        private void Xrf_Back_in_Enable()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 7;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF 이면투입허가신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF 이면투입허가신호 전송완료");
            }
        }
        #endregion
        #region [ 이면시험 완료(Xrf_Back_Measure_Complete) ]
        /// <summary>
        /// PLC로 이면시험완료 신호보내기
        /// </summary>
        private void Xrf_Back_Measure_Complete()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 9;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF 이면투입허가신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF 이면투입허가신호 전송완료");
            }
        }
        #endregion
        #region [ 이면 배출 신호(XRF_Back_Out_Req) ]
        /// <summary>
        /// PLC로 이면배출신호 전송
        /// </summary>
        private void XRF_Back_Out_Req()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 9;

            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF 이면배출신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF 이면배출신호 전송완료");
            }
        }
        #endregion

        #endregion
        #region [ BRUKER 시험기 진행 정보 PLC에 전송 ]

        #region [ 전면투입 허가(Xrf_Front_in_Enable_X2) ]
        /// <summary>
        /// PLC로 전면투입허가 신호보내기
        /// </summary>
        private void Xrf_Front_in_Enable_X2()
        {

            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 3;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP02", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF2 전면투입허가신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF2 전면투입허가신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }
        #endregion
        #region [ 전면투입 완료 신호 지우기(Xrf_Front_in_EnableOff_X2) ]
        /// <summary>
        /// PLC로 전면투입완료 신호 지우기
        /// </summary>
        private void Xrf_Front_in_EnableOff_X2()
        {

            object[] oWriteData = new object[1];
            oWriteData[0] = false;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 7;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP02", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF2 전면투입완료취소신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF2 전면투입완료취소신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }
        #endregion
        #region [ 전면시험완료 신호보내기(Xrf_Front_Measure_Complete_X2) ]
        /// <summary>
        /// PLC로 전면시험완료 신호보내기
        /// </summary>
        private void Xrf_Front_Measure_Complete_X2()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 7;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP02", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF2 전면시험완료신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF2 전면시험완료신호 전송완료");
            }
        }
        #endregion
        #region [ 이면투입허가 신호보내기(Xrf_Back_in_Enable_X2) ]
        /// <summary>
        /// PLC로 이면투입허가 신호보내기
        /// </summary>
        private void Xrf_Back_in_Enable_X2()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 8;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP02", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF2 이면투입허가신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF2 이면투입허가신호 전송완료");
            }
        }
        #endregion
        #region [ 이면시험완료 신호보내기(Xrf_Back_Measure_Complete_X2) ]
        /// <summary>
        /// PLC로 이면시험완료 신호보내기
        /// </summary>
        private void Xrf_Back_Measure_Complete_X2()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 10;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP02", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF2 이면시험완료 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF2 이면시험완료 전송완료");
            }
        }
        #endregion
        #region [ 이면투입완료 신호 지우기(Xrf_Back_Measure_CompleteOff_X2) ]
        /// <summary>
        /// PLC로 이면투입완료 Off 신호보내기
        /// </summary>
        private void Xrf_Back_Measure_CompleteOff_X2()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 9;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP02", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF2 이면투입완료 Off 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF2 이면투입완료 Off 전송완료");
            }
        }
        #endregion
        #region [ 시편배출완료 신호보내기(Xrf_SmlOut_Complete_X2) ]
        /// <summary>
        /// PLC로 시편배출완료 신호보내기
        /// </summary>
        private void Xrf_SmlOut_Complete_X2()
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 12;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP02", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF2 시편배출완료신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF2 시편배출완료신호 전송완료");
            }
        }
        #endregion

        #endregion

        private void Send_Item_Info()
        {

            try
            {
                if (grdNo.Rows.Count < 2)
                {
                    OPCSendCarryOut();
                    //MessageBox.Show("시편 정보를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                //var sData = grdNo.GetData(1, 1);
                //if (sData == null)
                //{
                //    OPCSendCarryOut();
                //    //MessageBox.Show("시편 정보를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                //sData = grdNo.GetData(1, 2);
                //if (sData == null)
                //{
                //    MessageBox.Show("TMB구분을 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                //sData = grdNo.GetData(1, 3);
                //if (sData == null)
                //{
                //    MessageBox.Show("길이를 입력하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}

                var sNo = grdNo.GetData(1, "시편번호");
                var sTMB = grdNo.GetData(1, "TMB");
                sNo = sNo.ToString().PadRight(8, ' ');
                sNo = sNo + sTMB.ToString().Trim();                                                               // 시편번호(시편번호 + T) 9자리

                var sLength = grdNo.GetData(1, "길이");                                                           // 소재길이(1WORD)
                var sDeviceName = grdNo.GetData(1, "시험기");                                                     // 시험기 코드
                var bCleanYN = grdNo.GetData(1, "세척유무") == null ? false : grdNo.GetData(1, "세척유무");       // 세척유무
                var sDivType = grdNo.GetData(1, "DivType");                                                       // 시험구분코드 문자열(WCD)


                object[] oWriteData = null;
                int[] iTagHandle = null;

                oWriteData = new object[OPCFunction.TAG_CONV_LOC];
                oWriteData[0] = sLength;                                                // 소재길이
                oWriteData[1] = sNo;                                                    // 시편번호
                oWriteData[17] = sDeviceName;                                           // 시험기코드
                oWriteData[13] = bCleanYN;                                              // 세척유무(name="M5005")

                // 시험구분 초기화
                oWriteData[14] = true;                                                // 제거(name="M5006"~name="M5008") 
                oWriteData[15] = true;                                                // 제거(name="M5006"~name="M5008") 
                oWriteData[16] = true;                                                // 제거(name="M5006"~name="M5008") 

                for (int iLength = 0; iLength < sDivType.ToString().Length; iLength++)
                {
                    switch (sDivType.ToString().Substring(iLength, 1))
                    {
                        case "W":
                            oWriteData[14] = false;
                            break;
                        case "C":
                            oWriteData[15] = false;
                            break;
                        case "D":
                            oWriteData[16] = false;
                            break;
                    }
                }

                sTmpLength = sLength.ToString();  // TB_XRF_FA_DIRECT 에 저장할 길이를 저장한다.

                int iResFunc = OPCFunction._OPC_Write_Ascyn_Group_Tags(m_opcMgr, "XRF_CONV_LOC", ref oWriteData);
                if (iResFunc != 1)
                {
                    InsertLog("소재정보 Write Failed! Error Code:" + iResFunc);
                }
                else
                {
                    InsertLog("소재정보 Write");
                }
                Thread.Sleep(300);


                oWriteData = new object[1];
                oWriteData[0] = true;
                iTagHandle = new int[1];
                iTagHandle[0] = 1;

                iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
                if (iResFunc != 1)
                {
                    InsertLog("소재정보 전송완료 Write Failed! Error Code:" + iResFunc);
                }
                else
                {
                    InsertLog("소재정보 전송완료 신호 Write");
                }

                if (!dicSampleInfo.ContainsKey(grdNo[1, "시편번호"].ToString().ToUpper()))
                {
                    dicSampleInfo.Add(grdNo[1, "시편번호"].ToString().ToUpper(), grdNo[1, "Application"].ToString().ToUpper());
                }
                else
                {
                    dicSampleInfo[grdNo[1, "시편번호"].ToString().ToUpper()] = grdNo[1, "Application"].ToString().ToUpper();
                }
                //dicSampleInfo.Add(grdNo[1, "시편번호"].ToString(), grdNo[1, "Application"].ToString());
                delGrid(grdNo, 1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OPCSendCarryOut()
        {
            object[] oWriteData = null;
            oWriteData = new object[OPCFunction.TAG_GROUP01];
            oWriteData[11] = true;   // Carry Out
            oWriteData[20] = false;  // Start Off

            int iResFunc = OPCFunction._OPC_Write_Ascyn_Group_Tags(m_opcMgr, "XRF_GROUP01", ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("CARRY Out Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("CARRY Out Write 성공");
            }

            m_bFlagStart = false;
            setText(btnActStart, "Start");
        }

        private void OPCSendSignalClear()
        {
            object[] oWriteData = null;
            oWriteData = new object[OPCFunction.TAG_GROUP01];
            oWriteData[4] = false;
            oWriteData[5] = false;
            oWriteData[6] = false;
            oWriteData[7] = false;
            oWriteData[8] = false;
            oWriteData[9] = false;

            int iResFunc = OPCFunction._OPC_Write_Ascyn_Group_Tags(m_opcMgr, "XRF_GROUP01", ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("SIGNAL CLEAR FAIL" + iResFunc);
            }
            else
            {
                InsertLog("SIGNAL CLEAR 성공");
            }

        }

        private void BufferClear(int nSelBufNumber)
        {
            if (grdBuffer.Rows[grdBuffer.RowSel][grdBuffer.ColSel] == null) return;

            if (MessageBox.Show(grdBuffer.Rows[grdBuffer.RowSel][grdBuffer.ColSel].ToString() + "\r\n" + "선택한 버퍼에 있는 정보를 삭제하시겠습니까?", "Buffer Clear", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                object[] oWriteData = null;
                oWriteData = new object[1];
                oWriteData[0] = "";
                int[] iTagHandle = new int[1];
                iTagHandle[0] = (grdBuffer.ColSel - 1) * 22 + grdBuffer.RowSel - 1;


                int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_BUFFER", ref iTagHandle, ref oWriteData);
                if (iResFunc != 1)
                {
                    InsertLog("Buffer Clear Failed! Error Code:" + iResFunc);
                }
                else
                {
                    InsertLog(grdBuffer.Rows[nSelBufNumber % 22][(nSelBufNumber - 1) / 22].ToString() + "번 Buffer Clear.");
                }
                sbBufGridAppl[nSelBufNumber - 1] = string.Empty;
                sSPL[nSelBufNumber - 1] = string.Empty;

                grdBuffer.Select(-1, -1);
            }
        }


        private void grdBuffer_DoubleClick(object sender, EventArgs e)
        {
            if (m_bFlagStart)
            {
                MessageBox.Show("기기 가동중에는 Buffer정보를 지울수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BufferClear((grdBuffer.ColSel - 1) * 22 + grdBuffer.RowSel);
        }


        private void LblLoc5_DoubleClick(object sender, EventArgs e)
        {
            if (m_bFlagStart)
            {
                MessageBox.Show("기기 가동중에는 Location내 저장정보를 지울수 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Label label = (Label)sender;
            LocationInfoClear(label);
        }

        private void LocationInfoClear(Label label)
        {
            if (label.Text.Length == 0) return;

            string sLocName = "";
            object[] oWriteData = new object[1];
            int[] iTagHandle = new int[1];
            oWriteData[0] = "";

            switch (label.Name.ToString())
            {
                case "LblLoc3":
                    iTagHandle[0] = 2;
                    sLocName = "소재공급부";
                    break;
                case "LblLoc4":
                    iTagHandle[0] = 3;
                    sLocName = "소재이송부";
                    break;
                case "LblLoc5":
                    iTagHandle[0] = 4;
                    sLocName = "Press";
                    break;
                case "LblLoc6":
                    iTagHandle[0] = 5;
                    sLocName = "투입";
                    break;
                case "LblLoc7":
                    iTagHandle[0] = 6;
                    sLocName = "마킹";
                    break;
                case "LblLoc8":
                    iTagHandle[0] = 7;
                    sLocName = "세척";
                    break;
                case "LblLoc9":
                    iTagHandle[0] = 8;
                    sLocName = "배출";
                    break;
                case "LblLoc10":
                    iTagHandle[0] = 9;
                    sLocName = "로보트 Gripper";
                    break;
            }

            if (MessageBox.Show(label.Text + "\r\n" + "선택한 위치에 있는 정보를 삭제하시겠습니까?", "정보 삭제", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {

                int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_CONV_LOC", ref iTagHandle, ref oWriteData);
                if (iResFunc != 1)
                {
                    InsertLog("위치 정보 Clear Failed! Error Code:" + iResFunc);
                }
                else
                {
                    InsertLog(sLocName + " 위치 정보 Clear.");
                }
            }

        }

        //private void btnConnect_Click(object sender, EventArgs e)
        //{
        //    XrfControl.m_Xrf_IP = GetIniValue("SERVER", "IP", sIniPath);

        //    IPAddress[] ipAddress = Dns.GetHostAddresses(XrfControl.m_Xrf_IP);
        //    IPEndPoint remoteEP = new IPEndPoint(ipAddress[0], 701);

        //    // Create a TCP/IP socket.
        //    client = new Socket(AddressFamily.InterNetwork,
        //        SocketType.Stream, ProtocolType.Tcp);

        //    SocketClient.Connect(remoteEP, client);

        //    Receive(client);

        //    XRF_Send_ENQ();

        //    timer.Enabled = true;
        //}

        private void XRF_Send_ENQ()
        {
            if (!SocketClient.SocketConnectCheck(client))
            {
                MessageBox.Show("XRF와 연결이 끊어졌습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string str = char.ConvertFromUtf32(5);
            SocketClient.Send(client, str);
            setText(lstUAIMessage, "<--ENQ");
            StateObject.Send_ENQ = true;
        }

        private void XRF_Send_ACK()
        {
            if (!SocketClient.SocketConnectCheck(client))
            {
                MessageBox.Show("XRF와 연결이 끊어졌습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string str = char.ConvertFromUtf32(6);
            SocketClient.Send(client, str);
            setText(lstUAIMessage, "<--ACK");
            StateObject.Send_ACK = true;
        }

        private void XRF_Send_NAK()
        {
            if (!SocketClient.SocketConnectCheck(client))
            {
                MessageBox.Show("XRF와 연결이 끊어졌습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string str = char.ConvertFromUtf32(21);
            SocketClient.Send(client, str);
            setText(lstUAIMessage, "<--NAK");
            StateObject.Send_NAK = true;
        }

        private void XRF_Send_MSG(string XRF_Msg)
        {
            if (!SocketClient.SocketConnectCheck(client))
            {
                MessageBox.Show("XRF와 연결이 끊어졌습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SocketClient.Send(client, XRF_Msg);
            setText(lstUAIMessage, "<--" + XRF_Msg);
            XrfControl.WriteLogData(XRF_Msg, "Send Data");
            StateObject.Send_MSG = true;
        }

        // for BRUKER
        private void XRF_Send_MSG_X2(string XRF_Msg, bool bLog)
        {
            if (!SocketClient.SocketConnectCheck(client_X2))
            {
                MessageBox.Show("XRF2와 연결이 끊어졌습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SocketClient.Send_X2(client_X2, XRF_Msg);
            if (bLog)
            {
                setText(lstUAIMessage2, "<--" + XRF_Msg);
                XrfControl.WriteLogData(XRF_Msg, "Send Data");
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (client == null) return;
            client.Disconnect(false);
            client.Close();

        }


        private static ManualResetEvent receiveDone =
                new ManualResetEvent(false);
        private static ManualResetEvent receiveDone_X2 =
                new ManualResetEvent(false);

        private static String response = String.Empty;
        private static String response_X2 = String.Empty; // for BRUKER

        private void Receive(Socket client)
        {
            //if (!SocketClient.ConnectFlag) return;

            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;

                // Server로부터 데이터 수신 시작
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogData(e.Message, "Receive");
            }
        }

        // for BRUKER
        private void Receive_X2(Socket client)
        {
            //if (!SocketClient.ConnectFlag) return;

            try
            {
                StateObject_X2 state_X2 = new StateObject_X2();
                state_X2.workSocket = client;

                // Server로부터 데이터 수신 시작
                client.BeginReceive(state_X2.buffer, 0, StateObject_X2.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback_X2), state_X2);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogData(e.Message, "Receive");
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Server에서 데이터 Read
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // 남은 데이타가 있을수 있기에 Data를 저장
                    //state.m_Response.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    response = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    WriteLogData(response, "ReceiveCallback");
                    XRF_Receve_Data(response);
                    // 나머지 데이터를 가져온다.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    // 모든 데이터 수신완료
                    if (state.m_Response.Length > 1)
                    {
                        response = state.m_Response.ToString();
                    }
                    // 수신완료 신호
                    receiveDone.Set();

                    // 수신 데이터 가공
                    XRF_Receve_Data(response);
                }
            }
            catch (Exception e)
            {
                WriteLogData(e.Message, "ReceiveCallback");
            }
        }

        // for BRUKER
        private void ReceiveCallback_X2(IAsyncResult ar)
        {
            try
            {
                StateObject_X2 state = (StateObject_X2)ar.AsyncState;
                Socket client_X2 = state.workSocket;

                // Server에서 데이터 Read
                int bytesRead = client_X2.EndReceive(ar);

                for (int iCnt = 0; iCnt < bytesRead; iCnt++)
                {
                    if (state.buffer[iCnt] == 13)
                    {
                        bytesRead = iCnt;
                        break;
                    }
                }
                if (bytesRead > 0)
                {
                    response_X2 = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                    XRF_Receve_Data_X2(response_X2);
                    WriteLogData(response, "ReceiveCallback_X2");
                }
            }
            catch (Exception e)
            {
                WriteLogData(e.Message, "ReceiveCallback_X2");
            }
            finally
            {
                Receive_X2(client_X2);
            }
        }

        private string m_recv_data = string.Empty;
        private bool m_recv_flag = false;
        private string m_recv_data_X2 = string.Empty; // for BRUKER
        private bool m_recv_flag_X2 = false;          // for BRUKER

        private void XRF_Receve_Data(string response)
        {
            char ch;
            string[] sRecevData = null;

            if (response.Length == 2)
            {
                ch = response[0];
                switch (ch)
                {
                    case (char)5:  // ENQ
                        if (StateObject.Send_MSG)   // MSG를 보낸상태에서 ENQ를 받은것이면
                        {
                            setText(lstUAIMessage, "-->ENQ");
                            XRF_Send_ACK();
                            StateObject.Recv_ENQ = true;
                            StateObject.Recv_ACK = false;
                        }
                        break;
                    case (char)6:  // ACK
                        if (StateObject.Send_ENQ)   // ENQ를 보낸상태에서 ACK를 받은것이면
                        {
                            setText(lstUAIMessage, "-->ACK");
                            //XRF_Send_MSG(txtSendData.Text);
                            StateObject.Send_ENQ = false;
                            StateObject.Recv_ACK = true;
                            timer.Enabled = false;
                        }
                        break;
                    case (char)21:  // NAK
                        if (StateObject.Send_ENQ)   // ENQ를 보낸상태에서 NAK를 받은것이면
                        {
                            setText(lstUAIMessage, "-->NAK");
                            XRF_Send_ENQ();
                            StateObject.Recv_NAK = true;
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (response.Length > 2)
            {
                if (response.Length > 4)
                {
                    ch = response[0];
                    switch (ch)
                    {
                        case (char)5:   // ENQ
                        case (char)6:   // ACK
                        case (char)21:  // NAK
                            break;
                        default:
                            if (StateObject.Send_MSG && StateObject.Send_ACK)   // ENQ를 보낸상태에서 ACK를 받은것이면
                            {
                                if (response.Length >= 263)   // Result 값을 받을 때 결과값이 263 byte를 넘어가는 경우가 있다.
                                {
                                    char cEtx = response[262];
                                    if (cEtx == (char)3)
                                    {
                                        if (response.Substring(257, 3) == "END")   // 결과값이 263 byte일 경우 남은 값이 있는지 확인 필요
                                        {
                                            m_recv_data = response;

                                            sRecevData = XrfControl.XRF_Message_Parsing(m_recv_data);
                                            setText(lstUAIMessage, "[RX] -----------------------------------");
                                            for (int iLoop = 0; iLoop < sRecevData.Length; iLoop++)
                                            {
                                                setText(lstUAIMessage, sRecevData[iLoop]);
                                            }
                                            setText(lstUAIMessage, "------------------------------------ [RX]");
                                            StateObject.Send_MSG = true;
                                            StateObject.Send_ACK = true;
                                            StateObject.Recv_MSG = true;
                                            m_recv_flag = false;
                                            m_recv_data = string.Empty;
                                        }
                                        else
                                        {
                                            m_recv_data = response.Substring(0, response.Length - 3);
                                            m_recv_flag = true;
                                        }
                                    }
                                    else
                                    {
                                        XrfControl.WriteLogData(response, "Error");
                                        m_recv_flag = false;
                                    }
                                    XRF_Send_ACK();
                                }
                                else
                                {
                                    if (m_recv_flag)
                                    {
                                        m_recv_data = m_recv_data + response.Substring(4);
                                        //XrfControl.WriteLogData(m_recv_data, "Receive2");
                                    }
                                    else
                                    {
                                        m_recv_data = response;
                                        //XrfControl.WriteLogData(m_recv_data, "Receive");
                                    }
                                    sRecevData = XrfControl.XRF_Message_Parsing(m_recv_data);
                                    setText(lstUAIMessage, "[RX] -----------------------------------");
                                    for (int iLoop = 0; iLoop < sRecevData.Length; iLoop++)
                                    {
                                        setText(lstUAIMessage, sRecevData[iLoop]);
                                    }
                                    setText(lstUAIMessage, "------------------------------------ [RX]");
                                    XRF_Send_ACK();

                                    //if (XRF_COMMAND.XRF_AUTO)   // 자동 모드일때만
                                    //{
                                    XRF_Received_Data_Analyze(sRecevData);
                                    //}

                                    StateObject.Send_MSG = true;
                                    StateObject.Send_ACK = true;
                                    StateObject.Recv_MSG = true;
                                    m_recv_flag = false;
                                    m_recv_data = string.Empty;
                                }
                            }
                            break;
                    }
                }
            }
        }

        // for BRUKER
        List<string> lstCommand = new List<string>();
        private void XRF_Receve_Data_X2(string response)
        {
            string sStatus = string.Empty;
            int nRStatusNum = -1; // Reply Status Number
            if (!response.Contains("STATUS"))
                setText(lstUAIMessage2, "<--" + response);
            #region [READRS]
            if (response.Contains("READRS"))
            {   // READ RESULTS
                nRStatusNum = Convert.ToInt32(response.Substring(7, 2));
                string sCommand = string.Empty;

                switch (nRStatusNum)
                {
                    case 0: // COMMAND ACCEPTED
                        BrukerMethod.getTestResult(response);
                        // 시험결과 중복해서 가져오지 않기 위해 마지막 시험결과 가지고 있음
                        sLastTestResultSML = TestResult.Instance.SampleNumber.PadRight(8, ' ') + TestResult.Instance.TMBDiv + TestResult.Instance.PunchLocation + TestResult.Instance.FrontAndBack;

                        ///////////// 2017.04.29 김정건 ///////////////////////////////////////////////////
                        //ApplicationName의 원소테이블



                        string strSmplno = TestResult.Instance.SampleNumber;
                        string strTmbdiv = TestResult.Instance.TMBDiv;//.sTmb;
                        string[] strParams = new string[] { strSmplno
                                                   ,strTmbdiv
                                                   ,TestResult.Instance.ApplicationName
                                                    };
                        DataTable dtElement = CommonDataBase.GetXRFApplicationElement(strParams);
                        if (dtElement.Columns.Count == 0)
                        {
                            dtElement.Columns.Add("LINENAME", typeof(string));
                            dtElement.Columns.Add("COILNAME", typeof(string));
                            dtElement.Columns.Add("SHTSPEC", typeof(string));
                            dtElement.Columns.Add("AFTCODE", typeof(string));
                            dtElement.Columns.Add("ELEMENT", typeof(string));
                            dtElement.Columns.Add("COMPUTEMODIFY", typeof(string));
                            dtElement.Columns.Add("COLNAME", typeof(string));
                            dtElement.Columns.Add("APPNAME", typeof(string));
                        }
                        dtElement.Columns.Add("ELEMENTVALUE", typeof(string));
                        dtElement.Columns.Add("DATE", typeof(string));
                        dtElement.Columns.Add("BEFOREELEMENTVALUE", typeof(string));
                        dtElement.Columns.Add("WCD", typeof(string));
                        dtElement.Columns.Add("SMPLNO", typeof(string));
                        dtElement.Columns.Add("TMBDIV", typeof(string));
                        dtElement.Columns.Add("FRONTBACK", typeof(string));

                        //TestResult.Instance.TestElement.Keys.Count
                        for (int iElement = 0; iElement < TestResult.Instance.TestElement.Keys.Count; iElement++)
                        {//string배열이 가진 원소기호 이름
                            string strElementName = TestResult.Instance.GetElementName(iElement);//기계에서 입력받은 원소 이름
                            bool bolElement = false; //기준정보에 등록된 원소라면 True
                            for (int iRow = 0; iRow < dtElement.Rows.Count; iRow++)
                            {
                                DataRow drTemp = dtElement.Rows[iRow];
                                if (strElementName.ToUpper().Equals(drTemp["ELEMENT"].ToString().ToUpper()))
                                {//TestResult클래스가 가진 원소기호와 기준정보의 원소기호가 동일하다면
                                    drTemp["ELEMENTVALUE"] = Math.Round(TestResult.Instance.TestElement[strElementName], 1, MidpointRounding.AwayFromZero);//원래값
                                    drTemp["BEFOREELEMENTVALUE"] = Math.Round(TestResult.Instance.TestElement[strElementName], 1, MidpointRounding.AwayFromZero);//원래값
                                    drTemp["DATE"] = TestResult.Instance.TestDateTime.ToString("yyyy-MM-dd HH:mm:ss"); //날짜
                                    drTemp["WCD"] = TestResult.Instance.PunchLocation; //WCD
                                    drTemp["SMPLNO"] = TestResult.Instance.SampleNumber; //시편번호
                                    drTemp["TMBDIV"] = TestResult.Instance.TMBDiv; //TMBDIV
                                    drTemp["FRONTBACK"] = TestResult.Instance.FrontAndBack.Equals("F") ? "FRONT" : "BACK"; //전면이면
                                    bolElement = true;
                                }
                            }

                            if (!bolElement)
                            {//기계에서 입력받은 원소 != DB에서 받아온 원소
                                DataRow drTemp = dtElement.Rows.Add();
                                drTemp["ELEMENT"] = strElementName;
                                drTemp["APPNAME"] = TestResult.Instance.ApplicationName;
                                drTemp["ELEMENTVALUE"] = Math.Round(TestResult.Instance.TestElement[strElementName], 1, MidpointRounding.AwayFromZero);//원래값
                                drTemp["DATE"] = TestResult.Instance.TestDateTime.ToString("yyyy-MM-dd HH:mm:ss"); //날짜
                                drTemp["WCD"] = TestResult.Instance.PunchLocation; //WCD
                                drTemp["SMPLNO"] = TestResult.Instance.SampleNumber; //SMPLNO
                                drTemp["TMBDIV"] = TestResult.Instance.TMBDiv; //TMBDIV
                                drTemp["FRONTBACK"] = TestResult.Instance.FrontAndBack.Equals("F") ? "FRONT" : "BACK"; //전면이면
                            }
                        }

                        for (int iElementRow = 0; iElementRow < dtElement.Rows.Count; iElementRow++)
                        {//DB에서 가져온 테이블 FOR문돌려서 변경된값 담는다
                            if (!string.IsNullOrEmpty(dtElement.Rows[iElementRow]["LINENAME"].ToString()))
                            {//기준정보에 등록된 원소라면 계산수식을 돌린다
                                strParams = new string[] { dtElement.Rows[iElementRow]["LINENAME"].ToString()
                                                        ,dtElement.Rows[iElementRow]["COILNAME"].ToString()
                                                        ,dtElement.Rows[iElementRow]["SHTSPEC"].ToString()
                                                        ,dtElement.Rows[iElementRow]["AFTCODE"].ToString()
                                                        ,dtElement.Rows[iElementRow]["ELEMENT"].ToString().ToUpper()
                                                        ,dtElement.Rows[iElementRow]["APPNAME"].ToString()
                                                        };
                                DataTable dtDetail = CommonDataBase.GetXRFApplicationDetail(strParams);
                                InoCompute ino = new InoCompute(dtElement, dtDetail);
                                dtElement.Rows[iElementRow]["ELEMENTVALUE"] = ino.Compute(dtElement.Rows[iElementRow]["ELEMENT"].ToString().ToUpper());//변경된 원소값 저장
                            }
                            else
                            {//기준정보에 등록되지 않은원소
                                dtElement.Rows[iElementRow]["BEFOREELEMENTVALUE"] = dtElement.Rows[iElementRow]["ELEMENTVALUE"];
                                dtElement.Rows[iElementRow]["ELEMENTVALUE"] = string.Empty;
                            }
                        }

                        //////////////////////////////////////////////////////////////////////////////////
                        string[] iData = new string[9];
                        iData[0] = TestResult.Instance.SampleNumber;// sSmplNo;
                        iData[1] = TestResult.Instance.TMBDiv;//.sTmb;
                        iData[2] = TestResult.Instance.PunchLocation;// sWcd;
                        iData[3] = TestResult.Instance.FrontAndBack;// s_FB;
                        iData[4] = Math.Round((!TestResult.Instance.TestElement.Keys.Contains("FE") ? 0 : TestResult.Instance.TestElement["FE"]), 1, MidpointRounding.AwayFromZero).ToString(); //Math.Round(dFe, 1, MidpointRounding.AwayFromZero).ToString();
                        iData[5] = Math.Round((!TestResult.Instance.TestElement.Keys.Contains("ZN") ? 0 : TestResult.Instance.TestElement["ZN"]), 1, MidpointRounding.AwayFromZero).ToString();
                        iData[6] = Math.Round((!TestResult.Instance.TestElement.Keys.Contains("CR") ? 0 : TestResult.Instance.TestElement["CR"]), 1, MidpointRounding.AwayFromZero).ToString();//dCr.ToString();
                        iData[7] = Math.Round((!TestResult.Instance.TestElement.Keys.Contains("P") ? 0 : TestResult.Instance.TestElement["P"]), 1, MidpointRounding.AwayFromZero).ToString();// Math.Round(dP, 1, MidpointRounding.AwayFromZero).ToString();
                        iData[8] = TestResult.Instance.TestDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                        for (int iCnt = 0; iCnt < iData.Length; iCnt++)
                        {
                            setText(lstUAIMessage2, "iData[" + iCnt.ToString() + "] : " + iData[iCnt]);
                        }

                        //string[] strResultHistory = new string[iData.Length];
                        //iData.CopyTo(strResultHistory, 0);
                        //CommonDataBase cDb = new CommonDataBase();
                        //cDb.Execute_BackGroundWorker(strResultHistory, 5);
                        CommonDataBase cDb = new CommonDataBase();
                        cDb.Execute_SaveData(dtElement, "History");
                        Thread.Sleep(100);

                        //string[] strTestResult = new string[iData.Length];
                        //iData.CopyTo(strTestResult, 0);
                        //CommonDataBase cDb2 = new CommonDataBase();
                        //cDb2.Execute_BackGroundWorker(strTestResult, 6);
                        //Thread.Sleep(100);

                        tb_Result_Display(iData);

                        iData[2] = "Y";
                        iData[3] = "Y";
                        CommonDataBase cDb4 = new CommonDataBase();
                        cDb4.Execute_BackGroundWorker(iData, 1);

                        Thread.Sleep(10000);
                        XRF_COMMAND xcmd_X2 = new XRF_COMMAND();
                        sCommand = xcmd_X2.XRF_BRUKER_UNLOAD(m_TestList[0].sSPLNAME);
                        XRF_Send_MSG_X2(sCommand, true);
                        break;
                }
            }
            #endregion
            #region [STATUS]
            if (response.Contains("STATUS"))
            {
                if (bX2Proc_Start == false)
                {
                    XRF_Start_Wait_X2();
                    bX2Proc_Start = true;
                }

                //nRStatusNum = Convert.ToInt32(response.Substring(7, 2));
                string sCommand = string.Empty;
                int iSmplnoCount = 0;                                                                   // 시험기에 구동중인 시편 수
                string[] strArrResponse = BrukerMethod.getBrukerAnswerStringSplit(response);

                // 에러응답
                if (strArrResponse.Length < 2)
                    return;

                //switch (nRStatusNum)
                switch (Convert.ToInt32(strArrResponse[1]))
                {
                    case 2: // Sample List
                        iSmplnoCount = (strArrResponse.Length - 2) / 3;

                        // 전면투입허가 신호가 없을때
                        if (iSmplnoCount == 0 && !bAStatus_X2[3])
                        {
                            X2_STATUS_MC(0);
                            //MeasureMentHistoryLog("전면투입처리 요청 X2_STATUS_MC(0)", 2);
                        }


                        //if (response.Length >= 28)
                        if (iSmplnoCount > 0)
                        {
                            // STATUS CODE에 따른 처리
                            switch (strArrResponse[3])
                            {
                                case "10":
                                    // 시험시작
                                    // 시편이 존재할때 이전 시편이 시험시작하고 전면투입신호가 없을때 추가 투입
                                    if (iSmplnoCount < m_MinBufferSampleCount && !bAStatus_X2[3])
                                    {
                                        X2_STATUS_MC(0);
                                    }
                                    break;
                                case "12":
                                    // 시험완료
                                    // MEASUREMENT FINISHED, RESULTS AVAILABLE
                                    // READ RESULTS
                                    if (!sLastTestResultSML.Equals(m_TestList[0].sSPLNAME))
                                    {
                                        XRF_COMMAND.UNLOAD_STATUS = true;

                                        setText(lstUAIMessage2, "-->READRS");
                                        XRF_COMMAND xcmd_X2 = new XRF_COMMAND();
                                        sCommand = xcmd_X2.XRF_BRUKER_READRS(strArrResponse[2]);
                                        XRF_Send_MSG_X2(sCommand, true);
                                    }
                                    break;
                                case "21":
                                    // UNLOAD 명령 전송
                                    // UNLOAD SAMPLE FROM MAGAZINE ONTO CONVEYOR BELT
                                    break;
                                case "03":
                                    // UNLOAD시 Z01에 시편이 올라감
                                    // 시편이 없으면 루프 있으면 다음
                                    while (!bAStatus_X2[0])
                                    {
                                        Thread.Sleep(500);
                                    }
                                    // F_SIDE => 이면 투입명령
                                    if (m_TestList[0].sSPLNAME.Substring(m_TestList[0].sSPLNAME.Length - 1).ToUpper().Equals("F"))
                                    {   // 이면 투입 허가
                                        Xrf_Back_in_Enable_X2();
                                    }
                                    else if (m_TestList[0].sSPLNAME.Substring(m_TestList[0].sSPLNAME.Length - 1).ToUpper().Equals("B"))
                                    {   // 이면 시험 완료
                                        Xrf_Back_Measure_Complete_X2();
                                    }
                                    break;
                            }
                        }
                        break;  //02
                }
            }
            #endregion
            if (response.Contains("UNLOAD"))
            {
                nRStatusNum = Convert.ToInt32(response.Substring(7, 2));
                string sCommand = string.Empty;

                if (!nRStatusNum.Equals(0))
                {
                    Thread.Sleep(2000);
                    XRF_COMMAND xcmd_X2 = new XRF_COMMAND();
                    sCommand = xcmd_X2.XRF_BRUKER_UNLOAD(m_TestList[0].sSPLNAME);
                    XRF_Send_MSG_X2(sCommand, true);
                }
            }
            //if (response.Contains("MEASMP"))
            //{
            //    nRStatusNum = Convert.ToInt32(response.Substring(7, 2));
            //    string sCommand = string.Empty;

            //    if (!nRStatusNum.Equals(0))
            //    {
            //        XRF_COMMAND xcmd_X2 = new XRF_COMMAND();
            //        sCommand = xcmd_X2.XRF_BRUKER_UNLOAD(m_TestList[0].sSPLNAME);
            //        XRF_Send_MSG_X2(sCommand, true);
            //    }
            //}
        }

        private void lstEquipMessage_DoubleClick(object sender, EventArgs e)
        {
            lstEquipMessage.Items.Clear();
        }

        private void lstUAIMessage_DoubleClick(object sender, EventArgs e)
        {
            lstUAIMessage.Items.Clear();

        }

        private void btnOrderCancel_Click(object sender, EventArgs e)
        {
            try
            {
                object bCheck = false;
                int iRowCount = grdNo.Rows.Count;


                string[] aData = new string[4];

                //if (chkAll.Checked)
                //if (grdNo.GetCellCheck(0, grdNo.Cols["취소"].Index) == CheckEnum.Checked)
                //{
                //    for (int iRow = 1; iRow < iRowCount; iRow++)
                //    {
                //        CommonDataBase cDb = new CommonDataBase();
                //        aData[0] = grdNo.GetData(iRow, 1).ToString();
                //        aData[1] = grdNo.GetData(iRow, 2).ToString();
                //        aData[2] = "N";
                //        aData[3] = "D";
                //        cDb.Execute_BackGroundWorker(aData, 1);  // Data Load를 완료하면 Load한 모든 시편을 'D'로 변경한다.
                //        Thread.Sleep(10);
                //        //CommonDataBase.update_TB_XRF_SEQ(dbConn, grdNo.GetData(iRow, 1).ToString(), grdNo.GetData(iRow, 2).ToString(), "N", "D");   // TB_XRF_SEQ Table의 RECHECK 항목을 'N'으로 바꾸고 Grid에서 Data 삭제
                //    }
                //    grdNo.Rows.Count = 1;
                //}
                //else
                //{
                for (int iRow = iRowCount - 1; iRow > 0; iRow--)
                {
                    bCheck = grdNo.GetData(iRow, 4);

                    if (Convert.ToBoolean(bCheck))
                    {
                        CommonDataBase cDb = new CommonDataBase();
                        aData[0] = grdNo.GetData(iRow, 1).ToString();
                        aData[1] = grdNo.GetData(iRow, 2).ToString();
                        aData[2] = "N";
                        aData[3] = "D";
                        cDb.Execute_BackGroundWorker(aData, 1);  // Data Load를 완료하면 Load한 모든 시편을 'D'로 변경한다.
                        Thread.Sleep(10);
                        //CommonDataBase.update_TB_XRF_SEQ(dbConn, grdNo.GetData(iRow, 1).ToString(), grdNo.GetData(iRow, 2).ToString(), "N", "D");   // TB_XRF_SEQ Table의 RECHECK 항목을 'N'으로 바꾸고 Grid에서 Data 삭제
                        GridAutoRowNumber(grdNo, iRow);
                    }
                }
                //}
                grdNo.SetCellCheck(0, grdNo.Cols["취소"].Index, CheckEnum.Unchecked);
                OrderListUpdate();
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "btnOrderCancel_Click");
            }
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int iRow = 1; iRow < grdNo.Rows.Count; iRow++)
            {
                grdNo.SetData(iRow, 4, grdNo.GetCellCheck(0, grdNo.Cols["취소"].Index) == CheckEnum.Checked ? true : false);

            }
        }

        /// <summary>
        /// Grid의 Row 삭제시 Seq No를 정리한다.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="index"></param>
        private void GridAutoRowNumber(C1.Win.C1FlexGrid.C1FlexGrid grid, int index)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                int iRow = 0;
                for (iRow = index; iRow < grid.Rows.Count - 1; iRow++)
                {
                    for (int iCol = 0; iCol < grid.Cols.Count; iCol++)
                    {
                        if (iCol == 0)
                        {
                            grid.SetData(iRow, iCol, grdNo.GetData(iRow, iCol));
                        }
                        else
                        {
                            grid.SetData(iRow, iCol, grdNo.GetData(iRow + 1, iCol));
                        }
                    }
                }
                grid.RemoveItem(iRow);
                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "GridAutoRowNumber");
            }
        }

        private void XRF_Received_Data_Analyze(string[] sAnalyze)
        {
            int iArrayCount = 0;
            string smplNo = string.Empty;
            string tmbDiv = string.Empty;
            string sCommand = string.Empty;
            iArrayCount = sAnalyze.Length;

            try
            {
                if (iArrayCount > 0)
                {
                    sCommand = sAnalyze[0];

                    switch (sCommand)
                    {
                        case "STATUS":
                            // STATUS REQUEST를 보내서 remote 상태인지 확인한다.
                            if (sAnalyze[2].ToString().IndexOf("SYSTEM") != -1)
                            {
                                if (sAnalyze[2].ToString().IndexOf("remote") != -1)
                                {

                                    if (m_firstSend == false)
                                    {
                                        m_firstSend = true;
                                        setText(lbXrfStat, "[XRF Mode Remote]");
                                        lbXrfStat.BackColor = Color.LightGreen;
                                        lbXrfStat.ForeColor = Color.Black;
                                        return;
                                    }
                                    if (XRF_COMMAND.XRF_AUTO == false) return;

                                    XRF_COMMAND.XRF_AUTO = true;

                                    XRF_Start_Wait();

                                    setText(lbXRFCommStat, "Message 수신완료", Color.LightGreen, Color.Black);
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    setText(lbXrfStat, "[XRF Mode Remote]");
                                    setText(lstUAIMessage, "XRF Status Message 수신 -- [Remote]");
                                    lbXrfStat.BackColor = Color.LightGreen;
                                    lbXrfStat.ForeColor = Color.Black;
                                    XRF_LIST_REQ();
                                }
                                else if (sAnalyze[2].ToString().IndexOf("local") != -1)
                                {
                                    if (m_firstSend == false)
                                    {
                                        m_firstSend = true;
                                        setText(lbXrfStat, "[XRF Mode Local]");
                                        lbXrfStat.BackColor = Color.Red;
                                        lbXrfStat.ForeColor = Color.White;
                                        MessageBox.Show("XRF 시스템이 remote 상태가 아닙니다. 확인후 진행하세요", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        return;
                                    }

                                    setText(lbXRFCommStat, "Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    setText(lbXrfStat, "[XRF Mode Local]");
                                    setText(lstUAIMessage, "XRF Status Message 수신 -- [Local]");
                                    lbXrfStat.BackColor = Color.Red;
                                    lbXrfStat.ForeColor = Color.White;
                                    MessageBox.Show("XRF 시스템이 remote 상태가 아닙니다. 확인후 진행하세요", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    setText(btnXRFStart, "XRF 자동 시험 시작");
                                    XRF_COMMAND.XRF_AUTO = false;

                                }
                                else
                                {
                                    if (m_firstSend == false)
                                    {
                                        m_firstSend = true;
                                        setText(lbXrfStat, "[XRF Mode Offline]");
                                        lbXrfStat.BackColor = Color.Red;
                                        lbXrfStat.ForeColor = Color.White;
                                        MessageBox.Show("XRF 시스템이 remote 상태가 아닙니다. 확인후 진행하세요", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        return;
                                    }

                                    setText(lbXRFCommStat, "Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    setText(lbXrfStat, "[XRF Mode Offline]");
                                    setText(lstUAIMessage, "XRF Status Message 수신 -- [Offline]");
                                    lbXrfStat.BackColor = Color.Red;
                                    lbXrfStat.ForeColor = Color.White;
                                    MessageBox.Show("XRF 시스템이 remote 상태가 아닙니다. 확인후 진행하세요", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    setText(btnXRFStart, "XRF 자동 시험 시작");
                                    XRF_COMMAND.XRF_AUTO = false;
                                }
                            }

                            // LIST REQ를 보내서 상태가 nolist인지를 확인한다.
                            else if (sAnalyze[2].ToString().IndexOf("LIST") != -1)
                            {
                                if (XRF_COMMAND.XRF_AUTO == false) return;  // 자동 모드가 이니면 되돌린다.

                                if (sAnalyze[3].ToString().IndexOf("nolist") == -1)
                                {
                                    setText(lbXRFCommStat, "Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    MessageBox.Show("LIST가 OPEN 되어 있습니다. LIST를 닫고 다시 하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    setText(btnXRFStart, "XRF 자동 시험 시작");
                                    XRF_COMMAND.XRF_AUTO = false;
                                }
                                else
                                {
                                    setText(lbXRFCommStat, "Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    XRF_LIST_OPEN();
                                }

                            }

                            break;
                        case "LIST":

                            if (XRF_COMMAND.XRF_AUTO == false) return;

                            if ((sAnalyze[1].ToString().IndexOf("STATUS=normal") != -1) && (sAnalyze[2].ToString().IndexOf("END") != -1)) // LIST OPEN 되었나 확인
                            {
                                if (XRF_COMMAND.XRF_TEMP_FLAG == true)   // // LIST START 성공
                                {
                                    setText(lbXRFCommStat, "Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;
                                    XRF_SAMPLE_ADD();

                                    XRF_COMMAND.XRF_TEMP_FLAG = false;

                                    setText(lstUAIMessage, "LIST START 결과 수신 -- " + sAnalyze[1].ToString());

                                }

                                if (XRF_COMMAND.XFR_FLAG_LIST_OPEN)   // // LIST OPEN 성공
                                {
                                    setText(lbXRFCommStat, "Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    setText(lstUAIMessage, "LIST Open 결과 수신 -- " + sAnalyze[1].ToString());
                                    XRF_COMMAND.XFR_FLAG_LIST_OPEN = false;

                                    XRF_LIST_START();

                                    //XRF_SAMPLE_ADD();
                                    //XRF_COMMAND.XFR_FLAG_SAMPLE_ADD = false;

                                }

                                if (XRF_COMMAND.XFR_FLAG_LIST_STOP)   // // LIST STOP 성공
                                {
                                    setText(lbXRFCommStat, "Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    setText(lstUAIMessage, "LIST STOP 결과 수신 -- " + sAnalyze[1].ToString());
                                    XRF_COMMAND.XFR_FLAG_LIST_STOP = false;

                                    //MessageBox.Show("X1 버퍼를 초기화 후 진행 하시겠습니까?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                                    if (MessageBox.Show("XRF MEASURE작업이 완료되었습니다." + Environment.NewLine + "X1 버퍼를 초기화 하시겠습니까?", "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
                                        == DialogResult.Yes)
                                    {
                                        X1BufferDataClear();
                                    }

                                    setClick(btnXRFStart);      // x1 시험종료버튼 클릭

                                    //작업종료시 버퍼데이터를 클리어 한다.
                                    //X1BufferDataClear();

                                    //// 2016.12.15 Mod. bInitState = true => 초기화로 설정한 경우에만 !!!!
                                    //if (bInitState) X1BufferDataClear();

                                    PutIniValue("STARTUP", "INFO", "OK", sStartUpIni);

                                    toolStripStatusLabel.Text = "SuperQ sample measure LIST를 닫고있습니다. 잠시 기다려주세요. ";

                                    Cursor.Current = Cursors.WaitCursor;
                                    Thread.Sleep(20000);
                                    XRF_LIST_CLOSE();
                                    Cursor.Current = Cursors.Default;

                                    chkXrfAuto.Checked = false;
                                    toolStripStatusLabel.Text = "완료되었습니다. ";

                                    XRF_COMMAND xCmd = new XRF_COMMAND();
                                    string cmd = xCmd.XRF_MAKE_SEND_COMMAND_SYSTEM_STATUS();
                                    XRF_Send_MSG(cmd);

                                    setText(lbXRFCommStat, string.Empty);
                                    MeasureMentHistoryLog("Panalytical 시험종료===================================================", 1);
                                }

                            }
                            break;
                        case "SAMPLE":

                            if (XRF_COMMAND.XRF_AUTO == false) return;

                            if (string.Compare(sAnalyze[1].ToString(), "ADD") == 0) // SAMPLE을 LIST에 등록했다.
                            {
                                if (string.Compare(sAnalyze[3].ToString(), "STATUS=normal") == 0)  // 정상등록 되었음.
                                {

                                    setText(lbXRFCommStat, "SAMPLE ADD Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    setText(lstUAIMessage, "SAMPLE ADD 결과 수신 -- " + sAnalyze[3].ToString());

                                    //last_measure_data.Enqueue(sAnalyze[2].Substring(10));       // New Data
                                    //setText(lbFrontBack, "enqueue: " +  sAnalyze[2].Substring(10));
                                    //setText(label9, "Count: " + last_measure_data.Count.ToString());

                                    if (XrfControl.m_XrfListBufferCount <= 3)
                                    {
                                        if (removeMok == 0)
                                        {
                                            XRF_SAMPLE_ADD();
                                        }
                                    }
                                    if (XrfControl.m_XrfListBufferCount == 4)
                                    {
                                        if (PLCDefine.firstTemp && XRF_COMMAND.XFR_FLAG_LIST_START == false)
                                        {
                                            PLCDefine.firstTemp = false;
                                            int idxLoc = (lineBufLoc % 80) - 1;
                                            if (idxLoc == 0) idxLoc = 79;
                                            if (XrfControl.m_XrfBuffer[idxLoc] != null)
                                            {
                                                // 2013-05-07 CAP CLOSE로 위치옮김(처음 동작시 캡이 한번 닫힌후 XRF가 동작하기 때문에)
                                                //Xrf_Front_in_Enable();
                                            }

                                        }
                                    }
                                }
                                else if (string.Compare(sAnalyze[3].ToString(), "STATUS=fatal") == 0)  // SAMPLE ADD중 오류발생.
                                {
                                    WriteLogData("SAMPLE ADD중 오류발생" + sAnalyze[2].ToString(), "SAMPLE ADD");
                                    MessageBox.Show("SAMPLE ADD중 오류발생" + sAnalyze[2].ToString() + "\r\n" + "시편정보 확인후 재작업요망", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    setClick(btnXRFStart);
                                    return;
                                }
                            }
                            if (string.Compare(sAnalyze[1].ToString(), "REMOVE") == 0) // SAMPLE을 LIST에서 지웠다.
                            {
                                if (string.Compare(sAnalyze[3].ToString(), "STATUS=normal") == 0)  // 정상처리 되었음.
                                {
                                    setText(lbXRFCommStat, "SAMPLE REMOVE Message 수신완료");
                                    lbXRFCommStat.BackColor = Color.LightGreen;

                                    setText(lstUAIMessage, "SAMPLE REMOVE 결과 수신 -- " + sAnalyze[3].ToString());

                                    int mokTmp = removeCnt % 4;
                                    if (mokTmp == 0)
                                    {
                                        listBufLoc = 3;
                                    }
                                    else
                                    {
                                        listBufLoc = mokTmp - 1;
                                    }

                                    if (last_measure_data.Count > 1)
                                    {
                                        XRF_SAMPLE_ADD();
                                    }
                                    if (string.Compare(sAnalyze[2], "SAMPLE_ID=TEMPB") == 0)
                                    {
                                        XRF_LIST_STOP();
                                    }
                                }
                            }

                            if (string.Compare(sAnalyze[1].ToString(), "MEASUREREQ") == 0)
                            {
                                last_measure_data.Enqueue(sAnalyze[2].Substring(10));       // New Data
                                setText(lbFrontBack, "enqueue: " + sAnalyze[2].Substring(10));
                                setText(label9, "Count: " + last_measure_data.Count.ToString());

                                //string smplNo = sAnalyze[2];
                                //XRF_SAMPLE_MEASURE("@"+smplNo);
                                //last_measure_data.Enqueue(sAnalyze[2].Substring(10));       // New Data
                            }

                            break;
                        case "RESULT":

                            if (XRF_COMMAND.XRF_AUTO == false) return;  // 자동 모드가 이니면 되돌린다.

                            if (string.Compare(sAnalyze[2].ToString(), "STATUS=result_ok") == 0)  // 검사결과 OK
                            {
                                string sData = string.Empty;

                                // DB Table에 Data 저장
                                XRF_COMMAND.XRF_LIST_REMOVE_SAMPLE_ID = sAnalyze[1].ToString().Substring(10);  // XRF LIST에서 지울 시편번호
                                removeCnt++;
                                removeMok++;
                                XRF_SAMPLE_REMOVE();

                                if (XRF_COMMAND.XRF_LIST_REMOVE_SAMPLE_ID.Length < 9) break;

                                string s_FrontBack = sAnalyze[1].ToString().Substring(20, 1);  //전면 이면 구분
                                //XRF_COMMAND.XFR_FLAG_MEASURE_END = true;

                                if (string.Compare(s_FrontBack, "F") == 0)
                                {
                                    //Xrf_Front_Measure_Complete();
                                    //setText(lbFrontBack, "전면 결과 받음");
                                }
                                else if (string.Compare(s_FrontBack, "B") == 0)
                                {
                                    //setText(lbFrontBack, "이면 결과 받음");
                                    //Xrf_Back_Measure_Complete();

                                }

                                Parsing_Result_Data(sAnalyze, s_FrontBack);

                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogData(ex.Message, "XRF_Received_Data_Analyze");
            }

        }

        Queue<string> last_measure_data = new Queue<string>();
        Queue<string> last_measure_data_X2 = new Queue<string>(); // for BRUKER

        private void Parsing_Result_Data(string[] sData, string s_FB)
        {
            try
            {
                int idx = 0;
                string sSmplNo = string.Empty;
                string sTmb = string.Empty;
                string sWcd = string.Empty;
                string strAppName = string.Empty;
                //s_FB 전면이면 구분
                Decimal dZn = 0.0M;
                Decimal dFe = 0.0M;
                Decimal dCr = 0M;
                Decimal dP = 0.0M;

                sSmplNo = sData[1].Substring(10, 8).Trim();
                sTmb = sData[1].Substring(18, 1).Trim();
                sWcd = sData[1].Substring(19, 1).Trim();

                for (idx = 0; idx < sData.Length; idx++)
                {
                    if (string.Compare(sData[idx], "COMP=Fe") == 0)
                    {
                        dFe = decimal.Parse(sData[idx + 1].Substring(5), System.Globalization.NumberStyles.Float);
                        if (dFe < 0.0M) dFe = 0M;
                    }

                    if (string.Compare(sData[idx], "COMP=Zn") == 0)
                    {
                        dZn = decimal.Parse(sData[idx + 1].Substring(5), System.Globalization.NumberStyles.Float);
                        if (dZn < 0.0M) dZn = 0M;
                    }

                    if (string.Compare(sData[idx], "COMP=Cr") == 0)
                    {
                        dCr = decimal.Parse(sData[idx + 1].Substring(5), System.Globalization.NumberStyles.Float);
                        if (dCr < 0.0M) dCr = 0M;
                    }

                    if (string.Compare(sData[idx], "COMP=P") == 0)
                    {
                        dP = decimal.Parse(sData[idx + 1].Substring(5), System.Globalization.NumberStyles.Float);
                        if (dP < 0.0M) dP = 0M;
                    }
                    //strAppName
                    //@APPLICATION=CGL_GG_ZFP
                    if (sData[idx].IndexOf("APPLICATION=") > -1)
                    {
                        strAppName = sData[idx].Split('=')[1];
                    }
                }

                //ApplicationName의 원소테이블
                string[] strParams = new string[] { sSmplNo
                                                   ,sTmb
                                                   ,strAppName
                                                    };
                DataTable dtElement = CommonDataBase.GetXRFApplicationElement(strParams);
                if (dtElement.Columns.Count == 0)
                {
                    dtElement.Columns.Add("LINENAME", typeof(string));
                    dtElement.Columns.Add("COILNAME", typeof(string));
                    dtElement.Columns.Add("SHTSPEC", typeof(string));
                    dtElement.Columns.Add("AFTCODE", typeof(string));
                    dtElement.Columns.Add("ELEMENT", typeof(string));
                    dtElement.Columns.Add("COMPUTEMODIFY", typeof(string));
                    dtElement.Columns.Add("COLNAME", typeof(string));
                    dtElement.Columns.Add("APPNAME", typeof(string));
                }
                dtElement.Columns.Add("ELEMENTVALUE", typeof(string));
                dtElement.Columns.Add("DATE", typeof(string));
                dtElement.Columns.Add("BEFOREELEMENTVALUE", typeof(string));
                dtElement.Columns.Add("WCD", typeof(string));
                dtElement.Columns.Add("SMPLNO", typeof(string));
                dtElement.Columns.Add("TMBDIV", typeof(string));
                dtElement.Columns.Add("FRONTBACK", typeof(string));

                //원소 테이블 입력받아온다
                for (idx = 0; idx < sData.Length; idx++)
                {//DB에서 가져온 원소에 값을 담는다
                    if (sData[idx].IndexOf("COMP=") > -1)
                    {

                        bool bolElement = false;
                        for (int iElementRow = 0; iElementRow < dtElement.Rows.Count; iElementRow++)
                        {
                            if (string.Compare(sData[idx].ToUpper(), "COMP=" + dtElement.Rows[iElementRow]["ELEMENT"].ToString().ToUpper()) == 0)
                            {//ApplicationName의 원소와 기계의 원소값이 동일하다면
                                bolElement = true;
                                decimal iEle = decimal.Parse(sData[idx + 1].Substring(5), System.Globalization.NumberStyles.Float);
                                dtElement.Rows[iElementRow]["ELEMENTVALUE"] = iEle; //원래값
                                dtElement.Rows[iElementRow]["BEFOREELEMENTVALUE"] = iEle; //원래값
                                dtElement.Rows[iElementRow]["DATE"] = sData[3].Substring(11);//날짜
                                dtElement.Rows[iElementRow]["WCD"] = sWcd; //WCD
                                dtElement.Rows[iElementRow]["SMPLNO"] = sSmplNo;//시편번호
                                dtElement.Rows[iElementRow]["TMBDIV"] = sTmb; //TBMDIV
                                dtElement.Rows[iElementRow]["FRONTBACK"] = s_FB.Equals("F") ? "FRONT" : "BACK"; //전면이면

                            }
                        }

                        if (!bolElement)
                        {//기계에서 입력받은 원소 != DB에서 받아온 원소
                            decimal dEle = decimal.Parse(sData[idx + 1].Substring(5), System.Globalization.NumberStyles.Float);
                            DataRow drTemp = dtElement.Rows.Add();
                            drTemp["ELEMENT"] = sData[idx].Split('=')[1];
                            drTemp["APPNAME"] = strAppName;
                            drTemp["ELEMENTVALUE"] = dEle;
                            drTemp["DATE"] = sData[3].Substring(11);//날짜
                            drTemp["WCD"] = sWcd;
                            drTemp["SMPLNO"] = sSmplNo;
                            drTemp["TMBDIV"] = sTmb;
                            drTemp["FRONTBACK"] = s_FB.Equals("F") ? "FRONT" : "BACK"; //전면이면
                        }

                    }
                }

                for (int iElementRow = 0; iElementRow < dtElement.Rows.Count; iElementRow++)
                {//DB에서 가져온 테이블 FOR문돌려서 변경된값 담는다
                    if (!string.IsNullOrEmpty(dtElement.Rows[iElementRow]["LINENAME"].ToString()))
                    {
                        strParams = new string[] { dtElement.Rows[iElementRow]["LINENAME"].ToString()
                                                          ,dtElement.Rows[iElementRow]["COILNAME"].ToString()
                                                          ,dtElement.Rows[iElementRow]["SHTSPEC"].ToString()
                                                          ,dtElement.Rows[iElementRow]["AFTCODE"].ToString()
                                                          ,dtElement.Rows[iElementRow]["ELEMENT"].ToString().ToUpper()
                                                          ,dtElement.Rows[iElementRow]["APPNAME"].ToString()
                                                            };
                        DataTable dtDetail = CommonDataBase.GetXRFApplicationDetail(strParams);
                        InoCompute ino = new InoCompute(dtElement, dtDetail);
                        dtElement.Rows[iElementRow]["ELEMENTVALUE"] = ino.Compute(dtElement.Rows[iElementRow]["ELEMENT"].ToString().ToUpper());//변경된 원소값 저장
                    }
                    else
                    {
                        dtElement.Rows[iElementRow]["BEFOREELEMENTVALUE"] = dtElement.Rows[iElementRow]["ELEMENTVALUE"];
                        dtElement.Rows[iElementRow]["ELEMENTVALUE"] = string.Empty;
                    }
                }

                //for (idx = 0; idx < sData.Length; idx++)
                //{//DB에 없는 원소 저장
                //    if (sData[idx].IndexOf("COMP=") > -1)
                //    {

                //        bool bolElement = false;
                //        for (int iElementRow = 0; iElementRow < dtElement.Rows.Count; iElementRow++)
                //        {
                //            string strMElement = sData[idx].Split('=')[1];
                //            if (strMElement.Equals(dtElement.Rows[iElementRow]["ELEMENT"].ToString()))
                //            {//기계에서 입력받은 원소 == DB에서 받아온 원소
                //                bolElement = true;

                //            }
                //        }

                //        if (!bolElement)
                //        {//기계에서 입력받은 원소 != DB에서 받아온 원소
                //            decimal dEle = decimal.Parse(sData[idx + 1].Substring(5), System.Globalization.NumberStyles.Float);
                //            //if (dEle < 0.0M) dEle = 0M; //2017.04.29 김정건 조건절은 다 빼야된다
                //            DataRow drTemp = dtElement.Rows.Add();
                //            drTemp["ELEMENT"] = sData[idx].Split('=')[1];
                //            drTemp["APPNAME"] = strAppName;
                //            drTemp["BEFOREELEMENTVALUE"] = dEle;
                //            drTemp["DATE"] = sData[3].Substring(11);//날짜
                //            drTemp["WCD"] = sWcd;
                //            drTemp["SMPLNO"] = sSmplNo;
                //            drTemp["TMBDIV"] = sTmb;
                //            drTemp["FRONTBACK"] = s_FB.Equals("F") ? "FRONT" : "BACK"; //전면이면
                //        }
                //    }
                //}

                string[] iData = new string[9];
                iData[0] = sSmplNo;
                iData[1] = sTmb;
                iData[2] = sWcd;
                iData[3] = s_FB;//전면이면 구분
                iData[4] = Math.Round(dFe, 1, MidpointRounding.AwayFromZero).ToString();
                iData[5] = dZn.ToString();
                iData[6] = dCr.ToString();
                iData[7] = Math.Round(dP, 1, MidpointRounding.AwayFromZero).ToString();
                iData[8] = sData[3].Substring(11);//날짜

                CommonDataBase cDb = new CommonDataBase();
                cDb.Execute_SaveData(dtElement, "History");
                //string[] strResultHistory = new string[iData.Length];
                //iData.CopyTo(strResultHistory, 0);
                //cDb.Execute_BackGroundWorker(strResultHistory, 5);

                //Thread.Sleep(100);
                //cDb.Execute_SaveData(dtElement, "Result");
                //string[] strTestResult = new string[iData.Length];
                //iData.CopyTo(strTestResult, 0);
                //CommonDataBase cDb2 = new CommonDataBase();
                //cDb2.Execute_BackGroundWorker(strTestResult, 6);

                Thread.Sleep(100);

                //if (s_FB == "B")
                //{
                //    CommonDataBase cDb3 = new CommonDataBase();
                //    cDb3.Execute_BackGroundWorker(iData, 7);
                //    Thread.Sleep(100);
                //}

                tb_Result_Display(iData);//InitFormatGrid에서 그리드 


                iData[0] = sSmplNo;
                iData[1] = sTmb;
                iData[2] = "Y";
                iData[3] = "Y";

                string[] strXrfSeq = new string[iData.Length];
                iData.CopyTo(strXrfSeq, 0);
                CommonDataBase cDb4 = new CommonDataBase();
                cDb4.Execute_BackGroundWorker(iData, 1);


            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "Parsing_Result_Data " + sData[1]);
            }
        }

        private void tb_Result_Display(string[] s)
        {
            try
            {
                string smplNo = s[0];
                string tmb = s[1];
                string wcd = s[2];
                string fb = s[3];
                string Fe = s[4];
                string Zn = s[5];
                string Cr = s[6];
                string P = s[7];
                string sDate = s[8];

                Decimal dFe = 0.000M;
                Decimal dZn = 0.000M;
                Decimal dCr = 0.000M;
                Decimal dP = 0.000M;

                dFe = decimal.Parse(Fe, System.Globalization.NumberStyles.Float);
                if (dFe < 0.0M) dFe = 0M;
                dZn = decimal.Parse(Zn, System.Globalization.NumberStyles.Float);
                if (dZn < 0.0M) dZn = 0M;
                dCr = decimal.Parse(Cr, System.Globalization.NumberStyles.Float);
                if (dCr < 0.0M) dCr = 0M;
                dP = decimal.Parse(P, System.Globalization.NumberStyles.Float);
                if (dP < 0.0M) dP = 0M;

                dFe = Math.Round(dFe, 1, MidpointRounding.AwayFromZero);
                dZn = Math.Round(dZn, 1, MidpointRounding.AwayFromZero);
                dFe = Math.Round(dFe, 1, MidpointRounding.AwayFromZero);
                dP = Math.Round(dP, 1, MidpointRounding.AwayFromZero);

                string smplKey = smplNo.PadRight(8) + tmb;
                int iRow = 0;
                bool bFound = false;

                if (cfgrdXrfList.Rows.Count > 1)
                {
                    foreach (C1.Win.C1FlexGrid.Row row in cfgrdXrfList.Rows)
                    {
                        if (string.Compare(row[29].ToString(), smplKey) == 0)
                        {
                            iRow = row.Index;  // cfgrdXrfList.RowSel;
                            bFound = true;
                            break;
                        }
                        else
                        {
                            bFound = false;
                        }
                    }
                    if (!bFound)
                    {
                        iRow = cfgrdXrfList.Rows.Count;
                        insGrid(cfgrdXrfList, -1);
                    }

                }
                else
                {
                    iRow = cfgrdXrfList.Rows.Count;
                    insGrid(cfgrdXrfList, -1);
                }


                WriteLogData(s[0] + s[1] + s[2] + " " + s[3] + " " + s[4] + " " + s[5] + " " + s[6] + " " + s[7] + " " + s[8], "tb_Result_Display");

                cfgrdXrfList.SetData(iRow, 0, iRow.ToString());

                string[] uData = new string[2];
                uData[0] = smplNo;
                uData[1] = tmb;

                CommonDataBase cDbLen = new CommonDataBase();
                cDbLen.Execute_BackGroundWorker(uData, 7);
                Thread.Sleep(100);

                if (wcd == "W" && fb == "F")
                {
                    cfgrdXrfList.SetData(iRow, 1, smplNo);
                    cfgrdXrfList.SetData(iRow, 2, tmb);
                    cfgrdXrfList.SetData(iRow, 3, CommonDataBase.Sample_Length);
                    cfgrdXrfList.SetData(iRow, 29, smplNo.PadRight(8) + tmb);
                    cfgrdXrfList.SetData(iRow, 4, sDate);
                    cfgrdXrfList.SetData(iRow, 5, dZn.ToString());
                    cfgrdXrfList.SetData(iRow, 11, dCr.ToString());
                    cfgrdXrfList.SetData(iRow, 17, dP.ToString());
                    cfgrdXrfList.SetData(iRow, 23, dFe.ToString());
                }
                if (wcd == "W" && fb == "B")
                {
                    cfgrdXrfList.SetData(iRow, 1, smplNo);
                    cfgrdXrfList.SetData(iRow, 2, tmb);
                    //cfgrdXrfList.SetData(iRow, 3, CommonDataBase.Sample_Length);
                    cfgrdXrfList.SetData(iRow, 29, smplNo.PadRight(8) + tmb);
                    cfgrdXrfList.SetData(iRow, 8, dZn.ToString());
                    cfgrdXrfList.SetData(iRow, 14, dCr.ToString());
                    cfgrdXrfList.SetData(iRow, 20, dP.ToString());
                    cfgrdXrfList.SetData(iRow, 26, dFe.ToString());
                }

                if (wcd == "C" && fb == "F")
                {
                    cfgrdXrfList.SetData(iRow, 1, smplNo);
                    cfgrdXrfList.SetData(iRow, 2, tmb);
                    //cfgrdXrfList.SetData(iRow, 3, CommonDataBase.Sample_Length);
                    cfgrdXrfList.SetData(iRow, 29, smplNo.PadRight(8) + tmb);
                    cfgrdXrfList.SetData(iRow, 6, dZn.ToString());
                    cfgrdXrfList.SetData(iRow, 12, dCr.ToString());
                    cfgrdXrfList.SetData(iRow, 18, dP.ToString());
                    cfgrdXrfList.SetData(iRow, 24, dFe.ToString());
                }
                if (wcd == "C" && fb == "B")
                {

                    cfgrdXrfList.SetData(iRow, 1, smplNo);
                    cfgrdXrfList.SetData(iRow, 2, tmb);
                    //cfgrdXrfList.SetData(iRow, 3, CommonDataBase.Sample_Length);
                    cfgrdXrfList.SetData(iRow, 29, smplNo.PadRight(8) + tmb);
                    cfgrdXrfList.SetData(iRow, 9, dZn.ToString());
                    cfgrdXrfList.SetData(iRow, 15, dCr.ToString());
                    cfgrdXrfList.SetData(iRow, 21, dP.ToString());
                    cfgrdXrfList.SetData(iRow, 27, dFe.ToString());
                }

                if (wcd == "D" && fb == "F")
                {

                    cfgrdXrfList.SetData(iRow, 1, smplNo);
                    cfgrdXrfList.SetData(iRow, 2, tmb);
                    //cfgrdXrfList.SetData(iRow, 3, CommonDataBase.Sample_Length);
                    cfgrdXrfList.SetData(iRow, 29, smplNo.PadRight(8) + tmb);
                    cfgrdXrfList.SetData(iRow, 7, dZn.ToString());
                    cfgrdXrfList.SetData(iRow, 13, dCr.ToString());
                    cfgrdXrfList.SetData(iRow, 19, dP.ToString());
                    cfgrdXrfList.SetData(iRow, 25, dFe.ToString());
                }
                if (wcd == "D" && fb == "B")
                {

                    cfgrdXrfList.SetData(iRow, 1, smplNo);
                    cfgrdXrfList.SetData(iRow, 2, tmb);
                    //cfgrdXrfList.SetData(iRow, 3, CommonDataBase.Sample_Length);
                    cfgrdXrfList.SetData(iRow, 29, smplNo.PadRight(8) + tmb);
                    cfgrdXrfList.SetData(iRow, 10, dZn.ToString());
                    cfgrdXrfList.SetData(iRow, 16, dCr.ToString());
                    cfgrdXrfList.SetData(iRow, 22, dP.ToString());
                    cfgrdXrfList.SetData(iRow, 28, dFe.ToString());
                }
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "tb_Result_Display");
            }

        }

        private void XRF_Start_Wait()
        {

            setText(btnXRFStart, "X1 자동 시험 종료");
            setText(lstUAIMessage, "X1 Status 조회 Message 송신");
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;
        }

        private void XRF_Start_Wait_X2()
        {
            setText(btnXRF2Start, "X2 자동 시험 종료");
            setText(lstUAIMessage2, "X2 Status 조회 Message 송신");
            lbXRF2CommStat.BackColor = Color.Yellow;
        }

        private void XRF_SAMPLE_REMOVE()
        {

            XRF_COMMAND xcmd = new XRF_COMMAND();

            string sCommand = xcmd.XRF_MAKE_SEND_COMMAND_REMOVE(XRF_COMMAND.XRF_LIST_REMOVE_SAMPLE_ID);
            XRF_Send_MSG(sCommand);
            setText(lstUAIMessage, "SAMPLE REMOVE Message 송신 SampleID=" + XRF_COMMAND.XRF_LIST_REMOVE_SAMPLE_ID + ", Application= " + XrfControl.m_Application);
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;

        }

        private void XRF_LIST_REQ()
        {
            string sCommand = string.Empty;

            XRF_COMMAND xcmd = new XRF_COMMAND();
            sCommand = xcmd.XRF_MAKE_SEND_COMMAND_LIST_REQ();

            XRF_Send_MSG(sCommand);
            setText(lstUAIMessage, "List 상태 요구 Message 송신");
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;
        }

        private void XRF_LIST_OPEN()
        {
            string sCommand = string.Empty;

            XRF_COMMAND xcmd = new XRF_COMMAND();
            sCommand = xcmd.XRF_MAKE_SEND_COMMAND_LIST_OPEN(XrfControl.XRF_LIST_NAME);
            XRF_COMMAND.XFR_FLAG_LIST_OPEN = true;

            XRF_Send_MSG(sCommand);
            setText(lstUAIMessage, "List OPEN Message 송신");
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;
        }

        private void XRF_ABORT()
        {
            string sCommand = string.Empty;

            XRF_COMMAND xcmd = new XRF_COMMAND();
            sCommand = "@ABORT@UNIT=xrf@END";

            XRF_Send_MSG(sCommand);
            setText(lstUAIMessage, "ABORT Message 송신");
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;
        }

        private void XRF_LIST_START()
        {
            string sCommand = string.Empty;

            XRF_COMMAND xcmd = new XRF_COMMAND();
            sCommand = xcmd.XRF_MAKE_SEND_COMMAND_LIST_START();
            XRF_COMMAND.XFR_FLAG_LIST_START = true;
            XRF_COMMAND.XRF_TEMP_FLAG = true;

            XRF_Send_MSG(sCommand);
            setText(lstUAIMessage, "List START Message 송신");
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;
        }

        private void XRF_LIST_STOP()
        {
            string sCommand = string.Empty;

            XRF_COMMAND xcmd = new XRF_COMMAND();
            sCommand = "@LIST@STOP@END";
            XRF_COMMAND.XFR_FLAG_LIST_STOP = true;

            XRF_Send_MSG(sCommand);
            setText(lstUAIMessage, "List STOP Message 송신");
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;
        }

        private void XRF_LIST_CLOSE()
        {
            string sCommand = string.Empty;

            XRF_COMMAND xcmd = new XRF_COMMAND();
            sCommand = "@LIST@CLOSE@END";

            XRF_Send_MSG(sCommand);
            setText(lstUAIMessage, "List CLOSE Message 송신");
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;
        }

        private void XRF_SAMPLE_MEASURE(string smplNo)
        {
            string sCommand = string.Empty;
            XRF_COMMAND xcmd = new XRF_COMMAND();
            sCommand = string.Format("@SAMPLE@MEASURE{0}@END", smplNo);

            XRF_Send_MSG(sCommand);
            setText(lstUAIMessage, "Sample Measure Message 송신");
            setText(lbXRFCommStat, "송신후 응답대기");
            lbXRFCommStat.BackColor = Color.Yellow;
        }



        //private string getXRF_ApplicationName(string smplNo, string tmbDiv)
        private string getXRF_ApplicationName(string smplNo, string tmbDiv)
        {
            string sReturn = string.Empty;

            sReturn = CommonDataBase.GetProgramName(dbConn, smplNo, tmbDiv, "TB_XRF_SEQ");

            if (sReturn.Length == 0)
            {
                sReturn = CommonDataBase.GetProgramName(dbConn, smplNo, tmbDiv, "TB_XRF_SEQ_TEMP");
            }

            //if (sReturn.Length == 0)
            //{
            //    for (int k = 0; k < sSPL.Length; k++)
            //    {
            //        if (sSPL[k].Contains(smplNo))
            //        {
            //            sReturn = sbBufGridAppl[k];
            //            break;
            //        }
            //    }
            //}
            ////

            sReturn = sbBufGridAppl[Array.FindIndex(sSPL, element => element.Contains(smplNo))];

            return sReturn;
        }


        /// <summary>
        /// XRF LIST 버퍼에 저장할 위치
        /// </summary>
        private int listBufLoc = 0;
        /// <summary>
        /// LINE의 물리적 버퍼의 현재까지 처리(XRF LIST 버퍼에 저장완료한)위치
        /// </summary>
        private int lineBufLoc = 0;
        /// <summary>
        /// XRF LIST 버퍼에 다음 저장할 위치
        /// </summary>
        private string listPosition = string.Empty;
        /// <summary>
        /// XRF LIST버퍼의 내용
        /// </summary>
        private string[] xrf_list_buf = new string[4];

        private bool sampleAddFlag = true;  // SAMPLE ADD가 처음인지 나타낸다.  true:처음, false:처음아님
        bool tmpFlag = true;

        private string[] tmpSmplNo = new string[2];

        private bool XRF_SAMPLE_ADD()
        {
            string sampData = string.Empty;

            if (sampleAddFlag)  // sample add가 처음이면 시편버퍼에 첫시편이 어느위치에 있는지 찾는다.
            {
                for (int idx = 0; idx < 88; idx++)
                //for (int idx = 0; idx < nBufferNumber; idx++)
                {
                    if (XrfControl.m_XrfBuffer[idx] != null)   //시편 버퍼 데이타가 들어있는 Array
                    {
                        sampData = XrfControl.m_XrfBuffer[idx];
                        lineBufLoc = idx;  // 시편버퍼의 첫 시편위치 저장
                        sampleAddFlag = false;
                        break;
                    }
                }
                if (sampData.Length == 0)
                {
                    MessageBox.Show("시편버퍼에 시편이 없습니다,", "알림");
                    return false;
                }
            }

            try
            {
                XRF_COMMAND xcmd = new XRF_COMMAND();
                string sCommand = string.Empty;
                switch (listBufLoc)
                {
                    case 0: // 처음위치 첫시편의 전면
                        if (tmpFlag)
                        {
                            tmpSmplNo[0] = XrfControl.m_XrfBuffer[lineBufLoc];
                        }
                        else  // 처음 시작이 아니면
                        {
                            tmpSmplNo[0] = XrfControl.m_XrfBuffer[lineBufLoc];
                            if (tmpSmplNo[0] == null) break;
                        }

                        if (tmpSmplNo[0].Length >= 10)
                        {
                            XrfControl.m_Application = getXRF_ApplicationName(tmpSmplNo[0].Substring(0, 8).ToString().Trim(), tmpSmplNo[0].Substring(8, 1).ToString());

                            if (XrfControl.m_Application.Length == 0)
                            {
                                MessageBox.Show("XRF프로그램명이 등록되지 않아서 더이상 진행할수 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }
                            else
                            {
                                xrf_list_buf[listBufLoc] = XrfControl.m_XrfBuffer[lineBufLoc] + "F";  // 첫시편의 전면

                                sCommand = xcmd.XRF_MAKE_SEND_COMMAND_SAMPLE_ADD(xrf_list_buf[listBufLoc], XrfControl.m_Application, "1,1");
                                //ListQueue.Enqueue(xrf_list_buf[listBufLoc]);
                                XRF_Send_MSG(sCommand);
                                setText(lstUAIMessage, "SAMPLE ADD Message 송신 SampleID=" + xrf_list_buf[listBufLoc] + ", Application= " + XrfControl.m_Application + ", position=1,1");
                                setText(lbXRFCommStat, "송신후 응답대기");
                                lbXRFCommStat.BackColor = Color.Yellow;

                                listBufLoc++;
                                lineBufLoc++;
                                XrfControl.m_XrfListBufferCount = 1;
                            }
                        }
                        break;
                    case 1:   // 2번째 위치 2번째 시편의 전면
                        if (tmpFlag)
                        {
                            tmpSmplNo[1] = XrfControl.m_XrfBuffer[lineBufLoc];
                            tmpFlag = false;
                        }
                        else  // 처음 시작이 아니면
                        {
                            if (tmpSmplNo[1].Substring(0, 4) == "TEMP") break;

                            tmpSmplNo[1] = XrfControl.m_XrfBuffer[lineBufLoc] == null ? string.Empty : XrfControl.m_XrfBuffer[lineBufLoc];

                            if (tmpSmplNo[1].Length < 9)
                            {
                                if (tmpSmplNo[1].Length == 0)
                                    tmpSmplNo[1] = "TEMP";
                                else
                                    break;
                            }
                        }

                        if (tmpSmplNo[1].Length >= 10)
                        {
                            XrfControl.m_Application = getXRF_ApplicationName(tmpSmplNo[1].Substring(0, 8).ToString().Trim(), tmpSmplNo[1].Substring(8, 1).ToString());
                            if (XrfControl.m_Application.Length == 0)
                            {
                                MessageBox.Show("XRF프로그램명이 등록되지 않아서 더이상 진행할수 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }
                        }

                        xrf_list_buf[listBufLoc] = tmpSmplNo[1] + "F";  //  XrfControl.m_XrfBuffer[lineBufLoc] + "F";  // 2번시편의 전면

                        sCommand = xcmd.XRF_MAKE_SEND_COMMAND_SAMPLE_ADD(xrf_list_buf[listBufLoc], XrfControl.m_Application, "1,2");
                        //ListQueue.Enqueue(xrf_list_buf[listBufLoc]);
                        XRF_Send_MSG(sCommand);
                        setText(lstUAIMessage, "SAMPLE ADD Message 송신 SampleID=" + xrf_list_buf[listBufLoc] + ", Application= " + XrfControl.m_Application + ", position=1,2");
                        setText(lbXRFCommStat, "송신후 응답대기");
                        lbXRFCommStat.BackColor = Color.Yellow;

                        listBufLoc++;
                        lineBufLoc--;  //이전위치로 이동(후면을 넣어야 하니까)
                        XrfControl.m_XrfListBufferCount = 2;
                        break;
                    case 2:   // 3번째 위치 1번째 시편의 전면
                        xrf_list_buf[listBufLoc] = tmpSmplNo[0] + "B";

                        if (xrf_list_buf[listBufLoc].Length >= 10)
                        {
                            XrfControl.m_Application = getXRF_ApplicationName(xrf_list_buf[listBufLoc].Substring(0, 8).ToString().Trim(), xrf_list_buf[listBufLoc].Substring(8, 1).ToString());
                            if (XrfControl.m_Application.Length == 0)
                            {
                                MessageBox.Show("XRF프로그램명이 등록되지 않아서 더이상 진행할수 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }
                            else
                            {


                                sCommand = xcmd.XRF_MAKE_SEND_COMMAND_SAMPLE_ADD(xrf_list_buf[listBufLoc], XrfControl.m_Application, "1,3");
                                //ListQueue.Enqueue(xrf_list_buf[listBufLoc]);
                                XRF_Send_MSG(sCommand);
                                setText(lstUAIMessage, "SAMPLE ADD Message 송신 SampleID=" + xrf_list_buf[listBufLoc] + ", Application= " + XrfControl.m_Application + ", position=1,3");
                                setText(lbXRFCommStat, "송신후 응답대기");
                                lbXRFCommStat.BackColor = Color.Yellow;

                                listBufLoc++;
                                lineBufLoc++;
                                XrfControl.m_XrfListBufferCount = 3;
                            }
                        }
                        break;
                    case 3:   // 4번째 위치 2번째 시편의 후면


                        if (tmpSmplNo[1].Length < 9)
                        {
                            if (tmpSmplNo[1] != "TEMP")
                                break;
                            else
                            {
                                xrf_list_buf[listBufLoc] = tmpSmplNo[1] + "B";
                            }
                        }


                        if (tmpSmplNo[1].Length >= 10)
                        {
                            xrf_list_buf[listBufLoc] = tmpSmplNo[1] + "B";
                            if (tmpSmplNo[1].Substring(0, 4) == "TEMP") { tmpSmplNo[1] = string.Empty; }

                            XrfControl.m_Application = getXRF_ApplicationName(xrf_list_buf[listBufLoc].Substring(0, 8).ToString().Trim(), xrf_list_buf[listBufLoc].Substring(8, 1).ToString());
                            if (XrfControl.m_Application.Length == 0)
                            {
                                MessageBox.Show("XRF프로그램명이 등록되지 않아서 더이상 진행할수 없습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return false;
                            }
                        }

                        sCommand = xcmd.XRF_MAKE_SEND_COMMAND_SAMPLE_ADD(xrf_list_buf[listBufLoc], XrfControl.m_Application, "1,4");
                        //ListQueue.Enqueue(xrf_list_buf[listBufLoc]);
                        XRF_Send_MSG(sCommand);
                        setText(lstUAIMessage, "SAMPLE ADD Message 송신 SampleID=" + xrf_list_buf[listBufLoc] + ", Application= " + XrfControl.m_Application + ", position=1,4");
                        setText(lbXRFCommStat, "송신후 응답대기");
                        lbXRFCommStat.BackColor = Color.Yellow;

                        listBufLoc = 0;
                        lineBufLoc++;
                        XrfControl.m_XrfListBufferCount = 4;
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLogData(ex.Message, "XRF_SAMPLE_ADD");
                return false;
            }
        }

        private void toolStrip1_Resize(object sender, EventArgs e)
        {
            btnX1_Connect.Left = toolStrip1.Width - (btnX1_Connect.Width + 20);
            lbXrfStat.Left = btnX1_Connect.Left - (lbXrfStat.Width + 20);
            //lbXrfStat.Left = toolStrip1.Width - (lbXrfStat.Width + btnX1_Connect.Width + 40);
            lbXRFCommStat.Left = lbXrfStat.Left - (lbXRFCommStat.Width + 20);

            btnX2_Connect.Left = lbXRFCommStat.Left - (btnX2_Connect.Width + 60);
            lbXrf2Stat.Left = btnX2_Connect.Left - (lbXrf2Stat.Width + 20);
            //lbXrfStat.Left = toolStrip1.Width - (lbXrfStat.Width + btnX1_Connect.Width + 40);
            lbXRF2CommStat.Left = lbXrf2Stat.Left - (lbXRF2CommStat.Width + 20);
        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox About = new AboutBox();
            About.ShowDialog();
            About.Dispose();
        }

        private int m_InputCnt = 0;
        private int m_mok = 0;
        private int m_na = 0;

        private void InputCountDiv()
        {
            try
            {
                m_InputCnt++;

                m_mok = m_InputCnt / 4;
                m_na = m_InputCnt % 4; // 1,2,3,0(1,2,3,4)   1,2,3,0(5,6,7,8)
                if (m_na.Equals(0))
                    m_na = 4;
                setText(lbMok, "mok: " + m_mok.ToString() + " Na: " + m_na.ToString() + ", cnt : " + m_InputCnt.ToString(), Color.Transparent, Color.Black);
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "InputCountDiv");
            }
        }


        private void BufferCountWrite()
        {
            try
            {
                int X1bufCount = -1;
                int X2bufCount = -1;

                // PANALYTICAL 장비의 버퍼 처리
                for (int i = 0; i < XrfControl.m_X1XrfBufferSize; i++)
                {
                    if (XrfControl.m_XrfBuffer[i] != null)   //시편 버퍼 데이타가 들어있는 Array
                    {
                        X1bufCount = i;
                    }
                }

                // BRUKER 장비의 버퍼처리
                for (int i = XrfControl.m_X1XrfBufferSize; i < XrfControl.m_XrfBuffer.Length; i++)
                {
                    if (XrfControl.m_XrfBuffer[i] != null)   //시편 버퍼 데이타가 들어있는 Array
                    {
                        X2bufCount = i;
                    }
                }

                object[] oWriteData = null;
                oWriteData = new object[2];

                int[] iTagHandle = new int[2];
                iTagHandle[0] = 11;
                iTagHandle[1] = 18;

                if (X1bufCount == -1)
                {
                    oWriteData[0] = "0";
                }
                else
                {
                    oWriteData[0] = (X1bufCount + 1).ToString();
                }
                if (X2bufCount == -1)
                {
                    oWriteData[1] = "0";
                }
                else
                {
                    oWriteData[1] = (X2bufCount - XrfControl.m_X1XrfBufferSize + 1).ToString();
                }

                int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_CONV_LOC", ref iTagHandle, ref oWriteData);
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "BufferCountWrite");
            }
        }


        private void btnManualAdd_Click(object sender, EventArgs e)
        {
            pnlManual.Visible = true;
            txtSmplNo.Focus();
        }

        //private string[] tmpData = new string[5];
        private string[] tmpData = new string[7];
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtSmplNo.Text.Length == 0)
                {
                    MessageBox.Show("시편번호를 입력하세요", "알림");
                    txtSmplNo.Focus();
                    return;
                }
                if (cmbTmb.Text.Length == 0)
                {
                    MessageBox.Show("TMB구분을 입력하세요", "알림");
                    cmbTmb.Focus();
                    return;
                }
                if (txtLength.Text.Length == 0)
                {
                    MessageBox.Show("길이를 입력하세요", "알림");
                    txtLength.Focus();
                    return;
                }
                if (cmbAppNameManual.Text.Length == 0)
                {
                    MessageBox.Show("프로그램을 입력하세요", "알림");
                    txtAppl.Focus();
                    return;
                }

                tmpData[0] = txtSmplNo.Text;
                tmpData[1] = cmbTmb.Text;
                tmpData[2] = txtSuji.Text;
                tmpData[3] = txtLength.Text;
                tmpData[4] = cmbAppNameManual.Text;
                tmpData[5] = cmbDIVTYPE.SelectedValue.ToString();
                tmpData[6] = cmbXRF.Text;

                CommonDataBase.insert_TB_XRF_SEQ_TEMP(dbConn, tmpData[0], tmpData[1], tmpData[2], tmpData[3], tmpData[4]);

                grdNo.Rows.Count += 1;

                grdNo.SetData(grdNo.Rows.Count - 1, 0, (grdNo.Rows.Count - 1).ToString());
                grdNo.SetData(grdNo.Rows.Count - 1, 1, tmpData[0]);
                grdNo.SetData(grdNo.Rows.Count - 1, 2, tmpData[1]);
                grdNo.SetData(grdNo.Rows.Count - 1, 3, tmpData[3]);
                grdNo.SetData(grdNo.Rows.Count - 1, 6, tmpData[2]);
                grdNo.SetData(grdNo.Rows.Count - 1, 8, tmpData[5]); // 시험 구분
                grdNo.SetData(grdNo.Rows.Count - 1, 9, tmpData[6]); // 시험기 구분
                //grdNo.SetData(grdNo.Rows.Count-1, 9, tmpData[4]); // 프로그램
                grdNo.SetData(grdNo.Rows.Count - 1, 12, tmpData[4]); // NEW 프로그램

                OrderListUpdate();
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "btnAdd_Click");
            }


        }

        private void txtLength_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            pnlManual.Visible = false;
        }

        private void txtSmplNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void btErrorConfirm_Click(object sender, EventArgs e)
        {
            OPCSendSignalClear();

            XRF_ABORT();
            // MessageBox.Show("XRF LIST를 STOP 하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);


            setVisible(pnlRobotError, false);


        }

        private void WriteLogData(string sData, string sFLAG)
        {
            try
            {
                string sFilePath = sErrLog + DateTime.Now.ToString("yyyyMMdd_Err") + ".log";
                TextWriter tw = null;

                if (!Directory.Exists(sErrLog))
                {
                    Directory.CreateDirectory(sErrLog);
                }
                tw = new StreamWriter(sFilePath, true);
                tw.WriteLine("[" + sFLAG + "]-------------------------------" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

                tw.WriteLine(sData);
                tw.WriteLine("-------------------------------[" + sFLAG + "]");
                tw.Close();
            }
            catch { }
        }

        private void lstUAIMessage_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pLC에정보쓰기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SignalInputForm siForm = new SignalInputForm(m_opcMgr, 1, this);
            siForm.ShowDialog();
            siForm.Dispose();

        }

        private void lblStart_Click(object sender, EventArgs e)
        {

        }

        private void chkXrfAuto_CheckedChanged(object sender, EventArgs e)
        {
            m_ChkAutoStart = chkXrfAuto.Checked;
        }

        private void chkXrfAuto_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = Convert.ToString(chkXrfAuto.Tag);
        }

        private void panel7_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void btnActStart_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = Convert.ToString(btnActStart.Tag);
        }

        private void btnActStart_MouseLeave(object sender, EventArgs e)
        {
            toolStripLabel1.Text = "";

        }

        private void btnManualAdd_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = Convert.ToString(btnManualAdd.Tag);
        }

        //private void chkAll_MouseMove(object sender, MouseEventArgs e)
        //{
        //    toolStripLabel1.Text = Convert.ToString(chkAll.Tag);

        //}

        private void btnOrderCancel_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = Convert.ToString(btnOrderCancel.Tag);

        }

        private void btOrder_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = Convert.ToString(btOrder.Tag);

        }

        private void btnBufferClear_MouseMove(object sender, MouseEventArgs e)
        {
            //toolStripLabel1.Text = Convert.ToString(btnBufferClear.Tag);

        }

        private void btnXRFStart_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripLabel1.Text = Convert.ToString(btnXRFStart.Tag);

        }

        #endregion

        #region SetText() 크로스 쓰레드에서 Control 에 데이터를 기록하기위해 사용
        delegate void Text_Involk(TextBox ctrl, string text);
        public void addText(TextBox text, string txtValue)
        {
            if (text.InvokeRequired)
            {
                Text_Involk CI = new Text_Involk(addText);
                text.Invoke(CI, text, txtValue);
            }
            else
            {
                text.Text += txtValue;
            }
        }

        delegate void Ctrl_Involk(Control ctrl, string text);
        public void setText(Control ctrl, string txtValue)
        {
            if (ctrl.InvokeRequired)
            {
                Ctrl_Involk CI = new Ctrl_Involk(setText);
                ctrl.Invoke(CI, ctrl, txtValue);
            }
            else
            {
                ctrl.Text = txtValue;
            }
        }

        delegate void Label_Color_Involk(Label ctrl, string text, Color bcolor, Color fcolor);
        public void setText(Label ctrl, string txtValue, Color bcolor, Color fcolor)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    Label_Color_Involk LbI = new Label_Color_Involk(setText);
                    ctrl.Invoke(LbI, ctrl, txtValue, bcolor, fcolor);
                }
                else
                {
                    ctrl.Text = txtValue;
                    ctrl.BackColor = bcolor;
                    ctrl.ForeColor = fcolor;
                }
            }
            catch { }
        }

        delegate void Button_Involk(Button ctrl);
        public void setClick(Button ctrl)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    Button_Involk Btn = new Button_Involk(setClick);
                    ctrl.Invoke(Btn, ctrl);
                }
                else
                {
                    ctrl.PerformClick();
                }
            }
            catch { }
        }

        delegate void Panel_Visible_Involk(Panel ctrl, bool vis);
        public void setVisible(Panel ctrl, bool vis)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    Panel_Visible_Involk Pnl = new Panel_Visible_Involk(setVisible);
                    ctrl.Invoke(Pnl, ctrl, vis);
                }
                else
                {
                    ctrl.Visible = vis;
                }
            }
            catch { }
        }

        delegate void List_Involk(ListBox ctrl, string text);
        public void setText(ListBox ctrl, string txtValue)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    List_Involk Lst = new List_Involk(setText);
                    ctrl.Invoke(Lst, ctrl, txtValue);
                }
                else
                {
                    ctrl.Items.Add(txtValue);
                    ctrl.SelectedIndex = ctrl.Items.Count - 1;
                }
            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "List_Involk");
            }
        }

        delegate void Grid_Involk(C1.Win.C1FlexGrid.C1FlexGrid ctrl, int index);
        public void delGrid(C1.Win.C1FlexGrid.C1FlexGrid ctrl, int index)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    Grid_Involk CI = new Grid_Involk(delGrid);
                    ctrl.Invoke(CI, ctrl, index);
                }
                else
                {
                    ctrl.RemoveItem(index);
                }
            }
            catch { }
        }
        public void insGrid(C1.Win.C1FlexGrid.C1FlexGrid ctrl, int index)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    Grid_Involk CI = new Grid_Involk(insGrid);
                    ctrl.Invoke(CI, ctrl, index);
                }
                else
                {
                    if (index == -1)
                    {
                        ctrl.Rows.Add();
                    }
                    else
                    {
                        ctrl.Rows.Insert(ctrl.RowSel);
                    }
                }
            }
            catch { }
        }

        delegate void SetGrid_Involk(C1.Win.C1FlexGrid.C1FlexGrid ctrl, int row, int col, object text);
        public void setGrid(C1.Win.C1FlexGrid.C1FlexGrid ctrl, int row, int col, object text)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    SetGrid_Involk CI = new SetGrid_Involk(setGrid);
                    ctrl.Invoke(CI, ctrl, row, col, text);
                }
                else
                {
                    ctrl.SetData(row, col, text);
                }
            }
            catch { }
        }

        delegate void Pic_Involk(PictureBox ctrl, int index);
        public void picChange(PictureBox ctrl, int index)
        {
            try
            {
                if (ctrl.InvokeRequired)
                {
                    Pic_Involk pic = new Pic_Involk(picChange);
                    ctrl.Invoke(pic, ctrl, index);
                }
                else
                {
                    switch (index)
                    {
                        case 0:
                            ctrl.Image = Properties.Resources.SignalEmpth;
                            break;
                        case 1:
                            ctrl.Image = Properties.Resources.SignalYellow;
                            break;
                        case 2:
                            ctrl.Image = Properties.Resources.SignalGreen;
                            break;
                        case 3:
                            ctrl.Image = Properties.Resources.SignalBlue;
                            break;
                        case 4:
                            ctrl.Image = Properties.Resources.SignalRed3;
                            break;
                    }
                }
            }
            catch { }
        }
        #endregion

        #region OPC 관련 함수 모음

        #region [OPCRegServer: OPC Server 연결]
        private bool OPCRegServer()
        {
            int iResFunc = 0;

            //{create instance}
            m_opcMgr = new opcMgrClass();

            //{로깅 패스를 설정합니다.} 
            m_opcMgr.opcSetLogDirectory(Application.StartupPath + "\\NetIFLog");

            // OPC Server에 연결한다.
            // 4번째 Parameter는 OPCWorkX에 등록된 Channel명 과 Device명 이다.  sCnfgPath
            iResFunc = m_opcMgr.opcRegSvrEx(sCnfgPath, "OPCsoft.OPCWorkX.V12_1", "", "[XRF_Hysco]XRF_FA.", 5);


            //{Write/Read시 Dataype 체크여부와 로그옵션설정}
            m_opcMgr.opcSetOptions(1, "111");

            //{mapping events}
            __opcMgr_evSvrShutdownEventHandler EvopcSvrShutDown = new __opcMgr_evSvrShutdownEventHandler(OnOpcSvrDown);
            m_opcMgr.evSvrShutdown += EvopcSvrShutDown;

            //{mapping events for Data Group}
            __opcMgr_evDataChangeEventHandler EvopcDataGroup = new __opcMgr_evDataChangeEventHandler(OnOpcDataChange);
            m_opcMgr.evDataChange += EvopcDataGroup;

            //{mapping events for Event Group}
            __opcMgr_evEventChangeEventHandler EvopcEventChange = new __opcMgr_evEventChangeEventHandler(OnOpcEventChange);
            m_opcMgr.evEventChange += EvopcEventChange;

            //{mapping events for Write Completed}
            __opcMgr_evAsyncWriteCompleteEventHandler EvopcAsyncWriteComplete = new __opcMgr_evAsyncWriteCompleteEventHandler(OnAsyncWriteComplete);
            m_opcMgr.evAsyncWriteComplete += EvopcAsyncWriteComplete;

            //{mapping events for Read Completed}
            __opcMgr_evAsyncReadCompleteEventHandler EvopcAsyncReadComplete = new __opcMgr_evAsyncReadCompleteEventHandler(OnAsyncReadComplete);
            m_opcMgr.evAsyncReadComplete += EvopcAsyncReadComplete;

            //{mapping events for timeout about Async-Functions}
            __opcMgr_evAsyncTimeoutEventHandler EvopcAsyncTimeout = new __opcMgr_evAsyncTimeoutEventHandler(OnAsyncTimeout);
            m_opcMgr.evAsyncTimeout += EvopcAsyncTimeout;

            if (iResFunc == 1)
            {
                InsertLog("OPC서버 연결 성공!");
                return true;
            }
            else
            {
                InsertLog("OPC서버 연결실패! Error Code:" + iResFunc);
                return false;
            }
        }
        #endregion

        #region 모든 그룹에 소속된 모든 Tag를 읽는다.
        //private void ReadBufferGroup()
        //{
        //    if (m_opcMgr == null)
        //    {
        //        InsertLog("Is NOT connected to OPC Server. Please, click opcRegSvr button first!");
        //        return;
        //    }

        //    int iResFunc = 0;
        //    object[] oReadVals = null;
        //    int[] iQualities = null;

        //    Thread.Sleep(1000);

        //    oReadVals =  new object[OPCFunction.TAG_BUFFER];
        //    iQualities = new int[OPCFunction.TAG_BUFFER];

        //    iResFunc = OPCFunction._OPC_Read_Group_Tags(m_opcMgr, "XRF_BUFFER", OPCFunction.TAG_BUFFER, ref oReadVals, ref iQualities);
        //    if (iResFunc == 1)
        //    {
        //        InsertLog("시편 버퍼 정보 Read 성공!");
        //    }
        //    else
        //    {
        //        InsertLog("시편 버퍼 정보 Read 실패! Error Code:" + iResFunc);
        //    }

        //    Thread.Sleep(1000);

        //    oReadVals = new object[OPCFunction.TAG_GROUP01];
        //    iQualities = new int[OPCFunction.TAG_GROUP01];

        //    iResFunc = OPCFunction._OPC_Read_Group_Tags(m_opcMgr, "XRF_GROUP01", OPCFunction.TAG_GROUP01, ref oReadVals, ref iQualities);
        //    if (iResFunc == 1)
        //    {
        //        InsertLog("작업 진행 정보 Read 성공!");
        //    }
        //    else
        //    {
        //        InsertLog("작업 진행 정보 Read 실패! Error Code:" + iResFunc);
        //    }
        //    Thread.Sleep(1000);

        //    oReadVals = new object[OPCFunction.TAG_CONV_LOC];
        //    iQualities = new int[OPCFunction.TAG_CONV_LOC];

        //    iResFunc = OPCFunction._OPC_Read_Group_Tags(m_opcMgr, "XRF_CONV_LOC", OPCFunction.TAG_CONV_LOC, ref oReadVals, ref iQualities);
        //    if (iResFunc == 1)
        //    {
        //        InsertLog("공정 버퍼 정보 Read 성공!");
        //    }
        //    else
        //    {
        //        InsertLog("공정 버퍼 정보 Read 실패! Error Code:" + iResFunc);
        //    }

        //    Thread.Sleep(1000);

        //    oReadVals = new object[OPCFunction.TAG_SIGNAL];
        //    iQualities = new int[OPCFunction.TAG_SIGNAL];

        //    iResFunc = OPCFunction._OPC_Read_Group_Tags(m_opcMgr, "XRF_SIGNAL", OPCFunction.TAG_SIGNAL, ref oReadVals, ref iQualities);
        //    if (iResFunc == 1)
        //    {
        //        InsertLog("공정 신호 정보 Read 성공!");
        //    }
        //    else
        //    {
        //        InsertLog("공정 신호 정보 Read 실패! Error Code:" + iResFunc);
        //    }
        //}
        #endregion

        //{event for server's shotdown}
        private void OnOpcSvrDown(String sSvrName, String sNode, String sReason)
        {
            InsertLog("SvrDown", "Svr", sSvrName + "@" + sNode + sReason);
        }

        private bool opcClearFirst = true;
        private void OnOpcDataChange(string sGroupName, int lTagCount, ref int[] lCltHandles, ref object[] vtVals, ref int[] lQualities)
        {

            InsertLog(sGroupName + "의 Tag Data Change Event " + lCltHandles.Length.ToString() + " 발생");
            InsertLog("lTagCount : " + lTagCount.ToString() + " | " + lCltHandles[lTagCount - 1].ToString() + " | " + vtVals[lTagCount - 1].ToString());
            //PLCDefine plcDef = new PLCDefine();

            for (int i = 0; i < lTagCount; i++)
            {
                if (string.Compare("XRF_BUFFER", sGroupName) == 0)
                {
                    BufferDataWrite(sGroupName, lCltHandles[i], vtVals[i].ToString());
                }
                if (string.Compare("XRF_GROUP01", sGroupName) == 0)
                {
                    BufferDataWrite(sGroupName, lCltHandles[i], vtVals[i].ToString());
                }
                if (string.Compare("XRF_CONV_LOC", sGroupName) == 0)
                {
                    BufferDataWrite(sGroupName, lCltHandles[i], vtVals[i].ToString());
                }
                if (string.Compare("XRF_SIGNAL", sGroupName) == 0)
                {
                    BufferDataWrite(sGroupName, lCltHandles[i], vtVals[i].ToString());
                }
                if (string.Compare("XRF_ALARM", sGroupName) == 0)
                {
                    BufferDataWrite(sGroupName, lCltHandles[i], vtVals[i].ToString());
                }
                if (string.Compare("XRF_GROUP02", sGroupName) == 0)
                {
                    BufferDataWrite(sGroupName, lCltHandles[i], vtVals[i].ToString());
                }
            }

            if (string.Compare("XRF_BUFFER", sGroupName) == 0)
            {
                // FA 자동으로 구동시 HMI => PLC로 버퍼 변경정보 자동으로 쓰지 않음
                if (!m_bFlagStart)
                    BufferCountWrite();
            }

            if (opcClearFirst)
            {
                opcClearFirst = false;
                OPCSendSignalClear();
            }

        }

        //{event for datachange of group} 
        private void OnOpcEventChange(string sGroupName, int lTagCount, ref int[] lCltHandles, ref object[] vtVals)
        {
            //PLCDefine plcDef = new PLCDefine();

            //string sValue = "";
            //for (int i = 0; i < lTagCount; i++)
            //{
            //    InsertLog("Event Change", plcDef[i], sValue);
            //}
        }

        //{event for read complete} - 20090211
        private void OnAsyncReadComplete(int lTransID, string sGroupName, int lTagCount, ref int[] lCltHandles, ref object[] vtVals, ref int[] lQualities)
        {
            for (int i = 0; i < lTagCount; i++)
            {
                if (string.Compare("XRF_BUFFER", sGroupName) == 0)
                {
                    BufferDataWrite("XRF_BUFFER", lCltHandles[i], vtVals[i].ToString());
                }
            }
            InsertLog("Async Read is Completed");
        }

        //{event for write complete} 
        private void OnAsyncWriteComplete(int lTransID, string sGroupName, int lTagCount, ref int[] lCltHandles, int lError)
        {
            // Write 를 끝낸후 본 이벤트에서 다음 Tag를 Write할수 있게 조건을 세울 것.

            InsertLog("Tag 에 정보 저장 성공 !!");
        }

        //{event for timeout about Async-Functions happened} 
        private void OnAsyncTimeout(int lTransID, int lCallType, int lCause)
        {
            string sValue = "";
            sValue = "[TransID: " + lTransID + "] CallType: " + lCallType + ", Cause: " + lCause;
            InsertLog("Async function is timeout.", "TimeOut", sValue);
        }

        #region Log Message 표시
        private void InsertLog(string msg)
        {
            setText(lstEquipMessage, DateTime.Now.ToString() + " : " + msg);
        }

        private void InsertLog(string sType, string sBelong, string sValue)
        {
            setText(lstEquipMessage, DateTime.Now.ToString() + " : " + sType + "," + sBelong + "," + sValue);
        }
        #endregion

        #endregion


        #region [ 세척 신호보내기 ]

        /// <summary>
        /// PLC로 세척 신호보내기. true = 세척
        /// </summary>
        private void SetClean(bool CleanYN)
        {

            object[] oWriteData = new object[1];
            oWriteData[0] = CleanYN;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 27;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("세척신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("세척신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }

        /// <summary>
        /// PLC로 W제거 신호보내기. True : W제거
        /// </summary>
        private void Remove_W(bool remYN)
        {

            object[] oWriteData = new object[1];
            oWriteData[0] = remYN;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 28;

            Thread.Sleep(30);

            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("W제거 신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("W제거 신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }

        /// <summary>
        /// PLC로 C제거 신호보내기. True : C제거
        /// </summary>
        private void Remove_C(bool remYN)
        {

            object[] oWriteData = new object[1];
            oWriteData[0] = remYN;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 29;


            Thread.Sleep(30);

            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("C제거 신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("C제거 신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }

        /// <summary>
        /// PLC로 D제거 신호보내기. True : D제거
        /// </summary>
        private void Remove_D(bool remYN)
        {

            object[] oWriteData = new object[1];
            oWriteData[0] = remYN;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 30;


            Thread.Sleep(30);

            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("D제거 신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("D제거 신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }

        /// <summary>
        /// PLC로 시험기코드 신호보내기. X1 = PANAlytical, X2 = Bruker
        /// </summary>
        private void SendXRF_Kind(string XRF)
        {

            object[] oWriteData = new object[1];
            oWriteData[0] = XRF;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 31;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP01", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("시험기코드 신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("시험기코드 신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }

        #endregion

        #region [ 버퍼 그리드 에디트 모드(grdBuffer_AfterEdit) ]
        /// <summary>
        /// 버퍼그리드에 직접 시편정보 입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grdBuffer_AfterEdit(object sender, RowColEventArgs e)
        {
            try
            {

                object[] oWriteData = null;
                int[] iTagHandle = null;
                int iInputQty = 0;

                if (grdBuffer.ColSel > 0 && grdBuffer.RowSel > 0)
                {
                    string sInputData = this.grdBuffer.GetData(grdBuffer.RowSel, grdBuffer.ColSel).ToString().ToUpper();
                    int iStartHandleIndex = ((grdBuffer.ColSel - 1) * 22 + grdBuffer.RowSel - 1);
                    if (sInputData.Length == 10)
                    {
                        oWriteData = new object[1];
                        iTagHandle = new int[1];

                        iTagHandle[0] = iStartHandleIndex;
                        oWriteData[0] = sInputData;


                        if (!dicSampleInfo.ContainsKey(grdBuffer.GetData(grdBuffer.RowSel, grdBuffer.ColSel).ToString().Substring(0, 8).Trim().ToUpper()))
                        {
                            dicSampleInfo.Add(grdBuffer.GetData(grdBuffer.RowSel, grdBuffer.ColSel).ToString().Substring(0, 8).Trim().ToUpper(), tbApplicationName.Text.Trim().ToUpper());
                        }
                        else
                        {
                            dicSampleInfo[grdBuffer.GetData(grdBuffer.RowSel, grdBuffer.ColSel).ToString().Substring(0, 8).Trim().ToUpper()] = tbApplicationName.Text.Trim().ToUpper();
                        }
                        sSPL[iStartHandleIndex] = (string)this.grdBuffer.GetData(grdBuffer.RowSel, grdBuffer.ColSel);       // 시편번호
                        sbBufGridAppl[iStartHandleIndex] = tbApplicationName.Text.Trim();                                   // 3. Application Name
                        iInputQty = iStartHandleIndex + 1;
                    }
                    else if (sInputData.Length == 9 &&
                            (sInputData.Substring(8, 1) == "T" || sInputData.Substring(8, 1) == "B" || sInputData.Substring(8, 1) == "M"))
                    {
                        int iDivCnt = cmbDIVTYPE_DIR.SelectedValue.ToString().Length;
                        string strDivType = cmbDIVTYPE_DIR.SelectedValue.ToString();

                        oWriteData = new object[iDivCnt];
                        iTagHandle = new int[iDivCnt];

                        for (int k = 0; k < iDivCnt; k++)
                        {
                            iTagHandle[k] = iStartHandleIndex + k;
                            oWriteData[k] = sInputData + strDivType.Substring(k, 1);

                            if (!dicSampleInfo.ContainsKey((sInputData + strDivType.Substring(k, 1)).Substring(0, 8).Trim().ToUpper()))
                            {
                                dicSampleInfo.Add((sInputData + strDivType.Substring(k, 1)).Substring(0, 8).Trim().ToUpper(), tbApplicationName.Text.Trim().ToUpper());
                            }
                            else
                            {
                                dicSampleInfo[(sInputData + strDivType.Substring(k, 1)).Substring(0, 8).Trim().ToUpper()] = tbApplicationName.Text.Trim().ToUpper();
                            }
                            sSPL[iStartHandleIndex + k] = sInputData + strDivType.Substring(k, 1);                                  // 시편번호
                            sbBufGridAppl[iStartHandleIndex + k] = tbApplicationName.Text.Trim();                                   // 3. Application Name
                        }
                        iInputQty = iStartHandleIndex + iDivCnt;
                    }
                    else
                    {
                        // 잘못된 데이터
                        grdBuffer.SetData(grdBuffer.RowSel, grdBuffer.ColSel, string.Empty);
                        return;
                    }

                }
                else
                {
                    return;
                }

                //SetBufferInputQty(iInputQty);

                string sGroupName = string.Empty;
                sGroupName = "XRF_BUFFER";
                int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, sGroupName, ref iTagHandle, ref oWriteData);
                if (iResFunc != 1)
                {
                    //msgLabel.Text = "Test Data Send Fail " + iResFunc;
                }
                else
                {
                    //msgLabel.Text = "Test Data Send OK";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        private int BufferStartPos(string strDeviceCode)
        {
            int iPos = -1;
            string[] strArrSPL = null;
            switch (strDeviceCode)
            {
                case "X1":
                    strArrSPL = new string[XrfControl.m_X1XrfBufferSize];
                    Array.Copy(sSPL, strArrSPL, XrfControl.m_X1XrfBufferSize);
                    //iPos = Array.FindLastIndex(strArrSPL, element => !string.IsNullOrEmpty(element));
                    break;
                case "X2":
                    strArrSPL = new string[XrfControl.m_X2XrfBufferSize];
                    Array.Copy(sSPL, XrfControl.m_X1XrfBufferSize, strArrSPL, 0, XrfControl.m_X2XrfBufferSize);
                    iPos = Array.FindLastIndex(strArrSPL, element => !string.IsNullOrEmpty(element));
                    //iPos += XrfControl.m_X1XrfBufferSize;
                    break;
            }
            iPos = Array.FindLastIndex(strArrSPL, element => !string.IsNullOrEmpty(element));
            return iPos;
        }




        private void grdBuffer_Click(object sender, EventArgs e)
        {
            if ((grdBuffer.RowSel >= grdBuffer.Rows.Fixed && grdBuffer.ColSel >= grdBuffer.Cols.Fixed)
                || !string.IsNullOrEmpty(grdBuffer.GetDataDisplay(grdBuffer.RowSel, grdBuffer.ColSel)))
            {
                int iIndex = -1;
                try
                {
                    iIndex = Array.FindIndex(sSPL, element => element.Equals(grdBuffer.GetDataDisplay(grdBuffer.RowSel, grdBuffer.ColSel)));
                    if (iIndex >= 0)
                    {
                        lblXrfApplication.Text = sbBufGridAppl[iIndex];
                        //tbApplicationName.Text = sbBufGridAppl[iIndex];
                    }
                    else
                    {
                        lblXrfApplication.Text = string.Empty;
                    }
                }
                catch
                {
                    lblXrfApplication.Text = string.Empty;
                }
            }
            else
            {
                lblXrfApplication.Text = string.Empty;
            }

            if (tbApplicationName.Text.Length == 0 && string.IsNullOrEmpty(lblXrfApplication.Text))
            {
                MessageBox.Show("시편을 분석할 프로그램명이 빠졌습니다!");
                grdBuffer.Select(-1, -1);
                return;
            }
        }

        #region [ 버퍼 정보 백업 테이블 ]
        /// <summary>
        /// 
        /// </summary>
        private void BufferInfoUpdate()
        {
            this.Invoke(new Action(() =>
            {
                // 버퍼 데이터 갱신
                for (int iRow = 0; iRow < 132; iRow++)
                {
                    m_dtBufferInfo.Rows[iRow]["시편정보"] = sSPL[iRow];
                    m_dtBufferInfo.Rows[iRow]["XRF프로그램명"] = sbBufGridAppl[iRow];
                }

                // 파일 저장
                DirectoryInfo di = new DirectoryInfo(m_strFilePath);
                if (!di.Exists)
                {
                    di.Create();
                }
                m_dtBufferInfo.WriteXml(m_strFilePath + m_strBufferFileName);
            }));
        }

        private void InitBufferDataTable()
        {
            m_dtBufferInfo = new DataTable("BUFFERINFO");
            m_dtBufferInfo.Columns.Add("버퍼정보");
            m_dtBufferInfo.Columns.Add("시편정보");
            m_dtBufferInfo.Columns.Add("XRF프로그램명");

            DataRow dr;
            for (int iBufferCnt = 0; iBufferCnt < 132; iBufferCnt++)
            {
                dr = m_dtBufferInfo.NewRow();
                dr["버퍼정보"] = (iBufferCnt + 1).ToString();
                dr["시편정보"] = "";
                dr["XRF프로그램명"] = "";
                m_dtBufferInfo.Rows.Add(dr);
            }
        }
        #endregion

        #region [ 오더리스트 백업 테이블 ]
        private void OrderListUpdate()
        {
            this.Invoke(new Action(() =>
            {
                m_dtOrderList.Clear();
                DataRow dr;
                // 버퍼 데이터 갱신
                for (int iRow = grdNo.Rows.Fixed; iRow < grdNo.Rows.Count; iRow++)
                {
                    dr = m_dtOrderList.NewRow();
                    dr["SEQ"] = grdNo.GetDataDisplay(iRow, "SEQ");
                    dr["시편번호"] = grdNo.GetDataDisplay(iRow, "시편번호");
                    dr["TMB"] = grdNo.GetData(iRow, "TMB");
                    dr["길이"] = grdNo.GetDataDisplay(iRow, "길이");
                    dr["취소"] = grdNo.GetData(iRow, "취소") == null ? false : grdNo.GetData(iRow, "취소");
                    dr["세척유무"] = grdNo.GetData(iRow, "세척유무") == null ? false : grdNo.GetData(iRow, "세척유무");
                    dr["DivType"] = grdNo.GetData(iRow, "DivType");
                    dr["시험기"] = grdNo.GetData(iRow, "시험기");
                    dr["Application"] = grdNo.GetDataDisplay(iRow, "Application");
                    m_dtOrderList.Rows.Add(dr);
                }

                // 파일 저장
                DirectoryInfo di = new DirectoryInfo(m_strFilePath);
                if (!di.Exists)
                {
                    di.Create();
                }
                m_dtOrderList.WriteXml(m_strFilePath + m_strOrderListFileName);
            }));
        }

        private void InitOrderListDataTable()
        {
            m_dtOrderList = new DataTable("ORDERLIST");
            m_dtOrderList.Columns.Add("SEQ");
            m_dtOrderList.Columns.Add("시편번호");
            m_dtOrderList.Columns.Add("TMB");
            m_dtOrderList.Columns.Add("길이");
            m_dtOrderList.Columns.Add("취소", typeof(bool));
            m_dtOrderList.Columns.Add("세척유무", typeof(bool));
            m_dtOrderList.Columns.Add("DivType");
            m_dtOrderList.Columns.Add("시험기");
            m_dtOrderList.Columns.Add("Application");
        }
        #endregion

        private int X2_STATUS_MC(int MC_STATUS)
        {
            int RetVal = 0;
            string sCommand = string.Empty;
            XRF_COMMAND xcmd_X2;

            switch (MC_STATUS)
            {
                case 0:
                    //XRF_COMMAND xcmd_X2 = new XRF_COMMAND();
                    //sCommand = xcmd_X2.XRF_BRUKER_MEASMP(mysSML_STATUS[k].sSPLNAME + "B", mysSML_STATUS[k].sPROGRAMNAME);
                    //sWorkingSMLName = mysSML_STATUS[k].sSPLNAME + "B";
                    //XRF_Send_MSG_X2(sCommand);

                    if (XRF_COMMAND.XRF_AUTO_X2 && X2_BufferSampleCount() > 0 && bAStatus_X2[1] && !bAStatus_X2[3] && m_TestList.Count < m_MinBufferSampleCount)
                    {
                        if (!XRF_COMMAND.UNLOAD_STATUS)
                        {
                            sSML_STATUS smlstatus = new sSML_STATUS();
                            smlstatus.sSPLNAME = sSPL[88 + Convert.ToInt32(txtX2OutPutBuffer.Text.Trim())] + "F";
                            smlstatus.sPROGRAMNAME = sbBufGridAppl[88 + Convert.ToInt32(txtX2OutPutBuffer.Text.Trim())];
                            smlstatus.nBufferIndex = 88 + Convert.ToInt32(txtX2OutPutBuffer.Text.Trim());
                            m_TestList.Add(smlstatus);
                            DisplaySmlinX2(m_TestList);

                            // PLC로 전면투입허가 신호보내기
                            Xrf_Front_in_Enable_X2();
                            MeasureMentHistoryLog("PLC로 전면투입허가 신호 보냄", 2);
                        }
                    }


                    //
                    break;
                case 5: // 전면투입허가
                    break;

                case 10: // 전면 투입 완료 신호가 왔을 때
                         //if (bAStatus_X2[0])
                         //{
                         //Xrf_Front_in_EnableOff_X2();                        
                    xcmd_X2 = new XRF_COMMAND();
                    sCommand = xcmd_X2.XRF_BRUKER_MEASMP(m_TestList[m_TestList.Count - 1].sSPLNAME, m_TestList[m_TestList.Count - 1].sPROGRAMNAME);
                    XRF_Send_MSG_X2(sCommand, true);
                    MeasureMentHistoryLog("전면 시험명령보냄(XRF_BRUKER_MEASMP)", 2);
                    //}
                    break;

                case 15:// 이면투입허가
                    xcmd_X2 = new XRF_COMMAND();
                    sCommand = xcmd_X2.XRF_BRUKER_SMPUNL(m_TestList[0].sSPLNAME);
                    XRF_Send_MSG_X2(sCommand, true);
                    Thread.Sleep(200);
                    MeasureMentHistoryLog("전면샘플 삭제 명령실행(XRF_BRUKER_SMPUNL)", 2);
                    break;

                case 20: // 이면투입완료 신호 On
                    XRF_COMMAND.UNLOAD_STATUS = false;

                    // List Add
                    sSML_STATUS aaa = new sSML_STATUS();
                    aaa.sSPLNAME = m_TestList[0].sSPLNAME.Substring(0, m_TestList[0].sSPLNAME.Length - 1) + "B";
                    aaa.sPROGRAMNAME = m_TestList[0].sPROGRAMNAME;
                    aaa.nBufferIndex = m_TestList[0].nBufferIndex;
                    m_TestList.Add(aaa);
                    DisplaySmlinX2(m_TestList);

                    // List Remove
                    m_TestList.RemoveAt(0);
                    DisplaySmlinX2(m_TestList);

                    xcmd_X2 = new XRF_COMMAND();
                    sCommand = xcmd_X2.XRF_BRUKER_MEASMP(m_TestList[m_TestList.Count - 1].sSPLNAME, m_TestList[m_TestList.Count - 1].sPROGRAMNAME);
                    XRF_Send_MSG_X2(sCommand, true);
                    MeasureMentHistoryLog("이면 시험명령보냄(XRF_BRUKER_MEASMP)", 2);
                    //}

                    break;

                case 25: // 이면시험완료
                    xcmd_X2 = new XRF_COMMAND();
                    sCommand = xcmd_X2.XRF_BRUKER_SMPUNL(m_TestList[0].sSPLNAME);
                    XRF_Send_MSG_X2(sCommand, true);
                    MeasureMentHistoryLog("이면샘플 삭제 명령실행(XRF_BRUKER_SMPUNL)", 2);
                    Thread.Sleep(200);
                    break;

                case 30: // 시편 배출완료
                    XRF_COMMAND.UNLOAD_STATUS = false;

                    m_TestList.RemoveAt(0);
                    DisplaySmlinX2(m_TestList);
                    break;
            }


            return RetVal;

        }   // End-X2_STATUS_MC


        private int X2_BufferSampleCount()
        {
            int bufCount = 0;
            for (int i = XrfControl.m_X1XrfBufferSize; i < XrfControl.m_XrfBuffer.Length; i++)
            {
                if (XrfControl.m_XrfBuffer[i] != null)   //시편 버퍼 데이타가 들어있는 Array for BRUKER
                {
                    bufCount++;
                }
            }
            return bufCount;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            object[] oWriteData = new object[1];
            oWriteData[0] = true;
            int[] iTagHandle = new int[1];
            iTagHandle[0] = 9;


            int iResFunc = OPCFunction._OPC_Writes_Ascyn_Tags(m_opcMgr, "XRF_GROUP02", ref iTagHandle, ref oWriteData);
            if (iResFunc != 1)
            {
                InsertLog("XRF2 이면투입완료신호 전송완료 Write Failed! Error Code:" + iResFunc);
            }
            else
            {
                InsertLog("XRF2 이면투입완료신호 전송완료");
                //m_InputCnt++;
                PLCDefine.m_PlcFrontInEnable = true;
            }
        }

        private void DisplaySmlinX2(List<sSML_STATUS> TestList)
        {
            setText(lblSML0, string.Empty);
            setText(lblSML1, string.Empty);
            setText(lblSML2, string.Empty);
            lblSML0.BackColor = Color.Transparent;
            lblSML1.BackColor = Color.Transparent;
            lblSML2.BackColor = Color.Transparent;

            if (TestList.Count > 0)
            {
                setText(lblSML0, TestList[0].sSPLNAME);
                lblSML0.BackColor = Color.Yellow;
            }
            if (TestList.Count > 1)
            {
                setText(lblSML1, TestList[1].sSPLNAME);
                lblSML1.BackColor = Color.Yellow;
            }
            if (TestList.Count > 2)
            {
                setText(lblSML2, TestList[2].sSPLNAME);
                lblSML2.BackColor = Color.Yellow;
            }
        }

        private void btnSelXRFRun_Click(object sender, EventArgs e)
        {
        }

        // BRUKER XRF RESET
        private void btnX2Reset_Click(object sender, EventArgs e)
        {
            XRF_COMMAND xcmd_X2 = new XRF_COMMAND();
            string sCommand = xcmd_X2.XRF_BRUKER_RESET();
            XRF_Send_MSG_X2(sCommand, true);
        }

        private void rbtn1_CheckedChanged(object sender, EventArgs e)
        {
            // X2 단동운전
            if (rbtn1.Checked)
            {
                m_MinBufferSampleCount = 3;
            }
            else
            {   // X1, X2 연동운전
                m_MinBufferSampleCount = 1;
            }

        }

        #region [ 시험 진행시 시험동작 엇갈리는 경우 찾기 위한 임시 로그 ]
        private string strMeasureMentLogPath = @"C:\MeasureLog";
        private void MeasureMentHistoryLog(string LogMessage, int DeviceNumber)
        {
            this.Invoke(new Action(() =>
            {
                string strLogFileNamePan = @"\XRF_FA_DATA_PAN" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                string strLogFileNameBru = @"\XRF_FA_DATA_BRU" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                DirectoryInfo di = new DirectoryInfo(strMeasureMentLogPath);
                if (!di.Exists)
                    di.Create();

                switch (DeviceNumber)
                {
                    case 1:
                        File.AppendAllText(di.FullName + strLogFileNamePan, "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] : " + LogMessage + Environment.NewLine);
                        break;
                    case 2:
                        File.AppendAllText(di.FullName + strLogFileNameBru, "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] : " + LogMessage + Environment.NewLine);
                        break;
                }
            }));
        }
        #endregion

        private void cmbAppNameManual_DropDown(object sender, EventArgs e)
        {

        }


        #region [ 사용자 지정 메서드 ]
        #endregion

        private void WriteLogDataTemp(string sData, string sFLAG)
        {
            try
            {
                string sFilePath = sErrLog + DateTime.Now.ToString("yyyyMMdd_Err") + "appTemp" + ".log";
                TextWriter tw = null;

                if (!Directory.Exists(sErrLog))
                {
                    Directory.CreateDirectory(sErrLog);
                }
                tw = new StreamWriter(sFilePath, true);
                tw.WriteLine("[" + sFLAG + "]-------------------------------" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

                tw.WriteLine(sData);
                tw.WriteLine("-------------------------------[" + sFLAG + "]");
                tw.Close();
            }
            catch { }
        }

        private void grdNo_SelChange(object sender, EventArgs e)
        {
            int iColSel = grdNo.ColSel;
            int iRowSel = grdNo.RowSel;
            if (grdNo.Cols[iColSel].Name.Equals("Application"))
            {
                txtAppName.Text = grdNo.GetDataDisplay(iRowSel, iColSel);
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MssqlConnect.Instance.Close();
        }
    }

}
