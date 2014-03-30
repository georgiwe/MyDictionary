namespace MyDictionary
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
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
        private IWord wordToEdit;

        public MasterWindow(string username)
        {
            this.InitializeComponent();

            this.Engine = new Engine(username);
            this.announcer = new WPFannouncer();

            this.Title += string.Format(Helper.SignedInMessage, this.engine.User.Name);

            this.RefreshData();

            this.searchBoxDefaultText = searchBox.Text;
            this.dictDefBoxDefaultText = dictDefinition.Text;

            this.InitializeAddWordTab();
            this.InitializeEditWordTab();
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

            this.HidePanelsAndEmptyTextBoxes(this.wordElemsWrapPanel);
            this.HandleDefinitionBoxesInPanel(this.definitionsStackPanel);
            this.articleCB.SelectedIndex = -1;
            this.vCaseCB.SelectedIndex = -1;

            this.ShowNecessaryItems(this.wordElemsWrapPanel, selection);

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

        private void ShowNecessaryItems(Panel panel, string reqTagInOrderToShow)
        {
            foreach (var item in panel.Children)
            {
                var itemAsPanel = item as Panel;

                if (itemAsPanel != null)
                {
                    if (itemAsPanel.Tag == null) continue;

                    var labels = itemAsPanel.Tag.ToString();

                    if (labels.Contains(reqTagInOrderToShow) ||
                        itemAsPanel.Tag.ToString() == "Type")
                    {
                        itemAsPanel.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private WordArticles AssignArticle(WordType type, ComboBox cbox)
        {
            WordArticles article = WordArticles.NotApplicable;

            if (type == WordType.Noun)
            {
                var artSelection = ((TextBlock)((ComboBoxItem)cbox.SelectedItem).Content).Text;

                article = (WordArticles)Enum.Parse(typeof(WordArticles), artSelection);
            }

            return article;
        }

        private WordType AssignType(ComboBox cbox)
        {
            var cbItem = cbox.SelectedItem as ComboBoxItem;
            var itemContent = cbItem.Content as TextBlock;
            var typeSelection = itemContent.Text;

            WordType type = (WordType)Enum.Parse(typeof(WordType), typeSelection);

            return type;
        }

        private string AssignContent(WordType type, TextBox tbox)
        {
            string content = null;

            if (tbox.Text != null)
            {
                content = tbox.Text.ToLower();
                content = content.Trim();
                content = Helper.CutMultipleSpaces(content);

                if (type == WordType.Noun)
                {
                    content = char.ToUpper(content[0]) + content.Substring(1).ToLower();
                }
            }

            return content;
        }

        private string[] GetVerbForms(WordType type, TextBox prt, TextBox part, TextBox psg)
        {
            string pratit = string.Empty;
            string partiz2 = string.Empty;
            string psgpras = string.Empty;

            if (type == WordType.Verb)
            {
                pratit = prt.Text.Trim().ToLower();
                partiz2 = part.Text.Trim().ToLower();
                psgpras = psg.Text.Trim().ToLower();

                pratit = Helper.CutMultipleSpaces(pratit);
                partiz2 = Helper.CutMultipleSpaces(partiz2);
                psgpras = Helper.CutMultipleSpaces(psgpras);
            }

            return new string[] { pratit, partiz2, psgpras };
        }

        private VerbCase AssignVerbCase(WordType type, ComboBox verbCaseCbox)
        {
            var vCase = VerbCase.NotApplicable;

            if (type == WordType.Verb)
            {
                var selectedItem = verbCaseCbox.SelectedItem as ComboBoxItem;

                var selectedVerbCaseStr = (selectedItem.Content as TextBlock).Text;

                vCase = (VerbCase)Enum.Parse(typeof(VerbCase), selectedVerbCaseStr);
            }

            return vCase;
        }

        private IList<string> AssignPrepositions(TextBox prepTBox)
        {
            return prepTBox.Text
                        .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => Helper.CutMultipleSpaces(p.Trim().ToLower()))
                        .ToList();
        }

        private IList<string> AssignDefinitions(Panel boxesContainer)
        {
            var allDefBoxes = this.GetAllTextBoxes(boxesContainer);
            var nonEmptyDefBoxes = this.GetAllNonEmptyBoxes(allDefBoxes);

            var definitions = nonEmptyDefBoxes
                .Select(tb => Helper.CutMultipleSpaces(tb.Text.Trim().ToLower()))
                .ToList();

            return definitions;
        }

        private void AddWordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Word type
                var type = this.AssignType(this.typeCBox);

                // Article
                var article = this.AssignArticle(type, this.articleCB);

                // Date added
                var added = addWDatePicker.SelectedDate as DateTime?;

                // Content
                string content = this.AssignContent(type, this.wordTB);

                // Plural
                string plural = pluralTB.Text.Trim();
                plural = Helper.CutMultipleSpaces(plural);

                // Verb forms
                var forms = this.GetVerbForms(
                    type, this.prateritumTB, this.partizip2TB, this.psgprasTB);
                var pratit = forms[0];
                var partiz2 = forms[1];
                var psgpras = forms[2];

                // Verb case
                var vCase = this.AssignVerbCase(type, this.vCaseCB);

                // Prepositions
                var prepos = this.AssignPrepositions(this.preposTB);

                // Definitions
                var definition = this.AssignDefinitions(this.definitionsStackPanel);

                this.engine.AddWord(
                    type, article, vCase, content, added, prepos, definition, plural, pratit, partiz2, psgpras);
            }
            catch (NullReferenceException)
            {
                this.announcer.WordNotAdded();

                return;
            }
            catch (ArgumentException ex)
            {
                this.announcer.CustomErrorMsg(ex.Message);

                return;
            }
            catch (WordExistsException ex)
            {
                this.announcer.CustomErrorMsg(ex.Message);

                return;
            }
            catch (IndexOutOfRangeException)
            {
            }

            this.AddWordEmptyAll_ButtClick(new object(), new RoutedEventArgs());
            this.RefreshData();
        }

        private void SaveAllWords(object sender, EventArgs e)
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

        private void InitializeAddWordTab()
        {
            this.HideAllPanels(this.wordElemsWrapPanel);
            this.typeSelectionPanel.Visibility = Visibility.Visible;
            this.addWordButt.Visibility = Visibility.Collapsed;
            this.addWTabClearButt.Visibility = Visibility.Collapsed;
        }

        private void InitializeEditWordTab()
        {
            this.HidePanelsAndEmptyTextBoxes(this.sWordElemsWrapPanel);
            this.sTypeAndDatePanel.Visibility = Visibility.Collapsed;
            this.sNounContainer.Visibility = Visibility.Collapsed;
        }

        private void HideAllPanels(Panel container)
        {
            foreach (var item in container.Children)
            {
                var itemAsPanel = item as Panel;

                if (itemAsPanel != null)
                {
                    //if (itemAsPanel.Tag.ToString() != "Type" ||
                    //    itemAsPanel.Tag == null)
                    //{
                    //    itemAsPanel.Visibility = Visibility.Collapsed;
                    //}

                    itemAsPanel.Visibility = Visibility.Collapsed;
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

        private void AddWordDefBoxChanged(object sender, TextChangedEventArgs e)
        {
            this.HandleDefinitionBoxesInPanel(this.definitionsStackPanel);
        }

        private void EditWordDefBoxChanged(object sender, TextChangedEventArgs e)
        {
            this.HandleDefinitionBoxesInPanel(this.sDefinitionsStackPanel);
        }

        private void HandleDefinitionBoxesInPanel(Panel panel)
        {
            var allTextBoxes = this.GetAllTextBoxes(panel);

            this.HideAllEmptyTextBoxes(allTextBoxes);

            var visibleBoxes = this.GetAllNonEmptyBoxes(allTextBoxes);
            var fullVisibleBoxesCount = visibleBoxes
                .Count(tb => !string.IsNullOrWhiteSpace(tb.Text));

            if (visibleBoxes.Count() == fullVisibleBoxesCount &&
                fullVisibleBoxesCount < allTextBoxes.Count)
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
            var oldestWordAddedDate = this.engine
                .GetAllWords()
                .Min(w => w.DateAdded);

            this.quizEndDate.DisplayDateStart = oldestWordAddedDate;
            this.quizStartDate.DisplayDateStart = oldestWordAddedDate;

            this.quizStartDate.SelectedDate = oldestWordAddedDate;
            this.quizEndDate.SelectedDate = DateTime.Now.Date;

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

        private void AddWordEmptyAll_ButtClick(object sender, RoutedEventArgs e)
        {
            this.ResetContents(this.wordElemsWrapPanel);
        }

        private void ResetContents(Panel panel)
        {
            this.EmptyAllTextBoxes(panel);
            this.EmptyAllComboBoxes(panel);
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

        private void HidePanelsAndEmptyTextBoxes(Panel panel)
        {
            this.HideAllPanels(panel);
            this.EmptyAllTextBoxes(panel);
        }

        private void FindWord_ButtClick(object sender, RoutedEventArgs e)
        {
            var searchFor = searchWordTB.Text.Trim();

            var searchResult = this.engine.GetWord(searchFor);

            if (searchResult == null)
            {
                this.announcer.WordNotFound(searchFor);

                this.InitializeEditWordTab();

                this.searchWordTB.Focus();
                this.searchWordTB.SelectAll();

                return;
            }

            this.InitializeEditWordTab();

            this.LoadWordItems(searchResult);

            this.sDefinitionsStackPanel.Children
                .OfType<TextBox>()
                .FirstOrDefault(tb => tb.Text == string.Empty)
                .Focus();

            this.wordToEdit = searchResult;
        }

        private void LoadWordItems(IWord word)
        {
            // Handle visibility of fields
            this.sTypeAndDatePanel.Visibility = Visibility.Visible;
            this.sButtPanel.Visibility = Visibility.Visible;
            this.ShowNecessaryItems(this.sWordElemsWrapPanel, word.Type.ToString());
            this.HandleDefinitionBoxesInPanel(this.sDefinitionsStackPanel);

            // Set Word field
            this.sWordTB.Text = word.Content;

            // Set date added field
            this.sAddWDatePicker.SelectedDate = word.DateAdded;

            // Set definitions
            var amtOfNeededTBs = this.sDefinitionsStackPanel
                .Children
                .OfType<TextBox>()
                .Take(word.Definition.Count);

            int defIndex = 0;
            var definitions = word.Definition;

            foreach (var box in amtOfNeededTBs)
            {
                box.Visibility = Visibility.Visible;
                box.Text = definitions[defIndex++];
            }

            // Set specific fields, depending on word type
            if (word.Type == WordType.Noun)
            {
                this.FillNounItems(word);
            }

            else if (word.Type == WordType.Verb)
            {
                this.FillVerbItems(word);
            }
        }

        private void FillNounItems(IWord noun)
        {
            this.sNounContainer.Visibility = Visibility.Visible;

            this.sArticleCB.SelectedIndex =
                this.GetAppropriateComboBoxIndex(noun.Article);

            this.sPluralTB.Text = noun.Plural;
        }

        private void FillVerbItems(IWord verb)
        {
            this.verbContainer.Visibility = Visibility.Visible;

            // Fill verb forms
            this.sPsgprasTB.Text = verb.PSgPras;
            this.sPrateritumTB.Text = verb.Pratitium;
            this.sPartizip2TB.Text = verb.Partizip2;

            this.sPreposTB.Text = string.Join(" ", verb.Prepositions);

            this.sVCaseCB.SelectedIndex = this.GetAppropriateComboBoxIndex(verb.Case);
        }

        private int GetAppropriateComboBoxIndex(Enum inputEnum)
        {
            if (inputEnum is WordArticles)
            {
                switch ((WordArticles)inputEnum)
                {
                    case WordArticles.der: return 0;
                    case WordArticles.das: return 1;
                    case WordArticles.die: return 2;
                }
            }

            else if (inputEnum is WordType)
            {
                switch ((WordType)inputEnum)
                {
                    case WordType.Adjective: return 1;
                    case WordType.Noun: return 0;
                    case WordType.Verb: return 2;
                }
            }

            else if (inputEnum is VerbCase)
            {
                switch ((VerbCase)inputEnum)
                {
                    case VerbCase.A: return 0;
                    case VerbCase.D: return 1;
                    case VerbCase.G: return 2;
                    case VerbCase.N: return 3;
                }
            }

            return -1;
        }

        private void sSaveWordButt_Click(object sender, RoutedEventArgs e)
        {
            var confirmation =
                this.announcer.AskForConfirmation(
                        string.Format("Are you sure you want to save changes to {0}?",
                            this.wordToEdit.Content));

            if (confirmation == false) return;

            IWord updatedWord = null;

            try
            {
                var type = this.wordToEdit.Type;

                var article = this.AssignArticle(type, this.sArticleCB);
                var added = sAddWDatePicker.SelectedDate as DateTime?;

                string content = this.AssignContent(type, this.sWordTB);
                bool contentHasChanged = this.wordToEdit.Content != content;

                string plural = sPluralTB.Text.Trim();
                plural = Helper.CutMultipleSpaces(plural);

                var forms = this.GetVerbForms(
                    type, this.sPrateritumTB, this.sPartizip2TB, this.sPsgprasTB);
                var pratit = forms[0];
                var partiz2 = forms[1];
                var psgpras = forms[2];

                var vCase = this.AssignVerbCase(type, this.sVCaseCB);

                var prepos = this.AssignPrepositions(this.sPreposTB);

                var definition = this.AssignDefinitions(this.sDefinitionsStackPanel);

                updatedWord = this.engine.Factory.CreateWord(
                    type, article, vCase, content, added, prepos, definition, plural, pratit, partiz2, psgpras);

                if (contentHasChanged == false ||
                    this.engine.GetWord(content) == null)
                {
                    this.engine.ReplaceWord(this.wordToEdit, updatedWord);
                }
                else
                {
                    throw new WordExistsException(
                        "Word {0} already exists in the dictionery!", updatedWord);
                }
            }
            catch (NullReferenceException)
            {
                this.announcer.WordNotEdited(this.wordToEdit);

                return;
            }
            catch (ArgumentException ex)
            {
                this.announcer.CustomErrorMsg(ex.Message);

                return;
            }
            catch (WordExistsException ex)
            {
                this.announcer.AlreadyExists(ex.Word, ex.Message);

                return;
            }
            catch (IndexOutOfRangeException)
            {
            }

            this.announcer.SuccessfulyEdited(this.wordToEdit);
            this.RefreshData();

            this.InitializeEditWordTab();
        }

        private void sDeleteWord_Click(object sender, RoutedEventArgs e)
        {
            var lookingFor = this.searchWordTB.Text.Trim();

            var wordToDel = this.engine.GetWord(lookingFor);

            if (wordToDel == null)
            {
                this.announcer.WordNotFound(lookingFor);

                return;
            }

            var confirmation =
                this.announcer.AskForConfirmation(
                    string.Format(
                        "Are you sure you want to delete word {0}?",
                            lookingFor));

            if (confirmation == false) return;

            this.engine.RemoveWord(wordToDel);
            this.RefreshData();

            this.InitializeEditWordTab();

            this.searchWordTB.Focus();
            this.searchWordTB.SelectAll();
        }

        private void sReloadWord_Click(object sender, RoutedEventArgs e)
        {
            this.InitializeEditWordTab();

            this.LoadWordItems(this.wordToEdit);
        }

        private void searchWordTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;

            this.FindWord_ButtClick(new object(), new RoutedEventArgs());
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.quizzesTab.IsSelected)
            {
                var oldestWordAddedDate = this.engine
                    .GetAllWords()
                    .Min(w => w.DateAdded);

                this.quizStartDate.SelectedDate = oldestWordAddedDate;
            }
        }
    }
}
