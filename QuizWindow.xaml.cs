namespace MyDictionary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    using MyDictionary.Classes;
    using MyDictionary.Enums;
    using MyDictionary.Helpers;
    using MyDictionary.Interfaces;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Interaction logic for QuizWindow.xaml
    /// </summary>
    public partial class QuizWindow : Window
    {
        private readonly string defaultAnswerText = "";
        private const string AnswerJoinSymbol = "$";

        private TextBox lastActiveTB;

        private DateTime? startDate;
        private DateTime? endDate;
        private Random rnd;
        private IAnnouncer announcer;
        private bool[] correctness;
        private IList<IWord> questionWords;
        private string[] answers;
        private QuizType quizType;
        private int numOfQuestions;
        private int currQind;
        private IDictionaryEngine engine;
        private string[] questions;
        private string[] correctAnswers;

        public QuizWindow(
            string quizType, int numOfQuestions, IDictionaryEngine engine,
            DateTime? startDate, DateTime? endDate)
        {
            this.InitializeComponent();

            this.engine = engine;
            this.NumOfQuestions = numOfQuestions;
            this.quizType = Helper.ParseQuizType(quizType);

            this.startDate = startDate;
            this.endDate = endDate;

            this.announcer = new WPFannouncer();
            this.rnd = new Random();

            this.Title = quizType + " - " + this.numOfQuestions + " Questions";
            this.Title += string.Format(Helper.SignedInMessage, this.engine.User.Name);

            this.InitializeData();
        }

        private int NumOfQuestions
        {
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "Questions number cannot be 0 or less.");
                }

                this.numOfQuestions = value;
            }
        }

        private void InitializeData()
        {
            this.answerTB.Text = defaultAnswerText;

            var acceptedTypes = this.DetermineAcceptedTypes();

            this.questionWords = this.engine.GenerateQuiz(
                this.numOfQuestions, acceptedTypes, 
                this.quizType, this.startDate, this.endDate);

            if (this.questionWords == null ||
                this.questionWords.Count == 0)
            {
                throw new QuizException(
                    "Not enough appropriate questions in dictionary.");
            }
            
            this.questions = new string[this.questionWords.Count];
            this.answers = new string[this.questionWords.Count];
            this.correctness = new bool[this.questionWords.Count];
            this.correctAnswers = new string[this.questionWords.Count];

            this.qNumIndicator.Text = "1";
            this.totalQs.Text = this.questionWords.Count.ToString();

            var currWord = this.questionWords[this.currQind];

            this.HandleAnswerTBsVisibility();

            this.RenderAppropriately(currWord);
        }

        private IEnumerable<WordType> DetermineAcceptedTypes()
        {
            var result = new List<WordType>()
            {
                WordType.Adjective,
                WordType.Noun,
                WordType.Verb
            };

            switch (this.quizType)
            {
                case QuizType.Article:
                    result.Remove(WordType.Verb);
                    result.Remove(WordType.Adjective);
                    break;

                case QuizType.VerbPastTenses:
                    result.Remove(WordType.Adjective);
                    result.Remove(WordType.Noun);
                    break;

                case QuizType.Preposition:
                    result.Remove(WordType.Noun);
                    result.Remove(WordType.Adjective);
                    break;
            }

            return result;
        }

        private void RenderAppropriately(IWord word)
        {
            switch (this.quizType)
            {
                case QuizType.ForeignToNative:
                    this.RenderForeignToNative(word);
                    break;

                case QuizType.Article:
                    this.RenderArticle(word);
                    break;

                case QuizType.VerbPastTenses:
                    this.RenderVerbPastTenses(word);
                    break;

                case QuizType.Preposition:
                    this.RenderPrepositions(word);
                    break;

                case QuizType.NativeToForeign:
                    this.RenderNativeToForeign(word);
                    break;
            }

            this.questions[currQind] = this.questionTBL.Text;
            this.answerTB.Focus();
        }

        private void RenderNativeToForeign(IWord word)
        {
            int ind = this.rnd.Next() % word.Definition.Count;

            this.questionTBL.Text = word.Definition[ind];
        }

        private void RenderForeignToNative(IWord wordToRender)
        {
            this.questionTBL.Text = string.Empty;

            // Add article if noun
            if (wordToRender.Type == WordType.Noun)
            {
                this.questionTBL.Text += wordToRender.Article + " ";
            }

            // Add content
            this.questionTBL.Text += wordToRender.Content;

            // Add plural if noun
            if (wordToRender.Type == WordType.Noun)
            {
                this.questionTBL.Text += " " + wordToRender.Plural;
            }
        }

        private void RenderArticle(IWord wordToRender)
        {
            this.questionTBL.Text = wordToRender.Content;
        }

        private void RenderVerbPastTenses(IWord wordToRender)
        {
            this.questionTBL.Text = wordToRender.Content;
        }

        private void RenderPrepositions(IWord wordToRender)
        {
            this.questionTBL.Text = wordToRender.Content;
        }

        private void MoveToQuestion(int qIndex)
        {
            if (qIndex < 0 || qIndex >= this.questionWords.Count)
            {
                throw new ArgumentOutOfRangeException("Question number is outside the accepted range");
            }

            this.currQind = qIndex;
            var wordToRender = this.questionWords[currQind]; // change from march 30th - ind changed to currQind from qIndex

            this.qNumIndicator.Text = (this.currQind + 1).ToString();
            this.AlternateAnswerButtonName();

            this.RenderAppropriately(wordToRender);
        }

        private void qNumIndicator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;

            if (!string.IsNullOrWhiteSpace(qNumIndicator.Text))
            {
                try
                {
                    int newQnum = int.Parse(qNumIndicator.Text);

                    this.MoveToQuestion(newQnum - 1);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    MessageBox.Show(ex.ParamName, "Error!");
                }
                catch (FormatException) { }
            }
        }

        private void Answer_ButtClick(object sender, RoutedEventArgs e)
        {
            string answer = string.Empty;

            try
            {
                answer = this.answStackPanel
                   .Children
                   .OfType<TextBox>()
                   .Where(c => !string.IsNullOrWhiteSpace(c.Text))
                   .Select(c => c.Text)
                   .Aggregate((s1, s2) => s1 + AnswerJoinSymbol + s2)
                   .Trim()
                   .ToLower();
            }
            catch (SystemException)
            {
                this.announcer.InvalidAnswer();

                return;
            }

            answer = Helper.CutMultipleSpaces(answer);

            if (string.IsNullOrWhiteSpace(answer))
            {
                this.announcer.InvalidAnswer();
                this.ResetAllTextBoxes();

                return;
            }

            this.answers[this.currQind] = answer;

            try
            {
                this.JudgeAnswer(answer);
            }
            catch (ArgumentException ex)
            {
                this.announcer.InvalidAnswer(ex.Message);

                return;
            }

            this.ResetAllAnswerTBs();
            this.HandleAnswerTBsVisibility();
            this.answerTB.Focus();

            if (this.currQind + 1 < this.questionWords.Count)
            {
                this.MoveToQuestion(this.currQind + 1);
            }
            else
            {
                this.QuizEnded();
            }
        }

        private IEnumerable<string> SplitAndTrimAnswers(string answer)
        {
            var answers = answer
                .Split(AnswerJoinSymbol.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim());

            return answers;
        }

        private void JudgeAnswer(string answer)
        {
            var currWord = this.questionWords[this.currQind];

            switch (this.quizType)
            {
                case QuizType.ForeignToNative:
                    this.JudgeForeignToNative(currWord, answer);
                    break;

                case QuizType.NativeToForeign:
                    this.JudgeNativeToForeign(currWord, answer);
                    break;

                case QuizType.Article:
                    this.JudgeArticleQuiz(currWord, answer);
                    break;

                case QuizType.VerbPastTenses:
                    this.JudgeVerbPastTensesQuiz(currWord, answer);
                    break;

                case QuizType.Preposition:
                    this.JudgePrepositionsQuiz(currWord, answer);
                    break;
            }
        }

        private void JudgeNativeToForeign(IWord currWord, string answer)
        {
            var answers = this.SplitAndTrimAnswers(answer).ToArray();

            if (answers.Length > 1)
            {
                throw new ArgumentException("There must be only one answer");
            }

            string compareWith = currWord.Content.ToLower();

            if (currWord.Type == WordType.Noun)
            {
                compareWith = currWord.Article + " " + compareWith;
            }

            if (compareWith == answers[0])
            {
                this.correctness[currQind] = true;
            }

            this.correctAnswers[currQind] = compareWith;
        }

        private void JudgeForeignToNative(IWord currWord, string answer)
        {
            double guessed = 0;
            var answers = this.SplitAndTrimAnswers(answer);

            foreach (var attempt in answers)
            {
                var defsToLower = currWord.Definition.Select(d => d.ToLower().Trim());

                if (defsToLower.Contains(attempt))
                {
                    guessed++;
                }
            }

            double definitionsCount = (double)currWord.Definition.Count;
            var malus = definitionsCount - answers.Count();

            if (malus < 0)
            {
                guessed += malus;
            }

            if (guessed / definitionsCount > 0.5)
            {
                this.correctness[currQind] = true;
            }

            this.correctAnswers[currQind] = string.Join(", ", currWord.Definition);
        }

        private void JudgeArticleQuiz(IWord currWord, string answer)
        {
            var answers = this.SplitAndTrimAnswers(answer);

            if (answers.Count() > 1)
            {
                throw new ArgumentException("There must only be one answer");
            }

            WordArticles answerType = WordArticles.NotApplicable;

            try
            {
                answerType = (WordArticles)Enum.Parse(typeof(WordArticles), answer);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentOutOfRangeException("Please enter a valid word article.", ex);
            }

            if (answerType == currWord.Article)
            {
                this.correctness[currQind] = true;
            }

            this.correctAnswers[currQind] = currWord.Article.ToString();
        }

        private void JudgeVerbPastTensesQuiz(IWord currWord, string answer)
        {
            int guessed = 0;
            var answers = this.SplitAndTrimAnswers(answer);

            this.correctAnswers[currQind] =
                currWord.Partizip2 + ", " + currWord.PSgPras + ", " + currWord.Pratitium;

            if (answers.Count() < 2)
            {
                return;
            }

            if (answers.Contains(currWord.Partizip2.ToLower()))
            {
                guessed++;
            }

            if (answers.Contains(currWord.PSgPras.ToLower()))
            {
                guessed++;
            }

            if (answers.Contains(currWord.Pratitium.ToLower()))
            {
                guessed++;
            }

            if (guessed >= 2)
            {
                this.correctness[currQind] = true;
            }
        }

        private void JudgePrepositionsQuiz(IWord currWord, string answer)
        {
            double guessed = 0;
            var answers = this.SplitAndTrimAnswers(answer);

            var preposToLower = currWord.Prepositions.Select(p => p.ToLower());

            foreach (var answ in answers)
            {
                if (preposToLower.Contains(answ))
                {
                    guessed++;
                }
            }

            double numOfPrepos = (double)currWord.Prepositions.Count;
            var malus = numOfPrepos - answers.Count();

            if (malus < 0)
            {
                guessed += malus;
            }

            if (guessed / numOfPrepos > 0.5)
            {
                this.correctness[currQind] = true;
            }

            var correctAnswer = string.Join(", ", currWord.Prepositions);

            this.correctAnswers[currQind] = correctAnswer;
        }

        private void AlternateAnswerButtonName()
        {
            if (this.currQind == this.questionWords.Count - 1)
            {
                (this.answerButt.Content as TextBlock).Text = "Complete Quiz";
            }
            else
            {
                (this.answerButt.Content as TextBlock).Text = "Answer";
            }
        }

        private void QuizEnded()
        {
            var resultsWindow = new QuizResultsWindow(this.quizType, this.questionWords, this.questions, this.correctAnswers, this.answers, this.correctness);

            resultsWindow.Owner = this.Owner;
            resultsWindow.Activate();
            resultsWindow.Show();

            this.Close();
            resultsWindow.Focus();
        }

        private void QuizWindow_Closed(object sender, EventArgs e)
        {
            this.Owner.Show();
        }

        private void ResetAllTextBoxes()
        {
            this.answerTB.Text = this.defaultAnswerText;
            this.ResetAllAnswerTBs();
            this.answerTB.Focus();
        }

        private void CharacterButtClick(object sender, RoutedEventArgs e)
        {
            var senderAsBtn = sender as Button;

            if (senderAsBtn != null)
            {
                this.AddStringToContainer(senderAsBtn.Content as string, this.lastActiveTB);
            }
        }

        private void AddStringToContainer(string str, object container)
        {
            var cntrAsTextBox = container as TextBox;

            if (cntrAsTextBox != null)
            {
                cntrAsTextBox.Focus();
                cntrAsTextBox.Text += str;
                cntrAsTextBox.CaretIndex = int.MaxValue;
            }
        }

        private void UpdateLastActiveTB(object sender, RoutedEventArgs e)
        {
            this.lastActiveTB = sender as TextBox;
        }

        private void AnswerTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return &&
                 e.Key != Key.Enter)
            {
                return;
            }

            this.Answer_ButtClick(new object(), new RoutedEventArgs());
        }

        private void HideAllTextBoxes(Panel panel)
        {
            foreach (var item in panel.Children)
            {
                var tb = item as TextBox;

                if (tb != null)
                {
                    tb.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void ResetAllAnswerTBs()
        {
            foreach (var item in this.answStackPanel.Children)
            {
                var tb = item as TextBox;

                tb.Text = string.Empty;
            }
        }

        private void HandleAnswerTBsVisibility()
        {
            this.HideEmptyAnswerTBs();

            var allTBs = this.answStackPanel.Children.OfType<TextBox>();

            var visibleTBs = allTBs
                .Where(tb => tb.Visibility == Visibility.Visible);

            var visibleEmpties = visibleTBs.Where(tb => tb.Text == string.Empty);

            if (visibleEmpties.Count() < 2)
            {
                var tbToShow = allTBs
                    .FirstOrDefault(tb => tb.Visibility == Visibility.Collapsed);

                if (tbToShow != null)
                {
                    tbToShow.Visibility = Visibility.Visible;
                }
            }
        }

        private void HideEmptyAnswerTBs()
        {
            foreach (var item in this.answStackPanel.Children)
            {
                var tb = item as TextBox;

                if (tb != null)
                {
                    if (string.IsNullOrWhiteSpace(tb.Text))
                    {
                        tb.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }

        private void HandleAnswerTBsVisibility(object sender, TextChangedEventArgs e)
        {
            this.HandleAnswerTBsVisibility();
        }
    }
}
