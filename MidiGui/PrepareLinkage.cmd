@echo off
set _ProDir_=%~dp0
set _arch_=%~1
set _conf_=%~2

set MIDICONTROLS=c:\WORKSPACE\PROJECTS\MidiGui\SliderControl\linkage

echo copying linkage for "%_arch_%" archtecture into the build's linkage folder...>build.log
copy /Y /B "%MIDICONTROLS%\*.*" "%_ProDir_%linkage" >>build.log

if "%ERRORLEVEL%" == "0" (
    echo Successfully copied linkage for "%_arch_%" archtecture into build's linkage folder
) else ( echo ERROR! Vorsicht! ... Schlimm! .. ERROR )
set _ProDir_=
set _conf_=
set _arch_=
set MIDICONTROLS=
