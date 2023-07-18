using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace QoreLib.Services;

public class JsonService
{
    public static Dictionary<string, List<double>> ReadBaseDataJson(string dataFilePath)
    {
        string jsonString = File.ReadAllText(dataFilePath);
        JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
        JsonElement indexArray = jsonDocument.RootElement.GetProperty("Index");
        List<double> indexList = new();
        if (indexArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement element in indexArray.EnumerateArray())
            {
                double value = element.GetDouble();
                indexList.Add(value);
            }
        }
        JsonElement valueRealArray = jsonDocument.RootElement.GetProperty("ValueReal");
        List<double> valueRealList = new();
        if (valueRealArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement element in valueRealArray.EnumerateArray())
            {
                double value = element.GetDouble();
                valueRealList.Add(value);
            }
        }
        JsonElement valueImagineArray = jsonDocument.RootElement.GetProperty("ValueImagine");
        List<double> valueImagineList = new();
        if (valueImagineArray.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement element in valueImagineArray.EnumerateArray())
            {
                double value = element.GetDouble();
                valueImagineList.Add(value);
            }
        }
        jsonDocument.Dispose();
        Dictionary<string, List<double>> dataToBeReturned = new()
        {
            { "Index", indexList },
            { "ValueReal", valueRealList },
            { "ValueImagine", valueImagineList }
        };
        return dataToBeReturned;
    }
}