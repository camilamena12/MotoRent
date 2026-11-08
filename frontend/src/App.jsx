
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

import Home from './pages/Home';
import AdminDashboard from './AdminDashboard';
import SolicitudAlquiler from './pages/SolicitudAlquiler';
import ClienteDashboard from './pages/ClienteDashboard';
import Login from './pages/Login';
import RutaProtegida from './components/RutaProtegida';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
       <Route path="/admin"  element={  <RutaProtegida rolPermitido="Administrador">
      <AdminDashboard />
       </RutaProtegida>
  }
/>

        <Route path="/login" element={<Login />} />

        <Route
  path="/cliente"
  element={
    <RutaProtegida rolPermitido="Cliente">
      <ClienteDashboard />
    </RutaProtegida>
  }
/>

        <Route path="*" element={<Navigate to="/" replace />}
        />

        <Route path="/solicitar-alquiler" element={<SolicitudAlquiler />}
        />


      </Routes>
    </BrowserRouter>
  );
}

export default App;
