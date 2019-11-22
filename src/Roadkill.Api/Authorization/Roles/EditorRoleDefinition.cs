using System.Collections.Generic;
using Roadkill.Api.Authorization.Policies;

namespace Roadkill.Api.Authorization.Roles
{
	public class EditorRoleDefinition : IUserRoleDefinition
	{
		public const string Name = "editor";
		string IUserRoleDefinition.Name => Name;

		private readonly List<string> _availablePolicies = new List<string>()
		{
			PolicyNames.AddPage,
			PolicyNames.UpdatePage
		};

		public bool ContainsPolicy(string policyName)
		{
			return _availablePolicies.Contains(policyName);
		}
	}
}
