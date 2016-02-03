using System.Runtime.Serialization;

namespace KeeAnywhere.Configuration
{
    [DataContract]
    public class PluginConfiguration
    {
        private AccountStorageLocation _storeLocation;        

        [DataMember]
        public bool IsOfflineCacheEnabled { get; set; }

        [DataMember]
        public AccountStorageLocation AccountStorageLocation { get; set; }

        public PluginConfiguration(bool isUnix)
        {
            this.IsOfflineCacheEnabled = true;
            if (isUnix) {
                this.AccountStorageLocation = AccountStorageLocation.KeePassConfig;
            } else {
                this.AccountStorageLocation = AccountStorageLocation.WindowsCredentialManager;
            }
        }
    }
}
