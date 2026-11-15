using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TableView3D.Models;

namespace TableView3D.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;

    public AccountController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _users.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _users.AddToRoleAsync(user, "Customer");
            await _signIn.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Search", "Reservations");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signIn.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
            return RedirectToAction("Search", "Reservations");
        }

        ModelState.AddModelError("", result.IsLockedOut
            ? "Too many attempts. Try again in a few minutes."
            : "Invalid email or password.");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}