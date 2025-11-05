using System;
using System.Collections.Generic;

namespace Checklist.Models.Dtos
{
	public class UserUpdateDto
	{
		public string FullName { get; set; } = string.Empty;
		public string Password { get; set; }  // optional
		public string Role { get; set; } = "User";
		public List<Guid>? ProjectIds { get; set; }  // the projects the user should have access to
	}
}
