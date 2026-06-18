import { client } from './client';

export type LeadStatus = 'New' | 'Contacted' | 'Qualified' | 'Unqualified' | 'Converted';

export interface LeadSummary {
  id: string;
  fullName: string;
  companyName: string;
  email: string;
  status: LeadStatus;
}

export interface LeadDetail {
  id: string;
  firstName: string;
  lastName: string | null;
  companyName: string;
  email: string;
  phone: string | null;
  status: LeadStatus;
  ownerId: string | null;
  createdAt: string;
}

export interface CreateLeadPayload {
  firstName: string;
  lastName?: string;
  companyName: string;
  email: string;
  phone?: string;
  status?: LeadStatus;
  ownerId?: string;
}

export interface UpdateLeadPayload extends CreateLeadPayload {
  id: string;
}

export interface LeadsPagedResult {
  items: LeadSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export async function getLeads(params?: { search?: string; page?: number; pageSize?: number }): Promise<LeadsPagedResult> {
  const { data } = await client.get<LeadsPagedResult>('/api/leads', { params });
  return data;
}

export async function getLeadById(id: string): Promise<LeadDetail> {
  const { data } = await client.get<LeadDetail>(`/api/leads/${id}`);
  return data;
}

export async function createLead(payload: CreateLeadPayload): Promise<string> {
  const { data } = await client.post<string>('/api/leads', payload);
  return data;
}

export async function updateLead(id: string, payload: UpdateLeadPayload): Promise<void> {
  await client.put(`/api/leads/${id}`, payload);
}

export async function deleteLead(id: string): Promise<void> {
  await client.delete(`/api/leads/${id}`);
}
