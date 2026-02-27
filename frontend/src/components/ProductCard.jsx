import { useNavigate } from 'react-router-dom';
import { FiHeart, FiStar, FiShoppingCart } from 'react-icons/fi';
import { FaHeart } from 'react-icons/fa';
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
                    <span className="price">${product.price.toFixed(2)}</span>
                    {product.oldPrice && <span className="old-price">${product.oldPrice.toFixed(2)}</span>}
                </div>
                <div className="rating">
                    <span className="stars"><FiStar /> {product.rating.toFixed(1)}</span>
                    <span>({product.reviewCount})</span>
                </div>
                <button
                    className="add-cart-btn"
                    onClick={e => { e.stopPropagation(); addToCart(product.id); }}
                >
                    <FiShoppingCart /> Add to Cart
                </button>
            </div>
        </motion.div>
    );
}
