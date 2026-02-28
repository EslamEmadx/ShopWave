import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { placeOrder, createCheckoutSession } from '../services/api';
import { FiArrowLeft, FiCreditCard } from 'react-icons/fi';
import toast from 'react-hot-toast';

export default function Checkout() {
    const { items, total, clearAll } = useCart();
    const navigate = useNavigate();
    const location = useLocation();
    const discount = location.state?.discount;
    const couponCode = location.state?.couponCode;

    const [form, setForm] = useState({ shippingAddress: '', shippingCity: '', phone: '', paymentMethod: 'Card' });
    const [loading, setLoading] = useState(false);

    const finalTotal = discount ? total - discount.discountAmount : total;

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!form.shippingAddress || !form.shippingCity || !form.phone) {
            toast.error('Please fill all fields');
            return;
        }
        setLoading(true);
        try {
            const { data } = await placeOrder({ ...form, couponCode });

            if (form.paymentMethod === 'COD') {
                toast.success('Order placed successfully (COD)!');
                await clearAll();
                navigate('/orders');
                return;
            }

            // Stripe checkout for Card
            try {
                const session = await createCheckoutSession({ orderId: data.orderId });
                if (session.data?.url) {
                    window.location.href = session.data.url;
                    return;
                }
            } catch (e) {
                toast.success('Order placed! (Simulated payment)');
            }

            await clearAll();
            navigate('/orders');
        } catch (e) {
            toast.error(e.response?.data?.message || 'Failed to place order');
        } finally { setLoading(false); }
    };

    if (items.length === 0) {
        return <div className="page container"><div className="empty-state"><h3>Cart is empty</h3><Link to="/products" className="btn btn-primary">Shop Now</Link></div></div>;
    }

    return (
        <div className="page container">
            <Link to="/cart" className="btn btn-ghost" style={{ marginBottom: 24 }}><FiArrowLeft /> Back to Cart</Link>
            <div className="page-header"><h1 className="page-title">Checkout</h1></div>

            <div className="cart-layout">
                <form onSubmit={handleSubmit}>
                    <div className="card" style={{ padding: 32 }}>
                        <h2 style={{ marginBottom: 24 }}>1. Shipping Information</h2>
                        <div className="form-group">
                            <label className="form-label">Shipping Address</label>
                            <input className="form-input" placeholder="Enter your full address" value={form.shippingAddress} onChange={e => setForm({ ...form, shippingAddress: e.target.value })} />
                        </div>
                        <div className="form-group">
                            <label className="form-label">City</label>
                            <input className="form-input" placeholder="Enter city" value={form.shippingCity} onChange={e => setForm({ ...form, shippingCity: e.target.value })} />
                        </div>
                        <div className="form-group">
                            <label className="form-label">Phone Number</label>
                            <input className="form-input" placeholder="Enter phone number" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
                        </div>

                        <h2 style={{ margin: '32px 0 24px' }}>2. Payment Method</h2>
                        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 32 }}>
                            <div
                                className={`card ${form.paymentMethod === 'Card' ? 'active' : ''}`}
                                style={{ padding: 16, cursor: 'pointer', borderColor: form.paymentMethod === 'Card' ? 'var(--accent-primary)' : '' }}
                                onClick={() => setForm({ ...form, paymentMethod: 'Card' })}
                            >
                                <FiCreditCard size={24} color={form.paymentMethod === 'Card' ? 'var(--accent-primary)' : ''} />
                                <div style={{ fontWeight: 600, marginTop: 8 }}>Credit Card</div>
                                <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Stripe / Paymob</div>
                            </div>
                            <div
                                className={`card ${form.paymentMethod === 'COD' ? 'active' : ''}`}
                                style={{ padding: 16, cursor: 'pointer', borderColor: form.paymentMethod === 'COD' ? 'var(--accent-primary)' : '' }}
                                onClick={() => setForm({ ...form, paymentMethod: 'COD' })}
                            >
                                <FiPackage size={24} color={form.paymentMethod === 'COD' ? 'var(--accent-primary)' : ''} />
                                <div style={{ fontWeight: 600, marginTop: 8 }}>Cash on Delivery</div>
                                <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>Pay when you receive</div>
                            </div>
                        </div>

                        <button type="submit" className="btn btn-primary btn-block btn-lg" disabled={loading}>
                            {loading ? 'Processing...' : `Place Order ($${finalTotal.toFixed(2)})`}
                        </button>
                    </div>
                </form>

                <div className="cart-summary">
                    <h2>Order Summary</h2>
                    <div style={{ maxHeight: 300, overflowY: 'auto', marginBottom: 20 }}>
                        {items.map(item => (
                            <div key={item.id} className="summary-row">
                                <span>{item.productName} × {item.quantity}</span>
                                <span>${(item.price * item.quantity).toFixed(2)}</span>
                            </div>
                        ))}
                    </div>
                    <div className="summary-row"><span>Subtotal</span><span>${total.toFixed(2)}</span></div>
                    {discount && (
                        <div className="summary-row" style={{ color: 'var(--success)' }}>
                            <span>Discount ({discount.code})</span><span>-${discount.discountAmount.toFixed(2)}</span>
                        </div>
                    )}
                    <div className="summary-row total"><span>Total</span><span>${finalTotal.toFixed(2)}</span></div>
                </div>
            </div>
        </div>
    );
}
