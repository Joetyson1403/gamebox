using System.Security.Claims;
using BCrypt.Net;
using gamebox.Data;
using gamebox.Models;
using gamebox.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gamebox.Controllers;

/// <summary>
/// Contrôleur dédié à la sécurité et à l'authentification :
/// inscription des nouveaux membres, connexion par mot de passe haché (BCrypt),
/// gestion des cookies de session et déconnexion.
/// </summary>
public class AuthController : Controller
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Action GET : Affiche la page de connexion.
    /// </summary>
    /// <param name="returnUrl">URL vers laquelle rediriger l'utilisateur après une connexion réussie.</param>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View("~/Views/Account/Login.cshtml", new LoginViewModel());
    }

    /// <summary>
    /// Action POST : Traite le formulaire de connexion.
    /// Vérifie le mot de passe via BCrypt et initialise le cookie de session si les identifiants sont valides.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        // Validation des règles du ViewModel (champs requis, formats)
        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/Login.cshtml", model);
        }

        var login = model.UsernameOrEmail.Trim().ToLower();
        // Recherche de l'utilisateur par nom d'utilisateur ou par adresse email
        var member = await _db.Members
            .SingleOrDefaultAsync(user => user.Username.ToLower() == login || user.Email.ToLower() == login);

        // Vérification sécurisée du mot de passe avec le hash BCrypt stocké en base
        if (member is null || !BCrypt.Net.BCrypt.Verify(model.Password, member.Password))
        {
            ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe incorrect.");
            return View("~/Views/Account/Login.cshtml", model);
        }

        // Création du cookie d'authentification ASP.NET Core
        await SignInAsync(member, model.RememberMe);

        // Redirection vers l'URL précédente si elle est locale et sécurisée
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Action GET : Affiche le formulaire d'inscription pour un nouveau membre.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View("~/Views/Account/Register.cshtml", new RegisterViewModel());
    }

    /// <summary>
    /// Action POST : Traite la création d'un nouveau compte membre.
    /// Vérifie l'unicité du pseudo et de l'email, hache le mot de passe et connecte automatiquement l'utilisateur.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/Register.cshtml", model);
        }

        var username = model.Username.Trim();
        var email = model.Email.Trim();

        // Contrôle d'unicité en base de données
        var usernameExists = await _db.Users.AnyAsync(user => user.Username.ToLower() == username.ToLower());
        var emailExists = await _db.Users.AnyAsync(user => user.Email.ToLower() == email.ToLower());

        if (usernameExists)
        {
            ModelState.AddModelError(nameof(model.Username), "Ce nom d'utilisateur est déjà utilisé.");
        }

        if (emailExists)
        {
            ModelState.AddModelError(nameof(model.Email), "Cette adresse email est déjà enregistrée.");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/Register.cshtml", model);
        }

        // Création de l'entité Member avec mot de passe haché par BCrypt (jamais en texte brut)
        var member = new Member
        {
            Username = username,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();

        // Connexion immédiate du nouvel utilisateur après inscription
        await SignInAsync(member, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Action POST : Déconnecte l'utilisateur en supprimant le cookie d'authentification.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Méthode privée utilitaire : Génère les Claims (données d'identité)
    /// et émet le cookie de session chiffré dans le navigateur.
    /// </summary>
    private async Task SignInAsync(Member member, bool isPersistent)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
            new Claim(ClaimTypes.Name, member.Username),
            new Claim(ClaimTypes.Email, member.Email),
            new Claim(ClaimTypes.Role, nameof(Member))
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14) // Persistant pendant 14 jours si "Se souvenir de moi"
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }
}
