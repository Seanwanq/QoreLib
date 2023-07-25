using System.Collections.Generic;

namespace QoreLib.Models;

public class SpectrumBaseDataModel
{
    public int GroupId { get; set; }
    public string Name { get; set; }
    public string ValueType { get; set; }
    public List<double> Index { get; set; }
    public List<double> ValueReal { get; set; }
    public List<double> ValueImagine { get; set; }
}