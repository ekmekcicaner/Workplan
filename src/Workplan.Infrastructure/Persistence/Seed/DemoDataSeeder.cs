using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.Domain.ValueObjects;
using Workplan.Infrastructure.Identity;
using Workplan.SharedKernel.Auth;

namespace Workplan.Infrastructure.Persistence.Seed;

// Case study teslimi için: bir elektrik santrali inşaatı senaryosuna uygun,
// anlamlı Türkçe mock veri üretir. Veritabanında zaten proje varsa çalışmaz (idempotent).
public static class DemoDataSeeder
{
    private const string DemoPassword = "Demo123!";

    private static readonly string[] WorkerNamePool =
    [
        "Hasan Yıldırım", "Mahmut Er", "Cemal Bulut", "Recep Turan", "Yusuf Aktaş",
        "Fatih Polat", "Emre Şen", "Barış Yavuz", "Kadir Uçar", "Metin Solmaz",
        "Erdal Bozkurt", "Sinan Aksoy", "Tarık Güler", "Volkan Ekinci", "Gökhan Tunç",
        "Cihan Bayram", "Adem Kurt", "Selim Acar", "Nurettin Balcı", "Turgay Onat",
        "Bilal Sağlam", "Ferhat Doğru", "Şükrü Ekiz", "Onur Karaca", "Necati Bulut"
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Projects.AnyAsync())
            return;

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var sicilCounter = 10001;

        string NextPersonnelRef()
        {
            var name = WorkerNamePool[(sicilCounter - 10001) % WorkerNamePool.Length];
            return $"{sicilCounter++} - {name}";
        }

        async Task<Guid> EnsureUserAsync(string email, string fullName, string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null) return existing.Id;

            var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, FullName = fullName };
            var result = await userManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Demo kullanıcısı oluşturulamadı: {email} - {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, role);
            return user.Id;
        }

        var pmId = await EnsureUserAsync("mehmet.ozkan@workplan.local", "Mehmet Özkan", Roles.ProjectManager);

        var project = Project.Create(
            "SLP-CCGT-01", "Silopi Kombine Çevrim Doğalgaz Santrali İnşaatı", pmId).Value;
        db.Projects.Add(project);

        WorkItemType Tow(string name) => WorkItemType.Create(name).Value;
        WorkItemType Child(WorkItemType parent, string name, Unit unit = Unit.None) =>
            WorkItemType.Create(name, parent.Id, parent.Level, unit).Value;

        var towBeton = Tow("Betonarme İşleri");
        var stowKalip = Child(towBeton, "Kalıp İşleri");
        var sstowTemelKalip = Child(stowKalip, "Temel Kalıbı", Unit.M2);
        var sstowKolonKalip = Child(stowKalip, "Kolon ve Perde Kalıbı", Unit.M2);
        var stowDemir = Child(towBeton, "Demir Donatı İşleri");
        var sstowTemelDemir = Child(stowDemir, "Temel Demiri", Unit.Ton);
        var sstowKolonDemir = Child(stowDemir, "Kolon ve Perde Demiri", Unit.Ton);

        var towCelik = Tow("Çelik Konstrüksiyon İşleri");
        var stowCelikMontaj = Child(towCelik, "Çelik Montaj İşleri");
        var sstowAnaTasiyici = Child(stowCelikMontaj, "Ana Taşıyıcı Çelik Montajı", Unit.Ton);
        var sstowIkincilCelik = Child(stowCelikMontaj, "İkincil Çelik Montajı", Unit.Ton);
        var stowKaynak = Child(towCelik, "Kaynak İşleri");
        var sstowSahaKaynagi = Child(stowKaynak, "Saha Kaynağı", Unit.Ton);
        var sstowNdtDuzeltme = Child(stowKaynak, "NDT Sonrası Düzeltme Kaynağı", Unit.Ton);

        var towMekanik = Tow("Mekanik Montaj İşleri");
        var stowEkipman = Child(towMekanik, "Ekipman Montajı");
        var sstowTurbinMontaj = Child(stowEkipman, "Türbin Montajı", Unit.Ton);
        var sstowJeneratorMontaj = Child(stowEkipman, "Jeneratör Montajı", Unit.Ton);
        var stowBoru = Child(towMekanik, "Boru Hattı İşleri");
        var sstowBoruMontaj = Child(stowBoru, "Boru Hattı Montajı", Unit.Ton);
        var sstowBoruIzolasyon = Child(stowBoru, "Boru İzolasyonu", Unit.M2);

        var towElektrik = Tow("Elektrik ve Enstrümantasyon İşleri");
        var stowKablo = Child(towElektrik, "Kablo Çekimi İşleri");
        var sstowKuvvetKablo = Child(stowKablo, "Kuvvet Kablosu Çekimi", Unit.Ton);
        var sstowEnstrumanKablo = Child(stowKablo, "Enstrüman Kablosu Çekimi", Unit.Ton);
        var stowTrafo = Child(towElektrik, "Trafo ve Şalt Montajı");
        var sstowTrafoMontaj = Child(stowTrafo, "Güç Trafosu Montajı", Unit.Ton);
        var sstowGisMontaj = Child(stowTrafo, "GIS Hücre Montajı", Unit.Ton);

        db.WorkItemTypes.AddRange(
            towBeton, stowKalip, sstowTemelKalip, sstowKolonKalip, stowDemir, sstowTemelDemir, sstowKolonDemir,
            towCelik, stowCelikMontaj, sstowAnaTasiyici, sstowIkincilCelik, stowKaynak, sstowSahaKaynagi, sstowNdtDuzeltme,
            towMekanik, stowEkipman, sstowTurbinMontaj, sstowJeneratorMontaj, stowBoru, sstowBoruMontaj, sstowBoruIzolasyon,
            towElektrik, stowKablo, sstowKuvvetKablo, sstowEnstrumanKablo, stowTrafo, sstowTrafoMontaj, sstowGisMontaj);

        var dailyPlans = new List<DailyPlan>();
        var notifications = new List<Notification>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        async Task SeedRegionAsync(
            string code, string regionName,
            string siteChiefEmail, string siteChiefName,
            string techOfficeEmail, string techOfficeName,
            (string email, string name)[] homs,
            string parentLocationName,
            (string name, WorkItemType leaf1, WorkItemType leaf2, WorkerType[] workerTypes)[] childLocations)
        {
            var siteChiefId = await EnsureUserAsync(siteChiefEmail, siteChiefName, Roles.SiteChief);
            var techOfficeId = await EnsureUserAsync(techOfficeEmail, techOfficeName, Roles.TechnicalOfficeEngineer);

            var region = CrewRegion.Create(project.Id, code, $"{code} Bölgesi - {regionName}").Value;
            region.AssignSiteChief(siteChiefId);
            region.AssignTechOffice(techOfficeId);
            db.CrewRegions.Add(region);

            var parentLocation = Location.Create(project.Id, region.Id, parentLocationName).Value;
            db.Locations.Add(parentLocation);

            for (var i = 0; i < childLocations.Length; i++)
            {
                var (childName, leaf1, leaf2, workerTypes) = childLocations[i];
                var (homEmail, homName) = homs[i % homs.Length];
                var homId = await EnsureUserAsync(homEmail, homName, Roles.HeadOfMaster);

                var childLocation = Location.Create(project.Id, region.Id, childName, parentLocation.Id).Value;
                childLocation.AssignHeadOfMaster(homId);
                db.Locations.Add(childLocation);

                var crew = Crew.Create(childLocation.Id, $"{childName} Ekibi", homId).Value;
                foreach (var workerType in workerTypes)
                    crew.AddMember(workerType, NextPersonnelRef());
                db.Crews.Add(crew);

                // Geçmişte kalmış, tamamen onaylanmış bir gün (rapor/geçmiş verisi için)
                var closedPlan = DailyPlan.CreateFromPlan(
                    project.Id, region.Id, childLocation.Id, leaf1.Id,
                    today.AddDays(-7 - i), techOfficeId, homId,
                    80 + i * 5, 6 + i, leaf1.Unit).Value;
                closedPlan.StartWork(homId, crew.Id);
                closedPlan.SubmitProgress(78 + i * 5, 6 + i, 1, null, homId);
                closedPlan.Approve(WorkStatus.ApprovedBySiteChief, siteChiefId, siteChiefId);
                closedPlan.Approve(WorkStatus.ApprovedByPM, pmId, pmId);
                dailyPlans.Add(closedPlan);

                // Yakın tarihli, durumu değişen bir gün (aktif akış demoları için)
                var recentPlan = DailyPlan.CreateFromPlan(
                    project.Id, region.Id, childLocation.Id, leaf2.Id,
                    today.AddDays(-i), techOfficeId, homId,
                    60 + i * 4, 4 + i, leaf2.Unit).Value;

                switch (i % 4)
                {
                    case 1:
                        recentPlan.StartWork(homId, crew.Id);
                        break;
                    case 2:
                        recentPlan.StartWork(homId, crew.Id);
                        recentPlan.SubmitProgress(58 + i * 4, 4 + i, 0, null, homId);
                        break;
                    case 3:
                        recentPlan.StartWork(homId, crew.Id);
                        recentPlan.SubmitProgress(58 + i * 4, 4 + i, 0, null, homId);
                        recentPlan.Approve(WorkStatus.ApprovedBySiteChief, siteChiefId, siteChiefId);
                        break;
                }

                dailyPlans.Add(recentPlan);
                notifications.Add(Notification.CreateDailyPlanAssigned(homId, recentPlan.Id, recentPlan.WorkDate).Value);
            }
        }

        await SeedRegionAsync(
            "A", "Türbin Binası ve Jeneratör Sahası",
            "hakan.yildiz@workplan.local", "Hakan Yıldız",
            "elif.sahin@workplan.local", "Elif Şahin",
            [("ali.kaya@workplan.local", "Ali Kaya"), ("veli.dogan@workplan.local", "Veli Doğan")],
            "Türbin Binası",
            [
                ("Türbin Binası - Zemin Kat", sstowTemelKalip, sstowTurbinMontaj,
                    [WorkerType.RebarFixer, WorkerType.Formworker, WorkerType.GeneralLabor, WorkerType.Survey]),
                ("Türbin Binası - 1. Kat", sstowKolonKalip, sstowJeneratorMontaj,
                    [WorkerType.Assembler, WorkerType.Welder, WorkerType.Operators, WorkerType.GeneralLabor])
            ]);

        await SeedRegionAsync(
            "B", "Kazan Dairesi ve Baca Gazı Sistemleri",
            "serkan.aydin@workplan.local", "Serkan Aydın",
            "burak.celik@workplan.local", "Burak Çelik",
            [("mustafa.arslan@workplan.local", "Mustafa Arslan"), ("ibrahim.koc@workplan.local", "İbrahim Koç")],
            "Kazan Dairesi",
            [
                ("Kazan Dairesi - Temel Seviyesi", sstowTemelDemir, sstowAnaTasiyici,
                    [WorkerType.RebarFixer, WorkerType.GeneralLabor, WorkerType.Survey, WorkerType.Slinger]),
                ("Kazan Dairesi - Çelik Konstrüksiyon Seviyesi", sstowIkincilCelik, sstowSahaKaynagi,
                    [WorkerType.Welder, WorkerType.Assembler, WorkerType.Operators, WorkerType.GeneralLabor])
            ]);

        await SeedRegionAsync(
            "C", "Şalt Sahası ve Trafo Merkezi",
            "cengiz.demir@workplan.local", "Cengiz Demir",
            "zeynep.arik@workplan.local", "Zeynep Arık",
            [("osman.yildirim@workplan.local", "Osman Yıldırım"), ("huseyin.celik@workplan.local", "Hüseyin Çelik")],
            "Şalt Sahası",
            [
                ("Trafo Alanı", sstowTrafoMontaj, sstowKuvvetKablo,
                    [WorkerType.Assembler, WorkerType.Slinger, WorkerType.Operators, WorkerType.GeneralLabor]),
                ("GIS Binası", sstowGisMontaj, sstowEnstrumanKablo,
                    [WorkerType.Assembler, WorkerType.ArchitecturalWorker, WorkerType.GeneralLabor, WorkerType.Survey])
            ]);

        await SeedRegionAsync(
            "D", "Soğutma Kulesi ve Su Alma Yapıları",
            "tolga.kara@workplan.local", "Tolga Kara",
            "selin.gunes@workplan.local", "Selin Güneş",
            [("ramazan.aydin@workplan.local", "Ramazan Aydın"), ("kemal.sahin@workplan.local", "Kemal Şahin")],
            "Soğutma Kulesi",
            [
                ("Kule Temeli", sstowTemelKalip, sstowTemelDemir,
                    [WorkerType.RebarFixer, WorkerType.Formworker, WorkerType.GeneralLabor, WorkerType.Survey]),
                ("Su Alma Yapısı", sstowBoruMontaj, sstowBoruIzolasyon,
                    [WorkerType.Welder, WorkerType.Operators, WorkerType.GeneralLabor, WorkerType.Slinger])
            ]);

        db.DailyPlans.AddRange(dailyPlans);
        db.Notifications.AddRange(notifications);

        await db.SaveChangesAsync();
    }
}
