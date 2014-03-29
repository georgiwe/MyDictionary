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

        void CustomErrorMsg(string msg);

        void InvalidAnswer(string msg = "Invalid answer.");

        void InvalidQuizType(string msg = "Invalid quiz type.");

        void CannotGenerateQuiz(string msg = "Not enough words in dictionary.");

        void WordNotEdited(IWord word, string msg = "Word {0} was not edited");

        void SuccessfulyEdited(IWord word, string msg = "Word {0} successfuly edited.");

        bool AskForConfirmation(string msg, string caption = "Are you sure?");

        void AlreadyExists(IWord word, string msg = "Word {0} already exists in the dictionery!");
    }
}
