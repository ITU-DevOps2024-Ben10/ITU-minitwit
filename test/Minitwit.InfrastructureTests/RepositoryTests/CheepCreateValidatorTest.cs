using Chirp.Core.Entities;
using Minitwit.Infrastructure.Repository;

namespace Chirp.InfrastructureTest.RepositoryTests;

public class CheepCreateValidatorTest
{
    private CheepCreateValidator Validator;
    private Author TestAuthor;

    public CheepCreateValidatorTest()
    {
        Validator = new CheepCreateValidator();

        TestAuthor = new Author
        {
            UserName = "TestUserName",
            Email = "TestEmail",
        };
    }

    [Fact]
    public void CheepValidValidates()
    {
        string text = "This is a valid Cheep";
        CreateCheep createCheep = new CreateCheep(TestAuthor, text);

        Assert.True(Validator.Validate(createCheep).IsValid);
    }

    [Fact]
    public void CheepTooShortFailsValidation()
    {
        //A valid cheep is 5 characters long. This one is 4.
        string text = "1234";
        CreateCheep createCheep = new CreateCheep(TestAuthor, text);
        
        Assert.False(Validator.Validate(createCheep).IsValid);
    }

    [Fact]
    public void CheepTooLongFailsValidation()
    {
        string text = new string('x', 200);
        CreateCheep createCheep = new CreateCheep(TestAuthor, text);
        
        Assert.False(Validator.Validate(createCheep).IsValid);
    }

    [Fact]
    public void EmptyCheepFailsValidation()
    {
        string text = "";
        CreateCheep createCheep = new CreateCheep(TestAuthor, text);
        
        Assert.False(Validator.Validate(createCheep).IsValid);
    }
}

