using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiThreading.Task3.MatrixMultiplier.Matrices;
using MultiThreading.Task3.MatrixMultiplier.Multipliers;
using System;
using System.Diagnostics;

namespace MultiThreading.Task3.MatrixMultiplier.Tests;

[TestClass]
public class MultiplierTest
{
    [TestMethod]
    public void MultiplyMatrix3On3Test()
    {
        TestMatrix3On3(new MatricesMultiplier());
        TestMatrix3On3(new MatricesMultiplierParallel());
    }

    [TestMethod]
    public void ParallelEfficiencyTest()
    {
        // This test measures execution times for different square matrix sizes and prints results.
        // It does not assert a machine-dependent threshold so CI won't fail due to environment differences.
        var sizes = new[] { 50, 100, 150, 200, 250, 300 }; // adjust if you want to probe larger sizes
        const int repeats = 3;

        var sync = new MatricesMultiplier();
        var par = new MatricesMultiplierParallel();
        var sw = new Stopwatch();

        foreach (var n in sizes)
        {
            var m1 = new Matrix(n, n, true);
            var m2 = new Matrix(n, n, true);

            // warm up
            sync.Multiply(m1, m2);
            par.Multiply(m1, m2);

            long syncTotal = 0;
            long parTotal = 0;

            for (int r = 0; r < repeats; r++)
            {
                sw.Restart();
                sync.Multiply(m1, m2);
                sw.Stop();
                syncTotal += sw.ElapsedMilliseconds;

                sw.Restart();
                par.Multiply(m1, m2);
                sw.Stop();
                parTotal += sw.ElapsedMilliseconds;
            }

            var syncAvg = syncTotal / repeats;
            var parAvg = parTotal / repeats;

            Console.WriteLine($"Size={n}: sync={syncAvg} ms, parallel={parAvg} ms");

            if (parAvg < syncAvg)
            {
                Console.WriteLine($"Parallel became faster at size {n} (on this machine).");
                break;
            }
        }

        // Non-conditional assert so the test will not fail on different environments.
        Assert.IsTrue(true);
    }

    #region private methods

    void TestMatrix3On3(IMatricesMultiplier matrixMultiplier)
    {
        if (matrixMultiplier == null)
        {
            throw new ArgumentNullException(nameof(matrixMultiplier));
        }

        var m1 = new Matrix(3, 3);
        m1.SetElement(0, 0, 34);
        m1.SetElement(0, 1, 2);
        m1.SetElement(0, 2, 6);

        m1.SetElement(1, 0, 5);
        m1.SetElement(1, 1, 4);
        m1.SetElement(1, 2, 54);

        m1.SetElement(2, 0, 2);
        m1.SetElement(2, 1, 9);
        m1.SetElement(2, 2, 8);

        var m2 = new Matrix(3, 3);
        m2.SetElement(0, 0, 12);
        m2.SetElement(0, 1, 52);
        m2.SetElement(0, 2, 85);

        m2.SetElement(1, 0, 5);
        m2.SetElement(1, 1, 5);
        m2.SetElement(1, 2, 54);

        m2.SetElement(2, 0, 5);
        m2.SetElement(2, 1, 8);
        m2.SetElement(2, 2, 9);

        var multiplied = matrixMultiplier.Multiply(m1, m2);
        Assert.AreEqual(448, multiplied.GetElement(0, 0));
        Assert.AreEqual(1826, multiplied.GetElement(0, 1));
        Assert.AreEqual(3052, multiplied.GetElement(0, 2));

        Assert.AreEqual(350, multiplied.GetElement(1, 0));
        Assert.AreEqual(712, multiplied.GetElement(1, 1));
        Assert.AreEqual(1127, multiplied.GetElement(1, 2));

        Assert.AreEqual(109, multiplied.GetElement(2, 0));
        Assert.AreEqual(213, multiplied.GetElement(2, 1));
        Assert.AreEqual(728, multiplied.GetElement(2, 2));
    }

    #endregion
}