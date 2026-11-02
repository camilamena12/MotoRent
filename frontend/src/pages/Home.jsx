import { Link } from 'react-router-dom';
import { Bike, ShieldCheck, Clock, ArrowRight } from 'lucide-react';
import './Home.css';
import { useEffect, useState } from 'react';
import api from '../services/api';




function Home() {
//conectar el catalogo con la API
const [motos, setMotos] = useState([]);
const [cargando, setCargando] = useState(true);
const [error, setError] = useState('');

useEffect(() => {
  const cargarMotos = async () => {
    try {
      const respuesta = await api.get('/Motos');

      const disponibles = respuesta.data.filter(
        (moto) =>
          moto.disponible &&
          moto.marca &&
          moto.modelo &&
          moto.precioPorDia > 0
      );

      setMotos(disponibles);
    } catch (error) {
      setError('No pudimos cargar las motos. Intentá nuevamente más tarde.');
    } finally {
      setCargando(false);
    }
  };

  cargarMotos();
}, []);



  return (
    <div className="home-page">

      {/* Navegación */}
      <header className="home-header">
        <div className="home-logo">
          <img src="/logo-motorent.png" alt="MotoRent" />
        </div>

        <nav className="home-nav">
          <a href="#inicio">Inicio</a>
          <a href="#motos">Nuestras motos</a>
          <a href="#requisitos">Requisitos</a>
          <a href="#contacto">Contacto</a>
        </nav>

        <Link to="/login" className="home-login">
          Iniciar sesión
        </Link>
      </header>

      {/* Presentación */}
      <section className="home-hero" id="inicio">
        <div className="hero-content">
          <span className="hero-label">MOTORENT · ALQUILER DE MOTOS</span>

          <h1>
            Tu próximo camino
            <br />
            <span>empieza acá.</span>
          </h1>

          <p>
            Alquilá tu moto y empezá a trabajar.
            Una solución práctica para moverte y generar ingresos.
          </p>

          <a href="#motos" className="hero-button">
            Conocé nuestras motos
            <ArrowRight size={19} />
          </a>
        </div>

        <div className="hero-image">
          <Bike size={180} strokeWidth={1} />
        </div>
      </section>

      {/* Beneficios */}
      <section className="home-benefits">
        <article>
          <Bike size={28} />
          <h3>Motos para trabajar</h3>
          <p>Opciones de 110cc y 125cc.</p>
        </article>

        <article>
          <ShieldCheck size={28} />
          <h3>Alquiler organizado</h3>
          <p>Condiciones claras desde el comienzo.</p>
        </article>

        <article>
          <Clock size={28} />
          <h3>Opción de compra</h3>
          <p>Posibilidad de adquirir tu moto según las condiciones del contrato.</p>
        </article>
      </section>

      {/* Catálogo */}
      <section className="home-catalog" id="motos">
        <span className="hero-label">CONOCÉ MOTORENT</span>
        <h2>Nuestras motos</h2>
        <p>Encontrá una moto para comenzar tu próximo proyecto.</p>

      {cargando ? (
  <p>Cargando motos disponibles...</p>
) : error ? (
  <p>{error}</p>
) : motos.length === 0 ? (
  <div className="catalog-placeholder">
    <Bike size={48} />
    <h3>Próximamente, nuevas motos</h3>
    <p>En este momento no hay motos disponibles para alquilar.</p>
  </div>
) : (
  <div className="row g-4 mt-3">
    {motos.map((moto) => (
      <div className="col-md-4" key={moto.id}>
        <div className="card h-100 shadow-sm">
          <div className="card-body">
            <Bike size={40} className="text-danger mb-3" />
            <h3 className="card-title">
              {moto.marca} {moto.modelo}
            </h3>
            <p>{moto.cilindrada} cc</p>
            <h5>
              ${Number(moto.precioPorDia).toLocaleString('es-AR')} por día
            </h5>
            <span className="badge bg-success">Disponible</span>
          </div>
        </div>
      </div>
    ))}
  </div>
)}
      </section>

      {/* Requisitos */}
      <section className="home-requirements" id="requisitos">
        <h2>¿Qué necesitás para alquilar?</h2>
        <p>Te contamos los requisitos para iniciar tu solicitud.</p>

        <ul>
          <li>DNI.</li>
          <li>Factura de servicio.</li>
          <li>Antecedentes penales.</li>
          <li>Licencia de conducir vigente.</li>
          <li>Garante.</li>
        </ul>
      </section>

      {/* Contacto */}
      <footer className="home-footer" id="contacto">
        <h2>MotoRent</h2>
        <p>Tu moto, tu oportunidad.</p>
        <span>© 2026 MotoRent. Todos los derechos reservados.</span>
      </footer>

    </div>
  );
}

export default Home;