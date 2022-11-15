namespace PooProject.Tests;
using PooProject;

public class Test_Dictionnaire
{
    [Fact]
    public void Test_Language()
    {
        Dictionnaire.GameLanguage lang = (Dictionnaire.GameLanguage)Dictionnaire.GetLanguageFromString("en");
        Assert.Equal(Dictionnaire.GameLanguage.ENGLISH, lang);
        Assert.Equal("english", Dictionnaire.GetLanguageString(lang));

        lang = (Dictionnaire.GameLanguage)Dictionnaire.GetLanguageFromString("fr");
        Assert.Equal(Dictionnaire.GameLanguage.FRENCH, lang);
        Assert.Equal("french", Dictionnaire.GetLanguageString(lang));
        
        lang = (Dictionnaire.GameLanguage)Dictionnaire.GetLanguageFromString("english");
        Assert.Equal(Dictionnaire.GameLanguage.ENGLISH, lang);

        lang = (Dictionnaire.GameLanguage)Dictionnaire.GetLanguageFromString("french");
        Assert.Equal(Dictionnaire.GameLanguage.FRENCH, lang);
        
        // test invalid input
        Dictionnaire.GameLanguage? option_lang = Dictionnaire.GetLanguageFromString("invalid");
        Assert.Equal(null, option_lang);
    }

    [Fact]
    public void Test_Dictionnaire_1()
    {
        var dico = new Dictionnaire("fr");
        Assert.Equal(Dictionnaire.GameLanguage.FRENCH, dico.Language);
        Assert.Equal("french", Dictionnaire.GetLanguageString(dico.Language));
        
        dico = new Dictionnaire("english");
        Assert.Equal(Dictionnaire.GameLanguage.ENGLISH, dico.Language);
        Assert.Equal("english", Dictionnaire.GetLanguageString(dico.Language));


        Assert.True(dico.RechDichoRecursif("TABLE"));
        Assert.True(dico.RechDichoRecursif("SPACE"));
        Assert.True(dico.RechDichoRecursif("CHAIR"));

        Assert.False(dico.RechDichoRecursif("?????????"));
        Assert.False(dico.RechDichoRecursif("afdssdfsd"));
        


    }


    
}