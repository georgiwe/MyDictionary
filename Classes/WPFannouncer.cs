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

            MessageBox.Show(message, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void WordNotFound(IWord word)
        {
            this.WordNotFound(word.Content);
        }

        public void NoWordsWereRemoved(IWord triedToRemWord)
        {
            this.WordNotFound(triedToRemWord);

            MessageBox.Show(NoWordsWereRemovedMsg, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void WordRemoved(IWord word)
        {
            string message = string.Format(WordRemovedMsg, word.Content);

            MessageBox.Show(message, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void UserNotSelected()
        {
            MessageBox.Show(NoUserSelectedMsg, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void WordAdded(IWord word)
        {
            string message = string.Format(WordAddedMsg, word.Content);

            MessageBox.Show(message, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void WordNotAdded()
        {
            MessageBox.Show(WordNotAddedMsg, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void CustomMsg(string msg)
        {
            MessageBox.Show(msg, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void CustomErrorMsg(string msg)
        {
            MessageBox.Show(msg, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void InvalidAnswer(string msg)
        {
            MessageBox.Show(msg, "Invalid Answer", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void InvalidQuizType(string msg = "Invalid quiz type.")
        {
            MessageBox.Show(msg, "Invalid Quiz Type", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void CannotGenerateQuiz(string msg = "Not enough words in dictionary.")
        {
            MessageBox.Show(msg, "Quiz Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void SuccessfulyEdited(IWord word, string msg = "Word {0} successfuly edited.")
        {
            msg = string.Format(msg, word.Content);

            MessageBox.Show(msg, "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool AskForConfirmation(string msg, string caption = "Are you sure?")
        {
            var btnMsgBox = MessageBoxButton.YesNo;
            var image = MessageBoxImage.Warning;

            var result = MessageBox.Show(msg, caption, btnMsgBox, image);

            if (result == MessageBoxResult.Yes) return true;
            else return false;
        }

        public void WordNotEdited(IWord word, string msg = "Word {0} was not edited")
        {
            msg = string.Format(msg, word.Content);

            MessageBox.Show(msg, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }


        public void AlreadyExists(IWord word, string msg = "Word {0} already exists in the dictionery!")
        {
            msg = string.Format(msg, word.Content);

            MessageBox.Show(msg, DefaultCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
