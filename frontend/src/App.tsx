import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { LandingPage } from './pages/LandingPage';
import { PlansPage } from './pages/PlansPage';
import { CheckoutPage } from './pages/CheckoutPage';
import { FailedPage, PaymentResultPage, SuccessPage } from './pages/ResultPages';
import { LoginPage } from './pages/LoginPage';
import { AdminOverviewPage } from './pages/AdminOverviewPage';
import { AdminProgramsPage } from './pages/AdminProgramsPage';
import { CreateProgramPage } from './pages/CreateProgramPage';
import { AdminPatientsPage } from './pages/AdminPatientsPage';
import { PatientPortalPage } from './pages/PatientPortalPage';
import { BenefitRedemptionPage } from './pages/BenefitRedemptionPage';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LandingPage />} />
        <Route path="/plans" element={<PlansPage />} />
        <Route path="/checkout" element={<CheckoutPage />} />
        <Route path="/checkout/success/:id" element={<SuccessPage />} />
        <Route path="/checkout/failed/:id?" element={<FailedPage />} />
        <Route path="/checkout/result" element={<PaymentResultPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/portal" element={<PatientPortalPage />} />
        <Route path="/portal/redeem" element={<BenefitRedemptionPage />} />
        <Route path="/admin" element={<AdminOverviewPage />} />
        <Route path="/admin/programs" element={<AdminProgramsPage />} />
        <Route path="/admin/programs/new" element={<CreateProgramPage />} />
        <Route path="/admin/patients" element={<AdminPatientsPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
