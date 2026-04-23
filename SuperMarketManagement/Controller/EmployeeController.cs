using System;
using System.Collections.Generic;
using System.Linq;
using SuperMarketManagement.Models;

namespace SuperMarketManagement.Controller;

public sealed class EmployeeController : IDisposable
{
    private readonly MarketDbContext _context = new();

    public List<User> GetEmployees()
    {
        return _context.Users
            .Where(u => u.Role == "Cashier" || u.Role == "Manager")
            .OrderBy(u => u.Id)
            .ToList();
    }

    public User? GetEmployeeById(int id)
    {
        return _context.Users.FirstOrDefault(u => u.Id == id);
    }

    public (bool IsValid, string Message) Validate(EmployeeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name) ||
            string.IsNullOrWhiteSpace(input.AgeText) ||
            string.IsNullOrWhiteSpace(input.Gender) ||
            string.IsNullOrWhiteSpace(input.Address) ||
            string.IsNullOrWhiteSpace(input.Role) ||
            string.IsNullOrWhiteSpace(input.Username) ||
            string.IsNullOrWhiteSpace(input.Password))
        {
            return (false, "Please fill all required fields.");
        }

        if (input.Role is not ("Cashier" or "Manager"))
        {
            return (false, "Only Cashier and Manager roles are allowed.");
        }

        if (!int.TryParse(input.AgeText, out var age) || age <= 0)
        {
            return (false, "Enter a valid age.");
        }

        if (!string.IsNullOrWhiteSpace(input.SalaryText) &&
            !decimal.TryParse(input.SalaryText, out _))
        {
            return (false, "Enter a valid salary amount.");
        }

        return (true, string.Empty);
    }

    public void AddEmployee(EmployeeInput input)
    {
        var user = new User
        {
            Name = input.Name,
            Age = int.Parse(input.AgeText),
            Gender = input.Gender,
            Address = input.Address,
            Role = input.Role,
            Username = input.Username,
            Password = input.Password,
            Salary = string.IsNullOrWhiteSpace(input.SalaryText)
                ? null
                : decimal.Parse(input.SalaryText)
        };

        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public bool UpdateEmployee(int id, EmployeeInput input)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return false;
        }

        user.Name = input.Name;
        user.Age = int.Parse(input.AgeText);
        user.Gender = input.Gender;
        user.Address = input.Address;
        user.Role = input.Role;
        user.Username = input.Username;
        user.Password = input.Password;
        user.Salary = string.IsNullOrWhiteSpace(input.SalaryText)
            ? null
            : decimal.Parse(input.SalaryText);

        _context.SaveChanges();
        return true;
    }

    public bool DeleteEmployee(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user is null)
        {
            return false;
        }

        _context.Users.Remove(user);
        _context.SaveChanges();
        return true;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

public sealed class EmployeeInput
{
    public string Name { get; set; } = string.Empty;
    public string AgeText { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SalaryText { get; set; } = string.Empty;
}
