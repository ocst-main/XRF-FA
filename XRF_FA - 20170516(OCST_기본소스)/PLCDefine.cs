using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Windows.Forms;

namespace XRF_FA
{
    public class PLCDefine
    {
        /// <summary>
        /// XRF CAP OPEN FLAG (true:Open, false:Close)
        /// </summary>
        public static bool m_PlcFrontInEnable = false;
        public static bool m_PlcFrontInComplete = false;
        public static bool m_PlcFrontInTestComplete = false;
        public static bool m_PlcBackInEnable = false;
        public static bool m_PlcBackInComplete = false;
        public static bool m_PlcBackInTestComplete = false;

        public static bool m_RobotRun = false;

        public static bool firstTemp = true;

        public string[] _PLCAddr = {
                             "소재길이 정보요구(R)",
                             "소재정보 전송완료 확인(W)",
                             "XRF CAP OPEN(W)",
                             "XRF CAP CLOSE(W)",
                             "XRF 전면 투입허가(READY)(W)",
                             "XRF 전면 투입완료(START)(R)",
                             "XRF 전면 시험 완료(W)",
                             "XRF 이면 투입허가(READY)(W)",
                             "XRF 이면 투입완료(START)(R)",
                             "XRF 이면 시험 완료(W)",
                             "시편준비완료(R)",
                             "공급소재 없음(CARRY OUT)(W)",
                             "XRF 이상(W)",
                             "L1000(R)",
                             "L1001(R)",
                             "L1002(R)", 
                             "L1003(R)", 
                             "L1004(R)", 
                             "L1005(R)",
                             "Auto Run중(R)",
                             "FA설비 Start(W)",
                             "버퍼 Clear(W)",
                             "CAP OPEN",
                             "이면배출완료",
                             "ROBOT 진공이상"
                            };
        public string[] _PLCLoc = {
                             "소재길이(W)",
                             "소재정보(W)",
                             "소재공급부(W)",  
                             "소재이송부(W)", 
                             "PRESS(W)",
                             "INDEX POS-1(W)",
                             "INDEX POS-2(W)",
                             "INDEX POS-3(W)",
                             "INDEX POS-4(W)",
                             "버퍼 투입시 ROBOT GRIPPER(W)",
                             "버퍼 배출시 ROBOT GRIPPER",
                             "버퍼 투입 수량"
                          };
        public string[] _PLCBuffer = {
                             "버퍼 01", 
                             "버퍼 02", 
                             "버퍼 03", 
                             "버퍼 04", 
                             "버퍼 05", 
                             "버퍼 06", 
                             "버퍼 07", 
                             "버퍼 08", 
                             "버퍼 09", 
                             "버퍼 10", 
                             "버퍼 11", 
                             "버퍼 12", 
                             "버퍼 13", 
                             "버퍼 14", 
                             "버퍼 15", 
                             "버퍼 16", 
                             "버퍼 17", 
                             "버퍼 18", 
                             "버퍼 19", 
                             "버퍼 20", 
                             "버퍼 21", 
                             "버퍼 22", 
                             "버퍼 23", 
                             "버퍼 24", 
                             "버퍼 25", 
                             "버퍼 26", 
                             "버퍼 27", 
                             "버퍼 28", 
                             "버퍼 29", 
                             "버퍼 30", 
                             "버퍼 31", 
                             "버퍼 32", 
                             "버퍼 33", 
                             "버퍼 34", 
                             "버퍼 35", 
                             "버퍼 36", 
                             "버퍼 37", 
                             "버퍼 38", 
                             "버퍼 39", 
                             "버퍼 40", 
                             "버퍼 41", 
                             "버퍼 42", 
                             "버퍼 43", 
                             "버퍼 44", 
                             "버퍼 45", 
                             "버퍼 46", 
                             "버퍼 47", 
                             "버퍼 48", 
                             "버퍼 49", 
                             "버퍼 50", 
                             "버퍼 51", 
                             "버퍼 52", 
                             "버퍼 53", 
                             "버퍼 54", 
                             "버퍼 55", 
                             "버퍼 56", 
                             "버퍼 57", 
                             "버퍼 58", 
                             "버퍼 59", 
                             "버퍼 60", 
                             "버퍼 61", 
                             "버퍼 62", 
                             "버퍼 63", 
                             "버퍼 64", 
                             "버퍼 65", 
                             "버퍼 66", 
                             "버퍼 67", 
                             "버퍼 68", 
                             "버퍼 69", 
                             "버퍼 70", 
                             "버퍼 71", 
                             "버퍼 72", 
                             "버퍼 73", 
                             "버퍼 74", 
                             "버퍼 75", 
                             "버퍼 76", 
                             "버퍼 77", 
                             "버퍼 78", 
                             "버퍼 79",
                             "버퍼 80",
                             "버퍼 81", 
                             "버퍼 82", 
                             "버퍼 83", 
                             "버퍼 84", 
                             "버퍼 85", 
                             "버퍼 86", 
                             "버퍼 87", 
                             "버퍼 88", 
                             "버퍼 89", 
                             "버퍼 90", 
                             "버퍼 91", 
                             "버퍼 92", 
                             "버퍼 93", 
                             "버퍼 94", 
                             "버퍼 95", 
                             "버퍼 96", 
                             "버퍼 97", 
                             "버퍼 98", 
                             "버퍼 99", 
                             "버퍼 100", 
                             "버퍼 101", 
                             "버퍼 102", 
                             "버퍼 103", 
                             "버퍼 104", 
                             "버퍼 105", 
                             "버퍼 106", 
                             "버퍼 107", 
                             "버퍼 108", 
                             "버퍼 109", 
                             "버퍼 110", 
                             "버퍼 111", 
                             "버퍼 112", 
                             "버퍼 113", 
                             "버퍼 114", 
                             "버퍼 115", 
                             "버퍼 116", 
                             "버퍼 117", 
                             "버퍼 118", 
                             "버퍼 119", 
                             "버퍼 120", 
                             "버퍼 121", 
                             "버퍼 122", 
                             "버퍼 123", 
                             "버퍼 124", 
                             "버퍼 125", 
                             "버퍼 126", 
                             "버퍼 127", 
                             "버퍼 128", 
                             "버퍼 129", 
                             "버퍼 130", 
                             "버퍼 131", 
                             "버퍼 132"};
        public static string[] _PLCAlarm = {
                            "PLC Batter Low 이상",
                            "CC-LINK MASTER 이상",
                            "Air-압력 부족 이상",
                            "비상정지 MOP",
                            "비상정지 SOP-1",
                            "비상정지 SOP-2",
                            "소재공급 LOADER 전후 이상",
                            "소재공급 LOADER 상하 이상",
                            "소재공급 LOADER 진공 이상(MAIN)",
                            "소재공급 LOADER 진공 이상(SUB-1)",
                            "소재공급 LOADER 진공 이상(SUB-2)",
                            "소재공급 LOADER 진공 이상(SUB-3)",
                            "소재공급 LOADER 진공 이상(SUB-4)",
                            "1축 P/P GRIPPER 이상",
                            "1축 P/P QD75D1  이상",
                            "1축 P/P SERVO AMP-J3A ALARM",
                            "1축 P/P GRIPPER 소재 감지 이상",
                            "1축 P/P 소재잡는 위치 초과 이상",
                            "잔재배출 상하 이상",
                            "잔재배출 회전 이상",
                            "PRESS 상하강 이상",
                            "시편받이 1단 상하 이상",
                            "시편받이 2단 상하 이상",
                            "시편받이 이송 전후 이상",
                            "MARKING 상하강  이상",
                            "MARKING CONTROL UNIT 이상",
                            "MARKING DATA전송 실패",
                            "세척이송 전후진 이상",
                            "세척이송 상하강 이상",
                            "세척이송 회전 이상",
                            "세척이송 GRIPPER 이상",
                            "INDEX   회전CYL 전후진  이상",
                            "INDEX   POS-1   소재정보 이상",
                            "INDEX   POS-2   소재정보 이상",
                            "INDEX   POS-3   소재정보 이상",
                            "INDEX   POS-4   소재정보 이상",
                            "반전UNIT 시편감지 이상",
                            "반전UNIT 회전CYL 이상",
                            "반전UNIT 상하CYL 이상",
                            "반전UNIT GRIPPER 이상",
                            "ROBOT GRIPPER 이상",
                            "ROBOT 진공이상",
                            "ROBOT 비상정지",
                            "ROBOT 이상",
                            "ROBOT 센터링부 시편감지 이상",
                            "ROBOT 반전UNIT 감지 이상" };
        // for BRUKER
        public string[] _PLCGroup2 = {
                             "Bruker 시편 유",
                             "Bruker 시편 무",
                             "버퍼 위치에 DATA 저장요구",  
                             "Bruker 전면투입 허가(READY)", 
                             "시편정보 비교 요구",
                             "시편정보 비교 OK",
                             "Bruker 전면투입 완료(START)",
                             "Bruker 전면시험 완료",
                             "Bruker 이면투입 허가(READY)",
                             "Bruker 이면투입 완료(START)",
                             "Bruker 이면시험 완료",
                             "시편준비 완료",
                             "공급소재없음(CARRY OUT)",
                             "Bruker 이상"
                          };

        public PLCDefine()
        {
        }

        ///// <summary>
        ///// String array property getter.
        ///// </summary>
        //public string[] PLCAddr
        //{
        //    get { return _PLCAddr; }
        //}

        ///// <summary>
        ///// String array indexer.
        ///// </summary>
        //public string this[int index]
        //{
        //    get { return _PLCAddr[index]; }
        //}


        #region OPC Config 파일 Parser
        /// <summary>
        /// XmlParser()  // Group 명 찾기
        /// </summary>
        public void XmlParser(ComboBox cmb)
        {
            string sPath = @"c:\work\config\UsrAppConf.xml";

            XmlDocument itemDoc = new XmlDocument();
            itemDoc.Load(sPath);
            Console.WriteLine("DocumentElement has {0} children.", itemDoc.DocumentElement.ChildNodes.Count);
            foreach (XmlNode itemNode in itemDoc.DocumentElement.ChildNodes)
            {
                XmlElement itemElement = (XmlElement)itemNode;
                if (itemNode.ChildNodes.Count == 0)
                    Console.WriteLine("(No additional Information)\n");
                else
                {
                    foreach (XmlNode childNode in itemNode.ChildNodes)
                    {
                        if (childNode.Name.ToUpper() == "GROUP")
                        {
                            cmb.Items.Add(childNode.Attributes["name"].Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// XmlParserTAG()  선택된 그룹명으로 Tag 불러오기
        /// </summary>
        public void XmlParserTAG(ComboBox cmbGroup, ComboBox cmbTag)
        {
            cmbTag.Items.Clear();

            if (cmbGroup.Text == "XRF_GROUP01")
            {
                foreach(string sTag in _PLCAddr) 
                {
                    cmbTag.Items.Add(sTag);
                }
            }

            if (cmbGroup.Text == "XRF_CONV_LOC")
            {
                foreach (string sTag in _PLCLoc)
                {
                    cmbTag.Items.Add(sTag);
                }
            }
            
            if (cmbGroup.Text == "XRF_BUFFER")
            {
                foreach (string sTag in _PLCBuffer)
                {
                    cmbTag.Items.Add(sTag);
                }
            }

            // for BRUKER
            if (cmbGroup.Text == "XRF_GROUP02")
            {
                foreach (string sTag in _PLCGroup2)
                {
                    cmbTag.Items.Add(sTag);
                }
            }
            
            //string sPath = @"c:\work\config\UsrAppConf.xml";

            //XmlDocument itemDoc = new XmlDocument();
            //itemDoc.Load(sPath);
            //foreach (XmlNode itemNode in itemDoc.DocumentElement.ChildNodes)
            //{
            //    XmlElement itemElement = (XmlElement)itemNode;
            //    if (itemNode.ChildNodes.Count == 0)
            //        Console.WriteLine("(No additional Information)\n");
            //    else
            //    {
            //        foreach (XmlNode childNode in itemNode.ChildNodes)
            //        {
            //            if (childNode.Name.ToUpper() == "GROUP")
            //            {
            //                if (childNode.Attributes["name"].Value == cmbGroup.Text)
            //                {
            //                    XmlElement subitemElement = (XmlElement)childNode;
            //                    foreach (XmlNode subchildNode in childNode.ChildNodes)
            //                    {
            //                        if (subchildNode.Name.ToUpper() == "TAG")
            //                        {
            //                            cmbTag.Items.Add(subchildNode.Attributes["name"].Value);
            //                        }
            //                    }

            //                }
            //            }
            //        }
            //    }
            //}

        }

        public void XmlParserTAG(string cmbGroup, ComboBox cmbTag)
        {
            cmbTag.Items.Clear();

            if (cmbGroup == "XRF_GROUP01")
            {
                foreach (string sTag in _PLCAddr)
                {
                    cmbTag.Items.Add(sTag);
                }
            }

            if (cmbGroup == "XRF_CONV_LOC")
            {
                foreach (string sTag in _PLCLoc)
                {
                    cmbTag.Items.Add(sTag);
                }
            }

            if (cmbGroup == "XRF_BUFFER")
            {
                foreach (string sTag in _PLCBuffer)
                {
                    cmbTag.Items.Add(sTag);
                }
            }

            // for BRUKER
            if (cmbGroup == "XRF_GROUP02")
            {
                foreach (string sTag in _PLCGroup2)
                {
                    cmbTag.Items.Add(sTag);
                }
            }

        }
        #endregion

    }
}
