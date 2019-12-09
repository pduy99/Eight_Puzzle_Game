using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Eight_Puzzle_Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Setting setting = new Setting(); //Save the setting of current game
        List<string> _listRandomImage = new List<string>(); //Save all picture available in Random mode
        //Count down time
        DispatcherTimer _timer; 
        TimeSpan _time;
        private int Moves; //Save the moves user have made
        int height = 120; // The height of cropped image
        int width = 150; // The width of cropped image
        int startX = 36;  //The margin left of groupbox contain cropped images
        int startY = 36; //The margin top of groupbox contain cropped images
        int[,] _a;  //Array that shows the current position of cropped images
        List<Image> _listCropImage; // List save the cropped images
        bool isPlaying;
        bool isSaved;

        public MainWindow()
        {
            InitializeComponent();
            _a = new int[3,3];
            _listCropImage = new List<Image>();
            Moves = 0;
            isPlaying = false;
            isSaved = true; 
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isSaved)
            {
                MessageBoxResult msgbox = MessageBox.Show("Do you want to save current game ?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (msgbox)
                {
                    case (MessageBoxResult.Cancel):
                        e.Cancel = true;
                        break;
                    case (MessageBoxResult.Yes):
                        BtnSave_Click(this, new RoutedEventArgs());
                        break;
                    case (MessageBoxResult.No):
                        break;
                }
            }
        }

        /// <summary>
        /// Get all picture available in folder "Images" and save in _listRandomImage
        /// </summary>
        private void getAllRandomPictureName()
        {
            
            DirectoryInfo d = new DirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}Images//");
            //Lay tat ca anh co ten bat dau bang "ea"
            FileInfo[] AllPicture = d.GetFiles("*.jpg");
            foreach(FileInfo picture in AllPicture)
            {
                _listRandomImage.Add("Images/" + picture.Name);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            getAllRandomPictureName();
           
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                const string filename = "save.8puzzle";

                var writer = new StreamWriter(filename);
                writer.WriteLine(setting.ImagePath);
                writer.WriteLine(setting.difficulty);
                writer.WriteLine(setting.timer);
                writer.WriteLine(setting.mode);
                writer.WriteLine(Moves);

                //Save the state of puzzle
                for(int i = 0; i < 3; i++)
                {
                    for(int j= 0; j < 3; j++)
                    {
                        writer.Write($"{_a[i,j]}");
                        if(j != 2)
                        {
                            writer.Write(" ");
                        }
                    }
                    writer.WriteLine();
                }
                writer.Close();
                MessageBox.Show("Game saved");
                isSaved = true;
            }
            else
            {
                MessageBox.Show("Nothing to save", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            string filename = "save.8puzzle";
            if(isPlaying)
            {
                ResetCountDown();
            }
            try
            {
                var reader = new StreamReader(filename);

                //Load the setting
                string imagepath = reader.ReadLine();
                string diff_str = reader.ReadLine();
                Difficulty difficulty = diff_str == "Easy" ? Difficulty.Easy : diff_str == "Medium" ? Difficulty.Medium : Difficulty.Hard;
                string time_str = reader.ReadLine();
                int timeleft = Int32.Parse(time_str);
                string mode_str = reader.ReadLine();
                Mode mode = mode_str == "Random_Image" ? Mode.Random_Image : Mode.Select_Image;
                string move = reader.ReadLine();
                Moves = Int32.Parse(move);
                setting = new Setting(imagepath, timeleft, difficulty, mode);

                //Load the state of puzzle
                for(int i=0;i<3; i++)
                {
                    var tokens = reader.ReadLine().Split(new string[] { " " }, StringSplitOptions.None);
                    for(int j = 0; j < 3; j++)
                    {
                        _a[i, j] = int.Parse(tokens[j]);
                    }
                }

                reader.Close();

                MessageBox.Show($"Load game successfully: \n  -You have made {Moves} moves \n  -You have {timeleft} seconds left", "Successfuly");
                isSaved = true;
                PlaySavedGame();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Nothing to load or filename is wrong", "Load Game failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PlaySavedGame()
        {
            if (Setting_Area.Visibility == Visibility.Visible)
            {
                Setting_Area.Visibility = Visibility.Collapsed;
                Fuction_Area.Visibility = Visibility.Visible;
            }

            MoveLabel.Content = Moves;

            isPlaying = true;

            //Set data context cho man hinh chinh
            Timer.DataContext = setting;
            BitmapImage image;
            if (setting.mode == Mode.Random_Image)
            {
                string absolute_imagepath = System.IO.Path.GetFullPath(setting.ImagePath);
                image = new BitmapImage(new Uri(absolute_imagepath, UriKind.Absolute));
            }
            else
            {
                image = new BitmapImage(new Uri(setting.ImagePath, UriKind.RelativeOrAbsolute));
            }
            Original_Image.Source = image;
            TimeCountDown();
            CropImage();
            LoadImagePosition(_a);
        }

        private void BtnNewGame_Click(object sender, RoutedEventArgs e)
        {
            if (isSaved)
            {
                //Stop countdown time and reset timer to 0
                ResetCountDown();
                //Clear the Canvas
                Crop_Image.Children.Clear();
                //Empty the list cropped image
                _listCropImage.Clear();
                //Show the setting for user to make new game
                Setting_Area.Visibility = Visibility.Visible;
                Fuction_Area.Visibility = Visibility.Collapsed;

                isPlaying = false;
                Moves = 0;
                MoveLabel.Content = Moves;
            }
            else
            {
                MessageBoxResult msgbox = MessageBox.Show("Do you want to save current game ?", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (msgbox)
                {
                    case (MessageBoxResult.Cancel):
                        break;
                    case (MessageBoxResult.Yes):
                        BtnSave_Click(this,new RoutedEventArgs());

                        // Stop countdown time and reset timer to 0
                        ResetCountDown();
                        //Clear the Canvas
                        Crop_Image.Children.Clear();
                        //Empty the list cropped image
                        _listCropImage.Clear();
                        //Show the setting for user to make new game
                        Setting_Area.Visibility = Visibility.Visible;
                        Fuction_Area.Visibility = Visibility.Collapsed;

                        isPlaying = false;
                        Moves = 0;
                        MoveLabel.Content = Moves;

                        break;
                    case (MessageBoxResult.No):
                        //Stop countdown time and reset timer to 0
                        ResetCountDown();
                        //Clear the Canvas
                        Crop_Image.Children.Clear();
                        //Empty the list cropped image
                        _listCropImage.Clear();
                        //Show the setting for user to make new game
                        Setting_Area.Visibility = Visibility.Visible;
                        Fuction_Area.Visibility = Visibility.Collapsed;

                        isPlaying = false;
                        Moves = 0;
                        MoveLabel.Content = Moves;

                        break;
                }
            }
        }

        private void BtnRandomImageMode_Click(object sender, RoutedEventArgs e)
        {
            // change the mode;
            btnSelectImageMode.IsChecked = !btnRandomImageMode.IsChecked;

            Random rng = new Random();
            setting.ImagePath = _listRandomImage[rng.Next(_listRandomImage.Count)];
            string absolute_imagepath = System.IO.Path.GetFullPath(setting.ImagePath);
            var image = new BitmapImage(new Uri(absolute_imagepath, UriKind.Absolute));
            ReviewImage.Source = image;
            
        }

        private void BtnSelectImageMode_Click(object sender, RoutedEventArgs e)
        {
            
            btnRandomImageMode.IsChecked = !btnSelectImageMode.IsChecked;

            var screen = new OpenFileDialog();
            if(screen.ShowDialog() == true)
            {
                setting.ImagePath = screen.FileName;
                var source = new BitmapImage(new Uri(setting.ImagePath, UriKind.Absolute));
                ReviewImage.Source = source;
               
                
            }
        }

        private void BtnEasyMode_Click(object sender, RoutedEventArgs e)
        {
            if (btnEasyMode.IsChecked == true)
            {
                btnMediumMode.IsChecked = !btnEasyMode.IsChecked;
                btnHardMode.IsChecked = !btnEasyMode.IsChecked;
                setting.difficulty = Difficulty.Easy;
                setting.setTimer(setting.difficulty);
            }
        }

        private void BtnMediumMode_Click(object sender, RoutedEventArgs e)
        {
            if (btnMediumMode.IsChecked == true)
            {
                btnEasyMode.IsChecked = !btnMediumMode.IsChecked;
                btnHardMode.IsChecked = !btnMediumMode.IsChecked;
                setting.difficulty = Difficulty.Medium;
                setting.setTimer(setting.difficulty);
            }
           
        }

        private void BtnHardMode_Click(object sender, RoutedEventArgs e)
        {
            if (btnHardMode.IsChecked == true)
            {
                btnMediumMode.IsChecked = !btnHardMode.IsChecked;
                btnEasyMode.IsChecked = !btnHardMode.IsChecked;
                setting.difficulty = Difficulty.Hard;
                setting.setTimer(setting.difficulty);

            }
            
        }

        private void Button_PlayClick(object sender, RoutedEventArgs e)
        {
            //Kiem tra xem da setting chua
            if (btnRandomImageMode.IsChecked == false && btnSelectImageMode.IsChecked == false
                || btnEasyMode.IsChecked == false && btnMediumMode.IsChecked == false && btnHardMode.IsChecked == false)
            {
                MessageBox.Show("Invalid setting","Error",MessageBoxButton.OK,MessageBoxImage.Error);
            }
            else
            {
                //Hide the setting
                Setting_Area.Visibility = Visibility.Collapsed;
                Fuction_Area.Visibility = Visibility.Visible;

                MoveLabel.Content = Moves;

                isPlaying = true;
                
                //Set data context cho man hinh chinh
                Timer.DataContext = setting;
                BitmapImage image;
                if (setting.mode == Mode.Random_Image)
                {
                    string absolute_imagepath = System.IO.Path.GetFullPath(setting.ImagePath);
                    image = new BitmapImage(new Uri(absolute_imagepath, UriKind.Absolute));
                }
                else
                {
                    image = new BitmapImage(new Uri(setting.ImagePath, UriKind.RelativeOrAbsolute));
                }
                Original_Image.Source = image;
             
                
                TimeCountDown();
                CropImage();
                RandomCroppedImage();
                               
                
                //this.Title = ($"{_a[0, 0]} : {_a[0, 1]} : {_a[0, 2]} : {_a[1, 0]} : {_a[1, 1]} : {_a[1, 2]} : {_a[2, 0]} : {_a[2, 1]} : {_a[2, 2]}  ");

            }
        }

       

        /// <summary>
        /// Countdown time and show on UI. 
        /// The initial time get from setting.timer
        /// </summary>
        private void TimeCountDown()
        {
            _time = TimeSpan.FromSeconds(setting.timer);
            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if(_time != TimeSpan.Zero)
            {
                _time = _time.Add(TimeSpan.FromSeconds(-1));
                setting.timer = (int)_time.TotalSeconds;
                Timer.Text = _time.ToString("c");
            }
            else
            {
                _timer.Stop();
                MessageBox.Show("Time up");
                DeactivePlaying();
               
            }
        }

        /// <summary>
        /// Cut image into 9 pieces and save in list
        /// </summary>
        private void CropImage()
        {

            var image = new BitmapImage(new Uri(setting.ImagePath, UriKind.RelativeOrAbsolute));

            //This code helps the program can run with any dpi image
            int a;
            if (image.DpiY == 72 && image.DpiX == 72)
            {
                a = 4;
            }
            else
            {
                a = 3;
            }
            var h = (int)image.Height / a;
            var w = (int)image.Width / a;

            //Crop image into 9 pieces
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!((i == 2) && (j == 2)))
                    {
                        var rect = new Int32Rect(j * w, i * h, w, h);
                        CroppedBitmap cropBitmap = null;
                        try
                        {
                            cropBitmap = new CroppedBitmap(image, rect);
                        } catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }

                        var cropImage = new Image();
                        cropImage.Stretch = Stretch.Fill;
                        cropImage.Width = width;
                        cropImage.Height = height;
                        cropImage.Source = cropBitmap;
                        cropImage.MouseLeftButtonDown += CropImage_MouseLeftButtonDown;
                        cropImage.PreviewMouseLeftButtonUp += CropImage_PreviewMouseLeftButtonUp;

                        int order = i * 3 + j;
                        cropImage.Tag = order;

                        _listCropImage.Add(cropImage);
                    }
                }
            }
        }

        //Set random position of cropped images
        private void RandomCroppedImage()
        {
            //Clone a new list from _listCropImage
            List<Image> _listImage = new List<Image>();
            foreach(Image image in _listCropImage)
            {
                _listImage.Add(image);
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!((i == 2) && (j == 2)))
                    {
                        var rnd = new Random();
                        int index = rnd.Next(_listImage.Count);
                        Crop_Image.Children.Add(_listImage[index]);
                        Canvas.SetLeft(_listImage[index], j * (width + 2));
                        Canvas.SetTop(_listImage[index], i * (height + 2));

                        _a[i, j] = (int)_listImage[index].Tag;
                        _listImage.RemoveAt(index);
                    }
                    else
                    {
                        _a[i, j] = -1;
                    }
                }
            }
            if (!isSolvable(_a))
            {
                Crop_Image.Children.Clear();
                RandomCroppedImage();
            }
        }

        private void LoadImagePosition(int[,] arr)
        {
            Crop_Image.Children.Clear();
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if (arr[i, j] != -1)
                    {
                        Image image = FindImageByTag(arr[i, j]);
                        Crop_Image.Children.Add(image);
                        Canvas.SetLeft(image, j * (width + 2));
                        Canvas.SetTop(image, i * (height + 2));
                    }
                }
            }
        }


        bool isDragging = false;
        Image selectedImage = null;
        Point lastPos;
        Point currentPos;

        private void CropImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            selectedImage = sender as Image;
            lastPos = e.GetPosition(this);
            currentPos = e.GetPosition(this);
            
        }

        private void CropImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var position = e.GetPosition(this);

            int x = (int)(position.X - startX) / (width + 2) * (width + 2);
            int y = (int)(position.Y - startY) / (height + 2) * (height + 2);

            int i = y / height;
            int j = x / width;

            int p = ((int)(lastPos.X - startX) / (width + 2) * (width + 2) + startX) / width;
            int k = ((int)(lastPos.Y - startY) / (height + 2) * (height + 2) + startY) / height;

            //Is enable to change the position of cropped image
            if (i < 3 && j < 3 && _a[i, j] == -1  //Out of range
                && ((k-1 == i && p == j) || (k+1 == i && p == j) || (p-1 == j && k == i) || (p+1 == j && k ==i))) //Not next to empty position
            {
                Canvas.SetLeft(selectedImage, x);
                Canvas.SetTop(selectedImage, y);
                var image = sender as Image;
                _a[i, j] = (int)image.Tag;

                //Update the value of array
                _a[k, p] = -1; //update the empty position
                _a[i, j] = (int)selectedImage.Tag; //change the value of empty postion to the image tag

                //Update move
                Moves++;
                MoveLabel.Content = Moves;

                //Game is changed so set it unsaved
                isSaved = false;
                
            }
            //Is unable to change the postion of cropped image
            else
            {
                var lastX = (int)(lastPos.X - startX) / (width + 2) * (width + 2);
                var lastY = (int)(lastPos.Y - startY) / (height + 2) * (height + 2);

                Canvas.SetLeft(selectedImage, lastX);
                Canvas.SetTop(selectedImage, lastY);

                var image = sender as Image;
            }

            checkWin(_a);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            
            int i = ((int)position.Y - startY ) / height;
            int j = ((int)position.X - startX) / width;
  
            if (isDragging)
            {
                var dx = position.X - currentPos.X;
                var dy = position.Y - currentPos.Y;

                var lastLeft = Canvas.GetLeft(selectedImage);
                var lastTop = Canvas.GetTop(selectedImage);
                Canvas.SetLeft(selectedImage, lastLeft + dx);
                Canvas.SetTop(selectedImage, lastTop + dy);

                currentPos = position;
            }
        }

        public bool isSolvable(int[,] arr)
        {
            //Count all inversions in array
            int invCount = 0;
            for (int i = 0; i < 3 - 1; i++)
            {
                for (int j = i + 1; j < 3; j++)
                {
                    // Value -1 is used for empty space 
                    if (arr[j, i] > -1 && arr[j, i] > -1 &&
                                    arr[j, i] > arr[i, j])
                        invCount++;
                }
            }
            
            //Return true if invCount is even
            if(invCount % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check win condition
        /// </summary>
        /// <param name="arr"></param>
        /// <returns>Return true if the array _a is increasing</returns>
        public void checkWin(int[,] arr)
        {
            bool isWin = true;
            int[] a = new int[9];
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    a[i * 3 + j] = arr[i, j];
                }
            }

            //return true if array a is an increasing array
            for (int i = 0; i < 8-1; i++)
            {
                for(int j = i + 1; j < 8; j++)
                {
                    if(a[i] > a[j])
                    {
                        isWin = false;
                        break;
                    }
                }
            }

            if (isWin)
            {
                MessageBox.Show("You win");
                DeactivePlaying();
            }
        }

        /// <summary>
        /// Prevent user playing after the game is over
        /// </summary>
        public void DeactivePlaying()
        {
            foreach(Image image in _listCropImage)
            {
                image.MouseLeftButtonDown -= CropImage_MouseLeftButtonDown;
                image.PreviewMouseLeftButtonUp -= CropImage_PreviewMouseLeftButtonUp;
            }
            isPlaying = false;
        }

        /// <summary>
        /// Find the index of empty position
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public Tuple<int,int> getEmptyPosition(int[,] arr)
        {
            Tuple<int, int> index;
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if(arr[i,j] == -1)
                    {
                        index = new Tuple<int, int>(i, j);
                        return index;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// This function rerurns an image at position [i,j] in canvas
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private Image FindImageByTag(int tag)
        {
            foreach(Image image in _listCropImage)
            {
                if((int)image.Tag == tag)
                {
                    return image;
                }
            }
            return null;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isPlaying)
            {
                var (i, j) = getEmptyPosition(_a);
                switch (e.Key)
                {
                    case Key.Down:
                        if (i - 1 >= 0)
                        {
                            Image image = FindImageByTag(_a[i - 1, j]);
                            //Change the position of image
                            Crop_Image.Children.Remove(image);
                            Crop_Image.Children.Add(image);
                            Canvas.SetLeft(image, j * (width + 2));
                            Canvas.SetTop(image, i * (height + 2));

                            //Update the value in array
                            _a[i, j] = (int)image.Tag;
                            _a[i - 1, j] = -1;

                            //Update move
                            Moves++;
                            MoveLabel.Content = Moves;

                            //Game is changed so set it unsaved
                            isSaved = false;

                        }
                        break;
                    case Key.Up:
                        if (i + 1 <= 2)
                        {
                            Image image = FindImageByTag(_a[i + 1, j]);
                            //Change the position of image
                            Crop_Image.Children.Remove(image);
                            Crop_Image.Children.Add(image);
                            Canvas.SetLeft(image, j * (width + 2));
                            Canvas.SetTop(image, i * (height + 2));

                            //Update the value in array
                            _a[i, j] = (int)image.Tag;
                            _a[i + 1, j] = -1;

                            //Update move
                            Moves++;
                            MoveLabel.Content = Moves;

                            //Game is changed so set it unsaved
                            isSaved = false;

                        }
                        break;
                    case Key.Right:
                        if (j - 1 >= 0)
                        {
                            Image image = FindImageByTag(_a[i, j - 1]);
                            //Change the position of image
                            Crop_Image.Children.Remove(image);
                            Crop_Image.Children.Add(image);
                            Canvas.SetLeft(image, j * (width + 2));
                            Canvas.SetTop(image, i * (height + 2));

                            //Update the value in array
                            _a[i, j] = (int)image.Tag;
                            _a[i, j - 1] = -1;

                            //Update move
                            Moves++;
                            MoveLabel.Content = Moves;

                            //Game is changed so set it unsaved
                            isSaved = false;
                        }
                        break;
                    case Key.Left:
                        if (j + 1 <= 2)
                        {
                            Image image = FindImageByTag(_a[i, j + 1]);
                            //Change the position of image
                            Crop_Image.Children.Remove(image);
                            Crop_Image.Children.Add(image);
                            Canvas.SetLeft(image, j * (width + 2));
                            Canvas.SetTop(image, i * (height + 2));

                            //Update the value in array
                            _a[i, j] = (int)image.Tag;
                            _a[i, j + 1] = -1;

                            //Update move
                            Moves++;
                            MoveLabel.Content = Moves;

                            //Game is changed so set it unsaved
                            isSaved = false;
                        }
                        break;
                }
                checkWin(_a);
            }
        }

        private void ResetCountDown()
        {
            //Stop countdown time and reset timer to 0
            _time = TimeSpan.FromSeconds(0);
            _timer.Stop();
            _timer.Tick -= _timer_Tick;
        }


        private void Button_ExitClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

       
    }
}
