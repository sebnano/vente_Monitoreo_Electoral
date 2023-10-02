#if IOS
using Microsoft.Maui.Platform;
using ElectoralMonitoring.Platforms.iOS.Utils;
using UIKit;
using CoreGraphics;
#endif

namespace ElectoralMonitoring
{
    public class FixedScrollView : ScrollView
    {
        public FixedScrollView()
        {
#if IOS
            UIKeyboard.Notifications.ObserveWillShow(OnKeyboardShowing);
            UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHiding);
#endif
        }

#if IOS
        private async void OnKeyboardShowing(object sender, UIKeyboardEventArgs args)
        {
            if (Shell.Current.CurrentPage is ContentPage page)
            {
                UIView control = this.ToPlatform(Handler.MauiContext).FindFirstResponder();
                UIView rootUiView = page.Content.ToPlatform(Handler.MauiContext);
                CGRect kbFrame = UIKeyboard.FrameEndFromNotification(args.Notification);
                double distance = control.GetOverlapDistance(rootUiView, kbFrame) + 10;
                if (distance > 0)
                {
                    Margin = new Thickness(Margin.Left, -distance, Margin.Right, distance);
                }
                await this.ScrollToAsync(0, distance, true);
            }
        }
        private async void OnKeyboardHiding(object sender, UIKeyboardEventArgs args)
        {
            Margin = new Thickness(Margin.Left, 0, Margin.Right, 0);
            await this.ScrollToAsync(0, 0, true);
        }
#endif
    }
}