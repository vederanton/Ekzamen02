using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ekzamen02
{
    public enum Language
    {
        Unknown = 0,
        English,
        Numbers
    }

    public class Menu
    {
        public Account MyAccount;
        public AllQuizzez MyAllQuizzez;
        public static string PathAccs;
        public Dictionary<string, string> Accs;
        public static string PathStats;
        public Dictionary<KeyValuePair<string, string>, int> Statistics;

        static Menu()
        {
            PathAccs = "Accs.txt";
            PathStats = "stats.txt";
        }

        public Menu()
        {
            MyAllQuizzez = new AllQuizzez();
            Accs = new Dictionary<string, string>();
            Statistics = new Dictionary<KeyValuePair<string, string>, int>();
            using (FileStream fileStream = new FileStream(PathAccs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    Regex regexAccs = new Regex(@"[A-z](\w*)");
                    Regex regexPasswords = new Regex(@"[0-9](\w*)");

                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        Match matchAccs = regexAccs.Match(currentLine);
                        Match matchPasswords = regexPasswords.Match(currentLine);
                        Accs.Add(matchAccs.Value, matchPasswords.Value);
                    }
                }
            }

            using (FileStream fileStream = new FileStream(PathStats, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    Regex regexQuizzez = new Regex("\".*?\"");
                    Regex regexLogins = new Regex(@"[A-z](\w*)");
                    Regex regexTryAnswers = new Regex(@"[0-9](\w*)");

                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        Match matchQuizzez = regexQuizzez.Match(currentLine);
                        Match matchLogins = regexLogins.Match(currentLine);
                        Match matchTryAnswers = regexTryAnswers.Match(currentLine);
                        Statistics.Add(new KeyValuePair<string, string>(matchQuizzez.Value.Trim('"'), matchLogins.Value), Convert.ToInt32(matchTryAnswers.Value));
                    }
                }
            }
            ShowLoginOrRegister(); // внутри будет инициализирован MyAccount
        }

        public bool EnterLogin(out string login) // проверяет на латиницу, на существование такого же и иниц. логин
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Введите ваш логин: ");
                login = Console.ReadLine();
                if (Account.IsEnglishLanguage(login) == false)
                    continue;
                if (Accs.ContainsKey(login) == false)
                {
                    Console.Clear();
                    Console.WriteLine("Данный логин не зарегистрирован!");
                    Console.ReadKey();
                    return false;
                }
                break;
            }
            while (true);
            return true;
        }

        public static void EnterPassword(out string pass) // проверяет на цифры и иниц. пароль
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Введите пароль цифрами от 0 до 9: ");
                pass = Console.ReadLine();
                if (Account.IsNumbers(pass) == false)
                    continue;
                break;
            }
            while (true);
        }

        public static void EnterBirthDay(out DateTime dateTime)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("Введите Вашу дату рождения в формате дд.мм.гггг : ");
                try
                {
                    dateTime = Convert.ToDateTime(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("Неправильный формат!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);
        }

        public void Register()
        {
            string login;
            do
            {
                Console.Clear();
                Console.WriteLine("Введите Логин латинскими символами верхнего и(или) нижнего регистра: ");
                login = Console.ReadLine();
                if (Account.IsEnglishLanguage(login) == false)
                    continue;
                if (Accs.ContainsKey(login) == true)
                {
                    Console.Clear();
                    Console.WriteLine("Данный логин уже зарегистрирован!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);

            string password;
            EnterPassword(out password);
            Accs.Add(login, password);

            DateTime dateTime;
            EnterBirthDay(out dateTime);
            SaveNewAccInFile(dateTime);

            MyAccount = new Account(login, password);

            Console.Clear();
            Console.WriteLine("Регистрация прошла успешно!");
            Console.ReadKey();
        }

        public void SaveNewAccInFile(DateTime dateTime)
        {
            using (StreamWriter writer = new StreamWriter(PathAccs, false, Encoding.Default))
            {
                foreach (var item in Accs)
                    writer.WriteLine($"{item.Key} - {item.Value} - {dateTime.ToString()}");
            }
        }

        public bool IsTruePassword(string login, string pass)
        {
            if (Accs[login] == pass)
                return true;
            else
            {
                Console.Clear();
                Console.WriteLine("Введен неверный пароль!");
                Console.ReadKey();
                return false;
            }
        }

        public static void SaveStatisticsInFile(Dictionary<KeyValuePair<string, string>, int> statistics)
        {
            using (StreamWriter writer = new StreamWriter(PathStats, false, Encoding.Default))
            {
                foreach (var item in statistics)
                    writer.WriteLine($"\"{item.Key.Key}\" - {item.Key.Value} - {item.Value}");
            }
        }

        public void ViewTop20()
        {
            int choiceQuiz;
            string quizNameToView;
            do
            {
                Console.Clear();
                Console.WriteLine("Выберите викторину для просмотра ТОП-20 лучших прошедших ее пользователей: ");
                Console.WriteLine();
                List<int> quizIds = new List<int>();
                foreach (Quiz quiz in MyAllQuizzez.AllQuizzezList)
                {
                    Console.WriteLine($"{quiz.QuizId} - {quiz.QuizName}");
                    quizIds.Add(Convert.ToInt32(quiz.QuizId));
                }
                Console.WriteLine("\nДля просмотра ТОП-20 смешанных викторин введите слово \"микс\".");
                Console.Write("\nВвод: ");
                try
                {
                    string choice = Console.ReadLine();
                    if (choice == "микс")
                    {
                        quizNameToView = "Смешанная викторина";
                        break;
                    }
                    choiceQuiz = Convert.ToInt32(choice);
                }
                catch
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                if (quizIds.Contains(choiceQuiz) == false)
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                quizNameToView = MyAllQuizzez.AllQuizzezList[choiceQuiz].QuizName;
                break;
            }
            while (true);

            var statsToShow = from x in Statistics
                              where x.Key.Key.Contains(quizNameToView)
                              orderby x.Value descending
                              select x;
            if (statsToShow == null || statsToShow.Count() == 0)
            {
                Console.Clear();
                Console.WriteLine($"Викторину \"{quizNameToView}\" еще никто не проходил!");
                Console.ReadKey();
            }
            else
            {
                var results = statsToShow.Take(20);
                Console.Clear();
                Console.WriteLine($"ТОП-20 участников викторины \"{quizNameToView}\":");
                Console.WriteLine();
                int i = 1;
                foreach (var item in results)
                    Console.WriteLine($"{i++} место: {item.Key.Value} ({item.Value} правильных ответов).");
                Console.ReadKey();
            }
        }

        public void ShowLoginOrRegister() // первый вызываемый метод меню.
        {
            char choice;
            do
            {
                Console.Clear();
                Console.WriteLine("Викторина v1.0");
                Console.WriteLine("\n1 - Регистрация нового пользователя");
                Console.WriteLine("2 - Вход в аккаунт");
                Console.WriteLine("\n3 - Выход из программы");
                choice = Console.ReadKey().KeyChar;
                switch (choice)
                {
                    case '1':
                        Register();
                        continue;
                    case '2':
                        string login;
                        if (EnterLogin(out login) == false)
                        {
                            choice = 'q';
                            continue;
                        }
                        else
                        {
                            string pass;
                            EnterPassword(out pass);
                            if (IsTruePassword(login, pass) == false)
                            {
                                choice = 'q';
                                continue;
                            }
                            MyAccount = new Account(login, pass);
                        }
                        break;
                    case '3':
                        break;
                    default:
                        continue;
                }
            }
            while (choice != '1' && choice != '2' && choice != '3');

            if (choice != '3')
                ShowMainMenu();
            else
                Console.Clear();
        }

        public void ShowMainMenu()
        {
            char choice;
            do
            {
                Console.Clear();
                Console.WriteLine($"Вход выполнен, {MyAccount.Login}!");
                Console.WriteLine("\n1 - Стартовать новую викторину");
                Console.WriteLine("2 - Посмотреть результаты своих прошлых викторин");
                Console.WriteLine("3 - Посмотреть Топ-20 по конкретной викторине");
                Console.WriteLine("\n4 - Поменять пароль");
                Console.WriteLine("5 - Изменить дату рождения");
                Console.WriteLine("\n6 - Выход из аккаунта");
                Console.WriteLine("7 - Выход из программы");
                choice = Console.ReadKey().KeyChar;
                switch (choice)
                {
                    case '1':
                        ShowMenuAllQuizzez();
                        continue;
                    case '2':
                        MyAccount.ViewPastQuizzezResults(Statistics);
                        continue;
                    case '3':
                        ViewTop20();
                        continue;
                    case '4':
                        MyAccount.ChangePassword();
                        continue;
                    case '5':
                        MyAccount.ChangeDateTimeBirthDay();
                        continue;
                    case '6':
                        MyAccount.Exit();
                        break;
                    case '7':
                        break;
                    default:
                        continue;
                }
            }
            while (choice != '7' && choice != '6');
            Console.Clear();
        }

        public void ShowMenuAllQuizzez()
        {
            int choiceQuiz;
            do
            {
                Console.Clear();
                Console.WriteLine("Выберите викторину для старта, нажав соответствующий ей номер: ");
                Console.WriteLine();
                List<int> quizIds = new List<int>();
                foreach (Quiz quiz in MyAllQuizzez.AllQuizzezList)
                {
                    Console.WriteLine($"{quiz.QuizId} - {quiz.QuizName}");
                    quizIds.Add(Convert.ToInt32(quiz.QuizId));
                }
                Console.WriteLine("\nДля старта викторины со случайными вопросами из всех викторин введите слово \"микс\".");
                Console.Write("\nВвод: ");
                try
                {
                    string choice = Console.ReadLine();
                    if (choice == "микс")
                    {
                        choiceQuiz = -1;
                        break;
                    }
                    choiceQuiz = Convert.ToInt32(choice);
                }
                catch
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                if (quizIds.Contains(choiceQuiz) == false)
                {
                    Console.WriteLine("\nНеверный ввод!");
                    Console.ReadKey();
                    continue;
                }
                break;
            }
            while (true);

            if (choiceQuiz == -1)
                MyAccount.StartMixedQuiz(MyAllQuizzez, Statistics);
            else
                MyAccount.StartQuiz(choiceQuiz, MyAllQuizzez, Statistics);
        }
    }
}
