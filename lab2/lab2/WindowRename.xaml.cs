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
using System.IO;

namespace lab2
{
    /// <summary>
    /// Interaction logic for WindowRename.xaml
    /// </summary>
    public partial class WindowRename : Window
    {
        string path;
        string fileName;
        MainWindow parent;

        public WindowRename(MainWindow parent, string fileName)
        {
            InitializeComponent();

            this.parent = parent;
            this.fileName = fileName;
            int i = fileName.LastIndexOf('\\') + 1;
            this.path = fileName.Substring(0, i);
            textBoxNewFileName.Text = fileName.Substring(i, fileName.Length - i); ;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            File.Move(fileName, path + textBoxNewFileName.Text);
            parent.ShowImages(null, null);
            Close();
        }
    }
}
