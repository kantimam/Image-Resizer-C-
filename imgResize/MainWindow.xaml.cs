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

            // create the output directory if it does not exist
            var picPath = @"C:\Users\Schwartza\Pictures";
            this.outputDirectory = System.IO.Path.Combine(picPath, "resizedPictures");

            if (!Directory.Exists(this.outputDirectory)){
                Directory.CreateDirectory(this.outputDirectory);
            }
        }

        
        private void Button_Click(object sender, RoutedEventArgs e)
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

            if (result!=false)
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
            var settings = new ProcessImageSettings()
            {
                Width = size,
                Height = size,
                ResizeMode = CropScaleMode.Max,
                SaveFormat = FileFormat.Jpeg,
                JpegQuality = 75,
                JpegSubsampleMode = ChromaSubsampleMode.Subsample420
            };

            using (var output = new FileStream(OutputPath(input, outputDirectory), FileMode.Create))
            {
                MagicImageProcessor.ProcessImage(input, output, settings);
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
    }
}
