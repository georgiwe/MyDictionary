namespace MyDictionary.Interfaces
{
    using System;
    using System.Collections.Generic;

    using MyDictionary.Enums;

    public interface IWord : IEquatable<IWord>
    {
        string Content { get; }

        IList<string> Definition { get; }

        string Plural { get; }

        WordType Type { get; }

        VerbCase Case { get; }

        WordArticles Article { get; }

        string Pratitium { get; }

        string Partizip2 { get; }

        string PSgPras { get; }

        IList<string> Prepositions { get; }

        DateTime? DateAdded { get; }

        string ToString();
    }
}
