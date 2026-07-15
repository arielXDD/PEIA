using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PEIA.Web.Pages;

public class GuiaDetalleModel : PageModel
{
    private static readonly HashSet<string> Topics = new(StringComparer.OrdinalIgnoreCase)
    {
        "intro", "inventario", "reportes", "reglas", "exportacion"
    };

    public string Topic { get; private set; } = "intro";

    public IActionResult OnGet(string topic)
    {
        if (!Topics.Contains(topic)) return RedirectToPage("/Guia");
        Topic = topic.ToLowerInvariant();
        return Page();
    }
}