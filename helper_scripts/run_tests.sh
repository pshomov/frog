#!/bin/sh
MODE=Release
mono libs/NUnit/nunit-console.exe src/tests/Frog.Domain.Specs/bin/$MODE/Frog.Domain.Test.dll
mono libs/NUnit/nunit-console.exe src/tests/Frog.System.Specs/bin/$MODE/Frog.System.Test.dll
