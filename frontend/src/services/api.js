//aca centralizamos la direccion de la API. 
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7206/api',
});

export default api;
