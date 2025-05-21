using Dapper;
using Api.Repositories.Base;
using Api.Repositories.Interfaces;
using Api.Data;
using Api.Models.Entities;
using Api.Models.ViewModels;

namespace Api.Repositories.Implementations
{
    public class PedidosRepository : DapperRepositoryBase, IPedidosRepository
    {
        private readonly ApplicationDbContext _context;
        public PedidosRepository(IConfiguration configuration, ApplicationDbContext context) : base(configuration)
        {
            _context = context;
        }

        public async Task<Pedido> GetById(uint codigo)
        {
            var pedido = await _context.Pedido.FindAsync(codigo);
            if (pedido != null)
            {
                return pedido;
            }
            else
            {
                return new Pedido { };
            }
        }

        public async Task<Pedido> CrearPedido(Pedido pedido)
        {
            var pedidoCreado = await _context.Pedido.AddAsync(pedido);
            await _context.SaveChangesAsync();

            return pedidoCreado.Entity;
        }

        public async Task<string> ProcesarPedido(int idPedido)
        {
            var connection = GetConnection();


            var query = @"
                UPDATE pedidos
                SET p_estado = 2
                WHERE p_id = @idPedido
                ";

            var parameter = new DynamicParameters();
            parameter.Add("idPedido", idPedido);
            var result = await connection.ExecuteAsync(query, parameter);
            if (result > 0)
            {
                return "Pedido procesado correctamente";
            }
            else
            {
                return "Error al procesar el pedido";
            }
        }

        public async Task<IEnumerable<PedidoViewModel>> GetPedidos(
            string? fechaDesde,
            string? fechaHasta,
            string? nroPedido,
            int? articulo,
            string? clientes,
            string? vendedores,
            string? sucursales,
            string? estado,
            int? moneda,
            string? factura
        )
        {
            var connection = GetConnection();
            var where = "1 = 1 ";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(nroPedido))
            {
                where += " AND p.p_codigo = @nroPedido";
                parameters.Add("nroPedido", nroPedido);
            }
            else
            {
                if (!string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta) && fechaDesde.Length !=0 && fechaHasta.Length !=0)
                {
                    where += " AND p.p_fecha BETWEEN @fechaDesde AND @fechaHasta";
                    parameters.Add("fechaDesde", $"{fechaDesde}");
                    parameters.Add("fechaHasta", $"{fechaHasta}");
                }
                else if (!string.IsNullOrEmpty(fechaDesde))
                {
                    where += " AND p.p_fecha >= @fechaDesde";
                    parameters.Add("fechaDesde", $"{fechaDesde}");
                }
                else if (!string.IsNullOrEmpty(fechaHasta))
                {
                    where += " AND p.p_fecha <= @fechaHasta";
                    parameters.Add("fechaHasta", $"{fechaHasta}");
                }

                if (articulo.HasValue)
                {
                    where += " AND p.p_codigo IN (SELECT z.dp_pedido FROM detalle_pedido z WHERE dp_articulo = @articulo)";
                    parameters.Add("articulo", articulo);
                }

                if (!string.IsNullOrEmpty(clientes))
                {
                    where += " AND p.p_cliente IN @clientes";
                    parameters.Add("clientes", clientes);
                }

                if (!string.IsNullOrEmpty(vendedores))
                {
                    where += " AND p.p_vendedor IN @vendedores";
                    parameters.Add("vendedores", vendedores);
                }

                if (!string.IsNullOrEmpty(sucursales))
                {
                    where += " AND p.p_sucursal IN @sucursales";
                    parameters.Add("sucursales", sucursales);
                }

                if (moneda.HasValue)
                {
                    where += " AND p.p_moneda = @moneda";
                    parameters.Add("moneda", moneda);
                }

                if (!string.IsNullOrEmpty(factura))
                {
                    where += " AND p.p_codigo IN (SELECT x.ve_pedido FROM ventas x WHERE ve_factura = @factura)";
                    parameters.Add("factura", factura);
                }

                if (estado == "1") where += " AND p.p_estado = 1";
                if (estado == "2") where += " AND p.p_estado = 2";
                if (estado == "3") where += " AND (p.p_estado = 1 OR p.p_estado = 2)";
            }
            var query = @"
                            SELECT
                                p.p_codigo AS pedido_id,
                                cli.cli_razon as cliente,
                                m.mo_descripcion as moneda,
                                p.p_fecha as fecha,
                                IFNULL(vp.ve_factura, '') as factura,
                                a.a_descripcion as area,
                                IFNULL((SELECT
                                    area.a_descripcion AS descripcion
                                    FROM area_secuencia arse
                                    INNER JOIN area area ON arse.ac_secuencia_area = area.a_codigo
                                    WHERE arse.ac_area = p.p_area
                                ), '-') as siguiente_area,
                                IF(p.p_estado = 1, 'Pendiente', IF(p.p_estado = 2, 'Facturado', 'Todos')) as estado,
                                p.p_estado as estado_num,
                                IF(p.p_credito = 1, 'Crédito', 'Contado') as condicion,
                                op.op_nombre as operador,
                                ope.op_nombre as vendedor,
                                dep.dep_descripcion as deposito,
                                p.p_cantcuotas,
                                p.p_entrega,
                                p.p_autorizar_a_contado,
                                p.p_imprimir as imprimir,
                                p.p_imprimir_preparacion as imprimir_preparacion,
                                p.p_cliente as cliente_id,
                                p.p_cantidad_cajas as cantidad_cajas,
                                IFNULL(p.p_obs, '') as obs,
                                FORMAT(ROUND(SUM(dp.dp_cantidad  * (dp.dp_precio - dp.dp_descuento)), 0), 0) as total,
                                IF(p.p_acuerdo = 1, 'Tiene acuerdo comercial', '') as acuerdo
                            FROM pedidos p
                            LEFT JOIN clientes cli ON p.p_cliente = cli.cli_codigo
                            LEFT JOIN monedas m ON p.p_moneda = m.mo_codigo
                            LEFT JOIN operadores op ON p.p_operador = op.op_codigo
                            LEFT JOIN operadores ope ON p.p_vendedor = ope.op_codigo
                            LEFT JOIN area a ON p.p_area = a.a_codigo
                            LEFT JOIN depositos dep ON p.p_deposito = dep.dep_codigo
                            LEFT JOIN ventas vp ON p.p_codigo = vp.ve_pedido
                            LEFT JOIN detalle_pedido dp ON p.p_codigo = dp.dp_pedido
                            WHERE " + where + @"
                            GROUP BY p.p_codigo
                            ORDER BY p.p_cliente DESC";


            Console.WriteLine(query);
foreach (var name in parameters.ParameterNames)
{
    Console.WriteLine($"{name}: {parameters.Get<dynamic>(name)}");
}

            return await connection.QueryAsync<PedidoViewModel>(query, parameters);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
