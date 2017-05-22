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

namespace MyMediaCenter.View
{
    /// <summary>
    /// Interaction logic for VideoView.xaml
    /// </summary>

    public class Item3
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Size { get; set; }
        //public Duration dure { get; set; }
        public Item3(string name, DateTime date, int size)
        {
            this.Name = name;
            this.Date = date;
            this.Size = size;
        }
    }
    public partial class MusicView : System.Windows.Controls.UserControl
    {
        public String[] SortingOrder = new String[] { "Ascending", "Descending" };
        public bool asToRessort = false;
        public String[] SortingType = new String[] { "Name", "Date", "Size", "Duration" };
        public int SortingView = 0;
        public string duration;
        bool check = false;
        bool isFullscreen = false;
        Dictionary<string, string> d = new Dictionary<string, string>();
        public Dictionary<int, string> ItPicture = new Dictionary<int, string>();
        Visibility rv = Visibility.Hidden;
        Visibility ry = Visibility.Visible;
        public double sound_volume;
        string pathN;
        public String[] Views = new String[] { "Thumbnail", "List" };
        public int view = 0;
        private bool userIsDraggingSlider = false;
        string MyConnectionStr = "server=sql12.freemysqlhosting.net;Port=3306; User ID = sql12174934; password=YpkJJk4RTk; database=sql12174934";
        int tmp = 0;
        bool loopPlay = false;
        bool RandomPlay = false;

        public MusicView()
        {
            InitializeComponent();
            BackHomeBtn.Click += BackHomeButtonClicked;
            ImportMusic.Click += ImportFolder;
            Pause.Click += MediaPause;
            Stop.Click += MediaStop;
            Play.Click += MediaPlay;
            Close.Click += MediaClose;
            Prev.Click += MediaRestart;
            Next.Click += MediaNext;
            VolumeOn.Click += MediaMute;
            VolumeOff.Click += MediaUnmute;
            Repeat.Click += MediaLoopPlay;
            Random.Click += MediaRandomPlay;
            media.MediaEnded += MediaEnded;
            //media.MouseDown += fullscreen;
            Music_duration.MouseDoubleClick += new MouseButtonEventHandler(ThumbMouseEnter);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            LoadPathFolder();
            loadFolderOnFolders();
            AnimActionsGrid();
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
            ImportMusic.BeginAnimation(OpacityProperty, Opacity);
            Folders.BeginAnimation(OpacityProperty, Opacity);
            View.BeginAnimation(OpacityProperty, Opacity);
            SortBy.BeginAnimation(OpacityProperty, Opacity);
            Sort.BeginAnimation(OpacityProperty, Opacity);
            BackHomeBtn.BeginAnimation(OpacityProperty, Opacity);
        }

        private async void BackHomeButtonClicked(object sender, EventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            var ActionsGridAnim = new DoubleAnimation();
            var BGAnim = new DoubleAnimation();
            LayoutRoot.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/homecinema.png")));
            //BGAnim.From = 0.3;
            //BGAnim.To = 1;
            //BGAnim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            //LayoutRoot.Background.BeginAnimation(SolidColorBrush.OpacityProperty, BGAnim);
            ActionsGridAnim.From = 220;
            ActionsGridAnim.To = 0;
            ActionsGridAnim.Duration = new Duration(TimeSpan.FromSeconds(0.25));
            await Task.Delay(TimeSpan.FromSeconds(0.25));
            await Task.Delay(TimeSpan.FromSeconds(0.25));
        }

        private Boolean isCorrupt(string item)
        {
            try
            {
                MediaElement myBitmapImage1 = new MediaElement();
                myBitmapImage1.BeginInit();
                myBitmapImage1.Source = new Uri(item);
                myBitmapImage1.EndInit();
                return true;
            }
            catch
            {
                System.Windows.MessageBox.Show("Music corrupted found");
                return false;
            }
        }

        private void ImportFolder(object sender, EventArgs e)
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
                cmd.CommandText = "SELECT * FROM musicfolders";
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
                            if ((Directory.Exists(pathfolder)))
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
                cmd.CommandText = "INSERT INTO musicfolders (id, name, path) VALUES (NULL, @namefolder, @pathfolder)";
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
                cmd.CommandText = "DELETE FROM musicfolders WHERE name = @namefolder";
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
            string[] ext = { ".wav", ".ogg", ".mp3", ".mp4", ".mpeg4", ".midi", ".flac" };
            string[] extcover = { ".jpg", ".jpeg", ".png", ".ico", ".JPG", ".PNG", ".ICO", ".JPEG" };
            ItPicture.Clear();
            foreach (var item in filePaths)
            {
                string extension = System.IO.Path.GetExtension(item);
                foreach (string it in ext)
                    if (it.Equals(extension))
                    {
                        if (isCorrupt(item) == true)
                        {
                            ItPicture.Add(PictureSelectedPos, item);
                            PictureSelectedPos++;
                        }
                    }
            }
        }
        private void PicThumbnail()
        {
            Thumbnail.Items.Clear();
            if ((!Search.Text.Equals("")) && (!Search.Text.Equals("Search...")))
            {
                OrganizeThumbnail();
                return;
            }
            else
            {
                LoadDictionnaries();
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
                    myBitmapImage1.UriSource = new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/ic_exit.png");
                    myBitmapImage1.EndInit();
                    Image myImage1 = new Image();
                    myImage1.Width = 100;
                    myImage1.Height = 100;
                    myImage1.Stretch = Stretch.Uniform;
                    myImage1.StretchDirection = StretchDirection.Both;
                    myImage1.Source = myBitmapImage1;
                    b.Child = myImage1;
                    //File names
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
                mediaBG.Visibility = rv;
                ButtonGridBorder.Visibility = rv;
                Thumbnail.MouseDoubleClick += Video_Thumbnail_is_Clicked;
            }
        }
        private void Video_Thumbnail_is_Clicked(object sender, EventArgs e) /* ON A CLIQUER SUR UNE VIDEO */
        {
            if (Thumbnail.SelectedItem == null)
                return;
            int a = 0;
            Thumbnail.Visibility = rv;
            List.Visibility = rv;
            mediaBG.Visibility = ry;
            ButtonGridBorder.Visibility = ry;
            foreach (var kvp in ItPicture)
            {
                if (Views[view].Equals("Thumbnail"))
                {
                    if (kvp.Key == Thumbnail.SelectedIndex) //création de la nouvelle image
                    {
                        MediaElementName.Text = Path.GetFileName(kvp.Value);
                        tmp = a;
                        media.Source = new Uri(kvp.Value); /* JE LANCE LA VIDEO */
                        media.Play();
                        media.Volume = 0.5;
                        ButtonGrid.Visibility = ry; /*J'affiche ma barre de navigation */
                        Play.Visibility = rv;
                    }
                }
                else if (Views[view].Equals("List"))
                {
                    if (kvp.Key == List.SelectedIndex) //création de la nouvelle image
                    {
                        MediaElementName.Text = Path.GetFileName(kvp.Value);
                        System.Windows.MessageBox.Show(Views[view]);
                        tmp = a;
                        media.Source = new Uri(kvp.Value); /* JE LANCE LA VIDEO */
                        media.Play();
                        media.Volume = 0.5;
                        ButtonGrid.Visibility = ry; /*J'affiche ma barre de navigation */
                        Play.Visibility = rv;
                    }
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
        private async void MediaRestart(object sender, EventArgs e) /*JE REMET A 0 LA VIDEO */
        {
            if (media.Position != TimeSpan.Zero)
            {
                media.Position = new TimeSpan(0);
                media.Pause();
                check = true;
            }
            else
                MediaPrevious();
            if (Play.Visibility == ry)
            {
                Play.Visibility = ry;
                Stop.Visibility = rv;
            }
            else
            {
                Play.Visibility = rv;
                Stop.Visibility = ry;
            }
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

        private async void MediaLoopPlay(object sender, EventArgs e) /*ACTIVE LA REPETITION EN BOUCLE*/
        {
            if (!loopPlay)
                Repeat.Background = Brushes.CornflowerBlue;
            else
                Repeat.Background = Brushes.Transparent;
            loopPlay = !loopPlay;
        }

        private async void MediaRandomPlay(object sender, EventArgs e) /*ACTIVE LA LECTURE ALEATOIRE*/
        {
            if (!RandomPlay)
                Random.Background = Brushes.CornflowerBlue;
            else
                Random.Background = Brushes.Transparent;
            RandomPlay = !RandomPlay;
        }


        private void MediaPrevious()
        {
            int a = 0;
            if (tmp == 0 && check == true) /* Je suis a la premiere video de la liste et je l'ai deja avancé jusqu'a la fin, donc je ferme tout*/
            {
                Thumbnail.SelectedItem = null;
                media.Close();
                Thumbnail.Visibility = ry;
                mediaBG.Visibility = rv;
                ButtonGridBorder.Visibility = rv;
                ViewDisplay(Views[view]);
            }
            foreach (var kvp in ItPicture)
            {
                if (a == tmp - 1) //création de la nouvelle image
                {
                    tmp = a;
                    media.Close();
                    media.Source = new Uri(kvp.Value);
                    media.Play();
                    MediaElementName.Text = Path.GetFileName(kvp.Value);
                    media.Volume = 0.5;
                    check = false;
                    break;
                }
                else
                    a++;
            }
        }
        private async void MediaNext(object sender, EventArgs e) /*MEDIA SUIVANT*/
        {
            int a = 0;
            if (tmp == ItPicture.Count - 1 && check == true) /* Je suis a la derniere video de la liste et je l'ai deja avancé jusqu'a la fin, donc je ferme tout*/
            {
                Thumbnail.SelectedItem = null;
                media.Close();
                Thumbnail.Visibility = ry;
                mediaBG.Visibility = rv;
                ButtonGridBorder.Visibility = rv;
                ViewDisplay(Views[view]);
            }
            foreach (var kvp in ItPicture)
            {
                if (a == tmp + 1) //création de la nouvelle image
                {
                    tmp = a;
                    media.Source = new Uri(kvp.Value);
                    media.Play();
                    MediaElementName.Text = Path.GetFileName(kvp.Value);
                    media.Volume = 0.5;
                    check = false;
                    break;
                }
                else
                    a++;
            }
        }

        private void MediaEnded(object sender, EventArgs e)
        {
            if (loopPlay)
            {
                media.Position = TimeSpan.Zero;
                media.Play();
            }
            else
                MediaNext(sender, e);
        }

        private async void MediaClose(object sender, EventArgs e) /*JE FERME MA VIDEO*/
        {
            tmp = 0;
            Thumbnail.SelectedItem = null;
            media.Close();
            Thumbnail.Visibility = ry;                            /*JE CACHE MES BOUTONS DE NAVIGATION */
            mediaBG.Visibility = rv;
            ButtonGridBorder.Visibility = rv;
            ViewDisplay(Views[view]);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) /*LE VOLUME A BOUGE */
        {
            var slider = sender as Slider;
            sound_volume = slider.Value;
            change_volume(sound_volume);
        }
        private void change_volume(double new_volume_sound) /* JAFFECTE LA NOUVELLE VALEUR DU VOLUME */
        {
            Console.WriteLine("SOUND ==> :");
            Console.WriteLine(new_volume_sound);
            media.Volume = new_volume_sound;
        }
        private void ViewDisplay(String view)
        {
            if (view.Equals("Thumbnail"))
            {
                List.Visibility = Visibility.Hidden;
                List.IsEnabled = false;
                //ListPreview.Visibility = Visibility.Hidden;
                //ListPreview.IsEnabled = false;
                PicThumbnail();
            }
            else if (view.Equals("List"))
            {
                Thumbnail.Items.Clear();
                List.Visibility = Visibility.Visible;
                List.IsEnabled = true;
                // ListPreview.Visibility = Visibility.Visible;
                // ListPreview.IsEnabled = true;
                MusicList();
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
        private void MusicList()
        {
            if ((!Search.Text.Equals("")) && (!Search.Text.Equals("Search...")))
                OrganizeGrid();
            else
            {
                LoadDictionnaries();
                var items = new List<Item2>();
                foreach (var kvp in ItPicture)
                {
                    FileInfo mon_fichier = new FileInfo(kvp.Value);
                    string taille = mon_fichier.Length.ToString();
                    DateTime date = File.GetCreationTime(kvp.Value);
                    items.Add(new Item2(System.IO.Path.GetFileName(kvp.Value), date, int.Parse(taille)));
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
            string[] ext = { ".wav", ".ogg", ".mp3", ".mp4", ".mpeg4", ".midi", ".flac" };
            ItPicture.Clear();
            foreach (var item in filePaths)
            {
                string extension = Path.GetExtension(item);
                foreach (string it in ext)
                    if (it.Equals(extension))
                    {
                        if (isCorrupt(item) == true)
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
            }
            List.ItemsSource = items;
            List.UpdateLayout();
        }
        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (List.SelectedItem == null) return;
            var selectedPicture = List.SelectedItem as Item2;
            foreach (var kvp in ItPicture)
            {
                if (Path.GetFileName(kvp.Value).Equals(selectedPicture.Name))
                {
                    List.Visibility = Visibility.Hidden;
                    media.Source = new Uri(kvp.Value); /* JE LANCE LA VIDEO */
                    media.Play();
                    media.Volume = 0.5;
                    Play.Visibility = rv;
                    mediaBG.Visibility = ry;
                    ButtonGridBorder.Visibility = ry;
                }
            }
        }

        private void List_MouseSimpleClick(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null) return;
            var selectedPicture = List.SelectedItem as Item;
            foreach (var kvp in ItPicture)
            {
                string result = Path.GetFileName(kvp.Value);
                if (result.Equals(selectedPicture.Name))
                {
                    BitmapImage myBitmapImage1 = new BitmapImage();
                    myBitmapImage1.BeginInit();
                    myBitmapImage1.UriSource = new Uri(kvp.Value);
                    myBitmapImage1.EndInit();
                    ListPreview.Source = null;
                    ListPreview.Source = myBitmapImage1;
                }
            }
        }

        private void OrganizeThumbnail()
        {
            if (ItPicture.Count > 0)
            {
                Thumbnail.Items.Clear();
                int PictureSelectedPos = 0;
                string[] filePaths = Directory.GetFiles(pathN, "*.*", SearchOption.AllDirectories);
                string[] ext = { ".wav", ".ogg", ".mp3", ".mp4", ".mpeg4", ".midi", ".flac" };
                ItPicture.Clear();
                foreach (var item in filePaths)
                {
                    string extension = Path.GetExtension(item);
                    foreach (string it in ext)
                        if (it.Equals(extension))
                        {
                            if (isCorrupt(item) == true)
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
                                    myBitmapImage1.UriSource = new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/icon-video.png");
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
                    MusicList();
                else
                    OrganizeGrid();
            }
        }

        public void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Search.Text.Equals("Search..."))
                Search.Text = "";
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((media.Source != null) && (media.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                Music_duration.Minimum = 0;
                Music_duration.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds;
                Music_duration.Value = media.Position.TotalSeconds;
            }
        }
        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }
        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            media.Position = TimeSpan.FromSeconds(Music_duration.Value);
        }
        private void ThumbMouseEnter(object sender, EventArgs e)
        {
            userIsDraggingSlider = true;
            media.Position = TimeSpan.FromSeconds(Music_duration.Value);
        }

        private void update_time_video(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Music_text_start.Text = TimeSpan.FromSeconds(Music_duration.Value).ToString(@"hh\:mm\:ss");
            Music_text_end.Text = TimeSpan.FromSeconds((media.NaturalDuration.TimeSpan.TotalSeconds - Music_duration.Value)).ToString(@"hh\:mm\:ss");
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