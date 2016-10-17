setlocal EnableDelayedExpansion

REM variables
set current_dir=%~dp0
set node_dir="%current_dir%..\..\interactivedisplay/InteractiveDisplayNode/"

REM start server
cd %node_dir%
cmd.exe /c tsc server.ts
node server.js
