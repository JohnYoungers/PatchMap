Feature: Update

Scenario: Should return not found
	When I PUT 'api/blogs/99999' with the following:
         """
         {}
         """
	Then the status should be 404

Scenario: Should return validation results
	When I PUT 'api/blogs/1' with the following:
         """
         {}
         """
	Then the status should be 400
	And the JSON at '[0].ErrorMessage' should be 'Name is required'

Scenario: Should update
	Given a new blog exists with placeholder blogId and first post placeholder postId
	When I generated a string for placeholder S1
	And I PUT 'api/blogs/{blogId}' with the following:
         """
         {
			Name: 'Updated Blog {S1}',
			Url: 'http://updatedblog.com',
			PromotedPost: { Id: {postId} }
		 }
         """
	Then the status should be 200
	And the JSON at 'Name' should be 'Updated Blog {S1}'