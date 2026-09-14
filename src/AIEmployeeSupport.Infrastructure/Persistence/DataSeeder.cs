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
                // In production without INITIAL_ADMIN_EMAIL/PASSWORD, do not seed weak default credentials
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
            }

            if (isDevelopment)
            {
                if (!await context.Users.AnyAsync(u => u.Email == "admin@system.local"))
                {
                    var systemAdmin = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = "admin@system.local",
                        FullName = "مدير النظام",
                        Role = UserRole.Administrator,
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    systemAdmin.PasswordHash = passwordHasher.HashPassword(systemAdmin, "Admin@123");
                    context.Users.Add(systemAdmin);
                }

                if (!await context.Users.AnyAsync(u => u.Email == "employee@company.com"))
                {
                    var companyEmployee = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = "employee@company.com",
                        FullName = "موظف الدعم",
                        Role = UserRole.Employee,
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    companyEmployee.PasswordHash = passwordHasher.HashPassword(companyEmployee, "Employee123!");
                    context.Users.Add(companyEmployee);
                }
            }
        }

        await context.SaveChangesAsync();

        // If categories already exist, we've already done knowledge seeding
        if (await context.KnowledgeCategories.AnyAsync())
            return;

        var adminUser = await context.Users.FirstAsync(u => u.Role == UserRole.Administrator);
        var adminId = adminUser.Id;

        // ── Knowledge Categories (Arabic) ──────────────────────

        var categories = new List<KnowledgeCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "المصادقة", Description = "مشاكل تسجيل الدخول والمصادقة", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "الإنترنت", Description = "مشاكل الاتصال بالإنترنت", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "الهاتف المحمول", Description = "مشاكل خدمات الهاتف المحمول", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "الفوترة", Description = "مشاكل الفواتير والرصيد", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "الدفع", Description = "مشاكل الدفع والتحصيل", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "المشاكل التقنية", Description = "مشاكل تقنية عامة", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "الحسابات", Description = "إدارة الحسابات والملفات الشخصية", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "التجوال", Description = "خدمات التجوال الدولي", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "أخرى", Description = "استفسارات أخرى", IsActive = true, CreatedAt = now, UpdatedAt = now },
        };

        context.KnowledgeCategories.AddRange(categories);

        var authCategory = categories[0]; // المصادقة
        var mobileCategory = categories[2]; // الهاتف المحمول
        var techCategory = categories[5]; // المشاكل التقنية

        // ── Keywords ───────────────────────────────────────────

        var kwOtp = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = "OTP", CreatedAt = now };
        var kwSms = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = "SMS", CreatedAt = now };
        var kwAuth = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = "مصادقة", CreatedAt = now };
        var kwLogin = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = "تسجيل دخول", CreatedAt = now };
        var kwPassword = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = "كلمة مرور", CreatedAt = now };
        var kwNetwork = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = "شبكة", CreatedAt = now };
        var kwSignal = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = "إشارة", CreatedAt = now };
        var kwSim = new KnowledgeKeyword { Id = Guid.NewGuid(), Name = "SIM", CreatedAt = now };

        context.KnowledgeKeywords.AddRange(kwOtp, kwSms, kwAuth, kwLogin, kwPassword, kwNetwork, kwSignal, kwSim);

        // ── Scenario 1: Customer Cannot Receive OTP ────────────

        var scenario1Id = Guid.NewGuid();
        var scenario1 = new KnowledgeScenario
        {
            Id = scenario1Id,
            Name = "العميل لا يستطيع استقبال رمز التحقق OTP",
            Description = "العميل يواجه مشكلة في استقبال رسائل رمز التحقق (OTP) عبر الرسائل القصيرة SMS. يجب التحقق من حالة الشبكة وإعدادات الهاتف وحالة الخدمة.",
            CategoryId = authCategory.Id,
            Status = ScenarioStatus.Active,
            CreatedBy = adminId,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.KnowledgeScenarios.Add(scenario1);

        context.ScenarioKeywords.AddRange(
            new ScenarioKeyword { ScenarioId = scenario1Id, KeywordId = kwOtp.Id },
            new ScenarioKeyword { ScenarioId = scenario1Id, KeywordId = kwSms.Id },
            new ScenarioKeyword { ScenarioId = scenario1Id, KeywordId = kwAuth.Id }
        );

        context.ResolutionSteps.AddRange(
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario1Id, StepOrder = 1, StepText = "تحقق من أن رقم هاتف العميل مسجل بشكل صحيح في النظام.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario1Id, StepOrder = 2, StepText = "اطلب من العميل التحقق من وجود إشارة شبكة كافية على هاتفه.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario1Id, StepOrder = 3, StepText = "تأكد من أن صندوق الرسائل ليس ممتلئاً - اطلب من العميل حذف بعض الرسائل القديمة.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario1Id, StepOrder = 4, StepText = "أعد إرسال رمز OTP من النظام وانتظر مع العميل حتى دقيقتين.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario1Id, StepOrder = 5, StepText = "إذا استمرت المشكلة، قم بتصعيد الحالة إلى الفريق التقني مع ذكر رقم العميل ووقت المحاولة.", CreatedAt = now, UpdatedAt = now }
        );

        // ── Scenario 2: Password Reset ─────────────────────────

        var scenario2Id = Guid.NewGuid();
        var scenario2 = new KnowledgeScenario
        {
            Id = scenario2Id,
            Name = "إعادة تعيين كلمة المرور",
            Description = "العميل يريد إعادة تعيين كلمة المرور الخاصة بحسابه. يجب التحقق من هوية العميل قبل إجراء التغيير.",
            CategoryId = authCategory.Id,
            Status = ScenarioStatus.Active,
            CreatedBy = adminId,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.KnowledgeScenarios.Add(scenario2);

        context.ScenarioKeywords.AddRange(
            new ScenarioKeyword { ScenarioId = scenario2Id, KeywordId = kwPassword.Id },
            new ScenarioKeyword { ScenarioId = scenario2Id, KeywordId = kwLogin.Id },
            new ScenarioKeyword { ScenarioId = scenario2Id, KeywordId = kwAuth.Id }
        );

        context.ResolutionSteps.AddRange(
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario2Id, StepOrder = 1, StepText = "تحقق من هوية العميل عبر طرح أسئلة الأمان أو إرسال رمز تحقق.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario2Id, StepOrder = 2, StepText = "وجّه العميل إلى صفحة إعادة تعيين كلمة المرور عبر الرابط المخصص.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario2Id, StepOrder = 3, StepText = "تأكد من أن العميل يستخدم كلمة مرور قوية تتوافق مع سياسة الأمان.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario2Id, StepOrder = 4, StepText = "أكد للعميل نجاح تغيير كلمة المرور واطلب منه تجربة تسجيل الدخول.", CreatedAt = now, UpdatedAt = now }
        );

        // ── Scenario 3: Weak Signal / No Network ───────────────

        var scenario3Id = Guid.NewGuid();
        var scenario3 = new KnowledgeScenario
        {
            Id = scenario3Id,
            Name = "ضعف الإشارة أو انقطاع الشبكة",
            Description = "العميل يعاني من ضعف إشارة الشبكة أو انقطاع كامل في الخدمة. يجب التحقق من حالة الشبكة في منطقة العميل.",
            CategoryId = mobileCategory.Id,
            Status = ScenarioStatus.Active,
            CreatedBy = adminId,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.KnowledgeScenarios.Add(scenario3);

        context.ScenarioKeywords.AddRange(
            new ScenarioKeyword { ScenarioId = scenario3Id, KeywordId = kwNetwork.Id },
            new ScenarioKeyword { ScenarioId = scenario3Id, KeywordId = kwSignal.Id },
            new ScenarioKeyword { ScenarioId = scenario3Id, KeywordId = kwSim.Id }
        );

        context.ResolutionSteps.AddRange(
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario3Id, StepOrder = 1, StepText = "اسأل العميل عن موقعه الجغرافي بالتفصيل للتحقق من تغطية الشبكة.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario3Id, StepOrder = 2, StepText = "تحقق من خريطة تغطية الشبكة في النظام لمنطقة العميل.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario3Id, StepOrder = 3, StepText = "اطلب من العميل إعادة تشغيل الهاتف وإزالة وإعادة تركيب شريحة SIM.", CreatedAt = now, UpdatedAt = now },
            new ResolutionStep { Id = Guid.NewGuid(), ScenarioId = scenario3Id, StepOrder = 4, StepText = "إذا كانت هناك مشكلة في البنية التحتية، أبلغ العميل بالوقت المتوقع للإصلاح.", CreatedAt = now, UpdatedAt = now }
        );

        // ── AI Provider & Configuration (Placeholders) ─────────

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

        // ── Save All ───────────────────────────────────────────

        await context.SaveChangesAsync();
    }
}
