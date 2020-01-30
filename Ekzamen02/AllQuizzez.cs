using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ekzamen02
{
    public class AllQuizzez
    {
        public static string Path;
        public List<Quiz> AllQuizzezList;

        static AllQuizzez()
        {
            Path = "quizes.xml";
        }

        public AllQuizzez()
        {
            AllQuizzezList = new List<Quiz>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(Path);
            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode quiz in xRoot)
            {
                XmlNode quizAttr = quiz.Attributes.GetNamedItem("id");
                int quizAttrId = Convert.ToInt32(quizAttr.Value); // получили id викторины
                quizAttr = quiz.Attributes.GetNamedItem("name");
                string quizAttrName = quizAttr.Value; // получили название викторины

                List<XmlQuestionData> questionList = new List<XmlQuestionData>();

                foreach (XmlNode item in quiz)
                {
                    XmlNode itemAttr = item.Attributes.GetNamedItem("id");
                    int itemAttrId = Convert.ToInt32(itemAttr.Value); // получили id вопроса   

                    string question = default;
                    List<string> options = new List<string>();
                    List<int> answers = new List<int>();

                    foreach (XmlNode childItem in item.ChildNodes)
                    {

                        if (childItem.Name == "question")
                            question = childItem.InnerText; // получили вопрос

                        if (childItem.Name == "possible")
                            options.Add(childItem.InnerText); // получили вариант ответа и добавили в массив

                        if (childItem.Name == "answer")
                            answers.Add(Convert.ToInt32(childItem.InnerText)); // получили номер правильного ответа и добавили в массив     
                    }

                    XmlQuestionData currentQuestion = new XmlQuestionData(itemAttrId, question, options, answers);
                    questionList.Add(currentQuestion);
                }

                Quiz currentQuiz = new Quiz(quizAttrId, quizAttrName, questionList);
                AllQuizzezList.Add(currentQuiz); // добавили все викторины со всем содержимым в массив
            }
        }
    }
}
