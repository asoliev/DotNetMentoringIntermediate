using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GameOfLife
{
    class AdWindow : Window
    {
        private readonly DispatcherTimer adTimer;
        private int imgNmb;     // the number of the image currently shown
        private string link = string.Empty;    // the URL where the currently shown ad leads to
        private ImageBrush myBrush;

        public AdWindow(Window owner)
        {
            myBrush = new ImageBrush();
            Random rnd = new Random();
            Owner = owner;
            Width = 350;
            Height = 100;
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.ToolWindow;
            Title = "Support us by clicking the ads";
            Cursor = Cursors.Hand;
            ShowActivated = false;
            MouseDown += OnClick;

            imgNmb = rnd.Next(1, 3);
            ChangeAds(this, new EventArgs());

            // Run the timer that changes the ad's image 
            adTimer = new DispatcherTimer();
            adTimer.Interval = TimeSpan.FromSeconds(3);
            adTimer.Tick += ChangeAds;
            adTimer.Start();
        }

        private void OnClick(object? sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(link);
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            adTimer.Stop();
            Unsubscribe();
            base.OnClosed(e);
        }

        public void Unsubscribe() => adTimer.Tick -= ChangeAds;

        private void ChangeAds(object? sender, EventArgs eventArgs)
        {
            //new Uri($"pack://application:,,,/Resources/Ad{imgNmb}.png", UriKind.Absolute);

            // Clear the old ImageSource to allow garbage collection of the previous BitmapImage
            // This prevents memory leaks when images are changed every 3 seconds
            myBrush.ImageSource = null;

            var uri = new Uri($"ad{imgNmb}.jpg", UriKind.Relative);
            var newImage = new BitmapImage(uri);
            // Freeze the image to make it thread-safe and allow better memory management
            newImage.Freeze();
            myBrush.ImageSource = newImage;

            Background = myBrush;

            link = "http://example.com";

            if (imgNmb == 3)
                imgNmb = 1;
            else
                imgNmb++;
        }
    }
}