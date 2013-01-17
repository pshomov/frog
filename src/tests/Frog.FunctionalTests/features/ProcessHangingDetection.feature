Feature: Detection of hanging process
	In order to avoid running around and restarting build agents every time some process hangs
	As an operator
	I want to be know that those processes will be killed after a certain period of inactivity

Scenario: Long running inactive process gets killed
	Given I have a project as "p1"
	And I add a test task "t1" with content "exec ruby -e 'sleep 300'" to project "p1"
	And I have registered project "p1"
	When I am on the status page for project "p1"
	Then The terminal output contains text "hang"
