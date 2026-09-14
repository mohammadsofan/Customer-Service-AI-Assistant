using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using AIEmployeeSupport.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace AIEmployeeSupport.IntegrationTests;

public class DynamicSecurityAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DynamicSecurityAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Test"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers["X-Test-UserId"].ToString();
        if (string.IsNullOrEmpty(userId))
        {
            userId = Guid.NewGuid().ToString();
        }

        var role = Request.Headers["X-Test-Role"].ToString();
        if (string.IsNullOrEmpty(role))
        {
            role = "Employee";
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Security Test User"),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class SecurityAccessTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SecurityAccessTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private (HttpClient Client, WebApplicationFactory<Program> CustomFactory) CreateSecurityTestSetup()
    {
        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, DynamicSecurityAuthHandler>("Test", _ => { });

                services.AddAuthorization(options =>
                {
                    options.AddPolicy("EmployeeOrAdmin", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireRole("Employee", "Administrator");
                    });

                    options.AddPolicy("AdminOnly", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireRole("Administrator");
                    });
                });
            });
        });

        var client = customFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        return (client, customFactory);
    }

    [Fact]
    public async Task GetQuestionById_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/support/questions/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetQuestionById_DifferentEmployee_ReturnsForbidden_IDOR_Prevented()
    {
        // Arrange
        var (client, factory) = CreateSecurityTestSetup();
        var employeeAId = Guid.NewGuid();
        var employeeBId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        // Seed employee A and question
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var employeeA = new User
            {
                Id = employeeAId,
                Email = $"empa_{employeeAId:N}@company.com",
                FullName = "Employee A",
                Role = UserRole.Employee,
                IsActive = true,
                PasswordHash = "hashed_pw",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(employeeA);

            var question = new SupportQuestion
            {
                Id = questionId,
                EmployeeId = employeeAId,
                QuestionText = "Private inquiry about compensation",
                AnswerText = "Confidential response",
                Status = QuestionStatus.Answered,
                CreatedAt = DateTime.UtcNow
            };
            db.SupportQuestions.Add(question);
            await db.SaveChangesAsync();
        }

        // Act as Employee B (different employee ID)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "token");
        client.DefaultRequestHeaders.Add("X-Test-UserId", employeeBId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Employee");

        var response = await client.GetAsync($"/api/support/questions/{questionId}");

        // Assert: Employee B MUST receive 403 Forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetQuestionById_OwnerEmployee_ReturnsOk()
    {
        // Arrange
        var (client, factory) = CreateSecurityTestSetup();
        var ownerId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var owner = new User
            {
                Id = ownerId,
                Email = $"owner_{ownerId:N}@company.com",
                FullName = "Owner Employee",
                Role = UserRole.Employee,
                IsActive = true,
                PasswordHash = "hashed_pw",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(owner);

            var question = new SupportQuestion
            {
                Id = questionId,
                EmployeeId = ownerId,
                QuestionText = "How do I reset my SIM pin?",
                AnswerText = "Follow these steps...",
                Status = QuestionStatus.Answered,
                CreatedAt = DateTime.UtcNow
            };
            db.SupportQuestions.Add(question);
            await db.SaveChangesAsync();
        }

        // Act as owner employee
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "token");
        client.DefaultRequestHeaders.Add("X-Test-UserId", ownerId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Employee");

        var response = await client.GetAsync($"/api/support/questions/{questionId}");

        // Assert: Owner receives 200 OK
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetQuestionById_Administrator_CanAccessAnyQuestion()
    {
        // Arrange
        var (client, factory) = CreateSecurityTestSetup();
        var employeeId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emp = new User
            {
                Id = employeeId,
                Email = $"emp_{employeeId:N}@company.com",
                FullName = "Employee User",
                Role = UserRole.Employee,
                IsActive = true,
                PasswordHash = "hashed_pw",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Users.Add(emp);

            var question = new SupportQuestion
            {
                Id = questionId,
                EmployeeId = employeeId,
                QuestionText = "Technical network issue reported",
                AnswerText = "Escalated to network team",
                Status = QuestionStatus.Answered,
                CreatedAt = DateTime.UtcNow
            };
            db.SupportQuestions.Add(question);
            await db.SaveChangesAsync();
        }

        // Act as Administrator
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "token");
        client.DefaultRequestHeaders.Add("X-Test-UserId", adminId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.GetAsync($"/api/support/questions/{questionId}");

        // Assert: Admin is authorized to inspect any question
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminEndpoint_EmployeeUser_ReturnsForbidden()
    {
        // Arrange
        var (client, _) = CreateSecurityTestSetup();
        var employeeId = Guid.NewGuid();

        // Act as Employee attempting to access Admin endpoint
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        client.DefaultRequestHeaders.Add("X-Test-UserId", employeeId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Employee");

        var response = await client.GetAsync("/api/admin/employees");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
