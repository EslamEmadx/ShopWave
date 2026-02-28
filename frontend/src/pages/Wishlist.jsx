import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getWishlist, removeWishlistItem } from '../services/api';
import { useCart } from '../context/CartContext';
import { FiHeart, FiTrash2, FiShoppingCart } from 'react-icons/fi';
import { motion, AnimatePresence } from 'framer-motion';
import toast from 'react-hot-toast';

export default function Wishlist() {
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const { addToCart } = useCart();

    useEffect(() => {
        getWishlist().then(r => {
            setItems(r.data?.items ?? r.data ?? []);
        }).catch(() => { }).finally(() => setLoading(false));
    }, []);

    const handleRemove = async (id) => {
        try {
            await removeWishlistItem(id);
            setItems(items.filter(i => i.id !== id));
            toast.success('Removed from wishlist');
        } catch (e) { toast.error('Failed to remove'); }
    };

    const moveToCart = async (item) => {
        try {
            await addToCart(item.productId);
            await removeWishlistItem(item.id);
            setItems(items.filter(i => i.id !== item.id));
            toast.success('Moved to cart');
        } catch (e) { toast.error('Failed to move to cart'); }
    };

    if (loading) return <div className="page container"><div className="spinner-container"><div className="spinner" /></div></div>;

    const wishlistItems = Array.isArray(items) ? items : [];

    return (
        <div className="page container">
            <div className="page-header">
                <h1 className="page-title">My Wishlist</h1>
                <p className="page-subtitle">{wishlistItems.length} item{wishlistItems.length !== 1 ? 's' : ''} saved</p>
            </div>

            {wishlistItems.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon"><FiHeart /></div>
                    <h3>Your wishlist is empty</h3>
                    <p>Save items you love to buy later</p>
                    <Link to="/products" className="btn btn-primary">Browse Products</Link>
                </div>
            ) : (
                <div className="products-grid">
                    <AnimatePresence>
                        {wishlistItems.map(item => (
                            <motion.div key={item.id} className="card" style={{ overflow: 'hidden' }} exit={{ opacity: 0, scale: 0.8 }}>
                                <Link to={`/products/${item.productId}`}>
                                    <div style={{ aspectRatio: '1', overflow: 'hidden', background: 'var(--bg-secondary)' }}>
                                        <img src={item.productImage} alt={item.productName} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                                    </div>
                                </Link>
                                <div style={{ padding: 16 }}>
                                    <h3 style={{ fontWeight: 600, fontSize: '1rem', marginBottom: 4 }}>{item.productName}</h3>
                                    <div style={{ color: 'var(--accent-primary)', fontWeight: 700, fontSize: '1.1rem', marginBottom: 12 }}>
                                        ${(item.price || 0).toFixed(2)}
                                    </div>
                                    <div style={{ display: 'flex', gap: 8 }}>
                                        <button className="btn btn-primary btn-sm" style={{ flex: 1 }} onClick={() => moveToCart(item)}>
                                            <FiShoppingCart /> Add to Cart
                                        </button>
                                        <button className="btn btn-danger btn-sm" onClick={() => handleRemove(item.id)}><FiTrash2 /></button>
                                    </div>
                                </div>
                            </motion.div>
                        ))}
                    </AnimatePresence>
                </div>
            )}
        </div>
    );
}
