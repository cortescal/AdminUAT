using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AdminUAT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AdminUAT.Data;
using System.Linq;
using AdminUAT.Models.MinisterioPublico;
using AdminUAT.Models.LoginUat;

namespace AdminUAT.Areas.Identity.Pages.Account
{
    //[AllowAnonymous]
    [Authorize(Roles = "Root")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly NewUatDbContext _contextUAT;

        private readonly ApplicationDbContext _context;

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger, RoleManager<IdentityRole> roleManager, NewUatDbContext contextUAT,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _contextUAT = contextUAT;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /*
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            */

            [Required(ErrorMessage = "Nombre Requerido")]
            [Display(Name = "Nombre(s)")]
            public string Nombre { get; set; }

            [Required(ErrorMessage = "Apellido Paterno Requerido")]
            [Display(Name = "Apellido Paterno")]
            public string PrimerApellido { get; set; }

            [Required(ErrorMessage = "Apellido Materno Requerido")]
            [Display(Name = "Apellido Materno")]
            public string SegundoApellido { get; set; }

            [Required(ErrorMessage = "El Rol es Requerido")]
            [Display(Name = "Rol")]
            public int Rol { get; set; }

            [Required(ErrorMessage = "El campo es Requerido")]
            [Display(Name = "UR")]
            public long UR { get; set; }

            [Display(Name = "Fiscalia Correspondiente")]
            public Guid? FiscaliaId { get; set; }
            
            [Display(Name = "Fiscalia Correspondiente")]
            public Guid? FiscaliaMpId { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ViewData["urs"] = _contextUAT.UR.Where(x => x.Id != 1).OrderBy(x => x.Nombre).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                    { 
                        UserName = Input.Email, 
                        Email = Input.Email, 
                        Nombre = Input.Nombre, 
                        PrimerApellido = Input.PrimerApellido, 
                        SegundoApellido = Input.SegundoApellido 
                    };
                user.AltaSistema = DateTime.Now;
                user.Estatus = true;
                user.Rol = Input.Rol;
                if (user.Rol == 2)
                {
                    MP obj = new MP()
                    {
                        Nombre = user.Nombre,
                        PrimerApellido = user.PrimerApellido,
                        SegundoApellido = user.SegundoApellido,
                        Activo = true,
                        Stock = 0,
                        Resuelto = 0,
                        AltaSistema = DateTime.Now,
                        URId = Input.UR,
                        FiscaliaId=Input.FiscaliaMpId
                    };

                    _contextUAT.Add(obj);
                    await _contextUAT.SaveChangesAsync();
                    user.MatchMP = obj.Id;
                }
                else
                {
                    user.MatchMP = 0;
                }

                var result = await _userManager.CreateAsync(user, "Uat@.2019*");

                if (result.Succeeded)
                {
                    //Asignación de Role
                    string nombreRol = "";
                    if (user.Rol == 1) { nombreRol = "Root"; }
                    else if (user.Rol == 2) { nombreRol = "MP"; }
                    else if (user.Rol == 3) { nombreRol = "FiscReg"; }
                    else if (user.Rol == 4) { nombreRol = "FiscMet"; }
                    else if (user.Rol == 5) { nombreRol = "AEI"; }
                    else if (user.Rol == 6) { nombreRol = "FiscEsp"; }

                    await _roleManager.CreateAsync(new IdentityRole(nombreRol));
                    await _userManager.AddToRoleAsync(user, nombreRol);
                    //user = await _userManager.GetUserAsync(HttpContext.User);

                    //_logger.LogInformation("User created a new account with password.");

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //return LocalRedirect(returnUrl);
                    if (user.Rol == 6)
                    {
                        var data = await _userManager.FindByEmailAsync(user.Email);
                        var obj = new RolFiscalia() {
                            UserId = Guid.Parse(data.Id),
                            FiscaliaId=Input.FiscaliaId
                        };

                        await _context.RolFiscalias.AddAsync(obj);
                        await _context.SaveChangesAsync();
                    }

                    return LocalRedirect("~/Usuarios");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

    }
}
