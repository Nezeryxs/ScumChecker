@echo off
chcp 65001 >nul
title ScumChecker • Git Update

echo ===============================
echo  ScumChecker - Git updater
echo ===============================
echo.

REM Переход в папку проекта
cd /d "%~dp0"

REM Проверка git
git --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Git not found. Install Git first.
    pause
    exit /b
)

REM Показываем статус
echo [INFO] Current status:
git status
echo.

REM Добавляем все файлы
echo [INFO] Adding changes...
git add .

REM Сообщение коммита
set msg=Update
if not "%~1"=="" set msg=%~1

echo [INFO] Commit message: "%msg%"
git commit -m "%msg%"
if errorlevel 1 (
    echo [INFO] Nothing to commit.
)

REM Отправка в репозиторий
echo [INFO] Pushing to GitHub...
git push

echo.
echo [DONE] Repository updated.
pause
