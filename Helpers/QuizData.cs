namespace MyDictionary.Helpers
{
    using System;
    using System.Collections.Generic;

    using MyDictionary.Helpers;
    using MyDictionary.Enums;
    using MyDictionary.Interfaces;

    public class QuizData
    {
        private const string whenTrue = "Yes";
        private const string whenFalse = "No";

        private bool correctness;

        public QuizData(int qNum, string question, string corrAnswer, string answer, bool correctness)
        {
            this.Question = question;
            this.Answer = answer;
            this.correctness = correctness;
            this.Correct = corrAnswer;
            this.QuestionNumber = qNum.ToString();
        }

        public string QuestionNumber { get; private set; }

        public string Question { get; private set; }

        public string Answer { get; private set; }

        public string Correct { get; private set; }

        public string Correctness
        {
            get
            {
                return this.correctness ? whenTrue : whenFalse;
            }
        }
    }
}
