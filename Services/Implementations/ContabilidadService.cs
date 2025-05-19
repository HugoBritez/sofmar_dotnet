using Api.Models.Dtos;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;
using Api.Services.Mappers;
using Microsoft.Identity.Client;


namespace Api.Services.Implementations
{
    public class ContabilidadService : IContabilidadService
    {
        private readonly IContabilidadRepository _contabilidadRepository;

        public ContabilidadService(IContabilidadRepository contabilidadRepository)
        {
            _contabilidadRepository = contabilidadRepository;
        }

        public async Task<uint> GuardarAsientoContable(GuardarAsientoContableDTO parametros)
        {
            if (!parametros.Automatico) return 0;

            var confiAsientoVenta = await _contabilidadRepository.GetConfiguracionAsiento(1); // para ventas facturadas
            var configAsientoVentaComun = await _contabilidadRepository.GetConfiguracionAsiento(8); // para ventas comunes

            var totalExentas = parametros.TotalExentas;
            var total5 = parametros.TotalCinco;
            var total10 = parametros.TotalDiez;
            var TotalDebe = parametros.TotalAPagar;

            if (parametros.Moneda == 2)
            {
                totalExentas *= parametros.Cotizacion;
                total5 *= parametros.Cotizacion;
                total10 *= parametros.Cotizacion;
                TotalDebe *= parametros.Cotizacion;
            }

            var asientoContable = new AsientoContableDTO
            {
                Sucursal = parametros.Sucursal,
                Moneda = parametros.Moneda,
                Operador = parametros.Operador,
                Documento = parametros.Factura ?? parametros.Referencia.ToString(),
                Numero = parametros.NumeroAsiento,
                Fecha = parametros.Fecha,
                FechaAsiento = parametros.Fecha,
                TotalDebe = TotalDebe,
                TotalHaber = TotalDebe,
                Cotizacion = parametros.Cotizacion,
                Referencia = parametros.Referencia,
                Origen = 1
            };

            var idAsiento = await _contabilidadRepository.InsertarAsientoContable(asientoContable);


            string conceptoAsiento = parametros.Factura != null
                ? $"Venta Factura {confiAsientoVenta.Concepto.Trim()} - {parametros.Factura}"
                : $"Venta Comprobante {idAsiento}";

            int planDeCuentas;

            if (parametros.TipoVenta == 1)
            {
                if (parametros.CajaDefinicion != null)
                {
                    planDeCuentas = await _contabilidadRepository.BuscarCodigoPlanCuentaCajaDef(parametros.CajaDefinicion ?? 0);
                }
                else
                {
                    planDeCuentas = (int)confiAsientoVenta.Contado;
                }
            }
            else
            {
                planDeCuentas = parametros.Moneda == 1
                    ? (int)configAsientoVentaComun.Credito
                    : (int)configAsientoVentaComun.CreditoD;
            }

            decimal debeCaja = totalExentas + total5 + total10;

            var detalleAsientoContable = new DetalleAsientoContableDTO
            {
                Asiento = idAsiento,
                Plan = (uint)planDeCuentas,
                Debe = _contabilidadRepository.QuitarComas(debeCaja),
                Haber = 0,
                Concepto = conceptoAsiento
            };

            await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoContable);

            if (total5 > 0)
            {
                uint planGravadaCinco = parametros.ImprimirLegal == 1
                    ? confiAsientoVenta.Gravada
                    : configAsientoVentaComun.Gravada;
                string montoGravadaCinco = (total5 - (total5 / 21)).ToString("0.00");

                var detalleAsientoGravadasCinco = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = planGravadaCinco,
                    Debe = 0,
                    Haber = _contabilidadRepository.QuitarComas(decimal.Parse(montoGravadaCinco)),
                    Concepto = $"{conceptoAsiento}"
                };

                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoGravadasCinco);


                uint planIvaCinco = parametros.ImprimirLegal == 1
                    ? confiAsientoVenta.Iva5
                    : configAsientoVentaComun.Iva5;
                string montoIvaCinco = (total5 / 21).ToString("0.00");

                var detalleAsientoIvaCinco = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = planIvaCinco,
                    Debe = 0,
                    Haber = _contabilidadRepository.QuitarComas(decimal.Parse(montoIvaCinco)),
                    Concepto = $"{conceptoAsiento}"
                };

                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoIvaCinco);
            }

            if (total10 > 0)
            {
                uint planGravadaDiez = parametros.ImprimirLegal == 1
                     ? confiAsientoVenta.Gravada10
                     : configAsientoVentaComun.Gravada10;
                string montoGravadaDiez = (total10 - (total10 / 11)).ToString("0.00");
                var detalleAsientoGravadasDiez = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = planGravadaDiez,
                    Debe = 0,
                    Haber = _contabilidadRepository.QuitarComas(decimal.Parse(montoGravadaDiez)),
                    Concepto = $"{conceptoAsiento}"
                };
                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoGravadasDiez);

                uint planIvaDiez = parametros.ImprimirLegal == 1
                    ? confiAsientoVenta.Iva10
                    : configAsientoVentaComun.Iva10;
                string montoIvaDiez = (total10 / 11).ToString("0.00");

                var detalleAsientoIvaDiez = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = planIvaDiez,
                    Debe = 0,
                    Haber = _contabilidadRepository.QuitarComas(decimal.Parse(montoIvaDiez)),
                    Concepto = $"{conceptoAsiento}"
                };

                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoIvaDiez);
            }

            if (totalExentas > 0)
            {
                uint planExentas = parametros.ImprimirLegal == 1
                    ? confiAsientoVenta.Exenta
                    : configAsientoVentaComun.Exenta;
                string montoExentas = totalExentas.ToString("0.00");

                var detalleAsientoExentas = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = planExentas,
                    Debe = 0,
                    Haber = _contabilidadRepository.QuitarComas(decimal.Parse(montoExentas)),
                    Concepto = $"{conceptoAsiento}"
                };

                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoExentas);
            }
            return idAsiento;
        }

        public async Task<uint> GuardarCostoAsientoContable(GuardarCostoAsientoContableDTO parametros)
        {
            if (!parametros.Automatico) return 0;

            var configAsientoCosto = await _contabilidadRepository.GetConfiguracionAsiento(6); // para ventas facturadas
            var configAsientoCostoComun = await _contabilidadRepository.GetConfiguracionAsiento(8); // para ventas comunes

            var numeroAsiento = await _contabilidadRepository.GenerarNroAsiento();

            var costoTotalExentas = parametros.CostoTotalExentas;
            var costoTotal5 = parametros.CostoTotalCinco;
            var costoTotal10 = parametros.CostoTotalDiez;
            var CostoTotalDebe = parametros.CostoTotalDiez + parametros.CostoTotalCinco + parametros.CostoTotalExentas;

            if (parametros.Moneda == 2)
            {
                costoTotalExentas *= parametros.Cotizacion;
                costoTotal5 *= parametros.Cotizacion;
                costoTotal10 *= parametros.Cotizacion;
                CostoTotalDebe *= parametros.Cotizacion;
            }

            var asientoContable = new AsientoContableDTO
            {
                Sucursal = parametros.Sucursal,
                Moneda = parametros.Moneda,
                Operador = parametros.Operador,
                Documento = parametros.Factura ?? parametros.Referencia.ToString(),
                Numero = numeroAsiento,
                Fecha = parametros.Fecha,
                FechaAsiento = parametros.Fecha,
                TotalDebe = CostoTotalDebe,
                TotalHaber = CostoTotalDebe,
                Cotizacion = parametros.Cotizacion,
                Referencia = parametros.Referencia,
                Origen = 16
            };

            var idAsiento = await _contabilidadRepository.InsertarAsientoContable(asientoContable);

            string conceptoAsiento = parametros.Factura != null
                ? $"Venta Factura {configAsientoCosto.Concepto.Trim()} - {parametros.Factura}"
                : $"Venta Comprobante {idAsiento}";

            var totalGravadas = costoTotal5 + costoTotal10;

            if (totalGravadas > 0)
            {
                uint planDeCuentas = parametros.ImprimirLegal == 1
                 ? configAsientoCosto.Gravada
                 : configAsientoCostoComun.Contado;

                var detalleAsientoGravadas = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = planDeCuentas,
                    Debe = totalGravadas,
                    Haber = 0,
                    Concepto = $"{conceptoAsiento}"
                };


                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoGravadas);
            }

            if (costoTotalExentas > 0)
            {
                var detalleAsientoExentas = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = configAsientoCosto.Exenta,
                    Debe = 0,
                    Haber = costoTotalExentas,
                    Concepto = $"{conceptoAsiento}"
                };

                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoExentas);
            }

            if (costoTotal5 > 0)
            {
                var detalleAsientoCinco = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = configAsientoCosto.Iva5,
                    Debe = 0,
                    Haber = costoTotal5,
                    Concepto = conceptoAsiento
                };

                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoCinco);
            }

            if (costoTotal10 > 0)
            {
                var detalleAsientoDiez = new DetalleAsientoContableDTO
                {
                    Asiento = idAsiento,
                    Plan = configAsientoCosto.Iva10,
                    Debe = 0,
                    Haber = costoTotal10,
                    Concepto = conceptoAsiento
                };

                await _contabilidadRepository.InsertarDetalleAsientoContable(detalleAsientoDiez);
            }


            return idAsiento;

        }
    }
}