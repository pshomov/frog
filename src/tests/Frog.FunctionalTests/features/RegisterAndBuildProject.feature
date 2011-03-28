Feature: Project registration
	In order to have Runz continously integrate my project
	As anyone on the Internet
	I want to be able to register the project

Background:
	Given I have a .NET simple project

Scenario: New project registration
	Given I am on the "registration" page
	And I type in the url input the project's repository URL and press the 'Register' button
	Then I get a link to the status page for the project

Scenario: "Check for project updates" builds the project
	Given I have registered the project
	When I check for updates
	And I am on the status page for the project
	Then I see the build is completed
	And The terminal output contains text from the build 
