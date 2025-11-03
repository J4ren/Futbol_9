using System.Linq;
using Futbol_9.Modelos;
using Futbol_9.Servicios;

namespace Futbol_9.Vistas;

public partial class PaginaAsignacionSorteo : ContentPage
{
    private readonly ServicioBaseDatos _db = new();
    private List<Campeonato> _campeonatos = new();
    private List<Grupo> _grupos = new();
    private List<Equipo> _equiposNoAsignados = new();
    private int _campId = 0;

    public PaginaAsignacionSorteo()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _db.InicializarAsync();
        await CargarCampeonatosAsync();
        await RecargarTodoAsync();
    }

    private async Task CargarCampeonatosAsync()
    {
        _campeonatos = await _db.ListarCampeonatosAsync();
        pkCampeonato.ItemsSource = _campeonatos.Select(c => $"{c.Nombre} {(c.Activo ? "(Activo)" : "")}").ToList();

        var activo = await _db.ObtenerCampeonatoActivoAsync();
        if (activo != null)
        {
            _campId = activo.CampeonatoId;
            var idx = _campeonatos.FindIndex(c => c.CampeonatoId == _campId);
            pkCampeonato.SelectedIndex = idx;
        }
        else
        {
            pkCampeonato.SelectedIndex = _campeonatos.Count > 0 ? 0 : -1;
            if (pkCampeonato.SelectedIndex >= 0)
                _campId = _campeonatos[pkCampeonato.SelectedIndex].CampeonatoId;
        }
    }

    private async Task RecargarTodoAsync()
    {
        if (_campId == 0) return;

        _grupos = await _db.ListarGruposPorCampeonatoAsync(_campId);
        pkGrupo.ItemsSource = _grupos.Select(g => $"Grupo {g.Nombre}").ToList();
        pkGrupo.SelectedIndex = _grupos.Count > 0 ? 0 : -1;

        _equiposNoAsignados = await _db.ListarEquiposNoAsignadosAsync(_campId);
        pkEquipo.ItemsSource = _equiposNoAsignados.Select(e => e.Nombre).ToList();
        pkEquipo.SelectedIndex = _equiposNoAsignados.Count > 0 ? 0 : -1;

        await RefrescarVistaGruposAsync();
    }

    private async Task RefrescarVistaGruposAsync()
    {
        var vista = new List<GrupoConEquipos>();
        foreach (var g in _grupos)
        {
            var miembros = await _db.ListarEquiposDeGrupoAsync(g.GrupoId);
            var equipos = new List<Equipo>();
            foreach (var m in miembros)
            {
                // buscar datos del equipo
                var todos = await _db.ListarEquiposAsync();
                var eq = todos.FirstOrDefault(x => x.EquipoId == m.EquipoId);
                if (eq != null) equipos.Add(eq);
            }
            vista.Add(new GrupoConEquipos { GrupoId = g.GrupoId, Nombre = $"Grupo {g.Nombre}", Equipos = equipos });
        }
        cvGrupos.ItemsSource = vista;
    }

    private async void pkCampeonato_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (pkCampeonato.SelectedIndex < 0 || pkCampeonato.SelectedIndex >= _campeonatos.Count) return;
        _campId = _campeonatos[pkCampeonato.SelectedIndex].CampeonatoId;
        await RecargarTodoAsync();
    }

    private async void Actualizar_Clicked(object sender, EventArgs e)
    {
        await RecargarTodoAsync();
    }

    private async void Agregar_Clicked(object sender, EventArgs e)
    {
        if (_campId == 0)
        {
            await DisplayAlert("Validación", "Seleccione un campeonato.", "OK");
            return;
        }
        if (pkGrupo.SelectedIndex < 0 || pkGrupo.SelectedIndex >= _grupos.Count)
        {
            await DisplayAlert("Validación", "Seleccione un grupo.", "OK");
            return;
        }
        if (pkEquipo.SelectedIndex < 0 || pkEquipo.SelectedIndex >= _equiposNoAsignados.Count)
        {
            await DisplayAlert("Validación", "Seleccione un equipo no asignado.", "OK");
            return;
        }

        var grupo = _grupos[pkGrupo.SelectedIndex];
        var equipo = _equiposNoAsignados[pkEquipo.SelectedIndex];

        //evitar doble asignación
        if (await _db.EquipoAsignadoEnCampeonatoAsync(_campId, equipo.EquipoId))
        {
            await DisplayAlert("Info", "Ese equipo ya está asignado a un grupo.", "OK");
            return;
        }

        await _db.AsignarEquipoAGrupoAsync(grupo.GrupoId, equipo.EquipoId);
        await RecargarTodoAsync();
    }

    private async void Quitar_Clicked(object sender, EventArgs e)
    {
        if (pkEquipo.SelectedIndex < 0)
        {
            await DisplayAlert("Info", "Seleccione un equipo en el selector o use el botón 'Quitar' junto a cada nombre en la lista.", "OK");
            return;
        }

        var equipo = _equiposNoAsignados.ElementAtOrDefault(pkEquipo.SelectedIndex);
        if (equipo is null)
        {
            await DisplayAlert("Info", "El equipo seleccionado ya aparece como no asignado.", "OK");
            return;
        }

        
        await DisplayAlert("Info", "Ese equipo no está asignado a ningún grupo.", "OK");
    }

    private async void QuitarItem_Clicked(object sender, EventArgs e)
    {
        // Botón "Quitar" dentro de la lista por grupo
        var btn = (Button)sender;
        var equipo = btn.BindingContext as Equipo;
        if (equipo is null) return;

        await _db.QuitarAsignacionDeEquipoAsync(equipo.EquipoId);
        await RecargarTodoAsync();
    }
}
