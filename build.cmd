dotnet restore --nologo
dotnet publish .\src\BehideServer\ -c Release -o .\bin\BehideServer\ --nologo --no-restore --sc
dotnet publish .\src\BehideServer.Interop\ -c Release -o .\bin\BehideServer.Interop\ --nologo --no-restore