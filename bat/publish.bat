@echo off
:: Move to the solution root (one level up from this script)
pushd "%~dp0.."

set VERSION=0.3.1
set OUTPUT_DIR=.\publish

echo --- Floundroid Publish Script (v%VERSION%) ---
echo Working directory: %CD%

echo Cleaning old builds...
if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR%

echo.
echo Compiling for Windows (Single File, Optimized)...
:: This will look for your .fsproj in the current (root) directory
dotnet publish Floundroid/Floundroid.fsproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -o %OUTPUT_DIR%

echo.
echo Publish complete! 
echo Your executable is in: %CD%\publish\Floundroid.exe

:: Return to the original folder context
popd
pause