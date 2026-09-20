using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spleet.Data;
using Spleet.Models;
using Spleet.Repositories;
using Spleet.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add Entity Framework Core
builder.Services.AddDbContext<SpleetDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Core Identity
builder.Services
    .AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SpleetDbContext>()
    .AddDefaultTokenProviders();

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ISettlementRepository, SettlementRepository>();
builder.Services.AddScoped<IGroupInviteRepository, GroupInviteRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();