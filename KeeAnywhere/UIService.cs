﻿using System;
using System.Linq;
using System.Threading.Tasks;
using KeeAnywhere.Configuration;
using KeeAnywhere.StorageProviders;
using KeePassLib.Utility;

namespace KeeAnywhere
{
    public class UIService
    {
        private readonly ConfigurationService _configService;
        private readonly StorageService _storageService;

        public UIService(ConfigurationService configService, StorageService storageService)
        {
            _configService = configService;
            _storageService = storageService;
        }

        public async Task<AccountConfiguration> CreateOrUpdateAccount(StorageType type)
        {
            var newAccount = await _storageService.CreateAccount(type);
            if (newAccount == null) return null;

            var existingAccount = _configService.Accounts.SingleOrDefault(_ => _.Type == newAccount.Type && _.Id == newAccount.Id);
            if (existingAccount == null) // New Account
            {
                // ensure account's name is unique
                var i = 1;
                var name = newAccount.Name;
                while (
                    _configService.Accounts.Any(
                        _ => _.Type == newAccount.Type && _.Name.Equals(newAccount.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    i++;
                    newAccount.Name = string.Format("{0} ({1})", name, i);
                }

                _configService.Accounts.Add(newAccount);
                return newAccount;
            }

            MessageService.ShowInfo("This account already exists.\r\nUpdating account data only.");

            //existingAccount.Name = newAccount.Name;
            existingAccount.Secret = newAccount.Secret;

            return existingAccount;
        }
    }
}