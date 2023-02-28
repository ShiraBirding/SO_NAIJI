using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace SO_NAIJI
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyBindingSource bindsource = new MyBindingSource();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = bindsource;
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.txtCustCode = this.txtCustCode.Text;
            Properties.Settings.Default.txtCSVword = this.txtCSVword.Text;
            Properties.Settings.Default.txtDeli = this.txtDeli.Text;
            Properties.Settings.Default.txtCustSoNo = this.txtCustSoNo.Text;
            Properties.Settings.Default.checkInputHeader = (bool)this.checkInputHeader.IsChecked;
            Properties.Settings.Default.checkOutputHeader = (bool)this.checkOutputHeader.IsChecked;
            Properties.Settings.Default.txtHeaderPath = this.txtHeaderPath.Text;
            Properties.Settings.Default.txtInputPath = this.txtInputPath.Text;
            Properties.Settings.Default.txtOutputPath = this.txtOutputPath.Text;
            Properties.Settings.Default.checkCopyPDF = (bool)this.checkCopyPDF.IsChecked;
            Properties.Settings.Default.checkMoveFolder = (bool)this.checkMoveFolder.IsChecked;
            Properties.Settings.Default.txtOrderDate = this.txtOrderDate.Text;
            Properties.Settings.Default.txtPDFstate = this.txtPDFstate.Text;
            Properties.Settings.Default.txtPDFtag = this.txtPDFtag.Text;
            Properties.Settings.Default.txtPDForder = this.txtPDForder.Text;
            Properties.Settings.Default.Save();
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            Main main = new Main();
            main._SOdate = this.SOdate.Text.Replace("/", "");
            main._CustCode = this.txtCustCode.Text;
            main._strCSVword = this.txtCSVword.Text;
            main._strDelimiter = this.txtDelimiter.Text;
            main._CustSoNoName = this.txtCustSoNo.Text;
            main._DeliName = this.txtDeli.Text;
            main._boolHasHeader = (bool)this.checkInputHeader.IsChecked;
            main._boolWriteHeader = (bool)this.checkOutputHeader.IsChecked;
            main._boolMoveFolder = (bool)this.checkMoveFolder.IsChecked;
            main._HeaderFilePath = this.txtHeaderPath.Text;
            main._InputFolderPath = this.txtInputPath.Text;
            main._OutputFolderPath = this.txtOutputPath.Text;
            main._boolCopyPDF = (bool)this.checkCopyPDF.IsChecked;
            main._txtOrderDate = this.txtOrderDate.Text;
            main._strPDFstate = this.txtPDFstate.Text;
            main._strPDFtag = this.txtPDFtag.Text;
            main._strPDForder = this.txtPDForder.Text;
            main.MainExecute();
        }

        private void checkCopyPDF_Checked(object sender, RoutedEventArgs e)
        {
            if(this.txtOrderDate != null)
            {
                this.txtOrderDate.IsEnabled = true;
                this.txtPDFstate.IsEnabled = true;
                this.txtPDFtag.IsEnabled = true;
                this.txtPDForder.IsEnabled = true;
            }
        }

        private void checkCopyPDF_Unchecked(object sender, RoutedEventArgs e)
        {
            this.txtOrderDate.IsEnabled = false;
            this.txtPDFstate.IsEnabled = false;
            this.txtPDFtag.IsEnabled = false;
            this.txtPDForder.IsEnabled = false;
        }
    }

    public class MyBindingSource
    {
        public DateTime MonthLaterDate { get; set; } = DateTime.Today.AddMonths(1);
    }
}

