function Pagos({ pagos, cargando, error }) {
  if (cargando) {
    return <p>Cargando pagos...</p>;
  }

  if (error) {
    return <p className="text-danger">{error}</p>;
  }

  return (
    <div>
      <h2>Gestión de pagos</h2>
      <p>Consultá los pagos registrados de los alquileres.</p>

      {pagos.length === 0 ? (
        <p>Todavía no hay pagos registrados.</p>
      ) : (
        <div className="table-responsive">
          <table className="table table-striped align-middle">
            <thead>
              <tr>
                <th>ID</th>
              <th>Cliente</th>
                <th>Moto</th>
                <th>Alquiler</th>
                <th>Día alquilado</th>
                <th>Importe</th>
                <th>Método</th>
                <th>Estado</th>
              </tr>
            </thead>

            <tbody>
              {pagos.map((pago) => (
                <tr key={pago.id}>
                  <td>{pago.id}</td>
                  <td>{pago.alquiler?.cliente? `${pago.alquiler.cliente.nombre} ${pago.alquiler.cliente.apellido}` : 'Cliente no disponible'}
                </td>

                <td>
                    {pago.alquiler?.moto
                     ? `${pago.alquiler.moto.marca} ${pago.alquiler.moto.modelo}`
                     : 'Moto no disponible'}
                </td>

<td>#{pago.alquilerId}</td>
                  <td>
                    {new Date(pago.diaAlquilado).toLocaleDateString('es-AR')}
                  </td>
                  <td>
                    ${Number(pago.importe).toLocaleString('es-AR')}
                  </td>
                  <td>{pago.metodoPago}</td>
                  <td>{pago.estado}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export default Pagos;