
🏍️ MotoRent
Sistema de alquiler de motos: Trabajo Final Integrador de Programación de Aplicaciones Distribuidas.
MotoRent permite que una persona vea las motos disponibles, solicite un alquiler validando su identidad y, una vez aprobada, pague los días de alquiler desde su panel con Mercado Pago. El administrador gestiona motos, clientes, solicitudes, alquileres y pagos desde su propio panel.
🌐 Ver la aplicación
👉 motorent-tucuman.netlify.app
No hace falta instalar nada: la aplicación está publicada y se usa directamente desde el navegador, en la computadora o en el celular.
Página	Dirección
Inicio y catálogo	/
Iniciar sesión	/login
Panel del cliente	`/cliente` (requiere iniciar sesión como cliente)
Panel del administrador	`/admin` (requiere iniciar sesión como administrador)
---
✨ Funcionalidades
Visitante
*Catálogo de motos disponibles (marca, modelo, cilindrada, precio por día).
*Solicitud de alquiler con validación de DNI contra el servicio RENAPER (académico, con datos ficticios).
*Opción de indicar interés en la compra de la moto después de 6 meses.
Cliente (`/cliente`)
*Ver la moto alquilada, los días transcurridos, los días pendientes y la deuda total.
*Calcular el importe de los días a pagar y pagar con Mercado Pago (Checkout Pro).
*Retomar o cancelar un pago pendiente.
*Ver el detalle de cada día de alquiler (pagado / pendiente).
Administrador (`/admin`)
*ABM de motos y clientes.
*Aprobar o rechazar solicitudes de alquiler.
*Crear cuentas de clientes, registrar devoluciones y consultar estados de cuenta.
*Consultar todos los pagos registrados.
---
🧰 Tecnologías
Parte	Tecnologías
Frontend:	React 19, Vite, React Router, Bootstrap 5, Axios, Lucide (íconos)
Backend:	ASP.NET Core 8 (Web API), Entity Framework Core, ASP.NET Identity, JWT, Swagger
Base de datos	SQL Server (SQL Express)
Servicios externos:	Mercado Pago (pagos), API RENAPER (validación de identidad)
Publicación	Netlify (frontend)
---
📁 Estructura del proyecto
```
MotoRent/
├── backend/
│   ├── TFI-PAD.slnx                 # Solución de Visual Studio
│   └── MotoRent.Api/
│       ├── Controllers/             # Endpoints de la API
│       ├── Models/                  # Entidades y DTOs
│       ├── Data/                    # DbContext (Entity Framework)
│       ├── Services/                # Integración con Mercado Pago y RENAPER
│       ├── Migrations/              # Migraciones de la base de datos
│       └── Program.cs               # Configuración (JWT, CORS, Swagger, roles)
└── frontend/
    ├── public/                      # Logo e íconos
    └── src/
        ├── pages/                   # Pantallas (Home, Login, ClienteDashboard, etc.)
        ├── components/              # Ruta protegida, botón de cerrar sesión
        ├── services/api.js          # Cliente Axios + envío del token JWT
        ├── AdminDashboard.jsx       # Panel del administrador
        └── theme.css                # Estilos globales
```
---
🔐 Roles y acceso
Administrador:Se crea automáticamente al iniciar el backend en desarrollo	`/admin`
Cliente	:El administrador le crea la cuenta cuando ya tiene un alquiler registrado	`/cliente`
El login : (`/login`) devuelve un token JWT que el frontend guarda en `sessionStorage` y envía en cada petición a la API.
---
🔌 Endpoints principales
Recurso	Ruta base	Acceso
Autenticación	`POST /api/Auth/login`	Público
Motos	`/api/Motos`	Lectura pública, edición solo admin
Validación de DNI	`POST /api/validacion-dni`	Público
Solicitudes	`/api/Solicitudes`	Crear: público · Gestionar: admin
Clientes / Cuentas	`/api/Clientes`, `/api/cuentas-clientes`	Admin
Alquileres	`/api/Alquileres`	Admin · `mi-estado-cuenta`: cliente
Pagos	`/api/Pagos`	Admin · `calcular`: cliente
Órdenes de pago (Mercado Pago)	`/api/ordenes-pago`	Cliente

---
👥 Equipo

Camila Mena: @camilamena12
Joel Mansilla : @joelmansilla2000
---
Proyecto académico. La validación de identidad usa un servicio con datos ficticios y los pagos se procesan en el entorno de prueba de Mercado Pago.
