using System;

namespace QoreLib.Models;

public class SpectrumModel
{
    public int Id { get; set; }
    public byte[] DataX { get; set; }
    public byte[] DataY { get; set; }
    public byte[]? Omega01X { get; set; }
    public byte[]? Omega01Y { get; set; }
    public byte[]? Omega12X { get; set; }
    public byte[]? Omega12Y { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public bool IsFilled { get; set; }
    public bool? IsUseful { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime AlterTime { get; set; }
    public DateTime MarkTime { get; set; }
}