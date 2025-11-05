using System;
using System.Collections.Generic;

namespace Checklist.Models.Dtos
{
	public class UserDetailsDto
	{
		public Guid Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;

		public List<ProjectDto> Projects { get; set; } = new();
	}

	public class ProjectDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
