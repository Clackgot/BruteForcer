using AngleSharp;
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace FixPriceBruteForce
{


    class Program
    {
        static void Main(string[] args)
        {
            //var fixprice = new FixPriceBF("test.txt");
            //FirstVariant(fixprice);
            var fixprice = new FixPriceBruter();
            Console.WriteLine(fixprice.getNormalizePhoneNumber(79995396765));
        }

        private static void FirstVariant(FixPriceBF fixprice)
        {
            Stopwatch watch = new Stopwatch();//Подсчитываем времяя выполнения
            watch.Start();//Начали
            fixprice.BruteFirst();
            watch.Stop();//Закончили
            Console.WriteLine($"Проверено {fixprice.Passwords.Count} паролей за {watch.Elapsed}");
            Console.WriteLine($"Валидных: {fixprice.Founded}");
            Console.WriteLine($"Валидные в result.txt");

            File.AppendAllText("log1.txt", 
                $"Проверено {fixprice.Passwords.Count} паролей за {watch.Elapsed}\n");//Добавляем результат перебора в лог(нужно переделать в логгер)

            Console.ReadKey();
        }
        private static void SecondVariant(FixPriceBF fixprice)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            fixprice.BruteSecond().Wait();
            watch.Stop();
            Console.WriteLine($"Проверено {fixprice.Passwords.Count} паролей за {watch.Elapsed}");
            Console.WriteLine($"Валидных: {fixprice.Founded}");
            Console.WriteLine($"Валидные в result.txt");

            File.AppendAllText("log2.txt", $"Проверено {fixprice.Passwords.Count} паролей за {watch.Elapsed}\n");

            Console.ReadKey();
        }
    }
}
