using System.Collections.Generic;

namespace Roadkill.Api.Authorization
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
