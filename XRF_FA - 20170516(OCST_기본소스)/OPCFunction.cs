using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

using opcNet.IF.SEM;
namespace XRF_FA
{
    public class OPCFunction: MainForm
    {
        //// Org(자동화 테스트전(2016-11-01))
        //public const int TAG_GROUP01 = 25;
        //public const int TAG_CONV_LOC = 12;
        //public const int TAG_BUFFER = 80;
        //public const int TAG_SIGNAL = 67;

        //// 2016-11-01 변경(자동화테스트)
        //public const int TAG_GROUP01 = 28;
        //public const int TAG_CONV_LOC = 20;
        //public const int TAG_BUFFER = 132;//80;
        //public const int TAG_SIGNAL = 67;

        // 2016-11-12 변경(BRUKER)
        public const int TAG_GROUP01 = 28;
        public const int TAG_CONV_LOC = 20;
        public const int TAG_BUFFER = 132;//80;
        public const int TAG_SIGNAL = 67;
        public const int TAG_GROUP02 = 15;


        //// Add
        //public const int TAG_GROUP01 = 32;
        //public const int TAG_CONV_LOC = 13;
        //public const int TAG_BUFFER = 132;
        //public const int TAG_SIGNAL = 67;
        //public const int TAG_ALARM = 46;

        ////// Mod2
        //public const int TAG_GROUP01 = 27;
        //public const int TAG_CONV_LOC = 13;
        //public const int TAG_BUFFER = 132;
        //public const int TAG_SIGNAL = 67;
        ////public const int TAG_ALARM = 46;

        //public string[] Group_Names = new string[3];
        //public string[] TagName_Group01 = new string[TAG_GROUP01];
        //public string[] TagName_Conv_Loc = new string[TAG_CONV_LOC];
        //public string[] TagName_Buffer = new string[TAG_BUFFER];

        // 2016-11-12 변경(BRUKER)
        public string[] Group_Names = new string[4];
        public string[] TagName_Group01 = new string[TAG_GROUP01];
        public string[] TagName_Conv_Loc = new string[TAG_CONV_LOC];
        public string[] TagName_Buffer = new string[TAG_BUFFER];
        public string[] TagName_Group02 = new string[TAG_GROUP02];

        #region  // OPC 공통 Method

        public void init_TagName()
        {
            for (int i = 0; i < TAG_BUFFER; i++)
            {
                TagName_Buffer[i] = "Buffer" + (i + 1).ToString();
            }


        }

        /// <summary>
        /// 해당 그룹에 소속된 모든 Tag들의 값을 비동기로 Write 한다.
        /// </summary>
        /// <param name="m_opcMgr"></param>
        /// <param name="sGroupName"></param>
        /// <param name="iGroupTagCount"></param>
        /// <param name="oWriteDatas"></param>
        /// <returns></returns>
        public static int _OPC_Write_Ascyn_Group_Tags(opcMgrClass m_opcMgr, string sGroupName, ref object[] oWriteDatas)
        {
            if (m_opcMgr == null)   // OPC Server가 실행되지 않았다.
            {
                return 999;
            }

            int iTagCount = oWriteDatas.Length;

            int iResFunc = m_opcMgr.opcWriteAsyncGroupTags(sGroupName, iTagCount, ref oWriteDatas, 3);
            Thread.Sleep(300);
            return iResFunc;
        }

        /// <summary>
        /// 해당 그룹에 소속된 일부 Tag들의 값을 비동기로 Write 한다.
        /// </summary>
        /// <param name="m_opcMgr"></param>
        /// <param name="sGroupName"></param>
        /// <param name="iGroupTagCount"></param>
        /// <param name="iTagHandle"></param>
        /// <param name="oWriteData"></param>
        /// <returns></returns>
        public static int _OPC_Writes_Ascyn_Tags(opcMgrClass m_opcMgr, string sGroupName, ref int[] iTagHandle, ref object[] oWriteData)
        {
            if (m_opcMgr == null)   // OPC Server가 실행되지 않았다.
            {
                return 999;
            }

            int iTagCount = iTagHandle.Length;

            int iResFunc = m_opcMgr.opcWritesAsync(sGroupName, iTagCount, ref iTagHandle, ref oWriteData, 5);

            Thread.Sleep(300);

            return iResFunc;
        }

        /// <summary>
        ///  해당 그룹에 소속된 모든 Tag들의 값을 읽는다.
        /// </summary>
        /// <param name="m_opcMgr"></param>
        /// <param name="sGroupName"></param>
        /// <param name="iTagCount"></param>
        /// <returns></returns>
        public static int _OPC_Read_Group_Tags(opcMgrClass m_opcMgr, string sGroupName, int iTagCount, ref object[] oReadVals, ref int[] iQualities)
        {
            if (m_opcMgr == null)   // OPC Server가 실행되지 않았다.
            {
                return 999;
            }

            int iResFunc = m_opcMgr.opcReadGroupTags(sGroupName, iTagCount, ref oReadVals, ref iQualities);

            Thread.Sleep(300);

            return iResFunc;

        }

        #endregion

    }
}
