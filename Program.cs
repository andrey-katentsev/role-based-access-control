using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase("SafeVault"));
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.Events.OnRedirectToLogin = context =>
	{
		if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == StatusCodes.Status200OK)
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return Task.CompletedTask;
		}
		context.Response.Redirect(context.RedirectUri);
		return Task.CompletedTask;
	};
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorizationBuilder()
		.AddPolicy("RequireAdminRole", policy => policy.RequireRole("admin"))
		.AddPolicy("RequireUserRole", policy => policy.RequireRole("user"));

var app = builder.Build();

app.MapGet("/", () => "ASP.NET");
app.MapGet("/public", () => "Public: Dashboard");
app.MapGet("/secure", () => "Secure: Dashboard").RequireAuthorization();
app.MapGet("/admin", () => "Admin: Dasboard").RequireAuthorization("RequireAdminRole");
app.MapGet("/user", () => "User: Dashboard").RequireAuthorization("RequireUserRole");

app.MapPost("/login", async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> users, string email) =>
{
	var user = await users.FindByEmailAsync(email);
	if (user == null)
	{
		return Results.NotFound("User not found.");
	}

	await signInManager.SignInAsync(user, isPersistent: false);
	return Results.Ok("User logged in.");
});

var roles = new[] { "admin", "user" };

app.MapPost("/api/create-roles", async (RoleManager<IdentityRole> manager) =>
{
	foreach (var role in roles)
	{
		if (!await manager.RoleExistsAsync(role))
		{
			await manager.CreateAsync(new IdentityRole(role));
		}
	}
	return Results.Ok("Roles created.");
});

app.MapPost("/api/assign-role", async (UserManager<IdentityUser> users, string email, string role) =>
{
	var user = await users.FindByEmailAsync(email);
	if (user == null)
	{
		return Results.NotFound("User not found.");
	}

	if (!await users.IsInRoleAsync(user, role))
	{
		await users.AddToRoleAsync(user, role);
		return Results.Ok($"User {email} assigned to role {role}.");
	}

	return Results.BadRequest($"User {email} is already in role {role}.");
});

app.MapPost("/api/create-user", async (UserManager<IdentityUser> users, string email, string password) =>
{
	var mail = new MailAddress(email);
	var user = new IdentityUser { UserName = mail.User, Email = email };
	var result = await users.CreateAsync(user, password);
	if (result.Succeeded)
	{
		return Results.Ok($"User {email} created successfully.");
	}
	return Results.BadRequest(result.Errors);
});

app.Run();