using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekzamen02
{
    public class XmlQuestionData
    {
        public int ItemId;
        public string Question;
        public List<string> PossibleOptionsList;
        public List<int> AnswersList;

        public XmlQuestionData(int itemId, string question, List<string> possibleOptions, List<int> answers)
        {
            ItemId = itemId;
            Question = question;
            PossibleOptionsList = new List<string>(possibleOptions);
            AnswersList = new List<int>(answers);
        }
    }
}
