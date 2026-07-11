using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Workplan.Domain.Common;
using Workplan.Domain.Entities;
using Workplan.Domain.Enums;
using Workplan.Domain.ValueObjects;
using Workplan.Infrastructure.Identity;
using Workplan.SharedKernel.Auth;
using Workplan.SharedKernel.Common;

namespace Workplan.Infrastructure.Persistence.Seed;

/// <summary>
/// Nükleer güç santrali inşaatı demosu için anlamlı ve ilişkili veri üretir.
///
/// Demo kullanıcılarının tamamının parolası: Demo123!
///
/// Üretilen başlıca veriler:
/// - Identity rolleri ve rol atamaları
/// - Proje yöneticisi, saha şefleri, teknik ofis mühendisleri ve ustabaşları
/// - Nükleer ada, konvansiyonel ada, yardımcı tesisler ve deniz yapıları bölgeleri
/// - KKK benzeri hiyerarşik lokasyonlar
/// - ToW -> SToW -> SSToW iş kırılımı
/// - Ekip tipleri
/// - Bugün, dün, önceki gün ve yarına bağlı çeşitli durumlarda günlük planlar
/// - Aggregate davranışları üzerinden status transition kayıtları
/// - Atama ve red bildirimleri
///
/// Refresh token, external login ve user token tabloları bilinçli olarak seed edilmez.
/// Bunlar gerçek authentication akışı sırasında üretilmesi gereken operasyonel verilerdir.
/// </summary>
public static class DemoDataSeeder
{
    private const string DemoPassword = "Demo123!";
    private const string DemoProjectCode = "MNGS-U1-2026";

    public static async Task SeedAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Bu proje varsa demo veri daha önce yüklenmiştir.
        if (await db.Projects.AnyAsync(
                x => x.Code == DemoProjectCode,
                cancellationToken))
        {
            return;
        }

        await EnsureRolesAsync(roleManager);

        var users = await SeedUsersAsync(userManager);

        // Tarih bazlı demo senaryoları özellikle local takvime göre üretilir.
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var yesterday = today.AddDays(-1);
        var twoDaysAgo = today.AddDays(-2);
        var threeDaysAgo = today.AddDays(-3);
        var tomorrow = today.AddDays(1);

        var project = Project.Create(
            DemoProjectCode,
            "Mersin Nükleer Güç Santrali - Ünite 1 İnşaat Projesi",
            users.ProjectManagerId).Value;

        db.Projects.Add(project);

        var workItems = SeedWorkItemTypes(db);
        var crews = SeedCrewTypes(db);

        var nuclearIsland = await AddRegionAsync(
            project.Id,
            "NI",
            "Nükleer Ada",
            users.NuclearIslandSiteChiefId,
            users.NuclearIslandTechOfficeId);

        var conventionalIsland = await AddRegionAsync(
            project.Id,
            "CI",
            "Konvansiyonel Ada ve Türbin Tesisi",
            users.ConventionalIslandSiteChiefId,
            users.ConventionalIslandTechOfficeId);

        var balanceOfPlant = await AddRegionAsync(
            project.Id,
            "BOP",
            "Yardımcı Tesisler ve Boru Koridorları",
            users.BopSiteChiefId,
            users.BopTechOfficeId);

        var marineWorks = await AddRegionAsync(
            project.Id,
            "MW",
            "Deniz Yapıları ve Soğutma Suyu Sistemleri",
            users.MarineSiteChiefId,
            users.MarineTechOfficeId);

        // ---------------------------------------------------------------------
        // Lokasyon hiyerarşisi
        // ---------------------------------------------------------------------

        var nuclearIslandRoot = AddParentLocation(
            project.Id,
            nuclearIsland.Id,
            "Nükleer Ada Ana Yerleşkesi");

        var reactorBuilding = AddChildLocation(
            project.Id,
            nuclearIsland.Id,
            nuclearIslandRoot.Id,
            "1UJA - Reaktör Binası",
            users.ReactorBuildingHomId);

        var auxiliaryBuilding = AddChildLocation(
            project.Id,
            nuclearIsland.Id,
            nuclearIslandRoot.Id,
            "1UKA - Yardımcı Reaktör Binası",
            users.AuxiliaryBuildingHomId);

        var dieselBuilding = AddChildLocation(
            project.Id,
            nuclearIsland.Id,
            nuclearIslandRoot.Id,
            "1UBN - Acil Dizel Jeneratör Binası",
            users.DieselBuildingHomId);

        var conventionalIslandRoot = AddParentLocation(
            project.Id,
            conventionalIsland.Id,
            "Konvansiyonel Ada Ana Yerleşkesi");

        var turbineBuilding = AddChildLocation(
            project.Id,
            conventionalIsland.Id,
            conventionalIslandRoot.Id,
            "1UMA - Türbin Binası",
            users.TurbineBuildingHomId);

        var transformerArea = AddChildLocation(
            project.Id,
            conventionalIsland.Id,
            conventionalIslandRoot.Id,
            "1UBD - Ana Trafo ve Şalt Sahası",
            users.TransformerAreaHomId);

        var bopRoot = AddParentLocation(
            project.Id,
            balanceOfPlant.Id,
            "Yardımcı Tesisler Ana Yerleşkesi");

        var pipeRack = AddChildLocation(
            project.Id,
            balanceOfPlant.Id,
            bopRoot.Id,
            "1UYA - Ana Boru Köprüsü ve Pipe Rack",
            users.PipeRackHomId);

        var wasteBuilding = AddChildLocation(
            project.Id,
            balanceOfPlant.Id,
            bopRoot.Id,
            "1UGW - Atık Yönetim Binası",
            users.WasteBuildingHomId);

        var marineRoot = AddParentLocation(
            project.Id,
            marineWorks.Id,
            "Deniz Yapıları Ana Yerleşkesi");

        var intakeStructure = AddChildLocation(
            project.Id,
            marineWorks.Id,
            marineRoot.Id,
            "1UQA - Deniz Suyu Alma Yapısı",
            users.IntakeStructureHomId);

        var pumpStation = AddChildLocation(
            project.Id,
            marineWorks.Id,
            marineRoot.Id,
            "1UPC - Soğutma Suyu Pompa İstasyonu",
            users.PumpStationHomId);

        // ---------------------------------------------------------------------
        // Günlük plan senaryoları
        // ---------------------------------------------------------------------
        // Her statü ayrı ve anlamlı bir operasyon senaryosuna bağlanmıştır.
        // DailyPlan davranışları StatusTransitions tablosunu da doldurur.
        // ---------------------------------------------------------------------

        AddPlan(
            project,
            nuclearIsland,
            reactorBuilding,
            workItems.ReactorBaseRebar,
            twoDaysAgo,
            users.NuclearIslandTechOfficeId,
            users.ReactorBuildingHomId,
            crews.RebarCrew,
            plannedQuantity: 42.50m,
            plannedManDay: 18.00m,
            state: DemoPlanState.ApprovedByPm,
            factQuantity: 41.80m,
            factManDay: 18.50m,
            overtime: 2.00m,
            comment: "Reaktör temel plakası kuzey zonu donatı montajı tamamlandı. QC kapanış kontrolü yapıldı.");

        AddPlan(
            project,
            nuclearIsland,
            reactorBuilding,
            workItems.ContainmentFormwork,
            yesterday,
            users.NuclearIslandTechOfficeId,
            users.ReactorBuildingHomId,
            crews.FormworkCrew,
            plannedQuantity: 310.00m,
            plannedManDay: 22.00m,
            state: DemoPlanState.ApprovedBySiteChief,
            factQuantity: 296.00m,
            factManDay: 23.00m,
            overtime: 1.50m,
            comment: "İç containment duvarı +7.20 kotu kalıp kapanışı tamamlandı; PM onayı bekleniyor.");

        AddPlan(
            project,
            nuclearIsland,
            reactorBuilding,
            workItems.EmbeddedPlateInstallation,
            today,
            users.NuclearIslandTechOfficeId,
            users.ReactorBuildingHomId,
            crews.MechanicalAssemblyCrew,
            plannedQuantity: 8.40m,
            plannedManDay: 10.00m,
            state: DemoPlanState.InProgress);

        AddPlan(
            project,
            nuclearIsland,
            reactorBuilding,
            workItems.ContainmentLinerWelding,
            tomorrow,
            users.NuclearIslandTechOfficeId,
            users.ReactorBuildingHomId,
            crews.CertifiedWelderCrew,
            plannedQuantity: 6.20m,
            plannedManDay: 12.00m,
            state: DemoPlanState.Assigned,
            notification: DemoNotification.Assignment);

        AddPlan(
            project,
            nuclearIsland,
            auxiliaryBuilding,
            workItems.WallRebar,
            threeDaysAgo,
            users.NuclearIslandTechOfficeId,
            users.AuxiliaryBuildingHomId,
            crews.RebarCrew,
            plannedQuantity: 28.00m,
            plannedManDay: 14.00m,
            state: DemoPlanState.ApprovedByPm,
            factQuantity: 28.60m,
            factManDay: 14.00m,
            overtime: 0.50m,
            comment: "Yardımcı bina perde donatısı A aksı tamamlandı ve teslim edildi.");

        AddPlan(
            project,
            nuclearIsland,
            auxiliaryBuilding,
            workItems.HvacDuctInstallation,
            yesterday,
            users.NuclearIslandTechOfficeId,
            users.AuxiliaryBuildingHomId,
            crews.HvacCrew,
            plannedQuantity: 5.80m,
            plannedManDay: 9.00m,
            state: DemoPlanState.Submitted,
            factQuantity: 4.90m,
            factManDay: 9.50m,
            overtime: 1.00m,
            comment: "Havalandırma kanalı askı revizyonu nedeniyle planın bir bölümü sonraki güne devretti.");

        AddPlan(
            project,
            nuclearIsland,
            auxiliaryBuilding,
            workItems.CableTrayInstallation,
            today,
            users.NuclearIslandTechOfficeId,
            users.AuxiliaryBuildingHomId,
            crews.ElectricalCrew,
            plannedQuantity: 4.20m,
            plannedManDay: 8.00m,
            state: DemoPlanState.Submitted,
            factQuantity: 4.35m,
            factManDay: 8.00m,
            overtime: 0.00m,
            comment: "Güvenlik sınıfı kablo tavaları B koridorunda tamamlandı.");

        AddPlan(
            project,
            nuclearIsland,
            dieselBuilding,
            workItems.StructuralSteelInstallation,
            yesterday,
            users.NuclearIslandTechOfficeId,
            users.DieselBuildingHomId,
            crews.SteelAssemblyCrew,
            plannedQuantity: 11.50m,
            plannedManDay: 12.00m,
            state: DemoPlanState.ApprovedByPm,
            factQuantity: 11.20m,
            factManDay: 12.00m,
            overtime: 2.00m,
            comment: "Dizel jeneratör binası çatı çeliği montajı tamamlandı.");

        AddPlan(
            project,
            nuclearIsland,
            dieselBuilding,
            workItems.EquipmentFoundationFormwork,
            today,
            users.NuclearIslandTechOfficeId,
            users.DieselBuildingHomId,
            crews.FormworkCrew,
            plannedQuantity: 145.00m,
            plannedManDay: 11.00m,
            state: DemoPlanState.Assigned,
            notification: DemoNotification.Assignment);

        AddPlan(
            project,
            conventionalIsland,
            turbineBuilding,
            workItems.TurbineFoundationRebar,
            twoDaysAgo,
            users.ConventionalIslandTechOfficeId,
            users.TurbineBuildingHomId,
            crews.RebarCrew,
            plannedQuantity: 36.00m,
            plannedManDay: 17.00m,
            state: DemoPlanState.ApprovedByPm,
            factQuantity: 35.40m,
            factManDay: 17.50m,
            overtime: 1.00m,
            comment: "Türbin pedestal donatı montajı tamamlandı; survey ölçümleri uygundur.");

        AddPlan(
            project,
            conventionalIsland,
            turbineBuilding,
            workItems.TurbineHallSteel,
            yesterday,
            users.ConventionalIslandTechOfficeId,
            users.TurbineBuildingHomId,
            crews.SteelAssemblyCrew,
            plannedQuantity: 18.00m,
            plannedManDay: 16.00m,
            state: DemoPlanState.Submitted,
            factQuantity: 16.70m,
            factManDay: 16.00m,
            overtime: 2.50m,
            comment: "Kuzey cephe ana kolon montajı rüzgâr limiti nedeniyle kısmi tamamlandı.");

        AddPlan(
            project,
            conventionalIsland,
            turbineBuilding,
            workItems.MainEquipmentInstallation,
            today,
            users.ConventionalIslandTechOfficeId,
            users.TurbineBuildingHomId,
            crews.HeavyLiftingCrew,
            plannedQuantity: 24.00m,
            plannedManDay: 20.00m,
            state: DemoPlanState.RejectedByPm,
            factQuantity: 21.50m,
            factManDay: 20.00m,
            overtime: 1.00m,
            comment: "Ana ekipman ankraj kontrolleri tamamlandı ve ilk onaya sunuldu.",
            rejectionReason: "Survey tolerans raporu eklenmeden nihai onay verilemez.",
            notification: DemoNotification.Rejection);

        AddPlan(
            project,
            conventionalIsland,
            transformerArea,
            workItems.GroundingGridInstallation,
            yesterday,
            users.ConventionalIslandTechOfficeId,
            users.TransformerAreaHomId,
            crews.ElectricalCrew,
            plannedQuantity: 7.80m,
            plannedManDay: 8.00m,
            state: DemoPlanState.ApprovedBySiteChief,
            factQuantity: 7.80m,
            factManDay: 8.00m,
            overtime: 0.00m,
            comment: "Ana trafo sahası ring topraklama bağlantıları test edildi.");

        AddPlan(
            project,
            conventionalIsland,
            transformerArea,
            workItems.TransformerSteelInstallation,
            today,
            users.ConventionalIslandTechOfficeId,
            users.TransformerAreaHomId,
            crews.SteelAssemblyCrew,
            plannedQuantity: 5.50m,
            plannedManDay: 7.00m,
            state: DemoPlanState.RejectedBySiteChief,
            factQuantity: 0.00m,
            factManDay: 0.00m,
            overtime: 0.00m,
            comment: "Kaldırma planı saha izin sürecinde onaylanmadığı için fiziksel montaj başlatılamadı.",
            rejectionReason: "Saha izin numarası ve revize kaldırma planı eklenmelidir.",
            notification: DemoNotification.Rejection);

        AddPlan(
            project,
            balanceOfPlant,
            pipeRack,
            workItems.ProcessPipingInstallation,
            threeDaysAgo,
            users.BopTechOfficeId,
            users.PipeRackHomId,
            crews.PipingCrew,
            plannedQuantity: 13.20m,
            plannedManDay: 14.00m,
            state: DemoPlanState.ApprovedByPm,
            factQuantity: 13.40m,
            factManDay: 14.00m,
            overtime: 1.00m,
            comment: "Ana buhar yardımcı hattı spool montajları tamamlandı.");

        AddPlan(
            project,
            balanceOfPlant,
            pipeRack,
            workItems.PipeSupportInstallation,
            yesterday,
            users.BopTechOfficeId,
            users.PipeRackHomId,
            crews.MechanicalAssemblyCrew,
            plannedQuantity: 9.30m,
            plannedManDay: 10.00m,
            state: DemoPlanState.ApprovedBySiteChief,
            factQuantity: 8.90m,
            factManDay: 10.00m,
            overtime: 0.50m,
            comment: "Pipe rack P-04 hattı sabit ve yaylı mesnet montajları tamamlandı.");

        AddPlan(
            project,
            balanceOfPlant,
            pipeRack,
            workItems.PipingInsulation,
            today,
            users.BopTechOfficeId,
            users.PipeRackHomId,
            crews.InsulationCrew,
            plannedQuantity: 185.00m,
            plannedManDay: 9.00m,
            state: DemoPlanState.InProgress);

        AddPlan(
            project,
            balanceOfPlant,
            wasteBuilding,
            workItems.ProtectiveCoating,
            yesterday,
            users.BopTechOfficeId,
            users.WasteBuildingHomId,
            crews.PaintingCrew,
            plannedQuantity: 420.00m,
            plannedManDay: 10.00m,
            state: DemoPlanState.Submitted,
            factQuantity: 365.00m,
            factManDay: 10.00m,
            overtime: 1.00m,
            comment: "Nem oranı yükseldiği için son kat uygulamasının bir bölümü ertelendi.");

        AddPlan(
            project,
            balanceOfPlant,
            wasteBuilding,
            workItems.ScaffoldingInstallation,
            today,
            users.BopTechOfficeId,
            users.WasteBuildingHomId,
            crews.ScaffoldingCrew,
            plannedQuantity: 260.00m,
            plannedManDay: 12.00m,
            state: DemoPlanState.Assigned,
            notification: DemoNotification.Assignment);

        AddPlan(
            project,
            marineWorks,
            intakeStructure,
            workItems.IntakeStructureRebar,
            twoDaysAgo,
            users.MarineTechOfficeId,
            users.IntakeStructureHomId,
            crews.RebarCrew,
            plannedQuantity: 31.00m,
            plannedManDay: 15.00m,
            state: DemoPlanState.ApprovedByPm,
            factQuantity: 30.50m,
            factManDay: 15.50m,
            overtime: 1.50m,
            comment: "Deniz suyu alma yapısı taban donatısı ikinci etap tamamlandı.");

        AddPlan(
            project,
            marineWorks,
            intakeStructure,
            workItems.IntakeFormwork,
            yesterday,
            users.MarineTechOfficeId,
            users.IntakeStructureHomId,
            crews.FormworkCrew,
            plannedQuantity: 280.00m,
            plannedManDay: 16.00m,
            state: DemoPlanState.ApprovedBySiteChief,
            factQuantity: 273.00m,
            factManDay: 16.00m,
            overtime: 2.00m,
            comment: "Kuzey hücresi perde kalıbı tamamlandı; PM kontrolü bekleniyor.");

        AddPlan(
            project,
            marineWorks,
            intakeStructure,
            workItems.EmbeddedPlateInstallation,
            today,
            users.MarineTechOfficeId,
            users.IntakeStructureHomId,
            crews.MechanicalAssemblyCrew,
            plannedQuantity: 6.10m,
            plannedManDay: 8.00m,
            state: DemoPlanState.Submitted,
            factQuantity: 5.70m,
            factManDay: 8.00m,
            overtime: 0.50m,
            comment: "Izgara kılavuz plakalarının montajı tamamlandı; iki plaka survey revizyonuna bırakıldı.");

        AddPlan(
            project,
            marineWorks,
            pumpStation,
            workItems.PumpStationSteel,
            yesterday,
            users.MarineTechOfficeId,
            users.PumpStationHomId,
            crews.SteelAssemblyCrew,
            plannedQuantity: 14.00m,
            plannedManDay: 13.00m,
            state: DemoPlanState.ApprovedByPm,
            factQuantity: 14.30m,
            factManDay: 13.00m,
            overtime: 1.00m,
            comment: "Pompa istasyonu platform çelikleri tamamlandı.");

        AddPlan(
            project,
            marineWorks,
            pumpStation,
            workItems.CoolingWaterPiping,
            today,
            users.MarineTechOfficeId,
            users.PumpStationHomId,
            crews.PipingCrew,
            plannedQuantity: 17.50m,
            plannedManDay: 15.00m,
            state: DemoPlanState.InProgress);

        AddPlan(
            project,
            marineWorks,
            pumpStation,
            workItems.PumpEquipmentInstallation,
            tomorrow,
            users.MarineTechOfficeId,
            users.PumpStationHomId,
            crews.HeavyLiftingCrew,
            plannedQuantity: 32.00m,
            plannedManDay: 18.00m,
            state: DemoPlanState.Assigned,
            notification: DemoNotification.Assignment);

        await db.SaveChangesAsync(cancellationToken);

        return;

        // ---------------------------------------------------------------------
        // Local helper methods
        // ---------------------------------------------------------------------

        async Task<CrewRegion> AddRegionAsync(
            Guid projectId,
            string code,
            string name,
            Guid siteChiefId,
            Guid techOfficeId)
        {
            var region = CrewRegion.Create(
                projectId,
                code,
                $"{code} - {name}").Value;

            region.AssignSiteChief(siteChiefId);
            region.AssignTechOffice(techOfficeId);

            db.CrewRegions.Add(region);
            await Task.CompletedTask;
            return region;
        }

        Location AddParentLocation(
            Guid projectId,
            Guid regionId,
            string name)
        {
            var location = Location.Create(
                projectId,
                regionId,
                name).Value;

            db.Locations.Add(location);
            return location;
        }

        Location AddChildLocation(
            Guid projectId,
            Guid regionId,
            Guid parentId,
            string name,
            Guid headOfMasterId)
        {
            var location = Location.Create(
                projectId,
                regionId,
                name,
                parentId).Value;

            location.AssignHeadOfMaster(headOfMasterId);
            db.Locations.Add(location);
            return location;
        }

        DailyPlan AddPlan(
            Project targetProject,
            CrewRegion region,
            Location location,
            WorkItemType workItem,
            DateOnly workDate,
            Guid plannedById,
            Guid headOfMasterId,
            CrewType crewType,
            decimal plannedQuantity,
            decimal plannedManDay,
            DemoPlanState state,
            decimal? factQuantity = null,
            decimal? factManDay = null,
            decimal overtime = 0,
            string? comment = null,
            string? rejectionReason = null,
            DemoNotification notification = DemoNotification.None)
        {
            var plan = DailyPlan.CreateFromPlan(
                targetProject.Id,
                region.Id,
                location.Id,
                workItem.Id,
                workDate,
                plannedById,
                headOfMasterId,
                plannedQuantity,
                plannedManDay,
                workItem.Unit).Value;

            switch (state)
            {
                case DemoPlanState.Assigned:
                    break;

                case DemoPlanState.InProgress:
                    EnsureSucceeded(plan.StartWork(headOfMasterId, crewType.Id), "İşi başlatma");
                    break;

                case DemoPlanState.Submitted:
                    EnsureSucceeded(plan.StartWork(headOfMasterId, crewType.Id), "İşi başlatma");
                    EnsureSucceeded(plan.SubmitProgress(
                        factQuantity ?? plannedQuantity,
                        factManDay ?? plannedManDay,
                        overtime,
                        comment,
                        headOfMasterId), "Gerçekleşme gönderme");
                    break;

                case DemoPlanState.ApprovedBySiteChief:
                    EnsureSucceeded(plan.StartWork(headOfMasterId, crewType.Id), "İşi başlatma");
                    EnsureSucceeded(plan.SubmitProgress(
                        factQuantity ?? plannedQuantity,
                        factManDay ?? plannedManDay,
                        overtime,
                        comment,
                        headOfMasterId), "Gerçekleşme gönderme");
                    EnsureSucceeded(plan.Approve(
                        WorkStatus.ApprovedBySiteChief,
                        region.SiteChiefUserId!.Value,
                        region.SiteChiefUserId.Value), "Şantiye şefi onayı");
                    break;

                case DemoPlanState.ApprovedByPm:
                    EnsureSucceeded(plan.StartWork(headOfMasterId, crewType.Id), "İşi başlatma");
                    EnsureSucceeded(plan.SubmitProgress(
                        factQuantity ?? plannedQuantity,
                        factManDay ?? plannedManDay,
                        overtime,
                        comment,
                        headOfMasterId), "Gerçekleşme gönderme");
                    EnsureSucceeded(plan.Approve(
                        WorkStatus.ApprovedBySiteChief,
                        region.SiteChiefUserId!.Value,
                        region.SiteChiefUserId.Value), "Şantiye şefi onayı");
                    EnsureSucceeded(plan.Approve(
                        WorkStatus.ApprovedByPM,
                        targetProject.PmUserId!.Value,
                        targetProject.PmUserId.Value), "Proje müdürü onayı");
                    break;

                case DemoPlanState.RejectedBySiteChief:
                    EnsureSucceeded(plan.StartWork(headOfMasterId, crewType.Id), "İşi başlatma");
                    EnsureSucceeded(plan.SubmitProgress(
                        factQuantity ?? plannedQuantity,
                        factManDay ?? plannedManDay,
                        overtime,
                        comment,
                        headOfMasterId), "Gerçekleşme gönderme");
                    EnsureSucceeded(plan.Reject(
                        WorkStatus.ApprovedBySiteChief,
                        region.SiteChiefUserId!.Value,
                        RequiredRejectionReason()), "Şantiye şefi reddi");
                    break;

                case DemoPlanState.RejectedByPm:
                    EnsureSucceeded(plan.StartWork(headOfMasterId, crewType.Id), "İşi başlatma");
                    EnsureSucceeded(plan.SubmitProgress(
                        factQuantity ?? plannedQuantity,
                        factManDay ?? plannedManDay,
                        overtime,
                        comment,
                        headOfMasterId), "Gerçekleşme gönderme");
                    EnsureSucceeded(plan.Approve(
                        WorkStatus.ApprovedBySiteChief,
                        region.SiteChiefUserId!.Value,
                        region.SiteChiefUserId.Value), "Şantiye şefi onayı");
                    EnsureSucceeded(plan.Reject(
                        WorkStatus.ApprovedByPM,
                        targetProject.PmUserId!.Value,
                        RequiredRejectionReason()), "Proje müdürü reddi");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(state),
                        state,
                        "Desteklenmeyen demo plan durumu.");
            }

            db.DailyPlans.Add(plan);

            if (notification == DemoNotification.Assignment)
            {
                var assignment = Notification.CreateDailyPlanAssigned(
                    headOfMasterId,
                    plan.Id,
                    plan.WorkDate).Value;

                db.Notifications.Add(assignment);
            }
            else if (notification == DemoNotification.Rejection)
            {
                var (recipientId, rejectedByLabel) = state switch
                {
                    DemoPlanState.RejectedBySiteChief => (headOfMasterId, "Şantiye Şefi"),
                    DemoPlanState.RejectedByPm => (region.SiteChiefUserId!.Value, "Project Manager"),
                    _ => throw new InvalidOperationException(
                        "Red bildirimi yalnızca reddedilmiş demo planları için oluşturulabilir.")
                };

                var rejection = Notification.CreateDailyPlanRejected(
                    recipientId,
                    plan.Id,
                    plan.WorkDate,
                    rejectedByLabel,
                    RequiredRejectionReason()).Value;

                db.Notifications.Add(rejection);
            }

            return plan;

            string RequiredRejectionReason() =>
                !string.IsNullOrWhiteSpace(rejectionReason)
                    ? rejectionReason
                    : throw new InvalidOperationException("Reddedilmiş demo planı için red gerekçesi zorunludur.");
        }

        static void EnsureSucceeded(Result result, string operation)
        {
            if (result.IsFailure)
                throw new InvalidOperationException($"Demo seed adımı başarısız: {operation} - {result.Error.Message}");
        }
    }

    private static async Task EnsureRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles =
        [
            Roles.ProjectManager,
            Roles.SiteChief,
            Roles.TechnicalOfficeEngineer,
            Roles.HeadOfMaster
        ];

        foreach (var roleName in roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(
                new IdentityRole<Guid>(roleName) { Id = EntityId.New() });

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Demo rolü oluşturulamadı: {roleName} - " +
                    string.Join(", ", result.Errors.Select(x => x.Description)));
            }
        }
    }

    private static async Task<DemoUsers> SeedUsersAsync(
        UserManager<ApplicationUser> userManager)
    {
        var pm = await EnsureUserAsync(
            userManager,
            "pm1@workplan.local",
            "Murat Karaca",
            Roles.ProjectManager);

        var niSiteChief = await EnsureUserAsync(
            userManager,
            "sc1@workplan.local",
            "Hakan Yıldız",
            Roles.SiteChief);

        var niTechOffice = await EnsureUserAsync(
            userManager,
            "to1@workplan.local",
            "Elif Şahin",
            Roles.TechnicalOfficeEngineer);

        var ciSiteChief = await EnsureUserAsync(
            userManager,
            "sc2@workplan.local",
            "Serkan Aydın",
            Roles.SiteChief);

        var ciTechOffice = await EnsureUserAsync(
            userManager,
            "to2@workplan.local",
            "Burak Çelik",
            Roles.TechnicalOfficeEngineer);

        var bopSiteChief = await EnsureUserAsync(
            userManager,
            "sc3@workplan.local",
            "Cengiz Demir",
            Roles.SiteChief);

        var bopTechOffice = await EnsureUserAsync(
            userManager,
            "to3@workplan.local",
            "Zeynep Arık",
            Roles.TechnicalOfficeEngineer);

        var marineSiteChief = await EnsureUserAsync(
            userManager,
            "sc4@workplan.local",
            "Tolga Kara",
            Roles.SiteChief);

        var marineTechOffice = await EnsureUserAsync(
            userManager,
            "to4@workplan.local",
            "Selin Güneş",
            Roles.TechnicalOfficeEngineer);

        var reactorHom = await EnsureUserAsync(
            userManager,
            "hom1@workplan.local",
            "Ali Kaya",
            Roles.HeadOfMaster);

        var auxiliaryHom = await EnsureUserAsync(
            userManager,
            "hom2@workplan.local",
            "Veli Doğan",
            Roles.HeadOfMaster);

        var dieselHom = await EnsureUserAsync(
            userManager,
            "hom3@workplan.local",
            "Mustafa Arslan",
            Roles.HeadOfMaster);

        var turbineHom = await EnsureUserAsync(
            userManager,
            "hom4@workplan.local",
            "İbrahim Koç",
            Roles.HeadOfMaster);

        var transformerHom = await EnsureUserAsync(
            userManager,
            "hom5@workplan.local",
            "Osman Yıldırım",
            Roles.HeadOfMaster);

        var pipeRackHom = await EnsureUserAsync(
            userManager,
            "hom6@workplan.local",
            "Hüseyin Çelik",
            Roles.HeadOfMaster);

        var wasteHom = await EnsureUserAsync(
            userManager,
            "hom7@workplan.local",
            "Ramazan Aydın",
            Roles.HeadOfMaster);

        var intakeHom = await EnsureUserAsync(
            userManager,
            "hom8@workplan.local",
            "Kemal Şahin",
            Roles.HeadOfMaster);

        var pumpHom = await EnsureUserAsync(
            userManager,
            "hom9@workplan.local",
            "Ömer Faruk Aksoy",
            Roles.HeadOfMaster);

        return new DemoUsers(
            pm,
            niSiteChief,
            niTechOffice,
            ciSiteChief,
            ciTechOffice,
            bopSiteChief,
            bopTechOffice,
            marineSiteChief,
            marineTechOffice,
            reactorHom,
            auxiliaryHom,
            dieselHom,
            turbineHom,
            transformerHom,
            pipeRackHom,
            wasteHom,
            intakeHom,
            pumpHom);
    }

    private static async Task<Guid> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = EntityId.New(),
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await userManager.CreateAsync(
                user,
                DemoPassword);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Demo kullanıcısı oluşturulamadı: {email} - " +
                    string.Join(", ", createResult.Errors.Select(x => x.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Demo kullanıcısına rol atanamadı: {email}/{role} - " +
                    string.Join(", ", roleResult.Errors.Select(x => x.Description)));
            }
        }

        return user.Id;
    }

    private static WorkItemCatalog SeedWorkItemTypes(AppDbContext db)
    {
        WorkItemType Tow(string name) =>
            WorkItemType.Create(name).Value;

        WorkItemType Child(
            WorkItemType parent,
            string name,
            Unit unit = Unit.None) =>
            WorkItemType.Create(
                name,
                parent.Id,
                parent.Level,
                unit).Value;

        // TOW: Betonarme ve gömülü parçalar
        var civil = Tow("Betonarme ve Gömülü Parça İşleri");

        var reactorCivil = Child(civil, "Nükleer Ada Betonarme İşleri");
        var reactorBaseRebar = Child(
            reactorCivil,
            "Reaktör Temel Plakası Donatı Montajı",
            Unit.Ton);
        var wallRebar = Child(
            reactorCivil,
            "Güvenlik Sınıfı Perde Donatı Montajı",
            Unit.Ton);
        var containmentFormwork = Child(
            reactorCivil,
            "Containment Duvar Kalıbı",
            Unit.M2);
        var equipmentFoundationFormwork = Child(
            reactorCivil,
            "Ekipman Temeli Kalıbı",
            Unit.M2);

        var marineCivil = Child(civil, "Deniz Yapıları Betonarme İşleri");
        var intakeStructureRebar = Child(
            marineCivil,
            "Deniz Suyu Alma Yapısı Donatı Montajı",
            Unit.Ton);
        var intakeFormwork = Child(
            marineCivil,
            "Deniz Suyu Alma Yapısı Perde Kalıbı",
            Unit.M2);

        // TOW: Çelik konstrüksiyon
        var structuralSteel = Tow("Çelik Konstrüksiyon İşleri");

        var buildingSteel = Child(
            structuralSteel,
            "Bina ve Platform Çelikleri");
        var structuralSteelInstallation = Child(
            buildingSteel,
            "Acil Dizel Binası Yapısal Çelik Montajı",
            Unit.Ton);
        var turbineHallSteel = Child(
            buildingSteel,
            "Türbin Holü Ana Taşıyıcı Çelik Montajı",
            Unit.Ton);
        var pumpStationSteel = Child(
            buildingSteel,
            "Pompa İstasyonu Platform Çelikleri",
            Unit.Ton);
        var transformerSteelInstallation = Child(
            buildingSteel,
            "Ana Trafo Taşıyıcı Çelik Montajı",
            Unit.Ton);

        // TOW: Mekanik montaj ve borulama
        var mechanical = Tow("Mekanik Montaj ve Borulama İşleri");

        var equipmentInstallation = Child(
            mechanical,
            "Ana Ekipman Montajı");
        var mainEquipmentInstallation = Child(
            equipmentInstallation,
            "Türbin-Jeneratör Ana Ekipman Montajı",
            Unit.Ton);
        var pumpEquipmentInstallation = Child(
            equipmentInstallation,
            "Soğutma Suyu Pompası Montajı",
            Unit.Ton);
        var embeddedPlateInstallation = Child(
            equipmentInstallation,
            "Gömülü Plaka ve Ankraj Montajı",
            Unit.Ton);

        var piping = Child(
            mechanical,
            "Proses Borulama İşleri");
        var processPipingInstallation = Child(
            piping,
            "Proses Borusu Spool Montajı",
            Unit.Ton);
        var coolingWaterPiping = Child(
            piping,
            "Soğutma Suyu Ana Boru Hattı Montajı",
            Unit.Ton);
        var pipeSupportInstallation = Child(
            piping,
            "Boru Askı ve Mesnet Montajı",
            Unit.Ton);

        // TOW: Kaynak ve kalite düzeltmeleri
        var welding = Tow("Kaynak ve Kalite Düzeltme İşleri");

        var nuclearWelding = Child(
            welding,
            "Nükleer Sınıf Kaynak İşleri");
        var containmentLinerWelding = Child(
            nuclearWelding,
            "Containment Liner Saha Kaynağı",
            Unit.Ton);

        // TOW: Elektrik ve enstrümantasyon
        var electrical = Tow("Elektrik ve Enstrümantasyon İşleri");

        var electricalInstallation = Child(
            electrical,
            "Elektrik Montaj İşleri");
        var cableTrayInstallation = Child(
            electricalInstallation,
            "Güvenlik Sınıfı Kablo Tavası Montajı",
            Unit.Ton);
        var groundingGridInstallation = Child(
            electricalInstallation,
            "Topraklama Ağı ve Ring İletken Montajı",
            Unit.Ton);

        // TOW: HVAC
        var hvac = Tow("HVAC ve Havalandırma İşleri");

        var hvacInstallation = Child(
            hvac,
            "Havalandırma Kanalı Montajı");
        var hvacDuctInstallation = Child(
            hvacInstallation,
            "Güvenlik Sınıfı HVAC Kanalı Montajı",
            Unit.Ton);

        // TOW: İzolasyon ve kaplama
        var finishing = Tow("İzolasyon, Kaplama ve Geçici İşler");

        var insulation = Child(
            finishing,
            "Mekanik İzolasyon İşleri");
        var pipingInsulation = Child(
            insulation,
            "Boru Hattı Isı İzolasyonu",
            Unit.M2);

        var coating = Child(
            finishing,
            "Koruyucu Kaplama İşleri");
        var protectiveCoating = Child(
            coating,
            "Kimyasala Dayanımlı Koruyucu Kaplama",
            Unit.M2);

        var temporaryWorks = Child(
            finishing,
            "Geçici Erişim ve İskele İşleri");
        var scaffoldingInstallation = Child(
            temporaryWorks,
            "Endüstriyel İskele Kurulumu",
            Unit.M2);

        // Konvansiyonel ada için özel donatı kalemi
        var turbineCivil = Child(
            civil,
            "Türbin Binası Betonarme İşleri");
        var turbineFoundationRebar = Child(
            turbineCivil,
            "Türbin Temeli ve Pedestal Donatısı",
            Unit.Ton);

        db.WorkItemTypes.AddRange(
            civil,
            reactorCivil,
            reactorBaseRebar,
            wallRebar,
            containmentFormwork,
            equipmentFoundationFormwork,
            marineCivil,
            intakeStructureRebar,
            intakeFormwork,
            turbineCivil,
            turbineFoundationRebar,
            structuralSteel,
            buildingSteel,
            structuralSteelInstallation,
            turbineHallSteel,
            pumpStationSteel,
            transformerSteelInstallation,
            mechanical,
            equipmentInstallation,
            mainEquipmentInstallation,
            pumpEquipmentInstallation,
            embeddedPlateInstallation,
            piping,
            processPipingInstallation,
            coolingWaterPiping,
            pipeSupportInstallation,
            welding,
            nuclearWelding,
            containmentLinerWelding,
            electrical,
            electricalInstallation,
            cableTrayInstallation,
            groundingGridInstallation,
            hvac,
            hvacInstallation,
            hvacDuctInstallation,
            finishing,
            insulation,
            pipingInsulation,
            coating,
            protectiveCoating,
            temporaryWorks,
            scaffoldingInstallation);

        return new WorkItemCatalog(
            reactorBaseRebar,
            wallRebar,
            containmentFormwork,
            equipmentFoundationFormwork,
            intakeStructureRebar,
            intakeFormwork,
            turbineFoundationRebar,
            structuralSteelInstallation,
            turbineHallSteel,
            pumpStationSteel,
            transformerSteelInstallation,
            mainEquipmentInstallation,
            pumpEquipmentInstallation,
            embeddedPlateInstallation,
            processPipingInstallation,
            coolingWaterPiping,
            pipeSupportInstallation,
            containmentLinerWelding,
            cableTrayInstallation,
            groundingGridInstallation,
            hvacDuctInstallation,
            pipingInsulation,
            protectiveCoating,
            scaffoldingInstallation);
    }

    private static CrewCatalog SeedCrewTypes(AppDbContext db)
    {
        var rebarCrew = CrewType.Create(
            "Nükleer Sınıf Demir Donatı Ekibi").Value;
        var formworkCrew = CrewType.Create(
            "Endüstriyel Kalıp Ekibi").Value;
        var steelAssemblyCrew = CrewType.Create(
            "Çelik Konstrüksiyon Montaj Ekibi").Value;
        var certifiedWelderCrew = CrewType.Create(
            "Sertifikalı Kaynak Ekibi").Value;
        var mechanicalAssemblyCrew = CrewType.Create(
            "Mekanik Montaj Ekibi").Value;
        var pipingCrew = CrewType.Create(
            "Boru Montaj Ekibi").Value;
        var electricalCrew = CrewType.Create(
            "Elektrik ve Enstrümantasyon Ekibi").Value;
        var hvacCrew = CrewType.Create(
            "HVAC Montaj Ekibi").Value;
        var insulationCrew = CrewType.Create(
            "İzolasyon Ekibi").Value;
        var paintingCrew = CrewType.Create(
            "Endüstriyel Boya ve Kaplama Ekibi").Value;
        var scaffoldingCrew = CrewType.Create(
            "İskele Kurulum Ekibi").Value;
        var heavyLiftingCrew = CrewType.Create(
            "Ağır Kaldırma ve Rigging Ekibi").Value;

        db.CrewTypes.AddRange(
            rebarCrew,
            formworkCrew,
            steelAssemblyCrew,
            certifiedWelderCrew,
            mechanicalAssemblyCrew,
            pipingCrew,
            electricalCrew,
            hvacCrew,
            insulationCrew,
            paintingCrew,
            scaffoldingCrew,
            heavyLiftingCrew);

        return new CrewCatalog(
            rebarCrew,
            formworkCrew,
            steelAssemblyCrew,
            certifiedWelderCrew,
            mechanicalAssemblyCrew,
            pipingCrew,
            electricalCrew,
            hvacCrew,
            insulationCrew,
            paintingCrew,
            scaffoldingCrew,
            heavyLiftingCrew);
    }

    private enum DemoPlanState
    {
        Assigned,
        InProgress,
        Submitted,
        ApprovedBySiteChief,
        ApprovedByPm,
        RejectedBySiteChief,
        RejectedByPm
    }

    private enum DemoNotification
    {
        None,
        Assignment,
        Rejection
    }

    private sealed record DemoUsers(
        Guid ProjectManagerId,
        Guid NuclearIslandSiteChiefId,
        Guid NuclearIslandTechOfficeId,
        Guid ConventionalIslandSiteChiefId,
        Guid ConventionalIslandTechOfficeId,
        Guid BopSiteChiefId,
        Guid BopTechOfficeId,
        Guid MarineSiteChiefId,
        Guid MarineTechOfficeId,
        Guid ReactorBuildingHomId,
        Guid AuxiliaryBuildingHomId,
        Guid DieselBuildingHomId,
        Guid TurbineBuildingHomId,
        Guid TransformerAreaHomId,
        Guid PipeRackHomId,
        Guid WasteBuildingHomId,
        Guid IntakeStructureHomId,
        Guid PumpStationHomId);

    private sealed record WorkItemCatalog(
        WorkItemType ReactorBaseRebar,
        WorkItemType WallRebar,
        WorkItemType ContainmentFormwork,
        WorkItemType EquipmentFoundationFormwork,
        WorkItemType IntakeStructureRebar,
        WorkItemType IntakeFormwork,
        WorkItemType TurbineFoundationRebar,
        WorkItemType StructuralSteelInstallation,
        WorkItemType TurbineHallSteel,
        WorkItemType PumpStationSteel,
        WorkItemType TransformerSteelInstallation,
        WorkItemType MainEquipmentInstallation,
        WorkItemType PumpEquipmentInstallation,
        WorkItemType EmbeddedPlateInstallation,
        WorkItemType ProcessPipingInstallation,
        WorkItemType CoolingWaterPiping,
        WorkItemType PipeSupportInstallation,
        WorkItemType ContainmentLinerWelding,
        WorkItemType CableTrayInstallation,
        WorkItemType GroundingGridInstallation,
        WorkItemType HvacDuctInstallation,
        WorkItemType PipingInsulation,
        WorkItemType ProtectiveCoating,
        WorkItemType ScaffoldingInstallation);

    private sealed record CrewCatalog(
        CrewType RebarCrew,
        CrewType FormworkCrew,
        CrewType SteelAssemblyCrew,
        CrewType CertifiedWelderCrew,
        CrewType MechanicalAssemblyCrew,
        CrewType PipingCrew,
        CrewType ElectricalCrew,
        CrewType HvacCrew,
        CrewType InsulationCrew,
        CrewType PaintingCrew,
        CrewType ScaffoldingCrew,
        CrewType HeavyLiftingCrew);
}
