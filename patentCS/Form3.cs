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
    public partial class Form3 : Form
    {
        public class CPatenInfo
        {
         public string sShenqingri;
         public string sMingcheng;
         public string sShenqinghao;
         public string sIPC;
         public string sGongkairi;
        }

        List<string> vCompanies = new List<string>();
        Dictionary<string, List<CPatenInfo>> vPatents = new Dictionary<string, List<CPatenInfo>>();
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
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
            FileStream fsFilestatistics = new FileStream("申请统计.txt", FileMode.Create);
            StreamWriter srFileBat = new StreamWriter(fsFilestatistics);

            for (int i = 0; i < nCMPS; i++)
            {
                string sComName = vCompanies[i];
                string sFileName = sComName + "发明.txt";

                Console.WriteLine(">>>>>>>>>>>> Collecting " + sComName + " Begin >>>>>>>>>>>");
                srFileBat.WriteLine(">>>>>>>>>>>> Collecting " + sComName + " Begin >>>>>>>>>>>");
                try
                {
                    FileStream fsFileBat = new FileStream(sFileName, FileMode.Open);
                    StreamReader swFileBat = new StreamReader(fsFileBat);
                    int nCount09 = 0;
                    int nCount10 = 0;
                    int nCount11 = 0;
                    int nCount12 = 0;
                    int nCount13 = 0;
                    List<CPatenInfo> lTmpPatentInfo = new List<CPatenInfo>();
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
                            else if (j==2)
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
                        CPatenInfo patentInfo = new CPatenInfo();
                        patentInfo.sShenqingri = sShenqingri;
                        patentInfo.sMingcheng = sGongkaiMing;
                        patentInfo.sShenqinghao = sShenqinghao;
                        patentInfo.sIPC = sIPC;
                        patentInfo.sGongkairi = sGongkairi;
                        lTmpPatentInfo.Add(patentInfo);

                        // analysis
                        int indexNian = sShenqingri.IndexOf(".");
                        string sShenqingNian = sShenqingri.Substring(0, indexNian);
                        switch (sShenqingNian)
                        {
                            case "2009":
                                nCount09++;
                                break;
                            case "2010":
                                nCount10++;
                                break;
                            case "2011":
                                nCount11++;
                                break;
                            case "2012":
                                nCount12++;
                                break;
                            case "2013":
                                nCount13++;
                                break;
                        }
                    }

                    vPatents.Add(sComName, lTmpPatentInfo);

                    srFileBat.WriteLine("09 " + nCount09);
                    srFileBat.WriteLine("10 " + nCount10);
                    srFileBat.WriteLine("11 " + nCount11);
                    srFileBat.WriteLine("12 " + nCount12);
                    srFileBat.WriteLine("13 " + nCount13);

                    srFileBat.WriteLine("<<<<<<<<<<<<<< Collecting " + sComName + " End <<<<<<<<<<<<<<");
                }
                catch (System.Exception ex)
                {
                    srFileBat.WriteLine(sComName + ": 0");
                    srFileBat.WriteLine("<<<<<<<<<<<<<< Collecting " + sComName + " End <<<<<<<<<<<<<<");
                    Console.WriteLine("===========ERROR " + ex.Message + "================");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }
    }
}
