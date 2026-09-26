using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entity;

namespace Modules.Demo;

/// <summary>
/// Rolling, deterministic inventory activity for the demo (idempotent):
/// <list type="bullet">
/// <item>1–2 intake shipments per day for the last 7 days (PO, lines, one lot per line, one "intake" movement per
/// line — the same shape the frontend's Intake flow creates);</item>
/// <item>per lot, a deterministic plan of later movements — daily "output" (sales), occasional "waste",
/// "adjustment" and "transfer" (which creates the destination lot), and disposal of expired leftovers — that is
/// applied as its time comes, decrementing Lot.Qty so stock, value and expiration KPIs stay consistent.</item>
/// </list>
/// Lots created by users (e.g. via the UI) are never touched. Days are keyed by date so every restart / tick only
/// adds what is missing and data keeps looking fresh no matter when the instance wakes up.
/// </summary>
internal sealed class DemoInventorySeeder(ILogger logger)
{
    private const int DaysBack = 8;
    private const int PruneAfterDays = 12;
    private const int PlanLookbackDays = 35;
    private static readonly TimeSpan SiteUtcOffset = TimeSpan.FromHours(-6); // Mexico (CST)

    private static readonly string[] Receivers = ["María López", "Jorge Ibarra", "Fernanda González", "Luis Ramírez"];
    private static readonly string[] Staff = ["María López", "Jorge Ibarra", "Fernanda González", "Luis Ramírez", "Ana Torres", "Carlos Méndez"];
    private static readonly string[] Drivers = ["Raúl Hernández", "Miguel Ángel Ruiz", "Óscar Salinas", "Pedro Castillo", "Juan Carlos Vega"];
    private static readonly string[] Vehicles = ["Refrigerated truck · JAL-48-21", "Reefer van · NLE-7K-09", "Refrigerated truck · CDMX-3F-77", "Box truck · MEX-12-54", "Reefer trailer · SIN-90-33"];
    private static readonly string[] Customers = ["Soriana", "La Comer", "Chedraui", "Walmart CEDIS", "Central de Abasto", "HORECA account", "City Market"];
    private static readonly string[] HandlingWasteNotes = ["Damaged in handling", "Quality rejection — bruising", "Crushed cases on arrival", "Mold found at QA check"];

    private sealed record ShipmentPlan(
        string Po, DateTime ArrivedAt, Guid SiteId, string Zone, Guid SupplierId, decimal TempC,
        string ReceivedBy, string Driver, string Vehicle, List<LinePlan> Lines);

    private sealed record LinePlan(string LotCode, InventoryProduct Product, decimal Qty, string LotUnit, string LineUnit,
        decimal CostPerUnit, DateTime ExpiresAt, int Index);

    private sealed record MovementPlan(string Type, DateTime At, decimal Fraction, string PerformedBy, string? Note,
        Guid? DestSiteId = null, string? DestLotCode = null, string? DestZone = null, bool Remainder = false);

    public async Task RunAsync(DateTime now, CancellationToken ct)
    {
        await using var ctx = DemoDb.Create(null);

        var sites = await ctx.InventorySites.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);
        var zones = await ctx.SiteZones.AsNoTracking().OrderBy(z => z.Name).ToListAsync(ct);
        var suppliers = await ctx.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct);
        var products = await ctx.InventoryProducts.AsNoTracking().Include(p => p.Category)
            .OrderBy(p => p.Sku).ToListAsync(ct);

        if (sites.Count == 0 || suppliers.Count == 0 || products.Count == 0)
        {
            logger.LogInformation("Demo: inventory reference data missing (sites/suppliers/products); skipping inventory activity");
            return;
        }

        var zonesBySite = sites.ToDictionary(
            s => s.Id,
            s => zones.Where(z => z.SiteId == s.Id).Select(z => z.Name).DefaultIfEmpty("Receiving").ToList());

        var today = DateOnly.FromDateTime(now);
        var plans = new List<ShipmentPlan>();
        for (var d = PlanLookbackDays; d >= 0; d--)
            plans.AddRange(PlanDay(today.AddDays(-d), sites, zonesBySite, suppliers, products));

        var planPos = plans.Select(p => p.Po).ToList();
        var existingShipments = await ctx.IntakeShipments
            .Where(s => planPos.Contains(s.PoReference))
            .Select(s => new { s.Id, s.PoReference, s.SupplierId, s.ReceivedBy })
            .ToListAsync(ct);
        var existingByPo = existingShipments.ToDictionary(s => s.PoReference);

        // ---- 1) new shipments (only within the visible window; never in the future)
        var windowStart = today.AddDays(-DaysBack);
        var newShipments = 0;
        var newLots = 0;
        foreach (var plan in plans)
        {
            if (plan.ArrivedAt > now || DateOnly.FromDateTime(plan.ArrivedAt) < windowStart || existingByPo.ContainsKey(plan.Po))
                continue;
            var codes = plan.Lines.Select(l => l.LotCode).ToList();
            if (await ctx.Lots.AnyAsync(l => codes.Contains(l.LotCode), ct))
                continue;

            var shipment = new IntakeShipment
            {
                Id = Guid.NewGuid(),
                PoReference = plan.Po,
                SupplierId = plan.SupplierId,
                Vehicle = plan.Vehicle,
                Driver = plan.Driver,
                SiteId = plan.SiteId,
                ReceivingZone = plan.Zone,
                ColdChainTempC = plan.TempC,
                ArrivedAt = plan.ArrivedAt,
                ReceivedBy = plan.ReceivedBy,
                Status = "received",
                CreatedAt = plan.ArrivedAt.AddMinutes(25),
                UpdatedAt = plan.ArrivedAt.AddMinutes(25),
            };
            ctx.IntakeShipments.Add(shipment);

            foreach (var line in plan.Lines)
            {
                var recordedAt = plan.ArrivedAt.AddMinutes(25 + line.Index);
                ctx.Lots.Add(new Lot
                {
                    LotCode = line.LotCode,
                    ProductId = line.Product.Id,
                    Qty = line.Qty,
                    Unit = line.LotUnit,
                    EntryAt = plan.ArrivedAt,
                    ExpiresAt = line.ExpiresAt,
                    SiteId = plan.SiteId,
                    Zone = plan.Zone,
                    SupplierId = plan.SupplierId,
                    IntakeShipmentId = shipment.Id,
                    CostPerUnit = line.CostPerUnit,
                    CreatedAt = recordedAt,
                    UpdatedAt = recordedAt,
                });
                ctx.IntakeShipmentLines.Add(new IntakeShipmentLine
                {
                    Id = Guid.NewGuid(),
                    ShipmentId = shipment.Id,
                    ProductId = line.Product.Id,
                    LotCode = line.LotCode,
                    Qty = line.Qty,
                    Unit = line.LineUnit,
                    CostPerUnit = line.CostPerUnit,
                    CreatedAt = recordedAt,
                });
                ctx.Movements.Add(new Movement
                {
                    Id = Guid.NewGuid(),
                    Type = "intake",
                    OccurredAt = plan.ArrivedAt,
                    ProductId = line.Product.Id,
                    Qty = line.Qty,
                    Unit = line.LotUnit,
                    LotCode = line.LotCode,
                    SiteId = plan.SiteId,
                    PerformedBy = plan.ReceivedBy,
                    Note = $"Intake shipment {plan.Po} · Temp: {plan.TempC:0.#}°C",
                    CreatedAt = recordedAt,
                });
                newLots++;
            }

            existingByPo[plan.Po] = new { shipment.Id, shipment.PoReference, shipment.SupplierId, shipment.ReceivedBy };
            newShipments++;
        }

        await ctx.SaveChangesAsync(ct);

        // ---- 2) apply due movements for demo lots (ours only: PO exists AND matches the plan's supplier/receiver)
        var ourPlans = plans.Where(p => existingByPo.TryGetValue(p.Po, out var s)
            && s.SupplierId == p.SupplierId && s.ReceivedBy == p.ReceivedBy).ToList();
        var ourCodes = ourPlans.SelectMany(p => p.Lines.Select(l => l.LotCode)).ToList();
        if (ourCodes.Count == 0)
        {
            logger.LogInformation("Demo: inventory — no demo lots yet");
            return;
        }

        var lots = await ctx.Lots.Where(l => ourCodes.Contains(l.LotCode)).ToDictionaryAsync(l => l.LotCode, ct);
        var existingMovements = await ctx.Movements.AsNoTracking()
            .Where(m => m.LotCode != null && ourCodes.Contains(m.LotCode))
            .Select(m => new { m.LotCode, m.Type, m.OccurredAt })
            .ToListAsync(ct);
        var done = existingMovements.Select(m => Key(m.LotCode!, m.Type, m.OccurredAt)).ToHashSet();

        // transfer destination lots (disposed when expired, otherwise held as stock)
        var destCodes = new List<string>();
        var newMovements = 0;

        foreach (var plan in ourPlans)
        {
            foreach (var line in plan.Lines)
            {
                if (!lots.TryGetValue(line.LotCode, out var lot))
                    continue;

                foreach (var mv in PlanMovements(plan, line, sites, zonesBySite))
                {
                    if (mv.DestLotCode != null)
                        destCodes.Add(mv.DestLotCode);
                    if (mv.At > now || done.Contains(Key(lot.LotCode, mv.Type, mv.At)))
                        continue;

                    var qty = mv.Remainder ? lot.Qty : RoundQty(line.Qty * mv.Fraction, lot.Unit);
                    if (mv.Type == "adjustment")
                        qty = Math.Max(qty, -lot.Qty);
                    else
                        qty = Math.Min(qty, lot.Qty);
                    if (qty == 0)
                        continue;

                    ctx.Movements.Add(new Movement
                    {
                        Id = Guid.NewGuid(),
                        Type = mv.Type,
                        OccurredAt = mv.At,
                        ProductId = lot.ProductId,
                        Qty = qty,
                        Unit = lot.Unit,
                        LotCode = lot.LotCode,
                        SiteId = lot.SiteId,
                        DestSiteId = mv.DestSiteId,
                        PerformedBy = mv.PerformedBy,
                        Note = mv.Note,
                        CreatedAt = mv.At,
                    });
                    lot.Qty = mv.Type == "adjustment" ? lot.Qty + qty : lot.Qty - qty;
                    lot.UpdatedAt = mv.At;
                    done.Add(Key(lot.LotCode, mv.Type, mv.At));
                    newMovements++;

                    if (mv.Type == "transfer" && mv.DestLotCode != null && !lots.ContainsKey(mv.DestLotCode)
                        && !await ctx.Lots.AnyAsync(l => l.LotCode == mv.DestLotCode, ct))
                    {
                        var dest = new Lot
                        {
                            LotCode = mv.DestLotCode,
                            ProductId = lot.ProductId,
                            Qty = qty,
                            Unit = lot.Unit,
                            EntryAt = mv.At,
                            ExpiresAt = lot.ExpiresAt,
                            SiteId = mv.DestSiteId!.Value,
                            Zone = mv.DestZone ?? "Receiving",
                            SupplierId = lot.SupplierId,
                            CostPerUnit = lot.CostPerUnit,
                            CreatedAt = mv.At,
                            UpdatedAt = mv.At,
                        };
                        ctx.Lots.Add(dest);
                        lots[dest.LotCode] = dest;
                        newLots++;
                    }
                }
            }
        }

        // Expired leftovers in transfer destination lots get disposed too.
        foreach (var code in destCodes.Distinct())
        {
            if (!lots.TryGetValue(code, out var lot) || lot.Qty <= 0)
                continue;
            var rng = new DemoRng(DemoHash.Of("dispose-" + code));
            var at = lot.ExpiresAt.AddHours(rng.Range(30, 100));
            if (at > now || done.Contains(Key(code, "waste", at)))
                continue;
            ctx.Movements.Add(new Movement
            {
                Id = Guid.NewGuid(),
                Type = "waste",
                OccurredAt = at,
                ProductId = lot.ProductId,
                Qty = lot.Qty,
                Unit = lot.Unit,
                LotCode = lot.LotCode,
                SiteId = lot.SiteId,
                PerformedBy = rng.Pick(Staff),
                Note = "Expired — disposed",
                CreatedAt = at,
            });
            lot.Qty = 0;
            lot.UpdatedAt = at;
            newMovements++;
        }

        await ctx.SaveChangesAsync(ct);

        // ---- 3) keep the dataset bounded: drop old demo shipments once all their lots are fully depleted
        var pruned = 0;
        foreach (var plan in ourPlans.Where(p => p.ArrivedAt < now.AddDays(-PruneAfterDays)))
        {
            var codes = plan.Lines.Select(l => l.LotCode).ToList();
            codes.AddRange(plan.Lines.SelectMany(l => PlanMovements(plan, l, sites, zonesBySite))
                .Where(m => m.DestLotCode != null).Select(m => m.DestLotCode!));
            if (await ctx.Lots.AnyAsync(l => codes.Contains(l.LotCode) && l.Qty > 0, ct))
                continue;

            await ctx.Movements.Where(m => m.LotCode != null && codes.Contains(m.LotCode)).ExecuteDeleteAsync(ct);
            await ctx.IntakeShipmentLines.Where(l => l.LotCode != null && codes.Contains(l.LotCode)).ExecuteDeleteAsync(ct);
            await ctx.Lots.Where(l => codes.Contains(l.LotCode)).ExecuteDeleteAsync(ct);
            await ctx.IntakeShipments.Where(s => s.PoReference == plan.Po).ExecuteDeleteAsync(ct);
            pruned++;
        }

        if (newShipments > 0 || newLots > 0 || newMovements > 0 || pruned > 0)
            logger.LogInformation("Demo: inventory — {Shipments} shipments, {Lots} lots, {Movements} movements added; {Pruned} old shipments pruned",
                newShipments, newLots, newMovements, pruned);
    }

    private static string Key(string lotCode, string type, DateTime at) => $"{lotCode}|{type}|{at.ToUniversalTime().Ticks / TimeSpan.TicksPerSecond}";

    private static decimal RoundQty(decimal qty, string unit) => unit == "kg"
        ? Math.Round(qty, 1, MidpointRounding.AwayFromZero)
        : Math.Round(qty, 0, MidpointRounding.AwayFromZero);

    private static DateTime Utc(DateTime t) => DateTime.SpecifyKind(t, DateTimeKind.Utc);

    // ------------------------------------------------------------------ plans

    private static List<ShipmentPlan> PlanDay(
        DateOnly day,
        List<InventorySite> sites,
        Dictionary<Guid, List<string>> zonesBySite,
        List<Supplier> suppliers,
        List<InventoryProduct> products)
    {
        var rng = new DemoRng(DemoHash.Combine(DemoHash.Of("inventory-day"), day.DayNumber));
        var count = rng.Chance(0.6) ? 2 : 1;
        var result = new List<ShipmentPlan>();
        var dayStart = day.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        for (var k = 0; k < count; k++)
        {
            var supplier = suppliers[(day.DayNumber + k * 3) % suppliers.Count];
            var site = sites[(day.DayNumber * 2 + k) % sites.Count];
            var siteZones = zonesBySite[site.Id];
            var coldZones = siteZones.Where(z => z.Contains("Cold", StringComparison.OrdinalIgnoreCase)
                || z.Contains("Refrig", StringComparison.OrdinalIgnoreCase)).ToList();
            var zone = rng.Pick(coldZones.Count > 0 ? coldZones : siteZones);
            // 06:00–13:00 local → 12:00–19:00 UTC
            var arrivedAt = dayStart.AddHours(12 + k * 3.5).AddMinutes(rng.Next(0, 150));
            arrivedAt = Utc(new DateTime(arrivedAt.Ticks - arrivedAt.Ticks % TimeSpan.FromMinutes(5).Ticks));

            var candidates = ProductsForSupplier(supplier, products);
            var lineCount = rng.Chance(0.5) ? 2 : 1;
            var localDate = arrivedAt + SiteUtcOffset;
            var lines = new List<LinePlan>();
            var used = new HashSet<Guid>();
            for (var i = 0; i < lineCount; i++)
            {
                var product = rng.Pick(candidates);
                if (!used.Add(product.Id))
                    continue;

                var isKg = string.Equals(product.Unit, "kg", StringComparison.OrdinalIgnoreCase);
                var isBox = string.Equals(product.Unit, "box", StringComparison.OrdinalIgnoreCase);
                var qty = isKg
                    ? Math.Round((decimal)rng.Range(40, 260), 0)
                    : isBox ? rng.Next(12, 45) : rng.Next(60, 260);
                var cost = Math.Round(product.Price * (decimal)rng.Range(0.45, 0.65), 2);
                var shelf = Math.Max(product.ShelfLifeDays, 2);
                var letters = new string(Enumerable.Range(0, 3).Select(_ => (char)('A' + rng.Next(0, 26))).ToArray());
                var seq = 10 + k * 10 + i;
                lines.Add(new LinePlan(
                    LotCode: $"L-{localDate:yyMMdd}-{seq:D2}{letters}",
                    Product: product,
                    Qty: qty,
                    LotUnit: isKg ? "kg" : "unit",
                    LineUnit: isKg ? "kg" : isBox ? "box" : "unit",
                    CostPerUnit: cost,
                    ExpiresAt: arrivedAt.AddDays(shelf),
                    Index: i));
            }

            result.Add(new ShipmentPlan(
                Po: $"PO-{day:yyyy}-{day.DayOfYear * 10 + k + 1:D4}",
                ArrivedAt: arrivedAt,
                SiteId: site.Id,
                Zone: zone,
                SupplierId: supplier.Id,
                TempC: Math.Round((decimal)rng.Range(2.5, 5.5), 1),
                ReceivedBy: rng.Pick(Receivers),
                Driver: rng.Pick(Drivers),
                Vehicle: rng.Pick(Vehicles),
                Lines: lines));
        }

        return result;
    }

    private static List<InventoryProduct> ProductsForSupplier(Supplier supplier, List<InventoryProduct> products)
    {
        var name = supplier.Name.ToLowerInvariant();
        string[] categories = name switch
        {
            _ when name.Contains("cítric") || name.Contains("citric") => ["citrus"],
            _ when name.Contains("hoja") => ["leafy", "herb"],
            _ when name.Contains("verdura") => ["vegetable"],
            _ when name.Contains("frut") => ["fruit"],
            _ => [],
        };
        var matching = products.Where(p => categories.Any(c =>
            (p.Category?.Name ?? string.Empty).Contains(c, StringComparison.OrdinalIgnoreCase))).ToList();
        return matching.Count > 0 ? matching : products;
    }

    private static IEnumerable<MovementPlan> PlanMovements(
        ShipmentPlan shipment, LinePlan line, List<InventorySite> sites, Dictionary<Guid, List<string>> zonesBySite)
    {
        var rng = new DemoRng(DemoHash.Of("lot-plan-" + line.LotCode));
        var arrived = shipment.ArrivedAt;
        var selector = (int)(DemoHash.Of(line.LotCode) % 12);
        var list = new List<MovementPlan>();

        // Handling waste on some lots
        if (selector % 3 == 2)
        {
            list.Add(new MovementPlan("waste", RoundTime(arrived.AddHours(rng.Range(30, 70))), (decimal)rng.Range(0.02, 0.06),
                rng.Pick(Staff), rng.Pick(HandlingWasteNotes)));
        }

        // Cycle count corrections (mostly negative)
        if (selector % 4 == 1)
        {
            var sign = rng.Chance(0.75) ? -1m : 1m;
            list.Add(new MovementPlan("adjustment", RoundTime(arrived.AddHours(rng.Range(40, 90))), sign * (decimal)rng.Range(0.005, 0.02),
                rng.Pick(Staff), "Cycle count correction"));
        }

        // Transfers to another site (creates the destination lot)
        if (selector % 5 == 0 && sites.Count > 1)
        {
            var others = sites.Where(s => s.Id != shipment.SiteId).ToList();
            var dest = others[rng.Next(0, others.Count)];
            var destZone = rng.Pick(zonesBySite[dest.Id]);
            var suffix = line.LotCode[^3..];
            var seq = int.Parse(line.LotCode.Substring(9, 2)) + 50;
            var destCode = $"{line.LotCode[..9]}{seq:D2}{suffix}";
            list.Add(new MovementPlan("transfer", RoundTime(arrived.AddHours(rng.Range(20, 44))), (decimal)rng.Range(0.15, 0.25),
                rng.Pick(Staff), $"Transfer to {dest.Name} · {destZone}", dest.Id, destCode, destZone));
        }

        // Daily sales until the lot is gone or expires
        var dailyFraction = rng.Range(0.08, 0.17);
        for (var day = 1; ; day++)
        {
            var at = RoundTime(arrived.Date.AddDays(day).AddHours(rng.Range(15, 23)));
            if (at >= line.ExpiresAt || day > 30)
                break;
            var fraction = (decimal)(dailyFraction * rng.Range(0.7, 1.3));
            list.Add(new MovementPlan("output", Utc(at), fraction, rng.Pick(Staff), $"Sale · {rng.Pick(Customers)}"));
        }

        // Expired leftovers are disposed 30–100 h after expiry (until then the lot shows as "past expiration")
        list.Add(new MovementPlan("waste", RoundTime(line.ExpiresAt.AddHours(rng.Range(30, 100))), 0m, rng.Pick(Staff),
            "Expired — disposed", Remainder: true));

        return list.OrderBy(m => m.At);
    }

    private static DateTime RoundTime(DateTime t) => Utc(new DateTime(t.Ticks - t.Ticks % TimeSpan.TicksPerMinute));
}
