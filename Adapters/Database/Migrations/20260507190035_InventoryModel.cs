using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adapters.Database.Migrations
{
    /// <inheritdoc />
    public partial class InventoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$;
");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inv_sites",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inv_sites_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_suppliers_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    kg_per_box = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    shelf_life_days = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "site_zones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_zones_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_site_zones_site_id",
                        column: x => x.site_id,
                        principalTable: "inv_sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "intake_shipments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    po_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    driver = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiving_zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    cold_chain_temp_c = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    arrived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_intake_shipments_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_intake_shipments_site_id",
                        column: x => x.site_id,
                        principalTable: "inv_sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_intake_shipments_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lots",
                columns: table => new
                {
                    lot_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    qty = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entry_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    zone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cost_per_unit = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lots_lot_code", x => x.lot_code);
                    table.ForeignKey(
                        name: "fk_lots_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lots_site_id",
                        column: x => x.site_id,
                        principalTable: "inv_sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_lots_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "intake_shipment_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    qty = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cost_per_unit = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_intake_shipment_lines_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_intake_shipment_lines_lot_code",
                        column: x => x.lot_code,
                        principalTable: "lots",
                        principalColumn: "lot_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_intake_shipment_lines_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_intake_shipment_lines_shipment_id",
                        column: x => x.shipment_id,
                        principalTable: "intake_shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    qty = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lot_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dest_site_id = table.Column<Guid>(type: "uuid", nullable: true),
                    performed_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movements_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_movements_dest_site_id",
                        column: x => x.dest_site_id,
                        principalTable: "inv_sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movements_lot_code",
                        column: x => x.lot_code,
                        principalTable: "lots",
                        principalColumn: "lot_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movements_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_movements_site_id",
                        column: x => x.site_id,
                        principalTable: "inv_sites",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_intake_lines_product_id",
                table: "intake_shipment_lines",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_intake_lines_shipment_id",
                table: "intake_shipment_lines",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_intake_shipment_lines_lot_code",
                table: "intake_shipment_lines",
                column: "lot_code");

            migrationBuilder.CreateIndex(
                name: "idx_intake_shipments_arrived_at",
                table: "intake_shipments",
                column: "arrived_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_intake_shipments_site_id",
                table: "intake_shipments",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "idx_intake_shipments_supplier_id",
                table: "intake_shipments",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_intake_shipments_po_reference",
                table: "intake_shipments",
                column: "po_reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_lots_entry_at",
                table: "lots",
                column: "entry_at");

            migrationBuilder.CreateIndex(
                name: "idx_lots_expires_at",
                table: "lots",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_lots_product_id",
                table: "lots",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_lots_site_id",
                table: "lots",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_lots_supplier_id",
                table: "lots",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "idx_movements_lot_code",
                table: "movements",
                column: "lot_code");

            migrationBuilder.CreateIndex(
                name: "idx_movements_occurred_at",
                table: "movements",
                column: "occurred_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_movements_product_id",
                table: "movements",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_movements_site_id",
                table: "movements",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "idx_movements_type",
                table: "movements",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_movements_dest_site_id",
                table: "movements",
                column: "dest_site_id");

            migrationBuilder.CreateIndex(
                name: "idx_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_products_sku",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_site_zones_site_id",
                table: "site_zones",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "IX_site_zones_site_id_name",
                table: "site_zones",
                columns: new[] { "site_id", "name" },
                unique: true);

            migrationBuilder.Sql(@"
CREATE TRIGGER trg_categories_updated_at
  BEFORE UPDATE ON categories FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_inv_sites_updated_at
  BEFORE UPDATE ON inv_sites FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_suppliers_updated_at
  BEFORE UPDATE ON suppliers FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_products_updated_at
  BEFORE UPDATE ON products FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_lots_updated_at
  BEFORE UPDATE ON lots FOR EACH ROW EXECUTE FUNCTION set_updated_at();
CREATE TRIGGER trg_intake_shipments_updated_at
  BEFORE UPDATE ON intake_shipments FOR EACH ROW EXECUTE FUNCTION set_updated_at();
");

            migrationBuilder.Sql(@"
INSERT INTO categories (id, name, color) VALUES
  ('11111111-0001-0001-0001-000000000001', 'Fruit',        'teal'),
  ('11111111-0001-0001-0001-000000000002', 'Vegetables',   'blue-gray'),
  ('11111111-0001-0001-0001-000000000003', 'Citrus',       'yellow'),
  ('11111111-0001-0001-0001-000000000004', 'Leafy Greens', 'teal'),
  ('11111111-0001-0001-0001-000000000005', 'Herbs',        'mint')
ON CONFLICT (id) DO NOTHING;

INSERT INTO inv_sites (id, name) VALUES
  ('22222222-0002-0002-0002-000000000001', 'CDMX · Iztapalapa'),
  ('22222222-0002-0002-0002-000000000002', 'Guadalajara · Zapopan'),
  ('22222222-0002-0002-0002-000000000003', 'Monterrey · Apodaca')
ON CONFLICT (id) DO NOTHING;

INSERT INTO site_zones (site_id, name) VALUES
  ('22222222-0002-0002-0002-000000000001', 'Cold Room A'),
  ('22222222-0002-0002-0002-000000000001', 'Cold Room B'),
  ('22222222-0002-0002-0002-000000000001', 'Dry Bay 1'),
  ('22222222-0002-0002-0002-000000000001', 'Dock 3'),
  ('22222222-0002-0002-0002-000000000002', 'Cold Room 1'),
  ('22222222-0002-0002-0002-000000000002', 'Cold Room 2'),
  ('22222222-0002-0002-0002-000000000002', 'Bay 4'),
  ('22222222-0002-0002-0002-000000000003', 'Refrig. 1'),
  ('22222222-0002-0002-0002-000000000003', 'Refrig. 2'),
  ('22222222-0002-0002-0002-000000000003', 'Dock 2')
ON CONFLICT (site_id, name) DO NOTHING;

INSERT INTO suppliers (id, name) VALUES
  ('33333333-0003-0003-0003-000000000001', 'Frutería Morales'),
  ('33333333-0003-0003-0003-000000000002', 'Verduras del Valle'),
  ('33333333-0003-0003-0003-000000000003', 'Cítricos del Sur'),
  ('33333333-0003-0003-0003-000000000004', 'Hojas Frescas S.A.')
ON CONFLICT (id) DO NOTHING;

INSERT INTO products (id, sku, name, category_id, unit, kg_per_box, shelf_life_days, price) VALUES
  ('44444444-0004-0004-0004-000000000001', 'AVO-HASS', 'Avocado · Hass',        '11111111-0001-0001-0001-000000000001', 'kg',   NULL,  10, 35.00),
  ('44444444-0004-0004-0004-000000000002', 'TOM-ROMA', 'Tomato · Roma',          '11111111-0001-0001-0001-000000000002', 'kg',   NULL,   7, 18.50),
  ('44444444-0004-0004-0004-000000000003', 'TOM-SAL',  'Tomato · Saladette',     '11111111-0001-0001-0001-000000000002', 'kg',   NULL,   7, 16.00),
  ('44444444-0004-0004-0004-000000000004', 'LIM-MEX',  'Lime · Mexicano',        '11111111-0001-0001-0001-000000000003', 'kg',   NULL,  14, 22.00),
  ('44444444-0004-0004-0004-000000000005', 'LEM-AMA',  'Lemon · Amarillo',       '11111111-0001-0001-0001-000000000003', 'kg',   NULL,  14, 24.00),
  ('44444444-0004-0004-0004-000000000006', 'ORA-VAL',  'Orange · Valencia',      '11111111-0001-0001-0001-000000000003', 'box', 20.0,  21, 320.00),
  ('44444444-0004-0004-0004-000000000007', 'LET-ROM',  'Lettuce · Romaine',      '11111111-0001-0001-0001-000000000004', 'unit', NULL,   5, 12.00),
  ('44444444-0004-0004-0004-000000000008', 'SPI-BAB',  'Spinach · Baby',         '11111111-0001-0001-0001-000000000004', 'kg',   NULL,   4, 45.00),
  ('44444444-0004-0004-0004-000000000009', 'CIL-FRE',  'Cilantro',               '11111111-0001-0001-0001-000000000005', 'kg',   NULL,   5, 60.00),
  ('44444444-0004-0004-0004-000000000010', 'HIE-BAE',  'Hierbabuena (Mint)',      '11111111-0001-0001-0001-000000000005', 'kg',   NULL,   5, 80.00),
  ('44444444-0004-0004-0004-000000000011', 'CHI-JAL',  'Chile · Jalapeño',       '11111111-0001-0001-0001-000000000002', 'kg',   NULL,  10, 28.00),
  ('44444444-0004-0004-0004-000000000012', 'CHI-SER',  'Chile · Serrano',        '11111111-0001-0001-0001-000000000002', 'kg',   NULL,  10, 30.00),
  ('44444444-0004-0004-0004-000000000013', 'MAN-ATA',  'Mango · Ataulfo',        '11111111-0001-0001-0001-000000000001', 'kg',   NULL,   7, 25.00),
  ('44444444-0004-0004-0004-000000000014', 'PAP-MAR',  'Papaya · Maradol',       '11111111-0001-0001-0001-000000000001', 'kg',   NULL,  14, 20.00)
ON CONFLICT (id) DO NOTHING;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "intake_shipment_lines");

            migrationBuilder.DropTable(
                name: "movements");

            migrationBuilder.DropTable(
                name: "site_zones");

            migrationBuilder.DropTable(
                name: "intake_shipments");

            migrationBuilder.DropTable(
                name: "lots");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "inv_sites");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS set_updated_at CASCADE;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS shipment_status;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS movement_type;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS lot_unit;");
            migrationBuilder.Sql("DROP TYPE IF EXISTS product_unit;");
        }
    }
}
