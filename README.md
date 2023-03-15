# MetaParser
LightWeight windows application that shows Stable Diffusion metadata stored in images by Automatic1111 webui.
You can drag and drop an image to the MetaParser or start the app with the name of an image file as a command line argument.

![metaparser](https://user-images.githubusercontent.com/35260274/225276612-f551d713-0ffb-4c4d-82ab-3abdcbc46932.gif)


Installation: Unzip and run.

Click on a row -> copy to clipboard.

Escape -> Close the application.

Ctrl+V -> Use the image path from clipboard.

The app remembers the last screen position and size. If something went wrong and the app is off-screen, delete the `window.cfg` file and restart the app.

You can set up colors in the `app.cfg` file. If something went wrong, delete this file. Also in the app.cfg you can set if the multiple instances of the app is allowed to be opened by changing `true` to `false` here: `<SingleInstance>true</SingleInstance>`.

Configure your favourite image viewer like FastStone ImageViewer to run the MetaParser as the external app.

Or you can use AutoHotkey application with script like this to run it from any explorer:
```
^g:: ; ctrl+g
Clipboard =
Send ^c
ClipWait ;waits for the clipboard to have content
Run, D:\Soft\MetaParser\MetaParser.exe "%clipboard%"
```
