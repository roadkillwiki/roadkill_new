namespace Roadkill.Api.Authorization.Roles
{
	public class AdminRoleDefinition : IUserRoleDefinition
	{
		public const string Name = "admin";
		string IUserRoleDefinition.Name => Name;

		public bool ContainsPolicy(string policyName)
		{
			return true;
		}
	}
}
