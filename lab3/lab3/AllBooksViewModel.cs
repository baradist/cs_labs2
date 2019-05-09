using lab3.model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace lab3
{
    class AllBooksViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private BaseCommand addCommand;
        public BaseCommand AddCommand
        {
            get
            {
                return addCommand ??
                 (addCommand = new BaseCommand(obj =>
                 {
                     Book Book = new Book ();
                     Books.Add(Book);
                     SelectedBook = Book;
                 }));
            }
        }

        private BaseCommand removeCommand;
        public BaseCommand RemoveCommand
        {
            get
            {
                return removeCommand ??
                 (removeCommand = new BaseCommand(obj =>
                 {
                     Books.Remove(SelectedBook);
                 },
                 (p) => SelectedBook != null));
            }
        }

        private BaseCommand saveCommand;
        public BaseCommand SaveCommand
        {
            get
            {
                return saveCommand ??
                 (saveCommand = new BaseCommand(obj =>
                 {
                     XElement x = new XElement("Books",
                          from book in Books
                          select new XElement("Book",
                           new XAttribute("Year", book.Year),
                           new XElement("Title", book.Title),
                           new XElement("Author", book.Author)));
                    string s = x.ToString();

                     SaveFileDialog dialog = new SaveFileDialog();
                     dialog.Filter = "XML file|*.xml";
                     dialog.InitialDirectory = Environment.CurrentDirectory;
                     if (dialog.ShowDialog() == true)
                     {
                         string fileName = dialog.FileName;
                         File.WriteAllText(fileName, s);
                     }
                 }));
            }
        }

        private BaseCommand loadCommand;
        public BaseCommand LoadCommand
        {
            get
            {
                return loadCommand ??
                 (loadCommand = new BaseCommand(obj =>
                 {
                     OpenFileDialog dialog = new OpenFileDialog();
                     dialog.Filter = "XML file|*.xml";
                     dialog.InitialDirectory = Environment.CurrentDirectory;
                     if (dialog.ShowDialog() == true)
                     {
                         string fileName = dialog.FileName;
                         string text = File.ReadAllText(fileName);
                         var x = XElement.Parse(text);
                         var q = from e in x.Elements()
                                 select new Book { Title = e.Element("Title").Value,
                                     Author = e.Element("Author").Value,
                                 Year = int.Parse(e.Attribute("Year").Value) };
                         Books.Clear();
                         foreach (var b in q)
                         {
                             Books.Add(b);
                         }
                     }
                 }));
            }
        }

        // скрытое поле, необходимо реализовать открытое свойство SelectedBook
        private Book selectedBook;

        public Book SelectedBook
        {
            get { return selectedBook; }
            set
            {
                selectedBook = value;
                OnPropertyChanged("SelectedBook");
            }
        }

        public ObservableCollection<Book> Books { get; set; }

        public AllBooksViewModel()
        {
            Books = new ObservableCollection<Book>
               {
                 new Book {Title="WPF Unleashed", Author="Adam Natan", Year=2012 },
                 new Book {Title="F# for Machine Learning", Author="Sudipta Mukherjee", Year=2016 },
                 new Book {Title="F# for Fun and Profit", Author="Scott Wlaschin", Year=2015 },
                 new Book {Title="Learning C# by Developing Games with Unity 3D",
                    Author="Terry Norton", Year=2013 }
               };
        }
    }
}
