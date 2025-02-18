echo "Start building Electron.NET dev stack..."

echo "Restore & Build API"
cd ElectronNet.API
dotnet restore
dotnet build
cd ..
echo "Restore & Build CLI"
cd ElectronNet.CLI
dotnet restore
dotnet build
echo "Restore & Build MSBuild"
cd ElectronNet.MSBuild
dotnet restore
dotnet build
cd ..
echo "Restore & Build WebApp Demo"
cd ElectronNet.WebApp
dotnet restore
dotnet build

echo "Invoke electronize build in WebApp Demo"

echo "Install CLI"

dotnet tool uninstall ElectronNET.CLI -g
dotnet tool install ElectronNET.CLI -g

echo "/target xxx (dev-build)"
electronize build /target custom win7-x86;win /dotnet-configuration Debug /electron-arch ia32  /electron-params "--publish never"

echo "/target win (dev-build)"
electronize build /target win /electron-params "--publish never"

echo "/target custom win7-x86;win (dev-build)"

electronize build /target custom win7-x86;win /electron-params "--publish never"

:: Be aware, that for non-electronnet-dev environments the correct 
:: invoke command would be dotnet electronize ...

:: Not supported on Windows Systems, because of SymLinks...
:: echo "/target osx"
::   dotnet electronize build /target osx

:: Linux and Mac is not supported on with this buildAll.cmd test file, because the electron bundler does some strange stuff
:: Help welcome!
:: echo "/target linux (dev-build)"
:: electronize build /target linux /electron-params "--publish never"
