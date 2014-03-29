namespace MyDictionary.Classes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using MyDictionary.Enums;
    using MyDictionary.Helpers;
    using MyDictionary.Interfaces;

    public class Engine : IDictionaryEngine
    {
        private const char EndOfWord = '%';
        private const char WordAnchor = '$';
        private const string UsernamesFilePath = "usernames.txt";
        private readonly string saveLoadWordsFilePath;

        private IUser user;
        private Random rnd;
        private IFactory factory;
        private IList<IWord> words;
        private IAnnouncer announcer;
        private IList<IUser> allUsers;

        public Engine(string userName)
        {
            this.rnd = new Random();
            this.factory = new Factory();
            this.words = new List<IWord>();
            this.allUsers = new List<IUser>();
            this.allUsers = this.ReadUsers();
            this.User = this.GetUserByUsername(userName);
            this.Announcer = this.factory.CreateAnnouncer();

            this.saveLoadWordsFilePath = this.User.Name + @"_words.txt";
            this.LoadWords();
        }

        public virtual IFactory Factory
        {
            get { return this.factory; }
        }

        public virtual IUser User
        {
            get
            {
                return (IUser)this.user.Clone();
            }

            private set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("User cannot be null.");
                }

                this.user = value;
            }
        }

        public virtual IList<IUser> AllUsers
        {
            get
            {
                return new List<IUser>(this.allUsers);
            }

            private set
            {
                if (value == null)
                {
                    throw new ArgumentException("User cannot be null");
                }

                this.allUsers = value;
            }
        }

        private IAnnouncer Announcer
        {
            get
            {
                return this.announcer;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Announcer cannot be null.");
                }

                this.announcer = value;
            }
        }

        public virtual void AddWord(
            WordType type, WordArticles article, VerbCase vCase, string content, DateTime? added, IList<string> prepos,
            IList<string> definition, string plural, string pratit, string partizip2, string psgpras)
        {
            IWord wordToAdd = this.factory.CreateWord(
                type, article, vCase, content, added, prepos, definition, plural, pratit, partizip2, psgpras);

            if (this.words.Contains(wordToAdd))
            {
                throw new WordExistsException(
                    "Word already exists in the dictionary.");
            }

            this.words.Add(wordToAdd);
            this.announcer.WordAdded(wordToAdd);
        }

        public virtual IWord GetWord(string content)
        {
            content = content.ToLower();

            return this.words.FirstOrDefault(w => w.Content.ToLower() == content);
        }

        public virtual IList<IWord> GetAllWords()
        {
            return new List<IWord>(this.words);
        }

        public virtual IList<IWord> GenerateQuiz(
            int quizLength, IEnumerable<WordType> acceptedTypes, QuizType quizType, DateTime? startDate, DateTime? endDate)
        {
            if (quizLength < 0)
            {
                throw new ArgumentException(
                    "Number of words cannot be negative");
            }

            var acceptedWordsIEnum = this.words
                .Where(w =>
                    w.DateAdded >= startDate &&
                    w.DateAdded <= endDate)
                .Where(w => acceptedTypes.Contains(w.Type));

            if (quizType == QuizType.Preposition)
            {
                acceptedWordsIEnum = acceptedWordsIEnum
                    .Where(w => w.Prepositions.Where(p => p != string.Empty).Count() > 0);
            }

            var acceptedWordsList = acceptedWordsIEnum.ToList();

            int acceptedWordsCount = acceptedWordsList.Count();

            if (quizLength > acceptedWordsCount)
            {
                quizLength = acceptedWordsCount;
            }

            var result = new List<IWord>();

            for (int i = 0; i < quizLength; i++)
            {
                int ind = this.rnd.Next() % acceptedWordsCount;

                while (result.Contains(acceptedWordsList[ind]))
                {
                    ind = this.rnd.Next() % acceptedWordsCount;
                }

                result.Add(acceptedWordsList[ind]);
            }

            return result;
        }

        public virtual void RemoveWord(IWord word)
        {
            if (this.words.Contains(word) == true)
            {
                this.words.Remove(word);
                this.announcer.WordRemoved(word);

                return;
            }

            this.announcer.NoWordsWereRemoved(word);
        }

        public virtual void ReplaceWord(IWord wordToReplace, IWord replacement)
        {
            if (this.words.Contains(wordToReplace) == false)
            {
                throw new ArgumentException(
                    string.Format(
                    "Cannot replace word {0} - word not in dict.", wordToReplace.Content));
            }

            int index = this.words.IndexOf(wordToReplace);

            this.words[index] = replacement;
        }

        public virtual void SaveWords()
        {
            if (this.words.Count == 0) return;

            var sw = new StreamWriter(this.saveLoadWordsFilePath);

            foreach (IWord word in this.words)
            {
                sw.WriteLine(word.Content);
                sw.WriteLine(word.Type.ToString());
                sw.WriteLine(word.Plural);
                sw.WriteLine(string.Join("|", word.Definition));
                sw.WriteLine(word.Article);
                sw.WriteLine(word.Pratitium);
                sw.WriteLine(word.Partizip2);
                sw.WriteLine(word.PSgPras);
                sw.WriteLine(word.DateAdded);
                sw.WriteLine(string.Join(" ", word.Prepositions));
                sw.WriteLine(word.Case);
                sw.WriteLine(EndOfWord);
            }

            sw.Dispose();
        }

        public virtual void LoadWords()
        {
            if (File.Exists(this.saveLoadWordsFilePath) == false)
            {
                return;
            }

            var sr = new StreamReader(this.saveLoadWordsFilePath);

            var sb = new StringBuilder();

            string line = string.Empty;

            while ((line = sr.ReadLine()) != null)
            {
                sb.Append(line + (line == "%" ? string.Empty : WordAnchor.ToString()));
            }

            string[] allWordsInfo = sb.ToString().Split(new char[] { EndOfWord }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var wordInfo in allWordsInfo)
            {
                string[] info = wordInfo.Split(new char[] { WordAnchor });

                if (info[0] == string.Empty)
                {
                    break;
                }

                WordType type = (WordType)Enum.Parse(typeof(WordType), info[1]);
                WordArticles article = (WordArticles)Enum.Parse(typeof(WordArticles), info[4]);
                string content = info[0];
                IList<string> definition = new List<string>(info[3].Split('|'));
                string plural = info[2];
                string pratit = info[5];
                string partizip2 = info[6];
                string psgpras = info[7];
                DateTime? added = DateTime.Parse(info[8]);
                var prepos = new List<string>(info[9].Split());
                var vCase = (VerbCase)Enum.Parse(typeof(VerbCase), info[10]);

                IWord word = this.factory.CreateWord(type, article, vCase, content, added, prepos, definition, plural, pratit, partizip2, psgpras);

                this.words.Add(word);
            }

            sr.Dispose();
        }

        public virtual IList<IUser> ReadUsers()
        {
            if (File.Exists(UsernamesFilePath))
            {
                using (var sr = new StreamReader(UsernamesFilePath))
                {
                    string line = string.Empty;

                    while ((line = sr.ReadLine()) != null)
                    {
                        var newUser = this.factory.CreateUser(line);
                        this.allUsers.Add(newUser);
                    }
                }
            }

            return this.AllUsers;
        }

        protected virtual IUser GetUserByUsername(string userName)
        {
            return this.allUsers.FirstOrDefault(user => user.Name == userName);
        }
    }
}
