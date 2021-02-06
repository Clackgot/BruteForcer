using AngleSharp;
using AngleSharp.Io;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FixPriceEmailChecker
{
    /// <summary>
    /// Информация о почти
    /// <list type="bullet">
    /// <item>None - состояние почты неизвестно</item>
    /// <item>Unregistered - почта ещё не зарегистрирована</item>
    /// <item>Registered - почта зарегистрирована</item>
    /// </list>
    /// </summary>
    public enum EmailInfo
    {
        None,
        Unregistered,
        Registered,
    }

    class Email
    {
        public Email(string name, EmailInfo info)
        {
            Info = info;
            Name = name;
        }

        public EmailInfo Info { get; private set; }
        public string Name { get; private set; }

        public override string ToString()
        {
            switch (Info)
            {
                case EmailInfo.None:
                    return $"Ошибка при проверке {Name}";
                case EmailInfo.Unregistered:
                    return $"{Name} не зарегестирован";
                case EmailInfo.Registered:
                    return $"{Name} зарегестирован";
                default:
                    return null;
            }
        }

        class Checker
        {
            private IBrowsingContext context;
            private List<string> emails = new List<string>();
            public Checker(string emailDataPath)
            {
                var config = Configuration.Default
    .WithDefaultCookies()
    .WithDefaultLoader();
                context = BrowsingContext.New(config);
                loadEmails(emailDataPath);
            }

            private void loadEmails(string path)
            {
                StreamReader sr = new StreamReader(path);//Считыватель потока
                string line;//Одна строка из файла
                while (!sr.EndOfStream)//Пока не конец файла
                {
                    line = sr.ReadLine();//Считать строку
                    emails.Add(line);//Добавить это строку в список паролей
                }
                sr.Close();//Закрыть поток
                Console.WriteLine($"Загружено {emails.Count} паролей");
            }

            public List<Email> Check()
            {
                List<Task<Email>> response = new List<Task<Email>>();
                foreach (var email in emails)
                {
                    response.Add(mailConfirm(email));
                }
                Task.WaitAll(response.ToArray());
                List<Email> mails = new List<Email>();
                return response.ConvertAll(e => e.Result);

            }

            private async Task<Email> mailConfirm(string email)
            {
                var dictonary = new Dictionary<string, string>();
                dictonary.Add(@"mail_confirm", "Y");
                dictonary.Add(@"action", "getCode");
                dictonary.Add(@"email", email);
                var documentRequest = DocumentRequest.PostAsUrlencoded(new Url("https://fix-price.ru/ajax/confirm_mail.php"),
                    dictonary);
                var result = await context.OpenAsync(documentRequest);
                JObject json = JObject.Parse(unicodeEncode(result.Source.Text));
                return new Email(email, getEmailInfo(json));
            }

            private static EmailInfo getEmailInfo(JObject json)
            {
                string status = (string)json["status"];
                switch (status)
                {
                    case "1":
                        return EmailInfo.Unregistered;
                    case "2":
                        return EmailInfo.Registered;
                    default:
                        return EmailInfo.None;
                }

            }
            private static string unicodeEncode(string text)
            {
                var rx = new Regex(@"\\u([0-9A-Z]{4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                return rx.Replace(text, p => new string((char)int.Parse(p.Groups[1].Value, NumberStyles.HexNumber), 1));
            }
        }

        class Program
        {
            static void Main(string[] args)
            {
                Checker checker = new Checker(@"emails\100.txt");
                var results = checker.Check();
                foreach (var result in results)
                {
                    Console.WriteLine(result);
                }
            }
        }
    }
}