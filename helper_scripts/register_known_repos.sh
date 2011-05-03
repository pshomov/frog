#!/bin/sh

export ACCEPTANCE_TEST_URL=http://testbee1.runzhq.com/runz/
mono libs/NUnit/nunit-console.exe src/tests/Frog.FunctionalTests/bin/Release/Frog.FunctionalTests.dll -run=Frog.FunctionalTests.LazyPetarFeature