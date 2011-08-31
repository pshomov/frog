#!/bin/bash

export RUNZ_ACCEPTANCE_MODE=ACCEPTANCE
LOCATION=`dirname "$0"`
MONO_IOMAP=case MONO_OPTIONS=--debug mono "$LOCATION/../output/agent/Frog.Agent.exe" &
MONO_IOMAP=case MONO_OPTIONS=--debug mono  "$LOCATION/../output/repotracker/Frog.RepositoryTracker.exe" &
MONO_IOMAP=case MONO_OPTIONS=--debug xsp4  --applications /:$LOCATION/../src/app/Frog.UI.Web --port 6502
