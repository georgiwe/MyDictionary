namespace MyDictionary.Interfaces
{
    using System;
    using System.Collections.Generic;

    using MyDictionary.Enums;

    public interface IFactory
    {
        IWord CreateWord(WordType type, WordArticles article, VerbCase vCase, string content, DateTime? added, IList<string> prepos,
            IList<string> definition, string plural, string pratit, string partizip2, string psgpras);

        IUser CreateUser(string name);

        IUser CreateUser(IUser user);

        IDictionaryEngine CreateEngine(string userName);

        IAnnouncer CreateAnnouncer();
    }
}
