Feature: Lazy Petar
	In order to avoid retyping urls to register builds over and over agian
	As a Runz tester
	I want to be setup the whole thing quickly

Scenario: Register the usual suspects
	Given I have registered project URL "http://github.com/pshomov/xray.git"
	And I have registered project URL "http://runzci:sans3r1f@github.com/pshomov/frog.git"
	And I have registered project URL "http://github.com/flq/MemBus.git"
	And I have registered project URL "http://github.com/grahamrhay/NHamcrest.git"
	And I have registered project URL "http://github.com/sinatra/sinatra.git"
