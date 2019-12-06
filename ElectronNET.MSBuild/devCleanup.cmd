taskkill /F /IM msbuild.exe
rd /s /q %userprofile%\.nuget\packages\electronnet.msbuild 2>nul
