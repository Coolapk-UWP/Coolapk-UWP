using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Update;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;

namespace CoolapkUWP.ViewModels.SettingsPages
{
    public class SettingsViewModel : IViewModel
    {
        public static SettingsViewModel Caches;
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("SettingsPage");

        public string Title => _loader.GetString("Title");

        public static string DeviceFamily => AnalyticsInfo.VersionInfo.DeviceFamily.Replace('.', ' ');

        public static string ToolkitVersion => Assembly.Load(new AssemblyName("Microsoft.Toolkit.Uwp")).GetName().Version.ToString();

        public bool IsLogin
        {
            get => !string.IsNullOrEmpty(SettingsHelper.Get<string>(SettingsHelper.Uid));
            set => RaisePropertyChangedEvent();
        }

        public DateTime UpdateDate
        {
            get => SettingsHelper.Get<DateTime>(SettingsHelper.UpdateDate);
            set
            {
                if (UpdateDate != value)
                {
                    SettingsHelper.Set(SettingsHelper.UpdateDate, value);
                    RaisePropertyChangedEvent();
                }
            }
        }

        public static int SelectedTheme
        {
            get => 2 - (int)ThemeHelper.RootTheme;
            set
            {
                if (SelectedTheme != value)
                {
                    ThemeHelper.RootTheme = (ElementTheme)(2 - value);
                }
            }
        }

        public static bool IsNoPicsMode
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.IsNoPicsMode);
            set
            {
                if (IsNoPicsMode != value)
                {
                    SettingsHelper.Set(SettingsHelper.IsNoPicsMode, value);
                    ThemeHelper.UISettingChanged?.Invoke(UISettingChangedType.NoPicChanged);
                }
            }
        }

        public static bool ShowOtherException
        {
            get => SettingsHelper.Get<bool>(SettingsHelper.ShowOtherException);
            set
            {
                if (ShowOtherException != value)
                {
                    SettingsHelper.Set(SettingsHelper.ShowOtherException, value);
                }
            }
        }

        private bool isCleanCache;
        public bool IsCleanCache
        {
            get => isCleanCache;
            set
            {
                if (isCleanCache != value)
                {
                    isCleanCache = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _checkingUpdate;
        public bool CheckingUpdate
        {
            get => _checkingUpdate;
            set
            {
                if (_checkingUpdate != value)
                {
                    _checkingUpdate = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _gotoUpdateTag;
        public string GotoUpdateTag
        {
            get => _gotoUpdateTag;
            set
            {
                if (_gotoUpdateTag != value)
                {
                    _gotoUpdateTag = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Visibility _gotoUpdateVisibility;
        public Visibility GotoUpdateVisibility
        {
            get => _gotoUpdateVisibility;
            set
            {
                if (_gotoUpdateVisibility != value)
                {
                    _gotoUpdateVisibility = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool _updateStateIsOpen;
        public bool UpdateStateIsOpen
        {
            get => _updateStateIsOpen;
            set
            {
                if (_updateStateIsOpen != value)
                {
                    _updateStateIsOpen = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _updateStateMessage;
        public string UpdateStateMessage
        {
            get => _updateStateMessage;
            set
            {
                if (_updateStateMessage != value)
                {
                    _updateStateMessage = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private InfoBarSeverity _updateStateSeverity;
        public InfoBarSeverity UpdateStateSeverity
        {
            get => _updateStateSeverity;
            set
            {
                if (_updateStateSeverity != value)
                {
                    _updateStateSeverity = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _updateStateTitle;
        public string UpdateStateTitle
        {
            get => _updateStateTitle;
            set
            {
                if (_updateStateTitle != value)
                {
                    _updateStateTitle = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string _aboutTextBlockText;
        public string AboutTextBlockText
        {
            get => _aboutTextBlockText;
            set
            {
                if (_aboutTextBlockText != value)
                {
                    _aboutTextBlockText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public string VersionTextBlockText
        {
            get
            {
                string ver = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";
                string name = ResourceLoader.GetForViewIndependentUse().GetString("AppName") ?? "酷安";
                GetAboutTextBlockText();
                return $"{name} v{ver}";
            }
        }

        public SettingsViewModel() => Caches = this;

        private async void GetAboutTextBlockText()
        {
            await Task.Run(async () =>
            {
                string langcode = LanguageHelper.GetPrimaryLanguage();
                Uri dataUri = new Uri($"ms-appx:///Assets/About/About.{langcode}.md");
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                if (file != null)
                {
                    string markdown = await FileIO.ReadTextAsync(file);
                    _ = DispatcherHelper.ExecuteOnUIThreadAsync(() => AboutTextBlockText = markdown);
                }
            });
        }

        public async void CleanCache()
        {
            IsCleanCache = true;
            await ImageCacheHelper.CleanCacheAsync();
            IsCleanCache = false;
        }

        public async void CheckUpdate()
        {
            CheckingUpdate = true;
            UpdateInfo info = null;
            try
            {
                info = await UpdateHelper.CheckUpdateAsync("Coolapk-UWP", "Coolapk-UWP");
            }
            catch (Exception ex)
            {
                UpdateStateIsOpen = true;
                UpdateStateMessage = ex.Message;
                UpdateStateSeverity = InfoBarSeverity.Error;
                GotoUpdateVisibility = Visibility.Collapsed;
                UpdateStateTitle = _loader.GetString("CheckFailed");
                SettingsHelper.LogManager.GetLogger(nameof(SettingsViewModel)).Error(ex.ExceptionToMessage(), ex);
            }
            if (info != null)
            {
                if (info.IsExistNewVersion)
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateTag = info.ReleaseUrl;
                    GotoUpdateVisibility = Visibility.Visible;
                    UpdateStateSeverity = InfoBarSeverity.Warning;
                    UpdateStateTitle = _loader.GetString("FindUpdate");
                    UpdateStateMessage = $"{VersionTextBlockText} -> {info.TagName}";
                }
                else
                {
                    UpdateStateIsOpen = true;
                    GotoUpdateVisibility = Visibility.Collapsed;
                    UpdateStateSeverity = InfoBarSeverity.Success;
                    UpdateStateTitle = _loader.GetString("UpToDate");
                }
            }
            UpdateDate = DateTime.Now;
            CheckingUpdate = false;
        }

        public Task Refresh(bool reset) => throw new NotImplementedException();
    }
}
