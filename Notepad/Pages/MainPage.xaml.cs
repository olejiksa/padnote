using Fonts;
using Notepad.Helpers;
using Portable.Text;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ude;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;

namespace Notepad.Pages
{
    public sealed partial class MainPage : Page
    {
        private bool isCtrlKeyPressed = false;
        StorageFile file2 = null;
        public static StatusBar status;
        public static string _isLightChecked;
        byte isFocused = 0;
        TextBox goToBox = new TextBox();
        ContentDialog goToDialog = new ContentDialog();
        public static string text = "", content = "";

        /// <summary>
        /// Менеджер по печати.
        /// </summary>
        PrintManager printmgr = PrintManager.GetForCurrentView();

        /// <summary>
        /// Документ для печати.
        /// </summary>
        PrintDocument PrintDoc = null;

        /// <summary>
        /// Трансформер.
        /// </summary>
        RotateTransform rottrf = null;

        PrintTask Task = null;

        PrintPage printPage;

        /// <summary>
        /// Доступ к специальным возможностям.
        /// </summary>
        private AccessibilitySettings access;

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            if (content.EndsWith("\r"))
            {
                content = content.Remove(content.Length - 1);
                richEditBox.Document.SetText(TextSetOptions.None, content);
            }
            List<string> fontStrings = new List<string>(FontEnumerator.GetFonts());
            fontStrings.Sort();
            //List<ComboBoxItem> fontsList = new List<ComboBoxItem>();

            //foreach (var font in fontStrings)
            //    fontsList.Add(new ComboBoxItem { Content = font, FontFamily = new FontFamily(font) } );

            family.ItemsSource = fontStrings;

            string str = null;
            richEditBox.Document.GetText(TextGetOptions.None, out str);
            access = new AccessibilitySettings();
            access.HighContrastChanged += Access_HighContrastChanged;
            Access_HighContrastChanged(null, null);
            printmgr.PrintTaskRequested += Printmgr_PrintTaskRequested;
        }

        private void Printmgr_PrintTaskRequested(PrintManager Sender, PrintTaskRequestedEventArgs args)
        {
            // Get PrintTaskRequest tasks associated with property from the Request Parameter in
            // After creating the print content and tasks calling the Complete method for printing 
            var deferral = args.Request.GetDeferral();
            // create a print task 
            Task = args.Request.CreatePrintTask(App.Title, OnPrintTaskSourceRequrested);
            Task.Completed += PrintTask_Completed;
            deferral.Complete();
        }

        private void PrintTask_Completed(PrintTask Sender, PrintTaskCompletedEventArgs args)
        {
            // print completed 
        }

        private async void OnPrintTaskSourceRequrested(PrintTaskSourceRequestedArgs args)
        {
            var def = args.GetDeferral();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                 // Set the print source 
                 args.SetSource(PrintDoc?.DocumentSource);
                });
            def.Complete();
        }

        private async void print_Click(object sender, RoutedEventArgs e)
        {
            if (PrintDoc != null)
            {
                PrintDoc.GetPreviewPage += OnGetPreviewPage;
                PrintDoc.Paginate += PrintDic_Paginate;
                PrintDoc.AddPages += PrintDic_AddPages;
            }

            PrintDoc = new PrintDocument();

            
            string q;
            richEditBox.Document.GetText(TextGetOptions.FormatRtf, out q);

            printPage = new PrintPage(q);
            // subscribe preview event 
            PrintDoc.GetPreviewPage += OnGetPreviewPage;
            // print parameters occur Subscribe preview change the direction of events such as the document 
            PrintDoc.Paginate += PrintDic_Paginate;
            // add a page to handle events 
            PrintDoc.AddPages += PrintDic_AddPages;

            // display the Print dialog box 
            bool showPrint = await PrintManager.ShowPrintUIAsync();
        }

        // add the contents of the printed page 
        private void PrintDic_AddPages(object sender, AddPagesEventArgs e)
        {
            // add elements of a page to be printed 
            PrintDoc.AddPage(printPage);

            // completed to increase the printed page 
            PrintDoc.AddPagesComplete();
        }

        private void PrintDic_Paginate(object sender, PaginateEventArgs e)
        {
            PrintTaskOptions opt = Task.Options;
            // to adjust according to the direction of the page print direction of rotation 
            switch (opt.Orientation)
            {
                case PrintOrientation.Default:
                    rottrf.Angle = 0d;
                    break;
                case PrintOrientation.Portrait:
                    rottrf.Angle = 0d;
                    break;
                case PrintOrientation.Landscape:
                    rottrf.Angle = 90d;
                    break;
            }
            PrintDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }

        private void OnGetPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            // set to preview page 
            PrintDoc.SetPreviewPage(e.PageNumber, printPage);
        }

        //private void CreateTitleBar()
        //{
        //    CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        //    coreTitleBar.ExtendViewIntoTitleBar = true;

        //    //TitleBar.Height = coreTitleBar.Height;
        //    Window.Current.SetTitleBar(MainTitleBar);
        //}

        private void Access_HighContrastChanged(AccessibilitySettings sender, object args)
        {
            if (access.HighContrast)
                if (access.HighContrastScheme == "High Contrast White")
                    RequestedTheme = ElementTheme.Light;
                else
                    RequestedTheme = ElementTheme.Dark;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            App.NavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            {
                status = StatusBar.GetForCurrentView();
                if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
                {
                    status.BackgroundColor = Color.FromArgb(255, 31, 31, 31);
                    status.ForegroundColor = Color.FromArgb(255, 199, 199, 199);
                }
                else if (Application.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    status.BackgroundColor = Color.FromArgb(255, 220, 220, 220);
                    status.ForegroundColor = Color.FromArgb(255, 96, 96, 96);
                }
                else
                {
                    byte theme = 3;

                    try
                    {
                        theme = byte.Parse(ApplicationData.Current.LocalSettings.Values["Background"].ToString());
                    }
                    catch { }

                    if (theme == 1)
                    {
                        status.BackgroundColor = Color.FromArgb(255, 220, 220, 220);
                        status.ForegroundColor = Color.FromArgb(255, 96, 96, 96);
                    }
                    else if (theme == 2)
                    {
                        status.BackgroundColor = Color.FromArgb(255, 31, 31, 31);
                        status.ForegroundColor = Color.FromArgb(255, 199, 199, 199);
                    }
                }
                status.BackgroundOpacity = 1;
            }
            else
                if (JumpList.IsSupported())
                    AppHelper.GetJumpList();

            try
            {
                StorageFile file = file2 = (StorageFile)e.Parameter;
                if (file != null)
                {
                    try
                    {
                        content = await ReadFileText(file);
                        richEditBox.Document.SetText(TextSetOptions.None, content);
                        if (file.Attributes.ToString().Contains("ReadOnly"))
                            richEditBox.IsReadOnly = true;
                        else
                            richEditBox.IsReadOnly = false;
                        text = content;
                        richEditBox.Document.UndoLimit = 0;
                        richEditBox.Document.UndoLimit = 100;
                    }
                    catch
                    {
                        await new MessageDialog(StringHelper.GetString("File") + " \"" + file.DisplayName + "\" " +
                            StringHelper.GetString("FileCorrupted"), "Padnote").ShowAsync();
                        ApplicationView.GetForCurrentView().Title = App.Title = StringHelper.GetString("Untitled");
                    }
                }
            }
            catch { }

            try
            {
                ApplicationView.GetForCurrentView().Title = App.Title = file2.DisplayName;
            }
            catch
            {
                ApplicationView.GetForCurrentView().Title = App.Title = StringHelper.GetString("Untitled");
            }

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Background"))
            {
                _isLightChecked = (string)ApplicationData.Current.LocalSettings.Values["Background"];

                if (int.Parse(_isLightChecked) == 1)
                {
                    RequestedTheme = ElementTheme.Light;
                    var appView = ApplicationView.GetForCurrentView();
                    var titleBar = appView.TitleBar;
                    titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor =
                        Color.FromArgb(255, 220, 220, 220);
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 206, 206, 206);
                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 192, 192, 192);
                    titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleBar.ButtonPressedForegroundColor = titleBar.ButtonHoverForegroundColor = Colors.Black;
                }
                else if (int.Parse(_isLightChecked) == 2)
                {
                    RequestedTheme = ElementTheme.Dark;
                    var appView = ApplicationView.GetForCurrentView();
                    var titleBar = appView.TitleBar;
                    titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor =
                        Color.FromArgb(255, 31, 31, 31);
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 53, 53, 53);
                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 76, 76, 76);
                    titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleBar.ButtonPressedForegroundColor = titleBar.ButtonHoverForegroundColor = Colors.White;
                }
                else
                    RequestedTheme = ElementTheme.Default;
            }
            else
            {
                if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                {
                    RequestedTheme = ElementTheme.Light;
                    var appView = ApplicationView.GetForCurrentView();
                    var titleBar = appView.TitleBar;
                    titleBar.BackgroundColor = titleBar.ButtonBackgroundColor = titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor =
                        Color.FromArgb(255, 220, 220, 220);
                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 196, 196, 196);
                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 192, 192, 192);
                    titleBar.ForegroundColor = titleBar.ButtonForegroundColor = titleBar.ButtonPressedForegroundColor = titleBar.ButtonHoverForegroundColor = Colors.Black;
                }
                else
                {
                    status.BackgroundColor = Color.FromArgb(255, 31, 31, 31);
                    status.ForegroundColor = Color.FromArgb(255, 199, 199, 199);
                }
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("SpellCheck"))
            {
                richEditBox.IsSpellCheckEnabled = bool.Parse((string)ApplicationData.Current.LocalSettings.Values["SpellCheck"]);
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("FullScreen"))
            {
                if (bool.Parse((string)ApplicationData.Current.LocalSettings.Values["FullScreen"]))
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                else
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("WordWrap"))
            {
                richEditBox.TextWrapping = bool.Parse((string)ApplicationData.Current.LocalSettings.Values["WordWrap"]) ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Bold"))
            {
                App.bold = bool.Parse((string)ApplicationData.Current.LocalSettings.Values["Bold"]);
                if (App.bold) richEditBox.FontWeight = FontWeights.Bold;
                else richEditBox.FontWeight = FontWeights.Normal;
                bold.IsChecked = App.bold;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Italic"))
            {
                App.italic = bool.Parse((string)ApplicationData.Current.LocalSettings.Values["Italic"]);
                if (App.italic) richEditBox.FontStyle = FontStyle.Italic;
                else richEditBox.FontStyle = FontStyle.Normal;
                italic.IsChecked = App.italic;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Underline"))
            {
                App.underline = bool.Parse((string)ApplicationData.Current.LocalSettings.Values["Underline"]);
                underline.IsChecked = App.underline;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Show"))
            {
                App.show = bool.Parse((string)ApplicationData.Current.LocalSettings.Values["Show"]);
                if (App.show) wordCount.Visibility = Visibility.Visible;
                else wordCount.Visibility = Visibility.Collapsed;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("FontSize"))
            {
                richEditBox.FontSize = int.Parse((string)ApplicationData.Current.LocalSettings.Values["FontSize"]);
                size.Label = richEditBox.FontSize.ToString();
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Family"))
                family.SelectedIndex = (int)ApplicationData.Current.LocalSettings.Values["Family"];
            else
                family.SelectedIndex = 0;
            if (CultureInfo.CurrentCulture.Name == "ru" || CultureInfo.CurrentCulture.Name == "uk" || CultureInfo.CurrentCulture.Name == "be")
            {
                FontIcon b = new FontIcon();
                b.FontSize = 28;
                b.Glyph = "\uE1B9";
                bold.Icon = b;
                FontIcon i = new FontIcon();
                i.FontSize = 28;
                i.Glyph = "\uE1B4";
                italic.Icon = i;
                FontIcon u = new FontIcon();
                u.FontSize = 28;
                u.Glyph = "\uE1B8";
                underline.Icon = u;
            }
            if (access.HighContrast)
                ApplicationData.Current.LocalSettings.Values["Background"] = access.HighContrastScheme == "High Contrast White" ? "1" : "2";
        }

        private async void open_Click(object sender, RoutedEventArgs e)
        {
            if (richEditBox.Document.CanUndo())
            {
                try
                {
                    MessageDialog unsaved = new MessageDialog(StringHelper.GetString("Command_Content") + " \"" +
                            ApplicationView.GetForCurrentView().Title + "\"?", "Padnote");
                    unsaved.Commands.Add(new UICommand { Label = StringHelper.GetString("Command_Save"), Id = 0 });
                    unsaved.Commands.Add(new UICommand { Label = StringHelper.GetString("Command_NotSave"), Id = 1 });
                    if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                        unsaved.Commands.Add(new UICommand
                        {
                            Label =
                            StringHelper.GetString("Command_Cancel"),
                            Id = 2
                        });
                    var result = await unsaved.ShowAsync();
                    if ((int)result.Id == 0)
                        save_Click(sender, e);
                    if ((int)result.Id == 0 || (int)result.Id == 1)
                    {
                        var picker = new FileOpenPicker();
                        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                        picker.FileTypeFilter.Add(".txt");
                        picker.FileTypeFilter.Add("*");

                        StorageFile file = await picker.PickSingleFileAsync();
                        richEditBox.IsReadOnly = false;
                        if (file != null)
                        {
                            file2 = file;
                            var content = await ReadFileText(file);
                            richEditBox.Document.SetText(TextSetOptions.None, content);
                            ApplicationView.GetForCurrentView().Title = App.Title = file.DisplayName;
                            text = content;
                            richEditBox.Document.SetText(TextSetOptions.None, content);
                            richEditBox.IsReadOnly = file.Attributes.ToString().Contains("ReadOnly");
                            richEditBox.Document.UndoLimit = 0;
                            richEditBox.Document.UndoLimit = 100;
                            AppHelper.AddFileToMRU(file);
                        }
                    }
                }
                catch
                {
                    await new MessageDialog(StringHelper.GetString("File") +
                            StringHelper.GetString("FileCorrupted"), "Padnote").ShowAsync();
                }
            }
            else
            {
                var picker = new FileOpenPicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add(".txt");
                picker.FileTypeFilter.Add("*");

                StorageFile file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    file2 = file;
                    try
                    {
                        var content = await ReadFileText(file);
                        richEditBox.IsReadOnly = false;
                        richEditBox.Document.SetText(TextSetOptions.None, content);
                        richEditBox.IsReadOnly = file.Attributes.ToString().Contains("ReadOnly");
                        ApplicationView.GetForCurrentView().Title = App.Title = file.DisplayName;
                        text = content;
                        richEditBox.Document.UndoLimit = 0;
                        richEditBox.Document.UndoLimit = 100;
                        AppHelper.AddFileToMRU(file);
                    }
                    catch
                    {
                        await new MessageDialog(StringHelper.GetString("File") + " \"" + file.DisplayName + "\" " +
                            StringHelper.GetString("FileCorrupted"), "Padnote").ShowAsync();
                    }
                }
            }
            if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                richEditBox.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Перехватывает содержимое читаемого файла, определяя его кодировку и возвращая строку.
        /// </summary>
        /// <returns>Текст.</returns>
        private async Task<string> ReadFileText(StorageFile file)
        {
            string text = null;
            if (file != null)
            {
                // Считывает содержимое в буфер.
                var buffer = await FileIO.ReadBufferAsync(file);
                using (DataReader dataReader = DataReader.FromBuffer(buffer))
                {
                    // Получает байты из буфера.
                    var data = new byte[buffer.Length];
                    dataReader.ReadBytes(data);
                    ICharsetDetector cdet = new CharsetDetector();
                    cdet.Feed(data, 0, data.Length);
                    cdet.DataEnd();
                    if (cdet.Charset != null)
                        text = Encoding.GetEncoding(cdet.Charset).GetString(data, 0, data.Length);
                    //// Конвертирует байты в строку.
                    //if (data.Length > 1 && data[0] == 0xff && data[1] == 0xfe)
                    //    // Little-endian Unicode.
                    //    text = System.Text.Encoding.Unicode.GetString(data, 0, data.Length);
                    //else if (data.Length > 1 && data[0] == 0xfe && data[1] == 0xff)
                    //    // Big-endian Unicode.
                    //    text = System.Text.Encoding.BigEndianUnicode.GetString(data, 0, data.Length);
                    //else
                    //    // Используется для не-Unicode-файлов (ASCII, 1252 и так далее).
                    //    try
                    //    {
                    //        text = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                    //        if (text.Contains("�"))
                    //            text = System.Text.Encoding.UTF7.GetString(data, 0, data.Length);
                    //    }
                    //    catch
                    //    {
                    //        var encoding = System.Text.Encoding.GetEncoding("Windows-1252");
                    //        text = encoding.GetString(data, 0, data.Length);
                    //    }
                    if (text.Contains("\r\n"))
                        AppHelper.CurrentNewline = "\r\n";
                    else if (text.Contains("\n"))
                        AppHelper.CurrentNewline = "\n";
                    else if (text.Contains("\r"))
                         AppHelper.CurrentNewline = "\r";
                    switch (AppHelper.CurrentNewline)
                    {
                        case "\r\n": endline.Text = "CR+LF"; break;
                        case "\n": endline.Text = "LF"; break;
                        case "\r": endline.Text = "CR"; break;
                    }
                }
            }
            return text;
        }

        private async void save_Click(object sender, RoutedEventArgs e)
        {
            if (file2 != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file2);
                // write to file
                await FileIO.WriteTextAsync(file2, content);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file2);
                if (status != FileUpdateStatus.Complete)
                    await new MessageDialog(StringHelper.GetString("MessageDialog_File") + " \"" + file2.DisplayName + "\" "
                        + StringHelper.GetString("MessageDialog_Couldn't")).ShowAsync();
                else
                    text = content;
            }
            else
                saveAs_Click(null, null);
        }

        private async void saveAs_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add(StringHelper.GetString("TextDocument"), new List<string>() { ".txt" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = StringHelper.GetString("Untitled");

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                file2 = file;
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Ending"))
                {
                    string s = ApplicationData.Current.LocalSettings.Values["Ending"].ToString();
                    switch (s)
                    {
                        case "CR": content = content.Replace(AppHelper.CurrentNewline, "\r"); break;
                        case "LF": content = content.Replace(AppHelper.CurrentNewline, "\n"); break;
                        case "CR+LF":
                            content = content.Replace(AppHelper.CurrentNewline, "\r\n");
                            break;
                    }
                }
                else
                    content = content.Replace(AppHelper.CurrentNewline, "\r\n");
                // write to file
                await FileIO.WriteTextAsync(file, content);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    ApplicationView.GetForCurrentView().Title = App.Title = file.DisplayName;
                    text = content;
                    AppHelper.AddFileToMRU(file);
                }
                else
                    await new MessageDialog(StringHelper.GetString("MessageDialog_File") + " \"" + file.DisplayName + "\" "
                        + StringHelper.GetString("MessageDialog_Couldn't")).ShowAsync();
            }
        }

        private void page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Control) isCtrlKeyPressed = true;
            if (isCtrlKeyPressed)
                switch (e.Key)
                {
                    case VirtualKey.O: open_Click(null, null); break;
                    case VirtualKey.S: save_Click(null, null); break;
                    case VirtualKey.N: new_Click(null, null); break;
                    case VirtualKey.F:
                        if (searchPanel.Visibility == Visibility.Visible)
                            searchBox.Focus(FocusState.Programmatic);
                        else if (searchBoxDesktop.Visibility == Visibility.Visible)
                            searchBoxDesktop.Focus(FocusState.Programmatic);
                        else
                        {
                            searchPanel.Visibility = Visibility.Visible;
                            find.IsChecked = true;
                        }
                        break;
                    case VirtualKey.G: wordCount_Tapped(null, null); break;
                    case VirtualKey.Add: increase_Click(null, null); break;
                    case VirtualKey.Subtract: decrease_Click(null, null); break;
                    case VirtualKey.F5: richEditBox.Document.Selection.TypeText(DateTime.Now.ToString()); break;
                    // Для шрифта.
                    case VirtualKey.B: bold.IsChecked = !bold.IsChecked; break;
                    case VirtualKey.I: italic.IsChecked = !italic.IsChecked; break;
                    case VirtualKey.U: underline.IsChecked = !underline.IsChecked; break;
                    // Для печати.
                    case VirtualKey.P: print_Click(null, null); break;
                }
            else
                switch (e.Key)
                {
                    case VirtualKey.F3: OnFindNextClicked(null, null); break;
                    case VirtualKey.F5: richEditBox.Document.Selection.TypeText(DateTime.Now.ToString()); break;
                    case VirtualKey.F11: fullScreenMode_Click(null, null); break;
                }
        }

        private void page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            isCtrlKeyPressed = false;
        }

        private void increase_Click(object sender, RoutedEventArgs e)
        {
            if (richEditBox.FontSize < 72)
            {
                richEditBox.FontSize = richEditBox.FontSize + 1;
                ApplicationData.Current.LocalSettings.Values["FontSize"] = size.Label = richEditBox.FontSize.ToString();
            }
        }

        private void decrease_Click(object sender, RoutedEventArgs e)
        {
            if (richEditBox.FontSize > 8)
            {
                richEditBox.FontSize = richEditBox.FontSize - 1;
                ApplicationData.Current.LocalSettings.Values["FontSize"] = size.Label = richEditBox.FontSize.ToString();
            }
        }

        private void bold_Checked(object sender, RoutedEventArgs e)
        {
            richEditBox.FontWeight = FontWeights.Bold;
            ApplicationData.Current.LocalSettings.Values["Bold"] = bool.TrueString;
        }

        private void bold_Unchecked(object sender, RoutedEventArgs e)
        {
            richEditBox.FontWeight = FontWeights.Normal;
            ApplicationData.Current.LocalSettings.Values["Bold"] = bool.FalseString;
        }

        private void italic_Checked(object sender, RoutedEventArgs e)
        {
            richEditBox.FontStyle = FontStyle.Italic;
            ApplicationData.Current.LocalSettings.Values["Italic"] = bool.TrueString;
        }

        private void italic_Unchecked(object sender, RoutedEventArgs e)
        {
            richEditBox.FontStyle = FontStyle.Normal;
            ApplicationData.Current.LocalSettings.Values["Italic"] = bool.FalseString;
        }

        private void undo_Click(object sender, RoutedEventArgs e)
        {
            richEditBox.Document.Undo();
            if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                richEditBox.Focus(FocusState.Programmatic);
        }

        private void redo_Click(object sender, RoutedEventArgs e)
        {
            richEditBox.Document.Redo();
            if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                richEditBox.Focus(FocusState.Programmatic);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //CreateTitleBar();

            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
                ApplicationData.Current.LocalSettings.Values["FullScreen"] = bool.TrueString;
            else
                ApplicationData.Current.LocalSettings.Values["FullScreen"] = bool.FalseString;
        }

        private void family_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                richEditBox.FontFamily = new FontFamily((string)family.SelectedItem);
                ApplicationData.Current.LocalSettings.Values["Family"] = family.SelectedIndex;
            }
            catch { }
        }

        private void richEditBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Tab)
            {
                RichEditBox richEditBox = sender as RichEditBox;
                if (richEditBox != null)
                {
                    richEditBox.Document.Selection.TypeText("\t");
                    e.Handled = true;
                }
            }
            l.Text = GetPosition(content).Item1.ToString();
            co.Text = GetPosition(content).Item2.ToString();
        }

        private void richEditBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            try
            {
                var editBox = sender as RichEditBox;
                if (editBox.Document.CanPaste())
                {
                    editBox.Document.Selection.Paste(1);
                    e.Handled = true;
                }
            }
            catch
            {
            }
        }

        private void underline_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["Underline"] = bool.TrueString;
            try
            {
                if (underline.IsChecked == true)
                {
                    var sel = richEditBox.Document.Selection;
                    sel.StartPosition = 0;
                    sel.EndPosition = sel.StoryLength;
                    sel.CharacterFormat.Underline = UnderlineType.Thin;
                }
                else
                {
                    var sel = richEditBox.Document.Selection;
                    sel.StartPosition = 0;
                    sel.EndPosition = sel.StoryLength;
                    sel.CharacterFormat.Underline = UnderlineType.None;
                }
            }
            catch { }
        }

        private void underline_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["Underline"] = bool.FalseString;
            try
            {
                if (underline.IsChecked == true)
                {
                    var sel = richEditBox.Document.Selection;
                    sel.StartPosition = 0;
                    sel.EndPosition = sel.StoryLength;
                    sel.CharacterFormat.Underline = UnderlineType.Thin;
                }
                else
                {
                    var sel = richEditBox.Document.Selection;
                    sel.StartPosition = 0;
                    sel.EndPosition = sel.StoryLength;
                    sel.CharacterFormat.Underline = UnderlineType.None;
                }
            }
            catch { }
        }

        private void richEditBox_LayoutUpdated(object sender, object e)
        {
            undo.IsEnabled = richEditBox.Document.CanUndo();
            redo.IsEnabled = richEditBox.Document.CanRedo();
            if (isFocused != 2)
            {
                isFocused++;
                richEditBox.Focus(FocusState.Programmatic);
            }
        }

        private Tuple<int, int> GetPosition(string content)
        {
            int i, pos = 0, line = 1;
            try
            {
                while ((i = content.IndexOf('\r',
                    pos, richEditBox.Document.Selection.StartPosition - pos)) != -1)
                {
                    pos = i + 1;
                    line++;
                }
            }
            catch
            {

            }
            int column = richEditBox.Document.Selection.StartPosition - pos + 1;

            return Tuple.Create(line, column);
        }

        private void richEditBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            richEditBox.Document.GetText(TextGetOptions.None, out content);
            cc.Text = (richEditBox.Document.Selection.StoryLength - 1).ToString();
            cc.Text = (richEditBox.Document.Selection.StoryLength - 1).ToString();
            int c = 0;
            for (int i = 1; i < content.Length; i++)
            {
                if (char.IsWhiteSpace(content[i - 1]) == true)
                    if (char.IsLetter(content[i]) == true && !char.IsPunctuation(content[i]))
                        c++;
            }
            if (content.Length > 2 && char.IsLetter(content.Trim()[0]))
                c++;
            wc.Text = c.ToString();
            l.Text = GetPosition(content).Item1.ToString();
            co.Text = GetPosition(content).Item2.ToString();
        }

        private void searchBoxDesktop_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                var sel = richEditBox.Document.Selection;
                int text = sel.FindText(searchBoxDesktop.Text, TextConstants.MaxUnitCount, FindOptions.None);
                richEditBox.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        private void searchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                var sel = richEditBox.Document.Selection;
                int text = sel.FindText(searchBox.Text, TextConstants.MaxUnitCount, FindOptions.None);
                richEditBox.Focus(FocusState.Programmatic);
                e.Handled = true;
            }
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void richEditBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Выделяет цвет при фокусе.
            //ITextSelection selectedText = richEditBox.Document.Selection;
            //if (selectedText != null)
            //{
            //    richEditBox.Document.Selection.SetRange(_selectionStart, _selectionEnd);
            //    selectedText.CharacterFormat.BackgroundColor = Colors.White;
            //}
        }

        private void richEditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Выделяет цвет при дефокусе.
            //_selectionEnd = richEditBox.Document.Selection.EndPosition;
            //_selectionStart = richEditBox.Document.Selection.StartPosition;

            //ITextSelection selectedText = richEditBox.Document.Selection;
            //if (selectedText != null)
            //    selectedText.CharacterFormat.BackgroundColor = Colors.Gray;
        }

        private void OnFindNextClicked(object sender, RoutedEventArgs e)
        {
            var sel = richEditBox.Document.Selection;
            if (searchPanel.Visibility == Visibility.Visible)
                sel.FindText(searchBox.Text, TextConstants.MaxUnitCount, FindOptions.None);
            else
                sel.FindText(searchBoxDesktop.Text, TextConstants.MaxUnitCount, FindOptions.None);
            richEditBox.Focus(FocusState.Programmatic);
        }

        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            forward.IsEnabled = !string.IsNullOrWhiteSpace(searchBoxDesktop.Text);
            forward2.IsEnabled = !string.IsNullOrWhiteSpace(searchBox.Text);
        }

        private void FindChecked(object sender, RoutedEventArgs e)
        {
            searchPanel.Visibility = bord.Visibility = Visibility.Visible;
            searchBox.Focus(FocusState.Programmatic);
        }

        private void FindUnchecked(object sender, RoutedEventArgs e)
        {
            searchPanel.Visibility = bord.Visibility = Visibility.Collapsed;
        }

        private void GoToBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && !string.IsNullOrWhiteSpace(goToBox.Text))
            {
                GoToDialog_PrimaryButtonClick(null, null);
                goToDialog.Hide();
                e.Handled = true;
            }
        }

        private void GoToDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                var save = richEditBox.Document.Selection.StartPosition;
                int linePointOne = GetPosition(content).Item1;
                int linePointTwo = GetPosition(content).Item2;
                int goToPoint = int.Parse(goToBox.Text);
                richEditBox.Document.Selection.StartPosition = richEditBox.Document.Selection.StoryLength;
                int lines = GetPosition(content).Item1;
                richEditBox.Document.Selection.StartPosition = save;
                while (linePointOne != goToPoint || linePointTwo != 1)
                {
                    if (goToPoint > 0 && goToPoint <= lines)
                    {
                        if (linePointOne > goToPoint)
                            richEditBox.Document.Selection.StartPosition--;
                        else if (linePointOne < goToPoint)
                            richEditBox.Document.Selection.StartPosition++;
                        else if (linePointTwo > 1)
                            richEditBox.Document.Selection.StartPosition--;
                        linePointOne = GetPosition(content).Item1;
                        linePointTwo = GetPosition(content).Item2;
                    }
                    else
                    {
                        break;
                    }
                }
                richEditBox.Document.Selection.EndPosition = richEditBox.Document.Selection.StartPosition;
                richEditBox.Focus(FocusState.Programmatic);
            }
            catch
            {
            }
        }

        private void GoToDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            goToBox.Text = string.Empty;
        }

        private void GoToBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            goToDialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(goToBox.Text);
        }

        /// <summary>
        /// Создаёт новый чистый файл.
        /// </summary>
        private async void new_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (richEditBox.Document.CanUndo())
                {
                    MessageDialog unsaved = new MessageDialog(StringHelper.GetString("Command_Content") + " \"" +
                            ApplicationView.GetForCurrentView().Title + "\"?", "Padnote");
                    unsaved.Commands.Add(new UICommand { Label = StringHelper.GetString("Command_Save"), Id = 0 });
                    unsaved.Commands.Add(new UICommand { Label = StringHelper.GetString("Command_NotSave"), Id = 1 });
                    if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                        unsaved.Commands.Add(new UICommand
                        {
                            Label =
                            StringHelper.GetString("Command_Cancel"),
                            Id = 2
                        });
                    var result = await unsaved.ShowAsync();
                    if ((int)result.Id == 0)
                        save_Click(null, null);
                    if ((int)result.Id == 0 || (int)result.Id == 1)
                        New();
                }
                else
                    New();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message).ShowAsync();
            }
        }

        private void New()
        {
            file2 = null;
            richEditBox.Document.SetText(TextSetOptions.None, string.Empty);
            text = content = string.Empty;
            ApplicationView.GetForCurrentView().Title = StringHelper.GetString("Untitled");
            richEditBox.Document.UndoLimit = 0;
            richEditBox.Document.UndoLimit = 100;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Ending"))
                endline.Text = ApplicationData.Current.LocalSettings.Values["Ending"].ToString();
        }

        private void richEditBox_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Copy";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
        }

        private async void richEditBox_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var texto = await e.DataView.GetStorageItemsAsync();
                if (texto.Any())
                {
                    file2 = texto[0] as StorageFile;
                    if (file2 != null)
                    {
                        try
                        {
                            IRandomAccessStream randAccStream =
                                   await file2.OpenAsync(FileAccessMode.Read);  
                            var st = randAccStream.AsStream();
                            StreamReader sr = new StreamReader(st);
                            string s = await sr.ReadToEndAsync();
                            richEditBox.Document.SetText(TextSetOptions.None, s);
                            if (file2.Attributes.ToString().Contains("ReadOnly"))
                                richEditBox.IsReadOnly = true;
                            else
                                richEditBox.IsReadOnly = false;
                            richEditBox.Document.UndoLimit = 0;
                            richEditBox.Document.UndoLimit = 100;
                            text = content;
                            ApplicationView.GetForCurrentView().Title = App.Title = file2.DisplayName;
                            AppHelper.AddFileToMRU(file2);
                        }
                        catch
                        {
                            await new MessageDialog(StringHelper.GetString("File") + " \"" + file2.DisplayName + "\" " +
                                StringHelper.GetString("FileCorrupted"), "Padnote").ShowAsync();
                        }
                    }
                }                
            }
        }

        private async void wordCount_Tapped(object sender, TappedRoutedEventArgs e)
        {
            richEditBox.Focus(FocusState.Programmatic);
            goToBox.Header = StringHelper.GetString("LineNumber");
            goToBox.Margin = new Thickness(0, 15, 0, 0);
            goToBox.TextChanged += GoToBoxTextChanged;
            goToBox.KeyDown += GoToBox_KeyDown;
            InputScope scope = new InputScope();
            InputScopeName scopeName = new InputScopeName();
            scopeName.NameValue = InputScopeNameValue.Number;
            scope.Names.Add(scopeName);
            goToBox.InputScope = scope;
            goToDialog.IsPrimaryButtonEnabled = false;
            goToDialog.RequestedTheme = RequestedTheme;
            goToDialog.Title = StringHelper.GetString("GoToTitle");
            goToDialog.Content = goToBox;
            goToDialog.Closed += GoToDialog_Closed;
            goToDialog.PrimaryButtonText = StringHelper.GetString("GoToText");
            goToDialog.SecondaryButtonText = StringHelper.GetString("Command_Cancel");
            goToDialog.PrimaryButtonClick += GoToDialog_PrimaryButtonClick;
            await goToDialog.ShowAsync();
        }

        private void fullScreenMode_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            var isFullScreenMode = view.IsFullScreenMode;

            if (isFullScreenMode)
                view.ExitFullScreenMode();
            else
                view.TryEnterFullScreenMode();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            NavigationHelper.FullScreenChecking();
        }
    }
}
