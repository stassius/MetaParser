# MetaParser
LightWeight windows application that shows Stable Diffusion metadata in images.
You can drag and drop an image to the MetaParser or start it with the name of an image file as a command line argument.

![image](https://user-images.githubusercontent.com/35260274/225069254-02c4fc19-3b08-431b-b7e6-6ea7d006790e.png)

### Click on a row -> copy to clipboard.

### Escape -> Close the application.

The app remembers the last screen position and size. If something went wrong and the app is off-screen, delete the `window.cfg` file.

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
