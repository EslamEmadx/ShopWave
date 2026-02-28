import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import { ThemeProvider } from './context/ThemeContext';
import { Toaster } from 'react-hot-toast';
import AppRoutes from './AppRoutes';
import ErrorBoundary from './components/ErrorBoundary';
import './index.css';

export default function App() {
    return (
        <BrowserRouter>
            <ThemeProvider>
                <AuthProvider>
                    <CartProvider>
                        <AppRoutes />
                        <Toaster position="top-right" toastOptions={{
                            style: { background: 'var(--bg-card)', color: 'var(--text-primary)', border: '1px solid var(--border-color)' }
                        }} />
                    </CartProvider>
                </AuthProvider>
            </ThemeProvider>
        </BrowserRouter>
    );
}
