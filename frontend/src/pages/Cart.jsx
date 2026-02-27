import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { validateCoupon } from '../services/api';
import { FiTrash2, FiMinus, FiPlus, FiShoppingBag, FiArrowRight } from 'react-icons/fi';
import { motion, AnimatePresence } from 'framer-motion';
import toast from 'react-hot-toast';

export default function Cart() {
    const { items, total, updateQuantity, removeItem } = useCart();
    const [couponCode, setCouponCode] = useState('');
    const [discount, setDiscount] = useState(null);
    const navigate = useNavigate();

    const applyCoupon = async () => {
        if (!couponCode.trim()) return;
        try {
            const { data } = await validateCoupon({ code: couponCode, orderTotal: total });
            if (data.isValid) {
                setDiscount(data);
                toast.success(data.message);
            } else {
                toast.error(data.message);
                setDiscount(null);
            }
        } catch (e) { toast.error('Failed to validate coupon'); }
    };

    const finalTotal = discount ? total - discount.discountAmount : total;

    if (items.length === 0) {
        return (
            <div className="page container">
                <div className="empty-state">
                    <div className="empty-icon">🛒</div>
                    <h3>Your cart is empty</h3>
                    <p>Start adding some items to your cart!</p>
                    <Link to="/products" className="btn btn-primary">Start Shopping</Link>
                </div>
            </div>
        );
    }

    return (
        <div className="page container">
            <div className="page-header">
                <h1 className="page-title">Shopping Cart</h1>
                <p className="page-subtitle">{items.length} item{items.length !== 1 ? 's' : ''} in your cart</p>
            </div>

            <div className="cart-layout">
                <div>
                    <AnimatePresence>
                        {items.map(item => (
                            <motion.div key={item.id} className="cart-item" layout exit={{ opacity: 0, x: -100 }}>
                                <img src={item.productImage} alt={item.productName} />
                                <div className="cart-item-info">
                                    <Link to={`/products/${item.productId}`}><h3>{item.productName}</h3></Link>
                                    <div className="item-price">${item.price.toFixed(2)}</div>
                                    <div className="quantity-controls">
                                        <button onClick={() => updateQuantity(item.id, item.quantity - 1)}><FiMinus /></button>
                                        <span>{item.quantity}</span>
                                        <button onClick={() => updateQuantity(item.id, item.quantity + 1)} disabled={item.quantity >= item.stock}><FiPlus /></button>
                                    </div>
                                </div>
                                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-end', justifyContent: 'space-between' }}>
                                    <button className="btn btn-ghost" onClick={() => removeItem(item.id)}><FiTrash2 /></button>
                                    <div style={{ fontWeight: 700, fontSize: '1.1rem' }}>${(item.price * item.quantity).toFixed(2)}</div>
                                </div>
                            </motion.div>
                        ))}
                    </AnimatePresence>
                </div>

                <div className="cart-summary">
                    <h2>Order Summary</h2>
                    <div className="summary-row"><span>Subtotal</span><span>${total.toFixed(2)}</span></div>
                    <div className="summary-row"><span>Shipping</span><span style={{ color: 'var(--success)' }}>Free</span></div>
                    {discount && (
                        <div className="summary-row" style={{ color: 'var(--success)' }}>
                            <span>Discount ({discount.discountPercent}%)</span>
                            <span>-${discount.discountAmount.toFixed(2)}</span>
                        </div>
                    )}
                    <div className="summary-row total"><span>Total</span><span>${finalTotal.toFixed(2)}</span></div>

                    <div className="coupon-input">
                        <input placeholder="Coupon code" value={couponCode} onChange={e => setCouponCode(e.target.value.toUpperCase())} />
                        <button className="btn btn-secondary btn-sm" onClick={applyCoupon}>Apply</button>
                    </div>

                    <button className="btn btn-primary btn-block btn-lg" onClick={() => navigate('/checkout', { state: { discount, couponCode: discount ? couponCode : null } })}>
                        Checkout <FiArrowRight />
                    </button>
                </div>
            </div>
        </div>
    );
}
