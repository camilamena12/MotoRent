import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { LogIn } from 'lucide-react';
import api from '../services/api';

function Login() {
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [cargando, setCargando] = useState(false);

  const iniciarSesion = async (e) => {
    e.preventDefault();
    setError('');
    setCargando(true);

    try {
      const respuesta = await api.post('/Auth/login', {
        email,
        password
      });

      const { token, roles } = respuesta.data;

      // Verificar qué panel corresponde al usuario
      const destino = roles.includes('Administrador')
        ? '/admin'
        : roles.includes('Cliente')
          ? '/cliente'
          : null;

      if (!token || !destino) {
        setError('No se pudo determinar el acceso del usuario.');
        return;
      }

      // Guardar el token y roles en sessionStorage
      sessionStorage.setItem('motorent_token', token);
      sessionStorage.setItem('motorent_roles', JSON.stringify(roles));

      navigate(destino, { replace: true });

    } catch (error) {
      console.error(error);
      setError('Correo o contraseña incorrectos.');
    } finally {
      setCargando(false);
    }
  };

  return (
    <div className="container py-5">
      <div
        className="card shadow-sm mx-auto"
        style={{ maxWidth: '420px' }}
      >
        <div className="card-body p-4">

          <div className="text-center mb-4">
            <LogIn size={36} className="mb-3" />

            <h2>Iniciar sesión</h2>

            <p className="text-muted">
              Ingresá a tu cuenta de MotoRent
            </p>
          </div>

          {error && (
            <div className="alert alert-danger">
              {error}
            </div>
          )}

          <form onSubmit={iniciarSesion}>

            <div className="mb-3">
              <label className="form-label">
                Correo electrónico
              </label>

              <input
                type="email"
                className="form-control"
                placeholder="Ingresá tu correo"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="mb-3">
              <label className="form-label">
                Contraseña
              </label>

              <input
                type="password"
                className="form-control"
                placeholder="Ingresá tu contraseña"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>

            <button
              type="submit"
              className="btn btn-primary w-100"
              disabled={cargando}
            >
              {cargando ? 'Ingresando...' : 'Iniciar sesión'}
            </button>

          </form>

          <div className="text-center mt-4">
            <Link to="/" className="text-decoration-none">
              Volver al inicio
            </Link>
          </div>

        </div>
      </div>
    </div>
  );
}

export default Login;