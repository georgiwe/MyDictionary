namespace MyDictionary.Interfaces
{
    public interface IAnnouncer
    {
        void WordNotFound(IWord word);

        void WordNotFound(string wordContent);

        void NoWordsWereRemoved(IWord triedToRemWord);

        void WordRemoved(IWord word);

        void UserNotSelected();

        void WordAdded(IWord word);

        void WordNotAdded();

        void CustomMsg(string msg);

        void InvalidAnswer(string msg = "Invalid answer.");

        void InvalidQuizType(string msg = "Invalid quiz type.");

        void CannotGenerateQuiz(string msg = "Not enough words in dictionary.");
    }
}
