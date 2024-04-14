using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace TextFile
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ProcessFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var punctuationChecked = RemovePunctuationCheckBox.IsChecked ?? false;
                var minLength = int.TryParse(MinLengthTextBox.Text, out int length) ? length : 0;

                await ProcessFilesAsync(openFileDialog.FileNames, punctuationChecked, minLength);

                MessageBox.Show("Файл(ы) обработаны.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task ProcessFilesAsync(string[] fileNames, bool removePunctuation, int minLength)
        {
            foreach (var fileName in fileNames)
            {
                string text = await ReadAllTextAsync(fileName);
                if (removePunctuation)
                {
                    text = RemovePunctuation(text);
                }
                if (minLength > 0)
                {
                    text = RemoveShortWords(text, minLength);
                }
                var newFileName = Path.Combine(Path.GetDirectoryName(fileName), "Processed_" + Path.GetFileName(fileName));
                await WriteAllTextAsync(newFileName, text);
            }
        }

        private async Task<string> ReadAllTextAsync(string fileName)
        {
            using (var reader = new StreamReader(fileName))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private async Task WriteAllTextAsync(string fileName, string text)
        {
            using (var writer = new StreamWriter(fileName))
            {
                await writer.WriteAsync(text);
            }
        }
        

        private string RemovePunctuation(string text)
        {
            return Regex.Replace(text, @"\p{P}", "");
        }

        private string RemoveShortWords(string text, int minLength)
        {
            return string.Join(" ", text.Split(' ').Where(word => word.Length >= minLength));
        }

        private async void SaveFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var folderPath = Path.GetDirectoryName(saveFileDialog.FileName);
                var filesToSave = Directory.GetFiles(folderPath, "Processed_*.txt");

                foreach (var fileToSave in filesToSave)
                {
                    var newFilePath = Path.Combine(saveFileDialog.FileName, Path.GetFileName(fileToSave));
                    await CopyFileAsync(fileToSave, newFilePath);
                }

                MessageBox.Show("Файл(ы) сохранены.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task CopyFileAsync(string sourceFileName, string destFileName)
        {
            using (var sourceStream = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (var destStream = new FileStream(destFileName, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.CopyToAsync(destStream);
            }
        }
    }
}