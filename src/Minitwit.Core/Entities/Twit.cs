using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Minitwit.Core.Entities;

/// <summary>
/// This class represents cheeps, created by the users of the _Chirp!_ application.
/// Cheeps are small messages,5 to 160 characters in length.
/// It's the only way for users to communicate with each other.
/// Cheeps hold the reactions given to them. 
/// </summary>

[Index(nameof(CheepId), IsUnique = true)]
public class Twit 
{
    [Required]
    public Guid CheepId {get; set;}
    
    [Required]
    public Guid AuthorId {get; set;}
    
    [StringLength(160, MinimumLength = 5)] [Required] 
    public required string Text { get; set; }
    
    [Required]
    public DateTime TimeStamp {get; set;}
    

}