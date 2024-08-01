using EF.AuditLog.Samples;
using Microsoft.EntityFrameworkCore;
using EF.AuditLog.Samples.Models;
using EF.AuditLog.Samples.Persistence;
using EF.AuditLog.Samples.Persistence.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        options.AddInterceptors(new AuditSaveChangesInterceptor());
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    context.Database.ExecuteSqlRaw("DELETE FROM \"Users\";");
    context.Database.ExecuteSqlRaw("DELETE FROM \"UserRoles\";");
    context.Database.ExecuteSqlRaw("DELETE FROM \"Cars\";");
    context.Database.ExecuteSqlRaw("DELETE FROM \"AuditEntries\";");

    var car = new Car("Audi A7");

    var user = new User
    {
        Name = "John Doe",
        Password = "Password",
        AdditionalInformation = "Additional information",
        Car = car,
        CarId = car.Id,
        Address = new Address("Rudnyy", "Lenina", new Building("123", 321)),
    };

    context.Cars.Add(car);
    context.Users.Add(user);

    await context.SaveChangesAsync();

    user.Name = "Changed Name";
    user.Password = "new password";
    user.AdditionalInformation = "new additional information";
    user.Address.City = "Changed City";
    user.Address.Building!.Name = "321";
    user.Roles = new List<UserRole>()
    {
	    new()
	    {
		    Role = "Admin",
		    User = user,
		    UserId = user.Id
	    }
    };

    await context.SaveChangesAsync();
}

app.Run();