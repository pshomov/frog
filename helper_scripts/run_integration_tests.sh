#!/bin/sh
MODE=Release
mono libs/NUnit/nunit-console.exe src/tests/Frog.Domain.IntegrationTests/bin/$MODE/Frog.Domain.IntegrationTests.dll
