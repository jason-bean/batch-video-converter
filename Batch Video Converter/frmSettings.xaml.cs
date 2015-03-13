using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Batch_Video_Converter
{
    /// <summary>
    /// Interaction logic for frmSettings.xaml
    /// </summary>
    public partial class frmSettings : Window
    {
        public frmSettings()
        {
            InitializeComponent();
        }

        private void btnHBBrowse_Click(object sender, RoutedEventArgs e)
        {
            //set open file dialog properties

            {
                Microsoft.Win32.OpenFileDialog openFD = new Microsoft.Win32.OpenFileDialog();

                openFD.Title = "Path to HandBrakeCLI.exe";
                openFD.Filter = "HandBrakeCLI.exe|HandBrakeCLI.exe";

                //set initial directory if path exists in textbox
                if (txtHBPath.Text != string.Empty)
                {
                    openFD.InitialDirectory = System.IO.Path.GetDirectoryName(txtHBPath.Text);
                }

                //show dialog
                openFD.FilterIndex = 1;

                Nullable<bool> result = openFD.ShowDialog();

                if (result == true)
                {
                    //set textbox if ok is pressed
                    txtHBPath.Text = openFD.FileName;
                }
            }
        }

        private void btnMp4boxBrowse_Click(object sender, RoutedEventArgs e)
        {
            //set open file dialog properties

            Microsoft.Win32.OpenFileDialog openFD = new Microsoft.Win32.OpenFileDialog();

            openFD.Title = "Path to MP4Box.exe";
            openFD.Filter = "MP4Box.exe|MP4Box.exe";

            //set initial directory if path exists in textbox
            if (txtMp4boxPath.Text != string.Empty)
            {
                openFD.InitialDirectory = System.IO.Path.GetDirectoryName(txtMp4boxPath.Text);
            }

            openFD.FilterIndex = 1;
            //show dialog and if ok is pressed set textbox

            Nullable<bool> result = openFD.ShowDialog();

            if (result == true)
            {
                txtMp4boxPath.Text = openFD.FileName;
            }
        }

        private void btnParsleyPath_Click(object sender, RoutedEventArgs e)
        {
            //set open file dialog properties

            Microsoft.Win32.OpenFileDialog openFD = new Microsoft.Win32.OpenFileDialog();

            openFD.Title = "Path to AtomicParsley.exe";
            openFD.Filter = "AtomicParsley.exe|AtomicParsley.exe";

            //set initial directory if path exists in textbox
            if (txtAtomicParsleyPath.Text != string.Empty)
            {
                openFD.InitialDirectory = System.IO.Path.GetDirectoryName(txtAtomicParsleyPath.Text);
            }

            openFD.FilterIndex = 1;
            //show dialog and if ok is pressed set textbox

            Nullable<bool> result = openFD.ShowDialog();

            if (result == true)
            {
                txtAtomicParsleyPath.Text = openFD.FileName;
            }
        }

        private void btnOutputBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog browse = new FolderBrowserDialog();

            //set browse dialog properties
            browse.Description = "Please select an output folder";
            browse.SelectedPath = txtOutputPath.Text;

            //show dialog and if ok is pressed set textbox

            if (browse.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtOutputPath.Text = browse.SelectedPath;
            }
        }

        private void Checked(object sender, RoutedEventArgs e)
        {
            chkDeleteOutfile.IsEnabled = true;
        }

        private void Unchecked(object sender, RoutedEventArgs e)
        {
            chkDeleteOutfile.IsEnabled = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //load settings from public variables
            txtHBPath.Text = Functions.HBPath;
            txtMp4boxPath.Text = Functions.mp4boxPath;
            txtAtomicParsleyPath.Text = Functions.APPath;
            txtOutputPath.Text = Functions.OutPath;
            chkRemoveAds.IsChecked = Functions.RemoveAds;
            chkAutoCrop.IsChecked = Functions.AutoCrop;
            chkMetadata.IsChecked = Functions.AutoTVMeta;
            chkMoviePoster.IsChecked = Functions.AutoMoviePoster;
            chkImportiTunes.IsChecked = Functions.AutoImportiTunes;
            if (chkImportiTunes.IsChecked == true)
            {
                chkDeleteOutfile.IsEnabled = true;
            }
            else
            {
                chkDeleteOutfile.IsEnabled = false;
            }
            chkDeleteOutfile.IsChecked = Functions.AutoDeleteExport;

            for (int x = 0; x <= (Functions.EncodeProfiles.Length - 1); x++)
            {
                cmbEncodeProfile.Items.Add(Functions.EncodeProfiles[x]);
            }
            cmbEncodeProfile.SelectedIndex = Functions.DefaultEncodeProfile;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey jbkey = default(RegistryKey);
            RegistryKey bvckey = default(RegistryKey);

            //set public removeads variable appropriately
            if (chkRemoveAds.IsChecked == true)
            {
                Functions.RemoveAds = true;
            }
            else
            {
                Functions.RemoveAds = false;
            }

            //set public autocrop variable
            if (chkAutoCrop.IsChecked == true)
            {
                Functions.AutoCrop = true;
            }
            else
            {
                Functions.AutoCrop = false;
            }

            if (chkMetadata.IsChecked == true)
            {
                Functions.AutoTVMeta = true;
            }
            else
            {
                Functions.AutoTVMeta = false;
            }

            if (chkMoviePoster.IsChecked == true)
            {
                Functions.AutoMoviePoster = true;
            }
            else
            {
                Functions.AutoMoviePoster = false;
            }

            if (chkImportiTunes.IsChecked == true)
            {
                Functions.AutoImportiTunes = true;
            }
            else
            {
                Functions.AutoImportiTunes = false;
            }

            if (chkDeleteOutfile.IsChecked == true)
            {
                Functions.AutoDeleteExport = true;
            }
            else
            {
                Functions.AutoDeleteExport = false;
            }

            if (cmbEncodeProfile.SelectedIndex != -1)
            {
                Functions.DefaultEncodeProfile = cmbEncodeProfile.SelectedIndex;
            }
            else
            {
                System.Windows.MessageBox.Show("Please select an encoding profile.");
                return;
            }

            //set registry values and public variables for each setting
            Functions.HBPath = txtHBPath.Text;
            Functions.mp4boxPath = txtMp4boxPath.Text;
            Functions.APPath = txtAtomicParsleyPath.Text;
            Functions.OutPath = txtOutputPath.Text;

            jbkey = Registry.CurrentUser.OpenSubKey(@"Software\JasonBean", true);
            bvckey = Registry.CurrentUser.OpenSubKey(@"Software\JasonBean\BVC", true);

            if (jbkey == null)
            {
                Registry.CurrentUser.CreateSubKey(@"Software\JasonBean");
            }

            if (bvckey == null)
            {
                Registry.CurrentUser.CreateSubKey(@"Software\JasonBean\BVC");
                bvckey = Registry.CurrentUser.OpenSubKey(@"Software\JasonBean\BVC", true);
            }

            bvckey.SetValue("HBPath", txtHBPath.Text);
            bvckey.SetValue("mp4boxPath", txtMp4boxPath.Text);
            bvckey.SetValue("APPath", txtAtomicParsleyPath.Text);
            bvckey.SetValue("OutPath", txtOutputPath.Text);
            bvckey.SetValue("RemoveAds", Functions.RemoveAds.ToString());
            bvckey.SetValue("AutoCrop", Functions.AutoCrop.ToString());
            bvckey.SetValue("AutoMeta", Functions.AutoTVMeta.ToString());
            bvckey.SetValue("AutoMoviePoster", Functions.AutoMoviePoster.ToString());
            bvckey.SetValue("AutoImportiTunes", Functions.AutoImportiTunes.ToString());
            bvckey.SetValue("AutoDeleteExport", Functions.AutoDeleteExport.ToString());
            bvckey.SetValue("EncodeProfile", Functions.DefaultEncodeProfile);

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
