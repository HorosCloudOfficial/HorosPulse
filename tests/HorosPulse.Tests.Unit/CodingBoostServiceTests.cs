namespace HorosPulse.Tests.Unit;

using FluentAssertions;
using HorosPulse.Services.CodingBoost;
using Xunit;

public class CodingBoostDirectXSettingsHelperTests
{
    [Fact]
    public void Parse_SplitsSemicolonSeparatedPairs()
    {
        var parsed = CodingBoostDirectXSettingsHelper.Parse("SwapEffectUpgradeEnable=1;VRROptimizeEnable=0;");

        parsed.Should().ContainKey("SwapEffectUpgradeEnable").WhoseValue.Should().Be("1");
        parsed.Should().ContainKey("VRROptimizeEnable").WhoseValue.Should().Be("0");
    }

    [Fact]
    public void SetSetting_PreservesOtherDirectXKeys()
    {
        var merged = CodingBoostDirectXSettingsHelper.SetSetting(
            "VRROptimizeEnable=1;AutoHDREnable=0;",
            CodingBoostDirectXSettingsHelper.SwapEffectUpgradeKey,
            "1");

        merged.Should().Contain("SwapEffectUpgradeEnable=1");
        merged.Should().Contain("VRROptimizeEnable=1");
        merged.Should().Contain("AutoHDREnable=0");
    }

    [Theory]
    [InlineData("SwapEffectUpgradeEnable=1;", null, true)]
    [InlineData("SwapEffectUpgradeEnable=0;", 1, true)]
    [InlineData("SwapEffectUpgradeEnable=0;", null, false)]
    [InlineData(null, 1, true)]
    public void IsWindowedOptimizationEnabled_DetectsActiveState(string? directX, int? cache, bool expected)
    {
        CodingBoostDirectXSettingsHelper.IsWindowedOptimizationEnabled(directX, cache).Should().Be(expected);
    }
}

public class CodingBoostServiceLogicTests
{
    [Theory]
    [InlineData(1, null, true)]
    [InlineData(1, 0, true)]
    [InlineData(null, 1, true)]
    [InlineData(0, 0, false)]
    [InlineData(null, null, false)]
    public void IsGameModeEnabled_UsesAutoGameModePrimary(int? auto, int? allow, bool expected)
    {
        CodingBoostService.IsGameModeEnabled(auto, allow).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(1, false)]
    [InlineData(0, false)]
    [InlineData(null, false)]
    public void IsHagsEnabled_RequiresHwSchModeTwo(int? mode, bool expected)
    {
        CodingBoostService.IsHagsEnabled(mode).Should().Be(expected);
    }
}
