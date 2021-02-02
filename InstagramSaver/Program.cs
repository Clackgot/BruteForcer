using AngleSharp;
using AngleSharp.Html.Parser;
using AngleSharp.Io;
using System;
using System.Threading.Tasks;

namespace InstagramSaver
{
    class Program
    {
        static void Main(string[] args)
        {
            Open().Wait();
        }

        private static async Task Open()
        {
            var config = Configuration.Default
    .WithDefaultCookies()
    .WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync("https://www.kinopoisk.ru/lists/top250/");

            foreach (var name in document.QuerySelectorAll("p.selection-film-item-meta__name"))
            {
                Console.WriteLine(name.TextContent);
            }
        }
    }
}
