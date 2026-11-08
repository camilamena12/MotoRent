import { useEffect, useState } from 'react';
import { Bike, CalendarDays, Wallet, CreditCard } from 'lucide-react';
import api from '../services/api';
import BotonCerrarSesion from '../components/BotonCerrarSesion';

function ClienteDashboard() {
  const [estadoCuenta, setEstadoCuenta] = useState(null);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState('');
  const [cantidadDias, setCantidadDias] = useState(1);
  const [calculoPago, setCalculoPago] = useState(null);
  const [creandoOrden, setCreandoOrden] = useState(false);
  const [ordenPendiente, setOrdenPendiente] = useState(null);
  const [cargandoOrden, setCargandoOrden] = useState(true);
  const [cancelandoOrden, setCancelandoOrden] = useState(false);
  const [errorOrden, setErrorOrden] = useState('');

  useEffect(() => {
    const cargarEstadoCuenta = async () => {
      try {
        const respuesta = await api.get(
  '/Alquileres/mi-estado-cuenta'
);

        setEstadoCuenta(respuesta.data);
      } catch (error) {
        console.error(error);
        setError('No pudimos cargar tu estado de cuenta.');
      } finally {
        setCargando(false);
      }
    };

    cargarEstadoCuenta();
  }, []);

  

useEffect(() => {
  if (!estadoCuenta?.alquilerId) return;

  let activo = true;
  let consultando = false;

  const verificarPagos = async () => {
    if (consultando) return;

    consultando = true;

    try {
      if (activo) {
        setCargandoOrden(true);
        setErrorOrden('');
      }

      const alquilerId = estadoCuenta.alquilerId;

      // Buscar una orden pendiente
      const respuesta = await api.get(
        `/ordenes-pago/pendiente/${alquilerId}`
      );

      if (!activo) return;

      const orden = respuesta.data;

      if (!orden.hayOrdenPendiente) {
        setOrdenPendiente(null);
        return;
      }

      setOrdenPendiente(orden);

      // Si la orden ya fue enviada a Mercado Pago,
      // comprobar si el pago fue acreditado
      if (!orden.checkoutUrl) return;

      await api.post(
        `/ordenes-pago/${orden.ordenId}/confirmar`
      );

      // Consultar nuevamente para saber si sigue pendiente
      const nuevaConsulta = await api.get(
        `/ordenes-pago/pendiente/${alquilerId}`
      );

      if (!activo) return;

      if (!nuevaConsulta.data.hayOrdenPendiente) {
        setOrdenPendiente(null);
        setCalculoPago(null);

        // Actualizar los días pagados y la deuda
        const cuentaActualizada = await api.get(
          '/Alquileres/mi-estado-cuenta'
        );

        if (activo) {
          setEstadoCuenta(cuentaActualizada.data);
        }
      } else {
        setOrdenPendiente(nuevaConsulta.data);
      }

    } catch (error) {
      console.error('Error al verificar pagos:', error);

      if (activo) {
        setErrorOrden(
          'No pudimos verificar el estado del pago. Actualizá la página.'
        );
      }
    } finally {
      consultando = false;

      if (activo) {
        setCargandoOrden(false);
      }
    }
  };

  // Verificar cuando se abre el panel
  verificarPagos();

  // Verificar también cuando el usuario vuelve
  window.addEventListener('pageshow', verificarPagos);

  return () => {
    activo = false;
    window.removeEventListener('pageshow', verificarPagos);
  };
}, [estadoCuenta?.alquilerId]);


const calcularPago = async () => {
  try {
    setCalculoPago(null);

    const respuesta = await api.post('/Pagos/calcular', {
      alquilerId: estadoCuenta.alquilerId,
      cantidadDias: Number(cantidadDias)
    });

    setCalculoPago(respuesta.data);
  } catch (error) {
    console.error(error);
    alert(
      error.response?.data ||
      'No se pudo calcular el pago.'
    );
  }
};


const iniciarPagoMercadoPago = async () => {
  if (!calculoPago || creandoOrden) return;

  try {
    setCreandoOrden(true);

    const respuesta = await api.post('/ordenes-pago', {
      alquilerId: estadoCuenta.alquilerId,
      cantidadDias: calculoPago.cantidadDias
    });

    const checkoutUrl = respuesta.data.checkoutUrl;

    if (!checkoutUrl || !checkoutUrl.startsWith('https://')) {
      throw new Error('Mercado Pago no devolvió un enlace válido.');
    }
    setOrdenPendiente({
  ordenId: respuesta.data.ordenId,
  importeTotal: respuesta.data.importeTotal,
  cantidadDias: calculoPago.cantidadDias,
  checkoutUrl
});

setCreandoOrden(false);
    // Redirigir al Checkout Pro de prueba
    window.location.assign(checkoutUrl);

  } catch (error) {
    console.error('Error al iniciar el pago:', error);

    const mensaje = error.response?.data;

    alert(
      typeof mensaje === 'string'
        ? mensaje
        : mensaje?.mensaje ||
          error.message ||
          'No se pudo iniciar el pago con Mercado Pago.'
    );

    setCreandoOrden(false);
  }
};




  if (cargando) {
    return <p className="text-center mt-5">Cargando tu cuenta...</p>;
  }

const cancelarOrdenPendiente = async () => {
  if (!ordenPendiente || cancelandoOrden) return;

  const confirmar = window.confirm(
    '¿Querés cancelar este pago pendiente para elegir otra cantidad de días?'
  );

  if (!confirmar) return;

  try {
    setCancelandoOrden(true);

    const respuesta = await api.post(
      `/ordenes-pago/${ordenPendiente.ordenId}/cancelar`
    );

    setOrdenPendiente(null);
    setCalculoPago(null);

    alert(respuesta.data.mensaje);

  } catch (error) {
    console.error('Error al cancelar:', error);

    const mensaje = error.response?.data;

    alert(
      typeof mensaje === 'string'
        ? mensaje
        : mensaje?.mensaje || 'No se pudo cancelar la orden.'
    );
  } finally {
    setCancelandoOrden(false);
  }
};




  if (error) {
    return <p className="text-center mt-5 text-danger">{error}</p>;
  }

  if (!estadoCuenta) {
    return <p className="text-center mt-5">No hay información disponible.</p>;
  }

  return (
    <div className="container py-5">
      <div className="d-flex justify-content-between align-items-start mb-4 flex-wrap gap-3">
  <div>
    <h2>Mi cuenta</h2>
    <p className="text-muted">
      Consultá tu alquiler y tus pagos.
    </p>
  </div>

  <BotonCerrarSesion />
</div>

      <div className="card shadow-sm mb-4">
        <div className="card-body">
          <h4>
            <Bike size={24} className="me-2" />
            Mi moto alquilada
          </h4>

          <h5 className="mt-3">{estadoCuenta.moto}</h5>
          <p className="text-muted mb-1">
            Alquiler #{estadoCuenta.alquilerId}
          </p>
          <p>
            Precio por día: ${estadoCuenta.precioPorDia.toLocaleString('es-AR')}
          </p>
        </div>
      </div>

      <div className="row g-3 mb-4">
        <div className="col-md-4">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <CalendarDays className="mb-2" />
              <p className="text-muted mb-1">Días transcurridos</p>
              <h3>{estadoCuenta.diasTranscurridos}</h3>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <CreditCard className="mb-2" />
              <p className="text-muted mb-1">Días pendientes</p>
              <h3>{estadoCuenta.diasPendientes}</h3>
            </div>
          </div>
        </div>

        <div className="col-md-4">
          <div className="card shadow-sm h-100">
            <div className="card-body">
              <Wallet className="mb-2" />
              <p className="text-muted mb-1">Deuda total</p>
              <h3>
                ${estadoCuenta.deudaTotal.toLocaleString('es-AR')}
              </h3>
            </div>
          </div>
        </div>
      </div>

      <div className="card shadow-sm">
        <div className="card-body">
            <div className="card shadow-sm mb-4">
  <div className="card-body">
    <h4 className="mb-3">
      <CreditCard size={24} className="me-2" />
      Pagar mi alquiler
    </h4>

    <p className="text-muted">
      Elegí cuántos días pendientes querés abonar.
    </p>

    
{errorOrden && (
  <div className="alert alert-danger">
    {errorOrden}
  </div>
)}

{ordenPendiente && (
  <div className="alert alert-warning mt-3">
    <h5>Tenés un pago pendiente</h5>

    <p>
      Días seleccionados: {ordenPendiente.cantidadDias}
    </p>

    <p>
      Importe: $
      {ordenPendiente.importeTotal.toLocaleString('es-AR')}
    </p>

    <div className="d-flex gap-2 flex-wrap">
      {ordenPendiente.checkoutUrl && (
        <button
          className="btn btn-success"
          onClick={() =>
            window.location.assign(ordenPendiente.checkoutUrl)
          }
        >
          Continuar con el pago
        </button>
      )}

      <button
        className="btn btn-outline-danger"
        onClick={cancelarOrdenPendiente}
        disabled={cancelandoOrden}
      >
        {cancelandoOrden
          ? 'Cancelando...'
          : 'Cancelar y cambiar días'}
      </button>
    </div>
  </div>
)}


    <div className="mb-3">
      <label className="form-label">
        Cantidad de días
      </label>

      <input
        type="number"
        className="form-control"
        min="1"
        max={estadoCuenta.diasPendientes}
        value={cantidadDias}
        onChange={(e) => {
          setCantidadDias(e.target.value);
          setCalculoPago(null);
        }}
      />
    </div>

    <button
      className="btn btn-primary"
      onClick={calcularPago}
     disabled={
  estadoCuenta.diasPendientes === 0 ||
  cargandoOrden ||
  !!ordenPendiente ||
  !!errorOrden
}
    >
      Calcular importe
    </button>

    {calculoPago && !ordenPendiente && !cargandoOrden && !errorOrden &&(
      <div className="alert alert-light border mt-4">
        <h5>Resumen del pago</h5>

        <p>
          Días seleccionados: {calculoPago.cantidadDias}
        </p>

        <p>
          Precio por día: $
          {calculoPago.precioPorDia.toLocaleString('es-AR')}
        </p>

        <h4>
          Total a pagar: $
          {calculoPago.importeTotal.toLocaleString('es-AR')}
        </h4>

       <button className="btn btn-success mt-2" onClick={iniciarPagoMercadoPago}
         disabled={creandoOrden}>
           {creandoOrden? 'Conectando con Mercado Pago...' : 'Pagar con Mercado Pago'}
        </button>
      </div>
    )}
  </div>
</div>
          <h4>Mis días de alquiler</h4>

          <div className="table-responsive mt-3">
            <table className="table">
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Importe</th>
                  <th>Estado</th>
                </tr>
              </thead>

              <tbody>
                {estadoCuenta.detalle.map((dia) => (
                  <tr key={dia.fecha}>
                    <td>
                      {new Date(dia.fecha).toLocaleDateString('es-AR', {
                        timeZone: 'UTC'
                      })}
                    </td>
                    <td>${dia.importe.toLocaleString('es-AR')}</td>
                    <td>
                      <span
                        className={`badge ${
                          dia.estado === 'Pagado'
                            ? 'bg-success'
                            : 'bg-warning text-dark'
                        }`}
                      >
                        {dia.estado}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

        </div>
      </div>
    </div>
  );
}

export default ClienteDashboard;