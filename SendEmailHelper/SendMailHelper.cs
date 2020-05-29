using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Data;
using System.IO;
using System.Web.Configuration;

namespace BranHelpers
{
    public class SendMailHelper
    {
        protected string SmtpServer = WebConfigurationManager.AppSettings["SmtpServer"];
        protected string MailFrom = WebConfigurationManager.AppSettings["MailFrom"];
        protected string MailCC = WebConfigurationManager.AppSettings["MailCC"];
        protected string MailBCC = WebConfigurationManager.AppSettings["MailBCC"];

        /// <summary>
        /// 發送信箱
        /// </summary>
        /// <param name="mailAddress">地址</param>
        /// <param name="subject">標題</param>
        /// <param name="htmlContent">樣板</param>
        public void SendMail(string mailAddress, string subject, string htmlContent)
        {
            MailMessage Message = new MailMessage();

            Message.From = new MailAddress(MailFrom);
            Message.To.Add(new MailAddress(mailAddress));
            if (MailBCC != "") Message.Bcc.Add(MailBCC);
            if (MailCC != "") Message.CC.Add(MailCC);

            Message.Subject = subject;
            Message.Body = htmlContent;
            Message.IsBodyHtml = true;

            SmtpClient stmp = new SmtpClient(SmtpServer);
            stmp.UseDefaultCredentials = false;
            stmp.Send(Message);
        }

        public static string GetSendMailTemplate(string mailTemplatePath)
        {
            //讀取模板檔案
            StreamReader streamReader = new StreamReader(
              mailTemplatePath, Encoding.GetEncoding("UTF-8")
          );
            string Content = streamReader.ReadToEnd();
            streamReader.Close();
            streamReader.Dispose();
            return Content;
        }

        public enum MailMode
        {
            None,
            Left,
            Right
        }
    }
}