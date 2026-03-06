using FionetixAPI.Models;

namespace FionetixAPI.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Only seed if Employees table is empty
        if (context.Employees.Any())
            return;

        var employees = new List<Employee>
        {
            new()
            {
                Name = "Md. Hasan Ali",
                NID = "1234567890",
                Phone = "+8801712345678",
                Department = "Engineering",
                BasicSalary = 45000.00m,
                Spouse = new Spouse { Name = "Sonia Begum", NID = "12345678901234567" },
                Children = new List<Child>
                {
                    new() { Name = "Rahim Ali", DateOfBirth = new DateOnly(2015, 4, 10) },
                    new() { Name = "Sofia Ali", DateOfBirth = new DateOnly(2018, 9, 22) }
                }
            },
            new()
            {
                Name = "Moushumi Begum",
                NID = "2345678901",
                Phone = "+8801823456789",
                Department = "HR",
                BasicSalary = 42000.00m,
                Spouse = new Spouse { Name = "Kamal Hossain", NID = "23456789012345678" },
                Children = new List<Child>
                {
                    new() { Name = "Tasnim Hossain", DateOfBirth = new DateOnly(2017, 1, 15) }
                }
            },
            new()
            {
                Name = "Tanvir Ahmed",
                NID = "3456789012",
                Phone = "+8801934567890",
                Department = "Finance",
                BasicSalary = 50000.00m,
                Spouse = new Spouse { Name = "Nusrat Jahan" },
                Children = new List<Child>
                {
                    new() { Name = "Arif Ahmed", DateOfBirth = new DateOnly(2019, 6, 5) },
                    new() { Name = "Ayesha Ahmed", DateOfBirth = new DateOnly(2021, 11, 20) }
                }
            },
            new()
            {
                Name = "Nasrin Akter",
                NID = "4567890123",
                Phone = "01745678901",
                Department = "Operations",
                BasicSalary = 38000.00m
                // No spouse or children
            },
            new()
            {
                Name = "Sabbir Rahman",
                NID = "5678901234",
                Phone = "+8801856789012",
                Department = "Engineering",
                BasicSalary = 55000.00m,
                Spouse = new Spouse { Name = "Farhana Rahman", NID = "56789012345678901" },
                Children = new List<Child>
                {
                    new() { Name = "Nabil Rahman", DateOfBirth = new DateOnly(2016, 3, 25) },
                    new() { Name = "Nabila Rahman", DateOfBirth = new DateOnly(2018, 8, 12) },
                    new() { Name = "Nahin Rahman", DateOfBirth = new DateOnly(2022, 2, 1) }
                }
            },
            new()
            {
                Name = "Fatema Khanam",
                NID = "6789012345",
                Phone = "01967890123",
                Department = "Sales",
                BasicSalary = 35000.00m
                // No spouse or children
            },
            new()
            {
                Name = "Jahangir Alam",
                NID = "78901234567890123",
                Phone = "+8801378901234",
                Department = "Finance",
                BasicSalary = 60000.00m,
                Spouse = new Spouse { Name = "Rehana Alam", NID = "78901234561234567" },
                Children = new List<Child>
                {
                    new() { Name = "Farhan Alam", DateOfBirth = new DateOnly(2010, 12, 3) }
                }
            },
            new()
            {
                Name = "Roksana Parvin",
                NID = "8901234567",
                Phone = "01589012345",
                Department = "HR",
                BasicSalary = 40000.00m,
                Spouse = new Spouse { Name = "Mizanur Rahman" }
                // No children
            },
            new()
            {
                Name = "Imran Hossain",
                NID = "90123456789012345",
                Phone = "+8801690123456",
                Department = "Operations",
                BasicSalary = 47000.00m
                // No spouse or children
            },
            new()
            {
                Name = "Shahnaz Begum",
                NID = "0123456789",
                Phone = "01301234567",
                Department = "Sales",
                BasicSalary = 36000.00m,
                Children = new List<Child>
                {
                    new() { Name = "Sakib Hasan", DateOfBirth = new DateOnly(2014, 7, 18) }
                }
                // Has child but no spouse record
            }
        };

        context.Employees.AddRange(employees);

        // Seed app users (placeholder Firebase UIDs — update after Firebase user creation)
        var users = new List<AppUser>
        {
            new()
            {
                FirebaseUid = "admin-placeholder-uid-1",
                Email = "admin@fionetix.com",
                Role = "Admin"
            },
            new()
            {
                FirebaseUid = "viewer-placeholder-uid-1",
                Email = "viewer@fionetix.com",
                Role = "Viewer"
            }
        };

        context.AppUsers.AddRange(users);

        await context.SaveChangesAsync();
    }
}
