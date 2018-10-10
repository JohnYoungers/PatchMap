Feature: ResponseHeaders

Scenario: Should respond as json and include access control
	When I GET 'api/blogs/1'
	Then the content type should be 'application/json'
	And the header 'Access-Control-Expose-Headers' should be 'Content-Type, Content-Disposition, X-ODATA-count, X-API-version'

Scenario: Should include ODATA count header
	When I GET 'api/blogs?$filter=BlogId eq 1&$count=true'
	Then the header 'X-ODATA-count' should be '1'