@echo off
set _ProDir_=%~dp0
set _arch_=%~1
set _conf_=%~2

set CONSOLADLL=c:\WORKSPACE\PROJECTS\GITSPACE\Consola\bin\v4.8
set MOTORSPORT=c:\WORKSPACE\PROJECTS\MotorSports\TaskAssist\bin\4.8

echo copying linkage for "%_arch_%" archtecture into the build's linkage folder...>build.log
cd "%_ProDir_%linkage"
del /f /s /q *.* >>build.log
copy /Y /B "%CONSOLADLL%\%_arch_%\%_conf_%\*.dll" "%_ProDir_%linkage" >>build.log
copy /Y /B "%MOTORSPORT%\%_arch_%\%_conf_%\*.*" "%_ProDir_%linkage" >>build.log

if "%ERRORLEVEL%" == "0" (
    echo Successfully copied linkage for "%_arch_%" archtecture into build's linkage folder
) else ( echo ERROR! Vorsicht! ... Schlimm! .. ERROR )
set _ProDir_=
set _conf_=
set _arch_=
set CONTROLLER=
set CONSOLADLL=
