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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace KinectSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BrowseButton1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Info1 = new OpenFileDialog();
            Info1.InitialDirectory = "c:\\";
            Info1.Filter = "(*.txt)|*.txt|(*.png)|*.png|(*.jpg)|*.jpg";
            Info1.RestoreDirectory = true;

            if (Info1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = Info1.FileName;
                InfoLabel1.Content = file;
            }
        }

        private void BrowseButton2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog Info2 = new OpenFileDialog();
            Info2.InitialDirectory = "c:\\";
            Info2.Filter = "(*.txt)|*.txt|(*.png)|*.png|(*.jpg)|*.jpg";
            Info2.RestoreDirectory = true;

            if (Info2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = Info2.FileName;
                InfoLabel2.Content = file;
            }
        }
    }
}
