#!/bin/sh
mono libs/NUnit/nunit-console.exe src/tests/Frog.Domain.Specs/bin/Debug/Frog.Domain.Test.dll
mono libs/mspec/mspec.exe src/tests/Frog.Domain.Specs/bin/Debug/Frog.Domain.Test.dll

mono libs/NUnit/nunit-console.exe src/tests/Frog.System.Specs/bin/Debug/Frog.System.Test.dll
