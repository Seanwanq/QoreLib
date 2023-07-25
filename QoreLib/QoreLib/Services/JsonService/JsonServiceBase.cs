using System;
using System.IO;
using System.Text.Json;
using LiveChartsCore.Drawing;
using QoreLib.Models;

namespace QoreLib.Services;

public class JsonServiceBase
{
    public static SpectrumBaseDataModel ReadSpectrumBaseDataJson(string datafilePath)
    {
        try
        {
            string jsonString = File.ReadAllText(datafilePath);
            SpectrumBaseDataModel spectrumBaseData = JsonSerializer.Deserialize<SpectrumBaseDataModel>(jsonString) ?? throw new InvalidOperationException();
            return spectrumBaseData;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
}