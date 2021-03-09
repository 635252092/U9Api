using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace U9Api.CustSV.Utils
{
    class LogUtil
    {
        private static Queue<string> m_Message = new Queue<string>();
        private static bool m_IsRunning;
        private static void Print()
        {
            while (true)
            {
                var messge = string.Empty;
                lock (m_Message)
                {
                    //队列里的条数为0时，跳出
                    if (m_Message.Count == 0)
                    {
                        m_IsRunning = false;
                        return;
                    }
                    //取出队列里的值
                    messge = m_Message.Dequeue();
                    string filePath = AppDomain.CurrentDomain.BaseDirectory + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    FileStream fs = new FileStream(filePath, FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                    sw.Write(messge);
                    sw.Close();
                    fs.Close();
                }
            }
        }
        public static void LogResult2(string sWord)
        {
            lock (m_Message)
            {
                //将日志加入到队列中
                m_Message.Enqueue(sWord);
            }
            //判断打印日志的方法是否在跑
            if (!m_IsRunning)
            {
                m_IsRunning = true;
                System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    Print();
                });
            }
        }

        public static void LogResult2(string className, string functionName, string title, string content)
        {
            lock (m_Message)
            {
                string result = string.Format("[{0}]{3}{1}{2}{1}", title, Environment.NewLine, content, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //将日志加入到队列中
                m_Message.Enqueue(result);
            }
            //判断打印日志的方法是否在跑
            if (!m_IsRunning)
            {
                m_IsRunning = true;
                System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {
                    Print(className, functionName);
                });
            }
        }
        //打印日志

        private static void Print(string className, string functionName)
        {
            while (true)
            {
                var messge = string.Empty;
                lock (m_Message)
                {
                    string rootFile = AppDomain.CurrentDomain.BaseDirectory + "CustLog" + "\\" + className + "\\" + functionName;
                    //队列里的条数为0时，跳出
                    if (m_Message.Count == 0)
                    {
                        //最后一步才去创建目录
                        CreateMuLu(rootFile, rootFile + "\\" + functionName + ".html");
                        m_IsRunning = false;
                        return;
                    }
                    //取出队列里的值
                    string result = m_Message.Dequeue();

                    if (!Directory.Exists(rootFile))
                    {
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(rootFile);
                    }

                    string filePath = rootFile + "\\" + functionName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".html";
                    bool isCreate = false;
                    if (!File.Exists(filePath))
                    {
                        isCreate = true;
                        result = "<html><meta charset=\"utf-8\">" + result;
                    }
                    result += "</html>";
                    result = result.Replace(Environment.NewLine, "<br>");
                    AppendHtmlLog(filePath, result, isCreate);
                }
            }
        }
        public static void WriteDebugInfoLog(string msg) {
            //LogUtil.WriteLog("CustLog", "U9Api", "DebugInfo", msg);
            LogResult2("CustLog", "U9Api", "DebugInfo", msg);
        }
        public static void WriteExceptionLog(string msg)
        {
            //LogUtil.WriteLog("CustLog", "U9Api", "Version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            //LogUtil.WriteLog("CustLog", "U9Api", "Exception", msg);
            LogResult2("CustLog", "U9Api", "Version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            LogResult2("CustLog", "U9Api", "Exception", msg);
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
            bool isCreate = false;
            if (!File.Exists(filePath))
            {
                isCreate = true;
                result = "<html><meta charset=\"utf-8\">" + result;
            }
            result += "</html>";
            result = result.Replace(Environment.NewLine, "<br>");
            AppendHtmlLog(filePath, result, isCreate);
            CreateMuLu(rootFile, rootFile + "\\" + functionName + ".html");
        }
        private static void CreateMuLu(string rootFile, string mm)
        {
            DirectoryInfo d = new DirectoryInfo(rootFile);
            FileInfo[] allFile = d.GetFiles();
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><meta charset=\"utf-8\">");
            IOrderedEnumerable<FileInfo> allFile2 = allFile.OrderByDescending(f=>f.Name);
            foreach (FileInfo fi in allFile2)
            {
                sb.AppendLine(string.Format("<a href='{0}'>{0}</a>", fi.Name));
            }
            sb.Append("</html>");
            string content = sb.ToString().Replace(Environment.NewLine, "<br>");
            //判断目录内容.相同就返回。不同则重新写入
            if (File.Exists(mm)
                && File.ReadAllText(mm).Equals(content))
            {
                return;
            }
            //FileStream fs = File.Create(mm);
            //fs.Close();
            AppendHtmlLog(mm, content, true);
        }
        /// <summary>
        /// response后追加日志
        /// </summary>
        /// <param name="path"></param>
        /// <param name="result"></param>
        private static void AppendHtmlLog(string filePath, string result,bool isCreate=false)
        {
            FileStream fs = null;
            //将待写的入数据从字符串转换为字节数组  
            Encoding encoder = Encoding.UTF8;
            //string myResult = SetContent("response", result);

            byte[] bytes = encoder.GetBytes(result);
            try
            {
                if (isCreate)
                {
                    fs = File.Create(filePath);
                }
                else
                {
                    fs = File.OpenWrite(filePath);
                }
              
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
