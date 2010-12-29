%~d1
if %errorlevel% neq 0 exit /b %errorlevel%

cd %1\%2
if %errorlevel% neq 0 exit /b %errorlevel%

cmd.exe /c git remote update
if %errorlevel% neq 0 exit /b %errorlevel%

cmd.exe /c git log HEAD..origin/master --exit-code
if %errorlevel% == 1 (
cmd.exe /c git merge origin/master
if %errorlevel% neq 1 exit /b 150
exit /b 201
)

@echo exit level for last command is %errorlevel%
exit /b %errorlevel%