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

namespace Batch_Video_Converter
{
    /// <summary>
    /// Interaction logic for frmTitleSelect.xaml
    /// </summary>
    public partial class frmTitleSelect : Window
    {
        public frmTitleSelect()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (lstTitles.SelectedIndex != -1)
            {
                if (lstTitles.SelectedIndex > 0)
                {
                    Functions.SelectedID[0] = Functions.Selections[(lstTitles.SelectedIndex - 1), 2];
                    Functions.SelectedID[1] = Functions.Selections[(lstTitles.SelectedIndex - 1), 0];
                    this.Close();
                }
                else
                {
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("Please Select a Title to Continue");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            int SelectLength = (Functions.Selections.Length / 3);

            txtInputTitle.Content = Functions.SearchedTitle;
            lstTitles.Items.Clear();
            lstTitles.Items.Add("No Titles Match");
            for (int Title = 0; Title <= (SelectLength - 1); Title++)
            {
                if (!string.IsNullOrEmpty(Functions.Selections[Title, 1]))
                {
                    lstTitles.Items.Add(Functions.Selections[Title, 0] + " - " + Functions.Selections[Title, 1].Substring(0, 4));
                }
                else
                {
                    lstTitles.Items.Add(Functions.Selections[Title, 0]);
                }

            }
        }
    }
}
