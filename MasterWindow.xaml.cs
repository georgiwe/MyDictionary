namespace MyDictionary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
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

    /// <summary>
    /// Interaction logic for MasterWindow.xaml
    /// </summary>
    public partial class MasterWindow : Window
    {
        private const string WeirdUmlautSymb = "-"; ////"-" + '\u0308'
        private readonly string searchBoxDefaultText;
        private readonly string dictDefBoxDefaultText;

        private IDictionaryEngine engine;
        private IAnnouncer announcer;
        private TextBox lastActiveTB;

        public MasterWindow(string username)
        {
            this.InitializeComponent();

            this.Engine = new Engine(username);
            this.announcer = new WPFannouncer();

            this.Title += string.Format(Helper.SignedInMessage, this.engine.User.Name);

            this.RefreshData();

            this.searchBoxDefaultText = searchBox.Text;
            this.dictDefBoxDefaultText = dictDefinition.Text;
        }

        private IDictionaryEngine Engine
        {
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Engine cannot be null");
                }

                this.engine = value;
            }
        }

        private void RefreshData()
        {
            var words = this.engine
                .GetAllWords()
                .Select(w => w.Content)
                .OrderBy(w => w);

            dictListBox.ItemsSource = words;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchBox.Text == this.searchBoxDefaultText)
            {
                searchBox.Text = string.Empty;
            }

            searchBox.GotFocus -= this.SearchBox_GotFocus;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.searchBox.Text))
            {
                this.searchBox.Text = this.searchBoxDefaultText;

                this.searchBox.GotFocus += this.SearchBox_GotFocus;
                dictDefinition.Text = string.Empty;
            }

            this.SaveActiveTB(this.searchBox, e);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.dictDefinition != null)
            {
                IWord word = this.engine.GetWord(searchBox.Text);

                if (word != null)
                {
                    dictDefinition.Text = word.ToString();
                    return;
                }

                this.dictDefinition.Text = this.dictDefBoxDefaultText;
            }
        }

        private void TypeCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selection = ((typeCBox.SelectedItem as ComboBoxItem).Content as TextBlock).Text;

            this.HideAllPanels(this.wordElemsWrapPanel);
            this.HandleDefinitionBoxes();
            this.EmptyAllTextBoxes(this.wordElemsWrapPanel);
            this.articleCB.SelectedIndex = -1;
            this.vCaseCB.SelectedIndex = -1;

            foreach (var item in wordElemsWrapPanel.Children)
            {
                var itemAsPanel = item as Panel;

                if (itemAsPanel != null)
                {
                    var labels = itemAsPanel.Tag.ToString();

                    if (labels.Contains(selection) ||
                        itemAsPanel.Tag.ToString() == "Type")
                    {
                        itemAsPanel.Visibility = Visibility.Visible;
                    }
                }
            }

            if (selection == "Verb")
            {
                this.caseContainer.Visibility = Visibility.Visible;
                this.nounContainer.Visibility = Visibility.Collapsed;
            }

            if (selection == "Noun")
            {
                this.caseContainer.Visibility = Visibility.Collapsed;
                this.nounContainer.Visibility = Visibility.Visible;
            }

            if (selection != null)
            {
                addWordButt.Visibility = Visibility.Visible;
                addWTabClearButt.Visibility = Visibility.Visible;
                this.buttPanel.Visibility = Visibility.Visible;
            }
        }

        private void AddWordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Word type
                string typeSelection = ((TextBlock)((ComboBoxItem)typeCBox.SelectedValue).Content).Text;
                WordType type = (WordType)Enum.Parse(typeof(WordType), typeSelection);

                // Article
                WordArticles article = WordArticles.NotApplicable;

                if (type == WordType.Noun)
                {
                    var artSelection = ((TextBlock)((ComboBoxItem)articleCB.SelectedItem).Content).Text;

                    article = (WordArticles)Enum.Parse(typeof(WordArticles), artSelection);
                }

                // Date added
                var added = addWDatePicker.SelectedDate as DateTime?;

                string content = null;

                if (wordTB.Text != null)
                {
                    content = wordTB.Text.ToLower();
                    content = content.Trim();
                    content = Helper.CutMultipleSpaces(content);

                    if (type == WordType.Noun)
                    {
                        content = char.ToUpper(content[0]) + content.Substring(1).ToLower();
                    }
                }

                // Plural
                string plural = pluralTB.Text.Trim();
                plural = Helper.CutMultipleSpaces(plural);

                // Verb forms
                string pratit = string.Empty;
                string partiz2 = string.Empty;
                string psgpras = string.Empty;

                if (type == WordType.Verb)
                {
                    pratit = prateritumTB.Text.Trim().ToLower();
                    partiz2 = partizip2.Text.Trim().ToLower();
                    psgpras = psgprasTB.Text.Trim().ToLower();

                    pratit = Helper.CutMultipleSpaces(pratit);
                    partiz2 = Helper.CutMultipleSpaces(partiz2);
                    psgpras = Helper.CutMultipleSpaces(psgpras);
                }

                // Verb case
                var vCase = VerbCase.NotApplicable;

                if (type == WordType.Verb)
                {
                    var selectedItem = vCaseCB.SelectedItem as ComboBoxItem;

                    var selectedVerbCaseStr = (selectedItem.Content as TextBlock).Text;

                    vCase = (VerbCase)Enum.Parse(typeof(VerbCase), selectedVerbCaseStr);
                }

                // Prepositions
                var prepos = new List<string>(preposTB.Text
                    .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => Helper.CutMultipleSpaces(p.Trim().ToLower())));

                // Definitions
                var allDefBoxes = this.GetAllTextBoxes(definitionsStackPanel);
                var nonEmptyDefBoxes = this.GetAllNonEmptyBoxes(allDefBoxes);
                var definitions = nonEmptyDefBoxes
                    .Select(tb => Helper.CutMultipleSpaces(tb.Text.Trim().ToLower()));

                var definition = new List<string>(definitions);

                this.engine.AddWord(
                    type, article, vCase, content, added, prepos, definition, plural, pratit, partiz2, psgpras);
            }
            catch (NullReferenceException)
            {
                this.announcer.WordNotAdded();
            }
            catch (ArgumentException ex)
            {
                this.announcer.CustomMsg(ex.ParamName);
            }
            catch (IndexOutOfRangeException)
            {
            }

            this.EmptyAll_ButtClick(new object(), new RoutedEventArgs());
            this.RefreshData();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.engine.SaveWords();
        }

        private void DictListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                string word = (string)e.AddedItems[0];

                dictDefinition.Text = this.engine.GetWord(word).ToString();
            }
        }

        private void DictListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            dictListBox.SelectedIndex = -1;
        }

        private void AddStringToContainer(string str, object container)
        {
            var cntAsTB = container as TextBox;

            if (cntAsTB != null && str != null)
            {
                cntAsTB.Focus();
                cntAsTB.Text += str;
                cntAsTB.CaretIndex = int.MaxValue;
            }
        }

        private void AddWord_TabLoaded(object sender, RoutedEventArgs e)
        {
            this.HideAllPanels(this.wordElemsWrapPanel);

            this.addWordButt.Visibility = Visibility.Collapsed;
            this.addWTabClearButt.Visibility = Visibility.Collapsed;
        }

        private void HideAllPanels(Panel container)
        {
            foreach (var item in container.Children)
            {
                var itemAsPanel = item as Panel;

                if (itemAsPanel != null)
                {
                    if (itemAsPanel.Tag.ToString() != "Type")
                    {
                        itemAsPanel.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void SaveActiveTB(object sender, RoutedEventArgs e)
        {
            this.lastActiveTB = sender as TextBox;
        }

        private void CharacterButtClick(object sender, RoutedEventArgs e)
        {
            var senderAsBtn = sender as Button;

            this.AddStringToContainer(senderAsBtn.Content as string, this.lastActiveTB);
        }

        private void DefinitionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.HandleDefinitionBoxes();
        }

        private void HandleDefinitionBoxes()
        {
            var allTextBoxes = this.GetAllTextBoxes(definitionsStackPanel);

            this.HideAllEmptyTextBoxes(allTextBoxes);

            var visibleBoxes = this.GetAllNonEmptyBoxes(allTextBoxes);
            var fullVisibleBoxesCount = visibleBoxes.Count(tb => !string.IsNullOrWhiteSpace(tb.Text));

            if (visibleBoxes.Count() == fullVisibleBoxesCount && fullVisibleBoxesCount < allTextBoxes.Count)
            {
                var firstInvis = allTextBoxes.First(tb => tb.Visibility == Visibility.Collapsed);

                firstInvis.Visibility = Visibility.Visible;
            }
        }

        private IEnumerable<TextBox> GetAllNonEmptyBoxes(IEnumerable<TextBox> boxes)
        {
            return boxes.Where(tb => !string.IsNullOrWhiteSpace(tb.Text));
        }

        private void HideAllEmptyTextBoxes(IEnumerable<TextBox> boxes)
        {
            foreach (var box in boxes)
            {
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    box.Visibility = Visibility.Collapsed;
                }
            }
        }

        private IList<TextBox> GetAllTextBoxes(Panel container)
        {
            var textBoxes = new List<TextBox>();

            foreach (var item in container.Children)
            {
                var textBox = item as TextBox;

                if (textBox != null)
                {
                    textBoxes.Add(textBox);
                }
            }

            return textBoxes;
        }

        private void Quizzes_SetDates(object sender, RoutedEventArgs e)
        {
            var allWords = this.engine.GetAllWords();

            var oldestWordAddedDate = allWords.Min(w => w.DateAdded);

            quizStartDate.SelectedDate = oldestWordAddedDate;
            quizEndDate.SelectedDate = DateTime.Now.Date;
        }

        private void EmptyAllTextBoxes(Panel container)
        {
            if (container == null)
            {
                throw new ArgumentException(
                    "Container is null. Cannot clear textboxes.");
            }

            foreach (var item in container.Children)
            {
                if (item is Panel)
                {
                    this.EmptyAllTextBoxes(item as Panel);
                }

                var textBox = item as TextBox;

                if (textBox != null)
                {
                    textBox.Text = string.Empty;
                }
            }
        }

        private void EmptyAllComboBoxes(Panel container)
        {
            if (container == null)
            {
                throw new ArgumentException(
                    "Container is null. Cannot clear textboxes.");
            }

            foreach (var item in container.Children)
            {
                if (item is Panel)
                {
                    this.EmptyAllComboBoxes(item as Panel);
                }

                var comboBox = item as ComboBox;

                if (comboBox != null)
                {
                    comboBox.Text = string.Empty;
                }
            }
        }

        private void EmptyAll_ButtClick(object sender, RoutedEventArgs e)
        {
            this.EmptyAllTextBoxes(this.wordElemsWrapPanel);
            this.EmptyAllComboBoxes(this.wordElemsWrapPanel);
        }

        private void BeginQuiz_ButtClick(object sender, RoutedEventArgs e)
        {
            string quizType = string.Empty;
            string numOfQuestionsStr = string.Empty;

            var startDate = this.quizStartDate.SelectedDate;
            var endDate = this.quizEndDate.SelectedDate;

            try
            {
                quizType =
                    ((this.quizTypeCB.SelectedItem as ComboBoxItem).Content as TextBlock).Text;

                numOfQuestionsStr =
                    ((this.numOfQs.SelectedItem as ComboBoxItem).Content as TextBlock).Text;
            }
            catch (SystemException)
            {
                this.announcer.InvalidQuizType();

                return;
            }

            int numOfQuestions = int.Parse(numOfQuestionsStr);

            QuizWindow quizWindow = null;

            try
            {
                quizWindow = new QuizWindow(quizType, numOfQuestions, this.engine, startDate, endDate);

                if (quizWindow == null)
                {
                    throw new QuizException("Quiz not generated.");
                }
            }
            catch (QuizException ex)
            {
                this.announcer.CannotGenerateQuiz(ex.Message);
                return;
            }

            quizWindow.Owner = masterWindow;

            quizWindow.Activate();
            quizWindow.Show();

            this.Hide();
        }
    }
}
