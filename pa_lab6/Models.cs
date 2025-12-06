using System.Collections.ObjectModel;

namespace pa_lab6;

public class ExperimentData
{
    public string ParameterName { get; set; }
    public ObservableCollection<DataPoint> DataPoints { get; set; } = new();
}

public class DataPoint
{
    public double XValue { get; set; }
    public double YResult { get; set; } // Цільова функція (розмір покриття)
}

public class GeneticParams
{
    public int PopSize { get; set; } = 50;
    public int Generations { get; set; } = 50;
    public double CrossRate { get; set; } = 0.8;
    public double MutRate { get; set; } = 0.1;
    
    public GeneticParams Clone() => (GeneticParams)this.MemberwiseClone();
}