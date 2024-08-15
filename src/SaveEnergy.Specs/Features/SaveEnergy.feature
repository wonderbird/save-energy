Feature: Save Energy

Reconfigure GitHub actions for your projects to save energy.

@WorkInProgress
Scenario: List GitHub repositories
	Given The application is authorized to read the user's repositories
	When I run the application
	Then At least one repository URL is printed to the console
