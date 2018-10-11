Feature: Swagger

Scenario: Should generate
	When I GET 'swagger/api/v1'
	Then the status should be 200
	And the response body should contain '"swagger": "2.0"'

Scenario: Should populate summaries
	When I GET 'swagger/api/v1'
	Then the JSON at 'paths['/api/blogs'].get.summary' should be 'Search'

Scenario: Should expand odata query parameters
	When I GET 'swagger/api/v1'
	Then the JSON at 'paths['/api/blogs'].get.parameters[0].description' should be 'Filters the results based on an OData formatted condition.'

Scenario: Should expand PatchCommandResult operations
	When I GET 'swagger/api/v1'
	Then the JSON at 'paths['/api/blogs'].post.responses['400'].description' should be 'Validation Failed'
	And the JSON at 'paths['/api/blogs'].post.responses['400'].schema.type' should be 'array'
	And the JSON at 'paths['/api/blogs'].post.responses['400'].schema.items.$ref' should be '#/definitions/ValidationResult'
	And the JSON at 'paths['/api/blogs'].post.responses['201'].description' should be 'Inserted'
	And the JSON at 'paths['/api/blogs'].post.responses['201'].schema.$ref' should be '#/definitions/BlogViewModel'
	And the JSON at 'paths['/api/blogs/{id}'].put.responses['200'].description' should be 'Updated'