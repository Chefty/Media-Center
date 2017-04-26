using MyMediaCenter.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyMediaCenter.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public enum ViewType { Home, Music, Video, Picture, Settings, Radio, Exit }
        public RelayCommand switchViewCommand { get; set; }

        private HomeViewModel homeView = new HomeViewModel();
        private MusicViewModel musicView = new MusicViewModel();
        private PictureViewModel pictureView = new PictureViewModel();
        private VideoViewModel videoView = new VideoViewModel();
        private SettingsViewModel settingsView = new SettingsViewModel();
        private RadioViewModel radioView = new RadioViewModel();

        private BaseViewModel currentView;
        public BaseViewModel CurrentView
        {
            get { return currentView; }
            set
            {
                currentView = value;
                RaisePropertyChanged("CurrentView");
            }
        }

        public delegate void changeView();
        public MainWindowViewModel()
        {
            switchViewCommand = new RelayCommand(switchView);
            this.CurrentView = homeView;
        }

        public void switchView(object param)
        {
            if ((ViewType)param == ViewType.Home)
                this.CurrentView = homeView;
            else if ((ViewType)param == ViewType.Music)
                this.CurrentView = musicView;
            else if ((ViewType)param == ViewType.Picture)
                this.CurrentView = pictureView;
            else if ((ViewType)param == ViewType.Video)
                this.CurrentView = videoView;
            else if ((ViewType)param == ViewType.Settings)
                this.CurrentView = settingsView;
            else if ((ViewType)param == ViewType.Radio)
                this.CurrentView = radioView;
            else
                Application.Current.Shutdown();
        }
    }
}
