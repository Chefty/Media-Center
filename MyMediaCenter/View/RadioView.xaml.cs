using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.IO;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using System.Data;
using System.Linq;
using MahApps.Metro.Controls.Dialogs;

namespace MyMediaCenter.View
{
    /// <summary>
    /// Interaction logic for VideoView.xaml
    /// </summary>

    public class Item4
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Size { get; set; }
        public Item4(string name, DateTime date, int size)
        {
            this.Name = name;
            this.Date = date;
            this.Size = size;
        }
    }
    public partial class RadioView : System.Windows.Controls.UserControl
    {
        string pathN;
        public string duration;
        public int view = 0;
        public int SortingView = 0;
        public double sound_volume;
        bool check = false;
        public bool isPlayingRadio = false;
        public bool isDisplayingError = false;
        public static string sSearch;
        public bool isSearchModified = false;
        public bool asToRessort = false;
        private bool userIsDraggingSlider = false;
        public String[] SortingOrder = new String[] { "Ascending", "Descending" };
        public String[] SortingType = new String[] { "Name", "Date", "Size", "Duration" };
        public String[] Views = new String[] { "Thumbnail", "List" };
        string MyConnectionStr = "server=mysql3.gear.host; Port=3306; User ID = mediacenter; password=Medi@center; database=mediacenter";
        int RadioIndex;
        Dictionary<string, string> d = new Dictionary<string, string>();
        public Dictionary<int, string> ItPicture = new Dictionary<int, string>();
        Visibility rv = Visibility.Hidden;
        Visibility ry = Visibility.Visible;


        public RadioView()
        {
            InitializeComponent();
            BackHomeBtn.Click += BackHomeButtonClicked;
            ImportRadio.Click += ImportFolder;
            Pause.Click += MediaPause;
            Stop.Click += MediaStop;
            Play.Click += MediaPlay;
            //Prev.Click += MediaRestart;
            //Next.Click += MediaNext;
            VolumeOn.Click += MediaMute;
            VolumeOff.Click += MediaUnmute;
            LoadPathFolder();
            loadFolderOnFolders();
            AnimActionsGrid();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }
        private async void BackHomeButtonClicked(object sender, EventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            var ActionsGridAnim = new DoubleAnimation();
            var BGAnim = new DoubleAnimation();
            LayoutRoot.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/homecinema.png")));
            BGAnim.From = 0.3;
            BGAnim.To = 1;
            BGAnim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            LayoutRoot.Background.BeginAnimation(SolidColorBrush.OpacityProperty, BGAnim);
            ActionsGridAnim.From = 220;
            ActionsGridAnim.To = 0;
            ActionsGridAnim.Duration = new Duration(TimeSpan.FromSeconds(0.25));
            await Task.Delay(TimeSpan.FromSeconds(0.25));
            await Task.Delay(TimeSpan.FromSeconds(0.25));
        }

        private async void AnimActionsGrid()
        {
            var ActionsGridAnim = new DoubleAnimation();
            var Opacity = new DoubleAnimation();
            await Task.Delay(TimeSpan.FromSeconds(0.01f));
            ActionsGridAnim.Duration = new Duration(TimeSpan.FromSeconds(0.4f));
            ActionsGridAnim.From = 0;
            ActionsGridAnim.To = 200;
            Timeline.SetDesiredFrameRate(ActionsGridAnim, 60);
            ActionsGrid.BeginAnimation(WidthProperty, ActionsGridAnim);
            Opacity.Duration = new Duration(TimeSpan.FromSeconds(1));
            Opacity.From = 0;
            Opacity.To = 1;
            Timeline.SetDesiredFrameRate(Opacity, 60);
            ActionIcon.BeginAnimation(OpacityProperty, Opacity);
            ActionName.BeginAnimation(OpacityProperty, Opacity);
            Search.BeginAnimation(OpacityProperty, Opacity);
            ImportRadio.BeginAnimation(OpacityProperty, Opacity);
            Folders.BeginAnimation(OpacityProperty, Opacity);
            View.BeginAnimation(OpacityProperty, Opacity);
            SortBy.BeginAnimation(OpacityProperty, Opacity);
            Sort.BeginAnimation(OpacityProperty, Opacity);
            BackHomeBtn.BeginAnimation(OpacityProperty, Opacity);
        }

        private void loadFolderOnFolders()
        {
            Folders.Items.Clear();
            foreach (var kvp in d)
            {
                System.Windows.Controls.TextBox folderbtn = new System.Windows.Controls.TextBox();
                folderbtn.Width = Folders.Width;
                folderbtn.Height = 25;
                folderbtn.TextAlignment = TextAlignment.Center;
                folderbtn.Text = kvp.Key;
                folderbtn.IsReadOnly = true;
                folderbtn.Cursor = System.Windows.Input.Cursors.Arrow;
                folderbtn.Background = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
                folderbtn.Foreground = new SolidColorBrush(Colors.White);
                folderbtn.PreviewMouseLeftButtonDown += MyLeftButtonHandler;
                folderbtn.PreviewMouseRightButtonDown += MyRightButtonHandler;
                Folders.Items.Add(folderbtn);
            }
        }
        private void LoadPathFolder()
        {
            MySqlConnection con = new MySqlConnection(MyConnectionStr);
            con.Open();
            try
            {
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT * FROM radiofolders";
                MySqlDataAdapter adap = new MySqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adap.Fill(ds);
                d.Clear();
                foreach (DataTable table in ds.Tables)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        object[] tmp = row.ItemArray;
                        Boolean check = false;
                        String namefolder = tmp[1].ToString();
                        String pathfolder = tmp[2].ToString();
                        foreach (var kvp in d)
                            if (kvp.Key.Equals(tmp[1].ToString()))
                                check = true;
                        if (check == false)
                            d.Add(namefolder, pathfolder);
                    }
                }
            }
            catch (Exception es)
            {
                System.Windows.MessageBox.Show(es.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void AddFolderToDataBase(String pathFolder, String nameFolder)
        {
            MySqlConnection con = new MySqlConnection(MyConnectionStr);
            MySqlCommand cmd;
            con.Open();
            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = "INSERT INTO radiofolders (id, name, path) VALUES (NULL, @namefolder, @pathfolder)";
                cmd.Parameters.AddWithValue("@namefolder", nameFolder);
                cmd.Parameters.AddWithValue("@pathfolder", pathFolder);
                cmd.ExecuteNonQuery();
            }
            catch (Exception es)
            {
                System.Windows.MessageBox.Show(es.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void MyRightButtonHandler(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBox btn = sender as System.Windows.Controls.TextBox;
            System.Windows.Controls.ContextMenu contextMenu1 = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem menuItem1 = new System.Windows.Controls.MenuItem();
            contextMenu1.Items.Add(menuItem1);
            menuItem1.Header = "Delete";
            btn.ContextMenu = contextMenu1;
            menuItem1.Click += delegate { DeleteFolder(btn.Text); };
        }

        void MyLeftButtonHandler(object sender, MouseButtonEventArgs e)
        {
            var folderbtn = sender as System.Windows.Controls.TextBox;
            string pathFolder = null;
            foreach (var kvp in d)
                if (folderbtn.Text.Equals(kvp.Key))
                {
                    pathFolder = kvp.Value;
                    break;
                }
            if (pathFolder.Equals(pathN))
                return;
            Thumbnail.Items.Clear();
            pathN = pathFolder;
            LoadDictionnaries();
            ViewDisplay(Views[view]);
        }
        private void DeleteFolder(String nameFolder)
        {
            MySqlConnection con = new MySqlConnection(MyConnectionStr);
            MySqlCommand cmd;
            con.Open();
            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = "DELETE FROM radiofolders WHERE name = @namefolder";
                cmd.Parameters.AddWithValue("@namefolder", nameFolder);
                cmd.ExecuteNonQuery();
                Dictionary<string, string> tmp2 = new Dictionary<string, string>();
                foreach (var kvp in d)
                    if (!(kvp.Key.Equals(nameFolder)))
                        tmp2.Add(kvp.Key, kvp.Value);
                d.Clear();
                foreach (var kvp in tmp2)
                    d.Add(kvp.Key, kvp.Value);
                LoadPathFolder();
                loadFolderOnFolders();
            }
            catch (Exception es)
            {
                System.Windows.MessageBox.Show(es.Message);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }
        private void LoadDictionnaries()
        {
            int PictureSelectedPos = 0;
            string[] filePaths = null;
            if (pathN == null)
                return;
            filePaths = Directory.GetFiles(pathN, "*.*", SearchOption.AllDirectories);
            string[] ext = { ".pls", ".m3u" };
            ItPicture.Clear();
            foreach (var item in filePaths)
            {
                string extension = System.IO.Path.GetExtension(item);
                foreach (string it in ext)
                    if (it.Equals(extension))
                    {
                        ItPicture.Add(PictureSelectedPos, item);
                        PictureSelectedPos++;
                    }
            }
        }

        private void PicThumbnail()
        {
            Thumbnail.Items.Clear();
            if ((!Search.Text.Equals("")) && (!Search.Text.Equals("Search...")))
                OrganizeThumbnail();
            else
            {
                foreach (var it in ItPicture)
                {
                    var b = new Border
                    {
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = new Thickness(5)
                    };
                    string nameFolder = new DirectoryInfo(System.IO.Path.GetFileName(it.Value)).Name;
                    BitmapImage myBitmapImage1 = new BitmapImage();
                    myBitmapImage1.BeginInit();
                    myBitmapImage1.UriSource = new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/radio.png");
                    myBitmapImage1.EndInit();
                    Image myImage1 = new Image();
                    myImage1.Width = 100;
                    myImage1.Height = 100;
                    myImage1.Stretch = Stretch.Uniform;
                    myImage1.StretchDirection = StretchDirection.Both;
                    myImage1.Source = myBitmapImage1;
                    b.Child = myImage1;
                    //files names
                    String name = "";
                    System.Windows.Controls.TextBox value = new System.Windows.Controls.TextBox();
                    value.Text = nameFolder;
                    StackPanel stackPnl = new StackPanel();
                    TextBlock printTextBlock = new TextBlock();
                    name = nameFolder.Substring(0, 10);
                    printTextBlock.Text = name;
                    printTextBlock.FontSize = 15;
                    printTextBlock.Foreground = new SolidColorBrush(Colors.White);
                    stackPnl.Children.Add(b);
                    stackPnl.Children.Add(printTextBlock);
                    //Add files
                    Thumbnail.Items.Add(stackPnl);
                }
                media.Visibility = rv;
                Thumbnail.MouseDoubleClick += Video_Thumbnail_is_Clicked;
            }
        }
        private void Video_Thumbnail_is_Clicked(object sender, EventArgs e)
        {
            int indexTmp = 0;
            System.IO.StreamReader PlsFile;
            string PlsContent;
            string Radiourl;
            Uri RadioTester;
            int cutBegin;
            int cutEnd;

            int a = 0;
            foreach (var kvp in ItPicture)
            {
                if (kvp.Key == Thumbnail.SelectedIndex)
                {
                    PlsFile = new System.IO.StreamReader(kvp.Value);
                    PlsContent = PlsFile.ReadToEnd();
                    if (PlsContent.Contains("File1=") && PlsContent.Contains("Title1="))
                    {
                        cutBegin = PlsContent.IndexOf("File1=") + "File1=".Length;
                        cutEnd = PlsContent.IndexOf("Title1=");
                        Radiourl = PlsContent.Substring(cutBegin, cutEnd - cutBegin);
                        if (!(Uri.TryCreate(Radiourl, UriKind.Absolute, out RadioTester) && (RadioTester.Scheme == Uri.UriSchemeHttp || RadioTester.Scheme == Uri.UriSchemeHttps)))
                        {
                            PlsFile.Close();
                            System.Windows.MessageBox.Show("Bad url in this file.");
                            return;
                        }
                        isPlayingRadio = true;
                        media.Source = new Uri(PlsContent.Substring(cutBegin, cutEnd - cutBegin));
                        media.Play();
                        media.Volume = 0.5;
                        media.Visibility = ry;
                        ButtonGrid.Visibility = ry;
                        Play.Visibility = rv;
                        RadioIndex = indexTmp;
                        cutBegin = PlsContent.IndexOf("Title1=") + "Title1=".Length;
                        cutEnd = PlsContent.IndexOf("Length1");
                        RadioChannel.Content = PlsContent.Substring(cutBegin, cutEnd - cutBegin);
                    }
                    PlsFile.Close();
                    ++indexTmp;
                }
                a++;
            }
        }

        private async void MediaPause(object sender, EventArgs e) /*J'APPUIE SUR PAUSE*/
        {
            media.Pause();
            Pause.Visibility = rv;
            Play.Visibility = ry;
        }

        private async void MediaPlay(object sender, EventArgs e) /*J'APPUIE SUR PLAY */
        {
            media.Play();
            Pause.Visibility = ry;
            Play.Visibility = rv;
        }

        private async void MediaStop(object sender, EventArgs e) /*J'APPUIE SUR PAUSE*/
        {
            media.Stop();
            Pause.Visibility = rv;
            Play.Visibility = ry;
        }

        private async void MediaMute(object sender, EventArgs e) /*MUTE LE SON*/
        {
            media.IsMuted = true;
            VolumeOn.Visibility = rv;
            VolumeOff.Visibility = ry;
        }

        private async void MediaUnmute(object sender, EventArgs e) /*UNMUTE LE SON*/
        {
            media.IsMuted = false;
            VolumeOn.Visibility = ry;
            VolumeOff.Visibility = rv;
        }

        private async void MediaRestart(object sender, EventArgs e)
        {
            System.IO.StreamReader PlsFile;
            bool isReopen = false;
            string PlsContent;
            int indexTmp = 0;
            int cutBegin;
            int cutEnd;


            media.Close();
            if (RadioIndex == 0)
            {
                Console.Write("Last Radio");
                var Tmp = ItPicture.Last();
                PlsFile = new System.IO.StreamReader(Tmp.Value);
                PlsContent = PlsFile.ReadToEnd();
                cutBegin = PlsContent.IndexOf("File1=") + "File1=".Length;
                cutEnd = PlsContent.IndexOf("Title1=");
                media.Source = new Uri(PlsContent.Substring(cutBegin, cutEnd - cutBegin));
                media.Play();
                media.Volume = 0.5;
                media.Visibility = ry;
                ButtonGrid.Visibility = ry;
                Play.Visibility = rv;
                RadioIndex = 0;
                return;
            }
            foreach (var kvp in ItPicture)
            {
                if (indexTmp == RadioIndex - 1)
                {
                    PlsFile = new System.IO.StreamReader(kvp.Value);
                    PlsContent = PlsFile.ReadToEnd();
                    if (PlsContent.Contains("File1=") && PlsContent.Contains("Title1="))
                    {
                        cutBegin = PlsContent.IndexOf("File1=") + "File1=".Length;
                        cutEnd = PlsContent.IndexOf("Title1=");
                        media.Source = new Uri(PlsContent.Substring(cutBegin, cutEnd - cutBegin));
                        media.Play();
                        media.Volume = 0.5;
                        media.Visibility = ry;
                        ButtonGrid.Visibility = ry;
                        Play.Visibility = rv;
                        RadioIndex = indexTmp;
                        isReopen = true;
                    }
                    if (!isReopen)
                    {
                        Thumbnail.Visibility = ry;
                        media.Visibility = rv;
                        media.Visibility = rv;
                        ButtonGrid.Visibility = rv;
                        ViewDisplay(Views[view]);
                    }
                    PlsFile.Close();
                    return;
                }
                ++indexTmp;
            }
        }

        private async void MediaNext(object sender, EventArgs e)
        {
            System.IO.StreamReader PlsFile;
            bool isReopen = false;
            string PlsContent;
            int indexTmp = 0;
            int cutBegin;
            int cutEnd;


            media.Close();
            if (RadioIndex == (ItPicture.Count - 1))
            {
                Console.Write("Last Radio");
                var Tmp = ItPicture.First();
                PlsFile = new System.IO.StreamReader(Tmp.Value);
                PlsContent = PlsFile.ReadToEnd();
                cutBegin = PlsContent.IndexOf("File1=") + "File1=".Length;
                cutEnd = PlsContent.IndexOf("Title1=");
                media.Source = new Uri(PlsContent.Substring(cutBegin, cutEnd - cutBegin));
                media.Play();
                media.Volume = 0.5;
                media.Visibility = ry;
                ButtonGrid.Visibility = ry;
                Play.Visibility = rv;
                RadioIndex = 0;
                return;
            }
            foreach (var kvp in ItPicture)
            {
                if (indexTmp > RadioIndex)
                {
                    PlsFile = new System.IO.StreamReader(kvp.Value);
                    PlsContent = PlsFile.ReadToEnd();
                    if (PlsContent.Contains("File1=") && PlsContent.Contains("Title1="))
                    {
                        cutBegin = PlsContent.IndexOf("File1=") + "File1=".Length;
                        cutEnd = PlsContent.IndexOf("Title1=");
                        media.Source = new Uri(PlsContent.Substring(cutBegin, cutEnd - cutBegin));
                        media.Play();
                        media.Volume = 0.5;
                        media.Visibility = ry;
                        ButtonGrid.Visibility = ry;
                        Play.Visibility = rv;
                        RadioIndex = indexTmp;
                        isReopen = true;
                    }
                    if (!isReopen)
                    {
                        Thumbnail.Visibility = ry;
                        media.Visibility = rv;
                        media.Visibility = rv;
                        ButtonGrid.Visibility = rv;
                        ViewDisplay(Views[view]);
                    }
                    PlsFile.Close();
                    return;
                }
                ++indexTmp;
            }
        }
        private async void MediaClose(object sender, EventArgs e)
        {
            if (!isPlayingRadio)
                return;
            isPlayingRadio = false;
            media.Close();
            Thumbnail.Visibility = ry;
            media.Visibility = rv;
            media.Visibility = rv;
            ButtonGrid.Visibility = rv;
            ViewDisplay(Views[view]);
        }
        private async void MouseOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(102, TimeSpan.FromMilliseconds(500));
            ButtonGrid.BeginAnimation(MediaElement.HeightProperty, animation);
        }
        private async void MouseNotOver(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(500));
            ButtonGrid.BeginAnimation(MediaElement.HeightProperty, animation);
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            sound_volume = slider.Value;
            change_volume(sound_volume);
        }
        private void change_volume(double new_volume_sound)
        {
            media.Volume = new_volume_sound;
        }
        private void ViewDisplay(String view)
        {
            if (view.Equals("Thumbnail"))
            {
                List.Visibility = Visibility.Hidden;
                List.IsEnabled = false;
                PicThumbnail();
            }
            else if (view.Equals("List"))
            {
                Thumbnail.Items.Clear();
                List.Visibility = Visibility.Visible;
                List.IsEnabled = true;
                VideoList();
            }
        }
        private void DisplayPictureSortByDate(Boolean sort)
        {

            int nb = 0;
            List<DateTime> tmpList = new List<DateTime>();
            Dictionary<int, string> tmpDict = new Dictionary<int, string>();
            foreach (var it in ItPicture)
            {
                DateTime date = File.GetCreationTime(it.Value);
                tmpList.Add(date);
            }
            if (sort == false)
                tmpList.Sort((a, b) => a.CompareTo(b));
            else
                tmpList.Sort((a, b) => b.CompareTo(a));

            foreach (var it in tmpList)
                foreach (var it2 in ItPicture)
                {
                    DateTime date = File.GetCreationTime(it2.Value);
                    if (it == date)
                    {
                        int tmp = 0;
                        foreach (var it3 in tmpDict)
                            if (it3.Value.Equals(it2.Value))
                                tmp = 1;
                        if (tmp == 0)
                        {
                            tmpDict.Add(nb, it2.Value);
                            nb++;
                        }
                    }
                }
            ItPicture.Clear();
            foreach (var it in tmpDict)
                ItPicture.Add(it.Key, it.Value);
            ViewDisplay(Views[view]);
        }
        private void DisplayPictureSortBySize(Boolean sort)
        {
            int nb = 0;
            List<int> tmpList = new List<int>();
            Dictionary<int, string> tmpDict = new Dictionary<int, string>();
            foreach (var it in ItPicture)
            {
                FileInfo mon_fichier = new FileInfo(it.Value);
                string taille = mon_fichier.Length.ToString();
                tmpList.Add(int.Parse(taille));
            }
            if (sort == false)
                tmpList.Sort();
            else
                tmpList.Reverse();

            foreach (var it in tmpList)
            {
                foreach (var it2 in ItPicture)
                {
                    FileInfo mon_fichier = new FileInfo(it2.Value);
                    string taille = mon_fichier.Length.ToString();
                    if (it == int.Parse(taille))
                    {
                        int tmp = 0;
                        foreach (var it3 in tmpDict)
                            if (it3.Value.Equals(it2.Value))
                                tmp = 1;
                        if (tmp == 0)
                        {
                            tmpDict.Add(nb, it2.Value);
                            nb++;
                        }
                    }
                }
            }
            ItPicture.Clear();
            foreach (var it in tmpDict)
                ItPicture.Add(it.Key, it.Value);
            ViewDisplay(Views[view]);

        }
        private void DisplayPictureSortByName(Boolean sort)
        {
            int nb = 0;
            List<string> tmpList = new List<string>();
            Dictionary<int, string> tmpDict = new Dictionary<int, string>();
            foreach (var it in ItPicture)
                tmpList.Add(System.IO.Path.GetFileName(it.Value));
            if (sort == false)
                tmpList.Sort();
            else
                tmpList.Reverse();

            foreach (var it in tmpList)
                foreach (var it2 in ItPicture)
                {
                    string result = System.IO.Path.GetFileName(it2.Value);
                    if (it.Equals(result))
                    {
                        int tmp = 0;
                        foreach (var it3 in tmpDict)
                        {
                            if (it3.Value.Equals(it2.Value))
                                tmp = 1;
                        }
                        if (tmp == 0)
                        {
                            tmpDict.Add(nb, it2.Value);
                            nb++;
                        }
                    }
                }
            ItPicture.Clear();
            foreach (var it in tmpDict)
                ItPicture.Add(it.Key, it.Value);
            ViewDisplay(Views[view]);
        }
        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (List.SelectedItem == null) return;
            var selectedPicture = List.SelectedItem as Item4;
            foreach (var kvp in ItPicture)
            {
                if (Path.GetFileName(kvp.Value).Equals(selectedPicture.Name))
                {

                    List.Visibility = Visibility.Hidden;
                    media.Source = new Uri(kvp.Value);
                    media.Play();
                    media.Volume = 0.5;
                    media.Visibility = ry;
                    ButtonGrid.Visibility = ry;
                    Play.Visibility = rv;
                }
            }
        }
        private void VideoList()
        {
            if ((!Search.Text.Equals("")) && (!Search.Text.Equals("Search...")))
                OrganizeGrid();
            else
            {
                var items = new List<Item4>();

                foreach (var kvp in ItPicture)
                {
                    FileInfo mon_fichier = new FileInfo(kvp.Value);
                    string taille = mon_fichier.Length.ToString();
                    DateTime date = File.GetCreationTime(kvp.Value);
                    items.Add(new Item4(System.IO.Path.GetFileName(kvp.Value), date, int.Parse(taille)));
                }
                List.ItemsSource = items;
                List.UpdateLayout();
            }
        }
        private void SortBy_Click(object sender, RoutedEventArgs e)
        {
            if (SortingView == 0)
            {
                DisplayPictureSortByName(asToRessort);
                SortBy.Content = "Sort by: Name";
            }
            if (SortingView == 1)
            {
                DisplayPictureSortByDate(asToRessort);
                SortBy.Content = "Sort by: Date";
            }
            else if (SortingView == 2)
            {
                DisplayPictureSortBySize(asToRessort);
                SortBy.Content = "Sort by: Size";
            }
            SortingView++;
            if (SortingView == 3)
                SortingView = 0;
        }
        private void Sort_Click(object sender, RoutedEventArgs e)
        {
            if (asToRessort)
            {
                Sort.Content = "Sort: Ascending";
                asToRessort = false;
            }
            else
            {
                Sort.Content = "Sort: Descending";
                asToRessort = true;
            }
            int tmp = SortingView - 1;
            if (tmp == 0)
                DisplayPictureSortByName(asToRessort);
            if (tmp == 1)
                DisplayPictureSortByDate(asToRessort);
            else if (tmp == -1)
                DisplayPictureSortBySize(asToRessort);
        }
        private void View_Click(object sender, RoutedEventArgs e)
        {
            if (isPlayingRadio)
                return;
            if (view == Views.Length - 1)
                view = 0;
            else
                view += 1;
            View.Content = "View: " + Views[view];
            ViewDisplay(Views[view]);
        }
        private void OrganizeGrid()
        {
            var items = new List<Item>();
            int PictureSelectedPos = 0;
            string[] filePaths = Directory.GetFiles(pathN, "*.*", SearchOption.AllDirectories);
            string[] ext = { ".pls", ".m3u" };
            ItPicture.Clear();
            foreach (var item in filePaths)
            {
                string extension = Path.GetExtension(item);
                foreach (string it in ext)
                    if (it.Equals(extension))
                    {
                        if (Path.GetFileName(item).StartsWith(Search.Text))
                        {
                            FileInfo mon_fichier = new FileInfo(item);
                            string taille = mon_fichier.Length.ToString();
                            DateTime date = File.GetCreationTime(item);
                            items.Add(new Item(Path.GetFileName(item), date, int.Parse(taille)));
                            ItPicture.Add(PictureSelectedPos, item);
                            PictureSelectedPos++;
                        }
                    }
            }
            List.ItemsSource = items;
            List.UpdateLayout();
        }
        private void OrganizeThumbnail()
        {
            if (ItPicture.Count > 0)
            {
                Thumbnail.Items.Clear();
                int PictureSelectedPos = 0;
                string[] filePaths = Directory.GetFiles(pathN, "*.*", SearchOption.AllDirectories);
                string[] ext = { ".pls", ".m3u" };
                ItPicture.Clear();
                foreach (var item in filePaths)
                {
                    string extension = Path.GetExtension(item);
                    foreach (string it in ext)
                        if (it.Equals(extension))
                        {
                            if (Path.GetFileName(item).StartsWith(Search.Text))
                            {
                                var b = new Border()
                                {
                                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                                    BorderThickness = new Thickness(5),
                                };
                                string nameFolder = new DirectoryInfo(System.IO.Path.GetFileName(item)).Name;
                                BitmapImage myBitmapImage1 = new BitmapImage();
                                myBitmapImage1.BeginInit();
                                myBitmapImage1.UriSource = new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/radio.png");
                                myBitmapImage1.EndInit();
                                Image myImage1 = new Image();
                                myImage1.Width = 100;
                                myImage1.Height = 100;
                                myImage1.Stretch = Stretch.Uniform;
                                myImage1.StretchDirection = StretchDirection.Both;
                                myImage1.Source = myBitmapImage1;
                                b.Child = myImage1;
                                //files names
                                String name = "";
                                System.Windows.Controls.TextBox value = new System.Windows.Controls.TextBox();
                                value.Text = nameFolder;
                                StackPanel stackPnl = new StackPanel();
                                TextBlock printTextBlock = new TextBlock();
                                name = nameFolder.Substring(0, 10);
                                printTextBlock.Text = name;
                                printTextBlock.FontSize = 15;
                                printTextBlock.Foreground = new SolidColorBrush(Colors.White);
                                stackPnl.Children.Add(b);
                                stackPnl.Children.Add(printTextBlock);
                                //Add files
                                Thumbnail.Items.Add(stackPnl);
                                ItPicture.Add(PictureSelectedPos, item);
                                PictureSelectedPos++;
                            }
                        }
                }
                Thumbnail.MouseDoubleClick += Video_Thumbnail_is_Clicked;
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Views[view].Equals("Thumbnail"))
            {
                if (Search.Text.Equals(""))
                    PicThumbnail();
                else
                    OrganizeThumbnail();
            }
            else
            {
                if (Search.Text.Equals(""))
                    VideoList();
                else
                    OrganizeGrid();
            }
        }

        public void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Search.Text.Equals("Search..."))
                Search.Text = "";
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void ImportFolder(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string pathFolder = fbd.SelectedPath;
                string nameFolder = new DirectoryInfo(Path.GetFileName(pathFolder)).Name;
                foreach (var kvp in d)
                    if (String.Compare(nameFolder, kvp.Key) == 0)
                        return;
                AddFolderToDataBase(pathFolder, nameFolder);
                LoadPathFolder();
                loadFolderOnFolders();
            }
        }

        private void ViewDisplay2(String view)
        {
            Thumbnail.Items.Clear();
            if (ItPicture.Count == 0)
                return;
            if (view.Equals("Thumbnail"))
            {
                List.Visibility = Visibility.Hidden;
                List.IsEnabled = false;
                ListPreview.Visibility = Visibility.Hidden;
                ListPreview.IsEnabled = false;
                foreach (var it in ItPicture)
                {
                    var b = new Border
                    {
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        BorderThickness = new Thickness(5)
                    };
                    BitmapImage myBitmapImage1 = new BitmapImage();
                    myBitmapImage1.BeginInit();
                    myBitmapImage1.UriSource = new Uri(it.Value);
                    myBitmapImage1.EndInit();
                    Image myImage1 = new Image();
                    myImage1.Width = 100;
                    myImage1.Height = 100;
                    myImage1.Stretch = Stretch.Uniform;
                    myImage1.StretchDirection = StretchDirection.Both;
                    myImage1.Source = myBitmapImage1;
                    b.Child = myImage1;
                    Thumbnail.Items.Add(b);
                }
                Thumbnail.MouseDoubleClick += Video_Thumbnail_is_Clicked;
            }

            else if (view.Equals("List"))
            {
                Thumbnail.Items.Clear();
                List.Visibility = Visibility.Visible;
                List.IsEnabled = true;
                ListPreview.Visibility = Visibility.Visible;
                ListPreview.IsEnabled = true;
                var items = new List<Item>();
                foreach (var kvp in ItPicture)
                {
                    FileInfo mon_fichier = new FileInfo(kvp.Value);
                    string taille = mon_fichier.Length.ToString();
                    DateTime date = File.GetCreationTime(kvp.Value);
                    items.Add(new Item(Path.GetFileName(kvp.Value), date, int.Parse(taille)));
                }
                List.ItemsSource = items;
                List.UpdateLayout();
            }
        }
    }
}