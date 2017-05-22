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
using MahApps.Metro.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;


namespace MyMediaCenter.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : System.Windows.Controls.UserControl
    {
        public HomeView()
        {
            InitializeComponent();
            SetMainButtonsUIComponentsActions();
            SetTimeUpdate();
        }

        private void SetMainButtonsUIComponentsActions()
        {
            System.Windows.Controls.Button[] BtnArray = new System.Windows.Controls.Button[] { MusicBtn, PictureBtn, VideoBtn, SettingsBtn, RadioBtn };
            foreach (System.Windows.Controls.Button btn in BtnArray)
            {
                btn.MouseEnter += CommonMouseOver;
                btn.MouseLeave += CommonMouseNotOver;
            }
        }

        private void UnsetMainButtonsUIComponentsActions()
        {
            System.Windows.Controls.Button[] BtnArray = new System.Windows.Controls.Button[] { MusicBtn, PictureBtn, VideoBtn, SettingsBtn };
            foreach (System.Windows.Controls.Button btn in BtnArray)
            {
                btn.MouseEnter -= CommonMouseOver;
                btn.MouseLeave -= CommonMouseNotOver;
            }
        }


        private void SetTimeUpdate()
        {
            var timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 100;
            timer.Start();
        }

        private void LoadBackgroundPicture(string ButtonName)
        {
            if (ButtonName.Equals("MusicBtn"))
                LayoutRoot.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/MusicBG.png")));
            else if (ButtonName.Equals("VideoBtn"))
                LayoutRoot.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/MovieBG.png")));
            else if (ButtonName.Equals("PictureBtn"))
                LayoutRoot.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/PictureBG.png")));
            else if (ButtonName.Equals("SettingsBtn"))
                LayoutRoot.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/SettingsBG.png")));
            else if (ButtonName.Equals("RadioBtn"))
                LayoutRoot.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/RadioBG.png")));
        }

        private async void CommonMouseOver(object sender, EventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            var LayoutRootAnim = new DoubleAnimation();
            LoadBackgroundPicture(btn.Name);
            //LayoutRootAnim.From = 0;
            //LayoutRootAnim.To = 1;
            //LayoutRootAnim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            //LayoutRoot.Background.BeginAnimation(SolidColorBrush.OpacityProperty, LayoutRootAnim);
            //await Task.Delay(TimeSpan.FromSeconds(0.5));
        }

        private void CommonMouseNotOver(object sender, EventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            var LayoutRootAnim = new DoubleAnimation();
            //LayoutRootAnim.From = 1;
            //LayoutRootAnim.To = 0;
            //LayoutRootAnim.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            //LayoutRoot.Background.BeginAnimation(SolidColorBrush.OpacityProperty, LayoutRootAnim);
            //LayoutRoot.Background.Opacity = 0;
            LayoutRoot.Background = new ImageBrush(new BitmapImage(new Uri(BaseUriHelper.GetBaseUri(this), "../Resources/homecinema.png")));
            //LayoutRootAnim.From = 0;
            //LayoutRootAnim.To = 1;
            //LayoutRootAnim.Duration = new Duration(TimeSpan.FromSeconds(1));
            //LayoutRoot.Background.BeginAnimation(SolidColorBrush.OpacityProperty, LayoutRootAnim);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Time.Text = DateTime.Now.ToString("HH:mm");
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
