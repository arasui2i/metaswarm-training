import { render, screen, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

vi.mock('../api/client', () => ({
  getToken: vi.fn(),
  clearToken: vi.fn(),
  setToken: vi.fn(),
  client: { post: vi.fn(), interceptors: { request: { use: vi.fn() } } },
}));

import { getToken, clearToken } from '../api/client';
import { AuthProvider, useAuth } from '../context/AuthContext';

function TestConsumer() {
  const { user, isAuthenticated, setAuth, logout } = useAuth();
  return (
    <div>
      <div data-testid="authenticated">{String(isAuthenticated)}</div>
      <div data-testid="email">{user?.email ?? 'none'}</div>
      <button onClick={() => setAuth({ id: '1', email: 'a@b.com', username: 'a', roles: [] }, 'tok')}>
        login
      </button>
      <button onClick={logout}>logout</button>
    </div>
  );
}

describe('AuthContext', () => {
  beforeEach(() => {
    vi.mocked(getToken).mockReturnValue(null);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('starts unauthenticated when no stored token', () => {
    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );
    expect(screen.getByTestId('authenticated').textContent).toBe('false');
    expect(screen.getByTestId('email').textContent).toBe('none');
  });

  it('rehydrates user from a valid stored JWT on mount', () => {
    const payload = btoa(JSON.stringify({ sub: '1', email: 'stored@crm.com', unique_name: 'stored' }));
    const fakeToken = `header.${payload}.sig`;
    vi.mocked(getToken).mockReturnValue(fakeToken);

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    expect(screen.getByTestId('authenticated').textContent).toBe('true');
    expect(screen.getByTestId('email').textContent).toBe('stored@crm.com');
  });

  it('setAuth marks user as authenticated', async () => {
    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {
      screen.getByRole('button', { name: 'login' }).click();
    });

    expect(screen.getByTestId('authenticated').textContent).toBe('true');
    expect(screen.getByTestId('email').textContent).toBe('a@b.com');
  });

  it('logout clears user and calls clearToken', async () => {
    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await act(async () => {
      screen.getByRole('button', { name: 'login' }).click();
    });

    await act(async () => {
      screen.getByRole('button', { name: 'logout' }).click();
    });

    expect(screen.getByTestId('authenticated').textContent).toBe('false');
    expect(clearToken).toHaveBeenCalled();
  });

  it('throws if useAuth is used outside AuthProvider', () => {
    const spy = vi.spyOn(console, 'error').mockImplementation(() => {});
    expect(() => render(<TestConsumer />)).toThrow();
    spy.mockRestore();
  });
});
