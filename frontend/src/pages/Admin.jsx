import { useState, useEffect } from 'react';
import { getDashboard, getOrders, updateOrderStatus, getProducts, createProduct, deleteProduct, getCoupons, createCoupon, deleteCoupon, getCategories } from '../services/api';
import { FiDollarSign, FiShoppingBag, FiUsers, FiPackage, FiPlus, FiTrash2, FiEdit } from 'react-icons/fi';
import { motion } from 'framer-motion';
import toast from 'react-hot-toast';

export default function Admin() {
    const [tab, setTab] = useState('dashboard');
    const [dashboard, setDashboard] = useState(null);
    const [orders, setOrders] = useState([]);
    const [products, setProducts] = useState([]);
    const [coupons, setCoupons] = useState([]);
    const [categories, setCategories] = useState([]);
    const [showProductModal, setShowProductModal] = useState(false);
    const [showCouponModal, setShowCouponModal] = useState(false);
    const [loading, setLoading] = useState(true);

    const [productForm, setProductForm] = useState({ name: '', description: '', price: '', oldPrice: '', imageUrl: '', stock: '', categoryId: '', isFeatured: false });
    const [couponForm, setCouponForm] = useState({ code: '', discountPercent: '', maxDiscount: '', minOrderAmount: '', usageLimit: '100', expiresAt: '' });

    useEffect(() => {
        loadData();
        getCategories().then(r => setCategories(r.data)).catch(() => { });
    }, []);

    const loadData = async () => {
        setLoading(true);
        try {
            const [d, o, p, c] = await Promise.all([getDashboard(), getOrders(), getProducts({ pageSize: 100 }), getCoupons()]);
            setDashboard(d.data);
            setOrders(o.data);
            setProducts(p.data.products);
            setCoupons(c.data);
        } catch (e) { /* silent */ }
        finally { setLoading(false); }
    };

    const handleStatusUpdate = async (orderId, status) => {
        try {
            await updateOrderStatus(orderId, { status });
            setOrders(orders.map(o => o.id === orderId ? { ...o, status } : o));
            toast.success('Status updated');
        } catch (e) { toast.error('Failed'); }
    };

    const handleCreateProduct = async (e) => {
        e.preventDefault();
        try {
            await createProduct({ ...productForm, price: parseFloat(productForm.price), oldPrice: productForm.oldPrice ? parseFloat(productForm.oldPrice) : null, stock: parseInt(productForm.stock), categoryId: parseInt(productForm.categoryId) });
            toast.success('Product created');
            setShowProductModal(false);
            loadData();
        } catch (e) { toast.error('Failed'); }
    };

    const handleDeleteProduct = async (id) => {
        if (!confirm('Delete this product?')) return;
        try { await deleteProduct(id); loadData(); toast.success('Deleted'); } catch (e) { toast.error('Failed'); }
    };

    const handleCreateCoupon = async (e) => {
        e.preventDefault();
        try {
            await createCoupon({ ...couponForm, discountPercent: parseInt(couponForm.discountPercent), maxDiscount: couponForm.maxDiscount ? parseFloat(couponForm.maxDiscount) : null, minOrderAmount: couponForm.minOrderAmount ? parseFloat(couponForm.minOrderAmount) : null, usageLimit: parseInt(couponForm.usageLimit), expiresAt: couponForm.expiresAt || null });
            toast.success('Coupon created');
            setShowCouponModal(false);
            loadData();
        } catch (e) { toast.error('Failed'); }
    };

    const handleDeleteCoupon = async (id) => {
        try { await deleteCoupon(id); loadData(); toast.success('Deleted'); } catch (e) { toast.error('Failed'); }
    };

    if (loading) return <div className="page container"><div className="spinner-container"><div className="spinner" /></div></div>;

    const statCards = dashboard ? [
        { icon: <FiDollarSign />, label: 'Total Revenue', value: `$${dashboard.totalRevenue.toFixed(2)}`, bg: 'rgba(108,99,255,0.15)', color: 'var(--accent-primary)' },
        { icon: <FiShoppingBag />, label: 'Total Orders', value: dashboard.totalOrders, bg: 'rgba(255,101,132,0.15)', color: 'var(--accent-secondary)' },
        { icon: <FiUsers />, label: 'Total Users', value: dashboard.totalUsers, bg: 'rgba(46,204,113,0.15)', color: 'var(--success)' },
        { icon: <FiPackage />, label: 'Total Products', value: dashboard.totalProducts, bg: 'rgba(243,156,18,0.15)', color: 'var(--warning)' }
    ] : [];

    return (
        <div className="page container">
            <div className="page-header"><h1 className="page-title">Admin Dashboard</h1></div>

            <div className="tabs">
                {['dashboard', 'orders', 'products', 'coupons'].map(t => (
                    <button key={t} className={`tab ${tab === t ? 'active' : ''}`} onClick={() => setTab(t)}>
                        {t.charAt(0).toUpperCase() + t.slice(1)}
                    </button>
                ))}
            </div>

            {/* Dashboard Tab */}
            {tab === 'dashboard' && dashboard && (
                <>
                    <div className="stats-grid">
                        {statCards.map((s, i) => (
                            <motion.div key={i} className="stat-card" initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: i * 0.1 }}>
                                <div className="stat-icon" style={{ background: s.bg, color: s.color }}>{s.icon}</div>
                                <div className="stat-value">{s.value}</div>
                                <div className="stat-label">{s.label}</div>
                            </motion.div>
                        ))}
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
                        <div className="card" style={{ padding: 24 }}>
                            <h3 style={{ marginBottom: 16 }}>Recent Orders</h3>
                            {dashboard.recentOrders?.map(o => (
                                <div key={o.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '10px 0', borderBottom: '1px solid var(--border-color)' }}>
                                    <div>
                                        <div style={{ fontWeight: 600 }}>#{o.id} — {o.username}</div>
                                        <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>{new Date(o.date).toLocaleDateString()}</div>
                                    </div>
                                    <div style={{ textAlign: 'right' }}>
                                        <div style={{ fontWeight: 600 }}>${o.total.toFixed(2)}</div>
                                        <span className={`order-status status-${o.status.toLowerCase()}`} style={{ fontSize: '0.7rem' }}>{o.status}</span>
                                    </div>
                                </div>
                            ))}
                        </div>
                        <div className="card" style={{ padding: 24 }}>
                            <h3 style={{ marginBottom: 16 }}>Top Products</h3>
                            {dashboard.topProducts?.map(p => (
                                <div key={p.id} style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '10px 0', borderBottom: '1px solid var(--border-color)' }}>
                                    <img src={p.imageUrl} alt={p.name} style={{ width: 40, height: 40, borderRadius: 8, objectFit: 'cover' }} />
                                    <div style={{ flex: 1 }}>
                                        <div style={{ fontWeight: 600, fontSize: '0.9rem' }}>{p.name}</div>
                                        <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>{p.totalSold} sold</div>
                                    </div>
                                    <div style={{ fontWeight: 600, color: 'var(--accent-primary)' }}>${p.revenue.toFixed(2)}</div>
                                </div>
                            ))}
                            {(!dashboard.topProducts || dashboard.topProducts.length === 0) && <p style={{ color: 'var(--text-muted)' }}>No sales data yet</p>}
                        </div>
                    </div>
                </>
            )}

            {/* Orders Tab */}
            {tab === 'orders' && (
                <table className="admin-table">
                    <thead><tr><th>ID</th><th>Date</th><th>Total</th><th>Status</th><th>Payment</th><th>Action</th></tr></thead>
                    <tbody>
                        {orders.map(o => (
                            <tr key={o.id}>
                                <td>#{o.id}</td>
                                <td>{new Date(o.createdAt).toLocaleDateString()}</td>
                                <td style={{ fontWeight: 600 }}>${o.totalAmount.toFixed(2)}</td>
                                <td><span className={`order-status status-${o.status.toLowerCase()}`}>{o.status}</span></td>
                                <td>{o.paymentStatus}</td>
                                <td>
                                    <select className="filter-select" style={{ padding: '6px 10px', fontSize: '0.8rem' }} value={o.status} onChange={e => handleStatusUpdate(o.id, e.target.value)}>
                                        {['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'].map(s => <option key={s} value={s}>{s}</option>)}
                                    </select>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            {/* Products Tab */}
            {tab === 'products' && (
                <>
                    <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'flex-end' }}>
                        <button className="btn btn-primary" onClick={() => setShowProductModal(true)}><FiPlus /> Add Product</button>
                    </div>
                    <table className="admin-table">
                        <thead><tr><th>Image</th><th>Name</th><th>Price</th><th>Stock</th><th>Category</th><th>Action</th></tr></thead>
                        <tbody>
                            {products.map(p => (
                                <tr key={p.id}>
                                    <td><img src={p.imageUrl} alt="" style={{ width: 40, height: 40, borderRadius: 6, objectFit: 'cover' }} /></td>
                                    <td style={{ fontWeight: 600 }}>{p.name}</td>
                                    <td>${p.price.toFixed(2)}</td>
                                    <td>{p.stock}</td>
                                    <td>{p.categoryName}</td>
                                    <td><button className="btn btn-danger btn-sm" onClick={() => handleDeleteProduct(p.id)}><FiTrash2 /></button></td>
                                </tr>
                            ))}
                        </tbody>
                    </table>

                    {showProductModal && (
                        <div className="modal-overlay" onClick={() => setShowProductModal(false)}>
                            <div className="modal-content" onClick={e => e.stopPropagation()}>
                                <h2>Add Product</h2>
                                <form onSubmit={handleCreateProduct}>
                                    <div className="form-group"><label className="form-label">Name</label><input className="form-input" required value={productForm.name} onChange={e => setProductForm({ ...productForm, name: e.target.value })} /></div>
                                    <div className="form-group"><label className="form-label">Description</label><textarea className="form-input" rows={3} required value={productForm.description} onChange={e => setProductForm({ ...productForm, description: e.target.value })} /></div>
                                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                                        <div className="form-group"><label className="form-label">Price</label><input className="form-input" type="number" step="0.01" required value={productForm.price} onChange={e => setProductForm({ ...productForm, price: e.target.value })} /></div>
                                        <div className="form-group"><label className="form-label">Old Price</label><input className="form-input" type="number" step="0.01" value={productForm.oldPrice} onChange={e => setProductForm({ ...productForm, oldPrice: e.target.value })} /></div>
                                    </div>
                                    <div className="form-group"><label className="form-label">Image URL</label><input className="form-input" required value={productForm.imageUrl} onChange={e => setProductForm({ ...productForm, imageUrl: e.target.value })} /></div>
                                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                                        <div className="form-group"><label className="form-label">Stock</label><input className="form-input" type="number" required value={productForm.stock} onChange={e => setProductForm({ ...productForm, stock: e.target.value })} /></div>
                                        <div className="form-group"><label className="form-label">Category</label>
                                            <select className="form-input" required value={productForm.categoryId} onChange={e => setProductForm({ ...productForm, categoryId: e.target.value })}>
                                                <option value="">Select</option>
                                                {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                                            </select>
                                        </div>
                                    </div>
                                    <div style={{ display: 'flex', gap: 12 }}>
                                        <button type="submit" className="btn btn-primary">Create</button>
                                        <button type="button" className="btn btn-secondary" onClick={() => setShowProductModal(false)}>Cancel</button>
                                    </div>
                                </form>
                            </div>
                        </div>
                    )}
                </>
            )}

            {/* Coupons Tab */}
            {tab === 'coupons' && (
                <>
                    <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'flex-end' }}>
                        <button className="btn btn-primary" onClick={() => setShowCouponModal(true)}><FiPlus /> Add Coupon</button>
                    </div>
                    <table className="admin-table">
                        <thead><tr><th>Code</th><th>Discount</th><th>Max</th><th>Min Order</th><th>Used</th><th>Expires</th><th>Action</th></tr></thead>
                        <tbody>
                            {coupons.map(c => (
                                <tr key={c.id}>
                                    <td style={{ fontWeight: 700, color: 'var(--accent-primary)' }}>{c.code}</td>
                                    <td>{c.discountPercent}%</td>
                                    <td>{c.maxDiscount ? `$${c.maxDiscount}` : '—'}</td>
                                    <td>{c.minOrderAmount ? `$${c.minOrderAmount}` : '—'}</td>
                                    <td>{c.timesUsed}/{c.usageLimit}</td>
                                    <td>{c.expiresAt ? new Date(c.expiresAt).toLocaleDateString() : 'Never'}</td>
                                    <td><button className="btn btn-danger btn-sm" onClick={() => handleDeleteCoupon(c.id)}><FiTrash2 /></button></td>
                                </tr>
                            ))}
                        </tbody>
                    </table>

                    {showCouponModal && (
                        <div className="modal-overlay" onClick={() => setShowCouponModal(false)}>
                            <div className="modal-content" onClick={e => e.stopPropagation()}>
                                <h2>Add Coupon</h2>
                                <form onSubmit={handleCreateCoupon}>
                                    <div className="form-group"><label className="form-label">Code</label><input className="form-input" required value={couponForm.code} onChange={e => setCouponForm({ ...couponForm, code: e.target.value.toUpperCase() })} /></div>
                                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                                        <div className="form-group"><label className="form-label">Discount %</label><input className="form-input" type="number" required value={couponForm.discountPercent} onChange={e => setCouponForm({ ...couponForm, discountPercent: e.target.value })} /></div>
                                        <div className="form-group"><label className="form-label">Max Discount $</label><input className="form-input" type="number" step="0.01" value={couponForm.maxDiscount} onChange={e => setCouponForm({ ...couponForm, maxDiscount: e.target.value })} /></div>
                                    </div>
                                    <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
                                        <div className="form-group"><label className="form-label">Min Order $</label><input className="form-input" type="number" step="0.01" value={couponForm.minOrderAmount} onChange={e => setCouponForm({ ...couponForm, minOrderAmount: e.target.value })} /></div>
                                        <div className="form-group"><label className="form-label">Usage Limit</label><input className="form-input" type="number" value={couponForm.usageLimit} onChange={e => setCouponForm({ ...couponForm, usageLimit: e.target.value })} /></div>
                                    </div>
                                    <div className="form-group"><label className="form-label">Expires At</label><input className="form-input" type="date" value={couponForm.expiresAt} onChange={e => setCouponForm({ ...couponForm, expiresAt: e.target.value })} /></div>
                                    <div style={{ display: 'flex', gap: 12 }}>
                                        <button type="submit" className="btn btn-primary">Create</button>
                                        <button type="button" className="btn btn-secondary" onClick={() => setShowCouponModal(false)}>Cancel</button>
                                    </div>
                                </form>
                            </div>
                        </div>
                    )}
                </>
            )}
        </div>
    );
}
