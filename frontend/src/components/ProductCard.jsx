import { useNavigate } from 'react-router-dom';
import { FiHeart, FiShoppingCart } from 'react-icons/fi';
import { FaHeart, FaStar, FaRegStar } from 'react-icons/fa';
import { useCart } from '../context/CartContext';
import { motion } from 'framer-motion';

export default function ProductCard({ product, wishlisted, onToggleWishlist }) {
    const navigate = useNavigate();
    const { addToCart } = useCart();
    const discount = product.oldPrice ? Math.round(((product.oldPrice - product.price) / product.oldPrice) * 100) : null;

    return (
        <motion.div
            className="card product-card"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
            onClick={() => navigate(`/products/${product.id}`)}
        >
            <div className="image-wrap">
                <img src={product.imageUrl} alt={product.name} loading="lazy" />
                {discount && <span className="sale-badge">-{discount}%</span>}
                <button
                    className={`wishlist-btn ${wishlisted ? 'active' : ''}`}
                    onClick={e => { e.stopPropagation(); onToggleWishlist?.(product.id); }}
                >
                    {wishlisted ? <FaHeart /> : <FiHeart />}
                </button>
            </div>
            <div className="card-body">
                <div className="category-tag">{product.categoryName}</div>
                <h3 className="product-name">{product.name}</h3>
                <div className="price-row">
                    <span className="price">${(product.price || 0).toFixed(2)}</span>
                    {product.oldPrice && <span className="old-price">${(product.oldPrice || 0).toFixed(2)}</span>}
                </div>

                {/* Real Reviews UI (Option A) */}
                <div className="product-card-reviews" style={{ marginTop: 8, minHeight: 46 }}>
                    {(product.reviewCount || 0) > 0 ? (
                        <div className="rating-active">
                            <div style={{ display: 'flex', gap: 2, color: '#FFD700', marginBottom: 4 }}>
                                {[1, 2, 3, 4, 5].map(s => (
                                    s <= Math.round(product.ratingAvg || 0) ? <FaStar key={s} size={14} /> : <FaRegStar key={s} size={14} />
                                ))}
                                <span style={{ color: 'var(--text-secondary)', fontSize: '0.8rem', marginLeft: 4, fontWeight: 600 }}>({(product.ratingAvg || 0).toFixed(1)})</span>
                            </div>
                            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>({product.reviewCount} reviews)</div>
                        </div>
                    ) : (
                        <div className="rating-empty">
                            <div style={{ display: 'flex', gap: 2, color: 'var(--text-muted)', opacity: 0.5, marginBottom: 2 }}>
                                {[1, 2, 3, 4, 5].map(s => <FaRegStar key={s} size={14} />)}
                            </div>
                            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)', lineHeight: 1.2 }}>
                                No reviews yet<br />
                                <span style={{ color: 'var(--accent-primary)', fontSize: '0.7rem', fontWeight: 600 }}>Be the first to review</span>
                            </div>
                        </div>
                    )}
                </div>
                <button
                    className="add-cart-btn"
                    onClick={e => { e.stopPropagation(); addToCart(product); }}
                >
                    <FiShoppingCart /> Add to Cart
                </button>
            </div>
        </motion.div>
    );
}
