cd %1\%2
if %errorlevel% neq 0 exit /b %errorlevel%

cmd.exe /c git remote update
if %errorlevel% neq 0 exit /b %errorlevel%

cmd.exe /c git log HEAD..origin/master --exit-code
if %errorlevel% == 1 exit /b 201

@echo exit level for last command is %errorlevel%
exit /b %errorlevel%