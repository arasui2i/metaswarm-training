import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';

let mockParams: { id?: string } = { id: undefined };
let mockLeadDetail: object | undefined = undefined;
const mockCreateMutateAsync = vi.fn();
const mockUpdateMutateAsync = vi.fn();
const mockNavigate = vi.fn();

vi.mock('../hooks/useLeads', () => ({
  useLeads: () => ({ data: undefined, isLoading: false }),
  useLeadDetail: () => ({ data: mockLeadDetail, isLoading: false }),
  useCreateLead: () => ({ mutateAsync: mockCreateMutateAsync, isPending: false }),
  useUpdateLead: () => ({ mutateAsync: mockUpdateMutateAsync, isPending: false }),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
    useParams: () => mockParams,
  };
});

import { LeadFormPage } from '../pages/Leads/LeadFormPage';
import { MemoryRouter } from 'react-router-dom';

function renderForm() {
  return render(<MemoryRouter><LeadFormPage /></MemoryRouter>);
}

describe('LeadFormPage', () => {
  beforeEach(() => {
    mockParams = { id: undefined };
    mockLeadDetail = undefined;
    mockCreateMutateAsync.mockClear().mockResolvedValue('new-id');
    mockUpdateMutateAsync.mockClear().mockResolvedValue(undefined);
    mockNavigate.mockClear();
  });

  it('renders Add Lead heading in create mode', () => {
    renderForm();
    expect(screen.getByText('Add Lead')).toBeInTheDocument();
  });

  it('renders Edit Lead heading in edit mode', () => {
    mockParams = { id: 'lead-123' };
    renderForm();
    expect(screen.getByText('Edit Lead')).toBeInTheDocument();
  });

  it('renders First Name, Company, and Email fields', () => {
    renderForm();
    expect(screen.getByLabelText(/first name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/company/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
  });

  it('shows validation error when First Name is empty on submit', async () => {
    const user = userEvent.setup();
    renderForm();
    await user.click(screen.getByRole('button', { name: /create/i }));
    expect(await screen.findByText('First name is required')).toBeInTheDocument();
  });

  it('shows validation error when Company is empty on submit', async () => {
    const user = userEvent.setup();
    renderForm();
    await user.type(screen.getByLabelText(/first name/i), 'Alice');
    await user.click(screen.getByRole('button', { name: /create/i }));
    expect(await screen.findByText('Company is required')).toBeInTheDocument();
  });

  it('calls createMutation and navigates to /leads on valid submit', async () => {
    const user = userEvent.setup();
    renderForm();
    await user.type(screen.getByLabelText(/first name/i), 'Alice');
    await user.type(screen.getByLabelText(/company/i), 'ACME');
    await user.type(screen.getByLabelText(/email/i), 'alice@acme.com');
    await user.click(screen.getByRole('button', { name: /create/i }));
    await waitFor(() => {
      expect(mockCreateMutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({ firstName: 'Alice', companyName: 'ACME', email: 'alice@acme.com' }),
      );
    });
    expect(mockNavigate).toHaveBeenCalledWith('/leads');
  });

  it('calls updateMutation and navigates to /leads on valid edit submit', async () => {
    mockParams = { id: 'lead-123' };
    mockLeadDetail = {
      id: 'lead-123', firstName: 'Alice', lastName: null,
      companyName: 'ACME', email: 'alice@acme.com', phone: null,
      status: 'New', ownerId: null, createdAt: '2026-01-01',
    };
    const user = userEvent.setup();
    renderForm();
    await user.click(screen.getByRole('button', { name: /save/i }));
    await waitFor(() => {
      expect(mockUpdateMutateAsync).toHaveBeenCalledWith(
        expect.objectContaining({ id: 'lead-123' }),
      );
    });
    expect(mockNavigate).toHaveBeenCalledWith('/leads');
  });

  it('navigates to /leads when Cancel is clicked', async () => {
    const user = userEvent.setup();
    renderForm();
    await user.click(screen.getByRole('button', { name: /cancel/i }));
    expect(mockNavigate).toHaveBeenCalledWith('/leads');
  });
});
