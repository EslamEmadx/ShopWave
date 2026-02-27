import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getOrders } from '../services/api';
import { FiPackage } from 'react-icons/fi';
import { motion } from 'framer-motion';

export default function Orders() {
    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getOrders().then(r => setOrders(r.data)).catch(() => { }).finally(() => setLoading(false));
    }, []);

    if (loading) return <div className="page container"><div className="spinner-container"><div className="spinner" /></div></div>;

    return (
        <div className="page container">
            <div className="page-header"><h1 className="page-title">My Orders</h1></div>
            {orders.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon"><FiPackage /></div>
                    <h3>No orders yet</h3>
                    <p>Your order history will appear here</p>
                    <Link to="/products" className="btn btn-primary">Start Shopping</Link>
                </div>
            ) : (
                orders.map(order => (
                    <motion.div key={order.id} className="order-card" initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}>
                        <div className="order-header">
                            <div>
                                <span className="order-id">Order #{order.id}</span>
                                <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginTop: 4 }}>
                                    {new Date(order.createdAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}
                                </div>
                            </div>
                            <div style={{ textAlign: 'right' }}>
                                <span className={`order-status status-${order.status.toLowerCase()}`}>{order.status}</span>
                                <div style={{ fontWeight: 700, fontSize: '1.1rem', marginTop: 8 }}>${order.totalAmount.toFixed(2)}</div>
                            </div>
                        </div>
                        <div className="order-items-list">
                            {order.items.map(item => (
                                <div key={item.id} className="order-item-row">
                                    <img src={item.productImage} alt={item.productName} />
                                    <div style={{ flex: 1 }}>
                                        <div style={{ fontWeight: 600 }}>{item.productName}</div>
                                        <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>Qty: {item.quantity} × ${item.price.toFixed(2)}</div>
                                    </div>
                                    <div style={{ fontWeight: 600 }}>${(item.price * item.quantity).toFixed(2)}</div>
                                </div>
                            ))}
                        </div>
                        {order.couponCode && (
                            <div style={{ marginTop: 12, padding: '8px 12px', background: 'rgba(46,204,113,0.1)', borderRadius: 8, fontSize: '0.85rem', color: 'var(--success)' }}>
                                Coupon: {order.couponCode} (-${order.discountAmount.toFixed(2)})
                            </div>
                        )}
                    </motion.div>
                ))
            )}
        </div>
    );
}
