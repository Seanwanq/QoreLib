using System.Collections.Generic;

namespace QoreLib.Models;

public class SpectrumAdditionalDataModel
{
    public int GroupId { get; set; } = 0;
    public string Name { get; set; } = "";
    public string ValueType { get; set; } = "";
    public List<double> Omega01Index { get; set; } = new();
    public List<double> Omega01ValueReal { get; set; } = new();
    public List<double> HalfOmega02Index { get; set; } = new();
    public List<double> HalfOmega02ValueReal { get; set; } = new();

    public void Clear()
    {
        GroupId = 0;
        Name = "";
        ValueType = "";
        Omega01Index.Clear();
        Omega01ValueReal.Clear();
        HalfOmega02Index.Clear();
        HalfOmega02ValueReal.Clear();
    }
}