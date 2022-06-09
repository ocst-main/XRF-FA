using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace XRF_FA
{
    public class TestResult
    {
        #region [ 싱글톤 설정 ]
        private static volatile TestResult _instance;

        public static TestResult Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(TestResult))
                    {
                        if (_instance == null)
                        {
                            _instance = new TestResult();
                        }
                    }
                }
                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// 시험시편정보
        /// </summary>
        public string SampleNumber { get; set; }

        /// <summary>
        /// TMB구분
        /// </summary>
        public string TMBDiv { get; set; }

        /// <summary>
        /// Punch 위치 코드(W,C,D)
        /// </summary>
        public string PunchLocation { get; set; }

        /// <summary>
        /// 전면 이면 구분(F, B)
        /// </summary>
        public string FrontAndBack { get; set; }


        /// <summary>
        /// 시험 프로그램명
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// 시험일시
        /// </summary>
        public DateTime TestDateTime { get; set; }

        /// <summary>
        /// 원소별 시험결과 값
        /// </summary>
        public ConcurrentDictionary<string, double> TestElement { get; set; }

        /// <summary>
        /// 인덱스를 통해 원소이름을 얻는다
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public string GetElementName(int idx)
        {
            return _instance.TestElement.ElementAt(idx).Key;
        }
    }
}
