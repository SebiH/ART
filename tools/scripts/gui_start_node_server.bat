setlocal EnableDelayedExpansion

REM variables
set current_dir=%~dp0
set node_dir="%current_dir%..\..\interactivedisplay\"

REM start server
cd %node_dir%
start.sh
