using Notepad.Helpers;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Notepad.Pages
{
    /// <summary>
    /// Страница настроек.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private StatusBar status;

        /// <summary>
        /// Доступ к специальным возможностям.
        /// </summary>
        AccessibilitySettings access;

        private List<string> _encodings = new List<string>()
        {
            "ASCII", "UTF-7", "UTF-8", "UTF-16", "UTF-32"
        };

        /// <summary>
        /// Список поддерживаемых для сохранения кодировок.
        /// </summary>
        public List<string> Encodings
        {
            get { return _encodings; }
        }

        public int EncodingsNumber
        {
            get { return 0; }
            set {  }
        }

        public SettingsPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            access = new AccessibilitySettings();
            access.HighContrastChanged += Access_HighContrastChanged;
        }

        private void Access_HighContrastChanged(AccessibilitySettings sender, object args)
        {
            if (access.HighContrast)
                if (access.HighContrastScheme == "High Contrast White")
                {
                    theme.SelectedIndex = 1;
                    RequestedTheme = stk.RequestedTheme = ElementTheme.Light;
                }
                else
                {
                    theme.SelectedIndex = 2;
                    RequestedTheme = stk.RequestedTheme = ElementTheme.Dark;
                }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.NavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            ApplicationView.GetForCurrentView().Title = string.Empty;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                default1.Visibility = Visibility.Visible;
                if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("Background"))
                {
                    try
                    {
                        status.BackgroundOpacity = 1;
                    }
                    catch { }
                    theme.SelectedIndex = 0;
                }
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Background"))
                theme.SelectedIndex = int.Parse((string)ApplicationData.Current.LocalSettings.Values["Background"]);
            else
                theme.SelectedIndex = 1;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("FullScreen"))
                if (bool.Parse((string)ApplicationData.Current.LocalSettings.Values["FullScreen"]))
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                else
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
            if (!App.show)
                statusBar.IsOn = false;
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                headerText.Margin = new Thickness(12,2,0,0);
                status = StatusBar.GetForCurrentView();
                if (stk.RequestedTheme == ElementTheme.Dark)
                {
                    status.BackgroundColor = Color.FromArgb(255, 31, 31, 31);
                    status.ForegroundColor = Color.FromArgb(255, 199, 199, 199);
                }
                else if (stk.RequestedTheme == ElementTheme.Light)
                {
                    status.BackgroundColor = Color.FromArgb(255, 220, 220, 220);
                    status.ForegroundColor = Color.FromArgb(255, 96, 96, 96);
                }
                else
                {
                    if (theme.SelectedIndex == 1)
                    {
                        status.BackgroundColor = Color.FromArgb(255, 220, 220, 220);
                        status.ForegroundColor = Color.FromArgb(255, 96, 96, 96);
                    }
                    else if (theme.SelectedIndex == 2)
                    {
                        status.BackgroundColor = Color.FromArgb(255, 31, 31, 31);
                        status.ForegroundColor = Color.FromArgb(255, 199, 199, 199);
                    }
                }
                status.BackgroundOpacity = 1;
            }

            if (Frame.ActualWidth < 480)
                stk2.Margin = new Thickness(15, 20, 0, 0);
            else
                stk2.Margin = new Thickness(30, 20, 0, 0);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            NavigationHelper.FullScreenChecking();
            App.NavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void PageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
                ApplicationData.Current.LocalSettings.Values["FullScreen"] = bool.TrueString;
            else
                ApplicationData.Current.LocalSettings.Values["FullScreen"] = bool.FalseString;
            NavigationHelper.FrameSizeChanged(Frame, stk2);

        }

        private void theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (theme.SelectedIndex == 1)
                {
                    RequestedTheme = ElementTheme.Light;
                    //commandBar.RequestedTheme = ElementTheme.Light;
                    var appView = ApplicationView.GetForCurrentView();
                    var titleBar = appView.TitleBar;
                    titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor =
                        Color.FromArgb(255, 220, 220, 220);
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 206, 206, 206);
                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 192, 192, 192);
                    titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleBar.ButtonPressedForegroundColor = titleBar.ButtonHoverForegroundColor = Colors.Black;
                    //stk.RequestedTheme = ElementTheme.Light;
                }
                else if (theme.SelectedIndex == 2)
                {
                    RequestedTheme = ElementTheme.Dark;
                    //commandBar.RequestedTheme = ElementTheme.Dark;
                    var appView = ApplicationView.GetForCurrentView();
                    var titleBar = appView.TitleBar;
                    titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor =
                        Color.FromArgb(255, 31, 31, 31);
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 53, 53, 53);
                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 76, 76, 76);
                    titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleBar.ButtonPressedForegroundColor = titleBar.ButtonHoverForegroundColor = Colors.White;
                    //stk.RequestedTheme = ElementTheme.Dark;
                }
                else
                    RequestedTheme = ElementTheme.Default;
                ApplicationData.Current.LocalSettings.Values["Background"] = theme.SelectedIndex.ToString();
                if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    status = StatusBar.GetForCurrentView();
                    if (stk.RequestedTheme == ElementTheme.Dark)
                    {
                        status.BackgroundColor = Color.FromArgb(255, 31, 31, 31);
                        status.ForegroundColor = Color.FromArgb(255, 199, 199, 199);
                    }
                    else if (stk.RequestedTheme == ElementTheme.Light)
                    {
                        status.BackgroundColor = Color.FromArgb(255, 220, 220, 220);
                        status.ForegroundColor = Color.FromArgb(255, 96, 96, 96);
                    }
                    else
                    {
                        if (theme.SelectedIndex == 1)
                        {
                            status.BackgroundColor = Color.FromArgb(255, 220, 220, 220);
                            status.ForegroundColor = Color.FromArgb(255, 96, 96, 96);
                        }
                        else if (theme.SelectedIndex == 2)
                        {
                            status.BackgroundColor = Color.FromArgb(255, 31, 31, 31);
                            status.ForegroundColor = Color.FromArgb(255, 199, 199, 199);
                        }
                    }
                    status.BackgroundOpacity = 1;
                }
            }
            catch
            {
            }
        }

        private void statusBar_Toggled(object sender, RoutedEventArgs e)
        {
            if (statusBar.IsOn)
                try { ApplicationData.Current.LocalSettings.Values["Show"] = bool.TrueString; } catch { }
            else
                try { ApplicationData.Current.LocalSettings.Values["Show"] = bool.FalseString; } catch { }
        }

        private void OnHyperlinkButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

        private void ExtendedSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ExtendedSettingsPage));
        }

        /// <summary>
        /// Выполняет открытие Магазина Windows с диалогом оценки и отзыва.
        /// </summary>
        private async void rate_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(@"ms-windows-store:REVIEW?PFN=" + Package.Current.Id.FamilyName));
        }
    }
}
