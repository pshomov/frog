#!/bin/sh
mono libs/NUnit/nunit-console.exe src/tests/Frog.Domain.Specs/bin/Debug/Frog.Domain.Specs.dll -noshadow
mono libs/mspec/mspec.exe src/tests/Frog.Domain.Specs/bin/Debug/Frog.Domain.Specs.dll

mono libs/NUnit/nunit-console.exe src/tests/Frog.System.Specs/bin/Debug/Frog.System.Specs.dll -noshadow
