Feature: Build history
	In order to get a feeling for how my project has built recent;ly
	As a project maintainer
	I want to be able to see the overall build status of the last few builds of my project at a glance

Background:
	Given I have a project as "p1"
	And I add a test task "t1" with content "testoutput" to project "p1"

Scenario: See the last 3 builds at a glance
	Given I have registered project "p1"
	When I check for updates
	And I am on the status page for project "p1"
	And I see the build is completed with status SUCCESS
	And I add a test task "t2" with content "testoutput" to project "p1"
	And I check for updates
	And I am on the status page for project "p1"
	Then I see the build is completed with status SUCCESS
	And I see build history contains 2 items
#	And I see build history item 1 contains 2 items
	 