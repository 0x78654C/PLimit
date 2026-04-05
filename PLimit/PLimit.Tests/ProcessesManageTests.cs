using PLimit.Utils;
using Xunit;

namespace PLimit.Tests;

public class ProcessesManageTests
{
    private readonly ProcessesManage _sut = new();

    // ── CountBits ─────────────────────────────────────────────────────────

    [Fact]
    public void CountBits_Zero_ReturnsZero() =>
        Assert.Equal(0, _sut.CountBits(0L));

    [Fact]
    public void CountBits_One_ReturnsOne() =>
        Assert.Equal(1, _sut.CountBits(1L));

    [Fact]
    public void CountBits_PowerOfTwo_ReturnsOne() =>
        Assert.Equal(1, _sut.CountBits(128L));  // 0b1000_0000

    [Fact]
    public void CountBits_AllOnes_EightBits_ReturnsEight() =>
        Assert.Equal(8, _sut.CountBits(0xFFL));

    [Fact]
    public void CountBits_AllOnes_SixteenBits_Returns16() =>
        Assert.Equal(16, _sut.CountBits(0xFFFFL));

    [Fact]
    public void CountBits_LongMaxValue_Returns63() =>
        // long.MaxValue = 0x7FFF_FFFF_FFFF_FFFF  (63 ones, sign bit is 0)
        Assert.Equal(63, _sut.CountBits(long.MaxValue));

    [Theory]
    [InlineData(0b0001,       1)]
    [InlineData(0b0011,       2)]
    [InlineData(0b0111,       3)]
    [InlineData(0b1111,       4)]
    [InlineData(0b1010_1010,  4)]
    [InlineData(0b1111_0000,  4)]
    [InlineData(0b1111_1111,  8)]
    public void CountBits_VariousPatterns_ReturnsCorrectCount(long value, int expected) =>
        Assert.Equal(expected, _sut.CountBits(value));
}
