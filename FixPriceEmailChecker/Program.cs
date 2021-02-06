using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FixPriceEmailChecker
{


    enum mailConfirmResult
    {
        Unregistered,
        Registered,
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

        public void Check()
        {
            List<Task<IDocument>> response = new List<Task<IDocument>>();
            foreach (var email in emails)
            {
                response.Add(mailConfirm(email));
            }
            Task.WaitAll(response.ToArray());
        }

        private async Task<IDocument> mailConfirm(string email)
        {
            var dictonary = new Dictionary<string, string>();
            dictonary.Add(@"mail_confirm", "Y");
            dictonary.Add(@"action", "getCode");
            dictonary.Add(@"email", email);
            var documentRequest = DocumentRequest.PostAsUrlencoded(new Url("https://fix-price.ru/ajax/confirm_mail.php"),
                dictonary);
            var result = await context.OpenAsync(documentRequest);
            Console.WriteLine(unicodeEncode(result.Source.Text));
            return result;
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
            Checker checker = new Checker("emails.txt");
            checker.Check();
        }
    }
}
