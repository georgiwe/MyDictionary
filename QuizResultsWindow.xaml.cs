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

    /// <summary>
    /// Interaction logic for QuizResultsWindow.xaml
    /// </summary>
    public partial class QuizResultsWindow : Window
    {
        private IList<IWord> questions;
        private string[] answers;
        private bool[] correctness;
        private QuizType quizType;
        private int corrs;
        private int wrngs;

        public QuizResultsWindow(QuizType quizType, IList<IWord> questions, string[] answers, bool[] correctness)
        {
            InitializeComponent();

            this.correctness = correctness;
            this.questions = questions;
            this.answers = answers;
            this.quizType = quizType;

            this.InitializeData();
        }

        //TODO: Add save result
        //TODO: Add wrong/right count to words, make recommendations/statistics in this regard

        private void InitializeData()
        {
            this.corrs = this.correctness.Count(a => a == true);
            this.wrngs = this.questions.Count - this.corrs;

            this.totalQs.Text = this.questions.Count.ToString();
            this.correctCount.Text = this.corrs.ToString();
            this.wrongCount.Text = this.wrngs.ToString();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Owner.Show();
            this.Owner.WindowState = System.Windows.WindowState.Normal;
        }
    }
}
