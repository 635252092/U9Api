using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace U9Api.CustSV.Utils
{
    class LogUtil
    {
        public static void WriteDebugInfoLog(string msg) {
            LogUtil.WriteLog("CustLog", "U9Api", "DebugInfo", msg);
        }
        public static void WriteExceptionLog(string msg)
        {
            LogUtil.WriteLog("CustLog", "U9Api", "Version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            LogUtil.WriteLog("CustLog", "U9Api", "Exception", msg);
        }
        /// <summary>
        /// 写log文件
        /// </summary>
        /// <param name="className"></param>
        /// <param name="functionName"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void WriteLog(string className, string functionName, string title, string content)
        {
            string result = string.Format("[{0}]{3}{1}{2}{1}", title, Environment.NewLine, content,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            string rootFile = AppDomain.CurrentDomain.BaseDirectory + "CustLog" + "\\" + className + "\\" + functionName;
            if (!Directory.Exists(rootFile))
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory(rootFile);
            }

            string filePath = rootFile + "\\" + functionName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".html";
            if (!File.Exists(filePath))
            {
                FileStream fs = File.Create(filePath);
                result = "<html><meta charset=\"utf-8\">" + result;
                fs.Close();
            }
            result += "</html>";
            AppendHtmlLog(filePath, result);
            CreateMuLu(rootFile, rootFile + "\\" + functionName + ".html");
        }
        private static void CreateMuLu(string rootFile, string mm)
        {
            DirectoryInfo d = new DirectoryInfo(rootFile);
            FileInfo[] allFile = d.GetFiles();
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><meta charset=\"utf-8\">");
            foreach (FileInfo fi in allFile)
            {
                sb.AppendLine(string.Format("<a href='{0}'>{0}</a>", fi.Name));
            }
            sb.Append("</html>");
            FileStream fs = File.Create(mm);
            fs.Close();
            AppendHtmlLog(mm, sb.ToString());
        }
        /// <summary>
        /// response后追加日志
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        private static void AppendHtmlLog(string path, string result)
        {
            result = result.Replace(Environment.NewLine, "<br>");
            FileStream fs = null;
            string filePath = path;
            //将待写的入数据从字符串转换为字节数组  
            Encoding encoder = Encoding.UTF8;
            //string myResult = SetContent("response", result);

            byte[] bytes = encoder.GetBytes(result);
            try
            {
                fs = File.OpenWrite(filePath);
                //设定书写的开始位置为文件的末尾  
                if (fs.Length - "</html>".Length > 0)
                    fs.Position = fs.Length - "</html>".Length;
                else
                    fs.Position = fs.Length;
                //将待写入内容追加到文件末尾  
                fs.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("文件打开失败{0}", ex.ToString());
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
