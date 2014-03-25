namespace MyDictionary.Classes
{
    using System;
    using System.Collections.Generic;

    using MyDictionary.Enums;
    using MyDictionary.Interfaces;

    public class Factory : IFactory
    {
        public virtual IWord CreateWord(
            WordType type, WordArticles article, VerbCase vCase, string content, DateTime? added, IList<string> prepos,
            IList<string> definition, string plural, string pratit, string partizip2, string psgpras)
        {
            return new Word(type, article, vCase, content, added, prepos, definition, plural, pratit, partizip2, psgpras);
        }

        public virtual IUser CreateUser(string name)
        {
            return new User(name);
        }

        public virtual IUser CreateUser(IUser user)
        {
            return this.CreateUser(user.Name);
        }

        public virtual IDictionaryEngine CreateEngine(string username)
        {
            return new Engine(username);
        }

        public virtual IAnnouncer CreateAnnouncer()
        {
            return new WPFannouncer();
        }
    }
}
