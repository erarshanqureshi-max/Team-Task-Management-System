@echo off
title TaskFlow - Starting Full-Stack Application
echo ========================================================
echo        Starting TaskFlow Full-Stack Application
echo ========================================================
echo.

echo [1/2] Starting ASP.NET Core Backend API (Port 5000)...
start "TaskFlow Backend API" cmd /k "cd /d "%~dp0Backend\TaskManagement.API" && dotnet run --urls http://localhost:5000"

timeout /t 3 /nobreak >nul

echo [2/2] Starting React Vite Frontend (Port 5173)...
set "PATH=C:\Users\hp\AppData\Local\Programs\nodejs;%PATH%"
start "TaskFlow React Frontend" cmd /k "cd /d "%~dp0Frontend" && npm run dev"

timeout /t 3 /nobreak >nul

echo.
echo ========================================================
echo  All services started!
echo  - Frontend: http://localhost:5173
echo  - Backend:  http://localhost:5000/swagger
echo ========================================================
echo.

start http://localhost:5173
start http://localhost:5000/swagger
