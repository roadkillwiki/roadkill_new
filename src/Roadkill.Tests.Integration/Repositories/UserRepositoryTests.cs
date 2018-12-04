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

// ReSharper disable PossibleMultipleEnumeration

namespace Roadkill.Tests.Integration.Repositories
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
			await repository.SaveOrUpdateUser(adminUser);
			await repository.SaveOrUpdateUser(editorUser);

			IEnumerable<User> admins = await repository.FindAllAdmins();
			Assert.NotEmpty(admins);

			IEnumerable<User> editors = await repository.FindAllEditors();
			Assert.NotEmpty(editors);

			// when
			await repository.DeleteAllUsers();

			// then
			admins = await repository.FindAllAdmins();
			editors = await repository.FindAllEditors();

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
				await repository.SaveOrUpdateUser(u);
			});

			User actualUser = _fixture.Build<User>()
									  .With(x => x.IsAdmin, false)
									  .With(x => x.IsEditor, true)
									  .Create();

			await repository.SaveOrUpdateUser(actualUser);

			// when
			await repository.DeleteUser(actualUser);

			// then
			User deletedUser = await repository.GetUserById(actualUser.Id);
			deletedUser.ShouldBeNull();

			Guid userId = remainingUsers.First().Id;
			User firstRemainingUser = await repository.GetUserById(userId);
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
				repository.SaveOrUpdateUser(user).GetAwaiter().GetResult();
			});

			// when
			IEnumerable<User> actualEditors = await repository.FindAllEditors();

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
				repository.SaveOrUpdateUser(user).GetAwaiter().GetResult();
			});

			// when
			IEnumerable<User> actualAdmins = await repository.FindAllAdmins();

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

			await repository.SaveOrUpdateUser(expectedUser);

			// when
			User actualUser = await repository.GetAdminById(expectedUser.Id);

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
			await repository.SaveOrUpdateUser(expectedUser);

			// when
			User actualUser = await repository.GetUserByActivationKey(expectedUser.ActivationKey);

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

			await repository.SaveOrUpdateUser(expectedUser);

			// when
			User actualUser = await repository.GetEditorById(expectedUser.Id);

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

			await repository.SaveOrUpdateUser(expectedUser);

			// when
			User actualUser = await repository.GetUserByEmail(expectedUser.Email, isActivated);

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

			await repository.SaveOrUpdateUser(expectedUser);

			// when
			User actualUser = await repository.GetUserById(expectedUser.Id, isActivated);

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

			await repository.SaveOrUpdateUser(expectedUser);

			// when
			User actualUser = await repository.GetUserByPasswordResetKey(expectedUser.PasswordResetKey);

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

			await repository.SaveOrUpdateUser(expectedUser);

			// when
			User actualUser = await repository.GetUserByUsername(expectedUser.Username);

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

			await repository.SaveOrUpdateUser(expectedUser);

			// when
			User actualUser = await repository.GetUserByUsernameOrEmail(expectedUser.Username, expectedUser.Email);

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
			await repository.SaveOrUpdateUser(expectedUser);

			// then
			User actualUser = await repository.GetUserById(expectedUser.Id);
			actualUser.ShouldNotBeNull();
		}

		[Fact]
		public async void SaveOrUpdateUser_should_update_user()
		{
			// given
			UserRepository repository = CreateRepository();
			User expectedUser = _fixture.Create<User>();
			await repository.SaveOrUpdateUser(expectedUser);

			expectedUser.Firstname = "My name";
			expectedUser.Lastname = "A Jeff";

			// when
			await repository.SaveOrUpdateUser(expectedUser);

			// then
			User actualUser = await repository.GetUserById(expectedUser.Id);
			actualUser.ShouldNotBeNull();
		}
	}
}