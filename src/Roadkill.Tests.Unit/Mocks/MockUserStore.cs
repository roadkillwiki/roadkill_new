using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Roadkill.Core.Authorization;

namespace Roadkill.Tests.Unit.Mocks
{
	[SuppressMessage("Stylecop", "CA1063", Justification = "IDisposable overkill")]
	[SuppressMessage("Stylecop", "CA1001", Justification = "IDisposable overkill")]
	public class MockUserStore : IQueryableUserStore<RoadkillUser>
	{
		private List<RoadkillUser> _users;

		public MockUserStore()
		{
			_users = new List<RoadkillUser>();
		}

		public IQueryable<RoadkillUser> Users
		{
			get => _users.AsQueryable();
			set => _users = new List<RoadkillUser>(value);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public Task<string> GetUserIdAsync(RoadkillUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetUserNameAsync(RoadkillUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task SetUserNameAsync(RoadkillUser user, string userName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<string> GetNormalizedUserNameAsync(RoadkillUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task SetNormalizedUserNameAsync(RoadkillUser user, string normalizedName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityResult> CreateAsync(RoadkillUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityResult> UpdateAsync(RoadkillUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IdentityResult> DeleteAsync(RoadkillUser user, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<RoadkillUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<RoadkillUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
