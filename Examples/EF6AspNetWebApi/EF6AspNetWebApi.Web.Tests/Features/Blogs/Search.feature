Feature: Search

Scenario: Should return list
	When I GET 'api/blogs?$filter=BlogId eq 1'
	Then the status should be 200
	And the JSON at '[0].Name' should be 'Seed Blog'
