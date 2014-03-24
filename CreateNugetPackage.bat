

RMDIR %CD%\Build /s /q

IF NOT EXIST %CD%\Build GOTO NODIR
   
:NODIR
MKDIR %CD%\Build

MKDIR %CD%\Build\content
  
COPY %CD%\SignalRAppender\SignalRAppender.cs %CD%\Build\content\SignalRAppender.cs
COPY %CD%\SignalRAppender\SignalRAppender.nuspec %CD%\Build\SignalRAppender.nuspec

"%CD%\.nuget\nuget.exe" pack %CD%\Build\SignalRAppender.nuspec