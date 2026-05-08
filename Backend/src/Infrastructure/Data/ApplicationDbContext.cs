using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Text.Json;
using Application.Interfaces;

namespace Infrastructure.Data
{
    /// <summary>
    /// Contexte de base de données principal
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly Guid _companyId;
        private readonly string _role;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
            _companyId = _currentUserService.CompanyId ?? Guid.Empty;
            _role = _currentUserService.Role ?? string.Empty;
        }

        // DbSets
        public DbSet<User>? Users { get; set; }
        public DbSet<Company>? Companies { get; set; }
        public DbSet<JobOffer>? JobOffers { get; set; }
        public DbSet<Candidate>? Candidates { get; set; }
        public DbSet<JobApplication>? JobApplications { get; set; }
        public DbSet<JobApplicationComment>? JobApplicationComments { get; set; }
        public DbSet<Interview>? Interviews { get; set; }
        public DbSet<Department>? Departments { get; set; }
        public DbSet<ActivityLog>? ActivityLogs { get; set; }
        public DbSet<Notification>? Notifications { get; set; }
        public DbSet<ApplicationRating>? ApplicationRatings { get; set; }
        public DbSet<Quiz>? Quizzes { get; set; }
        public DbSet<QuizQuestion>? QuizQuestions { get; set; }
        public DbSet<CandidateQuizResult>? CandidateQuizResults { get; set; }
        public DbSet<ContactMessage>? ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prepare value comparers for collection properties stored as JSON
            var listStringComparer = new ValueComparer<List<string>>(
                (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
                v => v == null ? 0 : v.Aggregate(0, (h, e) => ((h * 397) ^ (e == null ? 0 : e.GetHashCode()))),
                v => v == null ? new List<string>() : v.ToList());

            ValueComparer<List<WorkExperience>> workExpComparer = new ValueComparer<List<WorkExperience>>(
                (a, b) => JsonSerializer.Serialize(a) == JsonSerializer.Serialize(b),
                v => v == null ? 0 : JsonSerializer.Serialize(v).GetHashCode(),
                v => v == null ? new List<WorkExperience>() : JsonSerializer.Deserialize<List<WorkExperience>>(JsonSerializer.Serialize(v)) ?? new List<WorkExperience>());

            ValueComparer<List<Education>> educationComparer = new ValueComparer<List<Education>>(
                (a, b) => JsonSerializer.Serialize(a) == JsonSerializer.Serialize(b),
                v => v == null ? 0 : JsonSerializer.Serialize(v).GetHashCode(),
                v => v == null ? new List<Education>() : JsonSerializer.Deserialize<List<Education>>(JsonSerializer.Serialize(v)) ?? new List<Education>());

            ValueComparer<List<string>> identifiedSkillsComparer = listStringComparer;

            ValueComparer<Dictionary<string, string>> dictComparer = new ValueComparer<Dictionary<string, string>>(
                (a, b) => JsonSerializer.Serialize(a) == JsonSerializer.Serialize(b),
                v => v == null ? 0 : JsonSerializer.Serialize(v).GetHashCode(),
                v => v == null ? new Dictionary<string, string>() : JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(v)) ?? new Dictionary<string, string>());

            ValueComparer<List<InterviewQuestionRecord>> interviewQuestionComparer = new ValueComparer<List<InterviewQuestionRecord>>(
                (a, b) => JsonSerializer.Serialize(a) == JsonSerializer.Serialize(b),
                v => v == null ? 0 : JsonSerializer.Serialize(v).GetHashCode(),
                v => v == null ? new List<InterviewQuestionRecord>() : JsonSerializer.Deserialize<List<InterviewQuestionRecord>>(JsonSerializer.Serialize(v)) ?? new List<InterviewQuestionRecord>());

            // Configuration des entités
            ConfigureUser(modelBuilder);
            ConfigureCompany(modelBuilder);
            ConfigureCandidate(modelBuilder);
            ConfigureJobOffer(modelBuilder);
            ConfigureJobApplication(modelBuilder, listStringComparer, workExpComparer, educationComparer, identifiedSkillsComparer, dictComparer, interviewQuestionComparer);
            ConfigureJobApplicationComment(modelBuilder);
            ConfigureApplicationRating(modelBuilder);
            ConfigureInterview(modelBuilder);
            ConfigureDepartment(modelBuilder);

            ConfigureActivityLog(modelBuilder);
            ConfigureNotification(modelBuilder);
            ConfigureQuiz(modelBuilder);
            ConfigureQuizQuestion(modelBuilder);
            ConfigureCandidateQuizResult(modelBuilder);
            ConfigureContactMessage(modelBuilder);

            // Seed data initial with deterministic values (avoid DateTime.UtcNow / Guid.NewGuid() in HasData)
            SeedInitialData(modelBuilder);
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Role)
                    .HasConversion<int>(); // Store enum as int

                // Relation avec Company (nullable pour les candidats)
                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Users)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Global Query Filter for Users (Multi-tenancy)
                // Unauthenticated (login/register) and SuperAdmin see everyone.
                // Authenticated non-SuperAdmin users only see their own company.
                entity.HasQueryFilter(u => _role == string.Empty || _role == "SuperAdmin" || (u.CompanyId == _companyId && _companyId != Guid.Empty));
            });
        }

        private void ConfigureCandidate(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Candidate>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Index unique par entreprise (un même email peut exister dans deux entreprises différentes)
                entity.HasIndex(e => new { e.Email, e.CompanyId })
                    .IsUnique();

                // Relation avec Company
                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Global Query Filter for Candidates (Multi-tenancy)
                entity.HasQueryFilter(c => _role == string.Empty || _role == "SuperAdmin" || c.CompanyId == _companyId);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);
            });
        }

        private void ConfigureCompany(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ContactEmail)
                    .HasMaxLength(255);

                entity.Property(e => e.Industry)
                    .HasMaxLength(100);

                entity.Property(e => e.Size)
                    .HasMaxLength(50);

                entity.Property(e => e.PrimaryColor)
                    .HasMaxLength(7)
                    .HasDefaultValue("#FFD700");

                entity.Property(e => e.SecondaryColor)
                    .HasMaxLength(7)
                    .HasDefaultValue("#000000");



                // Index
                entity.HasIndex(e => e.Name);
            });
        }

        private void ConfigureJobOffer(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobOffer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .HasConversion<int>(); // Store enum as int

                entity.Property(e => e.Status)
                    .HasConversion<int>(); // Store enum as int

                entity.Property(e => e.RemotePolicy)
                    .HasConversion<int>()
                    .IsRequired(false);

                entity.Property(e => e.ExperienceLevel)
                    .HasConversion<int>()
                    .IsRequired(false);

                var skillsProp = entity.Property(e => e.Skills);
                skillsProp.HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v) : "[]",
                    v => !string.IsNullOrEmpty(v)
                        ? JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()
                        : new List<string>())
                    .IsRequired(false);
                skillsProp.Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
                    v => v == null ? 0 : v.Aggregate(0, (h, e) => ((h * 397) ^ (e == null ? 0 : e.GetHashCode()))),
                    v => v == null ? new List<string>() : v.ToList()));

                // Relations
                entity.HasOne(e => e.Company)
                    .WithMany(c => c.JobOffers)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany(u => u.CreatedJobOffers)
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configuration des owned entities with null-safe conversion
                entity.OwnsOne(e => e.FormConfig, fc =>
                {
                    var customFieldsProp = fc.Property(p => p.CustomFields);
                    customFieldsProp.HasConversion(
                        v => v != null ? JsonSerializer.Serialize(v) : "[]",
                        v => !string.IsNullOrEmpty(v)
                            ? JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()
                            : new List<string>());
                    customFieldsProp.Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
                        v => v == null ? 0 : v.Aggregate(0, (h, e) => ((h * 397) ^ (e == null ? 0 : e.GetHashCode()))),
                        v => v == null ? new List<string>() : v.ToList()));

                    var requiredDocsProp = fc.Property(p => p.RequiredDocuments);
                    requiredDocsProp.HasConversion(
                        v => v != null ? JsonSerializer.Serialize(v) : "[]",
                        v => !string.IsNullOrEmpty(v)
                            ? JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()
                            : new List<string>());
                    requiredDocsProp.Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
                        v => v == null ? 0 : v.Aggregate(0, (h, e) => ((h * 397) ^ (e == null ? 0 : e.GetHashCode()))),
                        v => v == null ? new List<string>() : v.ToList()));
                });

                entity.OwnsOne(e => e.DisplayConfig);

                // Index pour performance
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PublishedAt);

                // Global Query Filter for JobOffers (Multi-tenancy)
                // Unauthenticated and SuperAdmin bypass. Others filtered by CompanyId.
                entity.HasQueryFilter(j => _role == string.Empty || _role == "SuperAdmin" || j.CompanyId == _companyId);
            });
        }

        private void ConfigureJobApplication(ModelBuilder modelBuilder, ValueComparer<List<string>> listStringComparer, ValueComparer<List<WorkExperience>> workExpComparer, ValueComparer<List<Education>> educationComparer, ValueComparer<List<string>> identifiedSkillsComparer, ValueComparer<Dictionary<string, string>> dictComparer, ValueComparer<List<InterviewQuestionRecord>> interviewQuestionComparer)
        {
            modelBuilder.Entity<JobApplication>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Status)
                    .HasConversion<int>(); // Store enum as int

                // Relations
                entity.HasOne(e => e.JobOffer)
                    .WithMany(j => j.Applications)
                    .HasForeignKey(e => e.JobOfferId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Candidate)
                    .WithMany(c => c.Applications)
                    .HasForeignKey(e => e.CandidateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ReviewedBy)
                    .WithMany()
                    .HasForeignKey(e => e.ReviewedById)
                    .OnDelete(DeleteBehavior.Restrict);

                // Conversion des objets complexes en JSON with null-safe handling
                var customFieldsProp = entity.Property(e => e.CustomFieldsData);
                customFieldsProp.HasConversion(
                    v => v != null ? JsonSerializer.Serialize(v) : "{}",
                    v => !string.IsNullOrEmpty(v)
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(v) ?? new Dictionary<string, string>()
                        : new Dictionary<string, string>());
                customFieldsProp.Metadata.SetValueComparer(dictComparer);

                entity.OwnsOne(e => e.AIAnalysis, ai =>
                {
                    ai.OwnsOne(a => a.ExtractedData, ed =>
                    {
                        var workExpProp = ed.Property(p => p.WorkExperiences);
                        workExpProp.HasConversion(
                            v => v != null ? JsonSerializer.Serialize(v) : "[]",
                            v => !string.IsNullOrEmpty(v)
                                ? JsonSerializer.Deserialize<List<WorkExperience>>(v) ?? new List<WorkExperience>()
                                : new List<WorkExperience>());
                        workExpProp.Metadata.SetValueComparer(workExpComparer);

                        var educationsProp = ed.Property(p => p.Educations);
                        educationsProp.HasConversion(
                            v => v != null ? JsonSerializer.Serialize(v) : "[]",
                            v => !string.IsNullOrEmpty(v)
                                ? JsonSerializer.Deserialize<List<Education>>(v) ?? new List<Education>()
                                : new List<Education>());
                        educationsProp.Metadata.SetValueComparer(educationComparer);

                        var skillsProp = ed.Property(p => p.Skills);
                        skillsProp.HasConversion(
                            v => v != null ? JsonSerializer.Serialize(v) : "[]",
                            v => !string.IsNullOrEmpty(v)
                                ? JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()
                                : new List<string>());
                        skillsProp.Metadata.SetValueComparer(listStringComparer);
                    });

                    var identifiedSkillsProp = ai.Property(a => a.IdentifiedSkills);
                    identifiedSkillsProp.HasConversion(
                        v => v != null ? JsonSerializer.Serialize(v) : "[]",
                        v => !string.IsNullOrEmpty(v)
                            ? JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()
                            : new List<string>());
                    identifiedSkillsProp.Metadata.SetValueComparer(identifiedSkillsComparer);

                    var strengthsProp = ai.Property(a => a.Strengths);
                    strengthsProp.HasConversion(
                        v => v != null ? JsonSerializer.Serialize(v) : "[]",
                        v => !string.IsNullOrEmpty(v)
                            ? JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()
                            : new List<string>());
                    strengthsProp.Metadata.SetValueComparer(listStringComparer);

                    var weaknessesProp = ai.Property(a => a.Weaknesses);
                    weaknessesProp.HasConversion(
                        v => v != null ? JsonSerializer.Serialize(v) : "[]",
                        v => !string.IsNullOrEmpty(v)
                            ? JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()
                            : new List<string>());
                    weaknessesProp.Metadata.SetValueComparer(listStringComparer);

                    var interviewQuestionsProp = ai.Property(a => a.InterviewQuestions);
                    interviewQuestionsProp.HasConversion(
                        v => v != null ? JsonSerializer.Serialize(v) : "[]",
                        v => !string.IsNullOrEmpty(v)
                            ? JsonSerializer.Deserialize<List<InterviewQuestionRecord>>(v) ?? new List<InterviewQuestionRecord>()
                            : new List<InterviewQuestionRecord>());
                    interviewQuestionsProp.Metadata.SetValueComparer(interviewQuestionComparer);
                });

                // Index pour performance
                entity.HasIndex(e => e.JobOfferId);
                entity.HasIndex(e => e.CandidateId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.AppliedAt);

                // Global Query Filter for JobApplications (Multi-tenancy)
                // Unauthenticated and SuperAdmin bypass. Others filtered by CompanyId.
                entity.HasQueryFilter(ja => _role == string.Empty || _role == "SuperAdmin" || ja.JobOffer!.CompanyId == _companyId);
            });
        }

        private void ConfigureJobApplicationComment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JobApplicationComment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Content)
                    .IsRequired();

                entity.HasOne(e => e.JobApplication)
                    .WithMany(j => j.Comments)
                    .HasForeignKey(e => e.JobApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Author)
                    .WithMany()
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Global Query Filter: SuperAdmin sees all, others see comments for applications belonging to their company
                entity.HasQueryFilter(c => _role == string.Empty || _role == "SuperAdmin" || c.JobApplication.JobOffer!.CompanyId == _companyId);
            });
        }

        private void ConfigureApplicationRating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationRating>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Score).IsRequired();

                entity.HasOne(e => e.JobApplication)
                    .WithMany(j => j.Ratings)
                    .HasForeignKey(e => e.JobApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Recruiter)
                    .WithMany()
                    .HasForeignKey(e => e.RecruiterId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Global Query Filter: Multi-tenancy
                entity.HasQueryFilter(r => _role == string.Empty || _role == "SuperAdmin" || r.JobApplication.JobOffer!.CompanyId == _companyId);
            });
        }

        private void ConfigureInterview(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Interview>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(e => e.JobApplication)
                    .WithMany()
                    .HasForeignKey(e => e.JobApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Recruiter)
                    .WithMany()
                    .HasForeignKey(e => e.RecruiterId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Global Query Filter: SuperAdmin sees all, others see interviews for applications belonging to their company
                entity.HasQueryFilter(i => _role == string.Empty || _role == "SuperAdmin" || i.JobApplication!.JobOffer!.CompanyId == _companyId);

                entity.Property(e => e.Status)
                    .HasConversion<int>();
            });
        }

        private void ConfigureNotification(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.JobApplication)
                    .WithMany(j => j.Notifications)
                    .HasForeignKey(e => e.JobApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(n => _role == string.Empty || _role == "SuperAdmin" || n.JobApplication!.JobOffer!.CompanyId == _companyId);
            });
        }

        private void ConfigureDepartment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Departments)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Global Query Filter
                entity.HasQueryFilter(d => _role == string.Empty || _role == "SuperAdmin" || d.CompanyId == _companyId);
            });
        }


        private void ConfigureActivityLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired();

                // Global Query Filter
                entity.HasQueryFilter(l => _role == string.Empty || _role == "SuperAdmin" || l.CompanyId == _companyId);
            });
        }

        private void ConfigureQuiz(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.JobOffer)
                    .WithMany() // Or Add ICollection<Quiz> to JobOffer if needed
                    .HasForeignKey(e => e.JobOfferId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Multi-tenancy filter
                entity.HasQueryFilter(q => _role == string.Empty || _role == "SuperAdmin" || (q.JobOffer != null && q.JobOffer.CompanyId == _companyId));
            });
        }

        private void ConfigureQuizQuestion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuizQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired();
                entity.HasOne(e => e.Quiz)
                    .WithMany(q => q.Questions)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Matching multi-tenancy filter to avoid EF validation warning
                entity.HasQueryFilter(q => _role == string.Empty || _role == "SuperAdmin" || (q.Quiz != null && q.Quiz.JobOffer != null && q.Quiz.JobOffer.CompanyId == _companyId));
            });
        }

        private void ConfigureCandidateQuizResult(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CandidateQuizResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.JobApplication)
                    .WithMany() // Or add navigation to JobApplication
                    .HasForeignKey(e => e.JobApplicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Quiz)
                    .WithMany()
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Multi-tenancy filter
                entity.HasQueryFilter(r => _role == string.Empty || _role == "SuperAdmin" || (r.JobApplication != null && r.JobApplication.JobOffer != null && r.JobApplication.JobOffer.CompanyId == _companyId));
            });
        }

        private void ConfigureContactMessage(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Status).HasConversion<int>();
            });
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Use a deterministic seed timestamp to avoid EF model changes each build
            var seedCreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);



            // Société par défaut - NeoLedge
            var defaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            modelBuilder.Entity<Company>().HasData(new
            {
                Id = defaultCompanyId,
                Name = "NeoLedge Demo Company",
                Description = "Société de démonstration pour la plateforme",
                ContactEmail = "contact@neoledge.com",
                PrimaryColor = "#FFD700",
                IsActive = true,
                Status = CompanyStatus.Approved,
                CreatedAt = seedCreatedAt
            });

            // Seed Departments for Demo
            var techDeptId = Guid.Parse("d001d001-d001-d001-d001-d001d001d001");
            var hrDeptId = Guid.Parse("d002d002-d002-d002-d002-d002d002d002");
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = techDeptId, Name = "Tech & Innovation", CompanyId = defaultCompanyId },
                new Department { Id = hrDeptId, Name = "Human Resources", CompanyId = defaultCompanyId }
            );

            // Base64 encoded SHA256 hash for "Admin@123"
            var superAdminPasswordHash = "6G94qKPK8LYNjnTllCqm2G3BUM08AzOK7yW30tfjrMc=";

            // Super Admin par défaut (Global)
            var superAdminId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = superAdminId,
                FirstName = "Admin",
                LastName = "NeoLedge",
                Email = "admin@neoledge.com",
                PasswordHash = superAdminPasswordHash,
                Role = UserRole.SuperAdmin,
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = seedCreatedAt,
                CompanyId = null
            });

            // Company Admin (Local)
            var adminId = Guid.Parse("10001000-1000-1000-1000-100010001000");
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                FirstName = "Houssem",
                LastName = "Recruiter",
                Email = "admin.hr@neoledge.com",
                PasswordHash = superAdminPasswordHash,
                Role = UserRole.CompanyAdmin,
                JobTitle = "Chief Talent Officer",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = seedCreatedAt,
                CompanyId = defaultCompanyId,
                DepartmentId = hrDeptId
            });

            // Recruiter Aya
            var rec1Id = Guid.Parse("10001000-1000-1000-1000-100010001001");
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = rec1Id,
                FirstName = "Aya",
                LastName = "Talent",
                Email = "aya.t@neoledge.com",
                PasswordHash = superAdminPasswordHash,
                Role = UserRole.Recruiter,
                JobTitle = "Technical Recruiter Lead",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = seedCreatedAt,
                CompanyId = defaultCompanyId,
                DepartmentId = techDeptId
            });

            // Recruiter Mehdi
            var rec2Id = Guid.Parse("10001000-1000-1000-1000-100010001002");
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = rec2Id,
                FirstName = "Mehdi",
                LastName = "Sourcing",
                Email = "mehdi.s@neoledge.com",
                PasswordHash = superAdminPasswordHash,
                Role = UserRole.Recruiter,
                JobTitle = "Sourcing Consultant",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = seedCreatedAt,
                CompanyId = defaultCompanyId,
                DepartmentId = techDeptId
            });
        }

        // Override SaveChanges pour mettre à jour automatiquement les timestamps
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}
