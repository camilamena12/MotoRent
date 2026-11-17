# MotoRent

**Trabajo Final Integrador de Programación de Aplicaciones Distribuidas — UTN-FRT**

MotoRent es un sistema web de alquiler diario de motocicletas. Permite consultar las motos disponibles, solicitar un alquiler mediante validación de identidad y, una vez aprobada la solicitud, acceder a un portal de cliente para consultar y pagar los días de alquiler mediante Mercado Pago. El administrador gestiona motos, clientes, solicitudes, alquileres y pagos desde un panel propio.

## Aplicación publicada

- **Sitio web:** https://motorent-tucuman.netlify.app/
- **API:** https://motorent-api-0nzy.onrender.com/

La aplicación se utiliza desde el navegador, tanto en computadoras como en dispositivos móviles, sin necesidad de instalación.

| Página | Ruta |
|---|---|
| Inicio y catálogo | `/` |
| Inicio de sesión | `/login` |
| Panel del cliente | `/cliente` (requiere iniciar sesión como cliente) |
| Panel del administrador | `/admin` (requiere iniciar sesión como administrador) |

## Funcionalidades

### Visitante

- Consultar motos disponibles: marca, modelo, cilindrada y precio diario.
- Solicitar un alquiler con validación de DNI mediante un servicio académico de RENAPER con datos ficticios.
- Indicar interés en la opción de compra después de seis meses, según las condiciones del alquiler.

### Cliente

- Consultar la moto alquilada, los días transcurridos, los días pendientes de pago y la deuda total.
- Calcular el importe y pagar días de alquiler mediante Mercado Pago (Checkout Pro, entorno de prueba).
- Retomar o cancelar una orden de pago pendiente.
- Consultar el detalle de cada día de alquiler (pagado o pendiente).

### Administrador

- Gestionar motos y clientes (altas, bajas y modificaciones).
- Aprobar o rechazar solicitudes de alquiler.
- Crear cuentas de clientes, registrar devoluciones y consultar estados de cuenta.
- Consultar los pagos registrados y las estadísticas generales del negocio.

## Tecnologías utilizadas

| Componente | Tecnologías |
|---|---|
| Frontend | React, Vite, React Router, Bootstrap, Axios y Lucide React |
| Backend | ASP.NET Core Web API (.NET 8), Entity Framework Core, ASP.NET Identity y JWT |
| Base de datos | SQL Server (desarrollo local) y Azure SQL (producción) |
| Integraciones | API académica de RENAPER y Mercado Pago Checkout Pro |
| Despliegue | GitHub, Docker, Render y Netlify |

## Arquitectura del sistema

MotoRent utiliza una arquitectura cliente-servidor:

- **Frontend:** aplicación React publicada en Netlify.
- **Backend:** API REST desarrollada en .NET 8 y desplegada en Render mediante Docker.
- **Base de datos:** Azure SQL para almacenar motos, clientes, solicitudes, alquileres y pagos.
- **Servicios externos:** validación de DNI con una API académica de RENAPER y pagos mediante Mercado Pago en modo de prueba.

La autenticación utiliza JWT y distingue los roles de **Administrador** y **Cliente**.

## Estructura del proyecto

```text
MotoRent/
├── backend/
│   ├── TFI-PAD.slnx                 # Solución de Visual Studio
│   └── MotoRent.Api/
│       ├── Controllers/            # Endpoints de la API
│       ├── Models/                 # Entidades y DTOs
│       ├── Data/                   # DbContext de Entity Framework
│       ├── Services/               # Integraciones externas
│       ├── Migrations/             # Migraciones de la base de datos
│       ├── Dockerfile              # Configuración del contenedor
│       └── Program.cs              # Configuración de la aplicación
└── frontend/
    ├── public/                      # Recursos públicos
    └── src/
        ├── pages/                   # Pantallas de la aplicación
        ├── components/              # Componentes reutilizables
        ├── services/api.js          # Cliente Axios y token JWT
        ├── AdminDashboard.jsx       # Panel del administrador
        └── theme.css                # Estilos globales
```

## Roles y acceso

| Rol | Acceso | Descripción |
|---|---|---|
| Administrador | `/admin` | La cuenta inicial se crea si no existe, utilizando credenciales configuradas en variables de entorno. |
| Cliente | `/cliente` | El administrador crea su cuenta cuando corresponde a un alquiler registrado. |

El inicio de sesión se realiza desde `/login`. La API devuelve un token JWT que el frontend almacena en `sessionStorage` y envía en las solicitudes autenticadas.

## Endpoints principales

| Recurso | Ruta base | Acceso |
|---|---|---|
| Autenticación | `POST /api/Auth/login` | Público |
| Motos | `/api/Motos` | Consulta pública; administración restringida |
| Validación de DNI | `POST /api/validacion-dni` | Público |
| Solicitudes | `/api/Solicitudes` | Creación pública; gestión del administrador |
| Clientes y cuentas | `/api/Clientes`, `/api/cuentas-clientes` | Administrador |
| Alquileres | `/api/Alquileres` | Administrador; consulta de estado de cuenta para clientes |
| Pagos | `/api/Pagos` | Administrador; cálculo de importe para clientes |
| Órdenes de pago | `/api/ordenes-pago` | Cliente |

## Mejoras futuras

- Incorporar la gestión de seguros de cada moto, con aseguradora, póliza, fechas de pago y recordatorios de vencimiento.
- Registrar los días de mantenimiento y automatizar la exención del alquiler diario cuando la moto se encuentre en service.

## Equipo

- **Camila Mena:** [@camilamena12](https://github.com/camilamena12)
- **Joel Mansilla:** [@joelmansilla2000](https://github.com/joelmansilla2000)

---

**Nota:** MotoRent es un proyecto académico. La validación de identidad utiliza un servicio con datos ficticios y los pagos se procesan en el entorno de prueba de Mercado Pago.
