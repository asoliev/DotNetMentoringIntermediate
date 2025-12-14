using System;
using System.Windows;
using System.Windows.Threading;

namespace GameOfLife;

public partial class MainWindow : Window
{
    private Grid mainGrid;
    DispatcherTimer? timer;   // Generation timer
    private int genCounter;
    private AdWindow?[]? adWindow;

    public MainWindow()
    {
        InitializeComponent();
        mainGrid = new Grid(MainCanvas);

        timer = new DispatcherTimer();
        timer.Tick += OnTimer;
        timer.Interval = TimeSpan.FromMilliseconds(200);
        
        this.Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        mainGrid?.Dispose();
        if (timer != null)
        {
            timer.Stop();
            timer.Tick -= OnTimer;
            timer = null;
        }
    }

    private void StartAd()
    {
        adWindow ??= new AdWindow?[2];

        for (int i = 0; i < 2; i++)
            if (adWindow[i] == null)
            {
                var window = new AdWindow(this);
                adWindow[i] = window;
                window.Closed += AdWindowOnClosed;
                window.Top = this.Top + (330 * i) + 70;
                window.Left = this.Left + 240;
                window.Show();
            }
    }

    private void AdWindowOnClosed(object? sender, EventArgs eventArgs)
    {
        if (adWindow == null) return;
        
        for (int i = 0; i < 2; i++)
        {
            var window = adWindow[i];
            if (window != null)
            {
                window.Closed -= AdWindowOnClosed;
                adWindow[i] = null;
            }
        }
    }


    private void Button_OnClick(object? sender, EventArgs e)
    {
        if (timer == null) return;
        
        if (!timer.IsEnabled)
        {
            timer.Start();
            ButtonStart.Content = "Stop";
            StartAd();
        }
        else
        {
            timer.Stop();
            ButtonStart.Content = "Start";
        }
    }

    private void OnTimer(object? sender, EventArgs e)
    {
        mainGrid.Update();
        genCounter++;
        lblGenCount.Content = "Generations: " + genCounter;
    }

    private void ButtonClear_Click(object? sender, RoutedEventArgs e) => mainGrid.Clear();
}