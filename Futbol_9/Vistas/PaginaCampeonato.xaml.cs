using Futbol_9.Modelos;
using Futbol_9.Servicios;

namespace Futbol_9.Vistas;

public partial class PaginaCampeonato : ContentPage
{
    private readonly ServicioBaseDatos _db = new();
    private List<Campeonato> _lista = new();
    private Campeonato? _actual; // último guardado o activo cargado

    public PaginaCampeonato()
    {
        InitializeComponent();

        // Valores  defecto
        dpCampIni.Date = DateTime.Today;
        dpCampFin.Date = DateTime.Today.AddMonths(2);

        dpInsIni.Date = DateTime.Today;
        dpInsFin.Date = DateTime.Today.AddMonths(1);

        txtNombre.Text = "Campeonato Local";
        txtMonto.Text = "100";
        chkCuotas.IsChecked = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarAsync();
    }

    private async Task CargarAsync()
    {
        _lista = await _db.ListarCampeonatosAsync();
        _actual = await _db.ObtenerCampeonatoActivoAsync();

        cvListado.ItemsSource = _lista;

        if (_actual is not null)
        {
            // Cargar formulario con el activo
            txtNombre.Text = _actual.Nombre;
            dpCampIni.Date = _actual.FechaInicioCampeonato;
            dpCampFin.Date = _actual.FechaFinCampeonato;
            dpInsIni.Date = _actual.InscripcionInicio;
            dpInsFin.Date = _actual.InscripcionFin;
            txtMonto.Text = _actual.MontoInscripcion.ToString("0.##");
            chkCuotas.IsChecked = _actual.PermiteCuotas;
        }
    }

    private bool Validar(out string mensaje)
    {
        mensaje = "";

        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            mensaje = "Ingrese el nombre del campeonato.";
            return false;
        }

        if (dpCampFin.Date <= dpCampIni.Date)
        {
            mensaje = "La fecha fin del campeonato debe ser posterior a su inicio.";
            return false;
        }

        if (dpInsFin.Date <= dpInsIni.Date)
        {
            mensaje = "La fecha fin de inscripciones debe ser posterior a su inicio.";
            return false;
        }

        if (dpInsIni.Date < dpCampIni.Date || dpInsFin.Date > dpCampFin.Date)
        {
            mensaje = "El rango de inscripciones debe estar contenido dentro del rango del campeonato.";
            return false;
        }

        if (!decimal.TryParse(txtMonto.Text?.Trim(), out var monto) || monto <= 0)
        {
            mensaje = "Ingrese un monto de inscripción válido (> 0).";
            return false;
        }

        return true;
    }

    private async void Guardar_Clicked(object sender, EventArgs e)
    {
        if (!Validar(out var msg))
        {
            await DisplayAlert("Validación", msg, "OK");
            return;
        }

        var entidad = _actual ?? new Campeonato();
        entidad.Nombre = txtNombre.Text!.Trim();
        entidad.FechaInicioCampeonato = dpCampIni.Date;
        entidad.FechaFinCampeonato = dpCampFin.Date;
        entidad.InscripcionInicio = dpInsIni.Date;
        entidad.InscripcionFin = dpInsFin.Date;
        entidad.MontoInscripcion = decimal.Parse(txtMonto.Text!.Trim());
        entidad.PermiteCuotas = chkCuotas.IsChecked;

        if (entidad.CampeonatoId == 0)
            await _db.InsertarCampeonatoAsync(entidad);
        else
            await _db.ActualizarCampeonatoAsync(entidad);

        _actual = entidad;
        await CargarAsync();
        await DisplayAlert("OK", "Campeonato guardado.", "Continuar");
    }

    private async void Activar_Clicked(object sender, EventArgs e)
    {
        if (_actual is null)
        {
            await DisplayAlert("Info", "Primero guarde un campeonato.", "OK");
            return;
        }
        await _db.EstablecerComoActivoAsync(_actual.CampeonatoId);
        await CargarAsync();
        await DisplayAlert("OK", "Campeonato establecido como activo.", "Continuar");
    }

    private async void Recargar_Clicked(object sender, EventArgs e)
    {
        await CargarAsync();
    }
}