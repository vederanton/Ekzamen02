using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ekzamen02
{
    public class Account
    {
        public string Login;
        public string Password;
        public DateTime DateTimeBirthDay;

        public Account(string login, string password)
        {
            Login = login;
            Password = password;
            ReadBirthDayFromFile();
        }

        public void ReadBirthDayFromFile()
        {
            using (FileStream fileStream = new FileStream(Menu.PathAccs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    Regex regexDateTime = new Regex(@"\d{2}.\d{2}.\d{4}");

                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        if (currentLine.Contains($"{Login}") == true)
                        {
                            Match matchBirth = regexDateTime.Match(currentLine);
                            DateTimeBirthDay = Convert.ToDateTime(matchBirth.ToString());
                            break;
                        }
                    }
                }
            }
        }

        public void ChangePassword()
        {
            string newPassword;
            Menu.EnterPassword(out newPassword);
            string oldPassword = Password;
            Password = newPassword;

            SaveAccauntDataInFile(oldPassword, DateTimeBirthDay);

            Console.Clear();
            Console.WriteLine("Пароль успешно обновлен!");
            Console.ReadKey();
        }

        public void ChangeDateTimeBirthDay()
        {
            DateTime newDateTimeBirthDay;
            Menu.EnterBirthDay(out newDateTimeBirthDay);
            DateTime oldBirthDay = DateTimeBirthDay;
            DateTimeBirthDay = newDateTimeBirthDay;

            SaveAccauntDataInFile(Password, oldBirthDay);

            Console.Clear();
            Console.WriteLine("Дата рождения успешно обновлена!");
            Console.ReadKey();
        }

        private void SaveAccauntDataInFile(string oldPassword, DateTime oldDateTime)
        {
            List<string> tmp = new List<string>();
            using (FileStream fileStream = new FileStream(Menu.PathAccs, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                {
                    while (!reader.EndOfStream)
                    {
                        string currentLine = reader.ReadLine();
                        tmp.Add(currentLine);
                    }
                }
                int deleteIndex = Array.FindIndex(tmp.ToArray(), s => s == $"{Login} - {oldPassword} - {oldDateTime.ToString()}");
                tmp.RemoveAt(deleteIndex);
                tmp.Add($"{Login} - {Password} - {DateTimeBirthDay.ToString()}");
            }
            using (StreamWriter writer = new StreamWriter(Menu.PathAccs, false, Encoding.Default))
            {
                foreach (string item in tmp)
                    writer.WriteLine(item);
            }
        }

        public void StartQuiz(int choiceQuiz, AllQuizzez myAllQuizzez, Dictionary<KeyValuePair<string, string>, int> statistics)
        {
            Console.Clear();
            Console.WriteLine($"Начинаем викторину \"{myAllQuizzez.AllQuizzezList[choiceQuiz].QuizName}\".");
            Console.WriteLine("\nДля продолжения нажмите любую кнопку...");
            Console.ReadKey();

            int countRightQuestions = 0;
            int i = 0;
            while (i < myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList.Count)
            {
                Console.Clear();
                Console.WriteLine(myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].Question);
                Console.WriteLine();
                for (int j = 0; j < myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].PossibleOptionsList.Count; j++)
                {
                    Console.WriteLine($"{j} - {myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].PossibleOptionsList[j]}");
                }
                Console.WriteLine("\nВведите номер правильного ответа.");
                Console.WriteLine("Если их несколько, введите номера подряд без пробелов и запятых и нажмите Enter, иначе ответ не засчитается.");
                Console.Write("\nВвод: ");
                string userAnswers = Console.ReadLine();
                int countRightAnswers = 0;
                string tmp = "";
                foreach (char answer in userAnswers)
                {
                    if (userAnswers.Length != myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].AnswersList.Count)
                        break;

                    foreach (int answ in myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].AnswersList)
                        tmp += answ.ToString();

                    if (tmp.Contains(answer))
                        ++countRightAnswers;
                }
                if (countRightAnswers == myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList[i].AnswersList.Count)
                    ++countRightQuestions;
                i++;
            }

            KeyValuePair<string, string> del = new KeyValuePair<string, string>(myAllQuizzez.AllQuizzezList[choiceQuiz].QuizName, Login);
            statistics.Remove(del);
            statistics.Add(new KeyValuePair<string, string>(myAllQuizzez.AllQuizzezList[choiceQuiz].QuizName, Login), countRightQuestions);
            Menu.SaveStatisticsInFile(statistics);

            Console.Clear();
            Console.WriteLine("Викторина окончена!");
            Console.WriteLine($"Количество правильных ответов: {countRightQuestions} из {myAllQuizzez.AllQuizzezList[choiceQuiz].QuizList.Count}.");
            Console.WriteLine();

            ShowMyPlaceInCurrentQuiz(statistics, myAllQuizzez.AllQuizzezList[choiceQuiz].QuizName);
        }

        public void StartMixedQuiz(AllQuizzez myAllQuizzez, Dictionary<KeyValuePair<string, string>, int> statistics)
        {
            Console.Clear();
            Console.WriteLine($"Начинаем смешанную викторину!");
            Console.WriteLine("\nДля продолжения нажмите любую кнопку...");
            Console.ReadKey();

            List<XmlQuestionData> allQuestions = new List<XmlQuestionData>();
            foreach (Quiz quiz in myAllQuizzez.AllQuizzezList)
            {
                foreach (XmlQuestionData question in quiz.QuizList)
                    allQuestions.Add(question);
            }

            Random rnd = new Random();
            for (int l = 0; l < allQuestions.Count; l++)
            {
                int k = rnd.Next(0, l);
                XmlQuestionData value = allQuestions[k];
                allQuestions[k] = allQuestions[l];
                allQuestions[l] = value;
            }
            allQuestions.RemoveRange(20, allQuestions.Count - 20);

            int countRightQuestions = 0;
            int i = 0;
            while (i < allQuestions.Count)
            {
                Console.Clear();
                Console.WriteLine(allQuestions[i].Question);
                Console.WriteLine();

                for (int j = 0; j < allQuestions[i].PossibleOptionsList.Count; j++)
                    Console.WriteLine($"{j} - {allQuestions[i].PossibleOptionsList[j]}");

                Console.WriteLine("\nВведите номер правильного ответа.");
                Console.WriteLine("Если их несколько, введите номера подряд без пробелов и запятых и нажмите Enter, иначе ответ не засчитается.");
                Console.Write("\nВвод: ");
                string userAnswers = Console.ReadLine();
                int countRightAnswers = 0;
                string tmp = "";
                foreach (char answer in userAnswers)
                {
                    if (userAnswers.Length != allQuestions[i].AnswersList.Count)
                        break;

                    foreach (int answ in allQuestions[i].AnswersList)
                        tmp += answ.ToString();

                    if (tmp.Contains(answer))
                        ++countRightAnswers;
                }
                if (countRightAnswers == allQuestions[i].AnswersList.Count)
                    ++countRightQuestions;
                i++;
            }

            KeyValuePair<string, string> del = new KeyValuePair<string, string>("Смешанная викторина", Login);
            statistics.Remove(del);
            statistics.Add(new KeyValuePair<string, string>("Смешанная викторина", Login), countRightQuestions);
            Menu.SaveStatisticsInFile(statistics);

            Console.Clear();
            Console.WriteLine("Викторина окончена!");
            Console.WriteLine($"Количество правильных ответов: {countRightQuestions} из {allQuestions.Count}.");
            Console.WriteLine();

            ShowMyPlaceInCurrentQuiz(statistics, "Смешанная викторина");
        }

        public void ShowMyPlaceInCurrentQuiz(Dictionary<KeyValuePair<string, string>, int> statistics, string quizName)
        {
            var currentQuizStats = from x in statistics
                                   where x.Key.Key.Contains(quizName)
                                   orderby x.Value descending
                                   select x;
            int indexMyPlace = currentQuizStats.ToList().FindIndex(x => x.Key.Value == Login);
            Console.WriteLine($"Ваше место среди всех участников данной викторины: {++indexMyPlace} из {currentQuizStats.Count()}");
            Console.ReadKey();
        }

        public void ViewPastQuizzezResults(Dictionary<KeyValuePair<string, string>, int> statistics)
        {
            Console.Clear();
            Console.WriteLine("Результаты ваших ранее пройденных викторин:");
            Console.WriteLine();
            var MyQuizzezStats = from x in statistics
                                 where x.Key.Value.Contains(Login)
                                 select x;
            if (MyQuizzezStats == null || MyQuizzezStats.Count() == 0)
            {
                Console.Clear();
                Console.WriteLine($"Вы еще не прошли ни одной викторины!");
                Console.ReadKey();
            }
            else
            {
                foreach (var item in MyQuizzezStats)
                    Console.WriteLine($"\"{item.Key.Key}\" - {item.Value} правильных ответов.");
                Console.ReadKey();
            }
        }

        public void Exit()
        {
            Menu newMenu = new Menu();
        }

        public static Language CheckSymbolLanguage(int symbol)
        {
            if ((symbol >= 65 && symbol <= 90) || symbol >= 97 && symbol <= 122)
                return Language.English;
            if (symbol >= 48 && symbol <= 57)
                return Language.Numbers;
            return Language.Unknown;
        }

        public static bool IsEnglishLanguage(string word)
        {
            Language current = Language.English;
            Language tmp;
            for (int i = 0; i < word.Length; i++)
            {
                tmp = CheckSymbolLanguage(word[i]);
                if (current != tmp)
                {
                    Console.WriteLine("Допустимый ввод логина - латинские символы верхнего и нижнего регистра!");
                    Console.ReadKey();
                    return false;
                }
            }
            return true;
        }

        public static bool IsNumbers(string password)
        {
            Language current = Language.Numbers;
            Language tmp;
            for (int i = 0; i < password.Length; i++)
            {
                tmp = CheckSymbolLanguage(password[i]);
                if (current != tmp)
                {
                    Console.WriteLine("Допустимый ввод пароля - цифры от 0 до 9!");
                    Console.ReadKey();
                    return false;
                }
            }
            return true;
        }
    }
}