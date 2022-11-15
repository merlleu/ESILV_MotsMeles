namespace PooProject.Tests;
using PooProject;

public class Test_Plateau
{
    [Fact]
    public void Test_1() {
        var dic = new Dictionnaire("fr");

        var plateau = new Plateau(dic);
        plateau.Load(5);

        Assert.Equal(plateau.DoableDirections.Length, 8);
        Assert(plateau.Width, 20);
        Assert(plateau.Height, 20);
        Assert(plateau.Words.Length, 18);

        
    }
    
}