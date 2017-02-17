using System;
using System.Data;
using System.IO;
using System.Web;

namespace Tuhu.YeWu.TenGu
{
    public static class ExportImportUtil
    {
        public static void ExportExcel(HttpContextBase httpContext, string name, MemoryStream streamName)
        {
            httpContext.Response.ContentType = "applicationnd.ms-excel";
            name = HttpUtility.UrlEncode(name, System.Text.Encoding.GetEncoding("UTF-8"));
            httpContext.Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", name));
            httpContext.Response.Clear();
            httpContext.Response.BinaryWrite(streamName.ToArray());
            httpContext.Response.End();
        }

        public static void ExportExcel2(HttpContextBase httpContext, string name, MemoryStream streamName)
        {
            httpContext.Response.Clear();
            /*
             * 添加头信息, 为"文件下载/另存为"对话框指定默认文件名
             * 解决了firefox下文件名乱码问题
            */
            string browser = httpContext.Request.Browser.Browser; ;//浏览器名称
            if (browser.ToUpper().IndexOf("IE") >= 0 || browser == "InternetExplorer" || browser.ToLower() == "mozilla")
            {
                name = HttpUtility.UrlEncode(name);
            }
            if (httpContext.Request.UserAgent.ToLower().IndexOf("firefox") > -1)
            {
                httpContext.Response.AddHeader("Content-Disposition", "attachment;filename=\"" + name + "\"");
            }
            else
            {
                httpContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + name);
            }
            //  添加头信息，指定文件大小，让浏览器能够显示下载进度
            //httpContext.Response.AddHeader("Content-Length", file.Length.ToString());
            //  指定返回的是一个不能被客户端读取的流，必须被下载
            httpContext.Response.ContentType = "application/ms-excel";
            //  把文件流发送到客户端
            httpContext.Response.BinaryWrite(streamName.ToArray());
            //  停止页面的执行
            httpContext.Response.End();
        }

        public static void ExportExcel3(HttpContextBase httpContext, string fileName, string[] headText, string[] headValue, DataTable dt)
        {
            string title = "";
            title = fileName + "（" + DateTime.Now.ToString("yyyyMMddHHss") + "）";
            StringWriter sw = new StringWriter();
            string head = "";
            foreach (string str in headText)
                head += str + "\t";
            sw.WriteLine(head);
            string tmp = "";
            foreach (DataRow dr in dt.Rows)
            {
                tmp = "";
                foreach (string tmpStr in headValue)
                    if (tmpStr.ToLower() == "prodtype")
                        tmp += GetType(dr[tmpStr].ToString()) + "\t";
                    else
                        tmp += dr[tmpStr].ToString() + "\t";
                sw.WriteLine(tmp);
            }
            sw.Close();
            httpContext.Response.AddHeader("Content-Disposition", "attachment; filename=" + System.Web.HttpUtility.UrlEncode(title, System.Text.Encoding.UTF8) + ".xls");
            httpContext.Response.ContentType = "application/ms-excel";
            httpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("gb2312");
            httpContext.Response.Write(sw);
            httpContext.Response.End();
        }

        public static void ExportExcel4(string path, string[] headText, string data)
        {
            using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.GetEncoding("gb2312"));
                string head = "";
                foreach (string headTmp in headText)
                    head += headTmp + "\t";
                sw.WriteLine(head);
                string[] strs = data.Split('|');
                string result = "";
                foreach (string str in strs)
                {
                    result = "";
                    foreach (string tmp in str.Split(';'))
                        result += tmp + "\t";
                    sw.WriteLine(result);
                }
                sw.Close();
            }
        }

        public static void ExportExcel5(string path, string[] headText, string data)
        {
            using (FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                System.Text.StringBuilder table = new System.Text.StringBuilder();
                table.Append("<table style='border-right:1px solid #000000;border-bottom:1px solid #000000;'><tr>");
                foreach (string head in headText)
                {
                    table.Append("<td style='border-left:1px solid #000000;border-top:1px solid #000000;'>");
                    table.Append(head); //标格的标题  
                    table.Append("</td>");
                }
                table.Append("</tr>");
                string[] strs = data.Split('|');
                foreach (string str in strs)
                {
                    table.Append("<tr>");
                    foreach (string tmp in str.Split(';'))
                    {
                        table.Append("<td style='vnd.ms-excel.numberformat:@;border-left:1px solid #000000;border-top:1px solid #000000;'>");
                        table.Append(tmp);
                        table.Append("</td>");
                    }
                    table.Append("</tr>");
                }
                table.Append("</table>");
                sw.Write(table);
                sw.Close();
            }
        }

        public static void DownloadFile(HttpContextBase httpContext,string filePath)
        {
            string fileName = "VoucherItem_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";//客户端保存的文件名
            FileInfo fileInfo = new FileInfo(filePath);
            httpContext.Response.Clear();
            httpContext.Response.ClearContent();
            httpContext.Response.ClearHeaders();
            httpContext.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            httpContext.Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            httpContext.Response.AddHeader("Content-Transfer-Encoding", "binary");
            httpContext.Response.ContentType = "application/octet-stream";
            httpContext.Response.ContentEncoding = System.Text.Encoding.GetEncoding("gb2312");
            httpContext.Response.WriteFile(fileInfo.FullName);
            httpContext.Response.Flush();
            httpContext.Response.End();
        }

        private static string GetType(string prodtype) 
        {
            string result = "";
            switch (prodtype)
            {
                case "Maintenance":
                    result = "保养";
                    break;
                case "Others":
                    result = "其他";
                    break;
                case "Tire":
                    result = "轮胎";
                    break;
                case "Beauty":
                    result = "美容";
                    break;
            }
            return result;
        }
    }
}