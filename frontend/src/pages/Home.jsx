import { Link } from 'react-router-dom';
import { Bike, Motorbike, ShieldCheck, Clock, ArrowRight, Wrench } from 'lucide-react';
import './Home.css';
import { useEffect, useState } from 'react';
import api from '../services/api';
import motoImage from '../assets/moto.png';




function Home() {

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
          
          <h1>
            Queres trabajar con aplicaciones y no tenes moto?
                        <br />
            <span>Aca tenemos la solución.</span>
          </h1>

          <p>
            Alquilá tu moto y empezá a generar ingresos.
            Una solución práctica para trabajar con aplicaciones como Uber, Pedidos Ya, Didi, etc.
          </p>

          <a href="#motos" className="hero-button">
            Conocé nuestras motos
            <ArrowRight size={19} />
          </a>
        </div>

        <div className="hero-image">
          <img className="hero-motorcycle" src={motoImage} alt="MotoRent" />
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
          <Wrench size={28} />
          <h3>Service incluido</h3>
          <p>El mantenimiento corre por nuestra cuenta.
  ¡El día de service no pagás alquiler!</p>
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
        <p>Encontrá una moto para comenzar a generar ingresos, a tu tiempo y en tus horarios.</p>

      {cargando ? (
  <p>Cargando motos disponibles...</p>
) : error ? (
  <p>{error}</p>
) : motos.length === 0 ? (
  <div className="catalog-placeholder">
    <Motorbike size={46} strokeWidth={1.7} />
    <h3>Próximamente, nuevas motos</h3>
    <p>En este momento no hay motos disponibles para alquilar.</p>
  </div>
) : (
  <div className="row g-3 mt-3">
    {motos.map((moto) => (
     <div className="col-sm-6 col-lg-3" key={moto.id}>

        <article className="moto-catalogo-card">

          
            <div className="moto-catalogo-imagen">
  <Motorbike size={32} strokeWidth={1.6} />

  <span className="badge bg-success moto-badge-disponible">
    Disponible
  </span>
</div>

          <div className="moto-catalogo-contenido">

            <h3>
              {moto.marca} {moto.modelo}
            </h3>

            <p className="moto-catalogo-cilindrada">
              {moto.cilindrada} cc
            </p>

            <p className="moto-catalogo-precio">
              ${Number(moto.precioPorDia).toLocaleString('es-AR')}
              <span> por día</span>
            </p>

            <Link
              to="/solicitar-alquiler"
              state={{ moto }}
              className="moto-catalogo-boton"
            >
              Solicitar alquiler
            </Link>

          </div>
        </article>

      </div>
    ))}
  </div>
)}
</section>

      {/* Requisitos */}
      <section className="home-requirements" id="requisitos">
        <h2>¿Qué necesitás para alquilar?</h2>
        <p>Minimos requisitos</p>

        <ul>
          <li>DNI</li>
          <li>Factura de servicio</li>
          <li>Antecedentes penales</li>
          <li>Licencia de conducir vigente</li>
          <li>Garante</li>
        </ul>
      </section>

      {/* Contacto */}
      <footer className="home-footer" id="contacto">
        <h2>MotoRent</h2>
        <p>Tu moto, tu oportunidad.</p>
        <p>Motorent@gmail.com</p>
        <p>3816688507</p>
        <span>© 2026 MotoRent. Todos los derechos reservados.</span>
      </footer>

    </div>
  );
}

export default Home;