Feature: Get

Scenario: Should return not found
	When I GET 'api/blogs/99999'
	Then the status should be 404

Scenario: Should return item
	When I GET 'api/blogs/1'
	Then the status should be 200
	And the JSON at 'Name' should be 'Seed Blog'
