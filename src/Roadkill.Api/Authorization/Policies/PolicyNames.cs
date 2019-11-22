using System.Collections.Generic;

namespace Roadkill.Api.Authorization.Policies
{
	public static class PolicyNames
	{
		// This isn't an enum as the AuthorizeAttribute needs constant strings for its "Policy=" property

		public const string FindUsers = nameof(FindUsers);
		public const string GetUser = nameof(GetUser);
		public const string CreateEditorUser = nameof(CreateEditorUser);
		public const string CreateAdminUser = nameof(CreateAdminUser);
		public const string DeleteUser = nameof(DeleteUser);

		public const string AddPage = nameof(AddPage);
		public const string UpdatePage = nameof(UpdatePage);
		public const string DeletePage = nameof(DeletePage);

		public const string AddPageVersion = nameof(AddPageVersion);
		public const string UpdatePageVersion = nameof(UpdatePageVersion);
		public const string DeletePageVersion = nameof(DeletePageVersion);

		public const string RenameTag = nameof(RenameTag);

		public const string MarkdownUpdateLinks = nameof(MarkdownUpdateLinks);

		public static IEnumerable<string> AllPolicies => new List<string>()
		{
			FindUsers,
			GetUser,
			CreateEditorUser,
			CreateAdminUser,
			DeleteUser,

			AddPage,
			UpdatePage,
			DeletePage,

			AddPageVersion,
			UpdatePageVersion,
			DeletePageVersion,

			RenameTag,

			MarkdownUpdateLinks
		};
	}
}
