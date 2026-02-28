import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { getProducts, getCategories, getWishlist, toggleWishlist } from '../services/api';
import { useAuth } from '../context/AuthContext';
import { ProductCardSkeleton } from '../components/Skeleton';
import ProductCard from '../components/ProductCard';
import { FiSearch } from 'react-icons/fi';

export default function Products() {
    const [searchParams, setSearchParams] = useSearchParams();
    const [products, setProducts] = useState([]);
    const [categories, setCategories] = useState([]);
    const [totalPages, setTotalPages] = useState(1);
    const [loading, setLoading] = useState(true);
    const [wishlist, setWishlist] = useState([]);
    const { user } = useAuth();

    const page = parseInt(searchParams.get('page') || '1');
    const categoryId = searchParams.get('categoryId') || '';
    const search = searchParams.get('search') || '';
    const sort = searchParams.get('sort') || '';

    useEffect(() => {
        getCategories().then(r => setCategories(r.data)).catch(() => { });
        if (user) getWishlist().then(r => setWishlist(r.data.items || r.data || []).map(w => w.productId)).catch(() => { });
    }, [user]);

    useEffect(() => {
        setLoading(true);
        window.scrollTo({ top: 0, behavior: 'smooth' });
        const params = { page, pageSize: 12 };
        if (categoryId) params.categoryId = categoryId;
        if (search) params.search = search;
        if (sort) params.sort = sort;
        if (searchParams.get('featured')) params.featured = true;

        getProducts(params).then(r => {
            setProducts(r.data.items ?? []);
            setTotalPages(r.data.totalPages ?? 1);
        }).catch(() => { }).finally(() => setLoading(false));
    }, [page, categoryId, search, sort, searchParams.get('featured')]);

    const updateFilter = (key, value) => {
        const params = new URLSearchParams(searchParams);
        if (value) params.set(key, value);
        else params.delete(key);

        // Reset page to 1 when filters change, UNLESS we are specifically changing the page
        if (key !== 'page') params.delete('page');

        setSearchParams(params);
    };

    const handleWishlist = async (productId) => {
        if (!user) return;
        await toggleWishlist(productId);
        setWishlist(prev => prev.includes(productId) ? prev.filter(id => id !== productId) : [...prev, productId]);
    };

    return (
        <div className="page container">
            <div className="page-header">
                <h1 className="page-title">Shop All Products</h1>
                <p className="page-subtitle">Discover our collection of premium products</p>
            </div>

            <div className="filters-bar">
                <div style={{ position: 'relative', flex: 1, minWidth: 200 }}>
                    <FiSearch style={{ position: 'absolute', left: 14, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
                    <input
                        className="search-input"
                        style={{ paddingLeft: 40, width: '100%' }}
                        placeholder="Search products..."
                        value={search}
                        onChange={e => updateFilter('search', e.target.value)}
                    />
                </div>
                <select className="filter-select" value={categoryId} onChange={e => updateFilter('categoryId', e.target.value)}>
                    <option value="">All Categories</option>
                    {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
                <select className="filter-select" value={sort} onChange={e => updateFilter('sort', e.target.value)}>
                    <option value="">Default</option>
                    <option value="price_asc">Price: Low to High</option>
                    <option value="price_desc">Price: High to Low</option>
                    <option value="rating">Top Rated</option>
                    <option value="newest">Newest</option>
                    <option value="name">Name A-Z</option>
                </select>
            </div>


            {/* inside rendering:*/}
            {loading ? (
                <div className="products-grid">
                    {[1, 2, 3, 4, 5, 6, 7, 8].map(i => <ProductCardSkeleton key={i} />)}
                </div>
            ) : products.length === 0 ? (
                <div className="empty-state">
                    <div className="empty-icon">🔍</div>
                    <h3>No products found</h3>
                    <p>Try adjusting your search or filters</p>
                </div>
            ) : (
                <>
                    <div className="products-grid">
                        {products.map(p => (
                            <ProductCard key={p.id} product={p} wishlisted={wishlist.includes(p.id)} onToggleWishlist={handleWishlist} />
                        ))}
                    </div>
                    {totalPages > 1 && (
                        <div className="pagination">
                            <button disabled={page <= 1} onClick={() => updateFilter('page', page - 1)}>Prev</button>
                            {Array.from({ length: totalPages }, (_, i) => (
                                <button key={i + 1} className={page === i + 1 ? 'active' : ''} onClick={() => updateFilter('page', i + 1)}>{i + 1}</button>
                            ))}
                            <button disabled={page >= totalPages} onClick={() => updateFilter('page', page + 1)}>Next</button>
                        </div>
                    )}
                </>
            )}
        </div>
    );
}
