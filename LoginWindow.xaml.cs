namespace MyDictionary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    using MyDictionary.Classes;
    using MyDictionary.Interfaces;
    using MyDictionary.Enums;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const string DefaultTextBoxText = "Username";
        private const string UsernamesFilePath = "usernames.txt";

        private IAnnouncer announcer;
        private IList<string> users;

        public LoginWindow()
        {
            this.InitializeComponent();

            this.announcer = new WPFannouncer();
            this.InitializeData();
        }

        private void InitializeData()
        {
            this.users = this.ReadUsers();
            userSelectCB.ItemsSource = this.users;
        }

        private IList<string> ReadUsers()
        {
            var result = new List<string>();

            if (File.Exists(UsernamesFilePath))
            {
                using (var sr = new StreamReader(UsernamesFilePath))
                {
                    string line = string.Empty;

                    while ((line = sr.ReadLine()) != null)
                    {
                        result.Add(line);
                    }
                }
            }

            return result;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var txtBox = (TextBox)sender;

            txtBox.Text = string.Empty;

            txtBox.GotFocus -= this.TextBox_GotFocus;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userName = (string)userSelectCB.SelectedItem;

            if (string.IsNullOrWhiteSpace(userName))
            {
                this.announcer.UserNotSelected();
                return;
            }

            var newWindow = new MasterWindow(userName);

            newWindow.Activate();
            newWindow.Show();

            this.Close();
        }

        // Add username Button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = newUsernameTB.Text;

            if (this.users.Contains(newUsername))
            {
                MessageBox.Show(
                    "Username already exists. Please select it from the list.");

                return;
            }

            if (string.IsNullOrWhiteSpace(newUsername) == false &&
                newUsername != DefaultTextBoxText)
            {
                var sw = new StreamWriter("usernames.txt", true);
                sw.WriteLine(newUsername);
                sw.Dispose();

                this.InitializeData();
            }
        }
    }
}
