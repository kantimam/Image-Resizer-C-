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
            //openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image files (*.jpg)|*.png|All files (*.*)|*.*";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            var fileNames = new List<String>();

            var result = openFileDialog.ShowDialog();

            if (result == false)
            {
                return;
                //string filename = openFileDialog.FileName;
            }



            if (result!=false)
            {
                //var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                this.openStatus.Text = "Picked photo" + openFileDialog.FileName;
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

                

                //MagicImageProcessor.ProcessImage(openFileDialog, new FileStream(outPath, FileMode.Create), settings);
                //string outPath=await createOutputPath("resize");
                //this.openStatus.Text = "outDir";
                resizeImage(openFileDialog.FileName, 300);
            }
            else
            {
                //
                this.openStatus.Text = "Could not open file";

            }
        }

        

        async private void resizeImage(string input, int size)
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

            //var outputStream = new FileStream(outputPath.Path, FileMode.Create);
            //var outStream = await outputPath.OpenStreamForWriteAsync();

            //var outStream = await outputPath.OpenAsync(FileAccessMode.ReadWrite);

            //var ioStream = await outputPath.OpenStreamForWriteAsync();

            //MagicImageProcessor.ProcessImage(input, ioStream, settings);



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
