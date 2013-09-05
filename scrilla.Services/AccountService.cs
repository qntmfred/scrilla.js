﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using scrilla.Data;
using DapperExtensions;
using Dapper;

namespace scrilla.Services
{
	public class AccountService : EntityService, IAccountService
	{
		private ICategoryService _categoryService;

		public AccountService(IDatabase database, ICategoryService categoryService)
			: base(database) 
		{
			_categoryService = categoryService;
		}

		public ServiceResult<Account> GetAccount(int accountId)
		{
			return base.GetEntity<Account>(accountId);
		}

		public ServiceResult<AccountGroup> GetAccountGroup(int accountGroupId)
		{
			return base.GetEntity<AccountGroup>(accountGroupId);
		}
		
		public ServiceResult<IEnumerable<Account>> GetAllAccounts()
		{
			return base.GetAllEntity<Account>();
		}

		public ServiceResult<IEnumerable<AccountGroup>> GetAllAccountGroups()
		{
			return base.GetAllEntity<AccountGroup>();
		}

		public ServiceResult<IEnumerable<AccountNameMap>> GetAllAccountNameMaps()
		{
			return base.GetAllEntity<AccountNameMap>();
		}


		public ServiceResult<Account> AddAccount(string name, decimal initialBalance = 0.0M, int? defaultCategoryId = null, int? accountGroupId = null)
		{
			var result = new ServiceResult<Account>();

			// does AccountGroup exist?
			if (accountGroupId.HasValue)
			{
				var accountGroupResult = GetAccountGroup(accountGroupId.Value);
				if (accountGroupResult.HasErrors)
				{
					result.AddErrors(accountGroupResult);
					return result;
				}
			}

			// does default Category exist?
			if (defaultCategoryId.HasValue)
			{
				var categoryResult = _categoryService.GetCategory(defaultCategoryId.Value);
				if (categoryResult.HasErrors)
				{
					result.AddErrors(categoryResult);
					return result;
				}
			}

			// create Account
			var account = new Account()
			{
				Name = name,
				InitialBalance = initialBalance,
				Balance = initialBalance,
				BalanceTimestamp = DateTime.Now,
				DefaultCategoryId = defaultCategoryId,
				AccountGroupId = accountGroupId
			};

			_db.Insert<Account>(account);

			result.Result = account;
			return result;
		}

		public ServiceResult<AccountGroup> AddAccountGroup(string name, int displayOrder = 0, bool isActive = true)
		{
			var result = new ServiceResult<AccountGroup>();

			// create AccountGroup
			var accountGroup = new AccountGroup()
			{
				Name = name,
				DisplayOrder = displayOrder,
				IsActive = isActive
			};

			_db.Insert<AccountGroup>(accountGroup);

			result.Result = accountGroup;
			return result;
		}

		public ServiceResult<AccountNameMap> AddAccountNameMap(int accountId, string name)
		{
			var result = new ServiceResult<AccountNameMap>();

			var accountResult = GetAccount(accountId);
			if (accountResult.HasErrors)
			{
				result.AddErrors(accountResult);
				return result;
			}

			var accountNameMap = new AccountNameMap()
			{
				AccountId = accountId,
				Name = name
			};

			// TODO check for existing mappings?

			_db.Insert<AccountNameMap>(accountNameMap);

			result.Result = accountNameMap;
			return result;
		}


		public ServiceResult<bool> DeleteAccount(int accountId)
		{
			// TODO handle cascading deletes
			var result = new ServiceResult<bool>();

			var deletionResult = _db.Delete<Account>(Predicates.Field<Account>(x => x.Id, Operator.Eq, accountId));
			if (!deletionResult)
			{
				result.AddError(ErrorType.NotFound, "Account {0} not found", accountId);
				return result;
			}

			result.Result = deletionResult;
			return result;
		}

		public ServiceResult<bool> DeleteAccountGroup(int accountGroupId)
		{
			// TODO handle cascading deletes
			var result = new ServiceResult<bool>();

			var deletionResult = _db.Delete<AccountGroup>(Predicates.Field<AccountGroup>(x => x.Id, Operator.Eq, accountGroupId));
			if (!deletionResult)
			{
				result.AddError(ErrorType.NotFound, "AccountGroup {0} not found", accountGroupId);
				return result;
			}

			result.Result = deletionResult;
			return result;
		}


		public ServiceResult<Account> UpdateAccount(int accountId, Filter<string> name = null, Filter<decimal> initialBalance = null, Filter<int?> defaultCategoryId = null, Filter<int?> accountGroupId = null)
		{
			var result = new ServiceResult<Account>();

			// does account exist?
			var accountResult = GetAccount(accountId);
			if (accountResult.HasErrors)
			{
				result.AddErrors(accountResult);
				return result;
			}

			var account = accountResult.Result;

			if (name != null)
				account.Name = name.Object;

			if (initialBalance != null)
				account.InitialBalance = initialBalance.Object;

			if (defaultCategoryId != null)
			{
				// does category exist?
				if (defaultCategoryId.Object.HasValue)
				{
					var categoryResult = _categoryService.GetCategory(defaultCategoryId.Object.Value);
					if (categoryResult.HasErrors)
					{
						result.AddErrors(categoryResult);
						return result;
					}
				}

				account.DefaultCategoryId = defaultCategoryId.Object;
			}

			if (accountGroupId != null)
			{
				// does account group exist?
				if (accountGroupId.Object.HasValue)
				{
					var accountGroupResult = GetAccountGroup(accountGroupId.Object.Value);
					if (accountGroupResult.HasErrors)
					{
						result.AddErrors(accountGroupResult);
						return result;
					}
				}

				account.AccountGroupId = accountGroupId.Object;
			}

			_db.Update<Account>(account);

			UpdateAccountBalances();

			result.Result = account;
			return result;
		}

		public ServiceResult<bool> UpdateAccountBalances()
		{
			var result = new ServiceResult<bool>();

			var updateResult = _db.Connection.Execute(@"
;WITH balances AS
(
	SELECT a.Id as AccountId, COALESCE(SUM(t.Amount), 0) as TransactionSum
	FROM Account a
	LEFT JOIN [Transaction] t on t.AccountId = a.Id
	GROUP BY a.Id
)

UPDATE a
SET a.Balance = a.InitialBalance + b.TransactionSum, a.BalanceTimestamp = GETDATE()
FROM Account a
JOIN balances b on b.AccountId = a.Id");

			result.Result = true;
			return result;
		}
	}
}
