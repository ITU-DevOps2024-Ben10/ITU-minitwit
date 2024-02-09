using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Core.Entities;

/// <summary>
/// This class represents the user of the _Chirp!_ application. 
/// All authenticated users are represented by an instance of this class.
/// Having the ability to create cheeps, react to cheeps,
/// and follow other users.
/// </summary>


[Index(nameof(Id), IsUnique = true)]
public class Author : IdentityUser<Guid> {
    
    [StringLength(50)]
    [Required]
    public override required string UserName {get; set;}
    
    [StringLength(50)]
    [Required]
    public override required string Email {get; set;}

    public ICollection<Cheep> Cheeps { get; set; } = new List<Cheep>();
    
    public ICollection<Reaction>? Reactions { get; set; }
    
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
}