Feature: Patch

Scenario: Should return not found
	When I PATCH 'api/blogs/99999' with the following:
         """
         [
			{ op: 'replace', path: 'name', value: null }
		 ]
         """
	Then the status should be 404

Scenario: Should inform of invalid patch
	When I PATCH 'api/blogs/99999' with the following:
         """
         [
			{ op: 'replace', path: 'invalidpath', value: null }
		 ]
         """
	Then the status should be 400
	And the JSON at '[0].MemberNames[0]' should be 'Patch invalidpath'

Scenario: Should return validation results
	When I PATCH 'api/blogs/1' with the following:
         """
         [
			{ op: 'replace', path: 'name', value: null }
		 ]
         """
	Then the status should be 400
	And the JSON at '[0].ErrorMessage' should be 'Name is required'

Scenario: Should update
	Given a new blog exists with placeholder blogId and first post placeholder postId
	When I PATCH 'api/blogs/{blogId}' with the following:
         """
		 [
			{ op: 'replace', path: 'PromotedPost', value: { Id: {postId} } }
		 ]
         """
	Then the status should be 200
	And the JSON at 'PromotedPost.Id' should be '{postId}'