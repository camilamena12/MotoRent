
function BotonCerrarSesion() {

  const cerrarSesion = () => {
    // Eliminar los datos de la sesión actual
    sessionStorage.removeItem('motorent_token');
    sessionStorage.removeItem('motorent_roles');

    // Volver al login sin conservar la página anterior
    window.location.replace('/login');
  };

  return (
    <button
      type="button"
      className="btn btn-outline-primary"
      onClick={cerrarSesion}
    >
      Cerrar sesión
    </button>
  );
}

export default BotonCerrarSesion;
