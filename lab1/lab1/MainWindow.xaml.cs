using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string EXTENTION = "*.txt";
        BackgroundWorker worker;
        string path = ".";
        List<Tuple<int, int>> listWordsLengths;
        List<string> listFrequency;
        List<string> listLength;
        Stopwatch stopwatch = new Stopwatch();
        bool IsParallelMode = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonChoose_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    labelPath.Content = dialog.SelectedPath;
                    path = dialog.SelectedPath;
                }
            }
        }

        private void RefreshLists(System.Windows.Controls.ListBox listBox, List<string> list)
        {
            listBox.ItemsSource = list;
        }

        private void RefreshChart(List<Tuple<int, int>> list)
        {
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(new ChartArea("defaultArea"));
            chart.Series.Clear();
            chart.Series.Add(new Series("Series1"));
            chart.Series["Series1"].ChartArea = "defaultArea";
            chart.Series["Series1"].ChartType = SeriesChartType.Column;
            chart.DataSource = list;
            chart.Series["Series1"].XValueMember = "Item1";
            chart.Series["Series1"].YValueMembers = "Item2";
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            RunHeavyTask();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            worker.CancelAsync();
            stopwatch.Reset();
            labelSpentTime.Content = "";
        }

        private void RunHeavyTask()
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += VeryLongComputation;
            worker.RunWorkerCompleted += (sender, args) => {
                progressBar1.Value = 0;
                RefreshChart(listWordsLengths);
                RefreshLists(listBoxFrequency, listFrequency);
                RefreshLists(listBoxLength, listLength);
                stopwatch.Stop();
                labelSpentTime.Content = "Milliseconds: " + stopwatch.ElapsedMilliseconds;

            };
            worker.ProgressChanged += (sender, args) => progressBar1.Value = args.ProgressPercentage;
            worker.RunWorkerAsync();
            stopwatch.Restart();
        }

        private void VeryLongComputation(object sender, DoWorkEventArgs e)
        {
            var files = Directory.EnumerateFiles(path, EXTENTION);
            int filesAmount = files.Count();
            int count = 0;
            if (IsParallelMode)
            {
                var words = files.AsParallel()
                .SelectMany(file =>
                {
                    Interlocked.Increment(ref count);
                    worker.ReportProgress((int) 100.0 * count / filesAmount );
                    return Regex.Split(File.ReadAllText(file), @"\W+");
                });

                listWordsLengths = words
                    .GroupBy(w => w.Length)
                    .Select(g => new Tuple<int, int>(g.Key, g.Count()))
                    .ToList();

                listFrequency = words
                    .GroupBy(w => w)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key + ": " + g.Count())
                    .Take(5)
                    .ToList();

                listLength = words
                    .GroupBy(w => w)
                    .OrderByDescending(g => g.Key.Length)
                    .Select(g => g.Key + ": " + g.Key.Length)
                    .Take(5)
                    .ToList();
            }
            else
            {
                var words = files
                .SelectMany(file =>
                {
                    Interlocked.Increment(ref count);
                    worker.ReportProgress((int)100.0 * count / filesAmount);
                    return Regex.Split(File.ReadAllText(file), @"\W+");
                });

                listWordsLengths = words
                    .GroupBy(w => w.Length)
                    .Select(g => new Tuple<int, int>(g.Key, g.Count()))
                    .ToList();

                listFrequency = words
                    .GroupBy(w => w)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key + ": " + g.Count())
                    .Take(5)
                    .ToList();

                listLength = words
                    .GroupBy(w => w)
                    .OrderByDescending(g => g.Key.Length)
                    .Select(g => g.Key + ": " + g.Key.Length)
                    .Take(5)
                    .ToList();
            }
            worker.ReportProgress(99);
        }

        private void checkBoxIsParallel_Checked(object sender, RoutedEventArgs e)
        {
            IsParallelMode = checkBoxIsParallel.IsChecked.Value;
        }

        private void checkBoxIsParallel_Unchecked(object sender, RoutedEventArgs e)
        {
            IsParallelMode = checkBoxIsParallel.IsChecked.Value;
        }
    }
}
