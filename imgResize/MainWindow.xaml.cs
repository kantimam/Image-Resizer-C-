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

            // create folder on startup if not exists
            createFolder();

            fillOptions();
            
        }

        // some state
        private List<string> imageFileNames;
        private bool ResizeFinished = false;
        private bool OpenError = false;
        private Regex NumOnly = new Regex("[^0-9]+");

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
                Mode="cover"
            },
            new Option()
            {
                Name="HD",
                Width=1280,
                Height=720,
                Mode="cover"
            },
            new Option()
            {
                Name="Mobile",
                Width=480,
                Height=0,
                Mode="cover"
            },
            new Option()
            {
                Name="Small Thumb",
                Width=128,
                Height=128,
                Mode="crop"
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
            // fill the ListView with the sizeOptions
            this.sizeOptionsList.ItemsSource = settingsList;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.imageFileNames=getImageFiles();

            if (imageFileNames.Count()>0)
            {
                this.ResizeFinished = false;
                this.OpenError = false;
                this.openStatus.IsEnabled=true;
                this.openStatus.Content = "resize "+this.imageFileNames.Count()+" images";
                
            }
            else
            {
                this.OpenError = true;
                this.openStatus.IsEnabled=false;
                this.openStatus.Content = "Could not open file";

            }
        }

        private void ResizeButtonClick(object sender, RoutedEventArgs e)
        {
            // button only works if certain conditions are true clicking it anyway will give you a message inside the button
            if (this.OpenError)
            {
                this.openStatus.Content = "please try opening again";
            }
            else if (this.ResizeFinished)
            {
                this.openStatus.Content = "already finished";
            }
            else if (this.imageFileNames.Count() > 0)
            {
                resizeImagesParallel(this.imageFileNames);
            }
        }
        

        private List<string> getImageFiles()
        {
            // open dialog the get the image files png and jpg is allowed so far
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

        private void resizeImagesParallel(List<string> imgArray)
        {
            // actually resize list of images in parallel
            Parallel.ForEach(imgArray, imgPath =>
            {
                resizeImage(imgPath);
            });
            // update the state of the button after resize finished
            this.openStatus.IsEnabled = false;
            this.openStatus.Content = imageFileNames.Count() + " images resized!";
            this.ResizeFinished = true;
            this.OpenError = false;
        }



        private void resizeImage(string input)
        {

            using (var output = new FileStream(OutputPath(input, outputDirectory), FileMode.Create))
            {
                MagicImageProcessor.ProcessImage(input, output, this.settings);
            }

        }

        

        private string OutputPath(string inputPath, string outputDirectory)
        {
            // create outputpath for the images usually just the image name with resized tagged on
            return System.IO.Path.Combine(
                outputDirectory,                
                System.IO.Path.GetFileNameWithoutExtension(inputPath)
                + "-" + "resized"
                + System.IO.Path.GetExtension(inputPath)
           );
        }

        private void EnableResize()
        {
            // make the resize button clickable again if certain conditions are true
            if (OpenError || !ResizeFinished || imageFileNames.Count < 1) return;
            this.openStatus.IsEnabled = true;
            this.ResizeFinished = false;
        }
      

        private void AllowNumbers(object sender, TextCompositionEventArgs e)
        {
            e.Handled = NumOnly.IsMatch(e.Text);
        }

        private void OptionSelected(object sender, EventArgs e)
        {
            // select option by clicking on the settings presets list
            Option options=(Option)this.sizeOptionsList.SelectedItem;
            // set the custom values to the preset
            this.widthTextBox.Text = options.Width.ToString();
            this.heightTextBox.Text = options.Height.ToString();
            SelectOption(options);
        }


        private void SelectOption(Option option)
        {
            // set the settings to the options from the preset
            if (option.Width != settings.Width || option.Height != settings.Height) EnableResize(); // if settings change re enabled the button
            this.settings.Width = option.Width;
            this.settings.Height = option.Height;
            this.settings.ResizeMode = option.Mode == "crop" ? SetRadioCrop() : SetRadioCover();
           
        }

        CropScaleMode SetRadioCrop()
        {
            this.CoverRadio.IsChecked = false;
            this.CropRadio.IsChecked = true;
            if (CropScaleMode.Crop != settings.ResizeMode) EnableResize(); // if settings change re enabled the button
            return CropScaleMode.Crop;
        }
        CropScaleMode SetRadioCover()
        {
            this.CoverRadio.IsChecked = true;
            this.CropRadio.IsChecked = false;
            if (CropScaleMode.Max != settings.ResizeMode) EnableResize(); // if settings change re enabled the button
            return CropScaleMode.Max;
        }

        void SetModeCover(object sender, EventArgs e)
        {
            this.settings.ResizeMode = SetRadioCover();
        }

        void SetModeCrop(object sender, EventArgs e)
        {
            this.settings.ResizeMode = this.SetRadioCrop();
        }

        void CustomWidthChanged(object sender, TextChangedEventArgs e)
        {
            var textData = sender as TextBox;
            if (textData.Text.Length > 0 && !this.NumOnly.IsMatch(textData.Text))
            {
                int size = int.Parse(textData.Text);
                if (size != settings.Width)
                {
                    this.settings.Width = size; // only set width if it rly changed also re enable the resize button for the same files
                    EnableResize();
                }
            }
            else this.settings.Width = 128;

        }

        void CustomHeightChanged(object sender, TextChangedEventArgs e)
        {
            var textData = sender as TextBox;
            if (textData.Text.Length>0 && !this.NumOnly.IsMatch(textData.Text))
            {
                int size = int.Parse(textData.Text);
                if (size != settings.Height)
                {
                    this.settings.Height = size; // only set height if it rly changed also re enable the resize button for the same files
                    EnableResize();
                }
            }
            //else MessageBox.Show("only positive numbers allowed");
            else this.settings.Height = 128;
        }
    }


    public class SettingsValueConverter : IValueConverter
    {
        // convert height and width inside settings to "auto" if its 0 or lower. Just calculate this size by using the dimensions of the real image
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = value.ToString(); 
            return val==null || int.Parse(val)<1 ? "auto" : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value as string == "auto" ? "0" : value;
        }
    }
}
