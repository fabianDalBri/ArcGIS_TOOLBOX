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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FinalProject
{
    /// <summary>
    /// Interaction logic for form.xaml
    /// </summary>
    public partial class form : Window
    {
        public form()
        {
            InitializeComponent();
            //first push
        }
        public string ChooseFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            ofd.InitialDirectory = @"H:\";
            //MessageBox.show(ofd.SafeFileName);
            ofd.Filter = "Files (*.png;*.shp;*.tif)|*.png;*.shp;*.tif|All files(*.*) | *.* ";

            return ofd.FileName;

        }
        private void Bebyggelse_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Markdata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hojddata_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Vagdata_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
