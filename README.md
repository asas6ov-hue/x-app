# MO App - Real-Time System Metrics Overlay

A lightweight C# WPF application that displays real-time system metrics in a compact, transparent overlay window.

## Features

- **Real-time monitoring** of:
  - FPS (Frames Per Second)
  - CPU usage percentage
  - GPU usage percentage
  - RAM usage percentage
  - Network upload/download speeds with up/down arrows

- **Customizable colors** for each metric in settings
- **Compact vertical overlay** that sits on the side of your screen
- **Transparent background** for minimal visual intrusion
- **Low resource usage**

## Requirements

- Windows 10/11
- .NET 8.0 SDK

## Building

```bash
cd MOApp
dotnet build
```

## Running

```bash
dotnet run --project MOApp.csproj
```

Or after building:
```bash
./bin/Debug/net8.0-windows/MOApp.exe
```

## Usage

1. Launch the application to open the Settings window
2. Customize font colors for each metric using the dropdown menus
3. Click "Show Overlay" to hide the main window and display the compact overlay
4. The overlay appears on the right side of your screen
5. Double-click the overlay to return to settings
6. Click "Exit" to close the application

## Project Structure

```
MOApp/
├── App.xaml              # Application definition
├── App.xaml.cs           # Application startup logic
├── MainWindow.xaml       # Main settings window UI
├── MainWindow.xaml.cs    # Settings window logic with color pickers
├── OverlayWindow.xaml    # Overlay window UI definition
├── OverlayWindow.xaml.cs # Overlay rendering and metrics collection
├── MOApp.csproj          # Project file
└── app.manifest          # Application manifest
```