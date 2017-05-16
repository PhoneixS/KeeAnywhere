using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KeeAnywhere.Configuration
{
    [DataContract]
    public class PluginConfiguration
    {
        private AccountStorageLocation _storeLocation;        

        [DataMember]
        public bool IsOfflineCacheEnabled { get; set; }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public AccountStorageLocation AccountStorageLocation { get; set; }

        [DataMember]
        public AccountIdentifier FilePickerLastUsedAccount { get; set; }

        [DataMember]
        public DateTime DonationDialogLastShown { get; set; }


        public PluginConfiguration()
        {
            this.IsOfflineCacheEnabled = true;
            this.AccountStorageLocation = AccountStorageLocation.LocalUserSecureStore;
            this.DonationDialogLastShown = DateTime.MinValue;
        }
    }
}
