import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.ts'],
    server: {
      deps: {
        inline: ['react-transition-group', '@mui/material', '@mui/icons-material'],
      },
    },
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      thresholds: {
        lines: 80,
        branches: 80,
      },
      exclude: [
        'node_modules/',
        '**/*.config.*',
        'src/main.tsx',
        'src/setupTests.ts',
        '**/*.d.ts',
        'src/App.tsx',
        'src/api/**',
        'src/pages/Customers/**',
        'src/pages/ForgotPassword/**',
        'src/pages/Accounts/**',
      ],
    },
  },
});
