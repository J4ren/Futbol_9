using System.Linq;
using Futbol_9.Modelos;
using Futbol_9.Servicios;

namespace Futbol_9.Vistas;

public partial class PaginaEquipos : ContentPage
{
    private readonly ServicioBaseDatos _db = new();
    private List<Campeonato> _campeonatos = new();
    private List<Equipo> _equipos = new();

    public PaginaEquipos()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarCampeonatosAsync();
        await CargarEquiposAsync();
        LimpiarCampos();
    }

    private async Task CargarCampeonatosAsync()
    {
        _campeonatos = await _db.ListarCampeonatosAsync();
        pkCampeonato.ItemsSource = _campeonatos.Select(c => $"{c.Nombre} {(c.Activo ? "(Activo)" : "")}").ToList();

        var activo = await _db.ObtenerCampeonatoActivoAsync();
        if (activo != null)
        {
            var idx = _campeonatos.FindIndex(c => c.CampeonatoId == activo.CampeonatoId);
            pkCampeonato.SelectedIndex = idx;
        }
        else
        {
            pkCampeonato.SelectedIndex = _campeonatos.Count > 0 ? 0 : -1;
        }
    }

    private async Task CargarEquiposAsync()
    {
        _equipos = await _db.ListarEquiposAsync();
        cvEquipos.ItemsSource = _equipos;
    }

    private async void GuardarEquipo_Clicked(object sender, EventArgs e)
    {
        if (pkCampeonato.SelectedIndex < 0 || pkCampeonato.SelectedIndex >= _campeonatos.Count)
        {
            await DisplayAlert("Validación", "Seleccione un campeonato.", "OK");
            return;
        }

        var nombre = txtNombreEquipo.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nombre))
        {
            await DisplayAlert("Validación", "Ingrese el nombre del equipo.", "OK");
            return;
        }

        var existe = await _db.ObtenerEquipoPorNombreAsync(nombre);
        if (existe is not null)
        {
            await DisplayAlert("Duplicado", "Ya existe un equipo con ese nombre.", "OK");
            return;
        }

        var delNombre = txtDelegadoNombre.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(delNombre))
        {
            await DisplayAlert("Validación", "Ingrese el nombre del delegado.", "OK");
            return;
        }

        // Fechas desde campeonato (solo informativas en equipo)
        var camp = _campeonatos[pkCampeonato.SelectedIndex];

        var equipo = new Equipo
        {
            CampeonatoId = camp.CampeonatoId,
            Nombre = nombre,
            DelegadoNombre = delNombre,
            DelegadoDocumento = txtDelegadoDocumento.Text?.Trim(),
            DelegadoTelefono = txtDelegadoTelefono.Text?.Trim(),
            FechaInicioInscripcion = camp.InscripcionInicio,
            FechaFinInscripcion = camp.InscripcionFin
        };

        await _db.InsertarEquipoAsync(equipo);
        await DisplayAlert("OK", "Equipo registrado. Ahora cree el plan y complete el pago para habilitar jugadores.", "Continuar");

        LimpiarCampos();
        await CargarEquiposAsync();
    }

    private void Limpiar_Clicked(object sender, EventArgs e) => LimpiarCampos();

    private async void Actualizar_Clicked(object sender, EventArgs e)
    {
        await CargarCampeonatosAsync();
        await CargarEquiposAsync();
    }

    private void LimpiarCampos()
    {
        txtNombreEquipo.Text = string.Empty;
        txtDelegadoNombre.Text = string.Empty;
        txtDelegadoDocumento.Text = string.Empty;
        txtDelegadoTelefono.Text = string.Empty;
    }
}