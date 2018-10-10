Feature: Insert

Scenario: Should return validation results
	When I POST 'api/blogs' with the following:
         """
         {}
         """
	Then the status should be 400
	And the JSON at '[0].ErrorMessage' should be 'Name is required'

Scenario: Should insert
	When I generated a string for placeholder S1
	And I POST 'api/blogs' with the following:
         """
         {
			Name: 'Blog {S1}',
			Url: 'http://{S1}.com'
		 }
         """
	Then the status should be 201
	And the location header should be standard based on field 'Id'
	And the JSON at 'Name' should be 'Blog {S1}'