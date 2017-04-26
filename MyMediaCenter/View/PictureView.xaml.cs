using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Data;


namespace MyMediaCenter.View
{
    /// <summary>
    /// Interaction logic for PictureView.xaml
    /// </summary>
    /// 

    public class Item
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Size { get; set; }
        public Item(string name, DateTime date, int size)
        {
            this.Name = name;
            this.Date = date;
            this.Size = size;
        }
    }

    public partial class PictureView : System.Windows.Controls.UserControl
    {
        Dictionary<string, string> d = new Dictionary<string, string>();
        public Dictionary<int, string> Playlist1 = new Dictionary<int, string>();
        string pathN;
        Boolean playL = true;
        static int next = 0;
        Visibility rv = Visibility.Hidden;
        Visibility ry = Visibility.Visible;
        public String[] SortingOrder = new String[] { "Ascending", "Descending" };
        public bool asToRessort = false; // false = Ascending true = descending
        public String[] SortingType = new String[] { "Name", "Date", "Type" };
        public int SortingView = 0;
        public Dictionary<int, string> ItPicture = new Dictionary<int, string>();
        public String[] Views = new String[] { "Thumbnail", "List" };
        public int view = 0;
        string MyConnectionStr = "server=db4free.net;Port=3306; User ID = chefte_f; password=toto1234; database=fantasticfour";

        public PictureView()
        {
            InitializeComponent();
            BackHomeBtn.Click += BackHomeButtonClicked;
            ImportPicture.Click += ImportFolder;
            LoadPathFolder();
            loadFolderOnFolders();
            AnimActionsGrid();
            Prev.Click += MediaPrev;
            Close.Click += MediaClose;
            Next.Click += MediaNext;
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
            ImportPicture.BeginAnimation(OpacityProperty, Opacity);
            Folders.BeginAnimation(OpacityProperty, Opacity);
            View.BeginAnimation(OpacityProperty, Opacity);
            SortBy.BeginAnimation(OpacityProperty, Opacity);
            Sort.BeginAnimation(OpacityProperty, Opacity);
            BackHomeBtn.BeginAnimation(OpacityProperty, Opacity);
        }

        private async void BackHomeButtonClicked(object sender, EventArgs e)
        {
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
                folderbtn.BorderBrush = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));
                folderbtn.BorderThickness = new Thickness(1);
                folderbtn.Height = 25;
                folderbtn.TextAlignment = TextAlignment.Center;
                folderbtn.Text = kvp.Key;
                folderbtn.FontWeight = FontWeights.Bold;
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
                cmd.CommandText = "SELECT * FROM picturefolders";
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
                cmd.CommandText = "INSERT INTO picturefolders (id, name, path) VALUES (NULL, @namefolder, @pathfolder)";
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
            DisplayPictureSortByName(asToRessort);
            SortingView++;
        }
        private void DeleteFolder(String nameFolder)
        {
            MySqlConnection con = new MySqlConnection(MyConnectionStr);
            MySqlCommand cmd;
            con.Open();
            try
            {
                cmd = con.CreateCommand();
                cmd.CommandText = "DELETE FROM picturefolders WHERE name = @namefolder";
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

        private Boolean isCorrupt(string item)
        {
            try
            {
                BitmapImage myBitmapImage1 = new BitmapImage();
                myBitmapImage1.BeginInit();
                myBitmapImage1.UriSource = new Uri(item);
                myBitmapImage1.EndInit();
                return true;
            }
            catch
            {
                System.Windows.MessageBox.Show("Picture corrupted found");
                return false;
            }
        }
           

        private void LoadDictionnaries()
        {
            int PictureSelectedPos = 0;
            string[] filePaths = Directory.GetFiles(pathN, "*.*", SearchOption.AllDirectories);
            string[] ext = { ".jpg", ".jpeg", ".png", ".ico", ".JPG", ".PNG", ".ICO", ".JPEG" };
            ItPicture.Clear();
            foreach (var item in filePaths)
            {
                string extension = Path.GetExtension(item);
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
                Thumbnail.MouseDoubleClick += isClicked;
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
                    tmpList.Sort((a,b) => a.CompareTo(b));
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
            ViewDisplay2(Views[view]);
        }
        private void DisplayPictureSortBySize(Boolean sort)
        {
            int nb = 0;
            List<int> tmpList = new List<int>();
            Dictionary<int, string> tmpDict= new Dictionary<int, string>();
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
            ViewDisplay2(Views[view]);
     
        }
        private void isClicked(object sender, EventArgs e)
        {
            foreach (var kvp in ItPicture)
            {
                if (kvp.Key == Thumbnail.SelectedIndex) 
                {
                    next = kvp.Key;
                    BitmapImage myBitmapImage1 = new BitmapImage();
                    myBitmapImage1.BeginInit();
                    myBitmapImage1.UriSource = new Uri(kvp.Value);
                    myBitmapImage1.EndInit();
                    Viewer.Visibility = Visibility.Visible;
                    ViewerContent.Source = myBitmapImage1;
                    ViewerContent.Visibility = ry;
                    ButtonGrid.Visibility = ry;
                    Viewer.Visibility = ry;
                    ButtonGridBorder.Visibility = ry;
                }
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
        }
        private void HandleKeyPress(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Viewer.Visibility == Visibility.Visible)
            {
                if (e.Key == Key.Escape)
                {
                    Thumbnail.Visibility = ry;
                    Viewer.Visibility = rv;
                    ButtonGrid.Visibility = rv;
                    ButtonGridBorder.Visibility = rv;
                    ViewerContent.Visibility = rv;
                    ViewDisplay(Views[view]);
                }
                else if (e.Key == Key.Right)
                    MediaNext(sender, e);
                else if (e.Key == Key.Left)
                    MediaPrev(sender, e);
            }

        }
        private void DisplayPictureSortByName(Boolean sort)
        {
            int nb = 0;
            List<string> tmpList = new List<string>();
            Dictionary<int, string> tmpDict = new Dictionary<int, string>();
            foreach (var it in ItPicture)
                tmpList.Add(Path.GetFileName(it.Value));
            if (sort == false)
                tmpList.Sort();
            else 
                tmpList.Reverse();

            foreach (var it in tmpList)
                foreach (var it2 in ItPicture)
                {
                    string result = Path.GetFileName(it2.Value);
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
            ViewDisplay2(Views[view]);
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
        private void PicList()
        {
            if ((!Search.Text.Equals("")) && (!Search.Text.Equals("Search...")))
                OrganizeGrid();
            else
            {
                LoadDictionnaries();
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
        private void ViewDisplay(String view)
        {
            if (ItPicture.Count == 0)
                return;
            if (view.Equals("Thumbnail"))
            {
                List.Visibility = Visibility.Hidden;
                List.IsEnabled = false;
                ListPreview.Visibility = Visibility.Hidden;
                ListPreview.IsEnabled = false;
                PicThumbnail();
            }
            else if (view.Equals("List"))
            {
                Thumbnail.Items.Clear();
                List.Visibility = Visibility.Visible;
                List.IsEnabled = true;
                ListPreview.Visibility = Visibility.Visible;
                ListPreview.IsEnabled = true;
                PicList();
            }
        }
        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (List.SelectedItem == null) return;
            var selectedPicture = List.SelectedItem as Item;
            
            foreach (var kvp in ItPicture)
            {
                if (Path.GetFileName(kvp.Value).Equals(selectedPicture.Name))
                {
                    BitmapImage myBitmapImage1 = new BitmapImage();
                    myBitmapImage1.BeginInit();
                    myBitmapImage1.UriSource = new Uri(kvp.Value);
                    myBitmapImage1.EndInit();
                    Viewer.Visibility = Visibility.Visible;
                    ViewerContent.Source = myBitmapImage1;
                    ViewerContent.Visibility = ry;
                    ButtonGrid.Visibility = ry;
                    Viewer.Visibility = ry;
                    ButtonGridBorder.Visibility = ry;
                }
            }
        }
        private void List_MouseSimpleClick(object sender, RoutedEventArgs e)
        {
            if (List.SelectedItem == null) return;
            var selectedPicture = List.SelectedItem as Item;
            Console.WriteLine("Name = " + selectedPicture.Name + "  Date = " + selectedPicture.Date);
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
        private void button_Click(object sender, RoutedEventArgs e)
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
        private void button1_Click(object sender, RoutedEventArgs e)
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

        private void OrganizeGrid()
        {
            var items = new List<Item>();
            int PictureSelectedPos = 0;
            string[] filePaths = Directory.GetFiles(pathN, "*.*", SearchOption.AllDirectories);
            string[] ext = { ".jpg", ".jpeg", ".png", ".ico", ".JPG", ".PNG", ".ICO", ".JPEG" };
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
        private void OrganizeThumbnail()
        {
            if (ItPicture.Count > 0)
            {
                Thumbnail.Items.Clear();
                 int PictureSelectedPos = 0;
                 string[] filePaths = Directory.GetFiles(pathN, "*.*", SearchOption.AllDirectories);
                 string[] ext = { ".jpg", ".jpeg", ".png", ".ico", ".JPG", ".PNG", ".ICO", ".JPEG" };
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
                                    BitmapImage myBitmapImage1 = new BitmapImage();
                                    myBitmapImage1.BeginInit();
                                    myBitmapImage1.UriSource = new Uri(item);
                                    myBitmapImage1.EndInit();
                                    Image myImage1 = new Image();
                                    myImage1.Width = 100;
                                    myImage1.Height = 100;
                                    myImage1.Stretch = Stretch.Uniform;
                                    myImage1.StretchDirection = StretchDirection.Both;
                                    myImage1.Source = myBitmapImage1;
                                    b.Child = myImage1;
                                    Thumbnail.Items.Add(b);
                                    ItPicture.Add(PictureSelectedPos, item);
                                    PictureSelectedPos++;
                                }
                            }
                         }
                 }
                 Thumbnail.MouseDoubleClick += isClicked;
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
                    PicList();
                else
                    OrganizeGrid();
            }
        }

        public void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Search.Text.Equals("Search..."))
                Search.Text = "";
        }

        /*private void addPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (playL == true)
            {
                System.Windows.Controls.TextBox playlistbtn = new System.Windows.Controls.TextBox();
                playlistbtn.Width = Folders.Width;
                playlistbtn.Height = 25;
                playlistbtn.TextAlignment = TextAlignment.Center;
                playlistbtn.Text = "PLaylist";
                //playlistbtn.IsEnabled = false;
                playlistbtn.Cursor = System.Windows.Input.Cursors.Arrow;
                playlistbtn.Background = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
                playlistbtn.Foreground = new SolidColorBrush(Colors.White);
                playlistbtn.MouseDoubleClick += MyLeftButtonHandlerPlaylist;
                Playlist.Items.Add(playlistbtn);
                playL = false;
            }
        }
        private void addToPlaylist(object sender, EventArgs e)
        {
            int key2 = Thumbnail.SelectedIndex;

            foreach (var t in Playlist1)
                if (t.Key == key2)
                    return;
            foreach (var r in ItPicture)
                if (r.Key == key2)
                    Playlist1.Add(key2, r.Value);
        }

        void MyLeftButtonHandlerPlaylist(object sender, MouseButtonEventArgs e)
        {
            Thumbnail.Items.Clear();
            ItPicture.Clear();
            foreach (var kvp in Playlist1)
                ItPicture.Add(kvp.Key, kvp.Value);
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
            Thumbnail.MouseDoubleClick += isClicked;
            //Thumbnail.MouseDoubleClick += addToPlaylist;
        }*/

        private async void MediaNext(object sender, EventArgs e) /*J'APPUIE SUR PLAY */
        {
            if (next == ItPicture.Count - 1)
                return;
            ViewerContent.Source = null;
            foreach (var kvp in ItPicture)
                if (next == kvp.Key)
                    next = kvp.Key;
            next++;
            foreach (var kvp in ItPicture)
                if (next == kvp.Key)
                {
                    BitmapImage myBitmapImage1 = new BitmapImage();
                    myBitmapImage1.BeginInit();
                    myBitmapImage1.UriSource = new Uri(kvp.Value);
                    myBitmapImage1.EndInit();
                    Viewer.Visibility = Visibility.Visible;
                    ViewerContent.Source = myBitmapImage1;
                }

        }
        
        private async void MediaClose(object sender, EventArgs e) /*JE FERME MA VIDEO*/
        {
            Thumbnail.Visibility = ry;                            /*JE CACHE MES BOUTONS DE NAVIGATION */
            Viewer.Visibility = rv;
            ButtonGrid.Visibility = rv;
            ButtonGridBorder.Visibility = rv;
            ViewerContent.Visibility = rv;
            ViewDisplay2(Views[view]);
        }

        private async void MediaPrev(object sender, EventArgs e) /*J'APPUIE SUR PAUSE*/
        {
            if (next == 0)
                return;
            ViewerContent.Source = null;
            foreach (var kvp in ItPicture)
                if (next == kvp.Key)
                    next = kvp.Key;
            next--;
            foreach (var kvp in ItPicture)
                if (next == kvp.Key)    
                {
                    BitmapImage myBitmapImage1 = new BitmapImage();
                    myBitmapImage1.BeginInit();
                    myBitmapImage1.UriSource = new Uri(kvp.Value);
                    myBitmapImage1.EndInit();
                    Viewer.Visibility = Visibility.Visible;
                    ViewerContent.Source = myBitmapImage1;
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
                Thumbnail.MouseDoubleClick += isClicked;
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