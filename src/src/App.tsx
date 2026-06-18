import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { LoginPage } from './pages/Login/LoginPage';
import { CustomersPage } from './pages/Customers/CustomersPage';
import { ForgotPasswordPage } from './pages/ForgotPassword/ForgotPasswordPage';
import { LeadListPage } from './pages/Leads/LeadListPage';
import { LeadFormPage } from './pages/Leads/LeadFormPage';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AppLayout } from './components/Layout/AppLayout';
import { AccountsPage } from './pages/Accounts/AccountsPage';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="/customers" element={<CustomersPage />} />
            <Route path="/leads" element={<LeadListPage />} />
            <Route path="/leads/new" element={<LeadFormPage />} />
            <Route path="/leads/:id/edit" element={<LeadFormPage />} />
            <Route path="/accounts" element={<AccountsPage />} />
          </Route>
        </Route>
        <Route path="/" element={<Navigate to="/leads" replace />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
