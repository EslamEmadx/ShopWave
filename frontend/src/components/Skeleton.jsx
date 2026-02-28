
export function Skeleton({ width, height, borderRadius = 'var(--radius-sm)', className = '' }) {
    return (
        <div
            className={`skeleton-base ${className}`}
            style={{ width, height, borderRadius }}
        />
    );
}

export function ProductCardSkeleton() {
    return (
        <div className="card product-card skeleton-card">
            <div className="image-wrap skeleton-base" style={{ height: '260px' }} />
            <div className="card-body">
                <Skeleton width="40%" height="14px" className="mb-2" />
                <Skeleton width="80%" height="20px" className="mb-3" />
                <div style={{ display: 'flex', gap: 10 }}>
                    <Skeleton width="30%" height="22px" />
                    <Skeleton width="20%" height="22px" />
                </div>
                <Skeleton width="100%" height="40px" className="mt-4" />
            </div>
        </div>
    );
}
