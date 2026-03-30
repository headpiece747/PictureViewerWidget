# PictureViewerWidget

PictureViewerWidget is a widget designed for the WigiDash Widget Framework. It provides a simple and customizable interface to display a slideshow of images from a selected folder on your WigiDash screen, advancing to the next image whenever you click it.

## Features

* **Folder Selection:** Easily choose any local folder on your PC containing your .jpg, .png, .bmp, or .gif images.
* **Click-to-Advance:** Simply click the widget on your WigiDash screen to instantly advance to the next picture in the folder
* **Natural Sorting:** Feads and sorts filenames exactly like Windows File Explorer does (e.g., automatically understanding that image_2 comes before image_10).
* **Auto-Scaling:** Automatically scales your images to fit the widget dimensions without stretching.

## Project Structure

The project is built in C# and includes the following key files:

* `PictureViewerWidgetObject.cs`: Defines the widget's metadata such as name, author, description, and supported sizes. It also handles the generation or loading of the preview image for the widget gallery.
* `PictureViewerWidgetInstance.cs`: Contains the main logic for the widget instance. It reads the image files from the directory, handles the click-to-advance logic, and manages the visual rendering of the widget and custom overlays.
* `PictureViewerWidgetSettings.xaml & PictureViewerWidgetSettings.xaml.cs`: Defines the WPF user interface and logic for the widget's settings menu. It integrates with WigiDash's HandyControl components for a seamless, native design and automatically saves settings in the background.

## How It Works

The widget reads the directory path set by the user and loads all compatible image files into a list, sorting them using a Windows-native logical string comparer. It then renders the current image onto a bitmap alongside any user-defined text or background colors. When the user clicks the widget, it increments an internal index, grabs the next image from the list, and broadcasts the updated bitmap back to the WigiDash Manager.

## Pre-requisites

- Visual Studio 2022
- WigiDash Manager (https://wigidash.com/)

## Getting started

1. Clone this repository
2. Open PictureViewerWidget.csproj in Visual Studio
3. Resolve the dependancy for WigiDashWidgetFramework under References by adding a reference to 
```
C:\Program Files (x86)\G.SKILL\WigiDash Manager\WigiDashWidgetFramework.dll
```
4. Open Project properties -> Build Events and add this to Post-build event command line:
```
rd /s /q "%AppData%\G.SKILL\WigiDashManager\Widgets\$(TargetName)\"
xcopy "$(TargetDir)\" "%AppData%\G.SKILL\WigiDashManager\Widgets\$(TargetName)\" /F /Y /E /H /C /I
```
5. Open Project properties -> Debug and select Start external program: "C:\Program Files (x86)\G.SKILL\WigiDash Manager\WigiDashManager.exe".
6. Start debugging the project, and it should launch WigiDash Manager with your Widget loaded and debuggable.