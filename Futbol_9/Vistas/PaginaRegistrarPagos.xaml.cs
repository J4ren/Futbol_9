using System.Linq; // necesario para LINQ
using Futbol_9.Modelos;
using Futbol_9.Servicios;

namespace Futbol_9.Vistas;

public partial class PaginaRegistrarPagos : ContentPage
{
    private readonly ServicioBaseDatos _db = new();
    private List<Equipo> _equipos = new();
    private List<PlanPago> _planesEquipo = new();
    private PlanPago? _planSeleccionado;

    public PaginaRegistrarPagos()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarEquiposAsync();
    }

    private async Task CargarEquiposAsync()
    {
        _equipos = await _db.ListarEquiposAsync();
        pkEquipo.ItemsSource = _equipos.Select(e => e.Nombre).ToList();
        pkEquipo.SelectedIndex = _equipos.Count > 0 ? 0 : -1;

        if (_equipos.Count == 0)
        {
            await DisplayAlert("Info", "Primero registre un equipo en la pestaña Equipos.", "OK");
        }
        else
        {
            await CargarPlanesDelEquipoActualAsync();
        }
    }

    private async Task CargarPlanesDelEquipoActualAsync()
    {
        pkPlan.IsVisible = false;
        _planSeleccionado = null;
        cvCuotas.ItemsSource = null;
        ActualizarTotales(0m, 0m);

        var equipo = EquipoActual();
        if (equipo is null) return;

        _planesEquipo = await _db.ListarPlanesPorEquipoAsync(equipo.EquipoId);
        if (_planesEquipo.Count == 0)
        {
            await DisplayAlert("Info", "Este equipo no tiene planes. Cree uno en la pestaña Planes.", "OK");
            return;
        }

        if (_planesEquipo.Count == 1)
        {
            _planSeleccionado = _planesEquipo[0];
            pkPlan.IsVisible = false;
            await CargarCuotasYTotalesAsync();
        }
        else
        {
            pkPlan.ItemsSource = _planesEquipo.Select(p => $"{p.Tipo} | Límite {p.LimitePago:dd/MM}").ToList();
            pkPlan.SelectedIndex = 0;
            pkPlan.IsVisible = true;

            _planSeleccionado = _planesEquipo[0];
            await CargarCuotasYTotalesAsync();
        }
    }

    private Equipo? EquipoActual()
    {
        if (pkEquipo.SelectedIndex < 0 || pkEquipo.SelectedIndex >= _equipos.Count) return null;
        return _equipos[pkEquipo.SelectedIndex];
    }

    private PlanPago? PlanActual() => _planSeleccionado;

    private async Task CargarCuotasYTotalesAsync()
    {
        var plan = PlanActual();
        if (plan is null) return;

        var cuotas = await _db.ListarCuotasPorPlanAsync(plan.PlanPagoId);
        cvCuotas.ItemsSource = cuotas;

        var total = plan.MontoTotal;
        var pagado = cuotas.Where(c => c.Pagada).Sum(c => c.Importe);
        ActualizarTotales(total, pagado);

        // Banner informativo del plan que se usara
        var equipo = _equipos.FirstOrDefault(e => e.EquipoId == plan.EquipoId);
        frmInfoPlan.IsVisible = true;
        lblPlanEquipo.Text = $"Equipo: {equipo?.Nombre ?? plan.EquipoId.ToString()}";
        lblPlanLimite.Text = $"Límite de pago (desde campeonato): {plan.LimitePago:dd/MM/yyyy}";
        lblPlanTotal.Text = $"Total del plan: S/ {plan.MontoTotal:0.##}";
        
    }

    private void ActualizarTotales(decimal total, decimal pagado)
    {
        lblTotal.Text = $"Total: S/ {total:F2}";
        lblPagado.Text = $"  |  Pagado: S/ {pagado:F2}";
        lblSaldo.Text = $"  |  Saldo: S/ {(total - pagado):F2}";
    }

    private async void pkEquipo_SelectedIndexChanged(object sender, EventArgs e)
    {
        await CargarPlanesDelEquipoActualAsync();
    }

    private async void pkPlan_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_planesEquipo.Count == 0) return;
        if (pkPlan.SelectedIndex >= 0 && pkPlan.SelectedIndex < _planesEquipo.Count)
        {
            _planSeleccionado = _planesEquipo[pkPlan.SelectedIndex];
            await CargarCuotasYTotalesAsync();
        }
    }

    private async void Refrescar_Clicked(object sender, EventArgs e)
    {
        await CargarCuotasYTotalesAsync();
    }

    private async void Pagar_Clicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var cuota = (Cuota?)btn.BindingContext;
        if (cuota is null) return;

        if (cuota.Pagada)
        {
            await DisplayAlert("Info", "Esta cuota ya está pagada.", "OK");
            return;
        }

        var plan = PlanActual();
        if (plan is not null && DateTime.Today > plan.LimitePago)
        {
            bool continuar = await DisplayAlert("Advertencia",
                "La fecha límite del plan ya pasó. ¿Desea registrar el pago de todos modos?",
                "Sí", "No");
            if (!continuar) return;
        }

        bool ok = await DisplayAlert("Confirmar",
            $"¿Marcar como pagada la cuota {cuota.Numero} por S/ {cuota.Importe:F2}?",
            "Sí", "No");
        if (!ok) return;

        await _db.RegistrarPagoDeCuotaAsync(cuota.CuotaId);
        await CargarCuotasYTotalesAsync();
    }
}