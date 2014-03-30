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

    using MyDictionary.Interfaces;
    using MyDictionary.Enums;
    using MyDictionary.Helpers;

    /// <summary>
    /// Interaction logic for QuizResultsWindow.xaml
    /// </summary>
    public partial class QuizResultsWindow : Window
    {
        // add const for None in preps
        private IList<string> correctAnswers;
        private IList<string> questions;
        private IList<IWord> words;
        private bool[] correctness;
        private QuizType quizType;
        private string[] answers;
        private int corrs;
        private int wrngs;

        public QuizResultsWindow(QuizType quizType, IList<IWord> words, IList<string> questions, string[] correctAnswers, string[] answers, bool[] correctness)
        {
            InitializeComponent();

            this.quizTypeTBL.Text = Helper.QuizTypeEnumToString(quizType);

            this.correctAnswers = correctAnswers;
            this.questions = questions;
            this.correctness = correctness;
            this.words = words;
            this.quizType = quizType;
            this.answers = answers;

            this.InitializeData();
        }

        //TODO: Add save result
        //TODO: Add wrong/right count to words, make recommendations/statistics in this regard

        private void InitializeData()
        {
            this.corrs = this.correctness.Count(a => a == true);
            this.wrngs = this.words.Count - this.corrs;

            // Assign values to the text boxes
            this.totalQs.Text = this.words.Count.ToString();
            this.correctCount.Text = this.corrs.ToString();
            this.wrongCount.Text = this.wrngs.ToString();

            var data = new QuizData[this.questions.Count];

            for (int i = 0; i < data.Length; i++)
            {
                var answers = string.Join(", ", this.answers[i].Split('$'));
                var corAns = this.correctAnswers[i] == string.Empty ? "None" : this.correctAnswers[i];

                data[i] = new QuizData(
                    i + 1, this.questions[i], corAns, answers, this.correctness[i]);
            }

            this.report.ItemsSource = data;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Owner.Show();
            this.Owner.WindowState = WindowState.Normal;
        }
    }
}
