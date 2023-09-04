@echo off
Set SignFile="E:\Coding\Tools\Sign wh7r4bb17\SignFiles_w.exe"

Set Assembly=wh7r4bb17.NextcloudHelper.dll

Set Debug_Assembly="E:\Coding\Projects\wh7r4bb17\NextcloudHelper\Debug\net48\%Assembly%"
Set Release_Assembly="E:\Coding\Projects\Releases\NextcloudHelper\net48\%Assembly%"
if exist %Debug_Assembly% %SignFile% %Debug_Assembly%
if exist %Release_Assembly% %SignFile% %Release_Assembly%


Set Debug_Assembly481="E:\Coding\Projects\wh7r4bb17\NextcloudHelper\Debug\net481\%Assembly%"
Set Release_Assembly481="E:\Coding\Projects\Releases\NextcloudHelper\net481\%Assembly%"
if exist %Debug_Assembly481% %SignFile% %Debug_Assembly481%
if exist %Release_Assembly481% %SignFile% %Release_Assembly481%


Set Debug_Assembly20="E:\Coding\Projects\wh7r4bb17\NextcloudHelper\Debug\netstandard2.0\%Assembly%"
Set Release_Assembly20="E:\Coding\Projects\Releases\NextcloudHelper\netstandard2.0\%Assembly%"
if exist %Debug_Assembly20% %SignFile% %Debug_Assembly20%
if exist %Release_Assembly20% %SignFile% %Release_Assembly20%