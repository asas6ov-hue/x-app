using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace MOApp
{
    public partial class MainWindow : Window
    {
        private OverlayWindow? _overlayWindow;
        private DispatcherTimer _updateTimer;
        
        // Settings
        public Color FpsColor { get; set; } = Colors.Red;
        public Color CpuColor { get; set; } = Colors.White;
        public Color GpuColor { get; set; } = Colors.White;
        public Color RamColor { get; set; } = Colors.White;
        public Color NetworkColor { get; set; } = Colors.White;

        // Performance counters
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _ramCounter;
        private long _prevUploadBytes;
        private long _prevDownloadBytes;
        private DateTime _prevNetworkTime;

        public MainWindow()
        {
            InitializeComponent();
            Title = "MO App - Settings";
            Width = 400;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeCounters();
            SetupUI();
            
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void InitializeCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // Prime the counter
            }
            catch { }

            try
            {
                _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            }
            catch { }
        }

        private void SetupUI()
        {
            var mainPanel = new StackPanel { Margin = new Thickness(20) };

            // Title
            var title = new TextBlock
            {
                Text = "MO App Settings",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(title);

            // Color settings
            mainPanel.Children.Add(CreateColorPicker("FPS Color:", ref FpsColor, "fps"));
            mainPanel.Children.Add(CreateColorPicker("CPU Color:", ref CpuColor, "cpu"));
            mainPanel.Children.Add(CreateColorPicker("GPU Color:", ref GpuColor, "gpu"));
            mainPanel.Children.Add(CreateColorPicker("RAM Color:", ref RamColor, "ram"));
            mainPanel.Children.Add(CreateColorPicker("Network Color:", ref NetworkColor, "network"));

            // Spacer
            mainPanel.Children.Add(new Border { Height = 20 });

            // Show Overlay Button
            var overlayBtn = new Button
            {
                Content = "Show Overlay",
                Padding = new Thickness(20, 10, 20, 10),
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            overlayBtn.Click += OverlayBtn_Click;
            mainPanel.Children.Add(overlayBtn);

            // Exit button
            var exitBtn = new Button
            {
                Content = "Exit",
                Padding = new Thickness(20, 10, 20, 10),
                FontSize = 16,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            exitBtn.Click += (s, e) => Application.Current.Shutdown();
            mainPanel.Children.Add(exitBtn);

            Content = mainPanel;
        }

        private StackPanel CreateColorPicker(string label, ref Color color, string name)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            
            var lbl = new TextBlock
            {
                Text = label,
                Width = 120,
                VerticalAlignment = VerticalAlignment.Center
            };
            panel.Children.Add(lbl);

            var colorPicker = new ComboBox
            {
                Width = 100,
                Tag = name,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            colorPicker.Items.Add("Red");
            colorPicker.Items.Add("Green");
            colorPicker.Items.Add("Blue");
            colorPicker.Items.Add("White");
            colorPicker.Items.Add("Yellow");
            colorPicker.Items.Add("Cyan");
            colorPicker.Items.Add("Magenta");
            colorPicker.Items.Add("Orange");

            string initialColor = color.ToString();
            if (initialColor.Contains("FF0000")) colorPicker.SelectedItem = "Red";
            else if (initialColor.Contains("00FF00")) colorPicker.SelectedItem = "Green";
            else if (initialColor.Contains("0000FF")) colorPicker.SelectedItem = "Blue";
            else if (initialColor.Contains("FFFFFF")) colorPicker.SelectedItem = "White";
            else if (initialColor.Contains("FFFF00")) colorPicker.SelectedItem = "Yellow";
            else if (initialColor.Contains("00FFFF")) colorPicker.SelectedItem = "Cyan";
            else if (initialColor.Contains("FF00FF")) colorPicker.SelectedItem = "Magenta";
            else if (color == Colors.Orange) colorPicker.SelectedItem = "Orange";
            else colorPicker.SelectedItem = "White";

            colorPicker.SelectionChanged += (s, e) =>
            {
                var selected = colorPicker.SelectedItem?.ToString();
                switch (selected)
                {
                    case "Red": color = Colors.Red; break;
                    case "Green": color = Colors.Green; break;
                    case "Blue": color = Colors.Blue; break;
                    case "White": color = Colors.White; break;
                    case "Yellow": color = Colors.Yellow; break;
                    case "Cyan": color = Colors.Cyan; break;
                    case "Magenta": color = Colors.Magenta; break;
                    case "Orange": color = Colors.Orange; break;
                }
                if (_overlayWindow != null)
                {
                    _overlayWindow.UpdateColors(this);
                }
            };

            panel.Children.Add(colorPicker);
            return panel;
        }

        private void OverlayBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            
            if (_overlayWindow == null)
            {
                _overlayWindow = new OverlayWindow(this);
            }
            _overlayWindow.Show();
        }

        public void ShowMainWindow()
        {
            if (_overlayWindow != null)
            {
                _overlayWindow.Hide();
            }
            this.Show();
        }

        private async void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Update is mainly for preview in settings
            // Real updates happen in overlay
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _updateTimer.Stop();
            _overlayWindow?.Close();
            base.OnClosing(e);
        }
    }
}
