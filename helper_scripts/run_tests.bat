libs\NUnit\nunit-console.exe src\tests\Frog.Domain.Specs\bin\Debug\Frog.Domain.Test.dll
@if %errorlevel% neq 0 exit /b %errorlevel%
libs\NUnit\nunit-console.exe src\tests\Frog.System.Specs\bin\Debug\Frog.System.Test.dll
echo "Error level is " %errorlevel%
@if %errorlevel% neq 0 exit /b %errorlevel%
@echo "Build Runz!"
