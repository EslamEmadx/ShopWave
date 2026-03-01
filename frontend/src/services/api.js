import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5106/api';

const api = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' }
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth
export const login = (data) => api.post('/auth/login', data);
export const register = (data) => api.post('/auth/register', data);
export const getProfile = () => api.get('/auth/profile');
export const updateProfile = (data) => api.put('/auth/profile', data);
export const uploadAvatar = (data) => api.post('/auth/avatar', data, {
  headers: { 'Content-Type': 'multipart/form-data' }
});
export const deleteAvatar = () => api.delete('/auth/avatar');
export const getAvatarUrl = (ts) => ts ? `${API_URL}/auth/avatar?ts=${ts}` : null;
export const resolveAvatarUrl = (url) => {
  if (!url) return null;
  if (url.startsWith('http')) return url;
  const baseUrl = API_URL.endsWith('/api') ? API_URL.slice(0, -4) : API_URL;
  return `${baseUrl}${url}`;
};

// Products
export const getProducts = (params) => api.get('/products', { params });
export const getProduct = (id) => api.get(`/products/${id}`);
export const createProduct = (data) => api.post('/products', data);
export const updateProduct = (id, data) => api.put(`/products/${id}`, data);
export const deleteProduct = (id) => api.delete(`/products/${id}`);

// Categories
export const getCategories = () => api.get('/categories');
export const createCategory = (data) => api.post('/categories', data);
export const updateCategory = (id, data) => api.put(`/categories/${id}`, data);
export const deleteCategory = (id) => api.delete(`/categories/${id}`);

// Cart
export const getCart = () => api.get('/cart');
export const addToCart = (data) => api.post('/cart', data);
export const updateCartItem = (id, data) => api.put(`/cart/${id}`, data);
export const removeCartItem = (id) => api.delete(`/cart/${id}`);
export const clearCart = () => api.delete('/cart');

// Wishlist
export const getWishlist = () => api.get('/wishlist');
export const toggleWishlist = (productId) => api.post(`/wishlist/${productId}`);
export const removeWishlistItem = (id) => api.delete(`/wishlist/${id}`);

// Reviews
export const getProductReviews = (productId) => api.get(`/reviews/product/${productId}`);
export const createReview = (data) => api.post('/reviews', data);
export const getAllReviews = () => api.get('/reviews/pending');
export const approveReview = (id) => api.post(`/reviews/${id}/approve`);
export const rejectReview = (id) => api.post(`/reviews/${id}/reject`);

// Orders
export const placeOrder = (data) => api.post('/orders', data);
export const getOrders = () => api.get('/orders');
export const getOrder = (id) => api.get(`/orders/${id}`);
export const updateOrderStatus = (id, data) => api.put(`/orders/${id}/status`, data);

// Payment
export const createCheckoutSession = (data) => api.post('/payment/create-checkout-session', data);

// Coupons
export const validateCoupon = (data) => api.post('/coupons/validate', data);
export const getCoupons = () => api.get('/coupons');
export const createCoupon = (data) => api.post('/coupons', data);
export const deleteCoupon = (id) => api.delete(`/coupons/${id}`);

// Admin
export const getDashboard = () => api.get('/Admin/dashboard');

export default api;
