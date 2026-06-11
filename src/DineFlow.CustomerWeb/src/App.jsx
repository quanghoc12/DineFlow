import { Route, Routes } from 'react-router-dom';
import QrLandingPage from './pages/QrLandingPage.jsx';
import MenuPage from './pages/MenuPage.jsx';
import CartPage from './pages/CartPage.jsx';
import ServiceRequestPage from './pages/ServiceRequestPage.jsx';
import PaymentRequestPage from './pages/PaymentRequestPage.jsx';
import NotFoundPage from './pages/NotFoundPage.jsx';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<QrLandingPage />} />
      <Route path="/menu" element={<MenuPage />} />
      <Route path="/cart" element={<CartPage />} />
      <Route path="/call-staff" element={<ServiceRequestPage />} />
      <Route path="/payment-request" element={<PaymentRequestPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
