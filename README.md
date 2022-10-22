# AudioUIControlls
Set of control elements which 'out of the box' support MIDI automation


## Other Repos nessessary for builting the dlls:

- Consola *https://github.com/Kalle-Muenster/Consola*
- Int24Types *https://github.com/Kalle-Muenster/Int24Types*
- TaskAssist *https://github.com/Kalle-Muenster/TaskAssist-Motorsport*
- ControlledValues *https://github.com/Kalle-Muenster/ControlledValues*


A small preebuilt, runnable demo can be found here:

https://github.com/Kalle-Muenster/AudioUIControls/tree/main/bin/core5/x64/Debug/net5.0-windows

It can be run at best by calling TestContainer.exe by command line. Without any parameters, it
starts up as free manually testable demo. To let execute an automated testrun which generates result log output, these following parameters can be addded:

<code>
TestContainer --testrun --verbose --xmllogs

<code>
