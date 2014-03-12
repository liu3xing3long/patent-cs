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
        // CompName-->(PatentName,...PatentInfo)
        Dictionary<string, Dictionary<string, List<CPatenInfo>>> vPatents = new Dictionary<string, Dictionary<string, List<CPatenInfo>>>();
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
            FileStream fsFilestatistics = new FileStream("发明统计.txt", FileMode.Create);
            StreamWriter srFileBat = new StreamWriter(fsFilestatistics);

            for (int i = 0; i < nCMPS; i++)
            {
                string sComName = vCompanies[i];
                //string sComName = "邦讯技术";
                string sFileName = sComName + "发明.txt";

                Console.WriteLine(">>>>>>>>>>>> Collecting " + sComName + " Begin >>>>>>>>>>>");
                srFileBat.WriteLine("@" + sComName);
                int nCount09 = 0;
                int nCount10 = 0;
                int nCount11 = 0;
                int nCount12 = 0;
                int nCount13 = 0;
                int nCountV09 = 0;
                int nCountV10 = 0;
                int nCountV11 = 0;
                int nCountV12 = 0;
                int nCountV13 = 0;
                int nCountValid = 0; 
                try
                {
                    FileStream fsFileBat = new FileStream(sFileName, FileMode.Open);
                    StreamReader swFileBat = new StreamReader(fsFileBat);
                  
                    Dictionary<string, List<CPatenInfo>> lTmpPatentInfo = new Dictionary<string, List<CPatenInfo>>();
                    while (!swFileBat.EndOfStream)
                    {
                        string sShenqinghao = "";
                        string sGongkaiMing = "";
                        string sIPC = "";
                        string sShenqingri = "";
                        string sGongkairi = "";
                        // read 11 lines
                        for (int j = 0; j < 10; j++)
                        {
                            string strTmp = swFileBat.ReadLine();
                            strTmp = strTmp.Trim();
                            // 申请号
                            if (j ==0)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sShenqinghao = strTmp.Substring(indexS + 1);
                            }
                            else if (j==1)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sShenqingri = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 3)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sGongkairi = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 4)//公开名称
                            {
                                int indexS = strTmp.IndexOf(":");
                                sGongkaiMing = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 5)
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

                        List<CPatenInfo>tmpList;
                        bool bFound = lTmpPatentInfo.TryGetValue(sGongkaiMing, out tmpList);
                        if (bFound)
                        {
                            tmpList.Add(patentInfo);
                        }
                        else
                        {
                            List<CPatenInfo>tmpList2 =new List<CPatenInfo>();
                            tmpList2.Add(patentInfo);
                            lTmpPatentInfo.Add(sGongkaiMing, tmpList2);
                        }
                    }


                    // analysis 
                    foreach (var item in lTmpPatentInfo)
                    {
                        //Console.WriteLine("key:{0}", item.Key);
                        if (item.Value.Count>=2)
                        {
                            nCountValid++;
                            Console.WriteLine("Valid Found, Name:{0}, Time0:{1}, Time1:{2}, AccTime0:{3}, AccTime1:{4}", 
                                item.Value[0].sMingcheng, item.Value[0].sShenqingri, item.Value[1].sShenqingri, 
                                 item.Value[0].sGongkairi,  item.Value[1].sGongkairi);
                          
                            string sShenqingri = "";
                            string sGongkairi = "";
                            DateTime dt1 = Convert.ToDateTime(item.Value[0].sGongkairi);
                            int nMaxIndex = 0;
                            for (int iItemV = 0; iItemV < item.Value.Count; iItemV++)
                            {
                                DateTime dt2 = Convert.ToDateTime(item.Value[iItemV].sGongkairi);
                                TimeSpan ts = dt2 - dt1;
                                if (ts.TotalMilliseconds > 0)
                                {
                                    nMaxIndex = iItemV;
                                    dt1 = dt2;
                                }
                            }

                            sShenqingri = item.Value[nMaxIndex].sShenqingri;
                            sGongkairi = item.Value[nMaxIndex].sGongkairi;
                           
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
                            indexNian = sGongkairi.IndexOf(".");
                            sShenqingNian = sGongkairi.Substring(0, indexNian);
                            switch (sShenqingNian)
                            {
                                case "2009":
                                    nCountV09++;
                                    break;
                                case "2010":
                                    nCountV10++;
                                    break;
                                case "2011":
                                    nCountV11++;
                                    break;
                                case "2012":
                                    nCountV12++;
                                    break;
                                case "2013":
                                    nCountV13++;
                                    break;
                            }
                        }
                        else
                        {
                            string sShenqingri = item.Value[0].sShenqingri;
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
                    }                       

                 
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("===========ERROR " + ex.Message + "================");
                }
                srFileBat.WriteLine("09 " + nCount09 + "\t" + nCountV09);
                srFileBat.WriteLine("10 " + nCount10 + "\t" + nCountV10);
                srFileBat.WriteLine("11 " + nCount11 + "\t" + nCountV11);
                srFileBat.WriteLine("12 " + nCount12 + "\t" + nCountV12);
                srFileBat.WriteLine("13 " + nCount13 + "\t" + nCountV13);
                srFileBat.WriteLine("TotalValid " + nCountValid);
                srFileBat.WriteLine("#" + sComName);
            }

            srFileBat.Close();
            fsFilestatistics.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int nCMPS = vCompanies.Count;
            FileStream fsFilestatistics = new FileStream("实用统计.txt", FileMode.Create);
            StreamWriter srFileBat = new StreamWriter(fsFilestatistics);

            for (int i = 0; i < nCMPS; i++)
            {
                string sComName = vCompanies[i];
                string sFileName = sComName + "实用新型.txt";

                Console.WriteLine(">>>>>>>>>>>> Collecting " + sComName + " Begin >>>>>>>>>>>");
                srFileBat.WriteLine("@" + sComName);
                int nCount09 = 0;
                int nCount10 = 0;
                int nCount11 = 0;
                int nCount12 = 0;
                int nCount13 = 0;
                int nCountV09 = 0;
                int nCountV10 = 0;
                int nCountV11 = 0;
                int nCountV12 = 0;
                int nCountV13 = 0; 
                try
                {
                    FileStream fsFileBat = new FileStream(sFileName, FileMode.Open);
                    StreamReader swFileBat = new StreamReader(fsFileBat);

                    Dictionary<string, List<CPatenInfo>> lTmpPatentInfo = new Dictionary<string, List<CPatenInfo>>();
                    while (!swFileBat.EndOfStream)
                    {
                        string sShenqinghao = "";
                        string sGongkaiMing = "";
                        string sIPC = "";
                        string sShenqingri = "";
                        string sGongkairi = "";
                        // read 11 lines
                        for (int j = 0; j < 10; j++)
                        {
                            string strTmp = swFileBat.ReadLine();
                            strTmp = strTmp.Trim();
                            // 申请号
                            if (j == 0)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sShenqinghao = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 1)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sShenqingri = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 3)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sGongkairi = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 4)//公开名称
                            {
                                int indexS = strTmp.IndexOf(":");
                                sGongkaiMing = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 5)
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

                        indexNian = sGongkairi.IndexOf(".");
                        sShenqingNian = sGongkairi.Substring(0, indexNian);
                        switch (sShenqingNian)
                        {
                            case "2009":
                                nCountV09++;
                                break;
                            case "2010":
                                nCountV10++;
                                break;
                            case "2011":
                                nCountV11++;
                                break;
                            case "2012":
                                nCountV12++;
                                break;
                            case "2013":
                                nCountV13++;
                                break;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("===========ERROR " + ex.Message + "================");
                }
                srFileBat.WriteLine("09 " + nCount09 + "\t" + nCountV09);
                srFileBat.WriteLine("10 " + nCount10 + "\t" + nCountV10);
                srFileBat.WriteLine("11 " + nCount11 + "\t" + nCountV11);
                srFileBat.WriteLine("12 " + nCount12 + "\t" + nCountV12);
                srFileBat.WriteLine("13 " + nCount13 + "\t" + nCountV13);
                srFileBat.WriteLine("#" + sComName);
            }

            srFileBat.Close();
            fsFilestatistics.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int nCMPS = vCompanies.Count;
            FileStream fsFilestatistics = new FileStream("外观设计统计.txt", FileMode.Create);
            StreamWriter srFileBat = new StreamWriter(fsFilestatistics);

            for (int i = 0; i < nCMPS; i++)
            {
                string sComName = vCompanies[i];
                string sFileName = sComName + "外观设计.txt";

                Console.WriteLine(">>>>>>>>>>>> Collecting " + sComName + " Begin >>>>>>>>>>>");
                srFileBat.WriteLine("@" + sComName);
                int nCount09 = 0;
                int nCount10 = 0;
                int nCount11 = 0;
                int nCount12 = 0;
                int nCount13 = 0;
                int nCountV09 = 0;
                int nCountV10 = 0;
                int nCountV11 = 0;
                int nCountV12 = 0;
                int nCountV13 = 0;
                try
                {
                    FileStream fsFileBat = new FileStream(sFileName, FileMode.Open);
                    StreamReader swFileBat = new StreamReader(fsFileBat);

                    Dictionary<string, List<CPatenInfo>> lTmpPatentInfo = new Dictionary<string, List<CPatenInfo>>();
                    while (!swFileBat.EndOfStream)
                    {
                        string sShenqinghao = "";
                        string sGongkaiMing = "";
                        string sIPC = "";
                        string sShenqingri = "";
                        string sGongkairi = "";
                        // read 11 lines
                        for (int j = 0; j < 10; j++)
                        {
                            string strTmp = swFileBat.ReadLine();
                            strTmp = strTmp.Trim();
                            // 申请号
                            if (j == 0)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sShenqinghao = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 1)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sShenqingri = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 3)
                            {
                                int indexS = strTmp.IndexOf(":");
                                sGongkairi = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 4)//公开名称
                            {
                                int indexS = strTmp.IndexOf(":");
                                sGongkaiMing = strTmp.Substring(indexS + 1);
                            }
                            else if (j == 5)
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

                        indexNian = sGongkairi.IndexOf(".");
                        sShenqingNian = sGongkairi.Substring(0, indexNian);
                        switch (sShenqingNian)
                        {
                            case "2009":
                                nCountV09++;
                                break;
                            case "2010":
                                nCountV10++;
                                break;
                            case "2011":
                                nCountV11++;
                                break;
                            case "2012":
                                nCountV12++;
                                break;
                            case "2013":
                                nCountV13++;
                                break;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("===========ERROR " + ex.Message + "================");
                }
                srFileBat.WriteLine("09 " + nCount09 + "\t" + nCountV09);
                srFileBat.WriteLine("10 " + nCount10 + "\t" + nCountV10);
                srFileBat.WriteLine("11 " + nCount11 + "\t" + nCountV11);
                srFileBat.WriteLine("12 " + nCount12 + "\t" + nCountV12);
                srFileBat.WriteLine("13 " + nCount13 + "\t" + nCountV13);
                srFileBat.WriteLine("#" + sComName);
            }

            srFileBat.Close();
            fsFilestatistics.Close();
        }
    }
}
