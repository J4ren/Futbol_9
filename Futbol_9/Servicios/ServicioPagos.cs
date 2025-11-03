using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Futbol_9.Modelos;


namespace Futbol_9.Servicios
{
    public static class ServicioPagos
    {
        public static List<Cuota> GenerarCuotas(PlanPago plan, DateTime inicio, DateTime fin)
        {
            var cuotas = new List<Cuota>();
            DateTime hoy = DateTime.Today;

            switch (plan.Tipo)
            {
                case TipoPlanPago.PagoUnico:
                    cuotas.Add(new Cuota
                    {
                        PlanPagoId = plan.PlanPagoId,
                        Numero = 1,
                        Importe = plan.MontoTotal,
                        FechaVencimiento = Clampea(hoy, fin)
                    });
                    break;

                case TipoPlanPago.AnticipoMasSaldo:
                    cuotas.Add(new Cuota
                    {
                        PlanPagoId = plan.PlanPagoId,
                        Numero = 1,
                        Importe = 35m,
                        FechaVencimiento = Clampea(hoy, fin)
                    });
                    cuotas.Add(new Cuota
                    {
                        PlanPagoId = plan.PlanPagoId,
                        Numero = 2,
                        Importe = 65m,
                        FechaVencimiento = fin
                    });
                    break;

                case TipoPlanPago.TresCuotas:
                    var totalDias = (fin - hoy).TotalDays;
                    if (totalDias < 2) totalDias = 2; // evita fechas iguales
                    var c1 = Clampea(hoy, fin);
                    var c2 = Clampea(hoy.AddDays(totalDias / 2.0), fin);
                    var c3 = fin;

                    cuotas.Add(new Cuota { PlanPagoId = plan.PlanPagoId, Numero = 1, Importe = 35m, FechaVencimiento = c1 });
                    cuotas.Add(new Cuota { PlanPagoId = plan.PlanPagoId, Numero = 2, Importe = 35m, FechaVencimiento = c2 });
                    cuotas.Add(new Cuota { PlanPagoId = plan.PlanPagoId, Numero = 3, Importe = 30m, FechaVencimiento = c3 });
                    break;
            }

            return cuotas;
        }

        private static DateTime Clampea(DateTime fecha, DateTime fin) =>
            fecha > fin ? fin : fecha;
    }
}
