using AdminUAT.Data;
using AdminUAT.Dependencias;
using AdminUAT.Dependencias.EmailModel;
using AdminUAT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AdminUAT
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    //Configuration.GetConnectionString("localUATAdmin")));
                    Configuration.GetConnectionString("LoginUAT_1.15")));
                    //Configuration.GetConnectionString("LoginUAT"))); //produccion
                    //services.AddDefaultIdentity<IdentityUser>()

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddDbContext<NewUatDbContext>(options =>
                options.UseSqlServer(
                    //Configuration.GetConnectionString("localUAT")));
                    Configuration.GetConnectionString("TestUAT_1.15")));
                    //Configuration.GetConnectionString("NewUATPro48"))); //produccion

            services.AddDbContext<AgendaDbContext>(options =>
                options.UseSqlServer(
                    //Configuration.GetConnectionString("AgendaLocal")));
                    //Configuration.GetConnectionString("LocalAgenda")));
                    Configuration.GetConnectionString("AgendaUatTest_1.15"))); //Test
                    //Configuration.GetConnectionString("UATCitas_Produccion_1.48"))); //Produccion

            //Add dependencias
            services.AddTransient<ISubProceso, SubProceso>();
            services.AddTransient<IQueryDenuncias, QueryDenuncias>();
            services.AddTransient<IEnvioCorreo, EnvioCorreo>();

            services.Configure<EmailSenderOptions>(Configuration.GetSection("EmailSenderOptions"));

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true; //	Indica si una cookie es accesible mediante script de cliente.
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); //Controla cuánto tiempo el vale de autenticación que se almacena en la cookie se conserva válida desde el punto en que se crea. El valor predeterminado es 14 días.
                // If the LoginPath isn't set, ASP.NET Core defaults 
                // the path to /Account/Login.
                options.LoginPath = "/Identity/Account/Login"; //Cuando un usuario no está autorizado, se le redirige a esta ruta de acceso al inicio de sesión. El valor predeterminado es "/Account/Login";
                // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
                // the path to /Account/AccessDenied.
                options.AccessDeniedPath = "/Account/AccessDenied"; //Informa al controlador que debe cambiar en una salida 403 Prohibido código de estado en un redireccionamiento 302 en la ruta de acceso especificada.
                options.SlidingExpiration = true;
            });

            //Policy
            services.AddCors(options => {
                options.AddPolicy("Maps",
                    builder =>
                    {
                        builder.WithOrigins("http://172.15.24.61");
                    });
            });

            services.AddMvc(o =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                o.Filters.Add(new AuthorizeFilter(policy));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("Maps");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                name: "Bot",
                template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
