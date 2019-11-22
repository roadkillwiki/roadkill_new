namespace Roadkill.Api.Authorization.Roles
{
	public interface IUserRoleDefinition
	{
		string Name { get; }
		bool ContainsPolicy(string policyName);
	}
}
