using Microsoft.AspNetCore.Mvc.RazorPages;
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Global

namespace WebAspNetIdentityWithMongo.Pages;

public class PrivacyModel : PageModel
{
    private readonly ILogger<PrivacyModel> _logger;

    public PrivacyModel(ILogger<PrivacyModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}