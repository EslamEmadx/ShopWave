import { createContext, useContext, useState, useEffect } from 'react';
import { getCart as fetchCart, addToCart as apiAddToCart, updateCartItem, removeCartItem, clearCart as apiClearCart } from '../services/api';
import { useAuth } from './AuthContext';
import toast from 'react-hot-toast';

const CartContext = createContext(null);

export function CartProvider({ children }) {
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(false);
    const { user } = useAuth();

    useEffect(() => {
        if (user) loadCart();
        else setItems([]);
    }, [user]);

    const loadCart = async () => {
        try {
            setLoading(true);
            const { data } = await fetchCart();
            setItems(data);
        } catch (e) { /* silent */ }
        finally { setLoading(false); }
    };

    const addToCart = async (productId, quantity = 1) => {
        if (!user) { toast.error('Please login first'); return; }
        try {
            await apiAddToCart({ productId, quantity });
            await loadCart();
            toast.success('Added to cart!');
        } catch (e) {
            toast.error(e.response?.data?.message || 'Failed to add');
        }
    };

    const updateQuantity = async (id, quantity) => {
        try {
            await updateCartItem(id, { quantity });
            await loadCart();
        } catch (e) { toast.error('Failed to update'); }
    };

    const removeItem = async (id) => {
        try {
            await removeCartItem(id);
            setItems(items.filter(i => i.id !== id));
            toast.success('Removed from cart');
        } catch (e) { toast.error('Failed to remove'); }
    };

    const clearAll = async () => {
        try {
            await apiClearCart();
            setItems([]);
        } catch (e) { /* silent */ }
    };

    const total = items.reduce((sum, item) => sum + item.price * item.quantity, 0);
    const count = items.reduce((sum, item) => sum + item.quantity, 0);

    return (
        <CartContext.Provider value={{ items, loading, addToCart, updateQuantity, removeItem, clearAll, total, count, loadCart }}>
            {children}
        </CartContext.Provider>
    );
}

export const useCart = () => useContext(CartContext);
