using System;
using System.IO;
using System.Text.Json;
using QoreLib.Models;

namespace QoreLib.Services.JsonService;

public class SpectrumJsonService : JsonServiceBase
{
    public static SpectrumBaseDataModel ReadSpectrumBaseDataJson(string datafilePath)
    {
        string jsonString = File.ReadAllText(datafilePath);
        SpectrumBaseDataModel spectrumBaseData = JsonSerializer.Deserialize<SpectrumBaseDataModel>(jsonString) ??
                                                 throw new InvalidOperationException();
        return spectrumBaseData;
    }

    public static SpectrumAdditionalDataModel ReadSpectrumAdditionalDataJson(string datafilePath)
    {
        string jsonString = File.ReadAllText(datafilePath);
        SpectrumAdditionalDataModel spectrumAdditionalData =
            JsonSerializer.Deserialize<SpectrumAdditionalDataModel>(jsonString) ??
            throw new InvalidOperationException();
        return spectrumAdditionalData;
    }

    public static string WriteSpectrumAdditionalDataJson(string baseDir, string additionalRelativeDataFileDir,
        SpectrumAdditionalDataModel additionalData)
    {
        string relativeDatafilePath = Path.Combine(additionalRelativeDataFileDir, "SpectrumAdditionalData.json");
        string datafilePath = Path.Join(baseDir, relativeDatafilePath);
        string jsonString =
            JsonSerializer.Serialize(additionalData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(datafilePath, jsonString);
        string returnedRelativeDataFilePath = relativeDatafilePath.Replace('\\', '/');
        return returnedRelativeDataFilePath;
    }
}