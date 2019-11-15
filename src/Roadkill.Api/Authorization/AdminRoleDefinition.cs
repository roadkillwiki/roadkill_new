namespace Roadkill.Api.Authorization
{
	public class AdminRoleDefinition : IUserRoleDefinition
	{
		public const string Name = "admin";
		string IUserRoleDefinition.Name => Name;

		public bool ContainsClaim(string claimName)
		{
			return true;
		}
	}
}
