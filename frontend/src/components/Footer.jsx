import { Link } from 'react-router-dom';
import { FiGithub, FiTwitter, FiInstagram } from 'react-icons/fi';

export default function Footer() {
    return (
        <footer className="footer">
            <div className="container">
                <div className="footer-grid">
                    <div className="footer-brand">
                        <div className="footer-logo">ShopWave</div>
                        <p>Your premium destination for the latest products. Quality meets style with fast delivery and exceptional service.</p>
                    </div>
                    <div className="footer-col">
                        <h3>Shop</h3>
                        <Link to="/products">All Products</Link>
                        <Link to="/products?featured=true">Featured</Link>
                        <Link to="/products?sort=newest">New Arrivals</Link>
                        <Link to="/products?sort=price_asc">Best Deals</Link>
                    </div>
                    <div className="footer-col">
                        <h3>Account</h3>
                        <Link to="/profile">My Profile</Link>
                        <Link to="/orders">My Orders</Link>
                        <Link to="/wishlist">Wishlist</Link>
                        <Link to="/cart">Shopping Cart</Link>
                    </div>
                    <div className="footer-col">
                        <h3>Support</h3>
                        <a href="#">Help Center</a>
                        <a href="#">Shipping Info</a>
                        <a href="#">Returns</a>
                        <a href="#">Contact Us</a>
                    </div>
                </div>
                <div className="footer-bottom">
                    <p>© 2026 ShopWave. All rights reserved.</p>
                </div>
            </div>
        </footer>
    );
}
