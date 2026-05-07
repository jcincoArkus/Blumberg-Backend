using Adapters.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Modules.Inventory.Repository;
using Modules.Inventory.Service;

namespace Modules.Inventory;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services)
    {
        services.AddScopedWithSpan<ICategoryRepository, CategoryRepository>();
        services.AddScopedWithSpan<ICategoryService, CategoryService>();

        services.AddScopedWithSpan<IInventorySiteRepository, InventorySiteRepository>();
        services.AddScopedWithSpan<IInventorySiteService, InventorySiteService>();

        services.AddScopedWithSpan<ISiteZoneRepository, SiteZoneRepository>();
        services.AddScopedWithSpan<ISiteZoneService, SiteZoneService>();

        services.AddScopedWithSpan<ISupplierRepository, SupplierRepository>();
        services.AddScopedWithSpan<ISupplierService, SupplierService>();

        services.AddScopedWithSpan<IProductRepository, ProductRepository>();
        services.AddScopedWithSpan<IProductService, ProductService>();

        services.AddScopedWithSpan<ILotRepository, LotRepository>();
        services.AddScopedWithSpan<ILotService, LotService>();

        services.AddScopedWithSpan<IMovementRepository, MovementRepository>();
        services.AddScopedWithSpan<IMovementService, MovementService>();

        services.AddScopedWithSpan<IIntakeShipmentRepository, IntakeShipmentRepository>();
        services.AddScopedWithSpan<IIntakeShipmentService, IntakeShipmentService>();

        services.AddScopedWithSpan<IShipmentLineRepository, ShipmentLineRepository>();
        services.AddScopedWithSpan<IShipmentLineService, ShipmentLineService>();

        return services;
    }
}
