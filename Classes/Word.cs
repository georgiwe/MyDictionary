namespace MyDictionary.Classes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using MyDictionary.Enums;
    using MyDictionary.Interfaces;

    public class Word : IWord
    {
        private string content;
        private IList<string> definition;
        private string plural;
        private string pratit;
        private string partizip2;
        private string psgpras;
        private IList<string> prepositions;
        private DateTime? dateAdded;

        public Word(WordType type, WordArticles article, VerbCase vCase, string content, DateTime? added, IList<string> prepos,
            IList<string> definition, string plural, string pratit, string partizip2, string psgpras)
        {
            this.Content = content;
            this.Definition = definition;
            this.Plural = plural;
            this.Type = type;
            this.Article = article;
            this.Pratitium = pratit;
            this.Partizip2 = partizip2;
            this.PSgPras = psgpras;
            this.DateAdded = added;
            this.Prepositions = prepos;
            this.Case = vCase;
        }

        public IList<string> Prepositions
        {
            get
            {
                return new List<string>(this.prepositions);
            }

            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("List of propositions cannot be null.");
                }

                this.prepositions = value;
            }
        }

        public DateTime? DateAdded
        {
            get
            {
                return new DateTime?(this.dateAdded.Value);
            }

            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Date added cannot be null.");
                }

                this.dateAdded = value;
            }
        }

        public VerbCase Case { get; private set; }

        public WordType Type { get; private set; }

        public WordArticles Article { get; private set; }

        public string Content
        {
            get
            {
                return this.content;
            }

            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(
                        "Word cannot be null or whitespeace.");
                }

                this.content = value;
            }
        }

        public IList<string> Definition
        {
            get
            {
                return new List<string>(this.definition);
            }

            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(
                        "Word definition cannot be null");
                }

                if (value.Count < 1)
                {
                    throw new ArgumentException(
                        "Word must have at least one definition");
                }

                this.definition = value;
            }
        }

        public string Plural
        {
            get
            {
                return this.plural;
            }

            private set
            {
                if (string.IsNullOrWhiteSpace(value) && this.Type == WordType.Noun)
                {
                    throw new ArgumentNullException(
                        "Word plural form cannot be null or whitespace.");
                }

                this.plural = value;
            }
        }

        public string Pratitium
        {
            get
            {
                return this.pratit;
            }

            private set
            {
                if (this.Type == WordType.Verb &&
                    string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("Pratit can only be null of word is not a verb.");
                }

                this.pratit = value;
            }
        }

        public string Partizip2
        {
            get
            {
                return this.partizip2;
            }

            private set
            {
                if (this.Type == WordType.Verb &&
                    string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(
                        "Partizip2 can only be null of word is not a verb.");
                }

                this.partizip2 = value;
            }
        }

        public string PSgPras
        {
            get
            {
                return this.psgpras;
            }

            private set
            {
                if (this.Type == WordType.Verb &&
                    string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(
                        "PSgPras can only be null of word is not a verb.");
                }

                this.psgpras = value;
            }
        }

        public bool Equals(IWord other)
        {
            if (other == null || 
                this.Content != other.Content)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (this.Type == WordType.Noun)
            {
                sb.Append(this.Article.ToString().ToLower());
                sb.Append(" ");
            }

            sb.Append(this.Content);

            if (this.Type == WordType.Noun)
            {
                sb.Append(" ");
                sb.Append(this.Plural);
            }

            sb.AppendLine();
            sb.Append(this.Type.ToString().ToLower());

            if (this.Type == WordType.Verb)
            {
                sb.Append(" " + this.Case);
            }

            sb.AppendLine();
            sb.AppendLine();

            if (this.Type == WordType.Verb &&
                this.Prepositions.Where(p => p != string.Empty).Count() > 0)
            {
                sb.AppendLine("Prepositions:");

                foreach (var preposition in this.Prepositions)
                {
                    sb.AppendLine("- " + preposition);
                }

                sb.AppendLine();
            }

            sb.AppendLine("Definitions:");
            sb.Append("- ");
            sb.AppendLine(string.Join("\n- ", this.Definition));

            if (this.Type == WordType.Verb)
            {
                sb.AppendLine();
                sb.AppendLine("Verb forms:");
                sb.Append(" - 3.P.Sg.Präsens: ");
                sb.AppendLine(this.PSgPras);
                sb.Append(" - Präteritum: ");
                sb.AppendLine(this.Pratitium);
                sb.Append(" - Partizip II: ");
                sb.AppendLine(this.Partizip2);
            }

            return sb.ToString();
        }
    }
}
