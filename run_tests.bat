libs\NUnit\nunit-console.exe src\tests\Frog.Domain.Specs\bin\Debug\Frog.Domain.Specs.dll
@if %errorlevel% neq 0 exit /b %errorlevel%
libs\mspec\mspec.exe src\tests\Frog.Domain.Specs\bin\Debug\Frog.Domain.Specs.dll
@if %errorlevel% neq 0 exit /b %errorlevel%
libs\NUnit\nunit-console.exe src\tests\Frog.System.Specs\bin\Debug\Frog.System.Specs.dll
@if %errorlevel% neq 0 exit /b %errorlevel%
@echo "Build Runz!"