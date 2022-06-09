using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
//using System.Data.SqlClient;
//using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace XRF_FA
{
    public class CommonDataBase
    {

        public static string Sample_Length = string.Empty;

        BackgroundWorker bgW = null;
        private string sLogPath = Application.StartupPath + @"\Log\";


        #region [CommonDataBase: 생성자]
        public CommonDataBase()
        {
            bgW = new BackgroundWorker();
            bgW.WorkerReportsProgress = true;
            bgW.WorkerSupportsCancellation = true;
        }
        #endregion

        private void bgW_DoWork(object sender, DoWorkEventArgs e)
        {
        }

        /// <summary>
        /// TB_XRF_SEQ Table Update End
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void upt_TB_XRF_SEQ_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error");
            }
            //else
            //{
            //    MessageBox.Show("COMPLETE");
            //}
        }


        private void ins_TB_XRF_FA_DIRECT_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error");
            }
            //else
            //{
            //    MessageBox.Show("COMPLETE");
            //}
        }

        private void upt_TB_XRF_FA_DIRECT_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error");
            }
            //else
            //{
            //    MessageBox.Show("COMPLETE");
            //}
        }

        private void ins_TB_XRF_HISTORY_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error");
            }
            //else
            //{
            //    MessageBox.Show("COMPLETE");
            //}
        }

        private void ins_TB_TEST_RESULT_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error");
            }
            //else
            //{
            //    MessageBox.Show("COMPLETE");
            //}
        }

        private void GetSampleLength_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error");
            }
            else
            {
                Sample_Length = Convert.ToString(e.Result);
            }
        }

        /// <summary>
        /// BackGroundWorker 시작
        /// </summary>
        /// <param name="sData"></param>
        /// <param name="iFlag"></param>
        /// <returns></returns>
        public SqlDataReader Execute_BackGroundWorker(string[] sData, int iFlag)
        {
            switch (iFlag)
            {
                case 1:
                    bgW.DoWork += new DoWorkEventHandler(upt_TB_XRF_SEQ);
                    bgW.RunWorkerAsync(sData);
                    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(upt_TB_XRF_SEQ_RunWorkerCompleted);
                    break;
                case 2:
                    bgW.DoWork += new DoWorkEventHandler(ins_TB_XRF_FA_DIRECT);
                    bgW.RunWorkerAsync(sData);
                    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ins_TB_XRF_FA_DIRECT_RunWorkerCompleted);
                    break;
                case 3:
                    bgW.DoWork += new DoWorkEventHandler(upt_TB_XRF_FA_DIRECT);
                    bgW.RunWorkerAsync(sData);
                    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(upt_TB_XRF_FA_DIRECT_RunWorkerCompleted);
                    break;
                case 5:
                    bgW.DoWork += new DoWorkEventHandler(ins_TB_XRF_HISTORY);
                    bgW.RunWorkerAsync(sData);
                    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ins_TB_XRF_HISTORY_RunWorkerCompleted);
                    break;
                case 6:
                    bgW.DoWork += new DoWorkEventHandler(ins_TB_TEST_RESULT);
                    bgW.RunWorkerAsync(sData);
                    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ins_TB_TEST_RESULT_RunWorkerCompleted);
                    break;
                case 7:
                    bgW.DoWork += new DoWorkEventHandler(GetSampleLength);
                    bgW.RunWorkerAsync(sData);
                    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetSampleLength_RunWorkerCompleted);
                    break;


            }
            return null;

        }



        private bool Found_TB_XRF_SEQ(string smplNO, string tmbDiv)
        {
            SqlCommand command = null;
            object obj = null;
            bool bReturn = false;
            string sSql = string.Empty;

            SqlConnection dbCon = MssqlConnect.Instance.MssqlCnn;

            sSql = string.Format("SELECT SMPLNO FROM TB_XRF_SEQ WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", smplNO, tmbDiv);
            try
            {

                if (dbCon.State == ConnectionState.Closed)
                    dbCon.Open();

                command = dbCon.CreateCommand();

                command.CommandText = sSql;
                obj = command.ExecuteScalar();

                if (obj == null)
                {
                    bReturn = false;
                }
                else
                {
                    bReturn = true;
                }

            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "Found_TB_XRF_SEQ");
                return false;
            }
            finally
            {
                if (dbCon.State == ConnectionState.Open)
                    dbCon.Close();
                if (command != null) command.Dispose();
                command = null;
            }
            return bReturn;
        }


        /// <summary>
        /// 시편의 길이를 읽는다.
        /// </summary>
        /// <returns>string length</returns>
        private void GetSampleLength(object sender, DoWorkEventArgs e)
        {

            string[] s = (string[])e.Argument;
            string sSmplNo = s[0].Trim();
            string sTmb = s[1].Trim();

            SqlCommand command = null;
            object oLen = null;
            string Sql = string.Empty;
            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            try
            {
                if (Found_TB_XRF_SEQ(sSmplNo, sTmb))
                {
                    Sql = string.Format("SELECT SMPLLENGTH FROM TB_XRF_SEQ WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", sSmplNo, sTmb);
                }
                else
                {
                    Sql = string.Format("SELECT SMPLLENGTH FROM TB_XRF_SEQ_TEMP WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", sSmplNo, sTmb);
                }


                command = dbConn.CreateCommand();

                command.CommandText = Sql;

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                oLen = command.ExecuteScalar();

            }
            catch (Exception ex)
            {
                WriteLogData(ex.Message, "CommonDataBase:GetSampleLength");
                e.Result = oLen;
            }
            finally
            {
                if (dbConn.State == ConnectionState.Open)
                    dbConn.Close();

                if (command != null) command.Dispose();
                command = null;
            }

            e.Result = oLen;
        }


        /// <summary>
        /// 시험지시(TB_XRF_SEQ) Table Data를 읽는다.
        /// </summary>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader GetOrderList(SqlConnection dbConn)
        {

            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                command = dbConn.CreateCommand();

                command.CommandText = "SELECT SMPLNO, TMBDIV, ORDSEQ, MTCHECK, RECHECK, SUJI, SMPLLENGTH, EXNAME, DIVTYPE, CLEANYN   FROM TB_XRF_SEQ " +
                                             "WHERE RECHECK = 'S' OR RECHECK = 'D' ORDER BY ORDSEQ";


                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                reader = command.ExecuteReader();

            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:GetOrderList" + ex.ToString());
                return null;
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }

            return reader;
        }


        /// <summary>
        /// 시험방법을 읽는다.
        /// </summary>
        /// <returns>SqlDataReader</returns>
        public static DataTable GetDivTypeList(SqlConnection dbConn)
        {
            DataTable dtDivType = new DataTable();
            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                command = dbConn.CreateCommand();

                command.CommandText = "SELECT CODEVALUE, CODENAME FROM TB_CODE " +
                                             "WHERE CODEID = '11100'";


                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                reader = command.ExecuteReader();

                dtDivType.Columns.Add("VALUE");
                dtDivType.Columns.Add("NAME");
                DataRow dr;
                while (reader.Read())
                {
                    dr = dtDivType.NewRow();
                    dr["VALUE"] = reader[0].ToString();
                    dr["NAME"] = reader[1].ToString();
                    dtDivType.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:GetOrderList" + ex.ToString());
                return null;
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }
            return dtDivType;
        }


        private void upt_TB_XRF_SEQ(object sender, DoWorkEventArgs e)
        {
            SqlCommand command = null;
            int iReturn = 0;
            string sSql = string.Empty;

            string[] s = (string[])e.Argument;

            string sSmplNo = s[0];
            string sTmb = s[1];
            string sRECHK = s[2];
            string sFlag = s[3];

            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            try
            {
                if (sSmplNo.Length == 0)
                {
                    sSql = string.Format("UPDATE TB_XRF_SEQ SET RECHECK = '{0}' " +
                                         "WHERE RECHECK = '{1}'", sRECHK, sFlag);
                }
                else
                {
                    if (sRECHK == "Y" && sFlag == "Y")
                    {
                        sSql = string.Format("UPDATE TB_XRF_SEQ SET RECHECK = 'Y' " +
                                             "WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", sSmplNo, sTmb);
                    }
                    else
                    {
                        sSql = string.Format("UPDATE TB_XRF_SEQ SET RECHECK = '{0}' " +
                                             "WHERE SMPLNO = '{1}' AND TMBDIV = '{2}' AND RECHECK = '{3}'", sRECHK, sSmplNo, sTmb, sFlag);
                    }
                }
                command = dbConn.CreateCommand();

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command.CommandText = sSql;

                iReturn = command.ExecuteNonQuery();
                WriteLogData("SAMPLE_ID = " + sSmplNo.PadRight(8) + sTmb + ", Update Count: " + iReturn.ToString(), "upt_TB_XRF_SEQ");

            }
            catch (Exception ex)
            {
                //MessageBox.Show("CommonDataBase:GetOrderList" + ex.ToString());
                WriteLogData(ex.ToString(), "upt_TB_XRF_SEQ");
            }
            finally
            {
                WriteLogData(sSql, "upt_TB_XRF_SEQ");
                if (dbConn.State == ConnectionState.Open)
                    dbConn.Close();

                if (command != null) command.Dispose();
                command = null;
            }
        }


        private void ins_TB_XRF_FA_DIRECT(object sender, DoWorkEventArgs e)
        {
            string[] s = (string[])e.Argument;

            string sSmplNo = s[0];
            string sTmbDiv = s[1];
            string sCarve = s[2];
            string sLength = s[3];
            string sSuji = s[4];
            string sProcDiv = s[5];
            string sProc = s[6];
            string sDate = s[7];

            SqlCommand command = null;
            int iReturn = 0;
            string sSql = string.Empty;
            if (sLength.Length == 0)
            {
                sLength = "0";
            }

            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            if (!Found_TB_XRF_FA_DIRECT(sSmplNo, sTmbDiv, sCarve))
            {
                sSql = string.Format("INSERT INTO TB_XRF_FA_DIRECT (SMPLNO, TMBDIV, CARVENUMBER, SMPLLENGTH, SUJI, PROCESSDIV, LOADER, LOADER_DATE) " +
                                           "VALUES('{0}','{1}','{2}',{3},'{4}','{5}','{6}', TO_DATE('{7}','YYYY-MM-DD HH24:MI:SS'))", sSmplNo, sTmbDiv, sCarve, sLength, sSuji, sProcDiv, sProc, sDate);
                try
                {

                    if (dbConn.State == ConnectionState.Closed)
                        dbConn.Open();

                    command = dbConn.CreateCommand();

                    command.CommandText = sSql;
                    iReturn = command.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    //MessageBox.Show("CommonDataBase:insert_TB_XRF_FA_DIRECT" + ex.ToString());
                    WriteLogData(ex.ToString(), "insert_TB_XRF_FA_DIRECT:" + sSql);
                }
                finally
                {
                    if (dbConn.State == ConnectionState.Open)
                        dbConn.Close();
                    if (command != null) command.Dispose();
                    command = null;
                }
            }
        }

        private void upt_TB_XRF_FA_DIRECT(object sender, DoWorkEventArgs e)
        {
            string[] s = (string[])e.Argument;

            string sSmplNo = s[0];
            string sTmbDiv = s[1];
            string sCarve = s[2];
            string sProcDiv = s[3];
            string sProc = s[4];
            string sDate = s[5];

            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            SqlCommand command = null;
            int iReturn = 0;
            string sComSql = string.Empty;
            try
            {

                switch (Convert.ToInt16(sProcDiv))
                {
                    case 2:
                        sComSql = string.Format("UPDATE TB_XRF_FA_DIRECT  SET PROCESSDIV = '{0}', PRCMOVE = '{1}',  PRCMOVE_DATE = TO_DATE('{2}','YYYY-MM-DD HH24:MI:SS') ", sProcDiv, sProc, sDate);
                        break;
                    case 3:
                        sComSql = string.Format("UPDATE TB_XRF_FA_DIRECT  SET PROCESSDIV = '{0}', PRESS = '{1}',  PRESS_DATE = TO_DATE('{2}','YYYY-MM-DD HH24:MI:SS') ", sProcDiv, sProc, sDate);
                        break;
                    case 4:
                        sComSql = string.Format("UPDATE TB_XRF_FA_DIRECT  SET PROCESSDIV = '{0}', CARVE = '{1}',  CARVE_DATE = TO_DATE('{2}','YYYY-MM-DD HH24:MI:SS') ", sProcDiv, sProc, sDate);
                        break;
                    case 5:
                        sComSql = string.Format("UPDATE TB_XRF_FA_DIRECT  SET PROCESSDIV = '{0}', CLEAN = '{1}',  CLEAN_DATE = TO_DATE('{2}','YYYY-MM-DD HH24:MI:SS') ", sProcDiv, sProc, sDate);
                        break;
                    case 6:
                        sComSql = string.Format("UPDATE TB_XRF_FA_DIRECT  SET PROCESSDIV = '{0}', DISCHARGE = '{1}',  DISCHARGE_DATE = TO_DATE('{2}','YYYY-MM-DD HH24:MI:SS') ", sProcDiv, sProc, sDate);
                        break;
                    case 7:
                        sComSql = string.Format("UPDATE TB_XRF_FA_DIRECT  SET PROCESSDIV = '{0}', BUFFER = '{1}',  BUFFER_DATE = TO_DATE('{2}','YYYY-MM-DD HH24:MI:SS') ", sProcDiv, sProc, sDate);
                        break;
                }
                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command = dbConn.CreateCommand();

                string sSql = string.Empty;
                if (sCarve.Length == 0)
                {
                    sCarve = " ";
                    sSql = string.Format(sComSql +
                                                 "WHERE SMPLNO = '{3}' AND TMBDIV = '{4}' AND CARVENUMBER = '{5}'", sProcDiv, sProc, sDate, sSmplNo, sTmbDiv, sCarve);
                }
                else
                {
                    if (Convert.ToInt16(sProcDiv) <= 3)
                    {
                        sSql = string.Format(sComSql + ", CARVENUMBER = '{5}' " +
                                                     "WHERE SMPLNO = '{3}' AND TMBDIV = '{4}' AND CARVENUMBER = 'T'", sProcDiv, sProc, sDate, sSmplNo, sTmbDiv, sCarve);
                    }
                    else
                    {
                        sSql = string.Format(sComSql +
                                                     "WHERE SMPLNO = '{3}' AND TMBDIV = '{4}' AND CARVENUMBER = '{5}'", sProcDiv, sProc, sDate, sSmplNo, sTmbDiv, sCarve);
                    }
                }
                command.CommandText = sSql;
                iReturn = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                //MessageBox.Show("CommonDataBase:insert_TB_XRF_FA_DIRECT" + ex.ToString());
                WriteLogData(ex.ToString(), "Update_TB_XRF_FA_DIRECT " + sComSql);
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }
        }


        /// <summary>
        /// 시편 검사결과를 History Table에 저장한다.
        /// </summary>
        private void ins_TB_XRF_HISTORY(object sender, DoWorkEventArgs e)
        {
            string[] s = (string[])e.Argument;

            string smplNo = s[0];
            string tmb = s[1];
            string wcd = s[2];
            string fb = s[3];
            string Fe = s[4];
            string Zn = s[5];
            string Cr = s[6];
            string P = s[7];

            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            SqlCommand command = null;
            int iReturn = 0;
            string sSql = string.Empty;

            if (wcd == "W" && fb == "F")
            {
                sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFFRONTW, CRFRONTW, EXT1FRONTW, FEFRONTW) " +
                                           "VALUES(XRFHSEQ.NEXTVAL,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            }
            if (wcd == "W" && fb == "B")
            {
                sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFBACKW, CRBACKW, EXT1BACKW, FEBACKW) " +
                                           "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            }

            if (wcd == "C" && fb == "F")
            {
                sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFFRONTC, CRFRONTC, EXT1FRONTC, FEFRONTC) " +
                                           "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            }
            if (wcd == "C" && fb == "B")
            {
                sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFBACKC, CRBACKC, EXT1BACKC, FEBACKC) " +
                                           "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            }

            if (wcd == "D" && fb == "F")
            {
                sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFFRONTD, CRFRONTD, EXT1FRONTD, FEFRONTD) " +
                                           "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            }
            if (wcd == "D" && fb == "B")
            {
                sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFBACKD, CRBACKD, EXT1BACKD, FEBACKD) " +
                                           "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            }

            try
            {

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command = dbConn.CreateCommand();

                command.CommandText = sSql;
                iReturn = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                WriteLogData(ex.ToString(), "ins_TB_XRF_HISTORY :" + sSql);
                //MessageBox.Show("CommonDataBase:ins_TB_XRF_HISTORY" + ex.ToString());
            }
            finally
            {
                WriteLogData(sSql, "ins_TB_XRF_HISTORY");
                if (dbConn.State == ConnectionState.Open)
                    dbConn.Close();
                if (command != null) command.Dispose();
                command = null;
            }
        }

        /// <summary>
        /// 시편 검사결과를 RESULT Table에 저장한다.
        /// </summary>
        private void ins_TB_TEST_RESULT(object sender, DoWorkEventArgs e)
        {


            string[] s = (string[])e.Argument;

            string smplNo = s[0];
            string tmb = s[1];
            string wcd = s[2];
            string fb = s[3];
            string Fe = s[4];
            string Zn = s[5];
            string Cr = s[6];
            string P = s[7];
            string sDate = s[8];


            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            SqlCommand command = null;
            int iReturn = 0;
            string sSql = string.Empty;


            if (wcd == "W")
            {
                if (fb == "F")
                {
                    sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFFRONTW = {0}, CRFRONTW = {1} , RSNFRONTW = {2},  FEFRONTW = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
                                               " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
                }
                else
                {
                    sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFBACKW = {0}, CRBACKW = {1} , RSNBACKW = {2},  FEBACKW = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
                                               " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
                }
            }
            if (wcd == "C")
            {
                if (fb == "F")
                {
                    sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFFRONTC = {0}, CRFRONTC = {1} , RSNFRONTC = {2},  FEFRONTC = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
                                               " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
                }
                else
                {
                    sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFBACKC = {0}, CRBACKC = {1} , RSNBACKC = {2},  FEBACKC = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
                                               " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
                }
            }
            if (wcd == "D")
            {
                if (fb == "F")
                {
                    sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFFRONTD = {0}, CRFRONTD = {1} , RSNFRONTD = {2},  FEFRONTD = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
                                               " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
                }
                else
                {
                    sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFBACKD = {0}, CRBACKD = {1} , RSNBACKD = {2},  FEBACKD = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
                                               " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
                }
            }

            try
            {

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command = dbConn.CreateCommand();

                command.CommandText = sSql;
                iReturn = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                WriteLogData(ex.ToString(), "insert_TB_XRF_HISTORY");
                //MessageBox.Show("CommonDataBase:insert_TB_XRF_HISTORY" + ex.ToString());
            }
            finally
            {
                WriteLogData(sSql, "insert_TB_XRF_HISTORY");
                if (dbConn.State == ConnectionState.Open)
                    dbConn.Close();
                if (command != null) command.Dispose();
                command = null;
            }
        }


        /// <summary>
        /// XRF Application Name을 받아온다.
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="sSmplNo"></param>
        /// <param name="sTmb"></param>
        /// <param name="tblName"></param>
        /// <returns></returns>
        public static string GetProgramName(SqlConnection dbConn, string sSmplNo, string sTmb, string tblName)
        {
            SqlCommand command = null;
            string sReturn = string.Empty;
            object obj = null;

            try
            {
                command = dbConn.CreateCommand();

                string sSql = string.Format("SELECT EXNAME FROM {2} WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", sSmplNo, sTmb, tblName);
                command.CommandText = sSql;


                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                obj = command.ExecuteScalar();
                if (obj == null)
                {
                    sReturn = string.Empty;
                }
                else
                {
                    sReturn = obj.ToString();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:GetProgramName" + ex.ToString());
                return sReturn;
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }

            return sReturn;
        }



        /// <summary>
        /// 수동등록된 작업지시를 임시테이블에 등록한다.
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="sSmplNo"></param>
        /// <param name="sTmb"></param>
        /// <param name="sSUJI"></param>
        /// <param name="sLength"></param>
        /// <param name="sExName"></param>
        /// <returns></returns>
        /// 
        private static bool search_TB_XRF_SEQ_TEMP(SqlConnection dbConn, string sSmplNo, string sTmb)
        {
            SqlCommand command = null;
            bool iReturn = false;
            string sSql = string.Empty;
            object obj = null;

            try
            {
                sSql = string.Format("SELECT SMPLNO FROM TB_XRF_SEQ_TEMP " +
                                         "WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", sSmplNo, sTmb);
                command = dbConn.CreateCommand();

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command.CommandText = sSql;

                obj = command.ExecuteScalar();

            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:insert_TB_XRF_SEQ" + ex.ToString());
                return false;
            }
            finally
            {
                if (obj == null)
                {
                    iReturn = false;
                }
                else
                {
                    iReturn = true;
                }

                if (dbConn.State == ConnectionState.Open)
                    dbConn.Close();

                if (command != null) command.Dispose();
                command = null;
            }

            return iReturn;
        }

        public static int insert_TB_XRF_SEQ_TEMP(SqlConnection dbConn, string sSmplNo, string sTmb, string sSUJI, string sLength, string sExName)
        {
            SqlCommand command = null;
            int iReturn = 0;
            string sSql = string.Empty;

            try
            {
                if (!search_TB_XRF_SEQ_TEMP(dbConn, sSmplNo, sTmb))
                {
                    sSql = string.Format("INSERT INTO TB_XRF_SEQ_TEMP(SMPLNO, TMBDIV, SUJI, SMPLLENGTH, EXNAME, RECHECK) " +
                                             "VALUES('{0}','{1}','{2}','{3}','{4}', 'D')", sSmplNo, sTmb, sSUJI, sLength, sExName);
                }
                else
                {
                    sSql = string.Format("UPDATE TB_XRF_SEQ_TEMP SET SUJI = '{2}', SMPLLENGTH = '{3}', EXNAME = '{4}' " +
                                             "WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", sSmplNo, sTmb, sSUJI, sLength, sExName);
                }

                command = dbConn.CreateCommand();

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command.CommandText = sSql;

                iReturn = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:insert_TB_XRF_SEQ_TEMP" + ex.ToString());
                return -1;
            }
            finally
            {
                if (dbConn.State == ConnectionState.Open)
                    dbConn.Close();

                if (command != null) command.Dispose();
                command = null;
            }

            return iReturn;
        }

        private bool Found_TB_XRF_FA_DIRECT(string sSmplNo, string sTmbDiv, string sCarve)
        {
            SqlCommand command = null;
            bool bReturn = true;
            object iCount = null;

            SqlConnection dbCon = MssqlConnect.Instance.MssqlCnn;
            try
            {

                if (dbCon.State == ConnectionState.Closed)
                    dbCon.Open();

                command = dbCon.CreateCommand();

                string sSql = string.Format("SELECT COUNT(*) FROM TB_XRF_FA_DIRECT WHERE SMPLNO = '{0}' AND TMBDIV = '{1}' AND  CARVENUMBER = '{2}'", sSmplNo, sTmbDiv, sCarve);
                command.CommandText = sSql;
                iCount = command.ExecuteScalar();

                if (Convert.ToInt16(iCount) == 0)
                {
                    bReturn = false;   // record 없음
                }
            }
            catch (Exception ex)
            {
                WriteLogData(ex.ToString(), "Found_TB_XRF_FA_DIRECT");
                return true;
            }
            finally
            {
                if (dbCon.State == ConnectionState.Open)
                    dbCon.Close();
                if (command != null) command.Dispose();
                command = null;
            }

            return bReturn;  // record 존재함.
        }


        private void upt_TB_TEST_DIRECT(object sender, DoWorkEventArgs e)
        {
            string[] s = (string[])e.Argument;

            string sSmplNo = s[0];
            string sTmbDiv = s[1];
            string sStatus = "D";
            string sDate = s[3];

            if (!Found_TB_TEST_DIRECT(sSmplNo, sTmbDiv)) return;

            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            SqlCommand command = null;
            int iReturn = 0;
            string sComSql = string.Empty;

            try
            {

                sComSql = string.Format("UPDATE TB_TEST_DIRECT  SET XRFSTATUS = '{0}', IMPORTDATE = TO_DATE('{1}','YYYY-MM-DD HH24:MI:SS') WHERE SMPLNO = '{2}' AND TMBDIV = '{3}'", sStatus, sDate, sSmplNo, sTmbDiv);

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command = dbConn.CreateCommand();

                command.CommandText = sComSql;
                iReturn = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                WriteLogData(ex.ToString(), "upt_TB_TEST_DIRECT");
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }
        }



        private bool Found_TB_TEST_DIRECT(string smplNO, string tmbDiv)
        {
            SqlCommand command = null;
            object obj = null;
            bool bReturn = false;
            string sSql = string.Empty;

            SqlConnection dbCon = MssqlConnect.Instance.MssqlCnn;

            sSql = string.Format("SELECT SMPLNO FROM TB_TEST_DIRECT WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", smplNO, tmbDiv);
            try
            {

                if (dbCon.State == ConnectionState.Closed)
                    dbCon.Open();

                command = dbCon.CreateCommand();

                command.CommandText = sSql;
                obj = command.ExecuteScalar();

                if (obj == null)
                {
                    bReturn = false;
                }
                else
                {
                    bReturn = true;
                }

            }
            catch (Exception ex)
            {
                WriteLogData(ex.ToString(), "Found_TB_TEST_DIRECT");
                return false;
            }
            finally
            {
                if (dbCon.State == ConnectionState.Open)
                    dbCon.Close();
                if (command != null) command.Dispose();
                command = null;
            }
            return bReturn;
        }




        private void WriteLogData(string sData, string sFLAG)
        {
            try
            {
                string sFilePath = sLogPath + DateTime.Now.ToString("yyyyMMdd_DB") + ".log";
                TextWriter tw = null;

                if (!Directory.Exists(sLogPath))
                {
                    Directory.CreateDirectory(sLogPath);
                }
                tw = new StreamWriter(sFilePath, true);
                tw.WriteLine("[" + sFLAG + "]-------------------------------" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

                tw.WriteLine(sData);
                tw.WriteLine("-------------------------------[" + sFLAG + "]");
                tw.Close();
            }
            catch { }
        }

        #region [ bool - 시편변호에 맞는 검량선(프로그램)명 있는지 반환(CheckAppName) ]
        public static bool CheckAppName(string[] sParams)
        {
            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;
                command = dbConn.CreateCommand();
                string sql = string.Format(
                " SELECT DIRECTR.APPNAME                                                                                    "
                + "   FROM(                                                                                                   "
                + "     SELECT APP.APPNAME                                                                                    "
                + "           , APP.SHTSPEC                                                                                   "
                + "       FROM TB_TEST_DIRECT DIR                                                                             "
                + "           , TB_XRF_APPLICATION_NAME APP                                                                   "
                + "      WHERE DIR.LINENAME = APP.LINENAME(+)                                                                 "
                + "        AND DIR.COILNAME = APP.COILNAME(+)                                                                 "
                + "        AND DIR.AFTCODE = APP.AFTCODE(+)                                                                   "
                + "        AND SMPLNO = '{0}'                                                                                     "
                + "        AND TMBDIV = '{1}'                                                                                     "
                + "                                                                                                           "
                + "        ) DIRECTR                                                                                          "
                + "   WHERE DIRECTR.SHTSPEC = (                                                                               "
                 + "                                    SELECT NVL(XA.SHTSPEC, '--') SHTSPEC            "
                 + "                                       FROM (SELECT XAN.FACTORY                     "
                 + "                                                   ,XAN.LINENAME                    "
                 + "                                                   ,XAN.COILNAME                    "
                 + "                                                   ,CD.CODENAME SHTSPEC             "
                 + "                                                   ,XAN.AFTCODE                     "
                 + "                                                   ,XAN.APPNAME                     "
                 + "                                                   ,XAN.DEFAULTCHECK                "
                 + "                                               FROM TB_XRF_APPLICATION_NAME XAN     "
                 + "                                                   ,TB_CODE CD                      "
                 + "                                               WHERE XAN.SHTSPEC  = CD.CODEVALUE(+) "
                 + "                                                 AND CD.CODEID = '11400') XA        "
                 + "                                           ,TB_TEST_DIRECT TD                       "
                 + "                                     WHERE XA.linename(+) = TD.LINENAME             "
                 + "                                       AND XA.coilname(+) = TD.coilname             "
                 + "                                       AND XA.aftcode(+) = TD.aftcode               "
                 + "                                       AND XA.shtspec(+) = TD.shtspec               "
                 + "                                       AND TD.SMPLNO = '{2}'                            "
                 + "                                       AND TD.TMBDIV = '{3}'                            "
                + "                         )                                                                                 "
                + "     AND DIRECTR.APPNAME = '{4}'                                                                               "
                , sParams[0] //SMPLNO
                , sParams[1] //TMBDIV
                , sParams[0] //SMPLNO
                , sParams[1] //TMBDIV
                , sParams[2] //APPNAME
                );
                command.CommandText = sql;


                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                reader = command.ExecuteReader();


                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        int iRow = 1;
                        while (reader.Read())
                        {
                            if (!string.IsNullOrEmpty(reader.GetValue(0).ToString()))
                            {//시편번호에 맞는 동일한 AppName 있다면
                                return true;
                            }
                            iRow++;
                        }
                        reader = null;

                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:GetOrderList" + ex.ToString());
                return false;
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }

            return false;
        }
        #endregion
        #region [ 검량선(프로그램)명 변경(UpdateAppName) ]
        public static void UpdateAppName(string[] sParams)
        {
            SqlCommand command = null;
            SqlDataReader reader = null;
            string sql = string.Format(
                "UPDATE TB_XRF_SEQ "
                + " SET EXNAME = '{0}' "
                + " WHERE SMPLNO = '{1}' "
                + " AND TMBDIV = '{2}' "
                , sParams[0] //APPNAME
                , sParams[1] //SMPLNO
                , sParams[2] //TMBDIV
                );

            try
            {
                SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;
                command = dbConn.CreateCommand();

                command.CommandText = sql;


                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //WriteLogData(ex.ToString(), "UpdateAppName :" + sql);
                //MessageBox.Show("CommonDataBase:ins_TB_XRF_HISTORY" + ex.ToString());
            }
            finally
            {
            }
        }
        #endregion
        #region [ DataTable - 검량선(프로그램)명 전체 가져온다(GetAppName) ]
        public static DataTable GetAppName(SqlConnection dbConn)
        {
            DataTable dtAppName = new DataTable();
            SqlCommand command = null;
            SqlDataReader reader = null;

            try
            {
                command = dbConn.CreateCommand();

                command.CommandText = "SELECT APPNAME, APPNAME FROM TB_XRF_APPLICATION_NAME ";


                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                reader = command.ExecuteReader();

                dtAppName.Columns.Add("VALUE");
                dtAppName.Columns.Add("NAME");
                DataRow dr;
                while (reader.Read())
                {
                    dr = dtAppName.NewRow();
                    dr["VALUE"] = reader[0].ToString();
                    dr["NAME"] = reader[1].ToString();
                    dtAppName.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:GetAppName" + ex.ToString());
                return null;
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }
            return dtAppName;
        }
        #endregion
        #region [ DataTable - XRF Application Element 정보 조회(GetXRFApplicationElement) ]
        public static DataTable GetXRFApplicationElement(string[] sParams)
        {
            SqlCommand command = null;
            SqlDataReader reader = null;
            DataTable dtTemp = new DataTable();
            try
            {
                SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;
                command = dbConn.CreateCommand();
                string sql = string.Format(
                "SELECT DIRECTR.LINENAME"
                + "      ,DIRECTR.COILNAME"
                + "      ,DIRECTR.SHTSPEC"
                + "      ,DIRECTR.AFTCODE"
                + "      ,DIRECTR.ELEMENT"
                + "      ,DIRECTR.COMPUTEMODIFY                                                                         "
                + "      ,DIRECTR.COLNAME                                                                               "
                + "      ,DIRECTR.APPNAME                                                                               "
                + " FROM(                                                                                               "
                + "      SELECT '3000'                                                                                  "
                + "            , DIRECT.LINENAME                                                                        "
                + "            , DIRECT.COILNAME                                                                        "
                + "            , APP.SHTSPEC                                                                            "
                + "            , DIRECT.AFTCODE                                                                         "
                + "            , APP.ELEMENT                                                                            "
                + "            , APP.APPNAME                                                                            "
                + "            , APP.COMPUTEMODIFY                                                                      "
                + "            , APP.COLNAME                                                                            "
                + "        FROM TB_TEST_DIRECT DIRECT                                                                   "
                + "            , TB_XRF_APPLICATION_ELEMENT APP                                                         "
                + "       WHERE DIRECT.LINENAME = APP.LINENAME(+)                                                       "
                + "         AND DIRECT.COILNAME = APP.COILNAME(+)                                                       "
                + "         AND DIRECT.AFTCODE = APP.AFTCODE(+)                                                         "
                + "         AND SMPLNO = '{0}'                                                                        "
                + "         AND TMBDIV = '{1}'                                                                        "
                + "         AND APP.APPNAME = '{2}'                                                                        "
                + "                                                                                                     "
                + "         ) DIRECTR                                                                                   "
                + " WHERE DIRECTR.SHTSPEC = (                                                                           "
                + "                            SELECT NVL(XA.SHTSPEC, '--') SHTSPEC             "
                + "                               FROM (SELECT XAN.FACTORY                      "
                + "                                           ,XAN.LINENAME                     "
                + "                                           ,XAN.COILNAME                     "
                + "                                           ,CD.CODENAME SHTSPEC             "
                + "                                           ,XAN.AFTCODE                     "
                + "                                             ,XAN.APPNAME                   "
                + "                                             ,XAN.DEFAULTCHECK              "
                + "                                         FROM TB_XRF_APPLICATION_NAME XAN   "
                + "                                             ,TB_CODE CD                    "
                + "                                         WHERE XAN.SHTSPEC  = CD.CODEVALUE(+)"
                + "                                           AND CD.CODEID = '11400') XA       "
                + "                                     ,TB_TEST_DIRECT TD                      "
                + "                               WHERE XA.linename(+) = TD.LINENAME            "
                + "                                 AND XA.coilname(+) = TD.coilname            "
                + "                                 AND XA.aftcode(+) = TD.aftcode              "
                + "                                 AND XA.shtspec(+) = TD.shtspec              "
                + "                                 AND TD.SMPLNO = '{3}'                           "
                + "                                 AND TD.TMBDIV = '{4}'                           "
                + "                         )                                                           "
                , sParams[0] //SMPLNO
                , sParams[1] //TMBDIV
                , sParams[2] //APPNAME
                , sParams[0] //SMPLNO
                , sParams[1] //TMBDIV
                );
                command.CommandText = sql;


                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                reader = command.ExecuteReader();


                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        int iRow = 1;

                        dtTemp.Columns.Add("LINENAME");
                        dtTemp.Columns.Add("COILNAME");
                        dtTemp.Columns.Add("SHTSPEC");
                        dtTemp.Columns.Add("AFTCODE");
                        dtTemp.Columns.Add("ELEMENT");
                        dtTemp.Columns.Add("COMPUTEMODIFY");
                        dtTemp.Columns.Add("COLNAME");
                        dtTemp.Columns.Add("APPNAME");
                        DataRow dr;
                        while (reader.Read())
                        {
                            dr = dtTemp.NewRow();
                            dr["LINENAME"] = reader[0].ToString();
                            dr["COILNAME"] = reader[1].ToString();
                            dr["SHTSPEC"] = reader[2].ToString();
                            dr["AFTCODE"] = reader[3].ToString();
                            dr["ELEMENT"] = reader[4].ToString();
                            dr["COMPUTEMODIFY"] = reader[5].ToString();
                            dr["COLNAME"] = reader[6].ToString();
                            dr["APPNAME"] = reader[7].ToString();
                            dtTemp.Rows.Add(dr);
                        }

                        reader = null;

                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:GetOrderList" + ex.ToString());
                return null;
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }

            return dtTemp;
        }
        #endregion
        #region [ DataTable - XRF Application Detail 정보 조회() ]
        public static DataTable GetXRFApplicationDetail(string[] sParams)
        {
            SqlCommand command = null;
            SqlDataReader reader = null;
            DataTable dtTemp = new DataTable();
            try
            {
                SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;
                command = dbConn.CreateCommand();
                string sql = string.Format(
                "   SELECT *                          "
                + "   FROM TB_XRF_APPLICATION_DETAIL  "
                + "  WHERE LINENAME = '{0}'       "
                + "    AND COILNAME = '{1}'       "
                + "    AND SHTSPEC = '{2}'         "
                + "    AND AFTCODE = '{3}'           "
                + "    AND UPPER(ELEMENT) = '{4}'           "
                + "    AND APPNAME = '{5}'           "
                + "    ORDER BY DETAILSEQ               "
                , sParams[0] //라인
                , sParams[1] //품명
                , sParams[2] //규격
                , sParams[3] //후처리
                , sParams[4] //원소
                , sParams[5] //검량선명
                );
                command.CommandText = sql;


                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                reader = command.ExecuteReader();


                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        int iRow = 1;

                        dtTemp.Columns.Add("LINENAME");
                        dtTemp.Columns.Add("COILNAME");
                        dtTemp.Columns.Add("SHTSPEC");
                        dtTemp.Columns.Add("AFTCODE");
                        dtTemp.Columns.Add("ELEMENT");
                        dtTemp.Columns.Add("COMPUTEMODIFY");
                        dtTemp.Columns.Add("COLNAME");
                        dtTemp.Columns.Add("APPNAME");
                        DataRow dr;
                        while (reader.Read())
                        {
                            dr = dtTemp.NewRow();
                            dr["LINENAME"] = reader[0].ToString();
                            dr["COILNAME"] = reader[1].ToString();
                            dr["SHTSPEC"] = reader[2].ToString();
                            dr["AFTCODE"] = reader[3].ToString();
                            dr["ELEMENT"] = reader[4].ToString();
                            dr["COMPUTEMODIFY"] = reader[5].ToString();
                            dr["COLNAME"] = reader[6].ToString();
                            dr["APPNAME"] = reader[7].ToString();
                            dtTemp.Rows.Add(dr);
                        }

                        reader = null;

                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("CommonDataBase:GetOrderList" + ex.ToString());
                return null;
            }
            finally
            {
                if (command != null) command.Dispose();
                command = null;
            }

            return dtTemp;
        }
        #endregion
        #region [ 히스토리, 값, 결과처리 저장 ]
        public void Execute_SaveData(DataTable dtTemp, string txt)
        {
            switch (txt)
            {
                case "History":
                    bgW.DoWork += new DoWorkEventHandler(InsertHistory);
                    bgW.RunWorkerAsync(dtTemp);
                    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(upt_TB_XRF_SEQ_RunWorkerCompleted);
                    break;
                case "Result":
                    bgW.DoWork += new DoWorkEventHandler(UpdateResult);
                    bgW.RunWorkerAsync(dtTemp);
                    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(upt_TB_XRF_SEQ_RunWorkerCompleted);
                    break;
                    //case "End":
                    //    bgW.DoWork += new DoWorkEventHandler(UpdateXRFSEQ);
                    //    bgW.RunWorkerAsync(dtTemp);
                    //    bgW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(upt_TB_XRF_SEQ_RunWorkerCompleted);
                    //    break;

            }
        }
        #endregion
        #region [ 히스토리 저장(InsertHistory) ]
        private void InsertHistory(object sender, DoWorkEventArgs e)
        {

            DataTable dtElement = (DataTable)e.Argument;
            string SQL = string.Empty;
            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            SqlCommand command = null;
            #region [ 히스토리 저장 ]
            ////////////// 히스토리 저장 /////////////////////////////////////////////////////////////
            for (int iRow = 0; iRow < dtElement.Rows.Count; iRow++)
            {
                DataRow drTemp = dtElement.Rows[iRow];
                //if (!string.IsNullOrEmpty(drTemp["ELEMENTVALUE"].ToString()))//변경된값이 있다면 담는다
                {
                    SQL = string.Empty;
                    SQL = string.Format(
                                "INSERT INTO TB_XRF_AUTO_HISTORY "
                                + " (XRFHSEQ, SMPLNO, TMBDIV, ELEMENTNAME, ELEMENTVALUE, FBDIV, WCDDIV, XRFDATE, EXNAME, RESOURCEVALUE)"
                                + "VALUES(XRFHASEQ.NEXTVAL, '{0}', '{1}' ,'{2}','{3}', '{4}' ,'{5}',TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS'), '{7}' ,'{8}') "
                                , drTemp["SMPLNO"].ToString() //시편번호
                                , drTemp["TMBDIV"].ToString() //TMBDIV
                                , drTemp["ELEMENT"].ToString() //원소
                                , string.IsNullOrEmpty(drTemp["ELEMENTVALUE"].ToString()) ? "0" : drTemp["ELEMENTVALUE"].ToString() //변경된값 [DB에 등록된 원소가 아닌경우 0으로 저장]
                                , drTemp["FRONTBACK"].ToString().Substring(0, 1) //전면 이면 [F, B]
                                , drTemp["WCD"].ToString() //WCD
                                , drTemp["DATE"].ToString() //날짜
                                , drTemp["APPNAME"].ToString() //검량선명
                                , drTemp["BEFOREELEMENTVALUE"].ToString() //기계에서 입력받은값
                                );
                    try
                    {
                        //string strValue = "Common히스토리 원소 테이블 로우 갯수 : " + dtElement.Rows.Count;
                        //string strSql = "Common히스토리 SQL : " + SQL;
                        //WriteLogDataTemp(strValue, "원소히스토리");
                        //WriteLogDataTemp(strSql, "원소의 SQL 히스토리");
                        if (dbConn.State == ConnectionState.Closed)
                            dbConn.Open();

                        command = dbConn.CreateCommand();

                        command.CommandText = SQL;
                        command.ExecuteNonQuery();

                    }
                    catch (Exception ex)
                    {
                        WriteLogData(ex.ToString(), "InsertHistory :" + SQL);
                        //MessageBox.Show("CommonDataBase:ins_TB_XRF_HISTORY" + ex.ToString());
                    }
                    finally
                    {
                        WriteLogData(SQL, "InsertHistory");
                        if (dbConn.State == ConnectionState.Open)
                            dbConn.Close();
                        if (command != null) command.Dispose();
                        command = null;
                    }

                }

            }
            #endregion//히스토리 저장
            #region [ 입력받은값 저장 ]
            ///////////// 입력받은값 저장 ///////////////////////////////////////////////////////////////////
            SQL = string.Empty;//쿼리문
            string strFront = "UPDATE TB_TEST_RESULT SET ";
            string strEnd = string.Format(" WHERE SMPLNO = '{0}'", dtElement.Rows[0]["SMPLNO"]);
            if (dtElement.Rows[0]["TMBDIV"].ToString().Equals("B"))
                strEnd += string.Format(" AND TMBDIV = '{0}'", dtElement.Rows[0]["TMBDIV"]);

            for (int iRow = 0; iRow < dtElement.Rows.Count; iRow++)
            {//원소 내용 추가
                DataRow drTemp = dtElement.Rows[iRow];
                if (!string.IsNullOrEmpty(drTemp["ELEMENTVALUE"].ToString()))//변경된값이 있다면 담는다
                    strFront += " " + drTemp["COLNAME"] + drTemp["FRONTBACK"] + drTemp["WCD"] + " = " + drTemp["ELEMENTVALUE"] + " ,";


            }
            SQL = strFront.Remove(strFront.Length - 1, 1) + strEnd;

            try
            {
                //string strValue = "Common히스토리 원소 테이블 로우 갯수 : " + dtElement.Rows.Count;
                //string strSql = "Common히스토리 SQL : " + SQL;
                //WriteLogDataTemp(strValue, "원소히스토리");
                //WriteLogDataTemp(strSql, "원소의 SQL 히스토리");
                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command = dbConn.CreateCommand();

                command.CommandText = SQL;
                command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                WriteLogData(ex.ToString(), "UpdateResult :" + SQL);
                //MessageBox.Show("CommonDataBase:ins_TB_XRF_HISTORY" + ex.ToString());
            }
            finally
            {
                WriteLogData(SQL, "UpdateResult");
                if (dbConn.State == ConnectionState.Open)
                    dbConn.Close();
                if (command != null) command.Dispose();
                command = null;
            }
            #endregion//기계에서 입력받은값 저장
            #region [ 평균, 총합 값 저장 ]
            ////////////// Avg(평균) 처리 ///////////////////////////////////////////////////////
            strEnd = string.Format(" WHERE SMPLNO = '{0}'", dtElement.Rows[0]["SMPLNO"]);
            if (dtElement.Rows[0]["TMBDIV"].ToString().Equals("B"))
                strEnd += string.Format(" AND TMBDIV = '{0}'", dtElement.Rows[0]["TMBDIV"]);
            DataTable dtResult = SelectXrfValue(dtElement);//결과값 가져옴 WCD
            if (dtResult != null)
            {
                DataRow drResult = dtResult.Rows[0];
                for (int iRow = 0; iRow < dtElement.Rows.Count; iRow++)
                {
                    DataRow drTemp = dtElement.Rows[iRow];

                    if (!string.IsNullOrEmpty(drTemp["ELEMENTVALUE"].ToString()))
                    {
                        string strSqlAvg = string.Empty;
                        strSqlAvg = " UPDATE TB_TEST_RESULT SET ";
                        int iCount = 0;//나눗셈 카운트
                        string strW = "" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "W";//컬렴명 W
                        string strC = "" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "C";//컬렴명 C
                        string strD = "" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "D";//컬렴명 D
                        if (!drResult[strW].ToString().Equals("0"))
                        {//W값이 있을경우
                            ++iCount;
                        }
                        if (!drResult[strC].ToString().Equals("0"))
                        {//C값이 있을경우
                            ++iCount;
                        }
                        if (!drResult[strD].ToString().Equals("0"))
                        {//D값이 있을경우
                            ++iCount;
                        }

                        if (iCount > 0)
                        {//값이 하나라도 있을경우
                            strSqlAvg += " " + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "A"
                                + " = (";
                            if (!drResult[strW].ToString().Equals("0"))
                            {
                                strSqlAvg += strW + " +";
                            }
                            if (!drResult[strC].ToString().Equals("0"))
                            {
                                strSqlAvg += strC + " +";
                            }
                            if (!drResult[strD].ToString().Equals("0"))
                            {
                                strSqlAvg += strD + " +";
                            }
                            strSqlAvg = strSqlAvg.Remove(strSqlAvg.Length - 1, 1)
                            + ") / " + iCount + strEnd;
                            ;

                            try
                            {
                                //string strValue = "Common히스토리 원소 테이블 로우 갯수 : " + dtElement.Rows.Count;
                                //string strSql = "Common히스토리 SQL : " + SQL;
                                //WriteLogDataTemp(strValue, "원소히스토리");
                                //WriteLogDataTemp(strSql, "원소의 SQL 히스토리");
                                if (dbConn.State == ConnectionState.Closed)
                                    dbConn.Open();

                                command = dbConn.CreateCommand();
                                //평균처리
                                command.CommandText = strSqlAvg;
                                command.ExecuteNonQuery();



                            }
                            catch (Exception ex)
                            {
                                WriteLogData(ex.ToString(), "UpdateAvg :" + strSqlAvg);
                                //MessageBox.Show("CommonDataBase:ins_TB_XRF_HISTORY" + ex.ToString());
                            }
                            finally
                            {
                                WriteLogData(strSqlAvg, "UpdateAvg");
                                if (dbConn.State == ConnectionState.Open)
                                    dbConn.Close();
                                if (command != null) command.Dispose();
                                command = null;
                            }

                            //////////// 총합 계산 ////////////////////////////////////////////////////
                            string strAmt = string.Empty;
                            if (dtElement.Rows[iRow]["COLNAME"].ToString().Equals("XRF"))
                            {
                                strAmt = "UPDATE TB_TEST_RESULT SET " + dtElement.Rows[iRow]["COLNAME"].ToString() + "AMOUNT = NVL(" + dtElement.Rows[iRow]["COLNAME"] + "FRONT" + "A, 0) "
                                                                                                                             + " + NVL(" + dtElement.Rows[iRow]["COLNAME"] + "BACK" + "A, 0)"
                                         + " WHERE SMPLNO = '" + dtElement.Rows[iRow]["SMPLNO"] + "'";
                                if (dtElement.Rows[iRow]["TMBDIV"].ToString().Equals("B"))
                                    strAmt += " AND TMBDIV = '" + dtElement.Rows[iRow]["TMBDIV"] + "'";
                            }
                            //XRF일경우 총합 계산
                            if (!string.IsNullOrEmpty(strAmt))
                            {
                                try
                                {
                                    //string strValue = "Common히스토리 원소 테이블 로우 갯수 : " + dtElement.Rows.Count;
                                    //string strSql = "Common히스토리 SQL : " + SQL;
                                    //WriteLogDataTemp(strValue, "원소히스토리");
                                    //WriteLogDataTemp(strSql, "원소의 SQL 히스토리");
                                    if (dbConn.State == ConnectionState.Closed)
                                        dbConn.Open();

                                    command = dbConn.CreateCommand();

                                    command.CommandText = strAmt;
                                    command.ExecuteNonQuery();




                                }
                                catch (Exception ex)
                                {
                                    WriteLogData(ex.ToString(), "UpdateAmount :" + strAmt);
                                    //MessageBox.Show("CommonDataBase:ins_TB_XRF_HISTORY" + ex.ToString());
                                }
                                finally
                                {
                                    WriteLogData(strAmt, "UpdateAmount");
                                    if (dbConn.State == ConnectionState.Open)
                                        dbConn.Close();
                                    if (command != null) command.Dispose();
                                    command = null;
                                }
                            }

                        }
                    }
                }
            }
            #endregion //평균값 저장

            #region [ 정건 1차 ]
            /////////////////////////// 내가 만든 1차 ///////////////////////////////////////////////////////////////
            //string SQLAfter = string.Empty;//쿼리문
            //string SQLBefore = string.Empty;//쿼리문
            //string strFrontAfter = "INSERT INTO TB_XRF_HISTORY(XRFHSEQ, SMPLNO, TMBDIV";
            //string strEndAfter = " ) VALUE(XRFHSEQ.NEXTVAL, '" + dtElement.Rows[0]["SMPLNO"] + "', '" + dtElement.Rows[0]["TMBDIV"] + "'";
            //string strFrontBefore = "INSERT INTO TB_XRF_HISTORY(XRFHSEQ, SMPLNO, TMBDIV";
            //string strEndBefore = " ) VALUE(XRFHSEQ.NEXTVAL, '" + dtElement.Rows[0]["SMPLNO"] + "', '" + dtElement.Rows[0]["TMBDIV"] + "'";
            //for (int iRow = 1; iRow < dtElement.Rows.Count; iRow++)
            //{//원소 내용 추가
            //    DataRow drTemp = dtElement.Rows[iRow];
            //    strFrontAfter += ", " + drTemp["COLNAME"] + drTemp["FRONTBACK"] + drTemp["WCD"];
            //    strEndAfter += ", '" + drTemp["ELEMENTVALUE"]+ "'";

            //    strFrontBefore += ", " + drTemp["COLNAME"] + drTemp["FRONTBACK"] + drTemp["WCD"];
            //    strEndBefore += ", '" + drTemp["BEFOREELEMENTVALUE"] + "'";
            //}

            //SQLBefore = strFrontBefore + strEndBefore + " )";
            //SQLAfter = strFrontAfter + strEndAfter + " )";
            ///////////////////////// 내가 만든 1차 ///////////////////////////////////////////////////////////////
            #endregion
            #region [ 원본 ]
            //string[] s = (string[])e.Argument;

            //string smplNo = s[0];
            //string tmb = s[1];
            //string wcd = s[2];
            //string fb = s[3];
            //string Fe = s[4];
            //string Zn = s[5];
            //string Cr = s[6];
            //string P = s[7];


            //int iReturn = 0;

            //SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            //SqlCommand command = null;
            //string sSql = string.Empty;

            //if (wcd == "W" && fb == "F")
            //{
            //    sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFFRONTW, CRFRONTW, EXT1FRONTW, FEFRONTW) " +
            //                               "VALUES(XRFHSEQ.NEXTVAL,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            //}
            //if (wcd == "W" && fb == "B")
            //{
            //    sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFBACKW, CRBACKW, EXT1BACKW, FEBACKW) " +
            //                               "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            //}

            //if (wcd == "C" && fb == "F")
            //{
            //    sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFFRONTC, CRFRONTC, EXT1FRONTC, FEFRONTC) " +
            //                               "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            //}
            //if (wcd == "C" && fb == "B")
            //{
            //    sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFBACKC, CRBACKC, EXT1BACKC, FEBACKC) " +
            //                               "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            //}

            //if (wcd == "D" && fb == "F")
            //{
            //    sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFFRONTD, CRFRONTD, EXT1FRONTD, FEFRONTD) " +
            //                               "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            //}
            //if (wcd == "D" && fb == "B")
            //{
            //    sSql = string.Format("INSERT INTO TB_XRF_HISTORY (XRFHSEQ, SMPLNO, TMBDIV, XRFBACKD, CRBACKD, EXT1BACKD, FEBACKD) " +
            //                               "VALUES(XRFHSEQ.Nextval,'{0}','{1}',{2},{3},{4},{5})", smplNo, tmb, Zn, Cr, P, Fe);
            //}

            //try
            //{

            //    if (dbConn.State == ConnectionState.Closed)
            //        dbConn.Open();

            //    command = dbConn.CreateCommand();

            //    command.CommandText = SQLBefore;
            //    iReturn = command.ExecuteNonQuery();

            //    command.CommandText = SQLAfter;
            //    iReturn = command.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{
            //    WriteLogData(ex.ToString(), "InsertHistory :" + SQLBefore);
            //    WriteLogData(ex.ToString(), "InsertHistory :" + SQLAfter);
            //    //MessageBox.Show("CommonDataBase:ins_TB_XRF_HISTORY" + ex.ToString());
            //}
            //finally
            //{
            //    WriteLogData(SQLAfter, "InsertHistory");
            //    if (dbConn.State == ConnectionState.Open)
            //        dbConn.Close();
            //    if (command != null) command.Dispose();
            //    command = null;
            //}
            #endregion
        }
        #endregion
        #region [ WCD값 가져오기 ]
        public DataTable SelectXrfValue(DataTable dtElement)
        {
            List<string> list = new List<string>();
            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;
            SqlCommand command = null;
            SqlDataReader reader = null;
            DataTable dtTemp = new DataTable();
            StringBuilder strSql = new StringBuilder();
            DataTable dt = null;
            string Sql = @"SELECT ";
            string strEnd = string.Format(" FROM TB_TEST_RESULT  WHERE SMPLNO = '{0}' AND TMBDIV = '{1}' ", dtElement.Rows[0]["SMPLNO"], dtElement.Rows[0]["TMBDIV"]);
            if (dtElement != null && dtElement.Rows.Count > 0)
            {
                bool bolCheck = false;
                for (int iRow = 0; iRow < dtElement.Rows.Count; iRow++)
                {
                    if (!string.IsNullOrEmpty(dtElement.Rows[iRow]["COLNAME"].ToString()))
                    {//저장컬럼명이 있을때
                        Sql += "NVL(" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "W, 0) " + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "W, "
                             + "NVL(" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "C, 0) " + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "C, "
                             + "NVL(" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "D, 0) " + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "D ,";

                        //컬럼명 저장
                        list.Add("" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "W");
                        list.Add("" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "C");
                        list.Add("" + dtElement.Rows[iRow]["COLNAME"] + dtElement.Rows[iRow]["FRONTBACK"] + "D");
                        bolCheck = true;
                    }
                }
                if (bolCheck)
                {//저장컬럼명이 있는 로우가 있을때
                    Sql = Sql.Remove(Sql.Length - 1, 1) + strEnd;





                    try
                    {
                        //string strValue = "Common히스토리 원소 테이블 로우 갯수 : " + dtElement.Rows.Count;
                        //string strSql = "Common히스토리 SQL : " + SQL;
                        //WriteLogDataTemp(strValue, "원소히스토리");
                        //WriteLogDataTemp(strSql, "원소의 SQL 히스토리");
                        if (dbConn.State == ConnectionState.Closed)
                            dbConn.Open();

                        command = dbConn.CreateCommand();

                        command.CommandText = Sql;
                        reader = command.ExecuteReader();


                        if (reader != null)
                        {
                            if (reader.HasRows)
                            {
                                int iRow = 1;
                                for (int iCnt = 0; iCnt < list.Count; iCnt++)
                                {//컬럼명 저장
                                    dtTemp.Columns.Add(list[iCnt].ToString());
                                }
                                DataRow dr;
                                while (reader.Read())
                                {
                                    dr = dtTemp.NewRow();
                                    for (int iCnt = 0; iCnt < list.Count; iCnt++)
                                    {//컬럼에 데이터 저장
                                        dr[iCnt] = reader[iCnt].ToString();
                                    }

                                    dtTemp.Rows.Add(dr);
                                }

                                reader = null;
                                dt = dtTemp;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        WriteLogData(ex.ToString(), "평균 계산 :" + Sql);
                        //MessageBox.Show("CommonDataBase:ins_TB_XRF_HISTORY" + ex.ToString());
                    }
                    finally
                    {
                        WriteLogData(Sql, "평균 계산");
                        if (dbConn.State == ConnectionState.Open)
                            dbConn.Close();
                        if (command != null) command.Dispose();
                        command = null;
                    }
                }
            }
            return dt;
        }
        #endregion
        #region [ 값 저장(UpdateResult) ]
        private void UpdateResult(object sender, DoWorkEventArgs e)
        {

            DataTable dtElement = (DataTable)e.Argument;
            string SQL = string.Empty;//쿼리문
            string strFront = "UPDATE TB_TEST_RESULT SET ";
            string strEnd = string.Format(" WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", dtElement.Rows[0]["SMPLNO"]
                                                                                    , dtElement.Rows[0]["TMBDIV"]
                                                                                    );
            for (int iRow = 0; iRow < dtElement.Rows.Count; iRow++)
            {//원소 내용 추가
                DataRow drTemp = dtElement.Rows[iRow];
                if (!string.IsNullOrEmpty(drTemp["ELEMENTVALUE"].ToString()))//변경된값이 있다면 담는다
                    strFront += " " + drTemp["COLNAME"] + drTemp["FRONTBACK"] + drTemp["WCD"] + " = " + drTemp["ELEMENTVALUE"] + " ,";
            }
            SQL = strFront.Remove(strFront.Length - 1, 1) + strEnd;

            string strValue = "Common값저장 원소 테이블 로우 갯수 : " + dtElement.Rows.Count;
            string strSql = "Common값저장 SQL : " + SQL;
            WriteLogDataTemp(strValue, "값저장 로우갯수");
            WriteLogDataTemp(strSql, "값저장 SQL");
            //string[] s = (string[])e.Argument;

            //string smplNo = s[0];
            //string tmb = s[1];
            //string wcd = s[2];
            //string fb = s[3];
            //string Fe = s[4];
            //string Zn = s[5];
            //string Cr = s[6];
            //string P = s[7];
            //string sDate = s[8];


            SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

            SqlCommand command = null;
            int iReturn = 0;
            string sSql = string.Empty;


            //if (wcd == "W")
            //{
            //    if (fb == "F")
            //    {
            //        sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFFRONTW = {0}, CRFRONTW = {1} , RSNFRONTW = {2},  FEFRONTW = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
            //                                   " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
            //    }
            //    else
            //    {
            //        sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFBACKW = {0}, CRBACKW = {1} , RSNBACKW = {2},  FEBACKW = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
            //                                   " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
            //    }
            //}
            //if (wcd == "C")
            //{
            //    if (fb == "F")
            //    {
            //        sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFFRONTC = {0}, CRFRONTC = {1} , RSNFRONTC = {2},  FEFRONTC = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
            //                                   " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
            //    }
            //    else
            //    {
            //        sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFBACKC = {0}, CRBACKC = {1} , RSNBACKC = {2},  FEBACKC = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
            //                                   " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
            //    }
            //}
            //if (wcd == "D")
            //{
            //    if (fb == "F")
            //    {
            //        sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFFRONTD = {0}, CRFRONTD = {1} , RSNFRONTD = {2},  FEFRONTD = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
            //                                   " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
            //    }
            //    else
            //    {
            //        sSql = string.Format("UPDATE TB_TEST_RESULT SET  XRFBACKD = {0}, CRBACKD = {1} , RSNBACKD = {2},  FEBACKD = {3}, XRFTESTDATE = TO_DATE('{6}','YYYY-MM-DD HH24:MI:SS') " +
            //                                   " WHERE SMPLNO = '{4}' AND TMBDIV = '{5}'", Zn, Cr, P, Fe, smplNo, tmb, sDate);
            //    }
            //}

            try
            {

                if (dbConn.State == ConnectionState.Closed)
                    dbConn.Open();

                command = dbConn.CreateCommand();

                command.CommandText = SQL;
                iReturn = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                WriteLogData(ex.ToString(), "UpdateResult");
                //MessageBox.Show("CommonDataBase:insert_TB_XRF_HISTORY" + ex.ToString());
            }
            finally
            {
                WriteLogData(SQL, "UpdateResult");
                if (dbConn.State == ConnectionState.Open)
                    dbConn.Close();
                if (command != null) command.Dispose();
                command = null;
            }
        }
        #endregion
        //#region [ 결과 저장() ]
        //private void UpdateXRFSEQ(object sender, DoWorkEventArgs e)
        //{
        //    SqlCommand command = null;
        //    int iReturn = 0;
        //    string sSql = string.Empty;

        //    string[] s = (string[])e.Argument;

        //    string sSmplNo = s[0];
        //    string sTmb = s[1];
        //    string sRECHK = s[2];
        //    string sFlag = s[3];

        //    SqlConnection dbConn = MssqlConnect.Instance.MssqlCnn;

        //    try
        //    {
        //        if (sSmplNo.Length == 0)
        //        {
        //            sSql = string.Format("UPDATE TB_XRF_SEQ SET RECHECK = '{0}' " +
        //                                 "WHERE RECHECK = '{1}'", sRECHK, sFlag);
        //        }
        //        else
        //        {
        //            if (sRECHK == "Y" && sFlag == "Y")
        //            {
        //                sSql = string.Format("UPDATE TB_XRF_SEQ SET RECHECK = 'Y' " +
        //                                     "WHERE SMPLNO = '{0}' AND TMBDIV = '{1}'", sSmplNo, sTmb);
        //            }
        //            else
        //            {
        //                sSql = string.Format("UPDATE TB_XRF_SEQ SET RECHECK = '{0}' " +
        //                                     "WHERE SMPLNO = '{1}' AND TMBDIV = '{2}' AND RECHECK = '{3}'", sRECHK, sSmplNo, sTmb, sFlag);
        //            }
        //        }
        //        command = dbConn.CreateCommand();

        //        if (dbConn.State == ConnectionState.Closed)
        //            dbConn.Open();

        //        command.CommandText = sSql;

        //        iReturn = command.ExecuteNonQuery();
        //        WriteLogData("SAMPLE_ID = " + sSmplNo.PadRight(8) + sTmb + ", Update Count: " + iReturn.ToString(), "upt_TB_XRF_SEQ");

        //    }
        //    catch (Exception ex)
        //    {
        //        //MessageBox.Show("CommonDataBase:GetOrderList" + ex.ToString());
        //        WriteLogData(ex.ToString(), "upt_TB_XRF_SEQ");
        //    }
        //    finally
        //    {
        //        WriteLogData(sSql, "upt_TB_XRF_SEQ");
        //        if (dbConn.State == ConnectionState.Open)
        //            dbConn.Close();

        //        if (command != null) command.Dispose();
        //        command = null;
        //    }
        //}
        //#endregion
        private void WriteLogDataTemp(string sData, string sFLAG)
        {
            try
            {
                string sFilePath = sLogPath + DateTime.Now.ToString("yyyyMMdd_DB") + "Temp" + ".log";
                TextWriter tw = null;

                if (!Directory.Exists(sLogPath))
                {
                    Directory.CreateDirectory(sLogPath);
                }
                tw = new StreamWriter(sFilePath, true);
                tw.WriteLine("[" + sFLAG + "]-------------------------------" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

                tw.WriteLine(sData);
                tw.WriteLine("-------------------------------[" + sFLAG + "]");
                tw.Close();
            }
            catch { }
        }
    }
}
