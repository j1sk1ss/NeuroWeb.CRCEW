using FotNET.NETWORK.MATH.OBJECTS;

namespace FotNET.NETWORK.MATH.Initialization.RANDOM;

public class RandomInitialization : IWeightsInitialization {
    /// <summary>
    /// Initialize weights by random algorithm
    /// </summary>
    /// <param name="minValue"> Min value that can be generated by algorithm </param>
    /// <param name="maxValue"> Max value that can be generated by algorithm </param>
    public RandomInitialization(double minValue = -1, double maxValue = 1) {
        MaxValue = maxValue;
        MinValue = minValue;
    }
    
    private double MinValue { get; }
    private double MaxValue { get; }
    
    public Matrix Initialize(Matrix matrix) {
        for (var i = 0; i < matrix.Rows; i++)
            for (var j = 0; j < matrix.Columns; j++)
                matrix.Body[i, j] = new Random().NextDouble() % MaxValue - MinValue;

        return matrix;
    }
}