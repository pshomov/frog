Feature: Multiple projects
	In order to make this work
	As a service provider
	I want to handle multiple projects in a completely independent manner

Scenario: Build statuses for multiple projects
	Given I have .NET sample project with 1 unit testing project as "p1"
	And I have .NET sample project with 2 unit testing project as "p2"
	And I have .NET sample project with 0 unit testing project as "p3"
	And I have registered project "p1"
	And I have registered project "p2"
	And I have registered project "p3"
	And I check for updates
