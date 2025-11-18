using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class ArticleRead
{
    [Parameter, Required]
    public required string Id { get; set; }
}
