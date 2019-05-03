using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Marten;
using Roadkill.Core.Entities;
using Roadkill.Core.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Roadkill.Tests.Integration.Core.Repositories
{
	public class UserRepositoryTests
	{
		private readonly Fixture _fixture;
		private readonly ITestOutputHelper _outputHelper;

		public UserRepositoryTests(ITestOutputHelper outputHelper)
		{
			_fixture = new Fixture();
			_outputHelper = outputHelper;
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(UserRepositoryTests), outputHelper);

			try
			{
				new UserRepository(documentStore).Wipe();
			}
			catch (Exception e)
			{
				outputHelper.WriteLine(GetType().Name + " caught: " + e.Message);
			}
		}

		public UserRepository CreateRepository()
		{
			IDocumentStore documentStore = DocumentStoreManager.GetMartenDocumentStore(typeof(UserRepositoryTests), _outputHelper);
			return new UserRepository(documentStore);
		}

		[Fact]
		public async void DeleteAllUsers_should_clear_every_user()
		{
			// given
			User adminUser = _fixture.Build<User>()
									 .With(x => x.IsAdmin, true)
									 .With(x => x.IsEditor, false)
									 .Create();

			User editorUser = _fixture.Build<User>()
									  .With(x => x.IsAdmin, false)
									  .With(x => x.IsEditor, true)
									  .Create();

			UserRepository repository = CreateRepository();
			await repository.SaveOrUpdateUserAsync(adminUser);
			await repository.SaveOrUpdateUserAsync(editorUser);

			IEnumerable<User> admins = await repository.FindAllAdminsAsync();
			Assert.NotEmpty(admins);

			IEnumerable<User> editors = await repository.FindAllEditorsAsync();
			Assert.NotEmpty(editors);

			// when
			await repository.DeleteAllUsersAsync();

			// then
			admins = await repository.FindAllAdminsAsync();
			editors = await repository.FindAllEditorsAsync();

			admins.ShouldBeEmpty();
			editors.ShouldBeEmpty();
		}

		[Fact]
		public async void DeleteUser_should_delete_single_user()
		{
			// given
			UserRepository repository = CreateRepository();
			List<User> remainingUsers = _fixture.CreateMany<User>().ToList();
			remainingUsers.ForEach(async u =>
			{
				await repository.SaveOrUpdateUserAsync(u);
			});

			User actualUser = _fixture.Build<User>()
									  .With(x => x.IsAdmin, false)
									  .With(x => x.IsEditor, true)
									  .Create();

			await repository.SaveOrUpdateUserAsync(actualUser);

			// when
			await repository.DeleteUserAsync(actualUser);

			// then
			User deletedUser = await repository.GetUserByIdAsync(actualUser.Id);
			deletedUser.ShouldBeNull();

			Guid userId = remainingUsers.First().Id;
			User firstRemainingUser = await repository.GetUserByIdAsync(userId);
			firstRemainingUser.ShouldNotBeNull();
		}

		[Fact]
		public async void FindAllEditors_should_return_editors_only()
		{
			// given
			List<User> editorUsers = _fixture.Build<User>()
													.With(x => x.IsAdmin, false)
													.With(x => x.IsEditor, true)
													.CreateMany()
													.ToList();

			UserRepository repository = CreateRepository();

			editorUsers.ForEach(user =>
			{
				repository.SaveOrUpdateUserAsync(user).GetAwaiter().GetResult();
			});

			// when
			IEnumerable<User> actualEditors = await repository.FindAllEditorsAsync();

			// then
			actualEditors.Count().ShouldBe(editorUsers.Count);
			actualEditors.ShouldAllBe(x => x.IsEditor);
		}

		[Fact]
		public async void FindAllAdmins_should_return_admins_only()
		{
			// given
			List<User> adminUsers = _fixture.Build<User>()
											.With(x => x.IsAdmin, true)
											.With(x => x.IsEditor, false)
											.CreateMany()
											.ToList();

			UserRepository repository = CreateRepository();

			adminUsers.ForEach(user =>
			{
				repository.SaveOrUpdateUserAsync(user).GetAwaiter().GetResult();
			});

			// when
			IEnumerable<User> actualAdmins = await repository.FindAllAdminsAsync();

			// then
			actualAdmins.Count().ShouldBe(adminUsers.Count());
		}

		[Fact]
		public async void GetAdminById_should_return_admin_user()
		{
			// given
			UserRepository repository = CreateRepository();

			User expectedUser = _fixture.Build<User>()
										.With(x => x.IsAdmin, true)
										.With(x => x.IsEditor, false)
										.Create();

			await repository.SaveOrUpdateUserAsync(expectedUser);

			// when
			User actualUser = await repository.GetAdminByIdAsync(expectedUser.Id);

			// then
			actualUser.ShouldNotBeNull();
			actualUser.ShouldBeEquivalent(expectedUser);
		}

		[Fact]
		public async void GetUserByActivationKey_should_return_user()
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Create<User>();
			await repository.SaveOrUpdateUserAsync(expectedUser);

			// when
			User actualUser = await repository.GetUserByActivationKeyAsync(expectedUser.ActivationKey);

			// then
			actualUser.ShouldNotBeNull();
			actualUser.ShouldBeEquivalent(expectedUser);
		}

		[Fact]
		public async void GetEditorById_should_return_editor_only()
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Build<User>()
											.With(x => x.IsAdmin, false)
											.With(x => x.IsEditor, true)
											.Create();

			await repository.SaveOrUpdateUserAsync(expectedUser);

			// when
			User actualUser = await repository.GetEditorByIdAsync(expectedUser.Id);

			// then
			actualUser.ShouldNotBeNull();
			actualUser.ShouldBeEquivalent(expectedUser);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async void GetUserByEmail_should_return_user(bool isActivated)
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Build<User>()
										.With(x => x.IsActivated, isActivated)
										.Create();

			await repository.SaveOrUpdateUserAsync(expectedUser);

			// when
			User actualUser = await repository.GetUserByEmailAsync(expectedUser.Email, isActivated);

			// then
			actualUser.ShouldNotBeNull();
			actualUser.ShouldBeEquivalent(expectedUser);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async void GetUserById_should_return_user(bool isActivated)
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Build<User>()
				.With(x => x.IsActivated, isActivated)
				.Create();

			await repository.SaveOrUpdateUserAsync(expectedUser);

			// when
			User actualUser = await repository.GetUserByIdAsync(expectedUser.Id, isActivated);

			// then
			actualUser.ShouldNotBeNull();
			actualUser.ShouldBeEquivalent(expectedUser);
		}

		[Fact]
		public async void GetUserByPasswordResetKey_should_return_user()
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Create<User>();

			await repository.SaveOrUpdateUserAsync(expectedUser);

			// when
			User actualUser = await repository.GetUserByPasswordResetKeyAsync(expectedUser.PasswordResetKey);

			// then
			actualUser.ShouldNotBeNull();
			actualUser.ShouldBeEquivalent(expectedUser);
		}

		[Fact]
		public async void GetUserByUsername_should_return_user()
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Create<User>();

			await repository.SaveOrUpdateUserAsync(expectedUser);

			// when
			User actualUser = await repository.GetUserByUsernameAsync(expectedUser.Username);

			// then
			actualUser.ShouldNotBeNull();
			actualUser.ShouldBeEquivalent(expectedUser);
		}

		[Fact]
		public async void GetUserByUsernameOrEmail_should_return_user()
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Create<User>();

			await repository.SaveOrUpdateUserAsync(expectedUser);

			// when
			User actualUser = await repository.GetUserByUsernameOrEmailAsync(expectedUser.Username, expectedUser.Email);

			// then
			actualUser.ShouldNotBeNull();
			actualUser.ShouldBeEquivalent(expectedUser);
		}

		[Fact]
		public async void SaveOrUpdateUser_should_create_new_user()
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Create<User>();

			// when
			await repository.SaveOrUpdateUserAsync(expectedUser);

			// then
			User actualUser = await repository.GetUserByIdAsync(expectedUser.Id);
			actualUser.ShouldNotBeNull();
		}

		[Fact]
		public async void SaveOrUpdateUser_should_update_user()
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Create<User>();
			await repository.SaveOrUpdateUserAsync(expectedUser);

			expectedUser.Firstname = "My name";
			expectedUser.Lastname = "A Jeff";

			// when
			await repository.SaveOrUpdateUserAsync(expectedUser);

			// then
			User actualUser = await repository.GetUserByIdAsync(expectedUser.Id);
			actualUser.ShouldNotBeNull();
		}
	}
}
