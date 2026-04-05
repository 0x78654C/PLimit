using Xunit;

namespace PLimit.Tests;

/// <summary>
/// Tests for the internal ColumnComparer used by DoubleBufferedListView.
/// </summary>
public class ColumnComparerTests
{
    private static readonly HashSet<int> NoNumeric   = new();
    private static readonly HashSet<int> Col0Numeric = new() { 0 };

    private static ListViewItem Item(params string[] texts) => new(texts);

    // ── String column — ascending ─────────────────────────────────────────

    [Fact]
    public void Compare_StringAscending_ReturnsNegative_WhenALessThanB()
    {
        var sut = new ColumnComparer(0, asc: true, NoNumeric);

        Assert.True(sut.Compare(Item("apple"), Item("banana")) < 0);
    }

    [Fact]
    public void Compare_StringAscending_ReturnsPositive_WhenAGreaterThanB()
    {
        var sut = new ColumnComparer(0, asc: true, NoNumeric);

        Assert.True(sut.Compare(Item("zebra"), Item("ant")) > 0);
    }

    // ── String column — descending ────────────────────────────────────────

    [Fact]
    public void Compare_StringDescending_ReturnsPositive_WhenALessThanB()
    {
        var sut = new ColumnComparer(0, asc: false, NoNumeric);

        // "apple" < "banana" alphabetically; descending reverses this
        Assert.True(sut.Compare(Item("apple"), Item("banana")) > 0);
    }

    [Fact]
    public void Compare_StringDescending_ReturnsNegative_WhenAGreaterThanB()
    {
        var sut = new ColumnComparer(0, asc: false, NoNumeric);

        Assert.True(sut.Compare(Item("zebra"), Item("ant")) < 0);
    }

    // ── Case insensitivity ────────────────────────────────────────────────

    [Fact]
    public void Compare_StringColumn_IsCaseInsensitive()
    {
        var sut = new ColumnComparer(0, asc: true, NoNumeric);

        Assert.Equal(0, sut.Compare(Item("Apple"), Item("apple")));
    }

    // ── Equal values ──────────────────────────────────────────────────────

    [Fact]
    public void Compare_EqualStrings_ReturnsZero()
    {
        var sut = new ColumnComparer(0, asc: true, NoNumeric);

        Assert.Equal(0, sut.Compare(Item("same"), Item("same")));
    }

    [Fact]
    public void Compare_EqualNumbers_ReturnsZero()
    {
        var sut = new ColumnComparer(0, asc: true, Col0Numeric);

        Assert.Equal(0, sut.Compare(Item("42"), Item("42")));
    }

    // ── Numeric column — key distinction from lexicographic sort ──────────

    [Fact]
    public void Compare_NumericAscending_SortsNumericallyNotLexicographically()
    {
        // Lexicographically "9" > "10" (because '9' > '1').
        // Numerically        9  < 10   → ascending means 9 comes first.
        var sut = new ColumnComparer(0, asc: true, Col0Numeric);

        Assert.True(sut.Compare(Item("9"), Item("10")) < 0,
            "9 should sort before 10 when sorted numerically");
    }

    [Fact]
    public void Compare_NumericDescending_ReversesNumericOrder()
    {
        var sut = new ColumnComparer(0, asc: false, Col0Numeric);

        // Numerically 9 < 10; descending → 10 first → Compare("9","10") > 0
        Assert.True(sut.Compare(Item("9"), Item("10")) > 0);
    }

    [Fact]
    public void Compare_NumericAscending_LargePids_SortsCorrectly()
    {
        var sut = new ColumnComparer(0, asc: true, Col0Numeric);

        Assert.True(sut.Compare(Item("1234"), Item("56789")) < 0);
    }

    // ── Column index selection ────────────────────────────────────────────

    [Fact]
    public void Compare_SortsBySpecifiedColumnIndex_NotColumnZero()
    {
        // Sort by col 1 (PID). Row A has a smaller PID → should sort first ascending.
        var sut = new ColumnComparer(1, asc: true, NoNumeric);

        var a = Item("z_process", "100");
        var b = Item("a_process", "200");

        // Column 0 differs (z vs a), but we sort by col 1 ("100" < "200" lexicographically)
        Assert.True(sut.Compare(a, b) < 0);
    }

    // ── Missing sub-items ─────────────────────────────────────────────────

    [Fact]
    public void Compare_BothMissingSubItem_ReturnsZero()
    {
        // Sort by col 5, but both items only have 2 sub-items → both treated as ""
        var sut = new ColumnComparer(5, asc: true, NoNumeric);

        Assert.Equal(0, sut.Compare(Item("a", "b"), Item("x", "y")));
    }

    [Fact]
    public void Compare_OneMissingSubItem_EmptyStringSortsFirst_Ascending()
    {
        var sut = new ColumnComparer(1, asc: true, NoNumeric);

        var withValue    = Item("proc", "something");  // col 1 = "something"
        var withoutValue = Item("proc");               // col 1 = "" (missing)

        // "" < "something" → withoutValue should sort before withValue
        Assert.True(sut.Compare(withoutValue, withValue) < 0);
    }
}
