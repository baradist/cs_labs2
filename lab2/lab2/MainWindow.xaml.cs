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

namespace lab2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string EXTENTION = "*.jpg";
        object dummyNode = null;
        private ScaleTransform scaleTransform = new ScaleTransform(3, 3);
        TreeViewItem favorites = new TreeViewItem();
        List<string> favoriteFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            InitTree();
            pictureBox_SelectionChanged(null, null);
        }

        private void InitTree()
        {
            tree.SelectedItemChanged += ShowImages;
            favorites.Header = "Favourites";
            tree.Items.Add(favorites);

            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = GetNewNamedTreeViewItem(s);

                // Добавляем ветку в дерево 
                tree.Items.Add(item);
            }
        }

        public void ShowImages(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem item = (TreeViewItem)tree.SelectedItem;
            if (item == favorites)
            {
                FillPictureBox(favoriteFiles.ToArray());
            }
            else if (!IsDirectory(item.Tag.ToString()))
            {
                FillPictureBox(item.Tag.ToString());
            }
            else
            {
                try
                {
                    var files = Directory.GetFiles(item.Tag.ToString(), EXTENTION);
                    FillPictureBox(files);
                }
                catch (Exception)
                {
                    FillPictureBox();
                }
                
            }
        }

        private void FillPictureBox(params string[] files)
        {
            pictureBox.Items.Clear();
            foreach (var file in files)
            {
                pictureBox.Items.Add(GetPictureBoxItem(file));
            }
        }

        private ListBoxItem GetPictureBoxItem(string file)
        {
            ListBoxItem item = new ListBoxItem();
            item.Padding = new Thickness(3, 8, 3, 8);
            Image image = new Image();
            // Исходный размер 
            image.Height = 35;
            // Сохраняем путь к файлу 
            item.Tag = file;

            // Обработчик двойного щелчка – показываем картинку (н‐р, в отдельном окошке)
            item.MouseDoubleClick += delegate { ShowPhoto(); };

            // Для возможности преобразований картинки (повороты, изменение размера)
            // для каждой картинки устанавливаем свойство LayoutTransform
            // в случае двух преобразований необходимо использовать «группу»
            TransformGroup tg = new TransformGroup();
            // В эту группу поместим объект типа ScaleTranform для возможности изменять размер
            // каждая картинка сохраняет ссылку на один и тот же объект scaleTranform  
            // объявленный как элемент класса: var scaleTranform = new ScaleTransform(3, 3);
            tg.Children.Add(scaleTransform);
            // отдельный объект для возможности поворачивать картинку 
            tg.Children.Add(new RotateTransform());
            item.LayoutTransform = tg;
            // Объект uri относится к типу Uri и формируется по пути к файлу.
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = new Uri(file);
            bi.EndInit();
            image.Source = bi;
            item.Content = image;
            // Свойство ToolTip настраивает «подсказку» при наведении мышкой на элемент 
            //  Можно инициализировать любым объектом 
            // (н‐р, строка или контейнер из нескольких элементов)
            item.ToolTip = file;
            return item;
        }

        private void ShowPhoto()
        {
            var selectedPic = (ListBoxItem)pictureBox.SelectedItem;
            WindowViewPic viewPic = new WindowViewPic(selectedPic.Tag.ToString());
            viewPic.ShowDialog();
        }

        private TreeViewItem GetNewNamedTreeViewItem(string tag)
        {
            int i = tag.LastIndexOf('\\') + 1;
            string header = tag.Substring(i, tag.Length - i);
            // Отдельный элемент дерева (ветка)
            TreeViewItem item = new TreeViewItem();

            // В заголовке храним «короткое имя папки»
            item.Header = header == "" ? tag : header;
            // В тэге храним полный путь (в случае дисков Header и Tag совпадают)
            item.Tag = tag;
            if (IsDirectory(tag))
            {
                // Изначально ветка не содержит подкаталогов 
                // Используем «пустышку» ‐  object dummyNode = null;
                item.Items.Add(dummyNode);
                // Обработчик «раскрытия» каталога – нужно загрузить подкаталоги 
                item.Expanded += folder_Expanded;
            }
                
            return item;
        }

        private bool IsDirectory(string tag)
        {
            try
            {
                return (File.GetAttributes(tag) & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (!(item.Items.Count == 1 && item.Items[0] == dummyNode)
                || item.Parent == favorites)
            {
                return;
            }
            item.Items.Clear();

            foreach (var d in Directory.GetDirectories(item.Tag.ToString())) {
                
                item.Items.Add(GetNewNamedTreeViewItem(d));
            }
        }

        private void pictureBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            menuItemRename.IsEnabled = (pictureBox.SelectedItem != null);
            menuItemFavourites.IsEnabled = (pictureBox.SelectedItem != null);
            menuItemEdit.IsEnabled = (pictureBox.SelectedItem != null);
        }

        private void zoomPopup_MouseLeave(object sender, RoutedEventArgs e)
        {
            zoomPopup.IsOpen = false;
        }

        private void zoomSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            scaleTransform.ScaleX = zoomSlider.Value;
            scaleTransform.ScaleY = zoomSlider.Value;
        }

        private void ZoomButton_Click(object sender, RoutedEventArgs e)
        {
            zoomPopup.IsOpen = true;
        }

        private void MenuItemRename_Click(object sender, RoutedEventArgs e)
        {
            var selectedPic = (ListBoxItem)pictureBox.SelectedItem;
            WindowRename rename = new WindowRename(this, selectedPic.Tag.ToString());
            rename.ShowDialog();
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedPic = (ListBoxItem)pictureBox.SelectedItem;
            var r = MessageBox.Show("Do you really want to delete a file: \n" + selectedPic.Tag.ToString(), "Delete " + selectedPic.Tag.ToString(), MessageBoxButton.OKCancel);
            if (r == MessageBoxResult.OK)
            {
                File.Delete(selectedPic.Tag.ToString());
                ShowImages(null, null);
            }
        }

        private void MenuItemAddToFavourites_Click(object sender, RoutedEventArgs e)
        {
            var selectedPic = (ListBoxItem)pictureBox.SelectedItem;
            AddToFavourites(selectedPic.Tag.ToString());
        }

        private void AddToFavourites(string fileName)
        {
            favoriteFiles.Add(fileName);
            favorites.Items.Add(GetNewNamedTreeViewItem(fileName));
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedPic = (ListBoxItem)pictureBox.SelectedItem;
            System.Diagnostics.Process.Start("mspaint.exe", selectedPic.Tag.ToString());
        }

        private void ButtonRotateLeft_Click(object sender, RoutedEventArgs e)
        {
            var selectedPic = (ListBoxItem)pictureBox.SelectedItem;
            if (selectedPic == null) return;
            var tg = (TransformGroup) selectedPic.LayoutTransform;
            var rt = (RotateTransform) tg.Children[1];
            rt.Angle -= 90;
        }

        private void ButtonRotateRight_Click(object sender, RoutedEventArgs e)
        {
            var selectedPic = (ListBoxItem)pictureBox.SelectedItem;
            if (selectedPic == null) return;
            TransformGroup tg = (TransformGroup)selectedPic.LayoutTransform;
            RotateTransform rt = (RotateTransform)tg.Children[1];
            rt.Angle += 90;
        }
    }
}
