import { client } from './client';

export interface LoginRequest {
  emailOrUsername: string;
  password: string;
  rememberMe: boolean;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    username: string;
    roles: string[];
  };
}

export async function loginApi(payload: LoginRequest): Promise<LoginResponse> {
  const { data } = await client.post<LoginResponse>('/api/auth/login', payload);
  return data;
}
