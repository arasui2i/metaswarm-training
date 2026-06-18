import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';
import { getToken, clearToken } from '../api/client';

interface AuthUser {
  id: string;
  email: string;
  username: string;
  roles: string[];
}

interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  isAuthenticated: boolean;
  setAuth: (user: AuthUser, token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function parseJwt(token: string): AuthUser | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return {
      id: payload.sub,
      email: payload.email,
      username: payload.unique_name ?? payload.email,
      roles: Array.isArray(payload.role) ? payload.role : payload.role ? [payload.role] : [],
    };
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const stored = getToken();
    if (stored) {
      const parsed = parseJwt(stored);
      if (parsed) {
        setUser(parsed);
        setToken(stored);
      }
    }
  }, []);

  function setAuth(newUser: AuthUser, newToken: string) {
    setUser(newUser);
    setToken(newToken);
  }

  function logout() {
    clearToken();
    setUser(null);
    setToken(null);
  }

  return (
    <AuthContext.Provider value={{ user, token, isAuthenticated: !!user, setAuth, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
