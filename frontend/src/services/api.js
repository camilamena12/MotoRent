//aca centralizamos la direccion de la API. 
import axios from 'axios';

const api = axios.create({
 baseURL: import.meta.env.VITE_API_URL,
});

export default api;

// Enviar el token JWT en las solicitudes a la API
api.interceptors.request.use((config) => {
  const token = sessionStorage.getItem('motorent_token');

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});
