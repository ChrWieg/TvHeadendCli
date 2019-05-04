using System;
using System.Configuration;
using System.Net;
using AdysTech.CredentialManager;
using TvHeadendLib.Models;
using TvHeadendLib.Properties;

namespace TvHeadendLib.Helper
{
    public class CredentialHelper
    {
        private static Credential _networkCredential;
        private const string DefaultCredentialStoreServiceName = "TvHeadendApi";

        public static Credential GetStoredCredential(bool promptForCredentialsIfNotFond)
        {
            var serviceName = GetCredentialStoreServiceName();

            if (_networkCredential != null)
                return _networkCredential;

            var credentialFromStore = CredentialManager.GetCredentials(serviceName);

            if (credentialFromStore != null)
                return _networkCredential = new Credential {UserName = credentialFromStore.UserName, Password = credentialFromStore.Password};

            if (!promptForCredentialsIfNotFond)
                return null;

            var saveCredentials = true;
            var credentialFromUser = CredentialManager.PromptForCredentials(serviceName, ref saveCredentials, "Please provide credentials", $"Credentials for service {serviceName}");

            if (credentialFromUser == null)
                return _networkCredential;

            if (saveCredentials)
                CredentialManager.SaveCredentials(serviceName, credentialFromUser);

            _networkCredential = new Credential { UserName = credentialFromUser.UserName, Password = credentialFromUser.Password };

            return _networkCredential;
        }

        public static void ResetCredential(NetworkCredential networkCredential)
        {
            var serviceName = GetCredentialStoreServiceName();

            var saveCredentials = true;

            if (networkCredential == null)
            {
                networkCredential = CredentialManager.PromptForCredentials(serviceName, ref saveCredentials, "Please provide credentials", $"Credentials for service {serviceName}");
            }

            if (networkCredential == null) return;

            if (saveCredentials)
                CredentialManager.SaveCredentials(serviceName, networkCredential);

            _networkCredential = new Credential { UserName = networkCredential.UserName, Password = networkCredential.Password };
        }

        public static string GetCredentialStoreServiceName()
        {
            //ToDo: testen
            var propertyName = "CredentialStoreServiceName";
            var providerName = "LocalFileSettingsProvider";

            if (!string.IsNullOrWhiteSpace(Settings.Default.Properties[propertyName]?.DefaultValue.ToString()))
            {
                var attributes = new SettingsAttributeDictionary();
                var attr = new UserScopedSettingAttribute();
                attributes.Add(attr.TypeId, attr);

                var prop = new SettingsProperty(
                        propertyName, 
                        typeof(string), 
                        Settings.Default.Providers[providerName], 
                        false, DefaultCredentialStoreServiceName, SettingsSerializeAs.String, attributes, false, false);

                Settings.Default.Properties.Add(prop);
                Settings.Default.Save();
                Settings.Default.Reload();
            }

            var result = Settings.Default.Properties[propertyName]?.ToString();
            if (result != null)
            {
                Console.WriteLine(result);
                return result;
            }

            return DefaultCredentialStoreServiceName;
        }
    }
}
