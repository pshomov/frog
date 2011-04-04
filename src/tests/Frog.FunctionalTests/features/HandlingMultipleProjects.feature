Feature: Multiple projects
	In order to make this work
	As a service provider
	I want to handle multiple projects in a completely independent manner

Scenario: Build statuses for multiple successful projects
	Given I have a project as "p1"
	And I add a test task "t1" with content "p1t1" to project "p1"
	And I have a project as "p2"
	And I add a test task "t1" with content "p2t1" to project "p2"
	And I add a test task "t2" with content "p2t2" to project "p2"
	And I have a project as "p3"
	And I add a test task "t1" with content "p3t1" to project "p3"
	And I add a test task "t2" with content "p3t2" to project "p3"
	And I add a test task "t3" with content "p3t3" to project "p3"
	And I have registered project "p1"
	And I have registered project "p2"
	And I have registered project "p3"
	When I check for updates
	Then I am on the status page for project "p1"
	And I see the build is completed with status SUCCESS
	And The terminal output contains text "S>p1t1"
	And I am on the status page for project "p2"
	And I see the build is completed with status SUCCESS
	And The terminal output contains text "S>p2t1\nS>p2t2"
	And I am on the status page for project "p3"
	And I see the build is completed with status SUCCESS
	And The terminal output contains text "S>p3t1\nS>p3t2\nS>p3t3"

Scenario: Build statuses for multiple successful and one failing projects
	Given I have a project as "p1"
	And I add a test task "t1" with content "p1t1" to project "p1"
	And I have a project as "p2"
	And I add a test task "t1" with content "p2t1" to project "p2"
	And I add a test task "t2" with content "exception" to project "p2"
	And I have a project as "p3"
	And I add a test task "t1" with content "p3t1" to project "p3"
	And I add a test task "t2" with content "p3t2" to project "p3"
	And I add a test task "t3" with content "p3t3" to project "p3"
	And I have registered project "p1"
	And I have registered project "p2"
	And I have registered project "p3"
	When I check for updates
	Then I am on the status page for project "p1"
	And I see the build is completed with status SUCCESS
	And The terminal output has text "S>p1t1"
	And I am on the status page for project "p2"
	And I see the build is completed with status FAILURE
	And The terminal output contains text "S>p2t1\nS>exception"
	And I am on the status page for project "p3"
	And I see the build is completed with status SUCCESS
	And The terminal output has text "S>p3t1\nS>p3t2\nS>p3t3"
