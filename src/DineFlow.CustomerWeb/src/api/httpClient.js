import axios from 'axios';

const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7001',
  headers: {
    'Content-Type': 'application/json'
  }
});

export default httpClient;
