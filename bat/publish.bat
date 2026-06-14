@echo off
pushd "%~dp0.."

set VERSION=0.3.3
set OUTPUT_DIR=.\publish

echo --- Floundroid Optimized Publish (v%VERSION%) ---

if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR%

:: -p:PublishTrimmed=true : Removes unused .NET code
:: -p:TrimMode=link : Most aggressive trimming
:: -p:EnableCompressionInSingleFile=true : Squeezes the final EXE
dotnet publish Floundroid/Floundroid.fsproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:PublishReadyToRun=true ^
  -p:PublishTrimmed=true ^
  -p:TrimMode=link ^
  -p:EnableCompressionInSingleFile=true ^
  -o %OUTPUT_DIR%

echo.
echo Publish complete! 
popd
pause