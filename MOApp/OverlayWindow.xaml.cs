using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Management;

namespace MOApp
{
    public partial class OverlayWindow : Window
    {
        private readonly MainWindow _mainWindow;
        private DispatcherTimer _updateTimer;
        
        // UI Elements
        private TextBlock _fpsText = null!;
        private TextBlock _cpuText = null!;
        private TextBlock _gpuText = null!;
        private TextBlock _ramText = null!;
        private TextBlock _networkText = null!;

        // Performance tracking
        private Stopwatch _fpsStopwatch = new();
        private int _frameCount;
        private double _currentFps;
        
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _ramCounter;
        private long _prevUploadBytes;
        private long _prevDownloadBytes;
        private DateTime _prevNetworkTime;
        private bool _networkInitialized;

        public OverlayWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            
            // Setup overlay window properties
            Title = "MO App Overlay";
            Width = 180;
            Height = 250;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = new SolidColorBrush(Color.FromArgb(200, 30, 30, 30));
            Topmost = true;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            
            // Position on right side of screen
            Left = SystemParameters.WorkArea.Right - Width - 10;
            Top = 100;

            InitializeCounters();
            SetupUI();
            
            _fpsStopwatch.Start();
            
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();

            // Allow dragging
            MouseLeftButtonDown += (s, e) => DragMove();
            
            // Double-click to close and show main window
            MouseLeftButtonUp += (s, e) =>
            {
                if (e.ClickCount >= 2)
                {
                    this.Hide();
                    _mainWindow.ShowMainWindow();
                }
            };
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

            try
            {
                var uploadCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", "_Total");
                var downloadCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", "_Total");
                
                _prevUploadBytes = (long)uploadCounter.NextValue();
                _prevDownloadBytes = (long)downloadCounter.NextValue();
                _prevNetworkTime = DateTime.Now;
                _networkInitialized = true;
            }
            catch { }
        }

        private void SetupUI()
        {
            var mainPanel = new StackPanel 
            { 
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Top
            };

            // Header
            var header = new TextBlock
            {
                Text = "MO App",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            mainPanel.Children.Add(header);

            // FPS
            _fpsText = CreateMetricText("FPS: --", _mainWindow.FpsColor);
            mainPanel.Children.Add(_fpsText);

            // CPU
            _cpuText = CreateMetricText("CPU: --%", _mainWindow.CpuColor);
            mainPanel.Children.Add(_cpuText);

            // GPU
            _gpuText = CreateMetricText("GPU: --%", _mainWindow.GpuColor);
            mainPanel.Children.Add(_gpuText);

            // RAM
            _ramText = CreateMetricText("RAM: --%", _mainWindow.RamColor);
            mainPanel.Children.Add(_ramText);

            // Network
            _networkText = CreateMetricText("↑ -- ↓ --", _mainWindow.NetworkColor);
            mainPanel.Children.Add(_networkText);

            // Hint
            var hint = new TextBlock
            {
                Text = "Double-click to settings",
                FontSize = 9,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            mainPanel.Children.Add(hint);

            Content = mainPanel;
        }

        private TextBlock CreateMetricText(string text, Color color)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = 12,
                FontWeight = FontWeights.Normal,
                Foreground = new SolidColorBrush(color),
                Margin = new Thickness(0, 3, 0, 3),
                FontFamily = new FontFamily("Consolas")
            };
        }

        public void UpdateColors(MainWindow mainWindow)
        {
            _fpsText.Foreground = new SolidColorBrush(mainWindow.FpsColor);
            _cpuText.Foreground = new SolidColorBrush(mainWindow.CpuColor);
            _gpuText.Foreground = new SolidColorBrush(mainWindow.GpuColor);
            _ramText.Foreground = new SolidColorBrush(mainWindow.RamColor);
            _networkText.Foreground = new SolidColorBrush(mainWindow.NetworkColor);
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            UpdateFps();
            UpdateCpu();
            UpdateGpu();
            UpdateRam();
            UpdateNetwork();
        }

        private void UpdateFps()
        {
            _frameCount++;
            
            if (_fpsStopwatch.ElapsedMilliseconds >= 500)
            {
                _currentFps = _frameCount * 1000.0 / _fpsStopwatch.ElapsedMilliseconds;
                _fpsText.Text = $"FPS: {_currentFps:F0}";
                _frameCount = 0;
                _fpsStopwatch.Restart();
            }
        }

        private void UpdateCpu()
        {
            try
            {
                if (_cpuCounter != null)
                {
                    float cpuUsage = _cpuCounter.NextValue();
                    _cpuText.Text = $"CPU: {cpuUsage:F1}%";
                }
            }
            catch { _cpuText.Text = "CPU: N/A"; }
        }

        private void UpdateGpu()
        {
            try
            {
                // Try to get GPU usage from WMI (Windows Management Instrumentation)
                // This works for some GPUs, fallback to N/A if not available
                using var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var load = obj["LoadPercentage"];
                    if (load != null)
                    {
                        uint gpuLoad = Convert.ToUInt32(load);
                        _gpuText.Text = $"GPU: {gpuLoad}%";
                        return;
                    }
                }
                _gpuText.Text = "GPU: N/A";
            }
            catch { _gpuText.Text = "GPU: N/A"; }
        }

        private void UpdateRam()
        {
            try
            {
                if (_ramCounter != null)
                {
                    float availableMB = _ramCounter.NextValue();
                    long totalMB = GetTotalMemoryMB();
                    float usedPercent = ((totalMB - availableMB) / totalMB) * 100;
                    _ramText.Text = $"RAM: {usedPercent:F1}%";
                }
            }
            catch { _ramText.Text = "RAM: N/A"; }
        }

        private long GetTotalMemoryMB()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var total = obj["TotalVisibleMemorySize"];
                    if (total != null)
                    {
                        return Convert.ToInt64(total) / 1024; // Convert KB to MB
                    }
                }
            }
            catch { }
            return 8192; // Default fallback
        }

        private void UpdateNetwork()
        {
            try
            {
                if (!_networkInitialized) return;

                var uploadCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", "_Total");
                var downloadCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", "_Total");

                long currentUpload = (long)uploadCounter.NextValue();
                long currentDownload = (long)downloadCounter.NextValue();

                string uploadStr = FormatSpeed(currentUpload);
                string downloadStr = FormatSpeed(currentDownload);

                _networkText.Text = $"↑ {uploadStr} ↓ {downloadStr}";
            }
            catch { _networkText.Text = "↑ -- ↓ --"; }
        }

        private string FormatSpeed(long bytesPerSec)
        {
            if (bytesPerSec < 1024)
                return $"{bytesPerSec} B/s";
            else if (bytesPerSec < 1024 * 1024)
                return $"{bytesPerSec / 1024.0:F1} KB/s";
            else
                return $"{bytesPerSec / (1024.0 * 1024.0):F2} MB/s";
        }

        protected override void OnClosed(EventArgs e)
        {
            _updateTimer.Stop();
            _fpsStopwatch.Stop();
            base.OnClosed(e);
        }
    }
}
