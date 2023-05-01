using ServiceContracts;
using Services;
using Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.AspNetCore;
using RepositoryContracts;
using Repositories;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Middleware;
using CleanArchitecture.Core.Domain.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

//add services into IoC container
builder.Services.AddTransient<ResponseHeaderActionFilter>();
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>(); 
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonsAdderService, PersonsAdderService>();
builder.Services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
builder.Services.AddScoped<IPersonsGetterService, PersonsGetterService>();
builder.Services.AddScoped<IPersonsSorterService, PersonsSorterService>();
builder.Services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();

builder.Services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {
    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
    .ReadFrom.Services(services); //read out current app's services and make them available to serilog
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); //enforces authoriation policy (user must be authenticated) for all the action methods

    options.AddPolicy("NotAuthorized", policy =>
    {
        policy.RequireAssertion(context =>
        {
            return !context.User.Identity.IsAuthenticated;
        });
    });
});

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
});

//builder.Host.ConfigureLogging(logger =>
//{
//    logger.ClearProviders();
//    logger.AddConsole();
//});

//builder.Services.AddHttpLogging(logger =>
//{
//    logger.LoggingFields =
//    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties |
//    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
//});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseHsts();
app.UseHttpsRedirection();

//app.UseHttpLogging();

app.UseStaticFiles();

app.UseRouting(); //Identifying action method based on route
app.UseAuthentication(); //Reading Identity cookie
app.UseAuthorization(); //Validates access permissions of the user
app.MapControllers(); //Execute the filter pipiline (action + filters)

app.UseEndpoints(endpoints => {
    endpoints.MapControllerRoute(
     name: "areas",
     pattern: "{area:exists}/{controller=Home}/{action=Index}");

    //Admin/Home/Index
    //Admin

    endpoints.MapControllerRoute(
     name: "default",
     pattern: "{controller}/{action}/{id?}"
     );
});

app.Run();
