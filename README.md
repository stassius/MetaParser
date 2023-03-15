# MetaParser
Lightweight windows application that shows Stable Diffusion metadata stored in images by Automatic1111 webui.
You can drag and drop an image to the MetaParser or start the app with the name of an image file as a command line argument.

![metaparser](https://user-images.githubusercontent.com/35260274/225276612-f551d713-0ffb-4c4d-82ab-3abdcbc46932.gif)


### Installation: 
1. Download archive from releases: https://github.com/stassius/MetaParser/releases/tag/Release
2. Unzip and run
3. Allow it to install .NET package if needed
4. Configure the MetaParser.exe as an external editor in your favorite image viewer

Alternatively you can use AutoHotkey (https://www.autohotkey.com/) application to run it from any explorer. Set up the hotkey in the first line and the path to the executable in the last line in this Autohotkey script:
```
^g:: ; ctrl+g
Clipboard =
Send ^c
ClipWait ;waits for the clipboard to have content
Run, D:\Soft\MetaParser\MetaParser.exe "%clipboard%"
```

### Instruction:
Click on a row -> copy to clipboard.

Escape -> Close the application.

Ctrl+V -> Use the image path from clipboard.


The app remembers the last screen position and size. If something went wrong and the app is off-screen, delete the `window.cfg` file and restart the app.

You can set up colors in the `app.cfg` file. If something went wrong, delete this file. Also in the app.cfg you can set if the multiple instances of the app is allowed to be opened by changing `true` to `false` here: `<SingleInstance>true</SingleInstance>`.
