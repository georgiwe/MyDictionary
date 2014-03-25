namespace MyDictionary.Helpers
{
    using System;

    public class QuizException : ApplicationException
    {
        public QuizException(string msg)
            : base(msg)
        {
        }
    }
}
