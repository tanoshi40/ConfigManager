using ConfigManager.Generator.Helper;

namespace ConfigManagerTest.Tests;

public class ConfigManagerTests
{
    [Flags]
    private enum AbcEnum
    {
        A = 1,
        B = 2,
        C = 4,
        All = A|B|C
    }

    [Fact]
    public void TestGenericFlaggedEnumSplit_SplitsFlaggedToEnumerable()
    {
        // Arrange
        const AbcEnum a = AbcEnum.A;
        const AbcEnum c = AbcEnum.C;
        const AbcEnum ab = AbcEnum.A | AbcEnum.B;
        const AbcEnum abcEnum = AbcEnum.A | AbcEnum.B | AbcEnum.C;
        const AbcEnum bc = AbcEnum.B | AbcEnum.C;
        const AbcEnum all = AbcEnum.All;

        // Act
        AbcEnum[] aSplit = a.SplitFlagEnum();
        AbcEnum[] cSplit = c.SplitFlagEnum();
        AbcEnum[] abSplit = ab.SplitFlagEnum();
        AbcEnum[] abcSplit = abcEnum.SplitFlagEnum();
        AbcEnum[] bcSplit = bc.SplitFlagEnum();
        AbcEnum[] allSplit = all.SplitFlagEnum();

        // Assert
        Assert.Single(aSplit);
        Assert.Single(cSplit);
        Assert.Equal(2, abSplit.Length);
        Assert.Equal(4, abcSplit.Length);
        Assert.Equal(2, bcSplit.Length);
        Assert.Equal(4, allSplit.Length);
        
        Assert.Equal(AbcEnum.A,aSplit[0]);

        Assert.Equal(AbcEnum.C,cSplit[0]);
        
        Assert.Equal(AbcEnum.A,abSplit[0]);
        Assert.Equal(AbcEnum.B,abSplit[1]);
        
        Assert.Equal(AbcEnum.A,abcSplit[0]);
        Assert.Equal(AbcEnum.B,abcSplit[1]);
        Assert.Equal(AbcEnum.C,abcSplit[2]);
        Assert.Equal(AbcEnum.All,abcSplit[3]);
        
        Assert.Equal(AbcEnum.B,bcSplit[0]);
        Assert.Equal(AbcEnum.C,bcSplit[1]);
        
        Assert.Equal(AbcEnum.A,allSplit[0]);
        Assert.Equal(AbcEnum.B,allSplit[1]);
        Assert.Equal(AbcEnum.C,allSplit[2]);
        Assert.Equal(AbcEnum.All,allSplit[3]);
        
        Assert.True(allSplit.SequenceEqual(abcSplit));
    }
}
