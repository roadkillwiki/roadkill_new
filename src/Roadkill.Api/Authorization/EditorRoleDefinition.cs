using System.Collections.Generic;

namespace Roadkill.Api.Authorization
{
	public class EditorRoleDefinition : IUserRoleDefinition
	{
		public const string Name = "editor";
		string IUserRoleDefinition.Name => Name;

		private readonly List<string> _availableClaims = new List<string>()
		{
			PolicyNames.AddPage,
			PolicyNames.UpdatePage
		};

		public bool ContainsClaim(string claimName)
		{
			return _availableClaims.Contains(claimName);
		}
	}
}
