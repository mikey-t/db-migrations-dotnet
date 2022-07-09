using MikeyT.EnvironmentSettingsNS.Attributes;
using MikeyT.EnvironmentSettingsNS.Enums;

namespace ExampleApi
{
    public enum GlobalSettings
    {
        [SettingInfo(DefaultValue = "localhost", DefaultForEnvironment = DefaultSettingForEnvironment.LocalOnly, ShouldLogValue = true)]
        DB_HOST,

        [SettingInfo(DefaultValue = "5432", DefaultForEnvironment = DefaultSettingForEnvironment.LocalOnly, ShouldLogValue = true)]
        DB_PORT,
        
        [SettingInfo(DefaultValue = "dbmigrationsexample", DefaultForEnvironment = DefaultSettingForEnvironment.LocalOnly, ShouldLogValue = true)]
        DB_NAME,
        
        [SettingInfo(DefaultValue = "true", DefaultForEnvironment = DefaultSettingForEnvironment.AllEnvironments, ShouldLogValue = true)]
        POSTGRES_INCLUDE_ERROR_DETAIL,
      
        [SettingInfo(SettingType = SettingType.Secret)]
        DB_USER,
        
        [SettingInfo(SettingType = SettingType.Secret)]
        DB_PASSWORD
    }
}
