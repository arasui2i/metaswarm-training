import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { ReactNode } from 'react';

vi.mock('react-router-dom', async (importOriginal) => ({
  ...(await importOriginal<typeof import('react-router-dom')>()),
  useNavigate: vi.fn(),
}));

vi.mock('../context/AuthContext', () => ({
  useAuth: vi.fn(),
}));

vi.mock('../api/client', () => ({
  setToken: vi.fn(),
  getToken: vi.fn().mockReturnValue(null),
  clearToken: vi.fn(),
  client: { post: vi.fn(), interceptors: { request: { use: vi.fn() } } },
}));

vi.mock('../api/auth', () => ({
  loginApi: vi.fn(),
}));

import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { setToken } from '../api/client';
import { loginApi } from '../api/auth';
import { useLogin } from '../hooks/useLogin';

const mockNavigateFn = vi.fn();

function makeWrapper() {
  const qc = new QueryClient({ defaultOptions: { mutations: { retry: false } } });
  return function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={qc}>{children}</QueryClientProvider>;
  };
}

describe('useLogin', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useNavigate).mockReturnValue(mockNavigateFn);
    vi.mocked(useAuth).mockReturnValue({
      setAuth: vi.fn(),
      user: null,
      token: null,
      isAuthenticated: false,
      logout: vi.fn(),
    });
  });

  it('calls loginApi with the supplied payload', async () => {
    vi.mocked(loginApi).mockResolvedValue({
      accessToken: 'tok',
      expiresAt: '2099-01-01',
      user: { id: '1', email: 'a@b.com', username: 'a', roles: [] },
    });

    const { result } = renderHook(() => useLogin(), { wrapper: makeWrapper() });
    result.current.mutate({ emailOrUsername: 'a@b.com', password: '123456', rememberMe: false });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(loginApi).toHaveBeenCalledWith({
      emailOrUsername: 'a@b.com',
      password: '123456',
      rememberMe: false,
    });
  });

  it('stores token with rememberMe flag on success', async () => {
    vi.mocked(loginApi).mockResolvedValue({
      accessToken: 'jwt',
      expiresAt: '2099-01-01',
      user: { id: '1', email: 'a@b.com', username: 'a', roles: [] },
    });

    const { result } = renderHook(() => useLogin(), { wrapper: makeWrapper() });
    result.current.mutate({ emailOrUsername: 'a@b.com', password: '123456', rememberMe: true });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(setToken).toHaveBeenCalledWith('jwt', true);
  });

  it('sets auth context with user and token on success', async () => {
    const mockSetAuth = vi.fn();
    vi.mocked(useAuth).mockReturnValue({
      setAuth: mockSetAuth,
      user: null,
      token: null,
      isAuthenticated: false,
      logout: vi.fn(),
    });
    const user = { id: '1', email: 'a@b.com', username: 'a', roles: ['Admin'] };
    vi.mocked(loginApi).mockResolvedValue({ accessToken: 'tok', expiresAt: '2099-01-01', user });

    const { result } = renderHook(() => useLogin(), { wrapper: makeWrapper() });
    result.current.mutate({ emailOrUsername: 'a@b.com', password: '123456', rememberMe: false });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockSetAuth).toHaveBeenCalledWith(user, 'tok');
  });

  it('navigates to /customers on success', async () => {
    vi.mocked(loginApi).mockResolvedValue({
      accessToken: 'tok',
      expiresAt: '2099-01-01',
      user: { id: '1', email: 'a@b.com', username: 'a', roles: [] },
    });

    const { result } = renderHook(() => useLogin(), { wrapper: makeWrapper() });
    result.current.mutate({ emailOrUsername: 'a@b.com', password: '123456', rememberMe: false });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(mockNavigateFn).toHaveBeenCalledWith('/customers');
  });

  it('sets isError when loginApi rejects', async () => {
    vi.mocked(loginApi).mockRejectedValue(new Error('Unauthorized'));

    const { result } = renderHook(() => useLogin(), { wrapper: makeWrapper() });
    result.current.mutate({ emailOrUsername: 'bad', password: 'wrong!', rememberMe: false });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(mockNavigateFn).not.toHaveBeenCalled();
  });
});
