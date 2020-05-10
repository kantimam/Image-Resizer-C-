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
using System.IO;
using PhotoSauce.MagicScaler;
using System.Text.RegularExpressions;
using System.ComponentModel;

public class Option
{
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Mode { get; set; }
}

namespace imgResize
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string outputDirectory;
        public MainWindow()
        {
            InitializeComponent();

            createFolder();

            fillOptions();
            
        }

        // default Settings
        private ProcessImageSettings defaultSetting = new ProcessImageSettings()
        {
            Width = 480,
            Height = 480,
            ResizeMode = CropScaleMode.Max,
            SaveFormat = FileFormat.Jpeg,
            JpegQuality = 75,
            JpegSubsampleMode = ChromaSubsampleMode.Subsample420
        };

        private ProcessImageSettings settings = new ProcessImageSettings()
        {
            Width = 480,
            Height = 480,
            ResizeMode = CropScaleMode.Max,
            SaveFormat = FileFormat.Jpeg,
            JpegQuality = 75,
            JpegSubsampleMode = ChromaSubsampleMode.Subsample420
        };

        private List<Option> settingsList = new List<Option>()
        {
            new Option()
            {
                Name="Full HD",
                Width=1920,
                Height=1080,
                Mode="contain"
            },
            new Option()
            {
                Name="HD",
                Width=1280,
                Height=720,
                Mode="contain"
            },
            new Option()
            {
                Name="Mobile",
                Width=480,
                Height=0,
                Mode="contain"
            }
        }; 

        private void createFolder()
        {
            // create the output directory if it does not exist
            var picPath = @"C:\Users\Schwartza\Pictures";
            this.outputDirectory = System.IO.Path.Combine(picPath, "resizedPictures");

            if (!Directory.Exists(this.outputDirectory))
            {
                Directory.CreateDirectory(this.outputDirectory);
            }
        }

        private void fillOptions()
        {
            this.sizeOptionsList.ItemsSource = settingsList;
        }

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var fileArr=getImageFiles();

            if (fileArr.Count()>0)
            {
                this.openStatus.Text = "selected "+fileArr.Count()+" images";


                // get dimensions from input field
                int width = Int32.Parse(this.widthTextBox.Text);
                resizeImagesParallel(fileArr, width);
                
            }
            else
            {
                //
                this.openStatus.Text = "Could not open file";

            }
        }


        private void Button_Click_Old(object sender, RoutedEventArgs e)
        {
            this.openStatus.Text = "clicked";

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image files (*.jpg)|*.png|All files (*.*)|*.*";

            var fileNames = new List<String>();

            var result = openFileDialog.ShowDialog();

            if (result == false)
            {
                return;
            }

            if (result != false)
            {
                const int size = 200;
                const int quality = 75;

                var settings = new ProcessImageSettings()
                {
                    Width = size,
                    Height = size,
                    ResizeMode = CropScaleMode.Max,
                    SaveFormat = FileFormat.Jpeg,
                    JpegQuality = quality,
                    JpegSubsampleMode = ChromaSubsampleMode.Subsample420
                };


                foreach (string fileName in openFileDialog.FileNames)
                {
                    this.openStatus.Text = "Picked photo" + fileName;
                    resizeImage(fileName, 500);
                }

            }
            else
            {
                //
                this.openStatus.Text = "Could not open file";

            }
        }


        private List<string> getImageFiles()
        {
            this.openStatus.Text = "clicked";

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image files (*.jpg)|*.png|All files (*.*)|*.*";

            var fileNames = new List<string>();

            var result = openFileDialog.ShowDialog();

            if (result != false)
            {
                fileNames = openFileDialog.FileNames.ToList();
            }
            return fileNames;
        }

        private void resizeImagesParallel(List<string> imgArray, int maxSize)
        {
            Parallel.ForEach(imgArray, imgPath =>
            {
                resizeImage(imgPath, maxSize);
            });
        }

        

        private void resizeImage(string input, int size)
        {

            using (var output = new FileStream(OutputPath(input, outputDirectory), FileMode.Create))
            {
                MagicImageProcessor.ProcessImage(input, output, this.settings);
            }

        }

        

        private string OutputPath(string inputPath, string outputDirectory)
        {
            return System.IO.Path.Combine(
                outputDirectory,                
                System.IO.Path.GetFileNameWithoutExtension(inputPath)
                + "-" + "resized"
                + System.IO.Path.GetExtension(inputPath)
           );
        }

        private void heightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void AllowNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void OptionSelected(object sender, EventArgs e)
        {
            Option options=(Option)this.sizeOptionsList.SelectedItem;
            this.openStatus.Text = options.Name;
            this.widthTextBox.Text = options.Width.ToString();
            this.heightTextBox.Text = options.Height.ToString();
            SelectOption(options);
            return;
        }

        private void SelectOption(Option option)
        {
            this.settings.Width = option.Width;
            this.settings.Height = option.Height;
            
            this.settings.ResizeMode =
                option.Mode=="crop" ? CropScaleMode.Crop : CropScaleMode.Max;
            
            
        }
}
}
