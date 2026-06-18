import { useEffect } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box,
  Button,
  CircularProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material';
import { useCreateLead, useLeadDetail, useUpdateLead } from '../../hooks/useLeads';
import type { CreateLeadPayload, LeadStatus } from '../../api/leads';

const STATUSES: LeadStatus[] = ['New', 'Contacted', 'Qualified', 'Unqualified', 'Converted'];

interface FormValues {
  firstName: string;
  lastName: string;
  companyName: string;
  email: string;
  phone: string;
  status: LeadStatus;
}

export function LeadFormPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const { data: leadDetail, isLoading: loadingDetail } = useLeadDetail(id ?? '');
  const createMutation = useCreateLead();
  const updateMutation = useUpdateLead();

  const { control, handleSubmit, reset, formState: { errors } } = useForm<FormValues>({
    defaultValues: {
      firstName: '',
      lastName: '',
      companyName: '',
      email: '',
      phone: '',
      status: 'New',
    },
  });

  useEffect(() => {
    if (leadDetail) {
      reset({
        firstName: leadDetail.firstName,
        lastName: leadDetail.lastName ?? '',
        companyName: leadDetail.companyName,
        email: leadDetail.email,
        phone: leadDetail.phone ?? '',
        status: leadDetail.status,
      });
    }
  }, [leadDetail, reset]);

  const onSubmit = async (values: FormValues) => {
    const payload: CreateLeadPayload = {
      firstName: values.firstName,
      lastName: values.lastName || undefined,
      companyName: values.companyName,
      email: values.email,
      phone: values.phone || undefined,
      status: values.status,
    };
    if (isEdit && id) {
      await updateMutation.mutateAsync({ id, payload: { ...payload, id } });
    } else {
      await createMutation.mutateAsync(payload);
    }
    navigate('/leads');
  };

  if (isEdit && loadingDetail) return <CircularProgress />;

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ maxWidth: 600 }}>
      <Typography variant="h5" sx={{ mb: 3 }}>{isEdit ? 'Edit Lead' : 'Add Lead'}</Typography>
      <Controller
        name="firstName"
        control={control}
        rules={{ required: 'First name is required' }}
        render={({ field }) => (
          <TextField
            {...field}
            label="First Name"
            fullWidth
            sx={{ mb: 2 }}
            error={!!errors.firstName}
            helperText={errors.firstName?.message}
          />
        )}
      />
      <Controller
        name="lastName"
        control={control}
        render={({ field }) => (
          <TextField {...field} label="Last Name" fullWidth sx={{ mb: 2 }} />
        )}
      />
      <Controller
        name="companyName"
        control={control}
        rules={{ required: 'Company is required' }}
        render={({ field }) => (
          <TextField
            {...field}
            label="Company"
            fullWidth
            sx={{ mb: 2 }}
            error={!!errors.companyName}
            helperText={errors.companyName?.message}
          />
        )}
      />
      <Controller
        name="email"
        control={control}
        rules={{
          required: 'Email is required',
          pattern: { value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/, message: 'Invalid email' },
        }}
        render={({ field }) => (
          <TextField
            {...field}
            label="Email"
            type="email"
            fullWidth
            sx={{ mb: 2 }}
            error={!!errors.email}
            helperText={errors.email?.message}
          />
        )}
      />
      <Controller
        name="phone"
        control={control}
        render={({ field }) => (
          <TextField {...field} label="Phone" fullWidth sx={{ mb: 2 }} />
        )}
      />
      <Controller
        name="status"
        control={control}
        render={({ field }) => (
          <FormControl fullWidth sx={{ mb: 3 }}>
            <InputLabel>Status</InputLabel>
            <Select {...field} label="Status">
              {STATUSES.map((s) => (
                <MenuItem key={s} value={s}>{s}</MenuItem>
              ))}
            </Select>
          </FormControl>
        )}
      />
      <Box sx={{ display: 'flex', gap: 2 }}>
        <Button
          type="submit"
          variant="contained"
          disabled={createMutation.isPending || updateMutation.isPending}
        >
          {isEdit ? 'Save' : 'Create'}
        </Button>
        <Button variant="outlined" onClick={() => navigate('/leads')}>Cancel</Button>
      </Box>
    </Box>
  );
}
