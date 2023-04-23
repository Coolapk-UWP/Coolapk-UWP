using CoolapkUWP.Helpers;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.BrowserPages
{
    public class BrowserViewModel : IViewModel
    {
        private readonly ResourceLoader _loader = ResourceLoader.GetForViewIndependentUse("BrowserPage");

        private string title;
        public string Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private Uri uri;
        public Uri Uri
        {
            get => uri;
            set
            {
                if (uri != value)
                {
                    uri = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isLoginPage;
        public bool IsLoginPage
        {
            get => isLoginPage;
            set
            {
                if (isLoginPage != value)
                {
                    isLoginPage = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public BrowserViewModel(string url)
        {
            if (!url.Contains("://")) { url = $"https://{url}"; }
            Uri = url.ValidateAndGetUri();
            IsLoginPage = url == UriHelper.LoginUri;
            Title = _loader.GetString("Title");
        }

        public Task Refresh(bool reset) => throw new NotImplementedException();

        bool IViewModel.IsEqual(IViewModel other) => other is BrowserViewModel model && IsEqual(model);

        public bool IsEqual(BrowserViewModel other) => Uri == other.Uri;
    }
}
