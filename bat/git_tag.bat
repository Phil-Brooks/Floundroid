@echo off
:: Move to the solution root (one level up from this script)
pushd "%~dp0.."

echo --- Floundroid Git Tagger ---
echo Current Repository: %CD%
echo.

:: Check if git is initialized here
if not exist .git (
    echo ERROR: .git folder not found in %CD%
    popd
    pause
    exit /b
)

set /p VERSION="Enter Version Tag (e.g. v0.3.1): "
set /p COMMENT="Enter Release Comment: "

echo.
echo Creating tag %VERSION%...
git tag -a %VERSION% -m "%COMMENT%"

echo.
echo Pushing tag to GitHub...
git push origin %VERSION%

echo.
echo Done!
:: Return to original folder context
popd
pause