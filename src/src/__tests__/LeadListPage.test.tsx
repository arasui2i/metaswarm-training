import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import type { LeadsPagedResult } from '../api/leads';

let mockLeadsData: LeadsPagedResult = { items: [], totalCount: 0, page: 1, pageSize: 10 };
let mockIsLoading = false;
const mockDeleteMutateAsync = vi.fn();
const mockNavigate = vi.fn();

vi.mock('../hooks/useLeads', () => ({
  useLeads: () => ({ data: mockLeadsData, isLoading: mockIsLoading }),
  useDeleteLead: () => ({ mutateAsync: mockDeleteMutateAsync, isPending: false }),
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom');
  return { ...actual, useNavigate: () => mockNavigate };
});

import { LeadListPage } from '../pages/Leads/LeadListPage';
import { MemoryRouter } from 'react-router-dom';

function renderList() {
  return render(<MemoryRouter><LeadListPage /></MemoryRouter>);
}

const SAMPLE_LEADS: LeadsPagedResult['items'] = [
  { id: '1', fullName: 'Alice Smith', companyName: 'ACME Corp', email: 'alice@acme.com', status: 'New' },
  { id: '2', fullName: 'Bob Jones', companyName: 'Beta Ltd', email: 'bob@beta.com', status: 'Contacted' },
];

describe('LeadListPage', () => {
  beforeEach(() => {
    mockLeadsData = { items: [], totalCount: 0, page: 1, pageSize: 10 };
    mockIsLoading = false;
    mockNavigate.mockClear();
    mockDeleteMutateAsync.mockClear();
  });

  it('renders Leads heading', () => {
    renderList();
    expect(screen.getByText('Leads')).toBeInTheDocument();
  });

  it('renders Search text field', () => {
    renderList();
    expect(screen.getByRole('textbox', { name: /search/i })).toBeInTheDocument();
  });

  it('renders Add Lead button', () => {
    renderList();
    expect(screen.getByRole('button', { name: /add lead/i })).toBeInTheDocument();
  });

  it('navigates to /leads/new when Add Lead is clicked', async () => {
    const user = userEvent.setup();
    renderList();
    await user.click(screen.getByRole('button', { name: /add lead/i }));
    expect(mockNavigate).toHaveBeenCalledWith('/leads/new');
  });

  it('shows loading indicator when isLoading is true', () => {
    mockIsLoading = true;
    renderList();
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('renders lead rows when data is present', () => {
    mockLeadsData = { items: SAMPLE_LEADS, totalCount: 2, page: 1, pageSize: 10 };
    renderList();
    expect(screen.getByText('Alice Smith')).toBeInTheDocument();
    expect(screen.getByText('ACME Corp')).toBeInTheDocument();
    expect(screen.getByText('Bob Jones')).toBeInTheDocument();
  });

  it('navigates to edit page when edit button is clicked', async () => {
    const user = userEvent.setup();
    mockLeadsData = { items: SAMPLE_LEADS, totalCount: 2, page: 1, pageSize: 10 };
    renderList();
    const allButtons = screen.getAllByRole('button');
    await user.click(allButtons[1]); // Add Lead=[0], row1 Edit=[1]
    expect(mockNavigate).toHaveBeenCalledWith('/leads/1/edit');
  });

  it('opens delete confirmation dialog when delete button is clicked', async () => {
    const user = userEvent.setup();
    mockLeadsData = { items: SAMPLE_LEADS, totalCount: 2, page: 1, pageSize: 10 };
    renderList();
    const allButtons = screen.getAllByRole('button');
    await user.click(allButtons[2]); // row1 Delete=[2]
    expect(await screen.findByRole('dialog')).toBeInTheDocument();
    expect(screen.getByText('Delete Lead')).toBeInTheDocument();
  });

  it('closes dialog when Cancel is clicked', async () => {
    const user = userEvent.setup();
    mockLeadsData = { items: SAMPLE_LEADS, totalCount: 2, page: 1, pageSize: 10 };
    renderList();
    const allButtons = screen.getAllByRole('button');
    await user.click(allButtons[2]);
    await screen.findByRole('dialog');
    await user.click(screen.getByRole('button', { name: /cancel/i }));
    await waitFor(() => {
      expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    });
  });

  it('calls deleteMutation with lead id on confirm delete', async () => {
    const user = userEvent.setup();
    mockDeleteMutateAsync.mockResolvedValue(undefined);
    mockLeadsData = { items: SAMPLE_LEADS, totalCount: 2, page: 1, pageSize: 10 };
    renderList();
    const allButtons = screen.getAllByRole('button');
    await user.click(allButtons[2]);
    await screen.findByRole('dialog');
    await user.click(screen.getByRole('button', { name: /^delete$/i }));
    expect(mockDeleteMutateAsync).toHaveBeenCalledWith('1');
  });
});
