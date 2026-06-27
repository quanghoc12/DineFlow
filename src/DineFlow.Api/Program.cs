using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DineFlow.DataAccessObjects;
using DineFlow.BusinessObjects.Auth.Entities;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer("Server=localhost;Database=DineFlowDb;User Id=sa;Password=a1111111;TrustServerCertificate=True;");

using var context = new AppDbContext(optionsBuilder.Options);
var user = context.Users.FirstOrDefault(u => u.Username == "admin");

if (user == null)
{
    Console.WriteLine("FAIL: User 'admin' not found.");
}
else
{
    Console.WriteLine($"Found user: {user.Username}");
    Console.WriteLine($"Hash in DB: {user.PasswordHash}");
    
    bool isValid = BCrypt.Net.BCrypt.Verify("123456", user.PasswordHash);
    Console.WriteLine($"Verify('123456', Hash): {isValid}");
    
    Console.WriteLine($"Length of DB hash: {user.PasswordHash.Length}");
}



