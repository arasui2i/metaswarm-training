import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';

const mockMutate = vi.fn();
let mockIsPending = false;
let mockIsError = false;

vi.mock('../hooks/useLogin', () => ({
  useLogin: () => ({
    mutate: mockMutate,
    isPending: mockIsPending,
    isError: mockIsError,
  }),
}));

import { LoginPage } from '../pages/Login/LoginPage';

describe('LoginPage', () => {
  beforeEach(() => {
    mockMutate.mockClear();
    mockIsPending = false;
    mockIsError = false;
  });

  it('renders email/username, password fields and sign-in button', () => {
    render(<LoginPage />);
    expect(screen.getByLabelText(/email or username/i)).toBeInTheDocument();
    expect(screen.getByLabelText('Password')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('renders remember me checkbox', () => {
    render(<LoginPage />);
    expect(screen.getByRole('checkbox')).toBeInTheDocument();
    expect(screen.getByLabelText(/remember me/i)).toBeInTheDocument();
  });

  it('password field starts as type="password"', () => {
    render(<LoginPage />);
    expect(screen.getByLabelText('Password')).toHaveAttribute('type', 'password');
  });

  it('toggles password to visible and back on eye-icon click', async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    const passwordInput = screen.getByLabelText('Password');
    const toggleBtn = screen.getByRole('button', { name: /show password/i });

    await user.click(toggleBtn);
    expect(passwordInput).toHaveAttribute('type', 'text');

    await user.click(screen.getByRole('button', { name: /hide password/i }));
    expect(passwordInput).toHaveAttribute('type', 'password');
  });

  it('calls mutate with entered credentials on submit', async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText(/email or username/i), 'admin@crm.com');
    await user.type(screen.getByLabelText('Password'), 'secret123');
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    expect(mockMutate).toHaveBeenCalledWith({
      emailOrUsername: 'admin@crm.com',
      password: 'secret123',
      rememberMe: false,
    });
  });

  it('includes rememberMe:true when checkbox is checked', async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText(/email or username/i), 'u');
    await user.type(screen.getByLabelText('Password'), 'pass123');
    await user.click(screen.getByRole('checkbox'));
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    expect(mockMutate).toHaveBeenCalledWith(
      expect.objectContaining({ rememberMe: true }),
    );
  });

  it('shows error alert with correct message when mutation fails', () => {
    mockIsError = true;
    render(<LoginPage />);

    expect(screen.getByRole('alert')).toBeInTheDocument();
    expect(screen.getByText(/invalid credentials\. please try again\./i)).toBeInTheDocument();
  });

  it('disables the submit button when mutation is pending', () => {
    mockIsPending = true;
    render(<LoginPage />);

    const submitBtn = document.querySelector<HTMLButtonElement>('button[type="submit"]');
    expect(submitBtn).toBeDisabled();
  });

  it('does not call mutate when required fields are empty', async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.click(screen.getByRole('button', { name: /sign in/i }));

    expect(mockMutate).not.toHaveBeenCalled();
  });
});
