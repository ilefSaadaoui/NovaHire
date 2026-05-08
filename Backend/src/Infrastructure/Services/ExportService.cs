using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using ClosedXML.Excel;
using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services
{
    public class ExportService : IExportService
    {
        // ─── Palette ────────────────────────────────────────────────────────────
        private const string PrimaryBlue   = "#1D4ED8";
        private const string AccentBlue    = "#3B82F6";
        private const string SuccessGreen  = "#10B981";
        private const string WarningAmber  = "#F59E0B";
        private const string DangerRed     = "#EF4444";
        private const string TextDark      = "#0F172A";
        private const string TextMuted     = "#64748B";
        private const string SlateBg       = "#F8FAFC";
        private const string BorderLight   = "#E2E8F0";

        // ─── Analytics Snapshot ─────────────────────────────────────────────────
        private sealed class ReportAnalyticsSnapshot
        {
            public int    TotalCandidates    { get; init; }
            public int    AnalyzedCandidates { get; init; }
            public int    AverageScore       { get; init; }
            public int    ConversionRate     { get; init; }
            public int    EliteCount         { get; init; }
            public int    QualifiedCount     { get; init; }
            public int    LowCount           { get; init; }
            public double AverageDaysToApply { get; init; }

            public List<(string Label, string Value, string Color)>  Kpis          { get; init; } = new();
            public List<(string Name,  int Count)>                   PipelineData  { get; init; } = new();
            public List<(string Name,  int Count)>                   TopSkills     { get; init; } = new();
        }

        public ExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ════════════════════════════════════════════════════════════════════════
        //  PDF — JOB OFFER REPORT
        // ════════════════════════════════════════════════════════════════════════
        public async Task<byte[]> GenerateJobOfferReportPdfAsync(JobOffer offer, IEnumerable<JobApplication> applications)
        {
            return await Task.Run(() =>
            {
                try
                {
                    QuestPDF.Settings.License = LicenseType.Community;
                    
                    var apps = (applications ?? Enumerable.Empty<JobApplication>()).ToList();
                    var analytics = BuildAnalytics(apps);
                    var sorted = apps.OrderByDescending(a => a.AiScore ?? 0).ToList();

                    var document = Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(0);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Helvetica).FontColor(TextDark));
                            
                            // --- HEADER ---
                            page.Header().Background(PrimaryBlue).Padding(30).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("RAPPORT DE RECRUTEMENT").FontSize(10).SemiBold().FontColor(Colors.White).LetterSpacing(0.1f);
                                    col.Item().Text(offer.Title.ToUpper()).FontSize(24).ExtraBold().FontColor(Colors.White);
                                    col.Item().PaddingTop(5).Text($"{offer.Department?.Name ?? "General"} | {DateTime.Now:MMMM yyyy}").FontSize(10).FontColor(BorderLight);
                                });
                                row.ConstantItem(60).AlignCenter().AlignMiddle().Height(60).Width(60).Background(Colors.White).Padding(10).Text("NH").FontSize(20).ExtraBold().FontColor(PrimaryBlue);
                            });

                            page.Content().Padding(30).Column(col =>
                            {
                                // --- KPI GRID ---
                                col.Item().Row(row =>
                                {
                                    row.Spacing(15);
                                    foreach (var kpi in analytics.Kpis.Take(4))
                                    {
                                        row.RelativeItem().Background(SlateBg).Border(1).BorderColor(BorderLight).Padding(15).Column(kCol =>
                                        {
                                            kCol.Item().Text(kpi.Label.ToUpper()).FontSize(8).Bold().FontColor(TextMuted);
                                            kCol.Item().PaddingTop(4).Text(kpi.Value).FontSize(18).ExtraBold().FontColor(PrimaryBlue);
                                        });
                                    }
                                });

                                // --- TOP CANDIDATES SECTION ---
                                col.Item().PaddingTop(30).Text("TOP 10 CANDIDATS (MATCHING IA)").FontSize(14).Bold().FontColor(PrimaryBlue);
                                col.Item().PaddingTop(10).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(30);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(h =>
                                    {
                                        h.Cell().Element(CellStyle).Text("#");
                                        h.Cell().Element(CellStyle).Text("Candidat");
                                        h.Cell().Element(CellStyle).Text("Statut");
                                        h.Cell().Element(CellStyle).Text("Exp.");
                                        h.Cell().Element(CellStyle).Text("Score");

                                        static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(8).BorderBottom(1).BorderColor(TextDark);
                                    });

                                    int i = 1;
                                    foreach (var app in sorted.Take(10))
                                    {
                                        var scoreColor = (app.AiScore ?? 0) >= 85 ? SuccessGreen : (app.AiScore ?? 0) >= 70 ? WarningAmber : DangerRed;
                                        
                                        table.Cell().Element(RowStyle).Text(i++.ToString());
                                        table.Cell().Element(RowStyle).Text($"{app.Candidate?.FirstName} {app.Candidate?.LastName}").Bold();
                                        table.Cell().Element(RowStyle).Text(app.Status.ToString());
                                        table.Cell().Element(RowStyle).Text($"{app.AIAnalysis?.TotalYearsExperience ?? 0:0} ans");
                                        table.Cell().Element(RowStyle).Text($"{app.AiScore ?? 0}%").FontColor(scoreColor).Bold();

                                        static IContainer RowStyle(IContainer container) => container.PaddingVertical(8).BorderBottom(1).BorderColor(BorderLight);
                                    }
                                });

                                // --- STATS ROW ---
                                col.Item().PaddingTop(30).Row(row =>
                                {
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("PIPELINE DE RECRUTEMENT").FontSize(12).Bold();
                                        inner.Item().PaddingTop(10).Column(pCol =>
                                        {
                                            foreach (var step in analytics.PipelineData.Where(x => x.Count > 0))
                                            {
                                                pCol.Item().PaddingVertical(2).Row(r =>
                                                {
                                                    r.RelativeItem().Text(step.Name);
                                                    r.AutoItem().Text(step.Count.ToString()).Bold();
                                                });
                                            }
                                        });
                                    });
                                    row.Spacing(40);
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("TOP SKILLS DÉTECTÉS").FontSize(12).Bold();
                                        inner.Item().PaddingTop(10).Column(sCol =>
                                        {
                                            foreach (var skill in analytics.TopSkills.Take(5))
                                            {
                                                sCol.Item().PaddingVertical(2).Row(r =>
                                                {
                                                    r.RelativeItem().Text(skill.Name);
                                                    r.AutoItem().Text(skill.Count.ToString()).FontColor(AccentBlue);
                                                });
                                            }
                                        });
                                    });
                                });
                            });

                            page.Footer().Padding(30).AlignCenter().Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                        });
                    });

                    return document.GeneratePdf();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PDF Error] {ex.Message}");
                    throw;
                }
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        //  EXCEL — JOB OFFER REPORT
        // ════════════════════════════════════════════════════════════════════════
        public async Task<byte[]> GenerateJobOfferReportExcelAsync(
            JobOffer offer, IEnumerable<JobApplication> applications)
        {
            return await Task.Run(() =>
            {
                var apps      = (applications ?? Enumerable.Empty<JobApplication>()).ToList();
                var analytics = BuildAnalytics(apps);

                using var workbook = new XLWorkbook();

                // ── Sheet 1: Tableau de Bord ──────────────────────────────────
                var dashboard = workbook.Worksheets.Add("Tableau de Bord");
                StyleDashboardSheet(dashboard, offer, analytics);

                // ── Sheet 2: Candidatures ─────────────────────────────────────
                var candidatesSheet = workbook.Worksheets.Add("Candidatures");
                StyleCandidatesSheet(candidatesSheet, apps);

                // ── Sheet 3: Pipeline ─────────────────────────────────────────
                var pipelineSheet = workbook.Worksheets.Add("Pipeline");
                StylePipelineSheet(pipelineSheet, analytics);

                // ── Sheet 4: Top Compétences ──────────────────────────────────
                var skillsSheet = workbook.Worksheets.Add("Top Compétences");
                StyleSkillsSheet(skillsSheet, analytics);

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        //  PDF — CANDIDATE PROFILE
        // ════════════════════════════════════════════════════════════════════════
        public async Task<byte[]> GenerateCandidateProfilePdfAsync(JobApplication application)
        {
            return await Task.Run(() =>
            {
                var candidate = application.Candidate;
                var analysis  = application.AIAnalysis;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(0);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x =>
                            x.FontSize(10).FontFamily(Fonts.Helvetica).FontColor(TextDark));

                        // ── HEADER ───────────────────────────────────────────────
                        page.Header()
                            .Background(PrimaryBlue)
                            .PaddingHorizontal(40).PaddingVertical(30)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("DOSSIER D'ÉVALUATION ÉLITE")
                                       .FontSize(9).SemiBold().FontColor(Colors.White).LetterSpacing(0.2f);
                                    col.Item().PaddingTop(2)
                                       .Text($"{candidate?.FirstName} {candidate?.LastName}")
                                       .FontSize(28).ExtraBold().FontColor(Colors.White);
                                    col.Item().PaddingTop(5).Row(r =>
                                    {
                                        r.Spacing(15);
                                        r.AutoItem().Text(candidate?.Email ?? "—").FontSize(10).FontColor(Colors.White);
                                        r.AutoItem().Text("•").FontSize(10).FontColor(Colors.White);
                                        r.AutoItem().Text(candidate?.PhoneNumber ?? "—").FontSize(10).FontColor(Colors.White);
                                    });
                                });

                                if (application.AiScore.HasValue)
                                {
                                    var scoreColor = application.AiScore >= 85 ? SuccessGreen
                                                   : application.AiScore >= 70 ? WarningAmber
                                                                                : DangerRed;
                                    row.ConstantItem(120).AlignRight().Column(col =>
                                    {
                                        col.Item().AlignCenter().Background(Colors.White)
                                           .Padding(12).Column(inner =>
                                           {
                                               inner.Item().AlignCenter().Text("SCORE DE MATCHING")
                                                    .FontSize(7).Bold().FontColor(PrimaryBlue);
                                               inner.Item().AlignCenter()
                                                    .Text($"{application.AiScore}%")
                                                    .FontSize(32).ExtraBold().FontColor(scoreColor);
                                           });
                                    });
                                }
                            });

                        page.Content()
                            .PaddingHorizontal(40).PaddingVertical(35)
                            .Column(col =>
                            {
                                // --- EXECUTIVE SUMMARY ---
                                if (!string.IsNullOrEmpty(analysis?.AutoGeneratedSummary))
                                {
                                    col.Item().Text("RÉSUMÉ EXÉCUTIF").FontSize(12).Bold().FontColor(PrimaryBlue);
                                    col.Item().PaddingTop(8).Background(SlateBg).Padding(15).Text(analysis.AutoGeneratedSummary).LineHeight(1.4f).Italic();
                                }

                                // --- STATS ROW ---
                                col.Item().PaddingTop(25).Row(row =>
                                {
                                    row.Spacing(20);
                                    row.RelativeItem().Column(c => {
                                        c.Item().Text("EXPÉRIENCE").FontSize(8).Bold().FontColor(TextMuted);
                                        c.Item().Text($"{analysis?.TotalYearsExperience ?? 0:0.0} ans").FontSize(16).ExtraBold();
                                    });
                                    row.RelativeItem().Column(c => {
                                        c.Item().Text("ÉVALUATION TECHNIQUE").FontSize(8).Bold().FontColor(TextMuted);
                                        c.Item().Text(application.QuizScore.HasValue ? $"{application.QuizScore}%" : "Non passé").FontSize(16).ExtraBold().FontColor(application.QuizScore.HasValue ? SuccessGreen : TextDark);
                                    });
                                    row.RelativeItem().Column(c => {
                                        c.Item().Text("RECOMMANDATION IA").FontSize(8).Bold().FontColor(TextMuted);
                                        c.Item().Text(analysis?.AIRecommendation ?? "A examiner").FontSize(14).ExtraBold().FontColor(AccentBlue);
                                    });
                                });

                                // --- SKILLS CLOUD ---
                                if (analysis?.IdentifiedSkills?.Any() == true)
                                {
                                    col.Item().PaddingTop(30).Text("COMPÉTENCES CLÉS").FontSize(12).Bold().FontColor(PrimaryBlue);
                                    col.Item().PaddingTop(10).Row(row =>
                                    {
                                        row.Spacing(8);
                                        foreach (var skill in analysis.IdentifiedSkills.Take(12))
                                        {
                                            row.AutoItem().Background(BorderLight).PaddingHorizontal(10).PaddingVertical(4).Text(skill).FontSize(9).SemiBold();
                                        }
                                    });
                                }

                                // --- STRENGTHS & WEAKNESSES ---
                                col.Item().PaddingTop(35).Row(row =>
                                {
                                    row.Spacing(30);
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("POINTS FORTS").FontSize(11).Bold().FontColor(SuccessGreen);
                                        inner.Item().PaddingTop(10).Column(list => {
                                            foreach(var s in analysis?.Strengths ?? new List<string>())
                                                list.Item().PaddingVertical(3).Row(r => {
                                                    r.ConstantItem(10).Text("•").Bold().FontColor(SuccessGreen);
                                                    r.RelativeItem().Text(s).FontSize(9);
                                                });
                                        });
                                    });
                                    row.RelativeItem().Column(inner =>
                                    {
                                        inner.Item().Text("POINTS DE VIGILANCE").FontSize(11).Bold().FontColor(DangerRed);
                                        inner.Item().PaddingTop(10).Column(list => {
                                            foreach(var w in analysis?.Weaknesses ?? new List<string>())
                                                list.Item().PaddingVertical(3).Row(r => {
                                                    r.ConstantItem(10).Text("•").Bold().FontColor(DangerRed);
                                                    r.RelativeItem().Text(w).FontSize(9);
                                                });
                                        });
                                    });
                                });

                                // --- INTERVIEW QUESTIONS ---
                                if (analysis?.InterviewQuestions?.Any() == true)
                                {
                                    col.Item().PaddingTop(35).Text("QUESTIONS D'ENTRETIEN SUGGÉRÉES").FontSize(12).Bold().FontColor(PrimaryBlue);
                                    col.Item().PaddingTop(10).Column(list =>
                                    {
                                        foreach (var q in analysis.InterviewQuestions.Take(3))
                                        {
                                            list.Item().PaddingBottom(12).Border(1).BorderColor(BorderLight).Padding(10).Column(qCol =>
                                            {
                                                qCol.Item().Row(r => {
                                                    r.RelativeItem().Text(q.Question).Bold().FontSize(9);
                                                    r.AutoItem().Background(SlateBg).PaddingHorizontal(6).Text(q.Category).FontSize(7).SemiBold().FontColor(TextMuted);
                                                });
                                                qCol.Item().PaddingTop(4).Text($"Objectif : {q.Purpose}").FontSize(8).Italic().FontColor(TextMuted);
                                            });
                                        }
                                    });
                                }
                            });

                        page.Footer().Padding(30).AlignCenter().Column(fCol => {
                            fCol.Item().Text("Ce rapport a été généré par l'Intelligence Artificielle NovaHire. Il doit être utilisé comme outil d'aide à la décision.").FontSize(7).FontColor(TextMuted).AlignCenter();
                            fCol.Item().PaddingTop(5).Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" / ");
                                x.TotalPages();
                            });
                        });
                    });
                });

                return document.GeneratePdf();
            });
        }

        // ════════════════════════════════════════════════════════════════════════
        //  EXCEL STYLE HELPERS
        // ════════════════════════════════════════════════════════════════════════
        private static void StyleDashboardSheet(IXLWorksheet ws, JobOffer offer, ReportAnalyticsSnapshot analytics)
        {
            ws.Cell(1, 1).Value = "RAPPORT DE RECRUTEMENT - " + offer.Title.ToUpper();
            ApplyHeaderStyle(ws.Range(1, 1, 1, 5), "#1D4ED8", 14);

            int row = 3;
            foreach (var kpi in analytics.Kpis)
            {
                ws.Cell(row, 1).Value = kpi.Label;
                ws.Cell(row, 2).Value = kpi.Value;
                ws.Cell(row, 1).Style.Font.Bold = true;
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void StyleCandidatesSheet(IXLWorksheet ws, List<JobApplication> apps)
        {
            ws.Cell(1, 1).Value = "LISTE DES CANDIDATS";
            ApplyHeaderStyle(ws.Range(1, 1, 1, 5), "#3B82F6", 12);

            ws.Cell(2, 1).Value = "Candidat";
            ws.Cell(2, 2).Value = "Email";
            ws.Cell(2, 3).Value = "Statut";
            ws.Cell(2, 4).Value = "Score IA";
            ws.Cell(2, 5).Value = "Date";
            ApplyHeaderStyle(ws.Range(2, 1, 2, 5), "#F8FAFC", 10, "#64748B");

            int row = 3;
            foreach (var app in apps)
            {
                ws.Cell(row, 1).Value = $"{app.Candidate?.FirstName} {app.Candidate?.LastName}";
                ws.Cell(row, 2).Value = app.Candidate?.Email;
                ws.Cell(row, 3).Value = app.Status.ToString();
                ws.Cell(row, 4).Value = app.AiScore.HasValue ? $"{app.AiScore}%" : "—";
                ws.Cell(row, 5).Value = app.AppliedAt.ToShortDateString();
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void StylePipelineSheet(IXLWorksheet ws, ReportAnalyticsSnapshot analytics)
        {
            ws.Cell(1, 1).Value = "PIPELINE DE RECRUTEMENT";
            ApplyHeaderStyle(ws.Range(1, 1, 1, 2), "#3B82F6", 12);

            ws.Cell(2, 1).Value = "Étape";
            ws.Cell(2, 2).Value = "Nombre de candidats";
            ApplyHeaderStyle(ws.Range(2, 1, 2, 2), "#F8FAFC", 10, "#64748B");

            int row = 3;
            foreach (var step in analytics.PipelineData)
            {
                ws.Cell(row, 1).Value = step.Name;
                ws.Cell(row, 2).Value = step.Count;
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void StyleSkillsSheet(IXLWorksheet ws, ReportAnalyticsSnapshot analytics)
        {
            ws.Cell(1, 1).Value = "TOP COMPÉTENCES DÉTECTÉES";
            ApplyHeaderStyle(ws.Range(1, 1, 1, 2), "#3B82F6", 12);

            ws.Cell(2, 1).Value = "Compétence";
            ws.Cell(2, 2).Value = "Occurrences";
            ApplyHeaderStyle(ws.Range(2, 1, 2, 2), "#F8FAFC", 10, "#64748B");

            int row = 3;
            foreach (var skill in analytics.TopSkills)
            {
                ws.Cell(row, 1).Value = skill.Name;
                ws.Cell(row, 2).Value = skill.Count;
                row++;
            }
            ws.Columns().AdjustToContents();
        }

        private static void ApplyHeaderStyle(IXLRange range, string bgHex, int fontSize, string? fontColorHex = null)
        {
            range.Merge();
            range.Style.Font.Bold = true;
            range.Style.Font.FontSize = fontSize;
            range.Style.Fill.BackgroundColor = XLColor.FromHtml(bgHex);
            range.Style.Font.FontColor = fontColorHex != null ? XLColor.FromHtml(fontColorHex) : XLColor.White;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static ReportAnalyticsSnapshot BuildAnalytics(List<JobApplication> applications)
        {
            if (applications == null) return new ReportAnalyticsSnapshot();
            
            var total    = applications.Count;
            var analyzed = applications.Count(a => a.AiScore.HasValue);

            var scored      = applications.Where(a => a.AiScore.HasValue).ToList();
            var avgScore    = scored.Any() ? (int)scored.Average(a => a.AiScore!.Value) : 0;
            var eliteCount  = applications.Count(a => (a.AiScore ?? 0) >= 85);
            var qualCount   = applications.Count(a => (a.AiScore ?? 0) >= 70 && (a.AiScore ?? 0) < 85);
            var lowCount    = applications.Count(a => (a.AiScore ?? 0) < 70);

            var shortlisted = applications.Count(a =>
                a.Status is ApplicationStatus.Shortlisted
                         or ApplicationStatus.Interview
                         or ApplicationStatus.Accepted);

            var conversionRate = total > 0
                ? (int)Math.Round((double)shortlisted / total * 100) : 0;

            var avgDays = analyzed > 0
                ? applications.Where(a => a.AiScore.HasValue)
                              .Average(a => (DateTime.UtcNow - a.AppliedAt).TotalDays)
                : 0;

            var topSkills = applications
                .Where(a => a.AIAnalysis?.IdentifiedSkills != null)
                .SelectMany(a => a.AIAnalysis!.IdentifiedSkills)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .GroupBy(s => s.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => (Name: g.Key, Count: g.Count()))
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var pipeline = Enum.GetValues<ApplicationStatus>()
                .OrderBy(s => (int)s)
                .Select(s => (Name: s.ToString(), Count: applications.Count(a => a.Status == s)))
                .ToList();

            return new ReportAnalyticsSnapshot
            {
                TotalCandidates    = total,
                AnalyzedCandidates = analyzed,
                AverageScore       = avgScore,
                ConversionRate     = conversionRate,
                EliteCount         = eliteCount,
                QualifiedCount     = qualCount,
                LowCount           = lowCount,
                AverageDaysToApply = Math.Round(avgDays, 1),
                Kpis = new List<(string, string, string)>
                {
                    ("Total Candidatures",      total.ToString("N0"),        AccentBlue),
                    ("Analysés par IA",         analyzed.ToString("N0"),     SuccessGreen),
                    ("Score Moyen Matching",    $"{avgScore}%",              avgScore >= 70 ? SuccessGreen : WarningAmber),
                    ("Taux de Conversion",      $"{conversionRate}%",        conversionRate >= 30 ? SuccessGreen : DangerRed),
                    ("Délai Moyen (jours)",     $"{avgDays:0.0}j",           AccentBlue),
                    ("Profils Élite (85%+)",    eliteCount.ToString("N0"),   SuccessGreen),
                },
                PipelineData = pipeline,
                TopSkills    = topSkills,
            };
        }
    }
}