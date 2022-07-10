@echo off
set _current_=%CD%
cd /d %~dp0
copy /B /Y ".\..\..\ControlledValues\bin\core5\%~1\%~2\Ijwhost.dll" "%CD%"
