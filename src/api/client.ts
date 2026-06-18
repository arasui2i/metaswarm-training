import axios from 'axios';

export const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5000',
});

client.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export function setToken(token: string, rememberMe: boolean): void {
  const storage = rememberMe ? localStorage : sessionStorage;
  storage.setItem('crm_token', token);
}

export function getToken(): string | null {
  return localStorage.getItem('crm_token') ?? sessionStorage.getItem('crm_token');
}

export function clearToken(): void {
  localStorage.removeItem('crm_token');
  sessionStorage.removeItem('crm_token');
}
