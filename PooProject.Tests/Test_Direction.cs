namespace PooProject.Tests;
using PooProject;

public class Test_Direction
{
    [Fact]
    public void Test_Direction_NO()
    {
        var direction = new Direction("NO");
        Assert.Equal("NO", direction.ToString());
        Assert.Equal(-1, direction.X);
        Assert.Equal(-1, direction.Y);
    }

    [Fact]
    public void Test_Direction_E()
    {
        var direction = new Direction("E");
        Assert.Equal("E", direction.ToString());
        Assert.Equal(1, direction.X);
        Assert.Equal(0, direction.Y);
    }

    [Fact]
    public void Test_All_Directions() {
        var directions = new string[] { "N", "S", "E", "O", "NE", "NO", "SE", "SO" };
        var d2 = new Direction[directions.Length];

        for (int i = 0; i < directions.Length; i++) {
            var d = new Direction(directions[i]);
            d2[i] = d;
            Assert.True(d.IsValid(5));
        }

        string rslt = "";

        for (int i = 0; i < directions.Length; i++) {
            rslt += d2[i].ToString() + " ";
        }

        Assert.Equal("N S E O NE NO SE SO ", rslt);
    }

    
}