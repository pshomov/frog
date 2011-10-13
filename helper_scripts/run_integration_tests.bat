libs\NUnit\nunit-console.exe src\tests\Frog.Domain.IntegrationTests\bin\Debug\Frog.Domain.IntegrationTests.dll %*
@if %errorlevel% neq 0 exit /b %errorlevel%
@echo "Build Runz!"
