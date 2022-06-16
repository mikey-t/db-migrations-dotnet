using MikeyT.EnvironmentSettingsNS.Attributes;
using MikeyT.EnvironmentSettingsNS.Enums;

namespace MikeyT.DbMigrations;

public enum DbMigrationSettings
{
    [SettingInfo(ShouldLogValue = true)]
    DB_HOST,

    [SettingInfo(ShouldLogValue = true)]
    DB_PORT,

    [SettingInfo(ShouldLogValue = true)]
    DB_NAME,

    [SettingInfo(SettingType = SettingType.Secret)]
    DB_USER,

    [SettingInfo(SettingType = SettingType.Secret)]
    DB_PASSWORD,

    [SettingInfo(SettingType = SettingType.Secret)]
    DB_ROOT_USER,

    [SettingInfo(SettingType = SettingType.Secret)]
    DB_ROOT_PASSWORD
}
