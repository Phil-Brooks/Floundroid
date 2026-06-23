@echo off
pushd "%~dp0.."

set OUTPUT_DIR=.\publish

echo --- Floundroid Optimized Publish ---

if exist %OUTPUT_DIR% rd /s /q %OUTPUT_DIR%

:: -p:PublishTrimmed=true : Removes unused .NET code
:: -p:TrimMode=link : Most aggressive trimming
:: -p:EnableCompressionInSingleFile=true : Squeezes the final EXE
dotnet publish src/Floundroid/Floundroid.fsproj ^
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

echo.
echo Running perft suite...

set LOGFILE=%~dp0perft-log.txt
set STARTTIME=%TIME%

(
  echo uci
  echo isready
  echo position startpos
  echo perft suite 5
  echo quit
) | %OUTPUT_DIR%\Floundroid.exe

set ENDTIME=%TIME%

for /f "usebackq" %%t in (`powershell -NoProfile -Command ^
    "( [timespan]::Parse('%ENDTIME%') - [timespan]::Parse('%STARTTIME%') ).ToString()"`) do (
    set ELAPSED=%%t
)

echo %DATE% %TIME% - Perft suite depth 5 completed in %ELAPSED% >> "%LOGFILE%"

echo Perft suite complete in %ELAPSED%

echo.
echo Latest perft result:
for /f "usebackq tokens=* delims=" %%l in (`powershell -NoProfile -Command ^
    "(Get-Content '%LOGFILE%' | Select-Object -Last 2)"`) do echo %%l

popd
pause