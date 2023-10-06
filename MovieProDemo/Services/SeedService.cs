using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProDemo.Data;
using MovieProDemo.Models.Database;
using MovieProDemo.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MovieProDemo.Services
{
    public class SeedService
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedService(IOptions<AppSettings> appSettings, ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _appSettings = appSettings.Value;
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task ManageDataAsync()
        {
            await UpdateDatabaseAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedCollections();
        }
        private async Task UpdateDatabaseAsync()
        {
            await _dbContext.Database.MigrateAsync();
        }
        private async Task SeedRolesAsync()
        {
            if (_dbContext.Roles.Any())
            {
                return;
            }

            //var adminRole = _appSettings.MovieProSettings.DefaultCredentials.Role;
            var adminRole = "Admin";
            await _roleManager.CreateAsync(new IdentityRole(adminRole));
        }
        private async Task SeedUsersAsync()
        {
            if (_userManager.Users.Any())
            {
                return;
            }

            //var credentials = _appSettings.MovieProSettings.DefaultCredentials;

            //var newUser = new IdentityUser() { Email = credentials.Email, UserName = credentials.Email,EmailConfirmed = true};
            var newUser = new IdentityUser() { Email = "iparker3964@yahoo.com", UserName = "iparker3964@yahoo.com", EmailConfirmed = true };
            //await _userManager.CreateAsync(newUser,credentials.Password);
            //await _userManager.AddToRoleAsync(newUser,credentials.Role);
            await _userManager.CreateAsync(newUser, "Abc123$");
            await _userManager.AddToRoleAsync(newUser,"Admin");
        }
        private async Task SeedCollections()
        {
            if (_dbContext.Collection.Any())
            {
                return;
            }
            _dbContext.Add(new Collection()
            {
                Name = "All",
                Description = "All imported movies will automatically be assigned to the 'All' collection."
            });
            //_dbContext.Add(new Collection(){ 
            //   Name = _appSettings.MovieProSettings.DefaultCollection.Name,
            //   Description = _appSettings.MovieProSettings.DefaultCollection.Description
            //});

            await _dbContext.SaveChangesAsync();
        }
    }
}
