namespace MyDictionary.Helpers
{
    using MyDictionary.Interfaces;

    public class WordExistsException : System.SystemException
    {
        public WordExistsException(string msg)
            :base(msg)
        {
        }

        public WordExistsException(string msg, IWord word)
            :base(msg)
        {
            this.Word = word;
        }

        public WordExistsException(IWord word)
        {
            this.Word = word;
        }

        public IWord Word { get; private set; }
    }
}
