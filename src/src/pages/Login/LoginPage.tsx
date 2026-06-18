import { useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  Alert,
  Box,
  Button,
  Checkbox,
  CircularProgress,
  FormControlLabel,
  Grid,
  IconButton,
  InputAdornment,
  TextField,
  Typography,
} from '@mui/material';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import { useLogin } from '../../hooks/useLogin';

interface LoginFormValues {
  emailOrUsername: string;
  password: string;
  rememberMe: boolean;
}

export function LoginPage() {
  const [showPassword, setShowPassword] = useState(false);
  const mutation = useLogin();

  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormValues>({
    defaultValues: { emailOrUsername: '', password: '', rememberMe: false },
  });

  const onSubmit = (values: LoginFormValues) => {
    mutation.mutate(values);
  };

  return (
    <Grid container sx={{ minHeight: '100vh' }}>
      <Grid
        size={{ xs: false, md: 6 }}
        sx={{
          display: { xs: 'none', md: 'flex' },
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          bgcolor: 'primary.main',
          color: 'primary.contrastText',
          px: 6,
        }}
      >
        <Typography variant="h3" fontWeight={700} gutterBottom>
          CRM
        </Typography>
        <Typography variant="h6" textAlign="center">
          Manage your customers, leads, and accounts — all in one place.
        </Typography>
      </Grid>

      <Grid
        size={{ xs: 12, md: 6 }}
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          px: { xs: 3, sm: 8 },
        }}
      >
        <Box
          component="form"
          onSubmit={handleSubmit(onSubmit)}
          noValidate
          sx={{ width: '100%', maxWidth: 400 }}
        >
          <Typography variant="h5" fontWeight={600} mb={3}>
            Sign in
          </Typography>

          {mutation.isError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              Invalid credentials. Please try again.
            </Alert>
          )}

          <TextField
            label="Email or Username"
            fullWidth
            margin="normal"
            error={!!errors.emailOrUsername}
            helperText={errors.emailOrUsername?.message}
            {...register('emailOrUsername', { required: 'Email or username is required' })}
          />

          <TextField
            label="Password"
            type={showPassword ? 'text' : 'password'}
            fullWidth
            margin="normal"
            error={!!errors.password}
            helperText={errors.password?.message}
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      aria-label={showPassword ? 'Hide password' : 'Show password'}
                      onClick={() => setShowPassword((prev) => !prev)}
                      edge="end"
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              },
            }}
            {...register('password', { required: 'Password is required' })}
          />

          <FormControlLabel
            control={<Checkbox {...register('rememberMe')} />}
            label="Remember me"
            sx={{ mt: 1 }}
          />

          <Button
            type="submit"
            variant="contained"
            fullWidth
            disabled={mutation.isPending}
            sx={{ mt: 3, py: 1.5 }}
          >
            {mutation.isPending ? <CircularProgress size={24} color="inherit" /> : 'Sign in'}
          </Button>
        </Box>
      </Grid>
    </Grid>
  );
}
