namespace MyDictionary.Helpers
{
    using System;
    using System.Text.RegularExpressions;

    using MyDictionary.Enums;

    public static class Helper
    {
        public static string SignedInMessage = "         Signed in as \"{0}\"";

        public static QuizType ParseQuizType(string type)
        {
            switch (type)
            {
                case "Foreign to Native Language Word Quiz":
                    return QuizType.ForeignToNative;

                case "Native to Foreign Language Word Quiz":
                    return QuizType.NativeToForeign;

                case "Article Quiz":
                    return QuizType.Article;

                case "Preposition Quiz":
                    return QuizType.Preposition;

                case "Verb Past Tenses Quiz":
                    return QuizType.VerbPastTenses;

                default:
                    throw new ArgumentOutOfRangeException("Unexpected quiz type. Cannot parse.");
            }
        }

        public static string QuizTypeEnumToString(QuizType type)
        {
            switch (type)
            {
                case QuizType.ForeignToNative:
                    return "Foreign to Native Language Word Quiz";

                case QuizType.VerbPastTenses:
                    return "Verb Past Tenses Quiz";

                case QuizType.Article:
                    return "Article Quiz";

                case QuizType.Preposition:
                    return "Preposition Quiz";

                case QuizType.NativeToForeign:
                    return "Native to Foreign Language Word Quiz";

                default:
                    throw new ArgumentOutOfRangeException("Unexpected quiz type. Cannot parse.");
            }
        }

        public static string CutMultipleSpaces(string str)
        {
            return Regex.Replace(str, @"\s+", " ");
        }
    }
}
