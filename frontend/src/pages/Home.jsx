import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { FiArrowRight, FiTrendingUp, FiShield, FiTruck, FiHeadphones } from 'react-icons/fi';
import { motion } from 'framer-motion';
import { getProducts, getCategories, toggleWishlist, getWishlist } from '../services/api';
import { useAuth } from '../context/AuthContext';
import ProductCard from '../components/ProductCard';

export default function Home() {
    const [featured, setFeatured] = useState([]);
    const [categories, setCategories] = useState([]);
    const [wishlist, setWishlist] = useState([]);
    const { user } = useAuth();

    useEffect(() => {
        getProducts({ featured: true, pageSize: 8 }).then(r => setFeatured(r.data?.items ?? r.data?.products ?? [])).catch(() => { });
        getCategories().then(r => setCategories(r.data?.items ?? r.data ?? [])).catch(() => { });
        if (user) getWishlist().then(r => {
            const list = r.data?.items ?? r.data ?? [];
            setWishlist((Array.isArray(list) ? list : []).map(w => w.productId));
        }).catch(() => { });
    }, [user]);

    const handleWishlist = async (productId) => {
        if (!user) return;
        await toggleWishlist(productId);
        setWishlist(prev => prev.includes(productId) ? prev.filter(id => id !== productId) : [...prev, productId]);
    };

    const features = [
        { icon: <FiTruck />, title: 'Free Shipping', desc: 'On orders over $50' },
        { icon: <FiShield />, title: 'Secure Payment', desc: 'SSL encrypted checkout' },
        { icon: <FiHeadphones />, title: '24/7 Support', desc: 'Expert assistance' },
        { icon: <FiTrendingUp />, title: 'Best Prices', desc: 'Guaranteed best deals' }
    ];

    return (
        <div>
            <div className="container">
                <motion.div className="hero" initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ duration: 0.6 }}>
                    <div className="hero-content">
                        <span className="hero-badge">🔥 New Collection 2026</span>
                        <h1>Discover <span>Premium</span> Products at Best Prices</h1>
                        <p>Explore our curated collection of premium products. From electronics to fashion, find everything you need with unbeatable quality.</p>
                        <div className="hero-buttons">
                            <Link to="/products" className="btn btn-primary btn-lg">
                                Shop Now <FiArrowRight />
                            </Link>
                            <Link to="/products?featured=true" className="btn btn-secondary btn-lg">
                                View Featured
                            </Link>
                        </div>
                    </div>
                </motion.div>

                {/* Features */}
                <div className="section">
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))', gap: '20px' }}>
                        {features.map((f, i) => (
                            <motion.div key={i} className="card" style={{ padding: '24px', display: 'flex', alignItems: 'center', gap: '16px' }}
                                initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.1 }}>
                                <div style={{ width: 48, height: 48, borderRadius: 12, background: 'var(--accent-gradient)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '1.2rem', color: 'white' }}>
                                    {f.icon}
                                </div>
                                <div>
                                    <div style={{ fontWeight: 700, fontSize: '0.95rem' }}>{f.title}</div>
                                    <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{f.desc}</div>
                                </div>
                            </motion.div>
                        ))}
                    </div>
                </div>

                {/* Categories */}
                {categories.length > 0 && (
                    <div className="section">
                        <div className="section-header">
                            <h2 className="section-title">Shop by Category</h2>
                            <Link to="/products" className="section-link">View All <FiArrowRight /></Link>
                        </div>
                        <div className="categories-grid">
                            {categories.map(cat => (
                                <Link key={cat.id} to={`/products?categoryId=${cat.id}`} className="category-card">
                                    <img src={cat.imageUrl} alt={cat.name} loading="lazy" />
                                    <div className="overlay">
                                        <div className="cat-name">{cat.name}</div>
                                        <div className="cat-count">{cat.productCount} products</div>
                                    </div>
                                </Link>
                            ))}
                        </div>
                    </div>
                )}

                {/* Featured Products */}
                {featured.length > 0 && (
                    <div className="section">
                        <div className="section-header">
                            <h2 className="section-title">Featured Products</h2>
                            <Link to="/products?featured=true" className="section-link">View All <FiArrowRight /></Link>
                        </div>
                        <div className="products-grid">
                            {featured.map(p => (
                                <ProductCard key={p.id} product={p} wishlisted={wishlist.includes(p.id)} onToggleWishlist={handleWishlist} />
                            ))}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}
