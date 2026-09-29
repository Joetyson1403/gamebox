using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using gamebox.Models;

namespace gamebox.Controllers;

/// <summary>
/// Contrôleur gérant les pages statiques d'accueil, de politique de confidentialité et la page d'erreur.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Action GET : Affiche la page d'accueil du site.
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Action GET : Affiche la page de confidentialité.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Action GET : Affiche la page d'erreur en cas d'exception non gérée avec l'identifiant de requête.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

