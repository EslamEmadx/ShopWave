import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getWishlist, removeWishlistItem, toggleWishlist } from '../services/api';
import { useCart } from '../context/CartContext';
import { FiHeart, FiTrash2, FiShoppingCart } from 'react-icons/fi';
import { motion, AnimatePresence } from 'framer-motion';
import toast from 'react-hot-toast';

export default function Wishlist() {
    const [items, setItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const { addToCart } = useCart();

    useEffect(() => {
        getWishlist().then(r => setItems(r.data)).catch(() => { }).finally(() => setLoading(false));
    }, []);

    const handleRemove = async (id) => {
        await removeWishlistItem(id);
        setItems(items.filter(i => i.id !== id));
        toast.success('Removed from wishlist');
    };

    const moveToCart = async (item) => {
        await addToCart(item.productId);
        await removeWishlistItem(item.id);
        setItems(items.filter(i => i.id !== item.id));
    };

    if (loading) return <div className="page container"><div className="spinner-container"><div className="spinner" /></div></div>;

    return (
        <div className="page container">
            <div className="page-header"><h1 className="page-title">My Wishlist</h1><p className="page-subtitle">{items.length} item{items.length !== 1 ? 's' : ''} saved</p></div>

            {items.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon"><FiHeart /></div>
                    <h3>Your wishlist is empty</h3>
                    <p>Save items you love to buy later</p>
                    <Link to="/products" className="btn btn-primary">Browse Products</Link>
                </div>
            ) : (
                <div className="products-grid">
                    <AnimatePresence>
                        {items.map(item => (
                            <motion.div key={item.id} className="card" style={{ overflow: 'hidden' }} exit={{ opacity: 0, scale: 0.8 }}>
                                <Link to={`/products/${item.productId}`}>
                                    <div style={{ aspectRatio: '1', overflow: 'hidden', background: 'var(--bg-secondary)' }}>
                                        <img src={item.productImage} alt={item.productName} style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
                                    </div>
                                </Link>
                                <div style={{ padding: 16 }}>
                                    <h3 style={{ fontWeight: 600, fontSize: '1rem', marginBottom: 4 }}>{item.productName}</h3>
                                    <div style={{ color: 'var(--accent-primary)', fontWeight: 700, fontSize: '1.1rem', marginBottom: 12 }}>${item.price.toFixed(2)}</div>
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
