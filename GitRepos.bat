@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion
title ScumChecker • Git Update

echo ===============================
echo  ScumChecker - Git updater
echo ===============================
echo.

REM === Start in the folder where this .bat is located
cd /d "%~dp0"

REM === Find repo root (folder that contains .git)
:findrepo
if exist ".git" goto repofound
cd ..
if "%cd%"=="%SystemDrive%\" goto norepo
goto findrepo

:repofound
echo [INFO] Repo root: %cd%
echo.

REM === Check git
git --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Git not found. Install Git first.
    pause
    exit /b 1
)

REM === Show status
echo [INFO] Current status:
git status
echo.

REM === Pull latest changes (rebase to avoid merge commits)
echo [INFO] Pulling latest changes (rebase)...
git pull --rebase
if errorlevel 1 (
    echo [ERROR] Pull failed. If you see conflicts:
    echo         1) Resolve conflicts in files
    echo         2) Run: git add .
    echo         3) Run: git rebase --continue
    echo         Or cancel: git rebase --abort
    pause
    exit /b 1
)
echo.

REM === Check if there are any changes to commit
git diff --quiet
set "hasWorkTreeChanges=%errorlevel%"
git diff --cached --quiet
set "hasStagedChanges=%errorlevel%"

if "%hasWorkTreeChanges%"=="0" if "%hasStagedChanges%"=="0" (
    echo [INFO] No local changes detected.
    echo [INFO] Pushing anyway (in case you only pulled)...
    git push
    if errorlevel 1 (
        echo [ERROR] Push failed.
        pause
        exit /b 1
    )
    echo.
    echo [DONE] Repository updated.
    pause
    exit /b 0
)

REM === Stage everything
echo [INFO] Adding changes...
git add .

REM === Commit message
set "msg=Update"
if not "%~1"=="" set "msg=%~1"

REM === Commit only if there is something staged
git diff --cached --quiet
if "%errorlevel%"=="0" (
    echo [INFO] Nothing to commit (no staged changes).
) else (
    echo [INFO] Commit message: "%msg%"
    git commit -m "%msg%"
    if errorlevel 1 (
        echo [ERROR] Commit failed.
        pause
        exit /b 1
    )
)

REM === Push
echo [INFO] Pushing to GitHub...
git push
if errorlevel 1 (
    echo [ERROR] Push failed. Try manually:
    echo         git status
    echo         git pull --rebase
    echo         git push
    pause
    exit /b 1
)

echo.
echo [DONE] Repository updated.
pause
exit /b 0

:norepo
echo [ERROR] .git not found in this folder or any parent folders.
echo         Put this .bat inside your repository.
pause
exit /b 1

pause
