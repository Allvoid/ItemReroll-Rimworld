@echo off
rem Builds ItemReroll.dll into ItemReroll\Assemblies and copies the mod into the RimWorld Mods folder.
setlocal
set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set MODS=W:\SteamLibrary\steamapps\common\RimWorld\Mods

%MSBUILD% "%~dp0Source\ItemReroll\ItemReroll.csproj" -p:Configuration=Release -nologo -v:minimal || exit /b 1
robocopy "%~dp0ItemReroll" "%MODS%\ItemReroll" /MIR /NFL /NDL /NJH /NJS /NP
if %ERRORLEVEL% GEQ 8 exit /b 1
echo Installed to %MODS%\ItemReroll
exit /b 0
