namespace MyDictionary.Classes
{
    using System.Windows;

    using MyDictionary.Interfaces;

    public class WPFannouncer : IAnnouncer
    {
        private const string WordNotFoundMsg = "Word {0} was not found in the dictionary!";
        private const string NoWordsWereRemovedMsg = "No words were removed.";
        private const string WordRemovedMsg = "Word {0} was successfuly removed from the dictionary.";
        private const string NoUserSelectedMsg = "Please select a user before logging in.";
        private const string WordAddedMsg = "Word {0} successfuly added to dictionary!";
        private const string WordNotAddedMsg = "Word could not be added. Please check word parameters.";
        private const string DefaultCaption = "Error!";

        public void WordNotFound(string wordContent)
        {
            string message = string.Format(WordNotFoundMsg, wordContent);

            MessageBox.Show(message, DefaultCaption);
        }

        public void WordNotFound(IWord word)
        {
            this.WordNotFound(word.Content);
        }

        public void NoWordsWereRemoved(IWord triedToRemWord)
        {
            this.WordNotFound(triedToRemWord);

            MessageBox.Show(NoWordsWereRemovedMsg, DefaultCaption);
        }

        public void WordRemoved(IWord word)
        {
            string message = string.Format(WordRemovedMsg, word.Content);

            MessageBox.Show(message, DefaultCaption);
        }

        public void UserNotSelected()
        {
            MessageBox.Show(NoUserSelectedMsg, DefaultCaption);
        }

        public void WordAdded(IWord word)
        {
            string message = string.Format(WordAddedMsg, word.Content);

            MessageBox.Show(message, DefaultCaption);
        }

        public void WordNotAdded()
        {
            MessageBox.Show(WordNotAddedMsg, DefaultCaption);
        }

        public void CustomMsg(string msg)
        {
            MessageBox.Show(msg, DefaultCaption);
        }

        public void InvalidAnswer(string msg)
        {
            MessageBox.Show(msg, "Invalid Answer");
        }


        public void InvalidQuizType(string msg = "Invalid quiz type.")
        {
            MessageBox.Show(msg, "Invalid Quiz Type");
        }


        public void CannotGenerateQuiz(string msg = "Not enough words in dictionary.")
        {
            MessageBox.Show(msg, "Quiz Error");
        }
    }
}
