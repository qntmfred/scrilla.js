﻿using DapperExtensions;
using DapperExtensions.Sql;
using Ploeh.AutoFixture;
using scrilla.Data;
using scrilla.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace scrilla.Services.Tests
{
	public class GetAccountTests : BaseFixture
	{
		public GetAccountTests()
		{
			_fixture.Register<ICategoryService>(() => _fixture.Create<CategoryService>());
		}

		[Fact]
		public void GetAccount_ExistingAccount_WithDefaultCategory_And_AccountGroup()
		{
			var categoryService = _fixture.Create<CategoryService>();
			var sut = _fixture.Create<AccountService>();
			var accountName = "test account";
			var balance = 1.23M;

			// get a default category
			var categoryName = "test category";
			var categoryResult = categoryService.AddCategory(categoryName);
			Assert.False(categoryResult.HasErrors);

			// get a default account group
			var accountGroupName = "test account group";
			var accountGroupResult = sut.AddAccountGroup(accountGroupName);
			Assert.False(accountGroupResult.HasErrors);

			// create test account
			var addAccountResult = sut.AddAccount(accountName, balance, categoryResult.Result.Id, accountGroupResult.Result.Id);
			Assert.False(addAccountResult.HasErrors);
			Assert.Equal(accountName, addAccountResult.Result.Name);
			Assert.Equal(balance, addAccountResult.Result.Balance);
			Assert.Equal(categoryResult.Result.Id, addAccountResult.Result.DefaultCategoryId);
			Assert.Equal(accountGroupResult.Result.Id, addAccountResult.Result.AccountGroupId);

			// act
			var result = sut.GetAccount(addAccountResult.Result.Id);
			Assert.False(result.HasErrors);
			Assert.Equal(accountName, result.Result.Name);
			Assert.Equal(balance, result.Result.Balance);
			Assert.Equal(categoryResult.Result.Id, result.Result.DefaultCategoryId);
			Assert.Equal(accountGroupResult.Result.Id, result.Result.AccountGroupId);

			// cleanup
			sut.DeleteAccount(addAccountResult.Result.Id);
			categoryService.DeleteCategory(categoryResult.Result.Id);
			sut.DeleteAccountGroup(accountGroupResult.Result.Id);
		}

		[Fact]
		public void GetAccount_ExistingAccount_WithNullDefaultCategory_And_NullAccountGroup()
		{
			var sut = _fixture.Create<AccountService>();
			var accountName = "test account";
			var balance = 1.23M;
			int? defaultCategoryId = null;
			int? accountGroupId = null;

			// create test account
			var addAccountResult = sut.AddAccount(accountName, balance, defaultCategoryId, accountGroupId);
			Assert.False(addAccountResult.HasErrors);
			Assert.Equal(accountName, addAccountResult.Result.Name);
			Assert.Equal(balance, addAccountResult.Result.Balance);
			Assert.Equal(defaultCategoryId, addAccountResult.Result.DefaultCategoryId);
			Assert.Equal(accountGroupId, addAccountResult.Result.AccountGroupId);

			// act
			var result = sut.GetAccount(addAccountResult.Result.Id);
			Assert.False(result.HasErrors);
			Assert.Equal(accountName, result.Result.Name);
			Assert.Equal(balance, result.Result.Balance);
			Assert.Equal(defaultCategoryId, result.Result.DefaultCategoryId);
			Assert.Equal(accountGroupId, result.Result.AccountGroupId);

			// cleanup
			sut.DeleteAccount(addAccountResult.Result.Id);
		}

		[Fact]
		public void GetAccount_NonExistantAccount()
		{
			var sut = _fixture.Create<AccountService>();
			var nonExistantAccountId = -1;

			// act
			var result = sut.GetAccount(nonExistantAccountId);

			Assert.True(result.HasErrors);
			Assert.True(result.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));
		}
	}

	public class GetAccountGroupTests : BaseFixture
	{
		public GetAccountGroupTests()
		{
			_fixture.Register<ICategoryService>(() => _fixture.Create<CategoryService>());
		}

		[Fact]
		public void GetAccountGroup_ExistingAccountGroup()
		{
			var sut = _fixture.Create<AccountService>();
			var accountGroupName = "test account group";
			var displayOrder = 1;
			var isActive = true;

			// create test account group
			var addAccountGroupResult = sut.AddAccountGroup(accountGroupName, displayOrder, isActive);
			Assert.False(addAccountGroupResult.HasErrors);
			Assert.Equal(accountGroupName, addAccountGroupResult.Result.Name);
			Assert.Equal(displayOrder, addAccountGroupResult.Result.DisplayOrder);
			Assert.Equal(isActive, addAccountGroupResult.Result.IsActive);

			// act
			var result = sut.GetAccountGroup(addAccountGroupResult.Result.Id);
			Assert.False(result.HasErrors);
			Assert.Equal(accountGroupName, result.Result.Name);
			Assert.Equal(displayOrder, result.Result.DisplayOrder);
			Assert.Equal(isActive, result.Result.IsActive);

			// cleanup
			sut.DeleteAccountGroup(addAccountGroupResult.Result.Id);
		}

		[Fact]
		public void GetAccountGroup_NonExistantAccount()
		{
			var sut = _fixture.Create<AccountService>();
			var nonExistantAccountGroupId = -1;

			// act
			var result = sut.GetAccountGroup(nonExistantAccountGroupId);

			Assert.True(result.HasErrors);
			Assert.True(result.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));
		}
	}


	public class GetAllAccountsTests : BaseFixture
	{
		public GetAllAccountsTests()
		{
			_fixture.Register<ICategoryService>(() => _fixture.Create<CategoryService>());
		}

		[Fact]
		public void GetAllAccounts()
		{
			var sut = _fixture.Create<AccountService>();

			var accountsResult = sut.GetAllAccounts();
			Assert.False(accountsResult.HasErrors);
			Assert.Empty(accountsResult.Result);

			var name = "test account";
			var balance = 1.23M;
			var addAccountResult = sut.AddAccount(name, balance);
			Assert.False(addAccountResult.HasErrors);

			accountsResult = sut.GetAllAccounts();
			Assert.False(accountsResult.HasErrors);
			Assert.Equal(1, accountsResult.Result.Count());

			// cleanup
			sut.DeleteAccount(addAccountResult.Result.Id);
		}
	}


	public class AddAccountTests : BaseFixture
	{
		public AddAccountTests()
		{
			_fixture.Register<ICategoryService>(() => _fixture.Create<CategoryService>());
		}

		[Fact]
		public void AddAccount_NullDefaultCategory_And_NullAccountGroup()
		{
			var sut = _fixture.Create<AccountService>();
			var name = "test account";
			var balance = 1.23M;

			var result = sut.AddAccount(name, balance);

			Assert.False(result.HasErrors);
			Assert.Equal(name, result.Result.Name);
			Assert.Equal(balance, result.Result.Balance);
			Assert.Null(result.Result.DefaultCategoryId);
			Assert.Null(result.Result.AccountGroupId);

			// cleanup
			sut.DeleteAccount(result.Result.Id);
		}

		[Fact]
		public void AddAccount_NonNullDefaultCategory()
		{
			var categoryService = _fixture.Create<CategoryService>();
			var sut = _fixture.Create<AccountService>();
			var accountName = "test account";
			var balance = 1.23M;

			// get a default category
			var categoryName = "test category";
			var categoryResult = categoryService.AddCategory(categoryName);
			Assert.False(categoryResult.HasErrors);

			var result = sut.AddAccount(accountName, balance, categoryResult.Result.Id);

			Assert.False(result.HasErrors);
			Assert.Equal(accountName, result.Result.Name);
			Assert.Equal(balance, result.Result.Balance);
			Assert.Equal(categoryResult.Result.Id, result.Result.DefaultCategoryId);

			// cleanup
			sut.DeleteAccount(result.Result.Id);
			categoryService.DeleteCategory(categoryResult.Result.Id);
		}

		[Fact]
		public void AddAccount_NonExistantDefaultCategory()
		{
			var sut = _fixture.Create<AccountService>();
			var accountName = "test account";
			var balance = 1.23M;
			var defaultCategoryId = -1;

			var result = sut.AddAccount(accountName, balance, defaultCategoryId);

			Assert.True(result.HasErrors);
			Assert.True(result.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));
		}

		[Fact]
		public void AddAccount_NonNullAccountGroup()
		{
			var sut = _fixture.Create<AccountService>();
			var accountName = "test account";
			var balance = 1.23M;

			// get an account group
			var accountGroupName = "test account group";
			var accountGroupResult = sut.AddAccountGroup(accountGroupName);
			Assert.False(accountGroupResult.HasErrors);

			var result = sut.AddAccount(accountName, balance, accountGroupId: accountGroupResult.Result.Id);

			Assert.False(result.HasErrors);
			Assert.Equal(accountName, result.Result.Name);
			Assert.Equal(balance, result.Result.Balance);
			Assert.Equal(accountGroupResult.Result.Id, result.Result.AccountGroupId);

			// cleanup
			sut.DeleteAccount(result.Result.Id);
			sut.DeleteAccountGroup(accountGroupResult.Result.Id);
		}

		[Fact]
		public void AddAccount_NonExistantAccountGroup()
		{
			var sut = _fixture.Create<AccountService>();
			var accountName = "test account";
			var balance = 1.23M;
			var accountGroupId = -1;

			var result = sut.AddAccount(accountName, balance, accountGroupId: accountGroupId);

			Assert.True(result.HasErrors);
			Assert.True(result.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));
		}
	}

	public class AddAccountGroupTests : BaseFixture
	{
		public AddAccountGroupTests()
		{
			_fixture.Register<ICategoryService>(() => _fixture.Create<CategoryService>());
		}
	}

	public class DeleteAccountTests : BaseFixture
	{
		public DeleteAccountTests()
		{
			_fixture.Register<ICategoryService>(() => _fixture.Create<CategoryService>());
		}

		[Fact]
		public void DeleteAccount_ExistingAccount()
		{
			var sut = _fixture.Create<AccountService>();
			var name = "test account";
			var balance = 1.23M;

			// add a test account
			var addResult = sut.AddAccount(name, balance);
			Assert.False(addResult.HasErrors);

			// delete the test account
			var deletionResult = sut.DeleteAccount(addResult.Result.Id);
			Assert.False(deletionResult.HasErrors);

			// make sure the test account does not exist
			var getResult = sut.GetAccount(addResult.Result.Id);
			Assert.True(getResult.HasErrors);
			Assert.True(getResult.ErrorMessages.Any(x => x.Key == ErrorType.NotFound));
		}

		[Fact]
		public void DeleteAccount_NonExistantAccount()
		{
			var sut = _fixture.Create<AccountService>();

			var result = sut.DeleteAccount(-1);
			Assert.True(result.HasErrors);
		}
	}

	public class DeleteAccountGroupTests : BaseFixture
	{
		public DeleteAccountGroupTests()
		{
			_fixture.Register<ICategoryService>(() => _fixture.Create<CategoryService>());
		}
	}
}
