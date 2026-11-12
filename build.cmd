@echo off
rem Builds ItemReroll.dll twice and copies the mod into the RimWorld Mods folders:
rem   ItemReroll\1.6\Assemblies - RimWorld 1.6 (MSBuild, .NET Framework 4.7.2, against the 1.6 copy W:\SteamLibrary\RimWorld-1.6),
rem   ItemReroll\1.7\Assemblies - RimWorld 1.7 (dotnet, .NET 10, against the Steam folder, branch unstable).
rem LoadFolders.xml picks the right one for the running game.
setlocal
set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set MODS=W:\SteamLibrary\steamapps\common\RimWorld\Mods
set MODS16=W:\SteamLibrary\RimWorld-1.6\Mods

%MSBUILD% "%~dp0Source\ItemReroll\ItemReroll.csproj" -p:Configuration=Release -nologo -v:minimal || exit /b 1
dotnet build "%~dp0Source\ItemReroll.1.7\ItemReroll.1.7.csproj" -c Release -nologo -v:minimal || exit /b 1
call :install "%MODS%" || exit /b 1
if exist "%MODS16%" call :install "%MODS16%" || exit /b 1
exit /b 0

:install
robocopy "%~dp0ItemReroll" "%~1\ItemReroll" /MIR /XF PublishedFileId.txt /NFL /NDL /NJH /NJS /NP /R:3 /W:2
if %ERRORLEVEL% GEQ 8 (
    echo Could not copy the mod to %~1\ItemReroll: close RimWorld, it keeps the dll open.
    exit /b 1
)
echo Installed to %~1\ItemReroll
exit /b 0
