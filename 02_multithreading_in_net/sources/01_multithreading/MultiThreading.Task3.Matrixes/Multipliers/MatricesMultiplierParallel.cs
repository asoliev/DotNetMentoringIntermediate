using System;
using System.Threading.Tasks;
using MultiThreading.Task3.MatrixMultiplier.Matrices;

namespace MultiThreading.Task3.MatrixMultiplier.Multipliers;

public class MatricesMultiplierParallel : IMatricesMultiplier
{
    public IMatrix Multiply(IMatrix m1, IMatrix m2)
    {
        ArgumentNullException.ThrowIfNull(m1);
        ArgumentNullException.ThrowIfNull(m2);
        if (m1.ColCount != m2.RowCount) throw new ArgumentException("Incompatible matrix dimensions.");

        // Cast to int for Parallel.For (matrices sizes in this exercise fit into int)
        int rows = (int)m1.RowCount;
        int cols = (int)m2.ColCount;
        int inner = (int)m1.ColCount;

        var result = new Matrix(rows, cols);

        Parallel.For(0, rows, i =>
        {
            for (int j = 0; j < cols; j++)
            {
                long sum = 0;
                for (int k = 0; k < inner; k++)
                {
                    sum += m1.GetElement(i, k) * m2.GetElement(k, j);
                }

                result.SetElement(i, j, sum);
            }
        });

        return result;
    }
}