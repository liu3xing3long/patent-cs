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
    public partial class Form1 : Form
    {
        string shiyongxinxing = "实用新型";
        string faming = "发明";
        string waiguansheji = "外观设计";
        List<string> vCompanies = new List<string>();
        public Form1()
        {
            InitializeComponent();
            worker.WorkerReportsProgress = true;

            worker.WorkerSupportsCancellation = true;

            //正式做事情的地方
            worker.DoWork += new DoWorkEventHandler(DoWork);

            //任务完称时要做的，比如提示等等
            worker.ProgressChanged += new ProgressChangedEventHandler(ProgessChanged);

            //任务进行时，报告进度
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompleteWork);
        }
        //调用 RunWorkerAsync 时发生
        public void DoWork(object sender, DoWorkEventArgs e)
        {
            //e.Result = GrabWebPages(worker, e);
            //获取异步操作结果的值，当ComputeFibonacci(worker, e)返回时，异步过程结束
        }
        //调用 ReportProgress 时发生
        public void ProgessChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            //将异步任务进度的百分比赋给进度条
        }

        //当后台操作已完成、被取消或引发异常时发生
        public void CompleteWork(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("完成！");
        }


        /// <summary>
        /// 
        /// </summary>
        string sendHTTPRequest(string strREQ, ref CookieContainer cookie)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.pss-system.gov.cn/sipopublicsearch/search/executeGeneralSearch-executeGeneralSearch.shtml");
                request.CookieContainer = new CookieContainer();//如果用不到Cookie，删去即可  
                //以下是发送的http头，随便加，其中referer挺重要的，有些网站会根据这个来反盗链  
                cookie = request.CookieContainer;//如果用不到Cookie，删去即可  
                request.Referer = "http://www.pss-system.gov.cn/sipopublicsearch/search/searchHomeIndex.shtml";
                request.Accept = "Accept:text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en;q=0.6,zh-TW;q=0.4,ja;q=0.2,ko;q=0.2";
                request.Headers["Accept-Charset"] = "GBK,utf-8;q=0.7,*;q=0.3";
                request.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
                request.KeepAlive = true;
                //上面的http头看情况而定，但是下面俩必须加  
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = int.MaxValue-1;

                string postDataStr = strREQ;
                Encoding encoding = Encoding.UTF8;//根据网站的编码自定义  
                byte[] postData = encoding.GetBytes(postDataStr);//postDataStr即为发送的数据，格式还是和上次说的一样  
                request.ContentLength = postData.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postData, 0, postData.Length);

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

        //////////////////////////////////////////////////////////////////////////
        ///
        string sendHTTPRequest2(string strREQ, ref CookieContainer cookie)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.pss-system.gov.cn/sipopublicsearch/search/showSearchResultAC!startWa.do");
                request.CookieContainer = new CookieContainer();//如果用不到Cookie，删去即可  
                //以下是发送的http头，随便加，其中referer挺重要的，有些网站会根据这个来反盗链  
                request.CookieContainer = cookie;//如果用不到Cookie，删去即可  
                request.Referer = "http://www.pss-system.gov.cn/sipopublicsearch/search/searchHomeIndex.shtml";
                request.Accept = "Accept:text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en;q=0.6,zh-TW;q=0.4,ja;q=0.2,ko;q=0.2";
                request.Headers["Accept-Charset"] = "GBK,utf-8;q=0.7,*;q=0.3";
                request.UserAgent = "User-Agent:Mozilla/5.0 (Windows NT 5.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.202 Safari/535.1";
                request.KeepAlive = true;
                //上面的http头看情况而定，但是下面俩必须加  
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = int.MaxValue-1;

                string postDataStr = strREQ;
                Encoding encoding = Encoding.UTF8;//根据网站的编码自定义  
                byte[] postData = encoding.GetBytes(postDataStr);//postDataStr即为发送的数据，格式还是和上次说的一样  
                request.ContentLength = postData.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postData, 0, postData.Length);

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



        /// <summary>
        /// 去除字符串中的空格、回车、换行符、制表符，以及将中文下的扩靠转换为英文下的括号
        /// </summary>
        /// <param name="str">要操作的字符串</param>
        /// <returns>操作完成的字符</returns>
        public String replaceBlank(String str)
        {
            String result = str;
            if (str != null)
            {
                result = Regex.Replace(result, @"\s*|\t|\r|\n", "", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "（", "(", RegexOptions.IgnoreCase);
                result = Regex.Replace(result, "）", ")", RegexOptions.IgnoreCase);
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iNum"></param>
        /// <returns></returns>
        public string translateFormedWebString(string iNum)
        {
            string sTmp = "";
            sTmp += HttpUtility.UrlEncode("["+iNum+"][").ToUpper();
            sTmp += "+";
            sTmp += HttpUtility.UrlEncode("]{0,}").ToUpper();
            return sTmp;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
           // load up bat
            string fileBat = "专利.txt";
            try
            {
                FileStream fsFileBat = new FileStream(fileBat, FileMode.Open);
                StreamReader swFileBat = new StreamReader(fsFileBat);
                while(!swFileBat.EndOfStream)
                {
                    string strTmp = swFileBat.ReadLine();
                    strTmp = strTmp.Trim();
                    if (strTmp != "")
                    {
                        vCompanies.Add(strTmp);
                        txtSource.Text += strTmp;
                        txtSource.Text += "\r\n";
                    }
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("===========ERROR " + ex.Message + "================");
            }
        }

        private void request_one_company(string sComName)
        {
            string strCondition = HttpUtility.UrlEncode("申请日>=2009-01-01 AND 申请（专利权）人=(" + sComName + ")").ToUpper();
            string postDataStr = "searchCondition.searchExp=" + strCondition +
                 "&searchCondition.dbId=VDB&searchCondition.searchType=Sino_foreign&wee.bizlog.modulelevel=0200101&searchCondition.extendInfo%5B'MODE'%5D=MODE_GENERAL";
            CookieContainer cookie = new CookieContainer();
            int totalData = 0, perPageData = 0;
            string sREQ = sendHTTPRequest(postDataStr, ref cookie);
            if (sREQ.IndexOf("没有检索到结果") > 0)
                return;
            refresh_onePage(sREQ, sComName, ref perPageData, ref totalData, 0);

            if (totalData < perPageData)
                return;

            string VDB1 = "VDB:((APD>=";
            string VDB2 = " AND PAVIEW=";
            string VDB3 = "))";
            string keywords = "";
            string tmpkeywords = "";
            string keywords2 = translateFormedWebString(Convert.ToString(2)) + translateFormedWebString(Convert.ToString(0)) + translateFormedWebString(Convert.ToString(0)) + translateFormedWebString(Convert.ToString(9))
                              + translateFormedWebString(".") + translateFormedWebString(Convert.ToString(0)) + translateFormedWebString(Convert.ToString(1))
                              + translateFormedWebString(".") + translateFormedWebString(Convert.ToString(0)) + translateFormedWebString(Convert.ToString(1));

            TextElementEnumerator txtEnum = StringInfo.GetTextElementEnumerator(sComName);
            while (txtEnum.MoveNext())
            {
                tmpkeywords = "";
                tmpkeywords += "[";
                tmpkeywords += txtEnum.GetTextElement();
                tmpkeywords += "]";
                tmpkeywords += "[";
                keywords += HttpUtility.UrlEncode(tmpkeywords).ToUpper();
                keywords += "+";
                tmpkeywords = "";
                tmpkeywords += "]";
                tmpkeywords += "{0,}";
                keywords += HttpUtility.UrlEncode(tmpkeywords).ToUpper();
            }

            int nStart = perPageData;
            int nTotal = totalData;

            while (nStart < nTotal)
            {
                postDataStr = "resultPagination.limit=" + perPageData.ToString() + "&resultPagination.start=" + nStart.ToString()
                            + "&resultPagination.totalCount=" + totalData.ToString() + "&searchCondition.searchType=Sino_foreign&searchCondition.dbId=&searchCondition.power=false&searchCondition.strategy=&searchCondition.literatureSF="
                            + "&searchCondition.searchExp=" + strCondition + "&wee.bizlog.modulelevel=0200201&searchCondition.executableSearchExp=VDB%3A((APD%3E%3D'2009-01-01'+AND+PAVIEW%3D'"
                            + HttpUtility.UrlEncode(sComName).ToUpper() + "'))&searchCondition.searchKeywords=&searchCondition.searchKeywords=" + keywords
                            + "&searchCondition.searchKeywords=" + keywords2;
                sREQ = sendHTTPRequest2(postDataStr, ref cookie);
                if (sREQ.IndexOf("没有检索到结果") > 0)
                    return;

                if (sREQ=="")
                {
                    Console.WriteLine("------------------------- REQ EMPTY!! TIME OUT!!!! -------------------------");
                }
                refresh_onePage(sREQ, sComName, ref perPageData, ref totalData, 1);
                nStart += perPageData;
            }
        }
        private void refresh_onePage(string sHTML, string sCompanyName, ref int perpageData, ref int totalData, int mode)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            try
            {
                doc.LoadHtml(sHTML);
                //doc.Load("test.html", Encoding.UTF8);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            // There are various options, set as needed
            doc.OptionFixNestedTags = true;
            // ParseErrors is an ArrayList containing any errors from the Load statement
            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
            {
                Console.WriteLine("Format Error......");
            }

            //////////////////////////////////////////////////////////////////////////
            /// 3 File Handlers
            string fileName = sCompanyName;
            FileStream fsshiyongxinxing = new FileStream(fileName + shiyongxinxing + ".txt", mode==0?FileMode.Create:FileMode.Append);
            FileStream fsfaming = new FileStream(fileName + faming + ".txt", mode == 0 ? FileMode.Create : FileMode.Append);
            FileStream fswaiguansheji = new FileStream(fileName + waiguansheji + ".txt", mode == 0 ? FileMode.Create : FileMode.Append);

            StreamWriter swshiyongxinxing = new StreamWriter(fsshiyongxinxing);
            StreamWriter swfaming = new StreamWriter(fsfaming);
            StreamWriter swwaiguansheji = new StreamWriter(fswaiguansheji);

            HtmlNode node = doc.DocumentNode;
            HtmlNode nodeTotalData = node.SelectSingleNode("//input[@id=\"resultPagination.totalCount\"]");
            totalData = int.Parse(nodeTotalData.Attributes["value"].Value);

            HtmlNode nodePerpageData = node.SelectSingleNode("//input[@id=\"resultPagination.limit\"]");
            perpageData = int.Parse(nodePerpageData.Attributes["value"].Value);

            HtmlNodeCollection h3c = node.SelectNodes("//h3[@class=\"sqh\"]");
            if (h3c.Count > 0)
            {
                for (int i = 0; i < h3c.Count; i++)
                {
                    HtmlNode h3cNode = h3c[i];
                    string h3cnodeText = h3cNode.InnerText;
                    string strResult = replaceBlank(h3cnodeText);
                    bool bFound1 = strResult.IndexOf(shiyongxinxing) > 0 ? true : false;
                    bool bFound2 = strResult.IndexOf(faming) > 0 ? true : false;
                    bool bFound3 = strResult.IndexOf(waiguansheji) > 0 ? true : false;
                    swwaiguansheji.WriteLine("");
                    if (bFound1)
                        swshiyongxinxing.WriteLine(strResult);
                    else if (bFound2)
                        swfaming.WriteLine(strResult);
                    else if (bFound3)
                        swwaiguansheji.WriteLine(strResult);
                    else
                        Console.WriteLine("=====================ERROR, Not Invalid Index=====================");


                    string sLookupPattern = "//div[@id=\"result_inner_left_div" + i.ToString() + "\"]";
                    HtmlNode subDiv = node.SelectSingleNode(sLookupPattern);
                    HtmlNodeCollection tableNodeCollection = subDiv.SelectNodes("div[@class=\"conter_talbe\"]");
                    foreach (HtmlNode tableNode in tableNodeCollection)
                    {
                        string tableNodeText = tableNode.InnerText;
                        strResult = replaceBlank(tableNodeText);
                        if (bFound1)
                            swshiyongxinxing.WriteLine(strResult);
                        else if (bFound2)
                            swfaming.WriteLine(strResult);
                        else if (bFound3)
                            swwaiguansheji.WriteLine(strResult);
                        else
                            Console.WriteLine("=====================ERROR, Not Invalid Index=====================");
                    }
                }
            }

            //清空缓冲区
            swshiyongxinxing.Flush();
            swshiyongxinxing.Close();
            swshiyongxinxing.Close();
            //清空缓冲区
            swfaming.Flush();
            swfaming.Close();
            swfaming.Close();
            //清空缓冲区
            swwaiguansheji.Flush();
            swwaiguansheji.Close();
            swwaiguansheji.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GrabWebPages(/*sender, e*/);
//             worker.RunWorkerAsync();
//             button1.Enabled = false;
        }
        private int GrabWebPages(/*object sender, DoWorkEventArgs e*/)
        {
            int nCMPS = vCompanies.Count;
            for (int i = 0; i < nCMPS; i++ )
            {
                string sComName = vCompanies[i];
                request_one_company(sComName);
                string sTXT = txtSource.Text;
                sTXT=sTXT.Replace(sComName+"\r\n", "");
                txtSource.Text = sTXT;
                txtResult.Text += sComName;
                txtResult.Text += "\r\n";
                
                string tmpSTR = string.Format("-------------- %f Done!! ---------------", Convert.ToDouble(i+1) / nCMPS);
                Console.WriteLine(tmpSTR);
            }
   
            return -1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            request_one_company("东华软件");
        }
    }
}
