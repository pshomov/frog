﻿Feature: Project registration
	In order to have Runz continously integrate my project
	As anyone on the Internet
	I want to be able to register the project

Background:
	Given I have a project as "p1"
	And I add a test task "t1" with content "testoutput" to project "p1"

Scenario: New project registration
	Given I am on the "registration" page
	And I type in the url input the "p1" repository URL and press the 'Register' button
	Then I get a link to the status page for the project

Scenario: "Check for project updates" builds the project
	Given I have registered project "p1"
	When I check for updates
	And I am on the status page for project "p1"
	Then I see the build is completed with status SUCCESS
	And The terminal output contains text "S>testoutput"
