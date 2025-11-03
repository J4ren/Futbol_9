using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futbol_9.Modelos
{
    public enum TipoPlanPago
    {
        PagoUnico = 0,       // 100
        AnticipoMasSaldo = 1,// 35 + 65
        TresCuotas = 2       // 35 + 35 + 30
    }
}
