using AIEmployeeSupport.Domain.Entities;
using AIEmployeeSupport.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AIEmployeeSupport.Infrastructure.Persistence;

public static class DataSeeder
{
    public static Task SeedAsync(ApplicationDbContext context) => SeedAsync(context, null, true);

    public static async Task SeedAsync(ApplicationDbContext context, IConfiguration? configuration, bool isDevelopment)
    {
        var passwordHasher = new PasswordHasher<User>();
        var now = DateTime.UtcNow;

        var envAdminEmail = configuration?["INITIAL_ADMIN_EMAIL"];
        var envAdminPassword = configuration?["INITIAL_ADMIN_PASSWORD"];

        // In production, do NOT seed default credentials unless explicitly configured via environment variables
        if (!isDevelopment && (string.IsNullOrWhiteSpace(envAdminEmail) || string.IsNullOrWhiteSpace(envAdminPassword)))
        {
            if (!await context.Users.AnyAsync(u => u.Role == UserRole.Administrator))
            {
                return;
            }
        }
        else
        {
            var adminEmail = !string.IsNullOrWhiteSpace(envAdminEmail) ? envAdminEmail : "admin@company.com";
            var adminPassword = !string.IsNullOrWhiteSpace(envAdminPassword) ? envAdminPassword : "Admin123!";

            if (!await context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                var companyAdmin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = adminEmail,
                    FullName = "مدير النظام",
                    Role = UserRole.Administrator,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                companyAdmin.PasswordHash = passwordHasher.HashPassword(companyAdmin, adminPassword);
                context.Users.Add(companyAdmin);
                await context.SaveChangesAsync();
            }
        }

        // Leave providers, models, and AI configurations untouched.
        // Seed default fallback provider, model, and configuration ONLY if no provider exists at all.
        if (!await context.AIProviders.AnyAsync())
        {
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Administrator);
            var adminId = adminUser?.Id ?? Guid.NewGuid();

            var providerId = Guid.NewGuid();
            var modelId = Guid.NewGuid();

            var provider = new AIProvider
            {
                Id = providerId,
                Name = "OpenAI (Default)",
                ProviderType = ProviderType.OpenAI,
                EncryptedApiKey = "PLACEHOLDER_ENCRYPTED_KEY",
                IsActive = true,
                FallbackPriority = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            context.AIProviders.Add(provider);

            var model = new AIModel
            {
                Id = modelId,
                ProviderId = providerId,
                ModelName = "gpt-4o-mini",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            context.AIModels.Add(model);

            var aiConfig = new AIConfiguration
            {
                Id = Guid.NewGuid(),
                ActiveProviderId = providerId,
                ActiveModelId = modelId,
                Temperature = 0.7,
                MaxTokens = 1024,
                SimilarityThreshold = 0.7,
                SystemPrompt = @"You are an expert customer service AI assistant for our company. Your mission is to assist customer support agents by providing verified, accurate, and approved procedural solutions based strictly on the company's knowledge base.

MANDATORY RULES:
1. LANGUAGE RESTRICTION: You must generate ALL responses, summaries, procedural steps, and reasons EXCLUSIVELY in professional Modern Standard Arabic (العربية). Never output responses in English or any other language.
2. STRICT GROUNDING & NO HALLUCINATION: You must rely SOLELY and EXCLUSIVELY on the provided [SCENARIO] knowledge. Do NOT invent, assume, extrapolate, or use outside knowledge or general troubleshooting practices that are not explicitly present in the retrieved scenario.
3. RESOLUTION STEPS FIDELITY: When a relevant scenario matches the inquiry, you must extract and provide ONLY the exact resolution steps listed under ""Steps:"" for that scenario.
   - Do NOT add external steps (such as checking system updates, verifying device storage, restarting internet routers, checking connection, or contacting customer support) unless they are explicitly present under ""Steps:"".
   - Do NOT omit, reorder, or alter the meaning of the steps.
   - The number of steps returned in the ""steps"" array must match the steps in the approved scenario.
4. KEYWORDS & TOPIC MATCHING: A customer's problem matches a scenario if the problem relates to the scenario's title, description, or listed keywords. Once matched, use that scenario's approved steps.
5. ESCALATION / NO ANSWER: If no retrieved scenario addresses the inquiry, or if the provided knowledge does not contain a solution for the customer's problem, you must set ""answered"": false and explain the reason politely in Arabic.",
                UpdatedBy = adminId,
                UpdatedAt = now
            };
            context.AIConfigurations.Add(aiConfig);
            await context.SaveChangesAsync();
        }
    }
}
