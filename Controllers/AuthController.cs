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

public class AuthController : Controller
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View("~/Views/Account/Login.cshtml", new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/Login.cshtml", model);
        }

        var login = model.UsernameOrEmail.Trim().ToLower();
        var member = await _db.Members
            .SingleOrDefaultAsync(user => user.Username.ToLower() == login || user.Email.ToLower() == login);

        if (member is null || !BCrypt.Net.BCrypt.Verify(model.Password, member.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View("~/Views/Account/Login.cshtml", model);
        }

        await SignInAsync(member, model.RememberMe);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View("~/Views/Account/Register.cshtml", new RegisterViewModel());
    }

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
        var usernameExists = await _db.Users.AnyAsync(user => user.Username.ToLower() == username.ToLower());
        var emailExists = await _db.Users.AnyAsync(user => user.Email.ToLower() == email.ToLower());

        if (usernameExists)
        {
            ModelState.AddModelError(nameof(model.Username), "This username is already in use.");
        }

        if (emailExists)
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Account/Register.cshtml", model);
        }

        var member = new Member
        {
            Username = username,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        await SignInAsync(member, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

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
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }
}
