import { Navigate } from 'react-router-dom';

function RutaProtegida({ children, rolPermitido }) {
  const token = sessionStorage.getItem('motorent_token');

  let roles = [];

  try {
    roles = JSON.parse(
      sessionStorage.getItem('motorent_roles') || '[]'
    );
  } catch {
    roles = [];
  }

  // Si no inició sesión, enviarlo al login
  if (!token) {
    return <Navigate to="/login" replace />;
  }

  // Si no tiene el rol correspondiente, volver al inicio
  if (!Array.isArray(roles) || !roles.includes(rolPermitido)) {
    return <Navigate to="/" replace />;
  }

  // Si tiene permiso, mostrar el panel
  return children;
}

export default RutaProtegida;