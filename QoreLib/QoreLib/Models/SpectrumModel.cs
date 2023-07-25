using System;

namespace QoreLib.Models;

public class SpectrumModel
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string DataFile { get; set; }
    public string? Omega01AndOmega12File { get; set; }
    public string APP { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public bool IsFilled { get; set; }
    public bool? IsUseful { get; set; }
    public bool IsWrongData { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime AlterTime { get; set; }
    public DateTime? MarkTime { get; set; }
}