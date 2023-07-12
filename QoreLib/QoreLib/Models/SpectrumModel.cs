using System;

namespace QoreLib.Models;

public class SpectrumModel
{
    public int Id { get; set; }
    public double[] DataX { get; set; }
    public double[] DataY { get; set; }
    public double[]? Omega01X { get; set; }
    public double[]? Omega01Y { get; set; }
    public double[]? Omega12X { get; set; }
    public double[]? Omega12Y { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public bool IsFilled { get; set; }
    public bool? IsUseful { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime AlterTime { get; set; }
    public DateTime MarkTime { get; set; }
}