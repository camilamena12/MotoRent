
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

import Home from './pages/Home';
import AdminDashboard from './AdminDashboard';

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/admin" element={<AdminDashboard />} />

        <Route
          path="/login"
          element={<h2 className="text-center mt-5">Inicio de sesión: próximamente</h2>}
        />

        <Route
          path="/cliente"
          element={<h2 className="text-center mt-5">Panel del cliente: próximamente</h2>}
        />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
