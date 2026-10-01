using Mopups.Pages;
using Mopups.Services;

namespace MopupsIssue;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
#if IOS
        // Mopups sizes popups from this; on iOS 27 its Y and height are NaN
#pragma warning disable CA1422
        ApplicationFrameLabel.Text = $"UIScreen.ApplicationFrame: {UIKit.UIScreen.MainScreen.ApplicationFrame}";
#pragma warning restore CA1422
#endif
    }

    private async void OnShowPopupClicked(object? sender, EventArgs e)
    {
        await MopupService.Instance.PushAsync(new PopupPage
        {
            Content = new Label
            {
                Text = "Popup",
                BackgroundColor = Colors.White,
                Padding = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        });
    }
}
