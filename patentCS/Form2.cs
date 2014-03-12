using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Net;
using System.Web;
using System.IO.Compression;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;

namespace patentCS
{
    public partial class Form2 : Form
    {
        List<string> vCompanies = new List<string>();

        public Form2()
        {
            InitializeComponent();
        }

        private string sendHTTPRequest(string strREQ, Encoding encoding)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strREQ);
                request.Timeout = int.MaxValue;
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                //如果http头中接受gzip的话，这里就要判断是否为有压缩，有的话，直接解压缩即可  
                if (response.Headers["Content-Encoding"] != null && response.Headers["Content-Encoding"].ToLower().Contains("gzip"))
                    responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

                StreamReader streamReader = new StreamReader(responseStream, encoding);
                string retString = streamReader.ReadToEnd();

                streamReader.Close();
                responseStream.Close();

                return retString;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return "";
        }

        private int reqOnePatent(string sPName, string sRECID, string sIPC)
        {
            Encoding myEncoding = Encoding.GetEncoding("gb2312");
            string strKeyWords = "审定授权说明书";
            string baseAdd = "http://211.157.104.87:8080/sipo/zljs/hyjs-yx-new.jsp?";
            string sEncodeFileName = HttpUtility.UrlEncode(sPName, myEncoding);
            string strREQ = baseAdd + "recid=" + sRECID + "&leixin=fmzl" + "&title="
                            + sEncodeFileName + "&ipc=" + sIPC;

            string strRET = sendHTTPRequest(strREQ, myEncoding);
            int nValidCount = 0;
            if (strRET.IndexOf(strKeyWords) > 0)
                return 1;
            else
                return 0;
        }


        // recid=CN201310531082.8&leixin=fmzl&title=%BE%B2%B5%E7%BF%D5%C6%F8%BE%BB%BB%AF%D7%B0%D6%C3%BC%B0%B7%BD%B7%A8&ipc=B03C3/00(2006.01)I
        private void Form2_Load(object sender, EventArgs e)
        {
            // load up bat
            string fileBat = "专利.txt";
            try
            {
                FileStream fsFileBat = new FileStream(fileBat, FileMode.Open);
                StreamReader swFileBat = new StreamReader(fsFileBat);
                while (!swFileBat.EndOfStream)
                {
                    string strTmp = swFileBat.ReadLine();
                    strTmp = strTmp.Trim();
                    if (strTmp != "")
                    {
                        vCompanies.Add(strTmp);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("===========ERROR " + ex.Message + "================");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int nCMPS = vCompanies.Count;
            FileStream fsFilestatistics = new FileStream("统计.txt", FileMode.OpenOrCreate);
            StreamWriter srFileBat = new StreamWriter(fsFilestatistics);

            for (int i = 0; i < nCMPS; i++)
            {
                string sComName = vCompanies[i];
                string sFileName = sComName + "发明.txt";

                Console.WriteLine(">>>>>>>>>>>> Collecting " + sComName + " Begin >>>>>>>>>>>");
                srFileBat.WriteLine("@"+ sComName );
                try
                {
                    FileStream fsFileBat = new FileStream(sFileName, FileMode.Open);
                    StreamReader swFileBat = new StreamReader(fsFileBat);
                    int nValidCount = 0;
                    while (!swFileBat.EndOfStream)
                    {
                        string sShenqinghao = "";
                        string sGongkaiMing = "";
                        string sIPC = "";
                        string sShenqingri = "";
                        string sGongkairi = "";
                        // read 11 lines
                        for (int j = 0; j < 11; j++)
                        {
                            string strTmp = swFileBat.ReadLine();
                            strTmp = strTmp.Trim();
                            // 申请号
                            if (j == 1)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sShenqinghao = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 2)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sShenqingri = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 4)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sGongkairi = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 5)//公开名称
                            {
                                int indexS = strTmp.IndexOf(":");
                                sGongkaiMing = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 6)
                            {
                                int indexS = strTmp.IndexOf(":");
                                int indexE = strTmp.IndexOf(";");
                                if (indexE == -1)
                                {
                                    sIPC = strTmp.Substring(indexS + 1);
                                }
                                else
                                {
                                    sIPC = strTmp.Substring(indexS + 1, indexE - indexS - 1);
                                }
                            }
                        }
                      
                        int nRET = reqOnePatent(sGongkaiMing, sShenqinghao, sIPC);
                        nValidCount += nRET;
                        if (nRET>0)
                        {
                            //string tmp = string.Format("Name %s; CID %s; IPC %s", sGongkaiMing, sShenqinghao, sIPC);
                            Console.WriteLine("Valid......" + "Name " +sGongkaiMing + "; CID " +sShenqinghao + "; IPC "+ sIPC);
                            srFileBat.WriteLine("Valid......" + "Name " + sGongkaiMing + "; CID " + sShenqinghao + "; IPC " + sIPC + "申请日期:" + sShenqingri +";公开日期 " + sGongkairi);
                        }
                        //Console.WriteLine(sShenqinghao + "@@" + sGongkaiMing + "@@" + sIPC);
                    }
                    srFileBat.WriteLine(sComName + " Total " + nValidCount);
                    srFileBat.WriteLine("#" + sComName);
                    srFileBat.Flush();
                }
                catch (System.Exception ex)
                {
                    srFileBat.WriteLine(sComName + ": 0");
                    srFileBat.Flush();
                    Console.WriteLine("===========ERROR " + ex.Message + "================");
                }
            }
        }
    }
}
