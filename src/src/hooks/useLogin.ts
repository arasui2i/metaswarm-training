import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { loginApi, type LoginRequest } from '../api/auth';
import { setToken } from '../api/client';
import { useAuth } from '../context/AuthContext';

export function useLogin() {
  const navigate = useNavigate();
  const { setAuth } = useAuth();

  return useMutation({
    mutationFn: (payload: LoginRequest) => loginApi(payload),
    onSuccess: (data, variables) => {
      setToken(data.accessToken, variables.rememberMe);
      setAuth(data.user, data.accessToken);
      navigate('/customers');
    },
  });
}
