import { Check, X, Clock } from 'lucide-react';

function Solicitudes({
  solicitudes,
  cargando,
  error,
  onAprobar,
  onRechazar
}) {
  if (cargando) {
    return <p>Cargando solicitudes...</p>;
  }

  if (error) {
    return <p className="text-danger">{error}</p>;
  }

  return (
    <div className="container-fluid">
      <h2 className="mb-4">Solicitudes de alquiler</h2>

      {solicitudes.length === 0 ? (
        <p>No hay solicitudes registradas.</p>
      ) : (
        <div className="table-responsive">
          <table className="table table-hover align-middle">
            <thead>
              <tr>
                <th>Cliente</th>
                <th>Moto</th>
                <th>Fecha de inicio</th>
                <th>Opción de compra</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>

            <tbody>
              {solicitudes.map((solicitud) => (
                <tr key={solicitud.id}>
                  <td>
                    {solicitud.cliente?.nombre}{' '}
                    {solicitud.cliente?.apellido}
                  </td>

                  <td>
                    {solicitud.moto?.marca}{' '}
                    {solicitud.moto?.modelo}
                  </td>

                  <td>
                    {new Date(
                      solicitud.fechaInicioSolicitada
                    ).toLocaleDateString('es-AR')}
                  </td>

                  <td>
                    {solicitud.opcionCompra ? 'Sí' : 'No'}
                  </td>

                  <td>{solicitud.estado}</td>

                  <td>
                    {solicitud.estado === 'Pendiente' && (
                      <div className="d-flex gap-2">
                        <button
                          className="btn btn-success btn-sm"
                          onClick={() => onAprobar(solicitud.id)}
                          title="Aprobar"
                        >
                          <Check size={16} />
                        </button>

                        <button
                          className="btn btn-danger btn-sm"
                          onClick={() => onRechazar(solicitud.id)}
                          title="Rechazar"
                        >
                          <X size={16} />
                        </button>
                      </div>
                    )}

                    {solicitud.estado !== 'Pendiente' && (
                      <span className="text-muted">
                        Sin acciones
                      </span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default Solicitudes;