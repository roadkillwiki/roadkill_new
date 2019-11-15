namespace Roadkill.Api.Authorization
{
	public interface IUserRoleDefinition
	{
		string Name { get; }
		bool ContainsClaim(string claimName);
	}
}
