libs\NUnit\nunit-console.exe src\tests\Frog.FunctionalTests\bin\Debug\Frog.FunctionalTests.dll
@if %errorlevel% neq 0 exit /b %errorlevel%
@echo "Passed!"
