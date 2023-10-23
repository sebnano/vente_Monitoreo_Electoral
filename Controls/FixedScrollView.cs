#if IOS
using Microsoft.Maui.Platform;
using ElectoralMonitoring.Platforms.iOS.Utils;
using UIKit;
using CoreGraphics;
using Foundation;
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
            try
            {

                if (Shell.Current.CurrentPage is ContentPage page)
                {
                    UIView control = this.ToPlatform(Handler.MauiContext).FindFirstResponder();
                    UIView rootUiView = page.Content.ToPlatform(Handler.MauiContext);
                    CGRect kbFrame = UIKeyboard.FrameEndFromNotification(args.Notification);

#if IOS13_0_OR_GREATER
                    double distance = control.GetOverlapDistance(rootUiView, kbFrame) + 20;
#else
                    double distance = 20;
#endif
                    if (distance > 0)
                    {
                        Margin = new Thickness(Margin.Left, -distance, Margin.Right, distance);
                    }
                    await this.ScrollToAsync(0, distance, true);
                }
            }
            catch
            {

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