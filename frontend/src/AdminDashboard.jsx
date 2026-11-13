
import { useState, useEffect } from 'react';
import api from './services/api';
import Solicitudes from './pages/Solicitudes';
import Pagos from './pages/Pagos';
import BotonCerrarSesion from './components/BotonCerrarSesion';



import {
  LayoutDashboard,
  Bike,
  Users,
  ClipboardList,
  CreditCard,
  Menu,
  Bell,
  CircleDollarSign,
  Gauge,
  ArrowUpRight,
} from 'lucide-react';

import './App.css';

const secciones = [
  { nombre: 'Inicio', icono: LayoutDashboard },
  { nombre: 'Motos', icono: Bike },
  { nombre: 'Clientes', icono: Users },
  { nombre: 'Alquileres', icono: ClipboardList },
  { nombre: 'Solicitudes', icono: ClipboardList },
  { nombre: 'Pagos', icono: CreditCard },
];

function App() {
  const [seccion, setSeccion] = useState('Inicio');
  const [menuAbierto, setMenuAbierto] = useState(true);

const [resumen, setResumen] = useState({
  motos: null,
  clientes: null,
  alquileresActivos: null,
  pagos: null,
});
  // Estados de las motos
  const [motos, setMotos] = useState([]);
  const [cargandoMotos, setCargandoMotos] = useState(false);
  const [errorMotos, setErrorMotos] = useState('');

// Estados para agregar una moto
const [mostrarFormularioMoto, setMostrarFormularioMoto] = useState(false);
const [guardandoMoto, setGuardandoMoto] = useState(false);
const [errorFormularioMoto, setErrorFormularioMoto] = useState('');

const [nuevaMoto, setNuevaMoto] = useState({
  marca: '',
  modelo: '',
  cilindrada: '',
  precioPorDia: '',
  disponible: true,
});

// Estados para editar una moto
const [modoEdicionMoto, setModoEdicionMoto] = useState(false);
const [motoEditandoId, setMotoEditandoId] = useState(null);


  // Estados de los clientes
const [clientes, setClientes] = useState([]);
const [cargandoClientes, setCargandoClientes] = useState(false);
const [errorClientes, setErrorClientes] = useState('');

const [estadosCuentas, setEstadosCuentas] = useState({});
const [cargandoEstadosCuentas, setCargandoEstadosCuentas] = useState(true);

// Estados para registrar y editar clientes
const [mostrarFormularioCliente, setMostrarFormularioCliente] = useState(false);
const [guardandoCliente, setGuardandoCliente] = useState(false);
const [errorFormularioCliente, setErrorFormularioCliente] = useState('');

const [nuevoCliente, setNuevoCliente] = useState({
  nombre: '',
  apellido: '',
  dni: '',
  telefono: '',
  email: '',
});

const [modoEdicionCliente, setModoEdicionCliente] = useState(false);
const [clienteEditandoId, setClienteEditandoId] = useState(null);

//cuentas clientes
const [clienteAccesoId, setClienteAccesoId] = useState(null);
const [claveTemporal, setClaveTemporal] = useState('');
const [guardandoAcceso, setGuardandoAcceso] = useState(false);
const [errorAcceso, setErrorAcceso] = useState('');

// Estados de los alquileres
const [alquileres, setAlquileres] = useState([]);
const [cargandoAlquileres, setCargandoAlquileres] = useState(false);
const [errorAlquileres, setErrorAlquileres] = useState('');

// Estados de las solicitudes
const [solicitudes, setSolicitudes] = useState([]);
const [cargandoSolicitudes, setCargandoSolicitudes] = useState(false);
const [errorSolicitudes, setErrorSolicitudes] = useState('');

// Estados para registrar alquileres
const [mostrarFormularioAlquiler, setMostrarFormularioAlquiler] = useState(false);
const [guardandoAlquiler, setGuardandoAlquiler] = useState(false);
const [errorFormularioAlquiler, setErrorFormularioAlquiler] = useState('');


const [nuevoAlquiler, setNuevoAlquiler] = useState({
  clienteId: '',
  motoId: '',
  fechaInicio: '',
  opcionCompra: false,
});

//estadps de pago 
const [pagos, setPagos] = useState([]);
const [cargandoPagos, setCargandoPagos] = useState(false);
const [errorPagos, setErrorPagos] = useState('');

// Actualizar un campo del formulario
const manejarCambioMoto = (e) => {
  const { name, value, type, checked } = e.target;

  setNuevaMoto({
    ...nuevaMoto,
    [name]: type === 'checkbox' ? checked : value,
  });
};

// Actualizar los campos del formulario de clientes
const manejarCambioCliente = (e) => {
  const { name, value } = e.target;

  setNuevoCliente({
    ...nuevoCliente,
    [name]: value,
  });
};

// Actualizar los campos del formulario de alquileres
const manejarCambioAlquiler = (e) => {
  const { name, value, type, checked } = e.target;

  setNuevoAlquiler({
    ...nuevoAlquiler,
    [name]: type === 'checkbox' ? checked : value,
  });
};
// Cargar los datos del cliente seleccionado en el formulario
const editarCliente = (cliente) => {
  setNuevoCliente({
    nombre: cliente.nombre || '',
    apellido: cliente.apellido || '',
    dni: cliente.dni || '',
    telefono: cliente.telefono || '',
    email: cliente.email || '',
  });

  setClienteEditandoId(cliente.id);
  setModoEdicionCliente(true);
  setMostrarFormularioCliente(true);
};
// Cancelar el registro o edición de un cliente
const cancelarFormularioCliente = () => {
  setNuevoCliente({
    nombre: '',
    apellido: '',
    dni: '',
    telefono: '',
    email: '',
  });

  setModoEdicionCliente(false);
  setClienteEditandoId(null);
  setMostrarFormularioCliente(false);
  setErrorFormularioCliente('');
};

// Cargar los datos de la moto seleccionada en el formulario
const editarMoto = (moto) => {
  setNuevaMoto({
    marca: moto.marca || '',
    modelo: moto.modelo || '',
    cilindrada: moto.cilindrada || '',
    precioPorDia: moto.precioPorDia || '',
    disponible: moto.disponible,
  });

  setMotoEditandoId(moto.id);
  setModoEdicionMoto(true);
  setMostrarFormularioMoto(true);
};

// Cancelar el registro o edición de una moto
const cancelarFormularioMoto = () => {
  setNuevaMoto({
    marca: '',
    modelo: '',
    cilindrada: '',
    precioPorDia: '',
    disponible: true,
  });

  setModoEdicionMoto(false);
  setMotoEditandoId(null);
  setMostrarFormularioMoto(false);
  setErrorFormularioMoto('');
};

// Guardar una nueva moto o actualizar una existente
const guardarMoto = async (e) => {
  e.preventDefault();

  setGuardandoMoto(true);
  setErrorFormularioMoto('');

  try {
    const motoParaGuardar = {
      marca: nuevaMoto.marca.trim(),
      modelo: nuevaMoto.modelo.trim(),
      cilindrada: Number(nuevaMoto.cilindrada),
      precioPorDia: Number(nuevaMoto.precioPorDia),
      disponible: nuevaMoto.disponible,
    };

    if (modoEdicionMoto) {
      await api.put(`/Motos/${motoEditandoId}`, motoParaGuardar);
    } else {
      await api.post('/Motos', motoParaGuardar);
    }

    const respuesta = await api.get('/Motos');
    setMotos(respuesta.data);

    setNuevaMoto({
      marca: '',
      modelo: '',
      cilindrada: '',
      precioPorDia: '',
      disponible: true,
    });

    setModoEdicionMoto(false);
    setMotoEditandoId(null);
    setMostrarFormularioMoto(false);

  } catch (error) {
    console.error('Error al guardar la moto:', error);
    setErrorFormularioMoto(
      'No se pudo guardar la moto. Verificá los datos e intentá nuevamente.'
    );
  } finally {
    setGuardandoMoto(false);
  }
};

// Guardar un cliente nuevo o actualizar uno existente
const guardarCliente = async (e) => {
  e.preventDefault();

  setGuardandoCliente(true);
  setErrorFormularioCliente('');

  try {
    const clienteParaGuardar = {
      nombre: nuevoCliente.nombre.trim(),
      apellido: nuevoCliente.apellido.trim(),
      dni: nuevoCliente.dni.trim(),
      telefono: nuevoCliente.telefono.trim(),
      email: nuevoCliente.email.trim(),
    };

    if (modoEdicionCliente) {
      await api.put(`/Clientes/${clienteEditandoId}`, clienteParaGuardar);
    } else {
      await api.post('/Clientes', clienteParaGuardar);
    }

    const respuesta = await api.get('/Clientes');
    setClientes(respuesta.data);

    setNuevoCliente({
      nombre: '',
      apellido: '',
      dni: '',
      telefono: '',
      email: '',
    });

    setModoEdicionCliente(false);
    setClienteEditandoId(null);
    setMostrarFormularioCliente(false);

  } catch (error) {
    console.error('Error al guardar el cliente:', error);
    setErrorFormularioCliente(
      'No se pudo guardar el cliente. Verificá los datos e intentá nuevamente.'
    );
  } finally {
    setGuardandoCliente(false);
  }
};

// Registrar un nuevo alquiler
const guardarAlquiler = async (e) => {
  e.preventDefault();

  setGuardandoAlquiler(true);
  setErrorFormularioAlquiler('');

  try {
    const alquilerParaGuardar = {
      clienteId: Number(nuevoAlquiler.clienteId),
      motoId: Number(nuevoAlquiler.motoId),
      fechaInicio: nuevoAlquiler.fechaInicio,
      opcionCompra: nuevoAlquiler.opcionCompra,
    };

    await api.post('/Alquileres', alquilerParaGuardar);

    // Actualizar la lista de alquileres
    const respuestaAlquileres = await api.get('/Alquileres');
    setAlquileres(respuestaAlquileres.data);

    // Actualizar las motos porque la seleccionada ya no estará disponible
    const respuestaMotos = await api.get('/Motos');
    setMotos(respuestaMotos.data);

    setNuevoAlquiler({
      clienteId: '',
      motoId: '',
      fechaInicio: '',
      opcionCompra: false,
    });

    setMostrarFormularioAlquiler(false);

  } catch (error) {
    console.error('Error al registrar el alquiler:', error);

    setErrorFormularioAlquiler(
      error.response?.data || 'No se pudo registrar el alquiler. Verificá los datos e intentá nuevamente.'
    );
  } finally {
    setGuardandoAlquiler(false);
  }
};

const devolverAlquiler = async (id) => {
  const confirmar = window.confirm(
    '¿Estás seguro de que querés registrar la devolución de esta moto?'
  );

  if (!confirmar) return;

  try {
    await api.put(`/Alquileres/${id}/devolver`);

    // Actualizamos la lista de alquileres
    const respuestaAlquileres = await api.get('/Alquileres');
    setAlquileres(respuestaAlquileres.data);

    // Actualizamos la disponibilidad de las motos
    const respuestaMotos = await api.get('/Motos');
    setMotos(respuestaMotos.data);

  } catch (error) {
    console.error('Error al devolver la moto:', error);

    alert(
      error.response?.data ||
      'No se pudo registrar la devolución.'
    );
  }
};

// Aprobar una solicitud
const aprobarSolicitud = async (id) => {
  try {
    await api.put(`/Solicitudes/${id}/aprobar`);

    const respuesta = await api.get('/Solicitudes');
    setSolicitudes(respuesta.data);

    alert('Solicitud aprobada correctamente.');
  } catch (error) {
  console.error('Error al aprobar solicitud:', error);

  const mensaje = error.response?.data;

  alert(
    typeof mensaje === 'string'
      ? mensaje
      : mensaje?.mensaje || 'No se pudo aprobar la solicitud.'
  );
}
};

// Rechazar una solicitud
const rechazarSolicitud = async (id) => {
  try {
    await api.put(`/Solicitudes/${id}/rechazar`);

    const respuesta = await api.get('/Solicitudes');
    setSolicitudes(respuesta.data);

    alert('Solicitud rechazada correctamente.');
  } catch (error) {
    console.error('Error al rechazar solicitud:', error);
    alert(error.response?.data?.mensaje || 'No se pudo rechazar la solicitud.');
  }
};



  // Obtener motos desde la API
  useEffect(() => {
    if (seccion !== 'Motos' && seccion !== 'Alquileres') return;

    const obtenerMotos = async () => {
      setCargandoMotos(true);
      setErrorMotos('');

      try {
        const respuesta = await api.get('/Motos');
        setMotos(respuesta.data);
      } catch (error) {
        console.error('Error al obtener motos:', error);
        setErrorMotos(
          'No se pudieron cargar las motos. Verificá que la API esté funcionando.'
        );
      } finally {
        setCargandoMotos(false);
      }
    };

    obtenerMotos();
  }, [seccion]);

  // Obtener clientes desde la API
  useEffect(() => {
   if (seccion !== 'Clientes' && seccion !== 'Alquileres') return;

    const obtenerClientes = async () => {
      setCargandoClientes(true);
      setErrorClientes('');

      try {
        const respuesta = await api.get('/Clientes');
        setClientes(respuesta.data);
      } catch (error) {
        console.error('Error al obtener clientes:', error);
        setErrorClientes('No se pudieron cargar los clientes.');
      } finally {
        setCargandoClientes(false);
      }
    };

    obtenerClientes();
  }, [seccion]);
//obtener alquileres de la API

useEffect(() => {
  if (seccion !== 'Alquileres') return;

  const obtenerAlquileres = async () => {
    setCargandoAlquileres(true);
    setErrorAlquileres('');

    try {
      const respuesta = await api.get('/Alquileres');
      setAlquileres(respuesta.data);
    } catch (error) {
      console.error('Error al obtener alquileres:', error);
      setErrorAlquileres('No se pudieron cargar los alquileres.');
    } finally {
      setCargandoAlquileres(false);
    }
  };

  obtenerAlquileres();
}, [seccion]);
// Cargar solicitudes desde la API
useEffect(() => {
  if (seccion !== 'Solicitudes') return;

  const cargarSolicitudes = async () => {
    try {
      setCargandoSolicitudes(true);
      setErrorSolicitudes('');

      const respuesta = await api.get('/Solicitudes');

      setSolicitudes(respuesta.data);
    } catch (error) {
      console.error('Error al cargar solicitudes:', error);
      setErrorSolicitudes('No se pudieron cargar las solicitudes.');
    } finally {
      setCargandoSolicitudes(false);
    }
  };

  cargarSolicitudes();
}, [seccion]);

// Cargar pagos cuando entramos a la sección Pagos
useEffect(() => {
  if (seccion !== 'Pagos') return;

  const cargarPagos = async () => {
    try {
      setCargandoPagos(true);
      setErrorPagos('');

      const respuesta = await api.get('/Pagos');

      setPagos(respuesta.data);
    } catch (error) {
      console.error('Error al cargar pagos:', error);
      setErrorPagos('No se pudieron cargar los pagos.');
    } finally {
      setCargandoPagos(false);
    }
  };

  cargarPagos();
}, [seccion]);

useEffect(() => {
  if (seccion !== 'Clientes') return;

  const cargarEstadosCuentas = async () => {
    try {
      setCargandoEstadosCuentas(true);

      const respuesta = await api.get('/cuentas-clientes/estados');

      const estados = {};

      respuesta.data.forEach((item) => {
        estados[item.clienteId] = item;
      });

      setEstadosCuentas(estados);
    } catch (error) {
      console.error('Error al consultar cuentas:', error);
      setEstadosCuentas({});
    } finally {
      setCargandoEstadosCuentas(false);
    }
  };

  cargarEstadosCuentas();
}, [seccion]);

//asigna el rol cliene automaticamente 

const crearAccesoCliente = async (e, clienteId) => {
  e.preventDefault();

  try {
    setGuardandoAcceso(true);
    setErrorAcceso('');

    const respuesta = await api.post('/cuentas-clientes', {
      clienteId,
      password: claveTemporal
    });

    // Actualizar el estado de la cuenta en la tabla
    setEstadosCuentas((anteriores) => ({
      ...anteriores,
      [clienteId]: {
        ...anteriores[clienteId],
        tieneCuenta: true
      }
    }));

    setClienteAccesoId(null);
    setClaveTemporal('');

    alert(respuesta.data.mensaje);

  } catch (error) {
    console.error('Error al crear cuenta:', error);

    const datos = error.response?.data;

    setErrorAcceso(
      typeof datos === 'string'
        ? datos
        : datos?.errores?.join(', ') ||
          datos?.mensaje ||
          'No se pudo crear la cuenta.'
    );
  } finally {
    setGuardandoAcceso(false);
  }
};

useEffect(() => {
  if (seccion !== 'Inicio') return;

  const cargarResumen = async () => {
    try {
      const [
        respuestaMotos,
        respuestaClientes,
        respuestaAlquileres,
        respuestaPagos
      ] = await Promise.all([
        api.get('/Motos'),
        api.get('/Clientes'),
        api.get('/Alquileres'),
        api.get('/Pagos')
      ]);

      const alquileresActivos = respuestaAlquileres.data.filter(
        (alquiler) =>
          alquiler.estado?.toLowerCase() === 'activo'
      );

      setResumen({
        motos: respuestaMotos.data.length,
        clientes: respuestaClientes.data.length,
        alquileresActivos: alquileresActivos.length,
        pagos: respuestaPagos.data.length,
      });

    } catch (error) {
      console.error('Error al cargar el resumen:', error);
    }
  };

  cargarResumen();
}, [seccion]);

  return (
    <div className="app-layout">
      <aside
        className={`sidebar ${menuAbierto ? '' : 'sidebar-closed'}`}
      >
        <div className="brand">
          <img
            src="/logo-motorent.png"
            alt="MotoRent - Alquiler de motos"
            className="brand-logo"
          />
        </div>

        <p className="menu-label">MENÚ PRINCIPAL</p>

       <nav className="nav flex-column">
  {secciones.map(({ nombre, icono: Icono }) => (
    <button
      key={nombre}
      className={`nav-link ${
        seccion === nombre ? 'active' : ''
      }`}
      onClick={() => setSeccion(nombre)}
    >
      <Icono size={19} />
      <span>{nombre}</span>
    </button>
  ))}

  
</nav>

        <div className="sidebar-footer">
          <div className="status-dot" />
         MotoRent - Sistema de alquileres
          
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <button
            className="icon-button"
            onClick={() => setMenuAbierto(!menuAbierto)}
            aria-label="Mostrar u ocultar menú"
          >
            <Menu size={22} />
          </button>

          <div className="breadcrumb-text">
            Panel de control <span>/</span> {seccion}
          </div>

          <div className="topbar-right">
  <span className="admin-label">Administrador</span>
  <div className="avatar">A</div>

  <BotonCerrarSesion />

  <button
    className="icon-button"
    aria-label="Notificaciones"
  >
    <Bell size={20} />
  </button>
</div>
        </header>

        <section className="page-content">
          <div className="welcome-row">
            <div>
              <p className="eyebrow">MOTORENT / ADMINISTRACIÓN</p>

              <h1>
                {seccion === 'Inicio' ? 'Panel de control' : seccion}
              </h1>

            </div>

            <div className="date-chip">
              <Gauge size={17} />
              Sistema de alquileres
            </div>
          </div>

          {seccion === 'Inicio' ? (
            <>
              <div className="section-heading">
                <div>
                  <h2>Resumen general</h2>
                  <p>Vista general de tu negocio</p>
                </div>
              </div>

              <div className="stats-grid">
                <article className="stat-card">
                  <div className="stat-top">
                    <span>Total de motos</span>
                    <div className="stat-icon blue">
                      <Bike size={21} />
                    </div>
                  </div>
                  <strong>a{resumen.motos ?? '—'}</strong>
                  
                </article>

                <article className="stat-card">
                  <div className="stat-top">
                    <span>Clientes registrados</span>
                    <div className="stat-icon purple">
                      <Users size={21} />
                    </div>
                  </div>
                  <strong>{resumen.clientes ?? '—'}</strong>
                 
                </article>

                <article className="stat-card">
                  <div className="stat-top">
                    <span>Alquileres activos</span>
                    <div className="stat-icon orange">
                      <ClipboardList size={21} />
                    </div>
                  </div>
                  <strong>{resumen.alquileresActivos ?? '—'}</strong>
                 
                </article>

                <article className="stat-card">
                  <div className="stat-top">
                    <span>Pagos registrados</span>
                    <div className="stat-icon green">
                      <CircleDollarSign size={21} />
                    </div>
                  </div>
                  <strong>{resumen.pagos ?? '—'}</strong>
                  
                </article>
              </div>

              <div className="content-grid">
                <article className="welcome-card">
                  <div className="welcome-card-icon">
                    <Bike size={28} />
                  </div>

                  <p className="eyebrow">BIENVENIDO A MOTORENT</p>
                  <h2>Tu negocio, más organizado</h2>

                  <p>
                    Administrá la disponibilidad de las motos, los datos
                    de tus clientes, los alquileres y los pagos diarios.
                  </p>

                  <button
                    className="primary-button"
                    onClick={() => setSeccion('Alquileres')}
                  >
                    Ir a alquileres <ArrowUpRight size={17} />
                  </button>
                </article>

                <article className="quick-card">
                  <h3>Accesos rápidos</h3>
                  <p>Elegí qué querés gestionar.</p>

                  {secciones.slice(1).map(({ nombre, icono: Icono }) => (
                    <button
                      className="quick-link"
                      key={nombre}
                      onClick={() => setSeccion(nombre)}
                    >
                      <span className="quick-icon">
                        <Icono size={19} />
                      </span>
                      <span>{nombre}</span>
                      <ArrowUpRight size={16} />
                    </button>
                  ))}
                </article>
              </div>
            </>
          ) : seccion === 'Motos' ? (
           
<article className="placeholder-card motos-ancho">
  <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
    <h2 className="mb-0">Motos registradas</h2>

    <button
  className="primary-button"
  onClick={() => {
    if (mostrarFormularioMoto) {
      cancelarFormularioMoto();
    } else {
      cancelarFormularioMoto();
      setMostrarFormularioMoto(true);
    }
  }}
>
  <Bike size={17} />
  {mostrarFormularioMoto ? 'Cancelar' : 'Nueva moto'}
</button>
  </div>

  {mostrarFormularioMoto && (
    <form onSubmit={guardarMoto} className="card card-body mb-4">
      <h4 className="mb-3">
  {modoEdicionMoto ? 'Editar moto' : 'Registrar una moto'}
</h4>

      <div className="row g-3">
        <div className="col-md-6">
          <label className="form-label">Marca</label>
          <input
            type="text"
            name="marca"
            className="form-control"
            value={nuevaMoto.marca}
            onChange={manejarCambioMoto}
            placeholder="Ej: Honda"
            required
          />
        </div>

        <div className="col-md-6">
          <label className="form-label">Modelo</label>
          <input
            type="text"
            name="modelo"
            className="form-control"
            value={nuevaMoto.modelo}
            onChange={manejarCambioMoto}
            placeholder="Ej: Wave"
            required
          />
        </div>

        <div className="col-md-6">
          <label className="form-label">Cilindrada (cc)</label>
          <input
            type="number"
            name="cilindrada"
            className="form-control"
            value={nuevaMoto.cilindrada}
            onChange={manejarCambioMoto}
            min="1"
            required
          />
        </div>

        <div className="col-md-6">
          <label className="form-label">Precio por día ($)</label>
          <input
            type="number"
            name="precioPorDia"
            className="form-control"
            value={nuevaMoto.precioPorDia}
            onChange={manejarCambioMoto}
            min="1"
            required
          />
        </div>

        <div className="col-12">
          <div className="form-check">
            <input
              type="checkbox"
              name="disponible"
              className="form-check-input"
              checked={nuevaMoto.disponible}
              onChange={manejarCambioMoto}
              id="motoDisponible"
            />
            <label className="form-check-label" htmlFor="motoDisponible">
              Moto disponible para alquilar
            </label>
          </div>
        </div>
      </div>

      {errorFormularioMoto && (
        <p className="text-danger mt-3">{errorFormularioMoto}</p>
      )}

      <div className="mt-4">
        <button
          type="submit"
          className="primary-button"
          disabled={guardandoMoto}
        >
         {guardandoMoto? 'Guardando...': modoEdicionMoto ? 'Guardar cambios' : 'Guardar moto'}
        </button>
      </div>
    </form>
  )}

  {cargandoMotos && <p>Cargando motos...</p>}

  {errorMotos && (
    <p className="text-danger">{errorMotos}</p>
  )}

  {!cargandoMotos && !errorMotos && (
    <div className="table-responsive">
      <table className="table table-hover align-middle">
        <thead>
          <tr>
            <th>Marca</th>
            <th>Modelo</th>
            <th>Cilindrada</th>
            <th>Precio por día</th>
            <th>Disponibilidad</th>
            <th>Acciones</th>
          </tr>
        </thead>

        <tbody>
          {motos.map((moto) => (
            <tr key={moto.id}>
              <td>{moto.marca || 'Sin marca'}</td>
              <td>{moto.modelo || 'Sin modelo'}</td>
              <td>{moto.cilindrada} cc</td>
              <td>
                {Number(moto.precioPorDia ?? 0).toLocaleString(
                  'es-AR',
                  {
                    style: 'currency',
                    currency: 'ARS',
                    maximumFractionDigits: 0,
                  }
                )}
              </td>
              <td>
                <span
                  className={`badge ${
                    moto.disponible
                      ? 'text-bg-success'
                      : 'text-bg-secondary'
                  }`}
                >
                  {moto.disponible ? 'Disponible' : 'Alquilada'}
                </span>
              </td>

              <td>
              <button
                className="btn btn-sm btn-outline-primary"
                  onClick={() => editarMoto(moto)}
                  >
                  Editar
                </button>
              </td>
            </tr>
          ))}

          {motos.length === 0 && (
            <tr>
              <td colSpan="5">No hay motos registradas.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )}
</article>

          ) : seccion === 'Clientes' ? (
          <article className="placeholder-card clientes-ancho">
  <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
    <h2 className="mb-0">Clientes registrados</h2>

   <button
  className="primary-button"
  onClick={() => {
    if (mostrarFormularioCliente) {
      cancelarFormularioCliente();
    } else {
      cancelarFormularioCliente();
      setMostrarFormularioCliente(true);
    }
  }}
>
  <Users size={17} />
  {mostrarFormularioCliente ? 'Cancelar' : 'Nuevo cliente'}
</button>
  </div>

  {mostrarFormularioCliente && (
    <form onSubmit={guardarCliente} className="card card-body mb-4">
     <h4 className="mb-3">
       {modoEdicionCliente ? 'Editar cliente' : 'Registrar un cliente'}
      </h4>

      <div className="row g-3">
        <div className="col-md-6">
          <label className="form-label">Nombre</label>
          <input
            type="text"
            name="nombre"
            className="form-control"
            value={nuevoCliente.nombre}
            onChange={manejarCambioCliente}
            required
          />
        </div>

        <div className="col-md-6">
          <label className="form-label">Apellido</label>
          <input
            type="text"
            name="apellido"
            className="form-control"
            value={nuevoCliente.apellido}
            onChange={manejarCambioCliente}
            required
          />
        </div>

        <div className="col-md-6">
          <label className="form-label">DNI</label>
          <input
            type="text"
            name="dni"
            className="form-control"
            value={nuevoCliente.dni}
            onChange={manejarCambioCliente}
            required
          />
        </div>

        <div className="col-md-6">
          <label className="form-label">Teléfono</label>
          <input
            type="text"
            name="telefono"
            className="form-control"
            value={nuevoCliente.telefono}
            onChange={manejarCambioCliente}
            required
          />
        </div>

        <div className="col-12">
          <label className="form-label">Email</label>
          <input
            type="email"
            name="email"
            className="form-control"
            value={nuevoCliente.email}
            onChange={manejarCambioCliente}
            required
          />
        </div>
      </div>

      {errorFormularioCliente && (
        <p className="text-danger mt-3">{errorFormularioCliente}</p>
      )}

      <div className="mt-4">
        <button
          type="submit"
          className="primary-button"
          disabled={guardandoCliente}
        >
         {guardandoCliente ? 'Guardando...': modoEdicionCliente
         ? 'Guardar cambios': 'Guardar cliente'}
        </button>
      </div>
    </form>
  )}

  {cargandoClientes && <p>Cargando clientes...</p>}

  {errorClientes && (
    <p className="text-danger">{errorClientes}</p>
  )}

  {!cargandoClientes && !errorClientes && (
    <div className="table-responsive">
      <table className="table table-hover align-middle">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Apellido</th>
            <th>DNI</th>
            <th>Teléfono</th>
            <th>Email</th>
            <th>Cuenta de acceso</th>
            <th>Acciones</th>
          </tr>
        </thead>

        <tbody>
          {clientes.map((cliente) => (
            <tr key={cliente.id}>
              <td>{cliente.nombre}</td>
              <td>{cliente.apellido}</td>
              <td>{cliente.dni}</td>
              <td>{cliente.telefono}</td>
              <td>{cliente.email}</td>
              <td>
  {cargandoEstadosCuentas ? (
    <span className="text-muted">Consultando...</span>
  ) : estadosCuentas[cliente.id]?.tieneCuenta ? (
    <span className="badge bg-success">
      Cuenta creada
    </span>
  ) : estadosCuentas[cliente.id]?.tieneAlquilerActivo ? (
    <span className="badge bg-warning text-dark">
      Sin cuenta
    </span>
  ) : (
    <span className="badge bg-secondary">
      Sin alquiler activo
    </span>
  )}
</td>
            
<td>
  <div className="d-flex gap-2 flex-wrap">
    <button
      className="btn btn-sm btn-outline-primary"
      onClick={() => editarCliente(cliente)}
    >
      Editar
    </button>

    {!cargandoEstadosCuentas &&
      estadosCuentas[cliente.id]?.tieneAlquilerActivo &&
      !estadosCuentas[cliente.id]?.tieneCuenta && (
        <button
          className="btn btn-sm btn-outline-success"
          onClick={() => {
            setClienteAccesoId(
              clienteAccesoId === cliente.id ? null : cliente.id
            );
            setClaveTemporal('');
            setErrorAcceso('');
          }}
        >
          Crear acceso
        </button>
      )}
  </div>

  {clienteAccesoId === cliente.id && (
    <form
      className="mt-3"
      onSubmit={(e) => crearAccesoCliente(e, cliente.id)}
    >
      <label className="form-label">
        Contraseña temporal
      </label>

      <input
        type="password"
        className="form-control form-control-sm mb-2"
        value={claveTemporal}
        onChange={(e) => setClaveTemporal(e.target.value)}
        autoComplete="new-password"
        minLength={6}
        required
      />

      {errorAcceso && (
        <p className="text-danger small">{errorAcceso}</p>
      )}

      <button
        type="submit"
        className="btn btn-sm btn-success"
        disabled={guardandoAcceso}
      >
        {guardandoAcceso ? 'Creando...' : 'Guardar acceso'}
      </button>
    </form>
  )}
</td>

            </tr>
          ))}

          {clientes.length === 0 && (
            <tr>
              <td colSpan="7">No hay clientes registrados.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )}
</article>
          
) : seccion === 'Alquileres' ? (
  <article className="placeholder-card alquileres-ancho">
    <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
      <h2 className="mb-0">Alquileres registrados</h2>

      <button
        className="primary-button"
        onClick={() => setMostrarFormularioAlquiler(!mostrarFormularioAlquiler)}
      >
        <ClipboardList size={17} />
        {mostrarFormularioAlquiler ? 'Cancelar' : 'Nuevo alquiler'}
      </button>
    </div>

    {mostrarFormularioAlquiler && (
      <form onSubmit={guardarAlquiler} className="card card-body mb-4">
        <h4 className="mb-3">Registrar un alquiler</h4>

        <div className="row g-3">
          <div className="col-md-6">
            <label className="form-label">Cliente</label>
            <select
              name="clienteId"
              className="form-select"
              value={nuevoAlquiler.clienteId}
              onChange={manejarCambioAlquiler}
              required
            >
              <option value="">Seleccionar cliente</option>

              {clientes.map((cliente) => (
                <option key={cliente.id} value={cliente.id}>
                  {cliente.nombre} {cliente.apellido} - DNI: {cliente.dni}
                </option>
              ))}
            </select>
          </div>

          <div className="col-md-6">
            <label className="form-label">Moto disponible</label>
            <select
              name="motoId"
              className="form-select"
              value={nuevoAlquiler.motoId}
              onChange={manejarCambioAlquiler}
              required
            >
              <option value="">Seleccionar moto</option>

              {motos
                .filter((moto) => moto.disponible)
                .map((moto) => (
                  <option key={moto.id} value={moto.id}>
                    {moto.marca} {moto.modelo} - {moto.cilindrada} cc
                  </option>
                ))}
            </select>
          </div>

          <div className="col-md-6">
            <label className="form-label">Fecha de inicio</label>
            <input
              type="date"
              name="fechaInicio"
              className="form-control"
              value={nuevoAlquiler.fechaInicio}
              onChange={manejarCambioAlquiler}
              required
            />
          </div>

          <div className="col-12">
            <div className="form-check">
              <input
                type="checkbox"
                name="opcionCompra"
                className="form-check-input"
                checked={nuevoAlquiler.opcionCompra}
                onChange={manejarCambioAlquiler}
                id="opcionCompra"
              />

              <label className="form-check-label" htmlFor="opcionCompra">
                Incluir opción de compra después de 6 meses
              </label>
            </div>
          </div>
        </div>

        {errorFormularioAlquiler && (
          <p className="text-danger mt-3">
            {errorFormularioAlquiler}
          </p>
        )}

        <div className="mt-4">
          <button
            type="submit"
            className="primary-button"
            disabled={guardandoAlquiler}
          >
            {guardandoAlquiler ? 'Guardando...' : 'Registrar alquiler'}
          </button>
        </div>
      </form>
    )}

    {cargandoAlquileres && <p>Cargando alquileres...</p>}

    {errorAlquileres && (
      <p className="text-danger">{errorAlquileres}</p>
    )}

    {!cargandoAlquileres && !errorAlquileres && (
      <div className="table-responsive">
        <table className="table table-hover align-middle">
          <thead>
            <tr>
              <th>Cliente</th>
              <th>Moto</th>
              <th>Fecha de inicio</th>
              <th>Fecha de devolución</th>
              <th>Precio por día</th>
              <th>Opción de compra</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>

          <tbody>
            {alquileres.map((alquiler) => (
              <tr key={alquiler.id}>
                <td>
                  {alquiler.cliente
                    ? `${alquiler.cliente.nombre} ${alquiler.cliente.apellido}`
                    : 'Sin cliente'}
                </td>

                <td>
                  {alquiler.moto
                    ? `${alquiler.moto.marca} ${alquiler.moto.modelo}`
                    : 'Sin moto'}
                </td>

                <td>
                  {alquiler.fechaInicio
                    ? new Date(alquiler.fechaInicio).toLocaleDateString('es-AR')
                    : '—'}
                </td>

                <td>
               {alquiler.fechaDevolucion
                ? new Date(alquiler.fechaDevolucion).toLocaleDateString('es-AR')
                  : '—'}
                </td>

                <td>
                  {Number(alquiler.precioPorDia ?? 0).toLocaleString(
                    'es-AR',
                    {
                      style: 'currency',
                      currency: 'ARS',
                      maximumFractionDigits: 0,
                    }
                  )}
                </td>

                <td>{alquiler.opcionCompra ? 'Sí' : 'No'}</td>

               {/* Columna Estado */}
<td>
  <span
    className={`badge ${
      alquiler.estado === 'Activo'
        ? 'text-bg-success'
        : 'text-bg-secondary'
    }`}
  >
    {alquiler.estado}
  </span>
</td>

{/* Columna Acciones */}
<td>
  {alquiler.estado === 'Activo' && (
    <button
      type="button"
      className="btn btn-outline-primary btn-sm"
      onClick={() => devolverAlquiler(alquiler.id)}
    >
      Devolver moto
    </button>
  )}

  {alquiler.estado === 'Finalizado' && (
    <span className="text-muted">Sin acciones</span>
  )}
</td>
              </tr>
            ))}

            {alquileres.length === 0 && (
              <tr>
                <td colSpan="8">No hay alquileres registrados.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    )}
  </article>


  ) : seccion === 'Solicitudes' ? (
    <Solicitudes
      solicitudes={solicitudes}
      cargando={cargandoSolicitudes}
      error={errorSolicitudes}
      onAprobar={aprobarSolicitud}
      onRechazar={rechazarSolicitud}
    />
 
    ) : seccion === 'Pagos' ? (
  <Pagos
    pagos={pagos}
    cargando={cargandoPagos}
    error={errorPagos}
  />
) : (

  <article className="placeholder-card">
    <div className="placeholder-icon">
      {(() => {
        const Icono =
          secciones.find((item) => item.nombre === seccion)?.icono ??
          LayoutDashboard;

        return <Icono size={30} />;
      })()}
    </div>

    <h2>Gestión de {seccion.toLowerCase()}</h2>

    <p>
      Esta sección ya está preparada visualmente. En el siguiente
      paso conectaremos sus datos reales con la API de MotoRent.
    </p>

    <button
      className="primary-button"
      onClick={() => setSeccion('Inicio')}
    >
      Volver al inicio
    </button>
  </article>
)}


        </section>
      </main>
    </div>
  );
}

export default App;
