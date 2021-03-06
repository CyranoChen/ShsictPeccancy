﻿using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Shsict.Peccancy.Service.DbHelper;
using Shsict.Peccancy.Service.Extension;
using Shsict.Peccancy.Service.Logger;
using Shsict.Peccancy.Service.Scheduler;

namespace Shsict.Peccancy.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IAppLog log = new AppLog();

                using (IRepository repo = new Repository())
                {
                    var schedulers = repo.All<Schedule>().FindAll(x => x.IsActive && x.Seconds > 0);

                    if (schedulers.Count > 0)
                    {
                        var warningScheduler =
                            schedulers.FindAll(x => DateTime.Now > x.LastCompletedTime.AddSeconds(x.Seconds + 300));

                        System.Console.WriteLine("Scheduler Warning Count: " + warningScheduler.Count);

                        if (warningScheduler.Count > 0)
                        {
                            var content = new StringBuilder();
                            foreach (var s in warningScheduler)
                            {
                                content.Append(new
                                {
                                    ScheduleName = s.ScheduleKey,
                                    LastCompletedTime = s.LastCompletedTime.ToString("yyyyMMdd HH:mm:ss"),
                                    Interval = s.Seconds
                                }.ToJson());
                                content.Append("\r\n");
                            }

                            System.Console.WriteLine(content.ToString());

                            SendEmail(ConfigGlobal.AdminEmail, "外集卡违章数据同步停止提醒", content.ToString());

                            log.Warn("Send the mail of Scheduler Stopping Reminder");
                        }
                    }
                }

                log.Info("Examine Scheduler executed");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        private static void SendEmail(string[] to, string subject, string content)
        {
            var mail = new MailMessage();

            foreach (var t in to)
            {
                mail.To.Add(t);
            }

            // 如果cyrano在管理员名单中，则抄送给cyrano
            if (ConfigGlobal.SystemAdmin.Any(x => x.Equals("cyrano", StringComparison.OrdinalIgnoreCase)))
            {
                mail.CC.Add("cyrano@arsenalcn.com");
            }

            /* 上面3个参数分别是发件人地址（可以随便写），发件人姓名，编码*/
            mail.From = new MailAddress("SMS@shsict.com", "外集卡违章数据同步监控", Encoding.UTF8);

            mail.Subject = subject;//邮件标题 
            mail.SubjectEncoding = Encoding.UTF8;//邮件标题编码 

            mail.Body = "以下内容为系统自动发送，请勿直接回复，谢谢。\r\n\r\n";//邮件内容 
            mail.Body += content;
            mail.BodyEncoding = Encoding.UTF8;//邮件内容编码 

            mail.IsBodyHtml = false;//是否是HTML邮件 
            mail.Priority = MailPriority.High;//邮件优先级 

            var client = new SmtpClient
            {
                Credentials = new NetworkCredential("SMS@shsict.com", "Sms@2018"),
                Host = "lotus.shsict.com"
            };

            //object userState = mail;

            try
            {
                client.Send(mail);
            }
            catch (SmtpException ex)
            {
                IAppLog log = new AppLog();
                log.Error(ex);
            }
        }

    }
}
