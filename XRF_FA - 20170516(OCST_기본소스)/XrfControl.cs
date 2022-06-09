using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Text;
using System.IO;

namespace XRF_FA
{
    public class XRF_COMMAND
    {
        #region [ 상태표시 변수 ]
        public static bool XFR_FLAG_LIST_OPEN = false;                  // XRF LIST OPEN여부  
        public static bool XFR_FLAG_LIST_START = false;                 // XRF LIST START 여부
        public static bool XFR_FLAG_LIST_STOP = false;                  // XRF LIST STOP 여부
        public static bool XRF_TEMP_FLAG = false;                       // 한번만 사용하는 플래그
        public static bool XRF_FRONT_FIRST_RUN = false;                 // 이 플래그가 false일때 캡이 열리면 전면 투입신호를 보내고 플래그를 true로 바꾼다.
        public static string XRF_LIST_REMOVE_SAMPLE_ID = string.Empty;
        /// <summary>
        /// 자동운영모드 구분(true:자동모드, false:자동모드아님)
        /// </summary>
        public static bool XRF_AUTO = false;                            // 자동사용여부 플래그
        public static bool XRF_AUTO_X2 = false;                         // 자동사용여부 플래그 for BRUKER
        public static bool UNLOAD_STATUS = false;                       // UNLOAD 상태 체크 투입불가(시험끝나고 이면투입완료 및 배출완료시까지)
        #endregion

        #region [ 생성자 ]
        public XRF_COMMAND()
        {
        }
        #endregion

        #region [ PANALYTICAL 명령 ]
        /// <summary>
        /// XRF의 현재상태를 확인한다
        /// @SYSTEM=remote로 return 되어야 한다.
        /// </summary>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_SYSTEM_STATUS()
        {
            return "@STATUS_REQUEST@SYSTEM@END";
        }

        /// <summary>
        /// XRF에 LIST가 있는지 확인한다.
        /// </summary>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_LIST_REQ()
        {
            return "@STATUS_REQUEST@LIST@END";
        }

        /// <summary>
        /// XRF에 해당이름의 LIST를 OPEN한다.
        /// </summary>
        /// <param name="sListName"></param>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_LIST_OPEN(string sListName)
        {
            return "@LIST@OPEN" + "@NAME=" + sListName + "@END";
        }

        /// <summary>
        /// XRF에 해당이름의 LIST를 닫는다.
        /// </summary>
        /// <param name="sListName"></param>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_LIST_CLOSE()
        {
            return "@LIST@CLOSE@END";
        }

        /// <summary>
        /// 현재 열려있는 LIST를 활성화한다.(SAMPLE을 ADD 할수 있도록)
        /// </summary>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_LIST_START()
        {
            return "@LIST@START@END";
        }

        /// <summary>
        /// 현재 열려있는 LIST를 비활성화한다.
        /// </summary>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_LIST_STOP()
        {
            return "@LIST@STOP@END";
        }

        /// <summary>
        /// 현재 열려있는 LIST에 SAMPLE ID를 등록한다.
        /// </summary>
        /// <param name="sampleID"></param>
        /// <param name="applicationName"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_SAMPLE_ADD(string sampleID, string applicationName, string positionOnTheList)
        {
            return "@SAMPLE@ADD" + "@SAMPLE_ID=" + sampleID + "@APPLICATION=" + applicationName + "@AT=" + positionOnTheList + "@UNIT=xrf@END";
            //return "@SAMPLE@ADD" + "@SAMPLE_ID=" + sampleID + "@APPLICATION=" + applicationName + "@AT=" + positionOnTheList + "@UNIT=xrf@MEASURE=true" + "@END";
        }

        /// <summary>
        /// 시험할 시편의 ID와 position을 전송하여 시험을 시작한다.
        /// </summary>
        /// <param name="sampleID"></param>
        /// <param name="applicationName"></param>
        /// <param name="positionOnTheList"></param>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_MESURE(string sampleID, string applicationName, string positionOnTheList)
        {
            return "@SAMPLE" + "@MEASURE" + "@SAMPLE_ID=" + sampleID + "@AT=" + positionOnTheList + "@UNIT=xrf" + "@END";
        }

        /// <summary>
        /// LIST상에서 제거할 시편의 ID와 position으로 시편을 지운다.
        /// </summary>
        /// <param name="sampleID"></param>
        /// <param name="applicationName"></param>
        /// <param name="positionOnTheList"></param>
        /// <returns></returns>
        public string XRF_MAKE_SEND_COMMAND_REMOVE(string sampleID)
        {
            return "@SAMPLE" + "@REMOVE" + "@SAMPLE_ID=" + sampleID + "@UNIT=xrf" + "@END";
        }
        #endregion

        #region [ BRUKER 명령 ]
        /// <summary>
        /// Start the measurement
        /// </summary>
        /// <returns></returns>
        //public string XRF_BRUKER_MEASMP(string SAMPLENAME, string APPLICATION, string POS, string MODE)
        public string XRF_BRUKER_MEASMP(string SAMPLENAME, string APPLICATION)
        {
            return "MEASMP \"" + SAMPLENAME + "\" \"" + APPLICATION + "\" Z01 UN 0 1 0 10 50" + Environment.NewLine;
        }

        /// <summary>
        /// Watch the status of the sample
        /// </summary>
        /// <returns></returns>
        public string XRF_BRUKER_STATUS(string SAMPLENAME)
        {
            return "STATUS \"" + SAMPLENAME + "\"" + Environment.NewLine;
        }

        /// <summary>
        /// Watch the status of all sample
        /// </summary>
        /// <returns></returns>
        public string XRF_BRUKER_STATUS()
        {
            return "STATUS $ALL" + Environment.NewLine;
        }

        /// <summary>
        /// Watch the status of Instrument
        /// </summary>
        /// <returns></returns>
        public string XRF_BRUKER_STATUS(int nStatus)
        {
            return "STATUS" + Environment.NewLine;
        }

        /// <summary>
        /// Retrieve the results
        /// </summary>
        /// <returns></returns>
        public string XRF_BRUKER_READRS(string SAMPLENAME)
        {
            return "READRS \"" + SAMPLENAME + "\"" + Environment.NewLine;
        }

        /// <summary>
        /// Unload sample to Conveyor(In case you have a conveyor
        /// </summary>
        /// <returns></returns>
        public string XRF_BRUKER_UNLOAD(string SAMPLENAME)
        {
            return "UNLOAD \"" + SAMPLENAME + "\"" + Environment.NewLine;
        }

        /// <summary>
        /// Clear the position and sample name.
        /// </summary>
        /// <returns></returns>
        public string XRF_BRUKER_SMPUNL(string SAMPLENAME)
        {
            return "SMPUNL \"" + SAMPLENAME + "\"" + Environment.NewLine;
        }

        /// <summary>
        /// Reset.
        /// </summary>
        /// <returns></returns>
        public string XRF_BRUKER_RESET()
        {
            return "RESETD" + Environment.NewLine;
        }

        /// <summary>
        /// CANCEL.
        /// </summary>
        /// <returns></returns>
        public string XRF_BRUKER_CANCEL(string SAMPLENAME)
        {
            return "CANCEL \"" + SAMPLENAME + "\"" + Environment.NewLine;
        }
        #endregion
    }

    public class XrfControl
    {
        private static string sLogPath = Application.StartupPath + @"\Log\";


        public static string m_Application = "Z_GG_ZFP";    //string.Empty;
        public static string m_Xrf_IP = string.Empty;
        public static string m_Application_X2 = "Z_GG_ZFP";    //string.Empty;
        public static string m_Xrf_IP_X2 = string.Empty;    // BRUKER
        /// <summary>
        /// XRF LIST NAME ("HMTS2")
        /// </summary>
        public static string XRF_LIST_NAME = "HMTS2";
        /// <summary>
        /// 시편 버퍼 132개
        /// </summary>
        public static string[] m_XrfBuffer = new string[132];       // 전체 버퍼 배열
        public static int m_X1XrfBufferSize = 88;                   // PANALYTICAL 버퍼 크기
        public static int m_X2XrfBufferSize = 44;                   // BRUKER 버퍼 크기
        /// <summary>
        /// 현재 List 버퍼에 있는 시편의 수량 (0 ~ 4 까지)
        /// </summary>
        public static int m_XrfListBufferCount = 0; 
        /// <summary>
        /// 현재 List 버퍼에 있는 시편중 측정기에 넣어야할 시편의 순서(1 ~ 4 까지)
        /// </summary>
        public static int m_XrfListBufferSeq = 0;                  // 


        public XrfControl()
        {
        }


        public static string[] XRF_Message_Parsing(string sMessage)
        {
            string[] XRF_MSG = null;
            int FindIndex = -1;
            int iCount = -1;
            string sReminMSG = string.Empty;
            try {
                for (int i = 0; i < 100; i++)
                {
                    FindIndex = sMessage.IndexOf("@", FindIndex + 1);
                    iCount++;

                    if (FindIndex == -1)
                        break;
                }

                XRF_MSG = new string[iCount];

                FindIndex = 1;
                for (int i = 0; i < iCount; i++)
                {
                    FindIndex = sMessage.IndexOf("@", FindIndex + 1);
                    if (FindIndex == -1)
                        break;
                    int j = sMessage.IndexOf("@", FindIndex + 1);
                    int iLen = 0;
                    if (j == -1)
                    {
                        iLen = 3;
                    }
                    else
                    {
                        iLen = j - (FindIndex + 1);
                    }
                    XRF_MSG[i] = sMessage.Substring(FindIndex + 1, iLen);
                }
                if(XRF_MSG.Length > 0) WriteLogData(XRF_MSG, "Receive Data");
            }
            catch
            {
                XRF_MSG = null;
            }
            finally 
            {
            }
            return XRF_MSG;
        }

        public static void WriteLogData(string sData, string SendReceive)
        {
            string sFilePath = sLogPath + DateTime.Now.ToString("yyyyMMdd") + ".log";
            TextWriter tw = null;

            if (!Directory.Exists(sLogPath))
            {
                Directory.CreateDirectory(sLogPath);
            }
            tw = new StreamWriter(sFilePath, true);
            tw.WriteLine("[" + SendReceive + "]-------------------------------" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

            tw.WriteLine(sData);
            tw.WriteLine("-------------------------------[" + SendReceive + "]");
            tw.Close();
        }

        public static void WriteLogData(string[] sData, string SendReceive)
        {
            string sFilePath = sLogPath + DateTime.Now.ToString("yyyyMMdd") + ".log";
            TextWriter tw = null;

            if (!Directory.Exists(sLogPath))
            {
                Directory.CreateDirectory(sLogPath);
            }
            tw = new StreamWriter(sFilePath, true);
            tw.WriteLine("[" + SendReceive + "]-------------------------------" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

            for (int i = 0; i < sData.Length; i++)
            {
                tw.WriteLine(sData[i]);
            }
            tw.WriteLine("-------------------------------[" + SendReceive + "]");
            tw.Close();
        }

    }

    /// <summary>
    /// BRUKER 사용 메소드
    /// </summary>
    public class BrukerMethod
    {
        #region [ 통신 응답 문자열 배열로 반환 메서드 ]
        /// <summary>
        /// 통신 응답 문자열 배열로 반환 메서드
        /// </summary>
        /// <param name="strAnswer">Meas8m 응답 메시지</param>
        /// <returns></returns>
        public static string[] getBrukerAnswerStringSplit(string strAnswer)
        {
            if (string.IsNullOrEmpty(strAnswer))
                return null;

            string[] strArrList = new string[0];
            StringBuilder sbItem = new StringBuilder();
            int iDQMCount = 0;
            strAnswer = strAnswer.Replace(Environment.NewLine, "") + " ";
            sbItem.Clear();
            for (int iCnt = 0; iCnt < strAnswer.Length; iCnt++)
            {
                string strTemp = strAnswer.Substring(iCnt, 1);
                if (!strTemp.Equals(" ") && !strTemp.Equals("\""))
                {
                    // 일반문자열일 경우 계속 누적
                    sbItem.Append(strAnswer.Substring(iCnt, 1));
                }
                else if (strTemp.Equals("\""))
                {
                    // 큰따옴표일경우 홀수모드일경우 무시
                    iDQMCount++;
                }
                else
                {
                    // 공백이 들어왔을경우 처리
                    if (iDQMCount % 2 == 0)
                    {
                        Array.Resize(ref strArrList, strArrList.Length + 1);
                        strArrList[strArrList.Length - 1] = sbItem.ToString();
                        sbItem.Clear();
                    }
                    else
                    {
                        sbItem.Append(strAnswer.Substring(iCnt, 1));
                    }
                }
            }
            return strArrList;
        }
        #endregion

        #region [ 시험결과 파싱 ]
        public static void getTestResult(string strTestResult)
        {
            if (string.IsNullOrEmpty(strTestResult))
                return;
            #region [ 기존 소스 ]
            ///김정건
            ///2017.10.27
            ///사유 : 단위가 %가 아닌 m/s처럼 변경되기 때문에 단위 체크하지 않는 소스로 변경
            //string[] strArrList = new string[0];
            //StringBuilder sbItem = new StringBuilder();
            //int iDQMCount = 0;
            //strTestResult = strTestResult.Replace(Environment.NewLine, "") + " ";
            //sbItem.Clear();
            //for (int iCnt = 0; iCnt < strTestResult.Length; iCnt++)
            //{
            //    string strTemp = strTestResult.Substring(iCnt, 1);
            //    if (!strTemp.Equals(" ") && !strTemp.Equals("\""))
            //    {
            //        // 일반문자열일 경우 계속 누적
            //        sbItem.Append(strTestResult.Substring(iCnt, 1));
            //    }
            //    else if (strTemp.Equals("\""))
            //    {
            //        // 큰따옴표일경우 홀수모드일경우 무시
            //        iDQMCount++;
            //    }
            //    else
            //    {
            //        // 공백이 들어왔을경우 처리
            //        if (iDQMCount % 2 == 0)
            //        {
            //            Array.Resize(ref strArrList, strArrList.Length + 1);
            //            strArrList[strArrList.Length - 1] = sbItem.ToString();
            //            sbItem.Clear();
            //        }
            //        else
            //        {
            //            sbItem.Append(strTestResult.Substring(iCnt, 1));
            //        }
            //    }
            //}

            //TestResult.Instance.SampleNumber = strArrList[2].Length == 11 ? strArrList[2].Substring(0, 8).Trim() : string.Empty;
            //TestResult.Instance.TMBDiv = strArrList[2].Length == 11 ? strArrList[2].Substring(8, 1).Trim() : string.Empty;
            //TestResult.Instance.PunchLocation = strArrList[2].Length == 11 ? strArrList[2].Substring(9, 1).Trim() : string.Empty;
            //TestResult.Instance.FrontAndBack = strArrList[2].Length == 11 ? strArrList[2].Substring(10, 1).Trim() : string.Empty;
            //TestResult.Instance.ApplicationName = string.IsNullOrEmpty(strArrList[3]) ? string.Empty : strArrList[3];
            //TestResult.Instance.TestDateTime = DateTime.Parse(strArrList[4]);
            //int[] iPerResultIndex = new int[0];
            //for (int iCnt = 0; iCnt < strArrList.Length; iCnt++)
            //{
            //    if (strArrList[iCnt].Equals("%"))
            //    {
            //        Array.Resize(ref iPerResultIndex, iPerResultIndex.Length + 1);
            //        iPerResultIndex[iPerResultIndex.Length - 1] = iCnt;
            //    }
            //}

            //ConcurrentDictionary<string, double> conDic = new ConcurrentDictionary<string, double>();
            //for (int iPerIndex = 0; iPerIndex < iPerResultIndex.Length; iPerIndex++)
            //{
            //    conDic.TryAdd(strArrList[iPerResultIndex[iPerIndex] - 2].ToUpper(), double.Parse(strArrList[iPerResultIndex[iPerIndex] - 1]));
            //}
            //TestResult.Instance.TestElement = conDic;
            #endregion


            string[] strArrList = new string[0];
            StringBuilder sbItem = new StringBuilder();
            int iDQMCount = 0;
            strTestResult = strTestResult.Replace(Environment.NewLine, "") + " ";
            sbItem.Clear();
            for (int iCnt = 0; iCnt < strTestResult.Length; iCnt++)
            {
                string strTemp = strTestResult.Substring(iCnt, 1);
                if (!strTemp.Equals(" ") && !strTemp.Equals("\""))
                {
                    // 일반문자열일 경우 계속 누적
                    sbItem.Append(strTestResult.Substring(iCnt, 1));
                }
                else if (strTemp.Equals("\""))
                {
                    // 큰따옴표일경우 홀수모드일경우 무시
                    iDQMCount++;
                }
                else
                {
                    // 공백이 들어왔을경우 처리
                    if (iDQMCount % 2 == 0)
                    {
                        Array.Resize(ref strArrList, strArrList.Length + 1);
                        strArrList[strArrList.Length - 1] = sbItem.ToString();
                        sbItem.Clear();
                    }
                    else
                    {
                        sbItem.Append(strTestResult.Substring(iCnt, 1));
                    }
                }
            }

            TestResult.Instance.SampleNumber = strArrList[2].Length == 11 ? strArrList[2].Substring(0, 8).Trim() : string.Empty;
            TestResult.Instance.TMBDiv = strArrList[2].Length == 11 ? strArrList[2].Substring(8, 1).Trim() : string.Empty;
            TestResult.Instance.PunchLocation = strArrList[2].Length == 11 ? strArrList[2].Substring(9, 1).Trim() : string.Empty;
            TestResult.Instance.FrontAndBack = strArrList[2].Length == 11 ? strArrList[2].Substring(10, 1).Trim() : string.Empty;
            TestResult.Instance.ApplicationName = string.IsNullOrEmpty(strArrList[3]) ? string.Empty : strArrList[3];
            TestResult.Instance.TestDateTime = DateTime.Parse(strArrList[4]);

            string[] strReadData = strTestResult.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            ConcurrentDictionary<string, double> conDic = new ConcurrentDictionary<string, double>();
            for (int iCnt = 7; iCnt < strReadData.Length; iCnt++)
            {

                conDic.TryAdd(strReadData[iCnt].ToUpper(), double.Parse(strReadData[++iCnt]));
                iCnt += 2; // 단위 + 쓰레기값 처리하지 않음
            }
            TestResult.Instance.TestElement = conDic;
        }
        #endregion
    }
}
