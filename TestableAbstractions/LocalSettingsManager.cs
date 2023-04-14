using System.Configuration;

namespace TestableAbstractions;

public interface ISettingsManager : IDisposable
{
    string? this[string settingName] { get; set; }
}

public class OverrideSettingsManager : ISettingsManager
{
    private readonly Dictionary<string, string?> valuesDict = new();

    public string? this[string settingName] 
    { 
        get
        {
            if (valuesDict.ContainsKey(settingName))
                return valuesDict[settingName];
            else return null;
        } 
        set => valuesDict[settingName] = value; 
    }

    public void Dispose()
    {
        
    }
}

public class LocalSettingsManager : ISettingsManager
{
    public string? this[string settingName]
    {
        get => ConfigurationManager.AppSettings[settingName];
        set
        {
            var file = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            
            if (file.AppSettings.Settings[settingName] == null)
            {
                file.AppSettings.Settings.Add(settingName, value);
            }
            else
            {
                file.AppSettings.Settings[settingName].Value = value;
            }

            file.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(file.AppSettings.SectionInformation.Name);
        }
    }

    public void Dispose()
    {
        
    }
}