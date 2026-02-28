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
        if (user) {
            loadCart();
            mergeCart();
        } else {
            const guest = JSON.parse(localStorage.getItem('guestCart') || '[]');
            setItems(guest);
        }
    }, [user]);

    const mergeCart = async () => {
        const guest = JSON.parse(localStorage.getItem('guestCart') || '[]');
        if (guest.length > 0) {
            try {
                for (const item of guest) {
                    await apiAddToCart({ productId: item.productId, quantity: item.quantity });
                }
                localStorage.removeItem('guestCart');
                await loadCart();
                toast.success('Your guest cart has been merged!');
            } catch (e) { console.error("Merge failed", e); }
        }
    };

    const loadCart = async () => {
        try {
            setLoading(true);
            const { data } = await fetchCart();
            setItems(data?.items ?? data ?? []);
        } catch (e) { console.error("Cart load failed", e); }
        finally { setLoading(false); }
    };

    const addToCart = async (product, quantity = 1) => {
        const productId = typeof product === 'object' ? product.id : product;
        if (!user) {
            const guest = JSON.parse(localStorage.getItem('guestCart') || '[]');
            const existing = guest.find(i => i.productId === productId);
            if (existing) {
                existing.quantity += quantity;
            } else {
                // For guest cart, we store enough info to render it
                const info = typeof product === 'object' ? product : { id: productId, name: 'Product', price: 0, imageUrl: '' };
                guest.push({
                    id: `guest-${Date.now()}`,
                    productId,
                    quantity,
                    productName: info.name,
                    price: info.price,
                    productImage: info.imageUrl,
                    stock: info.stock || 99
                });
            }
            localStorage.setItem('guestCart', JSON.stringify(guest));
            setItems([...guest]);
            toast.success('Added to guest cart');
            return;
        }

        try {
            await apiAddToCart({ productId, quantity });
            await loadCart();
            toast.success('Added to cart!');
        } catch (e) {
            toast.error(e.response?.data?.message || 'Failed to add');
        }
    };

    const updateQuantity = async (id, quantity) => {
        if (!user) {
            const guest = JSON.parse(localStorage.getItem('guestCart') || '[]');
            const item = guest.find(i => i.id === id);
            if (item) {
                item.quantity = quantity;
                localStorage.setItem('guestCart', JSON.stringify(guest));
                setItems([...guest]);
            }
            return;
        }
        try {
            await updateCartItem(id, { quantity });
            await loadCart();
        } catch (e) { toast.error('Failed to update'); }
    };

    const removeItem = async (id) => {
        if (!user) {
            const guest = JSON.parse(localStorage.getItem('guestCart') || '[]');
            const filtered = guest.filter(i => i.id !== id);
            localStorage.setItem('guestCart', JSON.stringify(filtered));
            setItems(filtered);
            toast.success('Removed from guest cart');
            return;
        }
        try {
            await removeCartItem(id);
            setItems(items.filter(i => i.id !== id));
            toast.success('Removed from cart');
        } catch (e) { toast.error('Failed to remove'); }
    };

    const clearAll = async () => {
        if (!user) {
            localStorage.removeItem('guestCart');
            setItems([]);
            return;
        }
        try {
            await apiClearCart();
            setItems([]);
        } catch (e) { /* silent */ }
    };

    const safeItems = Array.isArray(items) ? items : [];
    const total = safeItems.reduce((sum, item) => sum + (item.price || 0) * (item.quantity || 0), 0);
    const count = safeItems.reduce((sum, item) => sum + (item.quantity || 0), 0);

    return (
        <CartContext.Provider value={{ items, loading, addToCart, updateQuantity, removeItem, clearAll, total, count, loadCart }}>
            {children}
        </CartContext.Provider>
    );
}

export const useCart = () => useContext(CartContext);
