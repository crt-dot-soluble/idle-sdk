using IdleSdk.Core.Skills;
using Xunit;

namespace IdleSdk.Core.Tests.Skills;

public sealed class ExponentialXpCurveTests
{
    [Fact]
    public void GetTotalXpForLevel_IncreasesExponentially()
    {
        var curve = new ExponentialXpCurve(60, 1.5);

        Assert.Equal(0, curve.GetTotalXpForLevel(1));
        Assert.Equal(60, curve.GetTotalXpForLevel(2));
        Assert.Equal(150, curve.GetTotalXpForLevel(3));
        Assert.Equal(285, curve.GetTotalXpForLevel(4));
    }

    [Fact]
    public void GetTotalXpForLevel_ValidatesArguments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExponentialXpCurve(-1, 1.2));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExponentialXpCurve(10, 0.9));

        var curve = new ExponentialXpCurve(10, 1.1);
        Assert.Throws<ArgumentOutOfRangeException>(() => curve.GetTotalXpForLevel(0));
    }
}
