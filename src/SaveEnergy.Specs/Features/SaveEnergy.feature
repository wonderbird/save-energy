Feature: Save Energy

	Reconfigure GitHub actions for your projects to save energy.
	
	@NonParallelizable
	Scenario: List GitHub repositories
		Given device flow is enabled for the GitHub app
		When I run the application
		Then at least one repository URL is printed to the console
