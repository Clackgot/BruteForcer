using AngleSharp;
using AngleSharp.Io;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FixPriceEmailChecker
{


    /// <summary>
    /// Информация о наличии почты в базе фикспрайса
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

    /// <summary>
    /// Результат проверки емейла на валидность
    /// </summary>
    class Email
    {

        public Email(string name, EmailInfo info)
        {
            Info = info;
            Name = name;
        }

        public EmailInfo Info { get; private set; }
        /// <summary>
        /// Сам email
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Перегрузка, для вывода в консоль
        /// </summary>
        /// <returns>Отформатированная информация в виде строки</returns>
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


    }
    /// <summary>
    /// Асинхронно проверяет почты <see cref="emails"/> на то, что те зарегестрированы на сайте
    /// </summary>
    class Checker
    {
        private int registeredEmailCount = 0;
        
        private IBrowsingContext context;//Контекст запросов(через эту штуку посылаются запросы, хранит куки)
        private List<string> emails = new List<string>();//Email'ы из файла
        public static List<string> registeredEmails = new List<string>();//Email'ы из файла
        /// <summary>
        /// Инициализация чекера почт
        /// </summary>
        /// <param name="emailDataPath">Путь к файлу с паролями</param>
        public Checker(string emailDataPath)
        {
            updateContext();
            loadEmails(emailDataPath);
        }

        public void updateContext()
        {
            var config = Configuration.Default
.WithDefaultCookies()
.WithDefaultLoader();
            context = BrowsingContext.New(config);
        }

        /// <summary>
        /// Загружает email'ы из файла в <see cref="emails"/>
        /// </summary>
        /// <param name="path"></param>
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
            Console.WriteLine($"Загружено {emails.Count} емэйлов");
        }

        /// <summary>
        /// Возвращает список проверенных почт
        /// </summary>
        /// <returns></returns>
        public List<Email> Check()
        {
            List<Task<Email>> response = new List<Task<Email>>();

            for (int i = 0; i < emails.Count; i++)
            {
                response.Add(mailConfirm(emails[i]));
            }
            Task.WaitAll(response.ToArray());
            List<Email> mails = new List<Email>();
            return response.ConvertAll(e => e.Result);

        }



        private static int checkedCounter = 0;
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
            var checkedEmail = new Email(email, getEmailInfo(json));
            if(checkedEmail.Info == EmailInfo.Registered)
            {
                registeredEmails.Add(email);
                registeredEmailCount++;
            }
            checkedCounter++;
            Console.WriteLine($"{checkedCounter}/{emails.Count} валидны:{registeredEmailCount}");
            return checkedEmail;
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
            Checker checker = new Checker(@"emails\1000.txt");
            Console.CancelKeyPress += Console_CancelKeyPress;
            checker.Check();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            string path = "valid.txt";
            if(File.Exists(path))File.Delete(path);
            File.AppendAllLines(path, Checker.registeredEmails);
        }
    }
}