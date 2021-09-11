# UnityLogFileReader
Simple windows app to make reading Unity *.log files easier on your eyes and brains.

> Note: there are plenty of edge cases not properly addressed here so don't expect to replace your text editors just yet.

## Features:
- Collapses repeating messages.
- Continuously updated file view.
- Single mouse-click copies the given message.
- Identical messages share color value.
- File history.

## See for yourself:

Notepad <--> this
 ![preview](https://i.imgur.com/v9tCo9N.jpg)

## Installation:
- Download [binary files](https://github.com/andrew-raphael-lukasik/UnityLogFileReader/releases) or compile project yourself.
- Open `*.log` file with this apllication. Program uses `CommandLineArgs` so uou can do that by:
  -  RMB over a log file->`OpenWith`->select this app's executable
  -  dropping a `*.log` file over app's executable.
