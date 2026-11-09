
import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { ArrowLeft, ShieldCheck } from 'lucide-react';
import api from '../services/api';

function SolicitudAlquiler() {
  const location = useLocation();
  const navigate = useNavigate();

  const moto = location.state?.moto;

  const [datos, setDatos] = useState({
    nombre: '',
    apellido: '',
    dni: '',
    telefono: '',
    email: '',
    fechaInicioSolicitada: '',
    opcionCompra: false,
  });

  const [dniValidado, setDniValidado] = useState(false);
  const [mensajeDni, setMensajeDni] = useState('');
  const [validandoDni, setValidandoDni] = useState(false);

  const manejarCambio = (e) => {
    const { name, value, type, checked } = e.target;

    setDatos({
      ...datos,
      [name]: type === 'checkbox' ? checked : value,
    });

    if (['dni', 'nombre', 'apellido'].includes(name)) {
  setDniValidado(false);
  setMensajeDni('');
}
  };

 // Validar DNI mediante nuestra API .NET y RENAPER
const validarDni = async () => {
  setValidandoDni(true);
  setDniValidado(false);
  setMensajeDni('');

  try {
    const respuesta = await api.post('/validacion-dni', {
      dni: datos.dni,
      nombre: datos.nombre.trim(),
      apellido: datos.apellido.trim()
    });

    if (respuesta.data.validado === true) {
      setDniValidado(true);
      setMensajeDni('DNI validado correctamente.');
    } else {
      setMensajeDni(
        respuesta.data.mensaje ||
        'No se pudieron validar los datos ingresados.'
      );
    }

  } catch (error) {
    console.error('Error al validar DNI:', error);

    setMensajeDni(
      error.response?.data?.mensaje ||
      'No se pudo conectar con RENAPER. Intentá más tarde.'
    );

  } finally {
    setValidandoDni(false);
  }
};

 const manejarEnvio = async (e) => {
  e.preventDefault();

  if (!dniValidado) {
    alert('Primero tenés que validar tu DNI.');
    return;
  }

  try {
    // Enviar todos los datos a nuestro backend
    await api.post('/Solicitudes', {
      nombre: datos.nombre.trim(),
      apellido: datos.apellido.trim(),
      dni: datos.dni,
      telefono: datos.telefono,
      email: datos.email,
      motoId: moto.id,
      fechaInicioSolicitada: datos.fechaInicioSolicitada,
      opcionCompra: datos.opcionCompra
    });

    alert(
      '¡Solicitud enviada correctamente! Quedó pendiente de aprobación.'
    );

    navigate('/');

  } catch (error) {
    console.error('Error al enviar la solicitud:', error);

    alert(
      error.response?.data?.mensaje ||
      'No se pudo enviar la solicitud. Intentá nuevamente.'
    );
  }
};
  if (!moto) {
    return (
      <div className="container py-5 solicitud-page">
        <h2>No se seleccionó ninguna moto.</h2>
        <button className="btn btn-primary" onClick={() => navigate('/')}>
          Volver al catálogo
        </button>
      </div>
    );
  }

  return (
    <div className="container py-5 solicitud-page">
      <button
        className="btn btn-link mb-3"
        onClick={() => navigate('/')}
      >
        <ArrowLeft size={18} /> Volver al catálogo
      </button>

      <h1>Solicitar alquiler</h1>
      <p>Completá tus datos para iniciar la solicitud.</p>

      <div className="card mb-4">
        <div className="card-body">
          <h4>{moto.marca} {moto.modelo}</h4>
          <p>{moto.cilindrada} cc</p>
          <h5>
            ${Number(moto.precioPorDia).toLocaleString('es-AR')} por día
          </h5>
        </div>
      </div>

      <form onSubmit={manejarEnvio} className="card card-body">
        <h4 className="mb-3">Datos personales</h4>

        <div className="row g-3">
          <div className="col-md-6">
            <label className="form-label">Nombre</label>
            <input
              className="form-control"
              name="nombre"
              value={datos.nombre}
              onChange={manejarCambio}
              required
            />
          </div>

          <div className="col-md-6">
            <label className="form-label">Apellido</label>
            <input
              className="form-control"
              name="apellido"
              value={datos.apellido}
              onChange={manejarCambio}
              required
            />
          </div>

          <div className="col-md-6">
            <label className="form-label">DNI</label>
            <input
              className="form-control"
              name="dni"
              value={datos.dni}
              onChange={manejarCambio}
              inputMode="numeric"
              maxLength={8}
              required
            />
          </div>

          <div className="col-md-6 d-flex align-items-end">
            <button
              type="button"
              className="btn btn-outline-primary w-100"
              onClick={validarDni}
              disabled={validandoDni || datos.dni.length !== 8}
            >
              <ShieldCheck size={17} className="me-2" />
              {validandoDni ? 'Validando...' : 'Validar DNI'}
            </button>
          </div>

          {mensajeDni && (
            <div className={dniValidado ? 'text-success' : 'text-danger'}>
              {mensajeDni}
            </div>
          )}

          <div className="col-md-6">
            <label className="form-label">Teléfono</label>
            <input
              className="form-control"
              name="telefono"
              value={datos.telefono}
              onChange={manejarCambio}
              required
            />
          </div>

          <div className="col-md-6">
            <label className="form-label">Correo electrónico</label>
            <input
              type="email"
              className="form-control"
              name="email"
              value={datos.email}
              onChange={manejarCambio}
              required
            />
          </div>

          <div className="col-md-6">
            <label className="form-label">Fecha de inicio solicitada</label>
            <input
              type="date"
              className="form-control"
              name="fechaInicioSolicitada"
              value={datos.fechaInicioSolicitada}
              onChange={manejarCambio}
              min={new Date().toLocaleDateString('en-CA')}
              required
            />
          </div>

          <div className="col-12">
            <div className="form-check">
              <input
                type="checkbox"
                className="form-check-input"
                id="opcionCompra"
                name="opcionCompra"
                checked={datos.opcionCompra}
                onChange={manejarCambio}
              />
              <label className="form-check-label" htmlFor="opcionCompra">
                Me interesa la opción de compra después de 6 meses.
              </label>
            </div>
          </div>
        </div>

        <button type="submit" className="btn btn-danger mt-4">
          Continuar con la solicitud
        </button>
      </form>

      <p className="text-muted mt-3 small">
      La validación de identidad utiliza un servicio académico con datos ficticios.
      </p>
    </div>
  );
}

export default SolicitudAlquiler;
