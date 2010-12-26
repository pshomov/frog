cd %1\%2

cmd.exe /c git remote update
cmd.exe /c git log HEAD..origin/master --exit-code
@echo exit level for last command is %ERRORLEVEL% 
exit /b %ERRORLEVEL%