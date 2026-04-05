using Xunit;

namespace PLimit.Tests;

public class ProcessDataTests
{
    private static ProcessData Make(
        string name       = "proc",
        string boosted    = "False",
        string io         = "Normal",
        string property   = "Normal",
        string affinity   = "4",
        string efficiency = "Disabled",
        string wdptb      = "Enabled")
        => new()
        {
            ProcessName = name,
            Boosted     = boosted,
            IOProperty  = io,
            Property    = property,
            Affinity    = affinity,
            Efficiency  = efficiency,
            Wdptb       = wdptb,
        };

    // ── Equals ────────────────────────────────────────────────────────────

    [Fact]
    public void Equals_IdenticalProperties_ReturnsTrue()
    {
        Assert.True(Make().Equals(Make()));
    }

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        var a = Make();
        Assert.True(a.Equals(a));
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        Assert.False(Make().Equals(null));
    }

    [Fact]
    public void Equals_WrongType_ReturnsFalse()
    {
        Assert.False(Make().Equals("not a ProcessData"));
    }

    [Theory]
    [InlineData("proc1", "proc",  "False", "False", "Normal", "Normal", "Normal", "Normal", "4",  "4",  "Disabled", "Disabled", "Enabled", "Enabled")]
    public void Equals_DifferentProcessName_ReturnsFalse(
        string n1, string n2,
        string b1, string b2,
        string io1, string io2,
        string p1, string p2,
        string a1, string a2,
        string e1, string e2,
        string w1, string w2)
    {
        var x = new ProcessData { ProcessName = n1, Boosted = b1, IOProperty = io1, Property = p1, Affinity = a1, Efficiency = e1, Wdptb = w1 };
        var y = new ProcessData { ProcessName = n2, Boosted = b2, IOProperty = io2, Property = p2, Affinity = a2, Efficiency = e2, Wdptb = w2 };
        Assert.False(x.Equals(y));
    }

    [Fact]
    public void Equals_DifferentBoosted_ReturnsFalse() =>
        Assert.False(Make(boosted: "True").Equals(Make(boosted: "False")));

    [Fact]
    public void Equals_DifferentIOProperty_ReturnsFalse() =>
        Assert.False(Make(io: "High").Equals(Make(io: "Normal")));

    [Fact]
    public void Equals_DifferentProperty_ReturnsFalse() =>
        Assert.False(Make(property: "High").Equals(Make(property: "Normal")));

    [Fact]
    public void Equals_DifferentAffinity_ReturnsFalse() =>
        Assert.False(Make(affinity: "2").Equals(Make(affinity: "8")));

    [Fact]
    public void Equals_DifferentEfficiency_ReturnsFalse() =>
        Assert.False(Make(efficiency: "Enabled").Equals(Make(efficiency: "Disabled")));

    [Fact]
    public void Equals_DifferentWdptb_ReturnsFalse() =>
        Assert.False(Make(wdptb: "Enabled").Equals(Make(wdptb: "Disabled")));

    // ── GetHashCode ───────────────────────────────────────────────────────

    [Fact]
    public void GetHashCode_EqualObjects_ReturnSameHash()
    {
        Assert.Equal(Make().GetHashCode(), Make().GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentNames_ReturnDifferentHash()
    {
        Assert.NotEqual(Make(name: "proc1").GetHashCode(), Make(name: "proc2").GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsStable_AcrossMultipleCalls()
    {
        var a = Make();
        Assert.Equal(a.GetHashCode(), a.GetHashCode());
    }
}
