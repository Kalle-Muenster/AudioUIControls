@echo off
set _ProDir_=%~dp0
set _arch_=%~1
set _conf_=%~2

set MOTORSPORT=c:\WORKSPACE\PROJECTS\MotorSports\bin\5.0\%_arch_%\%_conf_%\net5.0

echo copying linkage for "%_arch_%" archtecture into the build's linkage folder...>build.log
cd "%_ProDir_%linkage"
del /f /s /q *.* >>build.log
copy /Y /B "%MOTORSPORT%\*.*" "%_ProDir_%linkage" >>build.log

if "%ERRORLEVEL%" == "0" (
    echo Successfully copied linkage for "%_arch_%" archtecture into build's linkage folder
) else ( echo ERROR! Vorsicht! ... Schlimm! .. ERROR )
set _ProDir_=
set _conf_=
set _arch_=
set MOTORSPORT=
