using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models.ViewModels;
using Api.Repositories.Base;
using Dapper;
namespace Api.Repositories.Implementations
{
    public class VentaRepository : DapperRepositoryBase, IVentaRepository
    {

        private readonly ApplicationDbContext _context;
        public VentaRepository(IConfiguration configuration, ApplicationDbContext context) : base(configuration)
        {
            _context = context;
        }

        public async Task<Venta> CrearVenta(Venta venta)
        {
            var ventaCreada = await _context.Venta.AddAsync(venta);
            await _context.SaveChangesAsync();
            return ventaCreada.Entity;
        }

        public async Task<Venta?> GetById(uint? id)
        {
            if (id.HasValue)
            {
                var venta = await _context.Venta.FirstOrDefaultAsync(ve => ve.Codigo == id);
                return venta;
            }
            else
            {
                // Traer la última venta ordenada por código descendente
                var venta = await _context.Venta
                    .OrderByDescending(v => v.Codigo)
                    .Where(ve => ve.Estado == 1)
                    .FirstOrDefaultAsync();
                return venta;
            }
        }



        public async Task<IEnumerable<VentaViewModel>> ConsultaVentas(
        string? fecha_desde,
        string? fecha_hasta,
        uint? sucursal,
        uint? cliente,
        uint? vendedor,
        uint? articulo,
        uint? moneda,
        string? factura,
        uint? venta,
        uint? estadoVenta,
        uint? remisiones,
        bool? listaFacturasSinCDC,
        int? page = 1,
        int itemsPorPagina = 50)
        {

            using var connection = GetConnection();
            var where = "1 = 1";
            var limitOffset = "";
            var parameters = new DynamicParameters();

            // Si hay factura o venta, ignorar otros filtros y no usar paginación
            if (!string.IsNullOrEmpty(factura))
            {
                where = "(ve.ve_factura = @Factura OR vl.ve_factura = @Factura)";
                parameters.Add("@Factura", factura);
            }
            else if (venta.HasValue)
            {
                where = "ve.ve_codigo = @Venta";
                parameters.Add("@Venta", venta);
            }
            else
            {
                // Aplicar filtros y paginación solo si no hay factura ni venta
                if (!string.IsNullOrEmpty(fecha_desde))
                {
                    where += " AND ve.ve_fecha >= @FechaDesde";
                    parameters.Add("@FechaDesde", fecha_desde);
                }
                if (!string.IsNullOrEmpty(fecha_hasta))
                {
                    where += " AND ve.ve_fecha <= @FechaHasta";
                    parameters.Add("@FechaHasta", fecha_hasta);
                }
                if (sucursal.HasValue)
                {
                    where += " AND ve.ve_sucursal = @Sucursal";
                    parameters.Add("@Sucursal", sucursal);
                }
                if (cliente.HasValue)
                {
                    where += " AND ve.ve_cliente = @Cliente";
                    parameters.Add("@Cliente", cliente);
                }
                if (vendedor.HasValue)
                {
                    where += " AND ve.ve_operador = @Vendedor";
                    parameters.Add("@Vendedor", vendedor);
                }
                if (articulo.HasValue)
                {
                    where += " AND ve.ve_codigo IN (SELECT z.deve_venta FROM detalle_ventas z WHERE deve_articulo = @Articulo)";
                    parameters.Add("@Articulo", articulo);
                }
                if (moneda.HasValue)
                {
                    where += " AND ve.ve_moneda = @Moneda";
                    parameters.Add("@Moneda", moneda);
                }
                if (estadoVenta.HasValue)
                {
                    switch (estadoVenta.Value)
                    {
                        case 0:
                            where += " AND ve.ve_total = ve.ve_saldo";
                            break;
                        case 1:
                            where += " AND ve.ve_total > ve.ve_saldo";
                            break;
                        case 2:
                            where += " AND ve.ve_estado = 0";
                            break;
                        case 3:
                            where += " AND ve.ve_estado = 1";
                            break;
                    }
                }
                if (remisiones.HasValue)
                {
                    where += " AND ve.ve_estado = 0";
                }
                if (listaFacturasSinCDC.HasValue && listaFacturasSinCDC.Value)
                {
                    where += " AND (ve.ve_cdc = '' OR ve.ve_cdc IS NULL)";
                }

                var offset = ((page ?? 1) - 1) * itemsPorPagina;
                limitOffset = $"LIMIT {itemsPorPagina} OFFSET {offset}";
            }

            var query = $@"
                SELECT
                    ve.ve_codigo AS Codigo,
                    cli.cli_codigo AS CodCliente,
                    cli.cli_razon AS Cliente,
                    ve.ve_moneda AS MonedaId,
                    mo.mo_descripcion AS Moneda,
                    CONCAT(DATE_FORMAT(ve.ve_fecha, '%Y-%m-%d'), ' : ', ve.ve_hora) AS Fecha,
                    v.op_nombre AS Vendedor,
                    o.op_nombre AS Operador,
                    FORMAT(FLOOR(ve.ve_total), 0, 'es_ES') AS Total,
                    FORMAT(FLOOR(ve.ve_descuento), 0, 'es_ES') AS Descuento,
                    FORMAT(FLOOR(ve.ve_saldo), 0, 'es_ES') AS Saldo,
                    IF(ve.ve_credito = 1, 'Crédito', 'Contado') AS Condicion,
                    IF(ve.ve_vencimiento='0001-01-01', '0000-00-00', date_format(ve.ve_vencimiento, '%Y/%m/%d')) AS Vencimiento,
                    IFNULL(IF(ve.ve_factura <> '', ve.ve_factura, vl.ve_factura),'') AS Factura,
                    ve.ve_obs AS Obs,
                    ve.ve_estado AS Estado,
                    IF(ve.ve_estado = 1, 'Activo', 'Anulado') AS EstadoDesc,
                    av.obs AS ObsAnulacion,
                    ve.ve_timbrado AS Timbrado,
                    ve.ve_userpc AS Terminal,
                    dep.dep_descripcion AS Deposito,
                    (SELECT FORMAT(FLOOR(SUM(d.deve_cantidad)), 0, 'es_ES') 
                     FROM detalle_ventas d 
                     WHERE d.deve_venta = ve.ve_codigo) AS TotalArticulos,
                    (SELECT FORMAT(FLOOR(SUM(d.deve_exentas)), 0, 'es_ES')
                     FROM detalle_ventas d 
                     WHERE d.deve_venta = ve.ve_codigo) AS ExentasTotal,
                    (SELECT FORMAT(FLOOR(SUM(d.deve_cinco)), 0, 'es_ES')
                     FROM detalle_ventas d 
                     WHERE d.deve_venta = ve.ve_codigo) AS Iva5Total,
                    (SELECT FORMAT(FLOOR(SUM(d.deve_diez)), 0, 'es_ES')
                     FROM detalle_ventas d 
                     WHERE d.deve_venta = ve.ve_codigo) AS Iva10Total,
                    (SELECT FORMAT(FLOOR(SUM(d.deve_exentas + d.deve_cinco + d.deve_diez)), 0, 'es_ES')
                     FROM detalle_ventas d 
                     WHERE d.deve_venta = ve.ve_codigo) AS SubTotal,
                    (SELECT FORMAT(FLOOR(SUM(d.deve_descuento)), 0, 'es_ES')
                     FROM detalle_ventas d 
                     WHERE d.deve_venta = ve.ve_codigo) AS DescuentoTotal,
                    ve.ve_cdc AS CDC,
                    SUBSTRING_INDEX(SUBSTRING_INDEX(ve.ve_factura, '-', 1), '-', -1) AS Establecimiento,
                    SUBSTRING_INDEX(SUBSTRING_INDEX(ve.ve_factura, '-', 2), '-', -1) AS PuntoEmision,
                    SUBSTRING_INDEX(ve.ve_factura, '-', -1) AS NumeroFactura,
                    cli.cli_ruc AS ClienteRuc,
                    cli.cli_tipo_doc AS TipoDocumento,
                    cli.cli_descripcion AS ClienteDescripcion,
                    cli.cli_dir AS ClienteDireccion,
                    cli.cli_ciudad AS CiudadId,
                    ciu.ciu_descripcion AS CiudadDescripcion,
                    ciu.ciu_distrito AS DistritoId,
                    dis.d_descripcion AS DistritoDescripcion,
                    dis.d_departamento AS DepartamentoId,
                    departamento.dep_descripcion AS DepartamentoDescripcion,
                    cli.cli_tel AS ClienteTelefono,
                    cli.cli_mail AS ClienteEmail,
                    cli.cli_interno AS ClienteCodigoInterno,
                    o.op_nombre AS OperadorNombre,
                    o.op_documento AS OperadorDocumento,
                    ve.ve_cantCuotas AS CantCuotas
                FROM ventas ve
                INNER JOIN clientes cli ON ve.ve_cliente = cli.cli_codigo
                INNER JOIN monedas mo ON ve.ve_moneda = mo.mo_codigo
                INNER JOIN operadores v ON ve.ve_vendedor = v.op_codigo
                INNER JOIN operadores o ON ve.ve_operador = o.op_codigo
                INNER JOIN sucursales s ON ve.ve_sucursal = s.id
                INNER JOIN depositos dep ON ve.ve_deposito = dep.dep_codigo
                LEFT JOIN venta_vental vvl ON vvl.v_venta = ve.ve_codigo
                LEFT JOIN ventasl vl ON vvl.v_vental = vl.ve_codigo
                LEFT JOIN anulacionventa av ON ve.ve_codigo = av.venta
                LEFT JOIN ciudades ciu ON cli.cli_ciudad = ciu.ciu_codigo
                LEFT JOIN distritos dis ON dis.d_codigo = ciu.ciu_distrito
                LEFT JOIN departamentos departamento ON dis.d_departamento = departamento.dep_codigo
                WHERE {where}
                ORDER BY ve.ve_codigo DESC
                {limitOffset}";

            return await connection.QueryAsync<VentaViewModel>(query, parameters);
        }

        public async Task<IEnumerable<DetalleVentaViewModel>> ConsultaDetalles(uint ventaId)
        {
            var connection = GetConnection();
            var parameters = new DynamicParameters();
            var query =
            @$"
              SELECT
                    deve.deve_codigo AS det_codigo,
                    ar.ar_codigo AS art_codigo,
                    ar.ar_codbarra AS codbarra,
                    ar.ar_descripcion AS descripcion,
                    FORMAT(FLOOR(deve.deve_cantidad), 0, 'es_ES') AS cantidad,
                    FORMAT(FLOOR(deve.deve_precio), 0, 'es_ES') AS precio,
                    deve.deve_precio as precio_number,
                    FORMAT(FLOOR(deve.deve_descuento), 0, 'es_ES') AS descuento,
                    deve.deve_descuento as descuento_number,
                    FORMAT(FLOOR(deve.deve_exentas), 0, 'es_ES') AS exentas,
                    deve.deve_exentas as exentas_number,
                    FORMAT(FLOOR(deve.deve_cinco), 0, 'es_ES') AS cinco,
                    deve.deve_cinco as cinco_number,
                    FORMAT(FLOOR(deve.deve_diez), 0, 'es_ES') AS diez,
                    deve.deve_diez as diez_number,
                    al.al_lote AS lote,
                    DATE_FORMAT(al.al_vencimiento, '%Y-%m-%d') AS vencimiento,
                    m.m_largo AS largura,
                    m.m_altura AS altura,
                    m.m_mt2 AS mt2,
                    COALESCE(dae.a_descripcion, '') AS descripcion_editada,
                    ar.ar_kilos AS kilos,
                    um.um_cod_set as unidad_medida
                  FROM
                    detalle_ventas deve
                    LEFT JOIN articulos ar ON ar.ar_codigo = deve.deve_articulo
                    LEFT JOIN detalle_ventas_vencimiento dvv ON dvv.id_detalle_venta = deve.deve_codigo
                    LEFT JOIN articulos_lotes al ON al.al_codigo = dvv.loteid
                    LEFT JOIN detalle_articulo_mt2 m ON m.m_detalle_venta = deve.deve_codigo
                    LEFT JOIN detalle_articulos_editado dae ON deve.deve_codigo = dae.a_detalle_venta
                    LEFT JOIN unidadmedidas um ON um.um_codigo = ar.ar_unidadmedida
                  WHERE
                    deve.deve_venta = @VentaId
                  ORDER BY
                    deve.deve_codigo
            ";
            parameters.Add("@VentaId", ventaId);

            return await connection.QueryAsync<DetalleVentaViewModel>(query, parameters);
        }

        public async Task<IEnumerable<Impresionventa>> GetImpresion(uint venta)
        {
            var connection = GetConnection();
            var parameters = new DynamicParameters();
            var query =
            @$"
              SELECT
                  ve.ve_codigo as codigo,
                  case when ve.ve_credito = 1 then 'CREDITO' else 'CONTADO' end as TipoVenta,
                  date_format(ve.ve_fecha, '%d/%m/%Y') as FechaVenta,
                  date_format(ve.ve_hora, '%H:%i:%s') as HoraVenta,
                  date_format(ve.ve_vencimiento, '%d/%m/%Y') as FechaVencimiento,
                  op.op_nombre as Cajero,
                  op2.op_nombre as Vendedor,
                  cli.cli_razon as Cliente,
                  cli.cli_dir as Direccion,
                  cli.cli_tel as Telefono,
                  cli.cli_ruc as Ruc,
                  cli.cli_mail as ClienteCorreo,
                  ve.ve_total as Subtotal,
                  mo.mo_descripcion as Moneda,
                  dep.dep_descripcion as Deposito,
                  ve.ve_descuento as TotalDescuento,
                  (ve.ve_total - ve.ve_descuento) as TotalAPagar,
                  SUM(deve.deve_exentas) as TotalExentas,
                  SUM(deve.deve_cinco) as TotalCinco,
                  SUM(deve.deve_diez) as TotalDiez,
                  ve.ve_timbrado as Timbrado,
                  ve.ve_factura as Factura,
                  ve.ve_obs as Observacion,
                  ve.ve_cdc,
                  ve.ve_qr,
                  (SELECT EXISTS(SELECT 1 FROM config_recibo_electronica WHERE c_sucursal = ve.ve_sucursal AND c_estado = 1)) as UsaFe,
                  IFNULL((SELECT co_monto FROM cotizaciones WHERE co_fecha = ve.ve_fecha AND co_moneda = 2 ORDER BY co_codigo DESC LIMIT 1),
                  (SELECT co_monto FROM cotizaciones WHERE  co_moneda = 2 ORDER BY co_codigo DESC LIMIT 1)
                  ) as Cotizacion,
                  date_format(
                    (
                      SELECT d_fecha_in FROM definicion_ventas WHERE d_nrotimbrado = ve.ve_timbrado
                      AND d_comprobante = 1 AND d_activo = 1 AND d_sucursal = ve.ve_sucursal order by d_codigo asc LIMIT 1
                    ),
                    '%d/%m/%Y'
                  ) as FacturaValidoDesde,
                  date_format(
                    (
                      SELECT d_fecha_vence FROM definicion_ventas WHERE d_nrotimbrado = ve.ve_timbrado
                      AND d_comprobante = 1 AND d_activo = 1 AND d_sucursal = ve.ve_sucursal order by d_codigo asc LIMIT 1
                    ),
                    '%d/%m/%Y'
                  ) as FacturaValidoHasta,
                  JSON_ARRAYAGG(
                    JSON_OBJECT(
                      'Codigo', deve.deve_articulo,
                      'Descripcion', IFNULL(dae.a_descripcion, ar.ar_descripcion),
                      'Cantidad', deve.deve_cantidad,
                      'Precio', deve.deve_precio,
                      'Descuento', deve.deve_descuento,
                      'Total', deve.deve_cantidad * deve.deve_precio,
                      'Exentas', deve.deve_exentas,
                      'Cinco', deve.deve_cinco,
                      'Diez', deve.deve_diez,
                      'Lote', dvv.lote,
                      'FechaVencimiento', al.al_vencimiento,
                      'ControlVencimiento', ar.ar_vencimiento
                    )
                  ) as detalles,
                  JSON_ARRAYAGG(
                    JSON_OBJECT(
                      'SucursalNombre', suc.descripcion,
                      'SucursalDireccion', suc.direccion,
                      'SucursalTelefono', suc.tel,
                      'SucursalEmpresa', suc.titular,
                      'SucursalRuc', suc.ruc_emp,
                      'SucursalMatriz', CASE WHEN suc.matriz = 1 THEN 'Matriz' ELSE 'Nombre' END,
                      'SucursalCiudad', ciu.ciu_descripcion
                    )
                  ) as SucursalData,
                  JSON_ARRAYAGG(
                    JSON_OBJECT(
                      'Nombre', cf.c_desc_nombre,
                      'Fantasia', cf.c_desc_fantasia,
                      'Direccion', cf.c_direccion,
                      'Telefono', cf.c_telefono,
                      'Ruc', cf.c_ruc,
                      'Correo', cf.c_correo,
                      'DescripcionEstablecimiento', cf.c_descr_establecimiento,
                      'DatoEstablecimiento', cf.c_dato2_establecimiento
                    )
                  ) as ConfiguracionFacturaElectronica
                FROM ventas ve
                INNER JOIN detalle_ventas deve ON ve.ve_codigo = deve.deve_venta
                INNER JOIN articulos ar ON deve.deve_articulo = ar.ar_codigo
                LEFT JOIN detalle_articulos_editado dae ON deve.deve_codigo = dae.a_detalle_venta
                LEFT JOIN detalle_ventas_vencimiento dvv ON deve.deve_codigo = dvv.id_detalle_venta
                LEFT JOIN articulos_lotes al ON dvv.loteid = al.al_codigo
                LEFT JOIN operadores op ON ve.ve_operador = op.op_codigo
                LEFT JOIN operadores op2 ON ve.ve_vendedor = op2.op_codigo
                LEFT JOIN clientes cli ON ve.ve_cliente = cli.cli_codigo
                LEFT JOIN sucursales suc ON ve.ve_sucursal = suc.id
                LEFT JOIN sucursal_ciudad sc ON suc.id = sc.sucursal
                LEFT  JOIN ciudades ciu ON sc.ciudad = ciu.ciu_codigo
                INNER JOIN monedas mo ON ve.ve_moneda = mo.mo_codigo
                INNER JOIN depositos dep ON ve.ve_deposito = dep.dep_codigo
                LEFT JOIN config_factura_electronica cf ON ve.ve_sucursal = cf.c_sucursal
                WHERE ve.ve_codigo = @Venta
            ";
            parameters.Add("Venta", venta);

            return await connection.QueryAsync<Impresionventa>(query, parameters);
        }
    }
}