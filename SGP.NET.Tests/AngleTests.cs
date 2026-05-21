using System;
using SGPdotNET.Util;

namespace SGPdotNET.Tests;

/// <summary>
/// Tests for the Angle struct: round-trips, operators, formatting, and implicit casts.
/// </summary>
[TestClass]
public sealed class AngleTests
{
    /// <summary>
    /// Verifies degree↔radian round-trip for common angles.
    /// </summary>
    [TestMethod]
    [DataRow(0.0)]
    [DataRow(90.0)]
    [DataRow(180.0)]
    [DataRow(360.0)]
    [DataRow(-45.0)]
    [DataRow(270.0)]
    public void DegreeRadianRoundTrip_PreservesValue(double degrees)
    {
        // Arrange
        var angle = Angle.FromDegrees(degrees);

        // Act
        var roundTrip = angle.Degrees;

        // Assert
        Assert.AreEqual(degrees, roundTrip, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies radian→degree→radian round-trip for common values.
    /// </summary>
    [TestMethod]
    [DataRow(0.0)]
    [DataRow(Math.PI / 2)]
    [DataRow(Math.PI)]
    [DataRow(Math.PI * 2)]
    [DataRow(-Math.PI / 4)]
    public void RadianDegreeRoundTrip_PreservesValue(double radians)
    {
        // Arrange
        var angle = Angle.FromRadians(radians);

        // Act
        var roundTrip = Angle.FromDegrees(angle.Degrees).Radians;

        // Assert
        Assert.AreEqual(radians, roundTrip, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies ToDegreesMinutesSeconds for known angles.
    /// </summary>
    [TestMethod]
    [DataRow(0.0, "0°00'0.00\"")]
    [DataRow(90.0, "90°00'0.00\"")]
    [DataRow(45.5, "45°30'0.00\"")]
    [DataRow(12.3456, "12°20'44.16\"")]
    public void ToDegreesMinutesSeconds_ProducesCorrectFormat(double degrees, string expected)
    {
        // Arrange
        var angle = Angle.FromDegrees(degrees);

        // Act
        var result = angle.ToDegreesMinutesSeconds();

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies arithmetic operators produce correct results.
    /// </summary>
    [TestMethod]
    public void ArithmeticOperators_ProduceCorrectResults()
    {
        // Arrange
        var a = Angle.FromDegrees(30);
        var b = Angle.FromDegrees(15);

        // Act
        var sum = a + b;
        var diff = a - b;

        // Assert
        Assert.AreEqual(45, sum.Degrees, TestConstants.AngleTolerance);
        Assert.AreEqual(15, diff.Degrees, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies comparison operators work correctly.
    /// </summary>
    [TestMethod]
    public void ComparisonOperators_WorkCorrectly()
    {
        // Arrange
        var a = Angle.FromDegrees(30);
        var b = Angle.FromDegrees(15);
        var c = Angle.FromDegrees(30);

        // Act & Assert
        Assert.IsTrue(a > b);
        Assert.IsTrue(b < a);
        Assert.IsTrue(a >= c);
        Assert.IsTrue(a <= c);
        Assert.IsTrue(a == c);
        Assert.IsFalse(a != c);
    }

    /// <summary>
    /// Verifies implicit cast from double assumes degrees.
    /// </summary>
    [TestMethod]
    public void ImplicitCastFromDouble_AssumesDegrees()
    {
        // Arrange
        Angle angle = 45.0;

        // Act
        var degrees = angle.Degrees;

        // Assert
        Assert.AreEqual(45.0, degrees, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies Angle.Zero has zero radians and degrees.
    /// </summary>
    [TestMethod]
    public void AngleZero_HasZeroValue()
    {
        // Act
        var radians = Angle.Zero.Radians;
        var degrees = Angle.Zero.Degrees;

        // Assert
        Assert.AreEqual(0, radians, TestConstants.AngleTolerance);
        Assert.AreEqual(0, degrees, TestConstants.AngleTolerance);
    }

    /// <summary>
    /// Verifies equality and hash code consistency.
    /// </summary>
    [TestMethod]
    public void EqualityAndHashCode_Consistent()
    {
        // Arrange
        var a = Angle.FromDegrees(45);
        var b = Angle.FromDegrees(45);

        // Act & Assert
        Assert.IsTrue(a == b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}