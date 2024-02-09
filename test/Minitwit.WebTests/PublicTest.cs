// using Minitwit.Core.Entities;
// using Minitwit.Core.Repository;
// using Minitwit.Infrastructure;
// using Minitwit.Infrastructure.Repository;
// using Chirp.Web;
// using Chirp.Web.Pages;
// using Test_Utilities;
//
// namespace Minitwit.WebTests;
//
// public class PublicTest
// {
//     
//     private readonly ChirpDbContext context;
//
//     public PublicTest()
//     {
//         context = SqliteInMemoryBuilder.GetContext();
//     }
//     
//     [Fact]
//     public async void Test1()
//     {
//         // Arrange
//         var authorRepository = new AuthorRepository(context);
//         var cheepRepository = new CheepRepository(context);
//         var cheepService = new CheepService(cheepRepository, authorRepository);
//         var validator = new CheepCreateValidator();
//         PublicModel publicModel = new PublicModel(cheepService, cheepRepository, authorRepository, validator);
//
//         authorRepository.AddAuthor(new AuthorDTO()
//         {
//             AuthorId = Guid.NewGuid(), Name = "OliBolli",
//             Email = "Olibol@gmail.com", Cheeps = new List<CheepDTO>()
//         }); 
//         
//         
//         
//         // Act
//         CreateCheepDTO cheepDto = new CreateCheepDTO("OliBolli", "Test text");
//         await publicModel.CreateCheep(cheepDto);
//
//         CheepDTO returned = cheepRepository.GetCheepsByPage(1).FirstOrDefault();
//         
//         // Assert
//         Assert.Equal(cheepDto.Text, returned.Text);
//     }
//
//     [Fact]
//     public void SomeTest()
//     {
//         Assert.Equal(1,1);
//     }
// }