Feature: Multiple projects
	In order to make this work
	As a service provider
	I want to handle multiple projects in a completely independent manner

Scenario: Build statuses for multiple projects
	Given I have .NET sample project with 1 testing project as "p1"
	And I have .NET sample project with 2 testing project as "p2"
	And I have .NET sample project with 3 testing project as "p3"
	And I have registered project "p1"
	And I have registered project "p2"
	And I have registered project "p3"
	When I check for updates
	Then I am on the status page for project "p1"
	And I see the build is completed with status SUCCESS
	And The terminal output contains text "S>sample output from task 1 on project p1"
	And I am on the status page for project "p2"
	And I see the build is completed with status SUCCESS
	And The terminal output contains text "S>sample output from task 1 on project p2\nS>sample output from task 2 on project p2"
	And I am on the status page for project "p3"
	And I see the build is completed with status SUCCESS
	And The terminal output contains text "S>sample output from task 1 on project p3\nS>sample output from task 2 on project p3\nS>sample output from task 3 on project p3"
