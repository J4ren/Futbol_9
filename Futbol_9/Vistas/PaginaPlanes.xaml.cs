using Futbol_9.Modelos;
using Futbol_9.Servicios;


namespace Futbol_9.Vistas;

public partial class PaginaPlanes : ContentPage
{
    private readonly ServicioBaseDatos _db = new();
    private List<Equipo> _equipos = new();
    private Campeonato? _campActivo;
    private PlanPago? _planCreado;

    public PaginaPlanes()
    {
        InitializeComponent();
        pkPlan.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();

        // Cargar campeonato activo
        _campActivo = await _db.ObtenerCampeonatoActivoAsync();
        if (_campActivo is null)
        {
            frmInfoCamp.IsVisible = true;
            lblCampNombre.Text = "No hay campeonato activo.";
            lblCampIns.Text = "Configure uno en la pestaña Campeonato.";
            lblCampMonto.Text = "";
            // Bloquearre creación de planes sin campeonato
            dpInicio.Date = DateTime.Today;
            dpFin.Date = DateTime.Today;
        }
        else
        {
            frmInfoCamp.IsVisible = true;
            lblCampNombre.Text = $"Campeonato: {_campActivo.Nombre}";
            lblCampIns.Text = $"Inscripciones: {_campActivo.InscripcionInicio:dd/MM/yyyy} a {_campActivo.InscripcionFin:dd/MM/yyyy}";
            lblCampMonto.Text = $"Monto inscripción: S/ {_campActivo.MontoInscripcion:0.##} | Cuotas permitidas: {_campActivo.PermiteCuotas}";

            // Fecjsas bloqueadas desde campeonato
            dpInicio.Date = _campActivo.InscripcionInicio;
            dpFin.Date = _campActivo.InscripcionFin;
        }

        // Cargar equipo
        await CargarEquiposAsync();
    }

    private async Task CargarEquiposAsync()
    {
        _equipos = await _db.ListarEquiposAsync();
        pkEquipo.ItemsSource = _equipos.Select(e => e.Nombre).ToList();

        if (_equipos.Count == 0)
            await DisplayAlert("Info", "Primero registre un equipo en la pestaña Equipos.", "OK");
        else
            pkEquipo.SelectedIndex = 0;
    }

    private async void CrearPlan_Clicked(object sender, EventArgs e)
    {
        // Validaciones
        if (_campActivo is null)
        {
            await DisplayAlert("Validación", "No hay campeonato activo. Configúrelo antes de crear planes.", "OK");
            return;
        }

        if (!_campActivo.PermiteCuotas && pkPlan.SelectedIndex != 0)
        {
            await DisplayAlert("Política", "Este campeonato no permite cuotas. Use Pago Único.", "OK");
            return;
        }

        if (_equipos.Count == 0 || pkEquipo.SelectedIndex < 0)
        {
            await DisplayAlert("Validación", "Seleccione un equipo.", "OK");
            return;
        }

        // Tipo de plan
        var tipo = pkPlan.SelectedIndex switch
        {
            0 => TipoPlanPago.PagoUnico,
            1 => TipoPlanPago.AnticipoMasSaldo,
            2 => TipoPlanPago.TresCuotas,
            _ => TipoPlanPago.PagoUnico
        };

        // Moto y fechas SIEMPRE del campeonato activo
        var monto = _campActivo.MontoInscripcion;
        var ini = _campActivo.InscripcionInicio;
        var fin = _campActivo.InscripcionFin;

        var equipo = _equipos[pkEquipo.SelectedIndex];

        var plan = new PlanPago
        {
            EquipoId = equipo.EquipoId,
            Tipo = tipo,
            MontoTotal = monto,
            FechaCreacion = DateTime.Now,
            LimitePago = fin // límite de pago / fin de inscripciones
        };

        await _db.InsertarPlanAsync(plan); // genera Id
        _planCreado = plan;

        // Generar cuotas dentro del rango establecido por el campeonato
        var cuotas = ServicioPagos.GenerarCuotas(plan, ini, fin);
        foreach (var c in cuotas) c.PlanPagoId = plan.PlanPagoId;
        await _db.InsertarCuotasAsync(cuotas);

        // Mostrar cuotas
        cvCuotas.ItemsSource = await _db.ListarCuotasPorPlanAsync(plan.PlanPagoId);

        await DisplayAlert("OK", "Plan creado con fechas y monto del campeonato activo.", "Continuar");
    }
}