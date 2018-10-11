Feature: RequestHandling

Scenario: Static Content should not be cached
	When I GET 'wwwroot/index.html'
	Then the header 'Cache-Control' should be 'no-cache'

Scenario: Random files should return index.html
	When I GET 'asdfasdfasdfasdf'
	Then the status should be 200
	And the response body should contain '<title>Entity Framework 6 + Asp.net WebApi Example</title>'

Scenario: Incorrect API calls should result in a 404
	When I GET 'api/asdfsafasdfsdaf'
	Then the status should be 404