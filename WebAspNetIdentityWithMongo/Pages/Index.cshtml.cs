using Microsoft.AspNetCore.Mvc.RazorPages;
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global

namespace WebAspNetIdentityWithMongo.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}