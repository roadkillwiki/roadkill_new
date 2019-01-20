using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Identity;

namespace Roadkill.Tests.Unit.Mocks
{
	[SuppressMessage("Stylecop", "CA2227", Justification = "Sshhh")]
	public sealed class MockUserStore<TUser> : IUserStore<TUser>,
										  IUserPasswordStore<TUser>,
										  IUserEmailStore<TUser>,
										  IUserPhoneNumberStore<TUser>,
										  IUserTwoFactorStore<TUser>,
										  IUserAuthenticatorKeyStore<TUser>,
										  IUserTwoFactorRecoveryCodeStore<TUser>,
										  IQueryableUserStore<TUser>,
										  IUserClaimStore<TUser>
										where TUser : IdentityUser, IClaimsUser
	{
		public MockUserStore()
		{
			InternalUsers = new List<TUser>();
		}

		public List<TUser> InternalUsers { get; set; }

		public IQueryable<TUser> Users
		{
			get => InternalUsers.AsQueryable();
			set => InternalUsers = new List<TUser>(value);
		}

		public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.Id);
		}

		public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.UserName);
		}

		public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
		{
			user.UserName = userName;
			return Task.CompletedTask;
		}

		public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.NormalizedUserName);
		}

		public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
		{
			user.NormalizedUserName = normalizedName;
			return Task.CompletedTask;
		}

		public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			InternalUsers.Add(user);
			return Task.FromResult(IdentityResult.Success);
		}

		public Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			int index = InternalUsers.FindIndex(x => x.Id.Equals(user.Id, StringComparison.InvariantCultureIgnoreCase));

			if (index > -1)
			{
				InternalUsers[index] = user;
			}

			return Task.FromResult(IdentityResult.Success);
		}

		public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			int index = InternalUsers.FindIndex(x => x.Id.Equals(user.Id, StringComparison.InvariantCultureIgnoreCase));

			if (index > -1)
			{
				InternalUsers.RemoveAt(index);
			}

			return Task.FromResult(IdentityResult.Success);
		}

		public Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			var user = InternalUsers.FirstOrDefault(x => x.Id.Equals(userId, StringComparison.InvariantCultureIgnoreCase));

			return Task.FromResult(user);
		}

		public Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			var user = InternalUsers.FirstOrDefault(x => x.Id.Equals(normalizedUserName, StringComparison.InvariantCultureIgnoreCase));

			return Task.FromResult(user);
		}

		public void Dispose()
		{
		}

		// IUserPasswordStore
		public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
		{
			user.PasswordHash = passwordHash;
			return Task.CompletedTask;
		}

		public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.PasswordHash);
		}

		public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
		{
			bool hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
			return Task.FromResult(hasPassword);
		}

		// IUserEmailStore
		public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
		{
			user.Email = email;
			return Task.CompletedTask;
		}

		public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.Email);
		}

		public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.EmailConfirmed);
		}

		public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
		{
			user.EmailConfirmed = confirmed;
			return Task.CompletedTask;
		}

		public Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
		{
			var user = InternalUsers.FirstOrDefault(x => x.Email.Equals(normalizedEmail, StringComparison.InvariantCultureIgnoreCase));

			return Task.FromResult(user);
		}

		public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.NormalizedEmail);
		}

		public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
		{
			user.NormalizedEmail = normalizedEmail;
			return Task.CompletedTask;
		}

		// IUserPhoneNumberStore
		public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
		{
			user.PhoneNumber = phoneNumber;
			return Task.CompletedTask;
		}

		public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.PhoneNumber);
		}

		public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.PhoneNumberConfirmed);
		}

		public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
		{
			user.PhoneNumberConfirmed = confirmed;
			return Task.CompletedTask;
		}

		// IUserTwoFactorStore
		public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
		{
			user.TwoFactorEnabled = enabled;
			return Task.CompletedTask;
		}

		public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.TwoFactorEnabled);
		}

		// IUserAuthenticatorKeyStore
		public Task SetAuthenticatorKeyAsync(TUser user, string key, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task<string> GetAuthenticatorKeyAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult("1234123412");
		}

		// IUserTwoFactorRecoveryCodeStore
		public Task ReplaceCodesAsync(TUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task<bool> RedeemCodeAsync(TUser user, string code, CancellationToken cancellationToken)
		{
			return Task.FromResult(false);
		}

		public Task<int> CountCodesAsync(TUser user, CancellationToken cancellationToken)
		{
			return Task.FromResult(5);
		}

		// IUserClaimStore<TUser>
		public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
		{
			var resolvedUser = InternalUsers.FirstOrDefault(x => x.Id.Equals(user.Id, StringComparison.InvariantCultureIgnoreCase));

			var claimsList = new List<Claim>();
			if (resolvedUser.RoleClaims != null)
			{
				foreach (string roleClaim in resolvedUser.RoleClaims)
				{
					claimsList.Add(new Claim(ClaimTypes.Role, roleClaim));
				}
			}

			return Task.FromResult((IList<Claim>)claimsList);
		}

		public async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
		{
			var userRoleClaims = new List<string>();
			foreach (Claim claimItem in claims)
			{
				if (claimItem.Type == ClaimTypes.Role)
				{
					userRoleClaims.Add(claimItem.Value);
				}
			}

			user.RoleClaims = userRoleClaims;

			await UpdateAsync(user, cancellationToken);
		}

		public async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
		{
			if (claim.Type != ClaimTypes.Role || newClaim.Type != ClaimTypes.Role)
			{
				return;
			}

			var existingClaims = await GetClaimsAsync(user, cancellationToken);
			if (existingClaims != null)
			{
				List<Claim> claimsList = existingClaims.ToList();
				int index = claimsList.FindIndex(x => x.Value == claim.Value);
				claimsList.RemoveAt(index);
				claimsList.Add(newClaim);

				await AddClaimsAsync(user, claimsList, cancellationToken);
			}
		}

		public async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
		{
			var existingClaims = await GetClaimsAsync(user, cancellationToken);
			if (existingClaims != null)
			{
				var newClaims = existingClaims.ToList();
				foreach (Claim claimToRemove in claims)
				{
					int index = newClaims.FindIndex(x => x.Type == claimToRemove.Type && x.Value == claimToRemove.Value);
					newClaims.RemoveAt(index);
				}

				await AddClaimsAsync(user, newClaims, cancellationToken);
			}
		}

		public Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
		{
			var usersWithClaim = InternalUsers.Where(x => x.RoleClaims.Contains(claim.Value));
			return Task.FromResult(usersWithClaim as IList<TUser>);
		}
	}
}
