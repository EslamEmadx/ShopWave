import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getProduct, getProductReviews, createReview, toggleWishlist, getWishlist, getProducts } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { FiHeart, FiShoppingCart, FiStar, FiMinus, FiPlus, FiArrowLeft } from 'react-icons/fi';
import { FaHeart, FaStar, FaRegStar } from 'react-icons/fa';
import { motion } from 'framer-motion';
import ProductCard from '../components/ProductCard';
import toast from 'react-hot-toast';

export default function ProductDetail() {
    const { id } = useParams();
    const [product, setProduct] = useState(null);
    const [reviews, setReviews] = useState([]);
    const [related, setRelated] = useState([]);
    const [loading, setLoading] = useState(true);
    const [qty, setQty] = useState(1);
    const [wishlisted, setWishlisted] = useState(false);
    const [reviewForm, setReviewForm] = useState({ rating: 5, comment: '' });
    const { user } = useAuth();
    const { addToCart } = useCart();

    useEffect(() => {
        setLoading(true);
        window.scrollTo(0, 0);
        getProduct(id).then(r => {
            setProduct(r.data);
            getProducts({ categoryId: r.data.categoryId, pageSize: 4 }).then(rr => {
                const list = rr.data?.items ?? rr.data?.products ?? [];
                setRelated((Array.isArray(list) ? list : []).filter(p => p.id !== r.data.id).slice(0, 4));
            });
        }).catch(() => { }).finally(() => setLoading(false));
        getProductReviews(id).then(r => setReviews(Array.isArray(r.data) ? r.data : (r.data?.items ?? []))).catch(() => { });
        if (user) getWishlist().then(r => {
            const list = r.data?.items ?? r.data ?? [];
            setWishlisted((Array.isArray(list) ? list : []).some(w => w.productId === parseInt(id)));
        }).catch(() => { });
    }, [id, user]);

    const handleWishlist = async () => {
        if (!user) { toast.error('Please login first'); return; }
        const { data } = await toggleWishlist(parseInt(id));
        setWishlisted(data.isWishlisted);
        toast.success(data.message);
    };

    const submitReview = async (e) => {
        e.preventDefault();
        if (!user) { toast.error('Please login first'); return; }
        try {
            await createReview({ productId: parseInt(id), ...reviewForm });
            toast.success('Review submitted!');
            setReviewForm({ rating: 5, comment: '' });
            getProductReviews(id).then(r => setReviews(r.data));
            getProduct(id).then(r => setProduct(r.data));
        } catch (e) { toast.error(e.response?.data?.message || 'Failed to submit review'); }
    };

    if (loading) return <div className="page container"><div className="spinner-container"><div className="spinner" /></div></div>;
    if (!product) return <div className="page container"><div className="empty-state"><h3>Product not found</h3></div></div>;

    return (
        <div className="page container">
            <Link to="/products" className="btn btn-ghost" style={{ marginBottom: 24 }}><FiArrowLeft /> Back to Shop</Link>

            <motion.div className="product-detail" initial={{ opacity: 0 }} animate={{ opacity: 1 }}>
                <div className="product-image-main">
                    <img src={product.imageUrl} alt={product.name} />
                </div>
                <div className="product-info">
                    <div className="category-label">{product.categoryName}</div>
                    <h1>{product.name}</h1>
                    <div className="rating" style={{ fontSize: '0.9rem', marginBottom: 16 }}>
                        <span className="stars" style={{ display: 'flex', gap: 2 }}>
                            {[1, 2, 3, 4, 5].map(s => s <= Math.round(product.ratingAvg || 0) ? <FaStar key={s} color="#FFD700" /> : <FaRegStar key={s} color="#FFD700" />)}
                        </span>
                        <span>{(product.ratingAvg || 0).toFixed(1)} ({product.reviewCount || 0} reviews)</span>
                    </div>
                    <div className="price-section">
                        <span className="current-price">${product.price.toFixed(2)}</span>
                        {product.oldPrice && <span className="original-price">${product.oldPrice.toFixed(2)}</span>}
                    </div>
                    <p className="description">{product.description}</p>
                    <div className="stock-info">
                        {product.stock > 0 ? <span className="in-stock">✓ In Stock ({product.stock} available)</span> : <span className="out-stock">✗ Out of Stock</span>}
                    </div>

                    <div className="quantity-controls" style={{ marginBottom: 20 }}>
                        <button onClick={() => setQty(Math.max(1, qty - 1))}><FiMinus /></button>
                        <span>{qty}</span>
                        <button onClick={() => setQty(Math.min(product.stock, qty + 1))}><FiPlus /></button>
                    </div>

                    <div className="product-actions">
                        <button className="btn btn-primary btn-lg" onClick={() => addToCart(product.id, qty)} disabled={product.stock === 0}>
                            <FiShoppingCart /> Add to Cart
                        </button>
                        <button className={`btn btn-secondary btn-lg`} onClick={handleWishlist}>
                            {wishlisted ? <FaHeart color="var(--accent-secondary)" /> : <FiHeart />}
                        </button>
                    </div>
                </div>
            </motion.div>

            {/* Reviews */}
            <div className="reviews-section">
                <h2 className="section-title" style={{ marginBottom: 24 }}>Reviews ({reviews.length})</h2>

                {user && (
                    <form onSubmit={submitReview} className="card" style={{ padding: 24, marginBottom: 24 }}>
                        <h3 style={{ marginBottom: 16 }}>Write a Review</h3>
                        <div style={{ display: 'flex', gap: 4, marginBottom: 16 }}>
                            {[1, 2, 3, 4, 5].map(s => (
                                <button type="button" key={s} onClick={() => setReviewForm({ ...reviewForm, rating: s })} style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: '1.5rem' }}>
                                    {s <= reviewForm.rating ? <FaStar color="#FFD700" /> : <FaRegStar color="#FFD700" />}
                                </button>
                            ))}
                        </div>
                        <textarea className="form-input" rows={3} placeholder="Share your experience..." value={reviewForm.comment}
                            onChange={e => setReviewForm({ ...reviewForm, comment: e.target.value })} style={{ marginBottom: 12, resize: 'vertical' }} />
                        <button type="submit" className="btn btn-primary">Submit Review</button>
                    </form>
                )}

                {reviews.length === 0 ? <p style={{ color: 'var(--text-secondary)' }}>No reviews yet. Be the first!</p> :
                    reviews.map(r => (
                        <div key={r.id} className="review-card">
                            <div className="review-header">
                                <div className="review-user">
                                    <div className="review-avatar">{r.username?.charAt(0).toUpperCase()}</div>
                                    <div>
                                        <div style={{ fontWeight: 600 }}>{r.username}</div>
                                        <div style={{ display: 'flex', gap: 2 }}>
                                            {[1, 2, 3, 4, 5].map(s => s <= r.rating ? <FaStar key={s} size={12} color="#FFD700" /> : <FaRegStar key={s} size={12} color="#FFD700" />)}
                                        </div>
                                    </div>
                                </div>
                                <span className="review-date">{new Date(r.createdAt).toLocaleDateString()}</span>
                            </div>
                            <p className="review-comment">{r.comment}</p>
                        </div>
                    ))
                }
            </div>

            {/* Related Products */}
            {related.length > 0 && (
                <div className="section">
                    <h2 className="section-title" style={{ marginBottom: 24 }}>Related Products</h2>
                    <div className="products-grid">{related.map(p => <ProductCard key={p.id} product={p} />)}</div>
                </div>
            )}
        </div>
    );
}
