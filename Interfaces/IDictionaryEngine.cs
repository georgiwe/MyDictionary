namespace MyDictionary.Interfaces
{
    using System;
    using System.Collections.Generic;

    using MyDictionary.Enums;

    public interface IDictionaryEngine
    {
        IUser User { get; }

        IList<IUser> AllUsers { get; }

        void AddWord(WordType type, WordArticles article, VerbCase vCase, string content, DateTime? added, IList<string> prepos,
            IList<string> definition, string plural, string pratit, string partizip2, string psgpras);

        IWord GetWord(string content);

        IList<IWord> GetAllWords();

        IList<IUser> ReadUsers();

        IList<IWord> GenerateQuiz(
            int num, IEnumerable<WordType> acceptedTypes, QuizType quizType, DateTime? startDate, DateTime? endDate);

        void RemoveWord(IWord word);

        void ReplaceWord(IWord word);

        void SaveWords();

        void LoadWords();
    }
}
