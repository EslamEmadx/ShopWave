import { Link, useLocation, useNavigate } from 'react-router-dom';
import { FiShoppingCart, FiHeart, FiUser, FiPackage, FiLogOut, FiGrid, FiHome, FiShoppingBag, FiSun, FiMoon } from 'react-icons/fi';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { useTheme } from '../context/ThemeContext';
import { resolveAvatarUrl } from '../services/api';
import { useState } from 'react';

export default function Navbar() {
    const { user, logout, isAdmin } = useAuth();
    const [imgError, setImgError] = useState(false);
    const { count } = useCart();
    const { theme, toggleTheme } = useTheme();
    const location = useLocation();
    const navigate = useNavigate();

    const isActive = (path) => location.pathname === path ? 'active' : '';

    const handleLogout = () => { logout(); navigate('/'); };

    return (
        <nav className="navbar">
            <div className="navbar-inner">
                <Link to="/" className="navbar-logo">ShopWave</Link>
                <div className="navbar-links">
                    <Link to="/" className={`nav-link ${isActive('/')}`}>
                        <FiHome /> <span>Home</span>
                    </Link>
                    <Link to="/products" className={`nav-link ${isActive('/products')}`}>
                        <FiShoppingBag /> <span>Shop</span>
                    </Link>
                    {user && (
                        <>
                            <Link to="/wishlist" className={`nav-link ${isActive('/wishlist')}`}>
                                <FiHeart />
                            </Link>
                            <Link to="/cart" className={`nav-link ${isActive('/cart')}`}>
                                <FiShoppingCart />
                                {count > 0 && <span className="badge">{count}</span>}
                            </Link>
                            <Link to="/orders" className={`nav-link ${isActive('/orders')}`}>
                                <FiPackage />
                            </Link>
                            {isAdmin && (
                                <Link to="/admin" className={`nav-link ${isActive('/admin')}`}>
                                    <FiGrid /> <span>Admin</span>
                                </Link>
                            )}
                            <div className="nav-user">
                                <Link to="/profile" className="nav-avatar" style={{ overflow: 'hidden', padding: user.profilePictureUrl && !imgError ? 0 : undefined }}>
                                    {user.profilePictureUrl && !imgError ? (
                                        <img src={resolveAvatarUrl(user.profilePictureUrl)} alt="" style={{ width: '100%', height: '100%', objectFit: 'cover' }} onError={() => setImgError(true)} />
                                    ) : (
                                        user.username?.charAt(0).toUpperCase()
                                    )}
                                </Link>
                                <button className="nav-link" onClick={handleLogout}><FiLogOut /></button>
                            </div>
                        </>
                    )}
                    <button className="nav-link theme-toggle" onClick={toggleTheme}>
                        {theme === 'dark' ? <FiSun /> : <FiMoon />}
                    </button>
                    {!user && (
                        <Link to="/login" className={`nav-link ${isActive('/login')}`}>
                            <FiUser /> <span>Login</span>
                        </Link>
                    )}
                </div>
            </div>
        </nav>
    );
}
