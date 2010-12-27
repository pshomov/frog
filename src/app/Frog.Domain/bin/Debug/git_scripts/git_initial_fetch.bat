cd %1
if %errorlevel% neq 0 exit /b %errorlevel%

if EXIST %2 RMDIR /S/Q %2
if %errorlevel% neq 0 exit /b %errorlevel%

mkdir %2
if %errorlevel% neq 0 exit /b %errorlevel%

cd %2
if %errorlevel% neq 0 exit /b %errorlevel%

cmd.exe /c git init .
if %errorlevel% neq 0 exit /b %errorlevel%

cmd.exe /c git remote add origin %3
if %errorlevel% neq 0 exit /b %errorlevel%

cmd.exe /c git remote update
if %errorlevel% neq 0 exit /b %errorlevel%

cmd.exe /c git checkout -b master --track origin/master
if %errorlevel% neq 0 exit /b %errorlevel%